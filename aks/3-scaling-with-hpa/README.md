## Create AKS from CLI
```bash
RESOURCE_GROUP=aks-demo3
CLUSTER_NAME=aks-demo3-$(date '+%Y%m%d')
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

# az aks nodepool add \
#     --resource-group $RESOURCE_GROUP \
#     --cluster-name $CLUSTER_NAME \
#     --name userpool \
#     --node-count $USER_NODE_COUNT \
#     --node-vm-size Standard_B2s \
#     --no-wait
    
az aks nodepool add \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $CLUSTER_NAME \
    --name userspotpool \
    --enable-cluster-autoscaler \
    --max-count 4 \
    --min-count 1 \
    --priority Spot \
    --eviction-policy Delete \
    --spot-max-price 0.05 \
    --node-vm-size Standard_DS2_v2 \
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
wget https://raw.githubusercontent.com/bpelikan/az-cheat-sheet/main/aks/3-scaling-with-hpa/deployment.yaml
wget https://raw.githubusercontent.com/bpelikan/az-cheat-sheet/main/aks/3-scaling-with-hpa/service.yaml
wget https://raw.githubusercontent.com/bpelikan/az-cheat-sheet/main/aks/3-scaling-with-hpa/ingress.yaml
wget https://raw.githubusercontent.com/bpelikan/az-cheat-sheet/main/aks/3-scaling-with-hpa/hpa.yaml

kubectl apply -f ./deployment.yaml
# kubectl get deployment
kubectl apply -f ./service.yaml
# kubectl get service

ZoneName=$(az aks show -g $RESOURCE_GROUP -n $CLUSTER_NAME -o tsv --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName)
sed -i "s|<zone-name>|${ZoneName}|g" ./ingress.yaml

kubectl apply -f ./ingress.yaml
# kubectl get ingress

kubectl apply -f ./hpa.yaml
# kubectl get hpa
```

## Scaling demo
```bash
hey -n 100000 -c 100 -m GET http://contoso.36757be42f33438892d2.westeurope.aksapp.io/
kubectl get pods -o wide
```


## Delete
```bash
az group delete --name=$RESOURCE_GROUP --yes --no-wait
kubectl config delete-context $CLUSTER_NAME
```