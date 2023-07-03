# Azure Kubernetes Service


## Create AKS from CLI
```bash
LOCATION=westeurope
RESOURCE_GROUP=aks-demo2
CLUSTER_NAME=aks-demo2-$(date '+%Y%m%d')
SYSTEM_NODE_COUNT=1
USER_NODE_COUNT=1

az group create --name=$RESOURCE_GROUP --location=$LOCATION

VERSION=$(az aks get-versions --location $LOCATION --query 'orchestrators[?!isPreview] | [-1].orchestratorVersion' --output tsv)

az aks create \
    --resource-group $RESOURCE_GROUP \
    --name $CLUSTER_NAME \
    --location $LOCATION \
    --kubernetes-version $VERSION \
    --node-count $SYSTEM_NODE_COUNT \
    --load-balancer-sku standard \
    --vm-set-type VirtualMachineScaleSets \
    --node-vm-size Standard_B2s \
    --generate-ssh-keys

# domyślnie VM (bez parametru node-vm-size) -> Standard_DS2_v2
# --load-balancer-sku standard #basic LB nie jest wspierany dla multiple node pools
# -vm-set-type VirtualMachineScaleSets  #VMSS wymagane przy skalowaniu w AKS


az aks nodepool list --resource-group $RESOURCE_GROUP --cluster-name $CLUSTER_NAME

az aks nodepool add \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $CLUSTER_NAME \
    --name userpoolname \
    --node-count $USER_NODE_COUNT \
    --node-vm-size Standard_B2s \
    --no-wait

az aks nodepool list --resource-group $RESOURCE_GROUP --cluster-name $CLUSTER_NAME
```

## Scale node pool to 0
```bash
az aks nodepool scale \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $CLUSTER_NAME \
    --name userpoolname \
    --node-count 0 \
    --no-wait
```

## Create a spot node pool
```bash
az aks nodepool add \
    --resource-group $RESOURCE_GROUP \
    --cluster-name $CLUSTER_NAME \
    --name userspotpool \
    --enable-cluster-autoscaler \
    --max-count 3 \
    --min-count 1 \
    --priority Spot \
    --eviction-policy Delete \
    --spot-max-price 0.05 \
    --node-vm-size Standard_DS2_v2 \
    --no-wait

az aks nodepool show --resource-group $RESOURCE_GROUP --cluster-name $CLUSTER_NAME --name userspotpool
```

<details>
  <summary><b><i>output: az aks nodepool show</i></b></summary>

<!-- 
Spot nodes are configured with a label set to kubernetes.azure.com/scalesetpriority:spot.
Spot nodes are configured with a node taint set to kubernetes.azure.com/scalesetpriority=spot:NoSchedule

 -->

```json
{
  "availabilityZones": null,
  "count": 3,
  "creationData": null,
  "currentOrchestratorVersion": "1.26.3",
  "enableAutoScaling": true,
  "enableEncryptionAtHost": false,
  "enableFips": false,
  "enableNodePublicIp": false,
  "enableUltraSsd": false,
  "gpuInstanceProfile": null,
  "hostGroupId": null,
  "id": "/subscriptions/748173f1-20c4-4e68-ac58-641f67a83501/resourcegroups/aks-demo2/providers/Microsoft.ContainerService/managedClusters/aks-demo2-20230630/agentPools/userspotpool",
  "kubeletConfig": null,
  "kubeletDiskType": "OS",
  "linuxOsConfig": null,
  "maxCount": 3,
  "maxPods": 110,
  "minCount": 1,
  "mode": "User",
  "name": "userspotpool",
  "nodeImageVersion": "AKSUbuntu-2204gen2containerd-202306.19.0",
  "nodeLabels": {
    "kubernetes.azure.com/scalesetpriority": "spot"
  },
  "nodePublicIpPrefixId": null,
  "nodeTaints": [
    "kubernetes.azure.com/scalesetpriority=spot:NoSchedule"
  ],
  "orchestratorVersion": "1.26.3",
  "osDiskSizeGb": 128,
  "osDiskType": "Managed",
  "osSku": "Ubuntu",
  "osType": "Linux",
  "podSubnetId": null,
  "powerState": {
    "code": "Running"
  },
  "provisioningState": "Succeeded",
  "proximityPlacementGroupId": null,
  "resourceGroup": "aks-demo2",
  "scaleDownMode": "Delete",
  "scaleSetEvictionPolicy": "Delete",
  "scaleSetPriority": "Spot",
  "spotMaxPrice": 0.05,
  "tags": null,
  "type": "Microsoft.ContainerService/managedClusters/agentPools",
  "typePropertiesType": "VirtualMachineScaleSets",
  "upgradeSettings": {
    "maxSurge": null
  },
  "vmSize": "Standard_DS2_v2",
  "vnetSubnetId": null,
  "workloadRuntime": null
}
```
</details>



## Connect `kubectl` to AKS
```bash
az aks get-credentials --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP
kubectl get nodes


kubectl create namespace costsavings
kubectl apply --namespace costsavings -f spot-node-deployment.yaml
kubectl get pods --namespace costsavings -o wide
```

<!-- 
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
``` -->



## Delete
```bash
az group delete --name=$RESOURCE_GROUP --yes --no-wait
kubectl config delete-context $CLUSTER_NAME
```