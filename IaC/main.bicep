param location string = resourceGroup().location

var rg_name = resourceGroup().name
var storageAccountname = 'staccblobtbldiplom4'
var kind = 'StorageV2'
var skuName = 'Standard_LRS'

var subscription = az.subscription()
var tenant = subscription.tenantId
var diagnosticName = 'service'

var tablename = 'tablediplom4'
var containername = '${storageAccountname}/default/diplom4data'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountname
  kind: kind
  location: location
  sku: {
    name: skuName
  
  }
  tags: {
    'osv': tablename
  }
  properties: {
    isHnsEnabled: true
    allowSharedKeyAccess: true
  }
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-02-01' = {
  name: containername
  dependsOn: [
    storageAccount
  ]
  properties: {
    publicAccess: 'None'
  }
}


// resource tblDeploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
//   name: 'tblDeploymentScript'
//   location: location
//   kind: 'AzureCLI'
//   properties: {
//     azCliVersion: '2.26.0'
//     retentionInterval: 'PT1H'
//     scriptContent: '''
//       az extension add --name storage-preview
//       az storage table create -n ${tablename} --account-name ${storageAccount.name}  --account-key ${storageAccount.listKeys().keys[0].value}
//     '''
//   }
// }

module appInsight 'appInsight.bicep' = {
  name: 'appInsightModule'
  params: {
    location: location
  }
}

module keVault 'keyVault.bicep' = {
  name: 'keyVaultModule'
  dependsOn: [
    appInsight
  ]
  params: {

    location: location
    tenant: tenant
    logAworkspaceId: appInsight.outputs.logAnalyticsWorkspaceid
    diagnosticName: diagnosticName
  }
}

// module webAppModule 'webApp.bicep' = {
//   name: 'webAppModule'
//   dependsOn: [
//     keVault
//   ]
//   params: {

//     location: location
//     //logAworkspaceId: appInsight.outputs.logAnalyticsWorkspaceid
//     //diagnosticName: diagnosticName
//     //appInsightsName: appInsight.outputs.appInsightsName
//     appInsightsKey: appInsight.outputs.appInsightsKey
//     storageAccountName: storageAccountname
//     storageAccountKey: storageAccount.listKeys().keys[0].value
//   }
// }

// module funcModule 'func.bicep' = {
//   name: 'funcModule'
//   dependsOn: [
//     keVault
//   ]
//   params: {
//     location: location
//     storageAccountname: storageAccount.name
//     storageAccountKey: storageAccount.listKeys().keys[0].value
//     appInsightsKey: appInsight.outputs.appInsightsKey

//   }
// }

// resource deploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
//   kind: 'AzurePowerShell'
//   location: location
//   name: 'DeploymentScript'
//   dependsOn: [
//     keVault
//   ]
//   identity: {
//     type: 'UserAssigned'
//     userAssignedIdentities: {
//       '/subscriptions/5e4c91fa-85b7-4cdf-8eb6-bf792337dbb9/resourceGroups/rg_blobsttbl_diplom1/providers/Microsoft.ManagedIdentity/userAssignedIdentities/mi_diplom1': {}
//     } 
//   }
//   properties: {
//     azPowerShellVersion: '3.0'
//     environmentVariables: [
//       {
//         name: 'rg_name'
//         value: rg_name
//       }
//       {
//         name: 'stacc_Id'
//         value: storageAccount.id
//       }
//     ]
//     scriptContent: '''
//       param( [string]$rg_name, [string]$stacc_Id )

//       Include-Module -Name Az.Resources
// <#
//       $Resources = Get-AzResource -ResourceGroupName $rg_name
//       $metric = New-AzDiagnosticDetailSetting -Metric -RetentionInDays 30 -RetentionEnabled -Category AllMetrics -Enabled
//       $log = New-AzDiagnosticDetailSetting -Log -RetentionInDays 30 -RetentionEnabled -Category AuditEvent -Enabled
//       foreach ($res in $Resources) {
//           $res_Id = $res.Id
//           $setting = New-AzDiagnosticSetting -TargetResourceId $res_Id -Name "giagdplm4" -StorageAccountId $stacc_Id -Setting $log,$metric
//       }
// #>
//       $setting = $rg_name

//       Write-Output $setting

//       $DeploymentScriptOutputs = @{}
//       $DeploymentScriptOutputs['text'] = $setting

//     '''
//     retentionInterval: 'P1D'
//     storageAccountSettings: {
//       storageAccountName: storageAccountname
//       storageAccountKey: storageAccount.listKeys().keys[0].value
//     }
//   }
// }
