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
using System.IO;
using System.Linq;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using Google.Apis.Compute.v1.Data;

using Newtonsoft.Json;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Keyfactor.Extensions.Orchestrator.GCPLoadBalancer
{
    public class Management : IManagementJobExtension
    {
        private const int GCP_MAX_ALIAS_LENGTH = 62;
        private const int MAX_SIMPLENAME_LENGTH = 35;

        public string ExtensionName => string.Empty;

        private (byte[], byte[]) GetPemFromPFX(byte[] pfxBytes, char[] pfxPassword)
        {
            Pkcs12Store p = new Pkcs12Store(new MemoryStream(pfxBytes), pfxPassword);

            // Extract private key
            MemoryStream memoryStream = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(memoryStream);
            PemWriter pemWriter = new PemWriter(streamWriter);

            String alias = (p.Aliases.Cast<String>()).SingleOrDefault(a => p.IsKeyEntry(a));
            AsymmetricKeyParameter publicKey = p.GetCertificate(alias).Certificate.GetPublicKey();
            if (p.GetKey(alias) == null) { throw new Exception($"Unable to get the key for alias: {alias}"); }
            AsymmetricKeyParameter privateKey = p.GetKey(alias).Key;
            AsymmetricCipherKeyPair keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);

            pemWriter.WriteObject(keyPair.Private);
            streamWriter.Flush();
            String privateKeyString = Encoding.ASCII.GetString(memoryStream.GetBuffer()).Trim().Replace("\r", "").Replace("\0", "");
            memoryStream.Close();
            streamWriter.Close();

            // Extract server certificate
            String certStart = "-----BEGIN CERTIFICATE-----\n";
            String certEnd = "\n-----END CERTIFICATE-----";

            Func<String, String> pemify = null;
            pemify = (ss => ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + pemify(ss.Substring(64)));

            string certPem = string.Empty;
            foreach (X509CertificateEntry certEntry in p.GetCertificateChain(alias))
            {
                if (certEntry.Certificate.IssuerDN.ToString() == certEntry.Certificate.SubjectDN.ToString())
                    continue;
                certPem += (certStart + pemify(Convert.ToBase64String(certEntry.Certificate.GetEncoded())) + certEnd + "\n");
            }

            return (Encoding.ASCII.GetBytes(certPem), Encoding.ASCII.GetBytes(privateKeyString));
        }

        private string generateAlias(X509Certificate2 cert)
        {
            // If keyPairName is not specified then create one based upon prefix and serial number
            // limit to lowercase, number, and hyphen
            // convert period into hyphen.
            
            string prefix = "kf";
            string alias = prefix;

            string simpleName = cert.GetNameInfo(X509NameType.SimpleName, false);
            simpleName = simpleName.Replace('.', '-').ToLower();
            //set maxlength for simplename
            simpleName = simpleName.Substring(0, Math.Min(MAX_SIMPLENAME_LENGTH, simpleName.Length));

            alias = alias + "-" + simpleName;
            alias = alias + "-" + new String(cert.SerialNumber.ToString().ToLower().Reverse().ToArray());

            //strip characters that are not valid
            Regex alphaNumericHyphen = new Regex("[^a-z0-9-]");
            alias = alphaNumericHyphen.Replace(alias, "");

            //ensure alias is not longer then max length
            alias = alias.Substring(0, Math.Min(GCP_MAX_ALIAS_LENGTH, alias.Length));

            return alias;
        }

        private SslCertificate GetSslCertificate(string certificate, string alias, string pfxPassword)
        {
            ILogger logger = LogHandler.GetClassLogger<Management>();

            byte[] pfxBytes = Convert.FromBase64String(certificate);
            (byte[] certPem, byte[] privateKey) = GetPemFromPFX(pfxBytes, pfxPassword.ToCharArray());

            X509Certificate2 cert = new X509Certificate2(certPem);
            string jobOrGenerated = string.IsNullOrWhiteSpace(alias) ? "generated" : "job";
            alias = string.IsNullOrWhiteSpace(alias) ? generateAlias(cert) : alias;

            logger.LogDebug($"Using {jobOrGenerated} alias {alias}");

            return new SslCertificate
            {
                Certificate = System.Text.Encoding.Default.GetString(certPem),
                PrivateKey = System.Text.Encoding.Default.GetString(privateKey),
                Name = alias
            };
        }

        //Job Entry Point
        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger<Management>();

            logger.LogDebug($"Begin Management...");

            try
            {
                GCPStore store = new GCPStore(config.CertificateStoreDetails.StorePath, JsonConvert.DeserializeObject<Dictionary<string, string>>((string)config.CertificateStoreDetails.Properties));
                //Management jobs, unlike Discovery, Inventory, and Reenrollment jobs can have 3 different purposes:
                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        if (string.IsNullOrEmpty(config.JobCertificate.PrivateKeyPassword))
                            throw new Exception("Error attempting to add or renew a certificate.  No private key is present.");
                        store.insert(GetSslCertificate(config.JobCertificate.Contents, config.JobCertificate.Alias, config.JobCertificate.PrivateKeyPassword), config.Overwrite);
                        break;
                    case CertStoreOperationType.Remove:
                        store.delete(config.JobCertificate.Alias);
                        break;
                    case CertStoreOperationType.Create:
                        // The certificate store is remote
                        break;
                    default:
                        //Invalid OperationType.  Return error.  Should never happen though
                        return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}" };
                }
            }
            catch (Exception ex)
            {
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = ex.Message };
            }

            //Status: 2=Success, 3=Warning, 4=Error
            return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }
    }
}