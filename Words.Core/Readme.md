# Readme

## Azure Setup

### Resource Group

```txt
az group create --name words --location westeurope
{
  "id": "/subscriptions/<...>/resourceGroups/words",
  "location": "westeurope",
  "managedBy": null,
  "name": "words",
  "properties": {
    "provisioningState": "Succeeded"
  },
  "tags": null,
  "type": "Microsoft.Resources/resourceGroups"
}
```

### App Service Plan

```txt
az appservice plan create --name words --resource-group words --is-linux --sku FREE
{
  "freeOfferExpirationTime": null,
  "geoRegion": "West Europe",
  "hostingEnvironmentProfile": null,
  "hyperV": false,
  "id": "/subscriptions/<...>/resourceGroups/words/providers/Microsoft.Web/serverfarms/words",
  "isSpot": false,
  "isXenon": false,
  "kind": "app",
  "location": "West Europe",
  "maximumElasticWorkerCount": 1,
  "maximumNumberOfWorkers": 1,
  "name": "words",
  "numberOfSites": 0,
  "perSiteScaling": false,
  "provisioningState": "Succeeded",
  "reserved": false,
  "resourceGroup": "words",
  "sku": {
    "capabilities": null,
    "capacity": 0,
    "family": "F",
    "locations": null,
    "name": "F1",
    "size": "F1",
    "skuCapacity": null,
    "tier": "Free"
  },
  "spotExpirationTime": null,
  "status": "Ready",
  "subscription": "<...>",
  "tags": null,
  "targetWorkerCount": 0,
  "targetWorkerSizeId": 0,
  "type": "Microsoft.Web/serverfarms",
  "workerTierName": null
}
```

### Website (Windows)

```txt
az webapp create --name words-core --resource-group words --plan words
{
  "availabilityState": "Normal",
  "clientAffinityEnabled": true,
  "clientCertEnabled": false,
  "clientCertExclusionPaths": null,
  "cloningInfo": null,
  "containerSize": 0,
  "dailyMemoryTimeQuota": 0,
  "defaultHostName": "words-core.azurewebsites.net",
  "enabled": true,
  "enabledHostNames": [
    "words-core.azurewebsites.net",
    "words-core.scm.azurewebsites.net"
  ],
  "ftpPublishingUrl": "ftp://waws-prod-am2-277.ftp.azurewebsites.windows.net/site/wwwroot",
  "geoDistributions": null,
  "hostNameSslStates": [
    {
      "hostType": "Standard",
      "ipBasedSslResult": null,
      "ipBasedSslState": "NotConfigured",
      "name": "words-core.azurewebsites.net",
      "sslState": "Disabled",
      "thumbprint": null,
      "toUpdate": null,
      "toUpdateIpBasedSsl": null,
      "virtualIp": null
    },
    {
      "hostType": "Repository",
      "ipBasedSslResult": null,
      "ipBasedSslState": "NotConfigured",
      "name": "words-core.scm.azurewebsites.net",
      "sslState": "Disabled",
      "thumbprint": null,
      "toUpdate": null,
      "toUpdateIpBasedSsl": null,
      "virtualIp": null
    }
  ],
  "hostNames": [
    "words-core.azurewebsites.net"
  ],
  "hostNamesDisabled": false,
  "hostingEnvironmentProfile": null,
  "httpsOnly": false,
  "hyperV": false,
  "id": "/subscriptions/<...>/resourceGroups/words/providers/Microsoft.Web/sites/words-core",
  "identity": null,
  "inProgressOperationId": null,
  "isDefaultContainer": null,
  "isXenon": false,
  "kind": "app",
  "lastModifiedTimeUtc": "2019-10-28T21:33:49.673333",
  "location": "West Europe",
  "maxNumberOfWorkers": null,
  "name": "words-core",
  "outboundIpAddresses": "13.69.68.12,104.45.28.230,104.45.17.199,40.91.220.139,40.91.218.91",
  "possibleOutboundIpAddresses": "13.69.68.12,104.45.28.230,104.45.17.199,40.91.220.139,40.91.218.91,52.232.113.216,40.91.218.4,104.45.8.62,51.144.117.82,168.63.12.69",
  "redundancyMode": "None",
  "repositorySiteName": "words-core",
  "reserved": false,
  "resourceGroup": "words",
  "scmSiteAlsoStopped": false,
  "serverFarmId": "/subscriptions/<...>/resourceGroups/words/providers/Microsoft.Web/serverfarms/words",
  "siteConfig": null,
  "slotSwapStatus": null,
  "state": "Running",
  "suspendedTill": null,
  "tags": null,
  "targetSwapSlot": null,
  "trafficManagerHostNames": null,
  "type": "Microsoft.Web/sites",
  "usageState": "Normal"
}
```

