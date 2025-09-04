# Guia de Deployment - Contact Microservices

Este documento fornece instruções detalhadas para deploy do sistema Contact Microservices em diferentes ambientes.

## Pré-requisitos

### Ferramentas Necessárias

- **Docker**: Versão 20.10 ou superior
- **Docker Compose**: Versão 2.0 ou superior
- **Kubernetes**: Versão 1.24 ou superior
- **kubectl**: Configurado e conectado ao cluster
- **.NET SDK**: Versão 8.0 (para desenvolvimento local)
- **Git**: Para clonagem do repositório

### Recursos de Infraestrutura

#### Desenvolvimento Local
- **CPU**: 4 cores
- **RAM**: 8GB
- **Disco**: 20GB livres
- **Rede**: Acesso à internet

#### Produção
- **CPU**: 8+ cores por nó
- **RAM**: 16GB+ por nó
- **Disco**: 100GB+ SSD
- **Rede**: Baixa latência entre nós

## Deployment Local com Docker Compose

### 1. Preparação do Ambiente

```bash
# Clonar o repositório
git clone https://github.com/your-username/ContactMicroservices.git
cd ContactMicroservices

# Verificar versões das ferramentas
docker --version
docker-compose --version
```

### 2. Configuração de Variáveis

Crie um arquivo `.env` na raiz do projeto:

```bash
# .env
SQLSERVER_SA_PASSWORD=ContactsDB@2024
RABBITMQ_DEFAULT_USER=admin
RABBITMQ_DEFAULT_PASS=admin123
ASPNETCORE_ENVIRONMENT=Development
```

### 3. Execução dos Serviços

```bash
# Executar todos os serviços
docker-compose up -d

# Verificar status
docker-compose ps

# Visualizar logs
docker-compose logs -f

# Parar serviços
docker-compose down
```

### 4. Verificação da Instalação

```bash
# Testar APIs
curl http://localhost:5001/api/contact/health  # ContactCreate
curl http://localhost:5002/api/contact/health  # ContactRead
curl http://localhost:5003/api/contact/health  # ContactUpdate
curl http://localhost:5004/api/contact/health  # ContactDelete

# Acessar RabbitMQ Management
# http://localhost:15672 (admin/admin123)
```

### 5. Inicialização do Banco de Dados

```bash
# Conectar ao SQL Server
docker exec -it contact-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024'

# Executar script de criação
docker exec -i contact-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024' < src/Shared/Infrastructure/Scripts/CreateDatabase.sql
```

## Deployment em Kubernetes

### 1. Preparação do Cluster

#### Verificar Conectividade

```bash
# Verificar conexão com o cluster
kubectl cluster-info

# Verificar nós disponíveis
kubectl get nodes

# Verificar recursos disponíveis
kubectl top nodes
```

#### Instalar Dependências

```bash
# Instalar NGINX Ingress Controller (se necessário)
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Verificar instalação
kubectl get pods -n ingress-nginx
```

### 2. Configuração de Secrets

```bash
# Criar secrets manualmente (alternativa ao manifesto)
kubectl create namespace contact-microservices

kubectl create secret generic contact-secrets \
  --from-literal=RABBITMQ__PASSWORD=Z3Vlc3Q= \
  --from-literal=CONNECTIONSTRINGS__DEFAULTCONNECTION=U2VydmVyPXNxbHNlcnZlci1zZXJ2aWNlO0RhdGFiYXNlPUNvbnRhY3RzREI7VXNlciBJZD1zYTtQYXNzd29yZD1Db250YWN0c0RCQDIwMjQ7VHJ1c3RTZXJ2ZXJDZXJ0aWZpY2F0ZT10cnVlOw== \
  -n contact-microservices

kubectl create secret generic sqlserver-secrets \
  --from-literal=SA_PASSWORD=Q29udGFjdHNEQkAyMDI0 \
  -n contact-microservices
```

### 3. Deploy Automatizado

```bash
# Navegar para diretório k8s
cd k8s

# Executar script de deploy
./deploy.sh

# Acompanhar progresso
watch kubectl get pods -n contact-microservices
```

### 4. Deploy Manual Passo a Passo

