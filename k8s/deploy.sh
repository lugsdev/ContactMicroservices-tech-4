#!/bin/bash

# Script para deploy dos microserviços no Kubernetes

set -e

echo "🚀 Iniciando deploy dos microserviços de contatos no Kubernetes..."

# Definir variáveis
NAMESPACE="contact-microservices"
KUBECTL_CMD="kubectl"

# Verificar se kubectl está disponível
if ! command -v $KUBECTL_CMD &> /dev/null; then
    echo "❌ kubectl não encontrado. Instale o kubectl primeiro."
    exit 1
fi

# Verificar conexão com o cluster
echo "🔍 Verificando conexão com o cluster Kubernetes..."
if ! $KUBECTL_CMD cluster-info &> /dev/null; then
    echo "❌ Não foi possível conectar ao cluster Kubernetes."
    echo "   Verifique se o kubectl está configurado corretamente."
    exit 1
fi

echo "✅ Conectado ao cluster Kubernetes"

# Função para aplicar manifesto com verificação
apply_manifest() {
    local file=$1
    local description=$2
    
    echo "📦 Aplicando $description..."
    if $KUBECTL_CMD apply -f "$file"; then
        echo "✅ $description aplicado com sucesso"
    else
        echo "❌ Erro ao aplicar $description"
        exit 1
    fi
    echo ""
}

# 1. Criar namespace
apply_manifest "namespace.yaml" "Namespace"

# 2. Aplicar ConfigMaps
apply_manifest "configmap.yaml" "ConfigMaps"

# 3. Aplicar Secrets
apply_manifest "secrets.yaml" "Secrets"

# 4. Deploy SQL Server
apply_manifest "sqlserver.yaml" "SQL Server"

# 5. Deploy RabbitMQ
apply_manifest "rabbitmq.yaml" "RabbitMQ"

# 6. Aguardar SQL Server e RabbitMQ ficarem prontos
echo "⏳ Aguardando SQL Server ficar pronto..."
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/sqlserver -n $NAMESPACE

echo "⏳ Aguardando RabbitMQ ficar pronto..."
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/rabbitmq -n $NAMESPACE

# 7. Deploy dos microserviços
apply_manifest "contact-create-api.yaml" "ContactCreate API"
apply_manifest "contact-read-api.yaml" "ContactRead API"
apply_manifest "contact-update-api.yaml" "ContactUpdate API"
apply_manifest "contact-delete-api.yaml" "ContactDelete API"

# 8. Aguardar microserviços ficarem prontos
echo "⏳ Aguardando microserviços ficarem prontos..."
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/contact-create-api -n $NAMESPACE
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/contact-read-api -n $NAMESPACE
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/contact-update-api -n $NAMESPACE
$KUBECTL_CMD wait --for=condition=available --timeout=300s deployment/contact-delete-api -n $NAMESPACE

# 9. Aplicar Ingress (opcional)
if [ -f "ingress.yaml" ]; then
    echo "🌐 Aplicando Ingress..."
    if $KUBECTL_CMD apply -f "ingress.yaml"; then
        echo "✅ Ingress aplicado com sucesso"
    else
        echo "⚠️  Aviso: Erro ao aplicar Ingress (pode ser normal se o Ingress Controller não estiver instalado)"
    fi
    echo ""
fi

# 10. Mostrar status dos deployments
echo "📊 Status dos deployments:"
$KUBECTL_CMD get deployments -n $NAMESPACE

echo ""
echo "📊 Status dos services:"
$KUBECTL_CMD get services -n $NAMESPACE

echo ""
echo "📊 Status dos pods:"
$KUBECTL_CMD get pods -n $NAMESPACE

echo ""
echo "🎉 Deploy concluído com sucesso!"
echo ""
echo "📋 Comandos úteis:"
echo "   Ver logs: kubectl logs -f deployment/<deployment-name> -n $NAMESPACE"
echo "   Ver pods: kubectl get pods -n $NAMESPACE"
echo "   Deletar tudo: kubectl delete namespace $NAMESPACE"
echo ""
echo "🌐 URLs de acesso (se Ingress estiver configurado):"
echo "   API: https://api.contatos.com"
echo "   RabbitMQ Management: https://rabbitmq.contatos.com"

