---
name: frontend-admin-feature
description: Adicionar uma tela/módulo ao painel admin React consumindo a API existente (módulo api/, página shadcn, formulário react-hook-form+Zod, rota ProtectedRoute/RequirePermission, i18n pt-BR/en-US/es-ES, teste Vitest). Use para features de UI no admin. NÃO use para backend, app Flutter ou landing.
---

# Frontend Admin Feature

**Agente:** frontend-web-engineer.
**Fonte:** PROJECT_CONTEXT.md §5 + §13 passos 10–11.

## Objetivo
Adicionar uma tela ao painel admin consistente com o design, com RBAC, i18n completo e teste — sem libs novas.

## Pré-requisitos
- Ler PROJECT_CONTEXT §5, DOMAIN_KNOWLEDGE.
- Endpoint já existente no backend (se não existir/divergir → sinalizar ao backend, não inventar contrato).

## Entradas esperadas
Domínio/entidade, endpoints disponíveis, campos e validações do formulário, permissão/recurso exigido, textos pt-BR/en-US/es-ES.

## Processo
1. **Módulo de API** em `api/{dominio}.js` usando `lib/apiClient.js` (axios com interceptors JWT/tenant); normalizar DTOs aceitando PascalCase e camelCase. Não reimplementar refresh/redirect 402.
2. **Página/componentes** em PascalCase com sufixos `List`/`Form`/`Details`/`Dialog`; shadcn/ui ("new-york") + Tailwind; ícones lucide-react; toasts sonner; gráficos recharts.
3. **Formulário** com react-hook-form + Zod (`@hookform/resolvers`); espelhar `src/lib/passwordPolicy.js` em telas de senha (backend é a fonte da verdade — nunca divergir).
4. **Rotas** lazy-loaded com `React.lazy`; proteger com `<ProtectedRoute>` e `<RequirePermission>` conforme RBAC do backend.
5. **Erros da API**: consumir corpo `{ message }`; status 402 → `/billing` (já nos interceptors).
6. **i18n**: i18next/react-i18next; toda string visível com as **3 traduções** (pt-BR/en-US/es-ES).
7. **Testes**: Vitest + React Testing Library + jsdom; cada bug vira teste de regressão.

## Validações
- Strings têm as 3 traduções; rota protegida com permissão correta.
- Consumo do contrato real (sem inventar endpoint).
- Admin usa **pnpm**; `pnpm run test` verde (CI do admin BLOQUEIA deploy se falhar).

## Resultado esperado
Módulo `api/`, página(s)/componentes, formulário RHF+Zod, rota protegida, chaves i18n×3, teste(s) Vitest.

## Critérios de conclusão
`pnpm run test` verde; tela navegável e protegida; i18n completo; nenhuma lib nova.

## Quando NÃO usar
Backend/contrato de API (→ backend); app Flutter AppKids (→ mobile); pipelines/infra (→ devops); landing VerboPlus (marketing, não consome API).

## Exemplos
- "Tela de listagem + cadastro de Patrimônio no admin."
- "Dashboard de doações com gráfico recharts."
