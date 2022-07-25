# az-cheat-sheet

```bash
az upgrade

az login

az account list --output table
az account set --subscription "SUB_ID"
# az account set --subscription "sandbox"

az account list \
   --refresh \
   --query "[?contains(name, 'SUB_NAME')].id" \
   --output table

az configure --defaults group=RG_NAME
az configure --defaults group=''


az config get
az config unset defaults.group

subId=$(az account show --subscription "" | jq -r '.id')
```


#### Create VM and connect to SSH
```bash
RG_NAME="test-rg"
LOCATION="westeurope"
az group create --name $RG_NAME --location $LOCATION

export VMNAME=vm01
# export -p
export publicIP=$(az vm create \
    --name $VMNAME \
    --resource-group $RG_NAME \
    --image UbuntuLTS \
    --size Standard_B1ls \
    --generate-ssh-keys \
    --output tsv \
    --query "publicIpAddress")

export publicIP=$(az vm show \
    --name $VMNAME \
    --resource-group $RG_NAME \
    --show-details \
    --query [publicIps] \
    --output tsv)

ssh $publicIP
```

#### Service principal
```bash
SUB_ID="748173f1-20c4-4e68-ac58-641f67a83501"
az account set --subscription $SUB_ID

# Create Service principal
az ad sp create-for-rbac -n "testReaderApp" --role reader --scopes /subscriptions/$SUB_ID 
# Zapisujemy otrzymane dane w bezpiecznym miejscu

APP_ID="53a10bbf-b0d1-4c36-b1dc-31397e194cdc"
az ad sp show --id $APP_ID
az role assignment list --assignee $APP_ID --all
# az role assignment list --assignee $APP_ID --include-inherited --include-groups
# az ad sp delete --id $APP_ID

# sp list in tenant
# az ad sp list --all --query "[?appOwnerTenantId == '<TENANT_ID>']"


az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope RESOURCE_GROUP_ID \
  --description "Contributor role within the resource group."

az login --service-principal \
  --username $APP_ID \
  --password SERVICE_PRINCIPAL_KEY \
  --tenant TENANT_ID \
  --allow-no-subscriptions
  
az logout
```


#### Access Azure Instance Metadata Service
* [Access Azure Instance Metadata Service](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/instance-metadata-service?tabs=linux#access-azure-instance-metadata-service)

    `curl -s -H Metadata:true --noproxy "*" "http://169.254.169.254/metadata/instance?api-version=2021-02-01" | jq`
