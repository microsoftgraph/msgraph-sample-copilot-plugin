# Budget Tracker sample API

The Budget Tracker API is a simple example of a REST API that can be used as the basis of an [API plugin](https://learn.microsoft.com/microsoft-365-copilot/extensibility/overview-api-plugins) for Microsoft Copilot for Microsoft 365.

## Features

- [ASP.NET Core minimal API](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview)
- [Secured with Microsoft Entra ID](https://learn.microsoft.com/entra/identity-platform/scenario-protected-web-api-app-configuration) using [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web)
- OpenAPI generated automatically with [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- Uses [on-behalf-of flow](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-on-behalf-of-flow) to obtain an access token for Microsoft Graph

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/) 8.x or later
- [devtunnel CLI](https://learn.microsoft.com/azure/developer/dev-tunnels) for debugging sample locally
- A Microsoft work or school account with an Exchange Online mailbox. If you don't have a Microsoft 365 tenant, you might qualify for one through the [Microsoft 365 Developer Program](https://developer.microsoft.com/microsoft-365/dev-program); for details, see the [FAQ](/office/developer-program/microsoft-365-developer-program-faq#who-qualifies-for-a-microsoft-365-e5-developer-subscription-). Alternatively, you can [sign up for a 1-month free trial or purchase a Microsoft 365 plan](https://www.microsoft.com/microsoft-365/try).

## Configure the sample

To configure the sample, you will need to generate a number of values. For clarity, these are how the values are referred to in the following instructions. Use this table to track the values as you generate them in the instructions that follow.

| Configuration field        | Generated value |
| -------------------------- | --------------- |
| **API base URL**           |                 |
| **Tenant ID**              |                 |
| **API client ID**          |                 |
| **API client secret**      |                 |
| **API scope**              |                 |
| **Plugin client ID**       |                 |
| **Plugin client secret**   |                 |
| **Authorization endpoint** |                 |
| **Token endpoint**         |                 |

### Create a persistent dev tunnel

If you do not have the `devtunnel` CLI installed, see [Create and host a dev tunnel](https://learn.microsoft.com/azure/developer/dev-tunnels/get-started#install) for installation instructions.

1. If you are not already logged in to the devtunnel CLI, use `devtunnel user login --help` to see the available login options. Login to the CLI before proceeding.

1. Create the tunnel.

    ```powershell
    devtunnel create --allow-anonymous
    ```

    Copy **Tunnel ID** from output.

1. Add HTTPS port from API (7196). Replace `tunnel-id` with ID copied in previous step.

    ```powershell
    devtunnel port create tunnel-id -p 7196 --protocol https
    ```

1. Start the dev tunnel. Replace `tunnel-id` with ID copied in previous step.

    ```powershell
    devtunnel host tunnel-id
    ```

1. For the first time running this dev tunnel, copy the URL labeled **Connect via browser**. Open this URL in your browser and select **Continue** to enable the tunnel.

1. Save the URL as your **API base URL**.

Once you have enabled the tunnel in your browser, you can stop the tunnel with **CTRL + C**. You can restart the tunnel with the `devtunnel host host-id` command.

### Register applications in Microsoft Entra admin center

This sample uses Microsoft Entra ID to secure the API. It is designed to run as a single-tenant application. Specifically,

- The API is registered as a web application that supports the [authorization code flow](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-auth-code-flow). The app registration defines an API scope that is required to access the API.
- The API plugin is registered as a web application that supports the authorization code flow. The app is configured with the API scope from the API registration.

1. Open your browser and navigate to the [Microsoft Entra admin center](https://entra.microsoft.com/). Sign in with a **Work or School Account**.
1. Select **Applications** in the left-hand navigation bar, then select **App registrations**.

#### Register the API

1. Select **New registration**. Enter `Budget Tracker Service` as the name for your application.

1. Set **Supported account types** to **Accounts in this organizational directory only**.

1. For **Redirect URI**, change the **Select a platform** dropdown to **Web**, and set the value to `<api-base-url>/authcomplete`, replacing `<api-base-url>` with your **API base URL**.

1. Select **Register**.

1. On the **Overview**, copy the **Directory (tenant) ID** and save as your **Tenant ID**.

1. Copy the **Application (client) ID** and save as your **API client ID**.

1. Select **Endpoints**, copy the **OAuth 2.0 authorization endpoint (v2)**, and save as your **Authorization endpoint**.

1. Copy the **OAuth 2.0 token endpoint (v2)**, and save as your **Token endpoint**. Close the **Endpoints** dialog.

1. Select **Certificates & secrets** in the left-hand navigation, then select **New client secret**.

1. Enter a **Description** and select a time period for the **Expires** field, then select **Add**.

1. Copy the **Value** field of the secret and save it as your **API client secret**.

    > [!IMPORTANT]
    > The **Value** field is never shown again after you leave this screen. Be sure to copy this value now before moving to the next step.

1. Select **API permissions** in the left-hand navigation, then select **Add a permission**.

1. Select **Microsoft Graph**, then **Delegated permissions**. Search for **Mail.Send** and enable it. Select **Add permissions**.

1. Select **Expose an API** in the left-hand navigation, then choose **Add a scope**.

1. Accept the default **Application ID URI** and choose **Save and continue**.

1. Fill in the **Add a scope** form as follows:

    - **Scope name:** `access_as_user`
    - **Who can consent?:** Admins and users
    - **Admin consent display name:** Access Budget Tracker as the user
    - **Admin consent description:** Allows an app to access Budget Tracker as a user
    - **User consent display name:** Access Budget Tracker as you
    - **User consent description:** Allows an app to access Budget Tracker as you
    - **State:** Enabled

1. Select **Add scope**. Copy the new scope and save it as your **API scope**.

#### Register the plugin

1. Return to the **App registrations** page in the Microsoft Entra admin center.

1. Select **New registration**. Enter `Budget Tracker Plugin` as the name for your application.

1. Set **Supported account types** to **Accounts in this organizational directory only**.

1. For **Redirect URI**, change the **Select a platform** dropdown to **Web**, and set the value to `https://teams.microsoft.com/api/platform/v1.0/oAuthRedirect`.

1. Select **Register**.

1. Copy the **Application (client) ID** and save as your **Plugin client ID**.

1. Select **Certificates & secrets** in the left-hand navigation, then select **New client secret**.

1. Enter a **Description** and select a time period for the **Expires** field, then select **Add**.

1. Copy the **Value** field of the secret and save it as your **Plugin client secret**.

    > [!IMPORTANT]
    > The **Value** field is never shown again after you leave this screen. Be sure to copy this value now before moving to the next step.

1. Select **API permissions** in the left-hand navigation, then select **Add a permission**.

1. Select **APIs my organization uses**. Search for and select **Budget Tracker Service**.

1. Enable **access_as_user**, then select **Add permissions**.

#### Update API registration

In this step, you add the **Plugin client ID** value to the API registration to enable a [combined consent experience](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-on-behalf-of-flow#default-and-combined-consent) when the user signs in to the plugin.

1. Return to the **App registrations** page in the Microsoft Entra admin center.

1. Locate and select the **Budget Tracker Service** app registration.

1. Select **Manifest** in the left-hand navigation.

1. In the editor, locate the `knownClientApplications` property. Add your **Plugin client ID** to this value, then select **Save**.

    ```json
    "knownClientApplications": [
        "your-plugin-client-id"
    ],
    ```

### Update the sample

1. Make a copy of [./api/appsettings.json](/api/appsettings.json) in the **./api** directory named **appsettings.Development.json**.

1. Open **appsettings.Development.json** and update the following fields using your [generated configuration values](#configure-the-sample).

    - Set `TenantId` to **Tenant ID**.
    - Set `ClientId` to **API client ID**.
    - Set `ClientSecret` to **API client secret**.
    - Set `Audience` to `api://{client-secret}`, replacing `{client-secret}` with **API client ID**.
    - Set `ServerUrl` to **API base URL**.

## Run the sample

You can run the sample in one of the following ways.

- From the command line in the **./api** directory with the command `dotnet run`.
- Open the root directory with Visual Studio Code and press **F5**.
- Open **./api/BudgetTracker.csproj** with Visual Studio and press **F5**.

Once the sample is running, you can test the API with one of these methods.

- [Create an API plugin](https://learn.microsoft.com/microsoft-365-copilot/extensibility/build-api-plugins-existing-api) and use Microsoft Copilot for Microsoft 365 to interact with the API. See [Sample Copilot prompts](#sample-copilot-prompts) for some prompts to get you started.
- [Register a device code app](/register-device-code.md) and use [BudgetTracker.http](./api/BudgetTracker.http) and the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) Visual Studio Code extension.

### Sample Copilot prompts

- How much is left in the fourth coffee lobby renovation budget?
- What is the status of existing budgets in budget tracker?
- Charge $500 to the Contoso Copilot plugin project for Megan Bowen's airfare
- Send me a transaction report
- What transactions have posted to existing budgets?

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
