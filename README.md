<h1 align="center" style="border-bottom: none">
    GCP Load Balancer Universal Orchestrator Extension
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/gcp-loadbalancer-orchestrator/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/gcp-loadbalancer-orchestrator?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/gcp-loadbalancer-orchestrator?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/gcp-loadbalancer-orchestrator/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a>
  ·
  <a href="#installation">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=orchestrator">
    <b>Related Integrations</b>
  </a>
</p>


## Overview

The GCP Load Balancer Universal Orchestrator extension enables Keyfactor Command to manage SSL/TLS certificates within Google Cloud Platform (GCP) Load Balancers. GCP Load Balancers ensure that users can securely access applications using HTTPS by utilizing SSL/TLS certificates. These certificates need to be managed efficiently to ensure uninterrupted, secure connections.

Within Keyfactor Command, defined Certificate Stores represent the configured SSL/TLS certificates that are managed in the GCP environment. These Certificate Stores can be thought of as logical groupings of certificates, which can be associated with specific GCP projects or regions. The Orchestrator's job is to automate the inventory, addition, and removal of these certificates, streamlining the certificate lifecycle management process for GCP Load Balancers.

## Compatibility

This integration is compatible with Keyfactor Universal Orchestrator version 10.1 and later.

## Support
The GCP Load Balancer Universal Orchestrator extension is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 
 
> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Installation
Before installing the GCP Load Balancer Universal Orchestrator extension, it's recommended to install [kfutil](https://github.com/Keyfactor/kfutil). Kfutil is a command-line tool that simplifies the process of creating store types, installing extensions, and instantiating certificate stores in Keyfactor Command.


1. Follow the [requirements section](docs/gcploadbal.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    ### GCP Load Balancer Configuration

    **1. In Keyfactor Command, go to Settings (the gear icon in the top right) => Certificate Store Types and create a new certificate store type:**

    ![](images/image1.png)

    ![](images/image2.png)

    The certificate store type set up for the GCP Load Balancer Orchestrator should have the following options set:

    - **Name:** A descriptive name for the certificate store type
    - **Short Name:** Must be **GCPLoadBal** or the alternative name you used to create the folder in the {installation folder}\extensions folder.
    - **Custom Capability** - Leave unchecked
    - **Supported Job Types** – Select Inventory, Add, and Remove
    - **General Settings** - Leave Needs Server and Uses PowerShell unchecked.  Select Blueprint Allowed if you plan to use blueprinting.
    - **Password Settings** - Leave both options unchecked
    - **Store Path Type** - Freeform
    - **Supports Custom Alias** - Optional.  If no alias is provided, one will be dynamically created by the GCP Load Balancer Orchestrator.
    - **Private Key Handling: ** Required (Adding a certificate to a GCP Load Balancer certificate store without the private key is not a valid use case)
    - **PFX Password Style:** Default

    **Parameters:** Add 1 custom parameter if authenticating to the GCP API library by passing the GCP service account key from Keyfactor Command (see Authentication):

    ![](images/image3.png)

    - Name: Must be **jsonKey**
    - Display Name: Desired custom display name
    - Type: Secret
    - Change Default Value: Unchecked
    - Default Value: Leave blank



    **2. Create a new GCP Load Balancer certificate store.  Navigate to Certificate Locations =\> Certificate Stores within Keyfactor Command to add the store. Below are the values that should be entered.**
    ![](images/image4.png)

    - **Category:** Must be the GCP Load Balancer type you created in Step 1.

    - **Container:** Optional container name if using this feature.  Please consult the Keyfactor Command Reference Guide for more information on this feature.

    - **Client Machine:** The name or IP address of the Orchestrator server that will be handling GCP jobs.

    - **Store Path:** This should be your Google Cloud project ID.  This will work against GCP Global resources.  Optionally, you can append "/" with the region you wish to process against.  Please refer to the following page for a list of valid region codes (GCP code column): https://gist.github.com/rpkim/084046e02fd8c452ba6ddef3a61d5d59.

    - **Service Account Key:** If you will be authenticating via passing credentials from Keyfactor Command, you must add this value as follows:
      - No Service Account Key: Unchecked
      - Secret Source: "Keyfactor Secrets" if you wish to store the GCP service account key in the Keyfactor secrets engine or "Load From PAM Provider" if you have set up a PAM provider integration within Keyfactor Command and wish to store this value there.
      - Enter and Confirm Service Account Key: The JSON-based service account key you acquired from GCP (See Authentication).

    **Inventory Schedule:** Set whether to schedule Inventory jobs for this certificate store, and if so, the frequency here.

    ### Authentication

    A service account is necessary for authentication to GCP.  The following are the required permissions:
    - compute.sslCertificates.create
    - compute.sslCertificates.delete
    - compute.sslCertificates.list
    - compute.sslCertificates.get
    - compute.targetHttpsProxies.list
    - compute.targetHttpsProxies.setSslCertificates
    - compute.regionSslCertificates.list

    The agent supports having credentials provided by the environment, environment variable, or passed manually from Keyfactor Command.  You can read more about the first two options [here] (https://cloud.google.com/docs/authentication/production#automatically).

    To pass credentials from Keyfactor Command you need to first create a service account and then download a service account key.  Instructions are [here](https://cloud.google.com/docs/authentication/production#manually).  Remember to assign the appropriate role/permissions for the service account.  Afterwards inside Keyfactor Command copy and paste the contents of the service account key in the password field for the GCP Certificate Store Type.



    </details>

2. Create Certificate Store Types for the GCP Load Balancer Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # GCP Load Balancer
        kfutil store-types create GCPLoadBal
        ```

    * **Manually**:
        * [GCP Load Balancer](docs/gcploadbal.md#certificate-store-type-configuration)

3. Install the GCP Load Balancer Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e gcp-loadbalancer-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e gcp-loadbalancer-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [GCP Load Balancer Universal Orchestrator extension](https://github.com/Keyfactor/gcp-loadbalancer-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [GCP Load Balancer](docs/gcploadbal.md#certificate-store-configuration)



## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).