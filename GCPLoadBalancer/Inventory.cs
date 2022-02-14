//Copyright 2021 Keyfactor
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.


using System;
using System.Collections.Generic;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Keyfactor.Extensions.Orchestrator.GCPLoadBalancer
{
    // The Inventory class implementes IAgentJobExtension and is meant to find all of the certificates in a given certificate store on a given server
    //  and return those certificates back to Keyfactor for storing in its database.  Private keys will NOT be passed back to Keyfactor Command 
    public class Inventory : IInventoryJobExtension
    {
        public string GetJobClass()
        {
            //Setting to "Inventory" makes this the entry point for all Inventory jobs
            return "Inventory";
        }

        public string ExtensionName => string.Empty;


        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger<Inventory>();

            logger.LogDebug($"Begin Inventory...");

            //List<CurrentInventoryItem> is the collection that the interface expects to return from this job.  It will contain a collection of certificates found in the store along with other information about those certificates
            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();
            
            try
            {
                //Code logic to:
                // 1) Connect to the orchestrated server (config.Store.ClientMachine) containing the certificate store to be inventoried (config.Store.StorePath)
                //GCPStore store = new GCPStore(config);

                // 2) Custom logic to retrieve certificates from certificate store.
                GCPStore store = new GCPStore(config.CertificateStoreDetails.StorePath, JsonConvert.DeserializeObject<Dictionary<string, string>>((string)config.CertificateStoreDetails.Properties));
                inventoryItems = store.list();
            }
            catch (Exception ex)
            {
                logger.LogError("Error performing certificate inventory: " + ex.Message);
                logger.LogDebug(ex.StackTrace);
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = ex.Message + System.Environment.NewLine + ex.StackTrace };
            }

            try
            {
                logger.LogDebug("Sending certificates back to Command:" + inventoryItems.Count);
                //Sends inventoried certificates back to KF Command
                bool status = submitInventory.Invoke(inventoryItems);
                logger.LogDebug("Send Certificate response: " + status);
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                logger.LogError("Error submitting certificate inventory: " + ex.Message);
                logger.LogDebug(ex.StackTrace);
                // NOTE: if the cause of the submitInventory.Invoke exception is a communication issue between the Orchestrator server and the Command server, the job status returned here
                //  may not be reflected in Keyfactor Command.
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = ex.Message + System.Environment.NewLine + ex.StackTrace };
            }
        }
    }
}