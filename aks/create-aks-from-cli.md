# Azure Kubernetes Service


## Create AKS from CLI
```bash
export RESOURCE_GROUP=aks-demo
export CLUSTER_NAME=aks-demo-cluster
export LOCATION=westeurope
export SYSTEM_NODE_COUNT=1
export USER_NODE_COUNT=1

az group create --name=$RESOURCE_GROUP --location=$LOCATION

az aks create \
    --resource-group $RESOURCE_GROUP \
    --name $CLUSTER_NAME \
    --node-count $SYSTEM_NODE_COUNT \
    --enable-addons http_application_routing \
    --generate-ssh-keys \
    --node-vm-size Standard_B2s \
    --network-plugin azure

az aks nodepool add \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $CLUSTER_NAME \
    --name userpool \
    --node-count $USER_NODE_COUNT \
    --node-vm-size Standard_B2s
```


## Connect `kubectl` to AKS
```bash
az aks get-credentials --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP
# kubectl get nodes
# kubectl apply -f ./example-deployment.yaml
```



## Delete RG
```bash
az group delete --name=$RESOURCE_GROUP --no-wait
```