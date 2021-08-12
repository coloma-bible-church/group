# Coloma Bible Church

Group conversations

# Deploy with GitHub Actions

The following has already been done for this repository. Inspired heavily by [this](https://www.blendmastersoftware.com/blog/deploying-to-azure-using-terraform-and-github-actions).

1. Set up Terraform
   1. Create an account
   1. Generate an API token<br/>
      https://www.terraform.io/docs/cloud/users-teams-organizations/api-tokens.html
   1. Save the token as a repository secret named `TF_API_TOKEN`<br/>
      https://help.github.com/en/actions/configuring-and-managing-workflows/creating-and-storing-encrypted-secrets
1. Set up Azure
   1. Create an account
   1. Generate a service principle<br/>
      https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guides/service_principal_client_secret
      1. Install Azure CLI<br/>
         https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
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