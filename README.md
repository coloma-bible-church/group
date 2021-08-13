# Coloma Bible Church

Group conversations

# Deployment

## Initial setup

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
1. Push to GitHub and wait for the `Publish` action to complete
1. Sign into the [Azure portal](https://portal.azure.com/)
1. Create a phone number for sending/receiving SMS messages
   1. Navigate to Home > Subscriptions > _your subscription_ > Resources > `cbcgroup-prod-communication-service` > Voice Calling > Phone Numbers
   1. Click "+ Get" and follow the steps
   1. Make sure you get a number that can send and receive SMS messages
   1. Copy the phone number
1. Take note of the connection string for your communication service
   1. Navigate to Home > Subscriptions > _your subscription_ > Resources > `cbcgroup-prod-communication-service` > Tools > Keys
   1. Copy the Primary Key > Connection string
1. Navigate to your newly-created Azure app
   1. Sign into the [Azure portal](https://portal.azure.com/)
   1. Take note of 
   1. Navigate to Home > Subscriptions > _your subscription_ > Resources > `cbcgroup-prod-function-app`
1. Add some environment variables to your newly-created Azure Function app
   1. Navigate to Home > Subscriptions > _your subscription_ > Resources > `cbcgroup-prod-function-app` > Settings > Configuration
   1. Add these Application settings:
      |Name|Value|
      |-|-|
      |`CBC_GROUP_SMS_CONNECTION_STRING`|The connection string you copied above|
      |`CBC_GROUP_SMS_SERVICE_NUMBER`|The phone number you copied above|

## Ongoing changes

1. Install [Terraform CLI](https://www.terraform.io/downloads.html)
1. Install [.NET](https://dotnet.microsoft.com/)
1. Make your changes
   * After changing any `.tf` or `.tfvars` files:
     ```
     terraform fmt
     ```
   * After changing any C# files:
     ```
     dotnet test
     ```
1. Push to GitHub