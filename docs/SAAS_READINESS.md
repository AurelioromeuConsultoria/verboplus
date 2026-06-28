# Verbo+ — Roadmap de prontidão para venda (SaaS)

> Análise atualizada em 2026-06-28. Marcar `[x]` conforme concluir. Itens 🔴 são bloqueadores de lançamento.

## ✅ Já concluído
- [x] LGPD (export / anonimização / consentimento / solicitações do titular)
- [x] Billing de assinatura (Asaas: trial → inadimplência → suspensão; gating HTTP 402)
- [x] Onboarding self-service (signup + verificação de e-mail)
- [x] Segredos rotacionados e `appsettings` sanitizados (env vars no Coolify)
- [x] Observabilidade: Sentry (API + Worker + frontend) + health checks
- [x] Lockout de login + política de senha + rate limit no `/signup`
- [x] Testes backend (~209 arquivos, inclui isolamento de tenant) + frontend (71)
- [x] Branding Verbo+ (logo, tema Violeta Profundo, Poppins) + dashboard acionável
- [x] Infra migrada para GitHub + Coolify (domínio verboplus.com.br); landing no ar; auto-deploy via GitHub Actions (2026-06-28)

---

## 🔴 Bloqueadores (resolver ANTES de vender)

- [~] **1. Vazamento de dados entre igrejas na Comunicação — CRÍTICO. CÓDIGO PRONTO (falta aplicar migration + deploy).**
  Varredura de TODAS as entidades feita: as únicas sem isolamento eram a família `Comunicacao*` (Template/Campanha/CampanhaCanal/Entrega/Segmento/Automacao/Preferencia) e `NotificacaoUsuario`. (Globais corretas: Tenant, TenantDomain, Plano, EventoWebhookBilling, VerificacaoEmail.)
  Feito: (a) as 8 entidades agora implementam `ITenantEntity` + `TenantId` (liga o filtro de leitura); (b) **carimbo automático de TenantId no `SaveChanges` do DbContext** (rede de segurança no insert, API e Worker); (c) migration `AdicionarTenantIdComunicacaoNotificacoes` com **backfill** (herda tenant dos vínculos; sem vínculo → tenant 1); (d) teste de isolamento em `TenantQueryFilterTests`.
  **Falta:** aplicar a migration no banco de prod + commit/push + redeploy (API **e** Worker, porque o DbContext mudou).
- [ ] **2. Backup / restore / runbook / DR.** Não existe rotina de backup do banco, recuperação ou rollback documentado.
- [ ] **3. Uploads inseguros e que não escalam.** Disco local, servidos sem autenticação (URL previsível), não funcionam multi-instância. Migrar para Blob/S3 + URLs assinadas. Corrigir possível path traversal em galerias.

---

## 🟡 Importantes (logo após lançar / em paralelo)

- [ ] **Schedulers em dobro.** Jobs (mensagens, aniversário, escala) rodam na API E no Worker sem lock → risco de envio duplicado. Rodar em 1 processo só.
- [ ] **Login sem rate limit por IP** (lockout por conta existe; falta proteção distribuída).
- [ ] **Webhook Asaas só valida token** (sem assinatura HMAC).
- [ ] **Limites de plano não aplicados** (`MaxUsuarios`/`MaxMembros` existem mas não bloqueiam).
- [ ] **E-mail desligado por default** + SMTP não configurado em produção (verificação de signup não sai).
- [ ] **Swagger UI público em produção.**

---

## 🧩 Funcionalidades (diferenciais / roadmap — não bloqueiam, mas vendem)

- [ ] **Área do membro (autosserviço)** — atualizar dados, ver participações.
- [ ] **Visão 360° da pessoa** — histórico unificado (eventos + voluntariado + kids + comunicação).
- [ ] **Funil de conversão visitante → membro** (métrica de venda forte).
- [ ] **Omnichannel real** (e-mail / SMS / push maduros, além do WhatsApp).
- [ ] **AppKids para Android** (hoje só iOS).
- [ ] Dashboard #3/#6/#8 sugeridos (parabéns no WhatsApp, eventos c/ inscritos, conversão).

---

## 💼 Comercial / operacional (não-código, obrigatório p/ vender)

- [ ] Config de produção: conta Asaas Verbo+ + `Billing__Asaas__*` + webhook no painel.
- [ ] SMTP configurado (e `Email:Enabled=true`).
- [ ] CTA do site VerboPlus → `/signup`.
- [ ] Preços definitivos dos 3 planos (hoje placeholder).
- [ ] Suporte: canal de atendimento + base de ajuda/FAQ + e-mail de contato.
- [ ] Termos de Uso + Política de Privacidade publicados.
- [ ] Emissão de nota fiscal (fluxo via Asaas a confirmar).

---

## Ordem sugerida
1. Vazamento Comunicação (+ varredura de tenant) → 2. Backup/runbook → 3. Uploads seguros + schedulers em 1 processo → 4. Config de produção (Asaas/SMTP/CTA) + preços + termos → 5. Hardening (rate limit login, HMAC webhook, limites de plano) → 6. Diferenciais funcionais.