```bash
# 1. Namespace e configurações
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secrets.yaml

# 2. Infraestrutura
kubectl apply -f sqlserver.yaml
kubectl apply -f rabbitmq.yaml

# 3. Aguardar infraestrutura
kubectl wait --for=condition=available --timeout=300s deployment/sqlserver -n contact-microservices
kubectl wait --for=condition=available --timeout=300s deployment/rabbitmq -n contact-microservices

# 4. Microserviços
kubectl apply -f contact-create-api.yaml
kubectl apply -f contact-read-api.yaml
kubectl apply -f contact-update-api.yaml
kubectl apply -f contact-delete-api.yaml

# 5. Ingress (opcional)
kubectl apply -f ingress.yaml
```

### 5. Verificação do Deployment

```bash
# Status geral
kubectl get all -n contact-microservices

# Detalhes dos pods
kubectl describe pods -n contact-microservices

# Logs dos serviços
kubectl logs -f deployment/contact-create-api -n contact-microservices

# Testar conectividade interna
kubectl run test-pod --image=curlimages/curl -i --tty --rm -- sh
# Dentro do pod:
curl http://contact-create-api-service.contact-microservices.svc.cluster.local/api/contact/health
```

## Deployment em Produção

### 1. Preparação da Infraestrutura

#### Cluster Kubernetes

```bash
# Exemplo para AWS EKS
eksctl create cluster \
  --name contact-microservices-prod \
  --version 1.24 \
  --region us-west-2 \
  --nodegroup-name standard-workers \
  --node-type m5.large \
  --nodes 3 \
  --nodes-min 1 \
  --nodes-max 4 \
  --managed
```

#### Banco de Dados Gerenciado

```bash
# Exemplo para AWS RDS
aws rds create-db-instance \
  --db-instance-identifier contact-microservices-db \
  --db-instance-class db.t3.medium \
  --engine sqlserver-ex \
  --master-username sa \
  --master-user-password 'SecurePassword123!' \
  --allocated-storage 100 \
  --vpc-security-group-ids sg-12345678 \
  --db-subnet-group-name default
```

#### Message Broker Gerenciado

```bash
# Exemplo para AWS MQ
aws mq create-broker \
  --broker-name contact-microservices-mq \
  --engine-type RabbitMQ \
  --engine-version 3.9.16 \
  --host-instance-type mq.t3.micro \
  --users Username=admin,Password=SecurePassword123!
```

### 2. Configuração de Secrets de Produção

```bash
# Criar secrets com valores de produção
kubectl create secret generic contact-secrets-prod \
  --from-literal=RABBITMQ__PASSWORD=$(echo -n 'SecurePassword123!' | base64) \
  --from-literal=CONNECTIONSTRINGS__DEFAULTCONNECTION=$(echo -n 'Server=prod-db.region.rds.amazonaws.com;Database=ContactsDB;User Id=sa;Password=SecurePassword123!;TrustServerCertificate=true;' | base64) \
  -n contact-microservices
```

### 3. Configuração de Monitoramento

#### Prometheus e Grafana

```bash
# Instalar Prometheus Operator
kubectl apply -f https://raw.githubusercontent.com/prometheus-operator/prometheus-operator/main/bundle.yaml

# Configurar ServiceMonitor para os microserviços
cat <<EOF | kubectl apply -f -
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: contact-microservices
  namespace: contact-microservices
spec:
  selector:
    matchLabels:
      app.kubernetes.io/part-of: contact-system
  endpoints:
  - port: http
    path: /metrics
EOF
```

#### Logging com ELK Stack

```bash
# Instalar Elasticsearch
helm repo add elastic https://helm.elastic.co
helm install elasticsearch elastic/elasticsearch -n logging --create-namespace

# Instalar Kibana
helm install kibana elastic/kibana -n logging

# Instalar Filebeat
helm install filebeat elastic/filebeat -n logging
```

### 4. Configuração de Backup

#### Backup do Banco de Dados

```bash
# Script de backup automatizado
cat <<EOF > backup-database.sh
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
kubectl exec deployment/sqlserver -n contact-microservices -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024' \
  -Q "BACKUP DATABASE ContactsDB TO DISK = '/var/opt/mssql/backup/ContactsDB_\$DATE.bak'"
EOF

chmod +x backup-database.sh

# Configurar CronJob para backup
cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: CronJob
metadata:
  name: database-backup
  namespace: contact-microservices
spec:
  schedule: "0 2 * * *"  # Todo dia às 2h
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: mcr.microsoft.com/mssql-tools
            command: ["/bin/bash", "-c", "./backup-database.sh"]
          restartPolicy: OnFailure
EOF
```

### 5. Configuração de SSL/TLS

