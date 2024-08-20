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
#PASSWORD=$(tr -dc A-Za-z0-9 </dev/urandom 2>/dev/null | head -c 15)

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

#ssh -n -o BatchMode=yes -o StrictHostKeyChecking=no $publicIP "curl -s http://URL:8080/api/healthcheck"
```

#### Service principal
```bash
SUB_ID="748173f1-20c4-4e68-ac58-641f67a83501"
az account set --subscription $SUB_ID

# Create Service principal
az ad sp create-for-rbac -n "testReaderApp" --role reader --scopes "/subscriptions/$SUB_ID"
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

### App Registration and Service Principal for Workload identity 
```bash
githubOrganizationName='bpelikan'
githubRepositoryName='mslearn-test-bicep-code-using-github-actions'

# Create a workload identity
applicationRegistrationDetails=$(az ad app create --display-name 'mslearn-toy-website-test')
applicationRegistrationObjectId=$(echo $applicationRegistrationDetails | jq -r '.id')
applicationRegistrationAppId=$(echo $applicationRegistrationDetails | jq -r '.appId')

az ad app federated-credential create \
   --id $applicationRegistrationObjectId \
   --parameters "{\"name\":\"toy-website-test\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${githubOrganizationName}/${githubRepositoryName}:environment:Website\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

az ad app federated-credential create \
   --id $applicationRegistrationObjectId \
   --parameters "{\"name\":\"toy-website-test-branch\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${githubOrganizationName}/${githubRepositoryName}:ref:refs/heads/main\",\"audiences\":[\"api://AzureADTokenExchange\"]}"


# Create a resource group in Azure and grant the workload identity access
resourceGroupResourceId=$(az group create --name ToyWebsiteTest --location westus --query id --output tsv)

az ad sp create --id $applicationRegistrationObjectId
az role assignment create \
   --assignee $applicationRegistrationAppId \
   --role Contributor \
   --scope $resourceGroupResourceId

# Prepare GitHub secrets
echo "AZURE_CLIENT_ID: $applicationRegistrationAppId"
echo "AZURE_TENANT_ID: $(az account show --query tenantId --output tsv)"
echo "AZURE_SUBSCRIPTION_ID: $(az account show --query id --output tsv)"
```


#### Access Azure Instance Metadata Service
* [Access Azure Instance Metadata Service](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/instance-metadata-service?tabs=linux#access-azure-instance-metadata-service)

    `curl -s -H Metadata:true --noproxy "*" "http://169.254.169.254/metadata/instance?api-version=2021-02-01" | jq`


#### Get Access control (IAM) list
* [directoryObject: getByIds](https://docs.microsoft.com/en-us/graph/api/directoryobject-getbyids?view=graph-rest-1.0&tabs=http)
* [Authentication and authorization basics](https://docs.microsoft.com/en-us/graph/auth/auth-concepts)


```bash
az role assignment list --all 
az ad user show --id eff6dad5-9d3f-4e32-85b3-49729e48d66e
az ad user show --id 106be6a6-0240-45d9-aa44-ee8e1e43b53a

AZURE_SUBSCRIPTION_ID="748173f1-20c4-4e68-ac58-641f67a83501"
az rest -m get -u https://management.azure.com/subscriptions/$AZURE_SUBSCRIPTION_ID/providers/Microsoft.Authorization/roleAssignments?api-version=2015-07-01 
```

```json
{
   "ids":[
      "eff6dad5-9d3f-4e32-85b3-49729e48d66e",
      "106be6a6-0240-45d9-aa44-ee8e1e43b53a",
   ],
   "types":[
      "user",
      "group",
      "device"
   ]
}
```

```bash
az rest -m post -u 'https://graph.microsoft.com/v1.0/directoryObjects/getByIds' --body @body.json > response.json

```

```bash
 az group list --query "[?starts_with(name,'rg-testname')].[name]" --output tsv | xargs -L1 bash -c 'az group delete --name $0 --no-wait --yes'
```
