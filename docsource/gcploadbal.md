## Overview

The GCP Load Balancer Certificate Store Type in Keyfactor Command facilitates the management of SSL/TLS certificates specifically for GCP Load Balancers. This store type allows administrators to organize and handle certificates efficiently within the GCP ecosystem.

### Functions and Representation

The Certificate Store Type represents the logical grouping of certificates associated with specific GCP projects or regions. It supports three main job types: Inventory, Management Add, and Management Remove. These capabilities ensure that certificates can be discovered, added, and removed seamlessly within the GCP environment.

### Authentication and Parameters

To authenticate and interact with GCP, the Certificate Store Type utilizes a GCP service account key, which can be passed directly from Keyfactor Command. You can configure this using a specific custom parameter (`jsonKey`) when creating the Certificate Store Type. The store type expects a JSON-based service account key for authentication.

### Limitations and Considerations

When creating a Certificate Store in Keyfactor Command, it is essential to appropriately configure parameters such as the GCP project ID and region, along with the authentication details. The private key handling is required for adding certificates, as adding a certificate without its private key is not a valid use case for GCP Load Balancers.

### SDK and Caveats

While the readme does not explicitly mention an SDK, it relies on the GCP API for various operations such as listing, creating, and deleting certificates, as well as associating them with load balancers.

This store type does not have intricate caveats but requires careful configuration of credentials and parameters to ensure smooth operation. Additionally, interaction with GCP services mandates appropriate permissions for the service account used.

## Requirements

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

