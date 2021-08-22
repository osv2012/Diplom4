param location string = resourceGroup().location
param tenant string
param sku string = 'Standard'
param logAworkspaceId string
param diagnosticName string

param createMode string = 'default'
param enabledForDeployment bool = false
param enabledForTemplateDeployment bool = false
param enabledForDiskEncryption bool = false
//param enablePurgeProtection bool = false
param enableRbacAuthorization bool = false
param enableSoftDelete bool = true
param softDeleteRetentionInDays int = 90
param provisioningState string = 'Succeeded'

var name = 'kv-diplom4'
//var vaultUri = 'https://${name}.vault.azure.net/'

param networkAcls object = {
  ipRules: []
  virtualNetworkRules: []
}


resource shareKeyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' = {
  name: name
  location: location
  tags: {
    'osv': 'osv'
  }
  properties: {
    accessPolicies: []
    createMode: createMode
    enabledForDeployment: enabledForDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enabledForTemplateDeployment: enabledForTemplateDeployment
    //enablePurgeProtection: enablePurgeProtection
    enableRbacAuthorization: enableRbacAuthorization
    enableSoftDelete: enableSoftDelete
    networkAcls: networkAcls
    provisioningState: provisioningState
    sku: {
      family: 'A'
      name: sku
    }
    softDeleteRetentionInDays: softDeleteRetentionInDays
    tenantId: tenant
    //vaultUri: vaultUri
  }
}

resource shareKeyVaultDiagnostic 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: diagnosticName
  scope: shareKeyVault
  properties: {
    workspaceId: logAworkspaceId
    logs: [
      {
        category: 'AuditEvent'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  } 
}

