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