## Website (Linux)

```txt
az webapp create --name words-core --resource-group words --plan words --runtime "DOTNETCORE|3.0"
{
  "availabilityState": "Normal",
  "clientAffinityEnabled": true,
  "clientCertEnabled": false,
  "clientCertExclusionPaths": null,
  "cloningInfo": null,
  "containerSize": 0,
  "dailyMemoryTimeQuota": 0,
  "defaultHostName": "words-core.azurewebsites.net",
  "enabled": true,
  "enabledHostNames": [
    "words-core.azurewebsites.net",
    "words-core.scm.azurewebsites.net"
  ],
  "ftpPublishingUrl": "ftp://waws-prod-am2-273.ftp.azurewebsites.windows.net/site/wwwroot",
  "geoDistributions": null,
  "hostNameSslStates": [
    {
      "hostType": "Standard",
      "ipBasedSslResult": null,
      "ipBasedSslState": "NotConfigured",
      "name": "words-core.azurewebsites.net",
      "sslState": "Disabled",
      "thumbprint": null,
      "toUpdate": null,
      "toUpdateIpBasedSsl": null,
      "virtualIp": null
    },
    {
      "hostType": "Repository",
      "ipBasedSslResult": null,
      "ipBasedSslState": "NotConfigured",
      "name": "words-core.scm.azurewebsites.net",
      "sslState": "Disabled",
      "thumbprint": null,
      "toUpdate": null,
      "toUpdateIpBasedSsl": null,
      "virtualIp": null
    }
  ],
  "hostNames": [
    "words-core.azurewebsites.net"
  ],
  "hostNamesDisabled": false,
  "hostingEnvironmentProfile": null,
  "httpsOnly": false,
  "hyperV": false,
  "id": "/subscriptions/<...>/resourceGroups/words/providers/Microsoft.Web/sites/words-core",
  "identity": null,
  "inProgressOperationId": null,
  "isDefaultContainer": null,
  "isXenon": false,
  "kind": "app,linux",
  "lastModifiedTimeUtc": "2019-10-29T20:28:20.560000",
  "location": "West Europe",
  "maxNumberOfWorkers": null,
  "name": "words-core",
  "outboundIpAddresses": "13.69.68.11,104.45.2.57,137.117.162.115,51.145.157.213,40.114.230.242",
  "possibleOutboundIpAddresses": "13.69.68.11,104.45.2.57,137.117.162.115,51.145.157.213,40.114.230.242,137.117.211.198,13.80.131.89,137.117.173.224,13.94.133.227,40.115.15.39",
  "redundancyMode": "None",
  "repositorySiteName": "words-core",
  "reserved": true,
  "resourceGroup": "words",
  "scmSiteAlsoStopped": false,
  "serverFarmId": "/subscriptions/<...>/resourceGroups/words/providers/Microsoft.Web/serverfarms/words",
  "siteConfig": null,
  "slotSwapStatus": null,
  "state": "Running",
  "suspendedTill": null,
  "tags": null,
  "targetSwapSlot": null,
  "trafficManagerHostNames": null,
  "type": "Microsoft.Web/sites",
  "usageState": "Normal"
}
```
