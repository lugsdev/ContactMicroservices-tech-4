# Kubernetes Deployment - Contact Microservices

Este diretório contém todos os manifestos Kubernetes necessários para deploy dos microserviços de contatos.

## Estrutura dos Arquivos

### Configuração Base
- `namespace.yaml` - Namespace dedicado para os microserviços
- `configmap.yaml` - Configurações compartilhadas (não-sensíveis)
- `secrets.yaml` - Informações sensíveis (senhas, connection strings)

### Infraestrutura
- `sqlserver.yaml` - SQL Server com PersistentVolume
- `rabbitmq.yaml` - RabbitMQ com Management UI

### Microserviços
- `contact-create-api.yaml` - Serviço de criação de contatos
- `contact-read-api.yaml` - Serviço de leitura de contatos
- `contact-update-api.yaml` - Serviço de atualização de contatos
- `contact-delete-api.yaml` - Serviço de exclusão de contatos

### Exposição
- `ingress.yaml` - Ingress para expor os serviços externamente

### Scripts
- `deploy.sh` - Script automatizado de deploy
- `README.md` - Esta documentação

## Pré-requisitos

1. **Cluster Kubernetes** funcionando
2. **kubectl** configurado e conectado ao cluster
3. **Ingress Controller** (opcional, para exposição externa)
4. **StorageClass** `standard` disponível (para PersistentVolumes)

## Deploy Rápido

```bash
# Executar script automatizado
./deploy.sh
```

## Deploy Manual

```bash
# 1. Criar namespace
kubectl apply -f namespace.yaml

# 2. Aplicar configurações
kubectl apply -f configmap.yaml
kubectl apply -f secrets.yaml

# 3. Deploy da infraestrutura
kubectl apply -f sqlserver.yaml
kubectl apply -f rabbitmq.yaml

# 4. Aguardar infraestrutura ficar pronta
kubectl wait --for=condition=available --timeout=300s deployment/sqlserver -n contact-microservices
kubectl wait --for=condition=available --timeout=300s deployment/rabbitmq -n contact-microservices

# 5. Deploy dos microserviços
kubectl apply -f contact-create-api.yaml
kubectl apply -f contact-read-api.yaml
kubectl apply -f contact-update-api.yaml
kubectl apply -f contact-delete-api.yaml

# 6. Aplicar Ingress (opcional)
kubectl apply -f ingress.yaml
```

## Configurações Importantes

### Labels e Anotações

Todos os recursos seguem o padrão de labels recomendado pelo Kubernetes:

```yaml
labels:
  app.kubernetes.io/name: <nome-do-componente>
  app.kubernetes.io/version: "1.0.0"
  app.kubernetes.io/component: <tipo-do-componente>
  app.kubernetes.io/part-of: contact-system
  app.kubernetes.io/managed-by: kubectl
```

### Recursos e Limites

Cada microserviço tem recursos definidos:
- **Requests**: 128Mi RAM, 100m CPU
- **Limits**: 256Mi RAM, 200m CPU

### Health Checks

Todos os serviços têm:
- **Liveness Probe**: Verifica se o container está funcionando
- **Readiness Probe**: Verifica se o serviço está pronto para receber tráfego

### Segurança

- Containers executam como usuário não-root
- Filesystem read-only
- Capabilities desnecessárias removidas
- Secrets para informações sensíveis

## Monitoramento

### Verificar Status

```bash
# Status geral
kubectl get all -n contact-microservices

# Logs de um serviço específico
kubectl logs -f deployment/contact-create-api -n contact-microservices

# Descrever um pod
kubectl describe pod <pod-name> -n contact-microservices
```

### Métricas

Os pods estão configurados para exposição de métricas Prometheus:
- Porta: 8080
- Path: /metrics

## Acesso aos Serviços

### Interno (dentro do cluster)

```bash
# ContactCreate API
http://contact-create-api-service.contact-microservices.svc.cluster.local

# ContactRead API  
http://contact-read-api-service.contact-microservices.svc.cluster.local

# ContactUpdate API
http://contact-update-api-service.contact-microservices.svc.cluster.local

# ContactDelete API
http://contact-delete-api-service.contact-microservices.svc.cluster.local
```

### Externo (com Ingress)

```bash
# APIs (configurar DNS para apontar para o Ingress)
https://api.contatos.com/create
https://api.contatos.com/read
https://api.contatos.com/update
https://api.contatos.com/delete

# RabbitMQ Management
https://rabbitmq.contatos.com
```

### Port Forward (para desenvolvimento)

```bash
# ContactCreate API
kubectl port-forward service/contact-create-api-service 8001:80 -n contact-microservices

# RabbitMQ Management
kubectl port-forward service/rabbitmq-management-service 15672:80 -n contact-microservices
```

## Escalabilidade

### Escalar Horizontalmente

```bash
# Escalar ContactRead API para 5 réplicas
kubectl scale deployment contact-read-api --replicas=5 -n contact-microservices

# Escalar todos os microserviços
kubectl scale deployment contact-create-api --replicas=3 -n contact-microservices
kubectl scale deployment contact-read-api --replicas=5 -n contact-microservices
kubectl scale deployment contact-update-api --replicas=3 -n contact-microservices
kubectl scale deployment contact-delete-api --replicas=2 -n contact-microservices
```

### Horizontal Pod Autoscaler (HPA)

```bash
# Criar HPA para ContactRead API
kubectl autoscale deployment contact-read-api --cpu-percent=70 --min=2 --max=10 -n contact-microservices
```

## Troubleshooting

### Problemas Comuns

1. **Pods não iniciam**
   ```bash
   kubectl describe pod <pod-name> -n contact-microservices
   kubectl logs <pod-name> -n contact-microservices
   ```

2. **Problemas de conectividade**
   ```bash
   # Testar conectividade entre pods
   kubectl exec -it <pod-name> -n contact-microservices -- nslookup sqlserver-service
   ```

3. **Problemas de storage**
   ```bash
   kubectl get pv,pvc -n contact-microservices
   ```

### Limpeza

```bash
# Deletar tudo
kubectl delete namespace contact-microservices

# Deletar apenas os microserviços
kubectl delete deployment contact-create-api contact-read-api contact-update-api contact-delete-api -n contact-microservices
```

## Backup e Restore

### SQL Server

```bash
# Backup
kubectl exec -it deployment/sqlserver -n contact-microservices -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024' -Q "BACKUP DATABASE ContactsDB TO DISK = '/var/opt/mssql/backup/ContactsDB.bak'"

# Restore
kubectl exec -it deployment/sqlserver -n contact-microservices -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024' -Q "RESTORE DATABASE ContactsDB FROM DISK = '/var/opt/mssql/backup/ContactsDB.bak'"
```

## Atualizações

### Rolling Update

```bash
# Atualizar imagem de um microserviço
kubectl set image deployment/contact-create-api contact-create-api=ghcr.io/your-username/contact-create-api:v2.0.0 -n contact-microservices

# Verificar status do rollout
kubectl rollout status deployment/contact-create-api -n contact-microservices

# Rollback se necessário
kubectl rollout undo deployment/contact-create-api -n contact-microservices
```

