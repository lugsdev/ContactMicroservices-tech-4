#!/bin/bash

# Script para build de todas as imagens Docker dos microserviços

set -e

echo "🐳 Iniciando build de todas as imagens Docker..."

# Definir variáveis
REGISTRY="contactmicroservices"
VERSION=${1:-"latest"}

echo "📦 Registry: $REGISTRY"
echo "🏷️  Versão: $VERSION"
echo ""

# Build ContactCreate.API
echo "🔨 Building ContactCreate.API..."
docker build -f docker/Dockerfile.ContactCreate -t $REGISTRY/contact-create-api:$VERSION .
echo "✅ ContactCreate.API build concluído"
echo ""

# Build ContactRead.API
echo "🔨 Building ContactRead.API..."
docker build -f docker/Dockerfile.ContactRead -t $REGISTRY/contact-read-api:$VERSION .
echo "✅ ContactRead.API build concluído"
echo ""

# Build ContactUpdate.API
echo "🔨 Building ContactUpdate.API..."
docker build -f docker/Dockerfile.ContactUpdate -t $REGISTRY/contact-update-api:$VERSION .
echo "✅ ContactUpdate.API build concluído"
echo ""

# Build ContactDelete.API
echo "🔨 Building ContactDelete.API..."
docker build -f docker/Dockerfile.ContactDelete -t $REGISTRY/contact-delete-api:$VERSION .
echo "✅ ContactDelete.API build concluído"
echo ""

echo "🎉 Todas as imagens foram construídas com sucesso!"
echo ""
echo "📋 Imagens criadas:"
echo "  - $REGISTRY/contact-create-api:$VERSION"
echo "  - $REGISTRY/contact-read-api:$VERSION"
echo "  - $REGISTRY/contact-update-api:$VERSION"
echo "  - $REGISTRY/contact-delete-api:$VERSION"
echo ""
echo "🚀 Para executar os serviços, use: docker-compose up -d"

