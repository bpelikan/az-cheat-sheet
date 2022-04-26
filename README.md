# az-cheat-sheet

```bash
az upgrade

az login

az account list --output table
az account set --subscription "SUB_ID"

az account list \
   --refresh \
   --query "[?contains(name, 'SUB_NAME')].id" \
   --output table

az configure --defaults group=RG_NAME
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

#### Access Azure Instance Metadata Service
* [Access Azure Instance Metadata Service](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/instance-metadata-service?tabs=linux#access-azure-instance-metadata-service)

    `curl -s -H Metadata:true --noproxy "*" "http://169.254.169.254/metadata/instance?api-version=2021-02-01" | jq`