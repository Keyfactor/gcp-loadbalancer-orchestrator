## Overview

The Google Cloud Platform (GCP) Load Balancer Orchestrator allows for the management of Google Cloud Platform Load Balancer certificate stores. Inventory, Management-Add, and Management-Remove functions are supported. Also, re-binding to endpoints IS supported for certificate renewals (but NOT adding new certificates). The orchestrator uses the Google Cloud Compute Engine API (https://cloud.google.com/compute/docs/reference/rest/v1) to manage stores.


## Requirements

The orchestrator extension supports having credentials provided by the environment, environment variable, or passed manually from Keyfactor Command.  You can read more about the first two options [here](https://cloud.google.com/docs/authentication/production#automatically).

To pass credentials from Keyfactor Command you need to first create a service account within GCP and then download a [service account key](https://cloud.google.com/docs/authentication/set-up-adc-local-dev-environment#local-key)  Remember to assign the appropriate role/permissions for the service account (see below).  Afterwards inside Keyfactor Command copy and paste the contents of the service account key in the password field for the GCP Certificate Store you create.

The following are the required permissions for the GCP service account:
- compute.sslCertificates.create
- compute.sslCertificates.delete
- compute.sslCertificates.list
- compute.sslCertificates.get
- compute.targetHttpsProxies.list
- compute.targetHttpsProxies.setSslCertificates
- compute.regionSslCertificates.list

