# Contact Microservices - Sistema de Cadastro de Contatos

[![Build Status](https://github.com/your-username/ContactMicroservices/workflows/Build%20Solution/badge.svg)](https://github.com/your-username/ContactMicroservices/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-blue.svg)](https://kubernetes.io/)

Um sistema completo de microserviços para cadastro e gerenciamento de contatos, desenvolvido em C# .NET 8 com arquitetura moderna e práticas DevOps avançadas.

## 🏗️ Arquitetura

Este projeto implementa uma arquitetura de microserviços onde cada operação CRUD é separada em um serviço independente:

- **ContactCreate.API** - Responsável pela criação de novos contatos
- **ContactRead.API** - Responsável pela leitura e consulta de contatos
- **ContactUpdate.API** - Responsável pela atualização de contatos existentes
- **ContactDelete.API** - Responsável pela exclusão de contatos

### Tecnologias Utilizadas

- **Backend**: C# .NET 8, ASP.NET Core Web API
- **ORM**: Dapper para acesso a dados
- **Banco de Dados**: SQL Server
- **Mensageria**: RabbitMQ para comunicação assíncrona
- **Containerização**: Docker e Docker Compose
- **Orquestração**: Kubernetes com ConfigMaps e Secrets
- **CI/CD**: GitHub Actions
- **Monitoramento**: Health Checks, Prometheus metrics

## 📋 Funcionalidades

### Operações CRUD Completas
- ✅ Criação de contatos com validação
- ✅ Leitura individual e listagem paginada
- ✅ Busca por nome, email, DDD ou número
- ✅ Atualização completa e parcial
- ✅ Exclusão individual e em lote

### Recursos Avançados
- ✅ Comunicação assíncrona via eventos RabbitMQ
- ✅ Validação de email único
- ✅ Paginação e filtros de busca
- ✅ Health checks para monitoramento
- ✅ Logs estruturados
- ✅ Métricas para Prometheus

## 🚀 Início Rápido

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Executar com Docker Compose

```bash
# Clonar o repositório
git clone https://github.com/your-username/ContactMicroservices.git
cd ContactMicroservices

# Executar todos os serviços
docker-compose up -d

# Verificar status
docker-compose ps
```

### Executar Localmente

```bash
# Restaurar dependências
dotnet restore

# Executar SQL Server e RabbitMQ
docker-compose up -d sqlserver rabbitmq

# Executar cada microserviço (em terminais separados)
cd src/ContactCreate.API && dotnet run
cd src/ContactRead.API && dotnet run
cd src/ContactUpdate.API && dotnet run
cd src/ContactDelete.API && dotnet run
```

## 📡 Endpoints da API

### ContactCreate.API (Porta 5001)
```http
POST /api/contact
Content-Type: application/json

{
  "nome": "João Silva",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.silva@email.com"
}
```

### ContactRead.API (Porta 5002)
```http
# Buscar por ID
GET /api/contact/{id}

# Listar com paginação
GET /api/contact?page=1&pageSize=10&search=joão
```

### ContactUpdate.API (Porta 5003)
```http
# Atualização completa
PUT /api/contact/{id}
Content-Type: application/json

{
  "nome": "João Santos",
  "ddd": "11",
  "numeroCelular": "987654321",
  "email": "joao.santos@email.com"
}

# Atualização parcial
PATCH /api/contact/{id}
Content-Type: application/json

{
  "nome": "João Santos"
}
```

### ContactDelete.API (Porta 5004)
```http
# Exclusão individual
DELETE /api/contact/{id}

# Exclusão em lote
DELETE /api/contact/batch
Content-Type: application/json

{
  "ids": [1, 2, 3]
}
```

## 🐳 Docker

### Imagens Disponíveis

- `contactmicroservices/contact-create-api`
- `contactmicroservices/contact-read-api`
- `contactmicroservices/contact-update-api`
- `contactmicroservices/contact-delete-api`

### Build das Imagens

```bash
# Build de todas as imagens
./docker/build-all.sh

# Build individual
docker build -f docker/Dockerfile.ContactCreate -t contact-create-api .
```

## ☸️ Kubernetes

### Deploy Rápido

```bash
# Deploy completo
cd k8s
./deploy.sh
```

### Deploy Manual

```bash
# Aplicar manifestos
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/sqlserver.yaml
kubectl apply -f k8s/rabbitmq.yaml
kubectl apply -f k8s/contact-create-api.yaml
kubectl apply -f k8s/contact-read-api.yaml
kubectl apply -f k8s/contact-update-api.yaml
kubectl apply -f k8s/contact-delete-api.yaml
kubectl apply -f k8s/ingress.yaml
```

### Verificar Status

```bash
kubectl get all -n contact-microservices
```

## 🔧 Configuração

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|---------|
| `ConnectionStrings__DefaultConnection` | String de conexão SQL Server | - |
| `RabbitMQ__HostName` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__Port` | Porta do RabbitMQ | `5672` |
| `RabbitMQ__UserName` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` |

### Configuração do Banco de Dados

Execute o script SQL para criar o banco:

```sql
-- Localizado em: src/Shared/Infrastructure/Scripts/CreateDatabase.sql
USE master;
CREATE DATABASE ContactsDB;
```

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Monitoramento

### Health Checks

Cada microserviço expõe endpoints de health check:

```http
GET /api/contact/health
```

### Métricas Prometheus

Métricas disponíveis em:

```http
GET /metrics
```

### RabbitMQ Management

Interface web disponível em:

```http
http://localhost:15672
Usuário: guest
Senha: guest
```

## 🚀 CI/CD

O projeto inclui workflows GitHub Actions para:

- ✅ Build e teste automatizado
- ✅ Análise de qualidade de código
- ✅ Security scanning
- ✅ Build e push de imagens Docker
- ✅ Deploy automático para staging e produção

### Configurar Secrets

Configure os seguintes secrets no GitHub:

- `SONAR_TOKEN` - Token do SonarCloud (opcional)
- `DOCKER_USERNAME` - Usuário Docker Hub (opcional)
- `DOCKER_PASSWORD` - Senha Docker Hub (opcional)

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

