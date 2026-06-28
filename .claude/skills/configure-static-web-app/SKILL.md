---
name: configure-static-web-app
description: Configurar/deployar um frontend como Azure Static Web App via pipeline (admin com pnpm + testes que bloqueiam deploy; Portal com npm + staticwebapp.config.json). Use para deploy dos frontends web. NÃO use para o backend (roda em Coolify, use provision-integration-secret).
---

# Configure Static Web App (Azure)

**Agente:** devops-infra-engineer.
**Fonte:** PROJECT_CONTEXT §9, ARCHITECTURE (Deploy).

## Objetivo
Publicar/ajustar um frontend web como Azure Static Web App pelo pipeline correto, sem divergência entre ambientes.

## Pré-requisitos
- Ler PROJECT_CONTEXT §9.
- Saber qual frontend (admin / Portal / VerboPlus / CadastroMembro) e seu gerenciador de pacotes.

## Entradas esperadas
Frontend-alvo, variáveis do Static Web App, rotas/config de fallback (`staticwebapp.config.json`).

## Processo
1. **Admin**: `azure-pipelines.yml` — Node 20, **pnpm**; `pnpm run test` **BLOQUEIA deploy se falhar** → `pnpm run build` → Azure Static Web App (grupo `SWA_AppIgreja_Admin_Variables`).
2. **Portal**: Azure DevOps — **npm**, Node 18, `npm run build`, `staticwebapp.config.json`, `AzureStaticWebApp@0`; triggers `main`/`master`.
3. **VerboPlus / CadastroMembro**: build estático (Vite/HTML), hosting estático.
4. **Vars/segredos**: configurar no grupo de variáveis do pipeline; nada de secret no repositório.
5. **Fallback/rotas**: ajustar `staticwebapp.config.json` (SPA rewrite) quando aplicável.
6. **Pós-deploy**: validar carregamento e que o frontend aponta para a API correta.

## Validações
- Gerenciador certo (admin = pnpm; Portal = npm).
- Suíte de testes do admin verde (senão o deploy é bloqueado).
- Nenhum secret no repositório; config de rotas correta.

## Resultado esperado
Pipeline configurado e frontend publicado no Azure SWA, apontando para a API correta.

## Critérios de conclusão
Deploy concluído; app carrega; rotas SPA funcionam; testes do admin verdes.

## Quando NÃO usar
Deploy do backend API/Worker (Coolify — fora do escopo); provisionar segredo de integração de backend (→ `provision-integration-secret`).

## Exemplos
- "Deploy da landing VerboPlus."
- "Ajustar o staticwebapp.config.json do Portal para rotas SPA."