#### Cert-Manager

```bash
# Instalar cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.12.0/cert-manager.yaml

# Configurar ClusterIssuer para Let's Encrypt
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@empresa.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

## Estratégias de Deployment

### Blue-Green Deployment

```bash
# 1. Deploy da versão Green
kubectl apply -f k8s-green/

# 2. Testar versão Green
kubectl port-forward service/contact-create-api-service-green 8080:80 -n contact-microservices

# 3. Alternar tráfego
kubectl patch service contact-create-api-service -n contact-microservices -p '{"spec":{"selector":{"version":"green"}}}'

# 4. Remover versão Blue após validação
kubectl delete -f k8s-blue/
```

### Canary Deployment

```bash
# 1. Deploy da versão Canary (10% do tráfego)
cat <<EOF | kubectl apply -f -
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: contact-create-api
spec:
  replicas: 5
  strategy:
    canary:
      steps:
      - setWeight: 10
      - pause: {duration: 10m}
      - setWeight: 50
      - pause: {duration: 10m}
      - setWeight: 100
  selector:
    matchLabels:
      app: contact-create-api
  template:
    metadata:
      labels:
        app: contact-create-api
    spec:
      containers:
      - name: contact-create-api
        image: ghcr.io/your-username/contact-create-api:v2.0.0
EOF
```

### Rolling Update

```bash
# Atualizar imagem com rolling update
kubectl set image deployment/contact-create-api contact-create-api=ghcr.io/your-username/contact-create-api:v2.0.0 -n contact-microservices

# Acompanhar progresso
kubectl rollout status deployment/contact-create-api -n contact-microservices

# Rollback se necessário
kubectl rollout undo deployment/contact-create-api -n contact-microservices
```

## Troubleshooting

### Problemas Comuns

#### Pods não iniciam

```bash
# Verificar eventos
kubectl describe pod <pod-name> -n contact-microservices

# Verificar logs
kubectl logs <pod-name> -n contact-microservices

# Verificar recursos
kubectl top pods -n contact-microservices
```

#### Problemas de conectividade

```bash
# Testar DNS interno
kubectl run test-dns --image=busybox -i --tty --rm -- nslookup sqlserver-service.contact-microservices.svc.cluster.local

# Testar conectividade de rede
kubectl run test-network --image=nicolaka/netshoot -i --tty --rm -- bash
```

#### Problemas de performance

```bash
# Verificar métricas de recursos
kubectl top pods -n contact-microservices
kubectl top nodes

# Verificar limites de recursos
kubectl describe pod <pod-name> -n contact-microservices | grep -A 5 "Limits\|Requests"
```

### Logs e Debugging

```bash
# Logs em tempo real
kubectl logs -f deployment/contact-create-api -n contact-microservices

# Logs de todos os containers
kubectl logs -f deployment/contact-create-api --all-containers=true -n contact-microservices

# Executar shell em um pod
kubectl exec -it deployment/contact-create-api -n contact-microservices -- /bin/bash
```

## Manutenção

### Atualizações de Segurança

```bash
# Atualizar imagens base
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
docker pull mcr.microsoft.com/mssql/server:2022-latest
docker pull rabbitmq:3.12-management

# Rebuild e redeploy
./docker/build-all.sh
kubectl set image deployment/contact-create-api contact-create-api=ghcr.io/your-username/contact-create-api:latest -n contact-microservices
```

### Backup e Restore

```bash
# Backup completo
kubectl create job --from=cronjob/database-backup manual-backup-$(date +%s) -n contact-microservices

# Restore do banco
kubectl exec -i deployment/sqlserver -n contact-microservices -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'ContactsDB@2024' -Q "RESTORE DATABASE ContactsDB FROM DISK = '/var/opt/mssql/backup/ContactsDB_backup.bak' WITH REPLACE"
```

### Scaling

```bash
# Scale horizontal
kubectl scale deployment contact-read-api --replicas=5 -n contact-microservices

# Scale vertical (requer restart)
kubectl patch deployment contact-create-api -n contact-microservices -p '{"spec":{"template":{"spec":{"containers":[{"name":"contact-create-api","resources":{"requests":{"memory":"256Mi","cpu":"200m"},"limits":{"memory":"512Mi","cpu":"400m"}}}]}}}}'
```

Este guia fornece uma base sólida para deployment do sistema Contact Microservices em diferentes ambientes, desde desenvolvimento local até produção em larga escala.

