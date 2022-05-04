# REST API


### 1. Przygotowanie środowiska
```bash
# get token
# az account get-access-token

# create Service principal
az ad sp create-for-rbac

curl -X POST -d 'grant_type=client_credentials&client_id=[APP_ID]&client_secret=[PASSWORD]&resource=https%3A%2F%2Fmanagement.azure.com%2F' https://login.microsoftonline.com/[TENANT_ID]/oauth2/token

curl -X GET -H "Authorization: Bearer [TOKEN]" -H "Content-Type: application/json" https://management.azure.com/subscriptions/[SUBSCRIPTION_ID]/providers/Microsoft.Web/sites?api-version=2016-08-01

az rest -m get --header "Accept=application/json" -u 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Web/sites?api-version=2016-08-01'
```



## Linki
* [Azure REST APIs with Postman (2021)](https://youtu.be/6b1J03fDnOg)
* [Create an Azure service principal with the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli)
* [Calling Azure REST API via curl](https://mauridb.medium.com/calling-azure-rest-api-via-curl-eb10a06127)

