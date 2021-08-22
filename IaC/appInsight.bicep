param location string = resourceGroup().location
param sku string = 'Standard' //'PerGB2018'

param enableLogAccessUsingOnlyResourcePermissions bool = true
param publicNetworkAccessForIngestion string = 'Enabled'
param publicNetworkAccessForQuery string = 'Enabled'
//param capacityReservationLevel int = 300
param retentionInDays int = 30

var logAwname = 'logAw-${uniqueString(resourceGroup().id)}' // must be globally unique
var appInsname = 'appInsdiplom4' 


resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: logAwname
  location: location
  tags: {
    'osv': 'osv'
  }
  properties: {
    sku: {
      name: sku
      //capacityReservationLevel: capacityReservationLevel
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: enableLogAccessUsingOnlyResourcePermissions
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: publicNetworkAccessForIngestion
    publicNetworkAccessForQuery: publicNetworkAccessForQuery
  }
}

resource appInsightsComponents 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: appInsname
  dependsOn: [
    logAnalyticsWorkspace
  ]
  location: location
  tags: {
    'osv': 'osv'
  }
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: publicNetworkAccessForIngestion
    publicNetworkAccessForQuery: publicNetworkAccessForQuery
  
  }
}

output logAnalyticsWorkspaceid string = logAnalyticsWorkspace.id
output appInsightsName string = appInsightsComponents.properties.Name
output appInsightsKey string = appInsightsComponents.properties.InstrumentationKey

