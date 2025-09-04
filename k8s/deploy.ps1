# Deploy simplificado dos microserviços no Kubernetes (PowerShell)

$ErrorActionPreference = "Stop"

$Namespace = "contact-microservices"
$KubectlCmd = "kubectl"

Write-Host "🚀 Iniciando deploy dos microserviços de contatos no Kubernetes..."

# Verificar se kubectl está disponível
if (-not (Get-Command $KubectlCmd -ErrorAction SilentlyContinue)) {
    Write-Host "❌ kubectl não encontrado. Instale o kubectl primeiro."
    exit 1
}

# Verificar conexão com o cluster
Write-Host "🔍 Verificando conexão com o cluster Kubernetes..."
try {
    & $KubectlCmd cluster-info | Out-Null
    Write-Host "✅ Conectado ao cluster Kubernetes"
} catch {
    Write-Host "❌ Não foi possível conectar ao cluster Kubernetes."
    Write-Host "   Verifique se o kubectl está configurado corretamente."
    exit 1
}

# 1. Criar namespace
Write-Host "📦 Aplicando Namespace..."
& $KubectlCmd apply -f "namespace.yaml"
Write-Host "✅ Namespace aplicado`n"

# 2. Aplicar ConfigMaps
Write-Host "📦 Aplicando ConfigMaps..."
& $KubectlCmd apply -f "configmap.yaml"
Write-Host "✅ ConfigMaps aplicados`n"

# 3. Aplicar Secrets
Write-Host "📦 Aplicando Secrets..."
& $KubectlCmd apply -f "secrets.yaml"
Write-Host "✅ Secrets aplicados`n"

# 4. Deploy SQL Server
Write-Host "📦 Aplicando SQL Server..."
& $KubectlCmd apply -f "sqlserver.yaml"
Write-Host "✅ SQL Server aplicado`n"

# 5. Deploy RabbitMQ
Write-Host "📦 Aplicando RabbitMQ..."
& $KubectlCmd apply -f "rabbitmq.yaml"
Write-Host "✅ RabbitMQ aplicado`n"

# 6. Aguardar SQL Server e RabbitMQ ficarem prontos
Write-Host "⏳ Aguardando SQL Server ficar pronto..."
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/sqlserver -n $Namespace

Write-Host "⏳ Aguardando RabbitMQ ficar pronto..."
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/rabbitmq -n $Namespace

# 7. Deploy dos microserviços
Write-Host "📦 Aplicando ContactCreate API..."
& $KubectlCmd apply -f "contact-create-api.yaml"
Write-Host "✅ ContactCreate API aplicada`n"

Write-Host "📦 Aplicando ContactRead API..."
& $KubectlCmd apply -f "contact-read-api.yaml"
Write-Host "✅ ContactRead API aplicada`n"

Write-Host "📦 Aplicando ContactUpdate API..."
& $KubectlCmd apply -f "contact-update-api.yaml"
Write-Host "✅ ContactUpdate API aplicada`n"

Write-Host "📦 Aplicando ContactDelete API..."
& $KubectlCmd apply -f "contact-delete-api.yaml"
Write-Host "✅ ContactDelete API aplicada`n"

# 8. Aguardar microserviços ficarem prontos
Write-Host "⏳ Aguardando microserviços ficarem prontos..."
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/contact-create-api -n $Namespace
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/contact-read-api -n $Namespace
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/contact-update-api -n $Namespace
& $KubectlCmd wait --for=condition=available --timeout=300s deployment/contact-delete-api -n $Namespace

# 9. Aplicar Ingress (opcional)
if (Test-Path "ingress.yaml") {
    Write-Host "🌐 Aplicando Ingress..."
    try {
        & $KubectlCmd apply -f "ingress.yaml"
        Write-Host "✅ Ingress aplicado`n"
    } catch {
        Write-Host "⚠️  Aviso: Erro ao aplicar Ingress (pode ser normal se o Ingress Controller não estiver instalado)`n"
    }
}

# 10. Mostrar status dos deployments, services e pods
Write-Host "📊 Status dos deployments:"
& $KubectlCmd get deployments -n $Namespace

Write-Host "`n📊 Status dos services:"
& $KubectlCmd get services -n $Namespace

Write-Host "`n📊 Status dos pods:"
& $KubectlCmd get pods -n $Namespace

Write-Host "`n🎉 Deploy concluído com sucesso!"
Write-Host "📋 Comandos úteis:"
Write-Host "   Ver logs: kubectl logs -f deployment/<deployment-name> -n $Namespace"
Write-Host "   Ver pods: kubectl get pods -n $Namespace"
Write-Host "   Deletar tudo: kubectl delete namespace $Namespace"

Write-Host "`n🌐 URLs de acesso (se Ingress estiver configurado):"
Write-Host "   API: https://api.contatos.com"
Write-Host "   RabbitMQ Management: https://rabbitmq.contatos.com"
