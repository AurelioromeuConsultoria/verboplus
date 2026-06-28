#!/bin/bash

# Script para commitar apenas os arquivos necessários da migração PostgreSQL
# Exclui: .DS_Store, arquivos .backup, e appsettings.json (com senhas)

cd "$(dirname "$0")"

echo "📋 Verificando arquivos a serem commitados..."
echo ""

# Adicionar apenas arquivos modificados e novos (excluindo os que estão no .gitignore)
git add -A

# Remover arquivos que não devem ser commitados (se ainda estiverem staged)
git reset HEAD -- .DS_Store src/.DS_Store src/SistemaIgreja.Infrastructure/.DS_Store 2>/dev/null || true
git reset HEAD -- "*.backup" 2>/dev/null || true
git reset HEAD -- "src/SistemaIgreja.API/appsettings.json" 2>/dev/null || true

echo "✅ Arquivos preparados para commit:"
echo ""
git status --short | grep -v "\.DS_Store\|\.backup\|appsettings\.json$"
echo ""

# Contar arquivos staged
STAGED_COUNT=$(git diff --cached --name-only | wc -l | tr -d ' ')

if [ "$STAGED_COUNT" -eq 0 ]; then
    echo "⚠️  Nenhum arquivo para commitar."
    exit 0
fi

echo "📦 Total de arquivos a serem commitados: $STAGED_COUNT"
echo ""
read -p "Deseja continuar com o commit? (s/n) " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Ss]$ ]]; then
    echo "💾 Fazendo commit..."
    git commit -m "feat: Migração para PostgreSQL

- Configuração dinâmica de provider (SQL Server/PostgreSQL)
- Migration baseline PostgreSQL criada
- Suporte a timestamp sem timezone (legacy behavior)
- Adicionadas colunas faltantes (UltimoAcesso, DataConfirmacao, DataCancelamento, etc.)
- Correção de foreign keys em HubCasas
- Arquivos de exemplo de configuração adicionados"
    
    echo ""
    echo "✅ Commit realizado com sucesso!"
    echo ""
    echo "📝 Próximos passos:"
    echo "   1. Revise o commit: git log -1"
    echo "   2. Faça push: git push"
else
    echo "❌ Commit cancelado."
    echo ""
    echo "💡 Para fazer commit manualmente:"
    echo "   git add <arquivos>"
    echo "   git commit -m 'sua mensagem'"
fi
