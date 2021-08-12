# Coloma Bible Church

Group conversations

# Deploy with GitHub Actions

The following has already been done for this repository. Inspired heavily by [this](https://www.blendmastersoftware.com/blog/deploying-to-azure-using-terraform-and-github-actions).

1. Create an Azure account
1. Install Azure CLI<br/>
   https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
1. Generate a service principle<br/>
   https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guides/service_principal_client_secret
   1. Login
      ```
      az login
      ```
   1. Find the subscription ID you want
      ```
      az account list
      ```
   1. Create the service principle
      ```
      az ad sp create-for-rbac --name "sp-tf-cbc-group" --role Contributor --scopes /subscriptions/<subscription id here> --sdk-auth
      ```
      This service principle will be what Terraform uses to create/destroy resources in Azure
   1. Save service principle repository secrets<br/>
      _Outputted in the form of `"clientSecret": "xxxx-xxxxxxxxxxxxxxxxxxxxxx-xxxxxx"`_
      |`az` command output|GitHub repository secret name|
      |-|-|
      |`clientId`|`TF_AZ_CLIENT_ID`|
      |`clientSecret`|`TF_AZ_CLIENT_SECRET`|
      |`subscriptionId`|`TF_AZ_SUBSCRIPTION_ID`|
      |`tenantId`|`TF_AZ_TENANT_ID`|
1. Create a storage account and container
   ```
   az group create -g rg-tf-cbc-group -l northcentralus
   az storage account create -n satfcbcgroup -g rg-tf-cbc-group -l northcentralus --sku Standard_LRS
   az storage container create -n terraform-state --account-name satfcbcgroup
   ```
   This storage account and container will be used to store Terraform's state in Azure