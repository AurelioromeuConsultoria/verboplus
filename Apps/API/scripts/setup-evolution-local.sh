#!/bin/bash

# Script para configurar Evolution API localmente
# Uso: ./scripts/setup-evolution-local.sh

set -e

echo "🚀 Configurando Evolution API Localmente..."
echo ""

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker Desktop."
    exit 1
fi

echo "✅ Docker está rodando"
echo ""

# Verificar se docker-compose existe
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose não encontrado. Por favor, instale o Docker Compose."
    exit 1
fi

echo "✅ Docker Compose encontrado"
echo ""

# Verificar se .env.evolution existe
if [ ! -f ".env.evolution" ]; then
    echo "📝 Arquivo .env.evolution não encontrado. Criando a partir do exemplo..."
    
    if [ ! -f "env.evolution.example" ]; then
        echo "❌ Arquivo env.evolution.example não encontrado!"
        exit 1
    fi
    
    cp env.evolution.example .env.evolution
    
    # Gerar API Key aleatória
    if command -v openssl &> /dev/null; then
        API_KEY=$(openssl rand -hex 32)
        # Substituir no arquivo (macOS e Linux)
        if [[ "$OSTYPE" == "darwin"* ]]; then
            sed -i '' "s/SUA_CHAVE_API_SEGURA_AQUI/$API_KEY/" .env.evolution
        else
            sed -i "s/SUA_CHAVE_API_SEGURA_AQUI/$API_KEY/" .env.evolution
        fi
        echo "✅ API Key gerada automaticamente"
    else
        echo "⚠️  openssl não encontrado. Por favor, edite .env.evolution e defina AUTHENTICATION_API_KEY manualmente."
    fi
    
    echo "✅ Arquivo .env.evolution criado"
    echo "⚠️  IMPORTANTE: Revise e ajuste as configurações em .env.evolution antes de continuar"
    echo ""
    read -p "Pressione Enter para continuar após revisar o arquivo..."
fi

# Verificar se docker-compose.evolution.yml existe
if [ ! -f "docker-compose.evolution.yml" ]; then
    echo "❌ Arquivo docker-compose.evolution.yml não encontrado!"
    exit 1
fi

echo "✅ Arquivo docker-compose.evolution.yml encontrado"
echo ""

# Parar containers existentes (se houver)
echo "🛑 Parando containers existentes (se houver)..."
docker-compose -f docker-compose.evolution.yml down 2>/dev/null || true

# Iniciar containers
echo "🚀 Iniciando containers..."
docker-compose -f docker-compose.evolution.yml up -d

echo ""
echo "⏳ Aguardando serviços iniciarem..."
sleep 10

# Verificar status
echo ""
echo "📊 Status dos serviços:"
docker-compose -f docker-compose.evolution.yml ps

echo ""
echo "✅ Evolution API configurada!"
echo ""
echo "📝 Próximos passos:"
echo "1. Acesse http://localhost:8080 para verificar se está funcionando"
echo "2. Veja os logs: docker-compose -f docker-compose.evolution.yml logs -f evolution-api"
echo "3. Crie uma instância WhatsApp seguindo o guia EVOLUTION_API_LOCAL.md"
echo ""
echo "🔑 Sua API Key está no arquivo .env.evolution (variável AUTHENTICATION_API_KEY)"
echo ""

