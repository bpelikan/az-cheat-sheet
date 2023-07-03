## Create AKS from CLI
```bash
RESOURCE_GROUP=aks-demo1
CLUSTER_NAME=aks-demo1-$(date '+%Y%m%d')
LOCATION=westeurope
SYSTEM_NODE_COUNT=1
USER_NODE_COUNT=1

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
    --node-vm-size Standard_B2s \
    --no-wait
    
az aks nodepool list --resource-group $RESOURCE_GROUP --cluster-name $CLUSTER_NAME
```


## Connect `kubectl` to AKS
```bash
az aks get-credentials --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP
kubectl get nodes
```

## Deployment
```bash
kubectl apply -f ./deployment.yaml
# kubectl get deployment
kubectl apply -f ./service.yaml
# kubectl get service

ZoneName=$(az aks show -g $RESOURCE_GROUP -n $CLUSTER_NAME -o tsv --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName)
sed -i "s|<zone-name>|${ZoneName}|g" ./ingress.yaml

kubectl apply -f ./ingress.yaml
# kubectl get ingress
```



## Delete
```bash
az group delete --name=$RESOURCE_GROUP --yes --no-wait
kubectl config delete-context $CLUSTER_NAME
```