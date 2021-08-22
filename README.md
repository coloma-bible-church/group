# Coloma Bible Church group conversations

This software lets a group of people chat among themselves.

Right now it only supports SMS messages to/from a single group number, but soon there will be email support and more.

# APIs

Swagger UI automatically generates API documentation for the various web API controllers. Just navigate to the root of any deployed web app.

## Manage users

1. Authenticate with the `Group.Hub` API
   - If necessary, POST the server secret to `auth/secret`
1. Use API endpoints<br/>
   |URL|Verb|Description|
   |-|-|-|
   |`api/v1/users`|`GET`|List pointers to all users|
   |`api/v1/users/{id}`|`GET`|Get details for this user|
   |`api/v1/users/{id}`|`PUT`|Assign details for this user. Creates a new user if none existed before, and overwrites any existing user information|
   |`api/v1/users/{id}`|`DELETE`|Deletes this user|
   |`api/v1/contacts/{kind}/{value}`|`GET`|Gets a pointer to the user having these contact details. E.g. `api/v1/contacts/sms/+11234567890` will return a pointer to the user configured to have that SMS phone number|

# Deployment

## Tooling

You'll need to have the following tools installed:

 * [.NET](https://dotnet.microsoft.com/)
 * [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
 * [Terraform CLI](https://www.terraform.io/downloads.html)

## Initial setup

The following has already been done for this repository:

### Set up Twilio

1. Create a Twilio account<br/>
   Both of us will get $10 if you use this link: www.twilio.com/referral/DrzIKI
1. Get an SMS-capable phone number
1. Create a Messaging Service<br/>
   https://www.twilio.com/console/sms/services
1. Save these as GitHub repository secrets:<br/>
   |GitHub repository secret name|Twilio|
   |-|-|
   |`TWILIO_ACCOUNT_SID`|Your Twilio account ID|
   |`TWILIO_MESSAGING_SERVICE_ID`|Your messaging service ID|
   |`TWILIO_AUTH_TOKEN`|Your Twilio account auth token|

### Add more repository secrets

|GitHub repository secret name|Value|
|-|-|
|`SERVER_SECRET`|A random 512-bit number, encoded as a base64 string|
|`TWILIO_CONNECTION_SECRET`|A random GUID|
|`TWILIO_SERVER_SECRET`|_leave blank for now_|

### Set up Azure

1. Create an Azure account
1. Generate a service principle using the Azure CLI<br/>
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
      |GitHub repository secret name|`az` command output|
      |-|-|
      |`TF_AZ_CLIENT_ID`|`clientId`|
      |`TF_AZ_CLIENT_SECRET`|`clientSecret`|
      |`TF_AZ_SUBSCRIPTION_ID`|`subscriptionId`|
      |`TF_AZ_TENANT_ID`|`tenantId`|
1. Create a storage account and container
   ```
   az group create -g rg-tf-cbc-group -l northcentralus
   az storage account create -n satfcbcgroup -g rg-tf-cbc-group -l northcentralus --sku Standard_LRS
   az storage container create -n terraform-state --account-name satfcbcgroup
   ```
   This storage account and container will be used to store Terraform's state in Azure

### Finalize

1. Push to GitHub and wait for the `Publish` action to complete
1. Navigate to your deployed `Group.Hub` web app. Using the Swagger API page that comes up:
   1. Authenticate yourself by `POST`ing a message to the `/auth/secret` endpoint
      - Use the same base64-encoded random value that you put in the `SERVER_SECRET` GitHub repository secret
      - If you forgot what that was, you can always inspect the `SERVER_SECRET` environment variable that is configured on the `Group.Hub` web app in Azure
   1. Create a new SMS connection by `PUT`ing a connection to the `/api/v1/connections/sms` endpoint
      - `connectionEndpoint` will be the full URL to `Group.Twilio`'s `SmsController.ReceiveFromHubAsync` method. E.g. "https://_your-app-name-here_.azurewebsites.net/api/v1/sms/connection"
      - `connectionSecret` will be the GUID that you put in the `TWILIO_CONNECTION_SECRET` GitHub repository secret. If you forgot what that was you can inspect the `CONNECTION_SECRET` environment variable that is configured on the `Group.Twilio` web app in Azure
   1. Copy the `serverSecret` value from the response and put it into the `TWILIO_SERVER_SECRET` GitHub repository secret
1. Point Twilio to your deployed `Group.Twilio` web app
   1. https://www.twilio.com/console/sms/services
   1. Choose your Messaging Service
   1. Click "Integration" in the left menu
   1. Set "Incoming Messages" to "Send a webhook"
      - "Request URL" should be HTTP **POST** to the full URL to `Group.Twilio`'s `SmsController.ReceiveFromTwilio` method. E.g. "https://_your-app-name-here_.azurewebsites.net/api/v1/sms/twilio"
      - "Fallback URL" should be HTTP **GET** to the full URL "https://raw.githubusercontent.com/coloma-bible-church/group/stage/twilio.error.txt" (modify this link if you have forked this repository)
   1. Click "Save"
1. Add some users (see above)
1. Send a test message

## Ongoing changes

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
1. Wait for GitHub Actions to complete