# Event Grid


#### Event Grid and Storage account
```bash
RG_NAME="event-grid-sa-rg"
LOCATION="westeurope"
az group create --name $RG_NAME --location westeurope


sitename=webappbpevent0429
az deployment group create \
  --resource-group $RG_NAME  \
  --template-uri "https://raw.githubusercontent.com/Azure-Samples/azure-event-grid-viewer/master/azuredeploy.json" \
  --parameters siteName=$sitename hostingPlanName=viewerhost

az storage account create \
  --name stacceventbp0429 \
  --location $LOCATION \
  --resource-group $RG_NAME  \
  --sku Standard_LRS \
  --kind BlobStorage \
  --access-tier Hot


storageid=$(az storage account show --name stacceventbp0429  --resource-group $RG_NAME  --query id --output tsv)
endpoint=https://$sitename.azurewebsites.net/api/updates

az eventgrid event-subscription create \
  --source-resource-id $storageid \
  --name eventsubnmebp \
  --endpoint $endpoint


export AZURE_STORAGE_ACCOUNT=stacceventbp0429 
export AZURE_STORAGE_KEY="$(az storage account keys list --account-name <storage_account_name> --resource-group $RG_NAME --query "[0].value" --output tsv)"

az storage container create --name testcontainer

touch testfile.txt
az storage blob upload --file testfile.txt --container-name testcontainer --name testfile.txt


az group delete --name $RG_NAME  
```


#### Event Grid and Subscription
```bash
RG_NAME="event-grid-sub-rg"
LOCATION="westeurope"
az group create --name $RG_NAME --location westeurope

sitename=webappbpevent0429
az deployment group create \
  --resource-group $RG_NAME  \
  --template-uri "https://raw.githubusercontent.com/Azure-Samples/azure-event-grid-viewer/master/azuredeploy.json" \
  --parameters siteName=$sitename hostingPlanName=viewerhost


subid=/subscriptions/$(az account show --query id --output tsv)
echo $subid
endpoint=https://$sitename.azurewebsites.net/api/updates

az eventgrid event-subscription create \
  --source-resource-id $subid \
  --name eventsubnmebp \
  --endpoint $endpoint

az eventgrid event-subscription create \
  --source-resource-id /subscriptions/748173f1-20c4-4e68-ac58-641f67a83501 \
  --name eventsubsandbox \
  --endpoint $endpoint

az eventgrid event-subscription create \
  --source-resource-id /subscriptions/616bb79e-73be-40ca-bea5-219c413d4771 \
  --name eventsubpayg \
  --endpoint $endpoint
 

az eventgrid event-subscription create \
--source-resource-id /subscriptions/616bb79e-73be-40ca-bea5-219c413d4771 \
--name eventsubpaygwebhook \
--endpoint https://webhook.site/f8c4fdb5-9a99-4226-b110-4b33b5f31689


az group delete --name $RG_NAME
```



## Custom topics
* [Route custom events to web endpoint by using Azure CLI](https://docs.microsoft.com/en-us/learn/modules/azure-event-grid/8-event-grid-custom-events)

```bash
let rNum=$RANDOM*$RANDOM
myLocation=westeurope
myTopicName="az204-egtopic-${rNum}"
mySiteName="az204-egsite-${rNum}"
mySiteURL="https://${mySiteName}.azurewebsites.net"

az group create --name az204-evgrid-rg --location $myLocation

az eventgrid topic create --name $myTopicName \
    --location $myLocation \
    --resource-group az204-evgrid-rg

az deployment group create \
    --resource-group az204-evgrid-rg \
    --template-uri "https://raw.githubusercontent.com/Azure-Samples/azure-event-grid-viewer/main/azuredeploy.json" \
    --parameters siteName=$mySiteName hostingPlanName=viewerhost

echo "Your web app URL: ${mySiteURL}"


endpoint="${mySiteURL}/api/updates"
subId=$(az account show --subscription "" | jq -r '.id')

az eventgrid event-subscription create \
    --source-resource-id "/subscriptions/$subId/resourceGroups/az204-evgrid-rg/providers/Microsoft.EventGrid/topics/$myTopicName" \
    --name az204ViewerSub \
    --endpoint $endpoint


topicEndpoint=$(az eventgrid topic show --name $myTopicName -g az204-evgrid-rg --query "endpoint" --output tsv)
key=$(az eventgrid topic key list --name $myTopicName -g az204-evgrid-rg --query "key1" --output tsv)

event='[ {"id": "'"$RANDOM"'", "eventType": "recordInserted", "subject": "myapp/vehicles/motorcycles", "eventTime": "'`date +%Y-%m-%dT%H:%M:%S%z`'", "data":{ "make": "Contoso", "model": "Monster"},"dataVersion": "1.0"} ]'

curl -X POST -H "aeg-sas-key: $key" -d "$event" $topicEndpoint


az group delete --name az204-evgrid-rg --no-wait
```