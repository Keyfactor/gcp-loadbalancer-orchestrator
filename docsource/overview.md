## Overview

The GCP Load Balancer Universal Orchestrator extension enables Keyfactor Command to manage SSL/TLS certificates within Google Cloud Platform (GCP) Load Balancers. GCP Load Balancers ensure that users can securely access applications using HTTPS by utilizing SSL/TLS certificates. These certificates need to be managed efficiently to ensure uninterrupted, secure connections.

Within Keyfactor Command, defined Certificate Stores represent the configured SSL/TLS certificates that are managed in the GCP environment. These Certificate Stores can be thought of as logical groupings of certificates, which can be associated with specific GCP projects or regions. The Orchestrator's job is to automate the inventory, addition, and removal of these certificates, streamlining the certificate lifecycle management process for GCP Load Balancers.

