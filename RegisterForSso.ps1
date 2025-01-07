# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.

param(
  [Parameter(Mandatory=$true,
  HelpMessage="The friendly name of the app registration")]
  [String]
  $AppName
)

# Requires an admin
Connect-MgGraph -Scopes "Application.ReadWrite.All" -UseDeviceAuthentication -ErrorAction Stop

$graphAppId = "00000003-0000-0000-c000-000000000000"

# Create app registration
$appRegistration = New-MgApplication -DisplayName $AppName -SignInAudience "AzureADMyOrg" -ErrorAction Stop
Write-Host -ForegroundColor Cyan "App registration created with app ID" $appRegistration.AppId

# Create corresponding service principal
New-MgServicePrincipal -AppId $appRegistration.AppId -ErrorAction SilentlyContinue `
  -ErrorVariable SPError | Out-Null
if ($SPError)
{
  Write-Host -ForegroundColor Red "A service principal for the app could not be created."
  Write-Host -ForegroundColor Red $SPError
  Exit
}

Write-Host -ForegroundColor Cyan "Service principal created"

# Lookup available Graph application permissions
$graphServicePrincipal = Get-MgServicePrincipal -Filter ("appId eq '" + $graphAppId + "'") -ErrorAction Stop
$graphPermissions = $graphServicePrincipal.Oauth2PermissionScopes

$resourceAccess = @()

$mailPermission = $graphPermissions | Where-Object { $_.Value -eq "Mail.Send" }
if ($mailPermission)
{
  $resourceAccess += @{ Id = $mailPermission.Id; Type = "Scope" }
}
else
{
  Write-Host -ForegroundColor Red "Invalid scope: Mail.Send"
}

$userPermission = $graphPermissions | Where-Object { $_.Value -eq "User.Read" }
if ($userPermission)
{
  $resourceAccess += @{ Id = $userPermission.Id; Type = "Scope" }
}
else
{
  Write-Host -ForegroundColor Red "Invalid scope: User.Read"
}

# Set redirect to the Teams token store's redirect
$web = @{ RedirectUris = @("https://teams.microsoft.com/api/platform/v1.0/oAuthConsentRedirect") }

# Set API details
# Set Application ID URI
$appIdentifierUri = "api://" + $appRegistration.AppId

# Add the access_as_user scope
$scopeId = New-Guid
$permissionScopes = @(@{ AdminConsentDescription = "Allows an app to access Budget Tracker as a user"; `
 AdminConsentDisplayName = "Access Budget Tracker as the user"; Id = $scopeId; IsEnabled = $true; Type = "User"; `
 UserConsentDescription = "Allows an app to access Budget Tracker as you"; UserConsentDisplayName = "Access Budget Tracker as you"; Value = "access_as_user" })

$apiApplication = @{ Oauth2PermissionScopes = $permissionScopes }

Update-MgApplication -ApplicationId $appRegistration.Id -Api $apiApplication -IdentifierUris @($appIdentifierUri) -Web $web `
 -RequiredResourceAccess @{ ResourceAppId = $graphAppId; ResourceAccess = $resourceAccess } -ErrorAction Stop

# Add the Teams token store app ID as a pre-authorized app
# This must be done as a separate call because the scope has to be added to the app registration
# before you can pre-authorize an app to use it
$preAuth = @(@{ AppId = "ab3be6b7-f5df-413d-ac2d-abf1e3fd9c0b"; DelegatedPermissionIds = @($scopeId) })
Update-MgApplication -ApplicationId $appRegistration.Id -Api @{ PreAuthorizedApplications = $preAuth } -ErrorAction Stop

# Add a client secret
$clientSecret = Add-MgApplicationPassword -ApplicationId $appRegistration.Id -PasswordCredential `
 @{ DisplayName = "Added by PowerShell" } -ErrorAction Stop

Write-Host
Write-Host -ForegroundColor Green "SUCCESS"
Write-Host -ForegroundColor Cyan -NoNewline "Client ID: "
Write-Host -ForegroundColor Yellow $appRegistration.AppId
Write-Host -ForegroundColor Cyan -NoNewline "Client secret: "
Write-Host -ForegroundColor Yellow $clientSecret.SecretText
