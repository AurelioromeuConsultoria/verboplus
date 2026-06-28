---
name: handle-data-subject-request
description: Atender uma solicitação de titular LGPD (acesso/exportação/correção/eliminação/revogação) dentro do SLA de 15 dias, com eliminação = anonimização (nunca exclusão física), consentimento append-only e trilha de auditoria. Use ao processar direitos de titular.
---

# Handle Data Subject Request (LGPD)

**Agente:** plataforma-seguranca-lgpd.
**Fonte:** DOMAIN_KNOWLEDGE.md §8 (LGPD), PROJECT_CONTEXT §10/§12.

## Objetivo
Atender um direito do titular (LGPD Art. 18/19) dentro do prazo legal, com anonimização e auditabilidade.

## Pré-requisitos
- Ler DOMAIN_KNOWLEDGE (LGPD).
- Entidades `SolicitacaoTitular`, `ConsentimentoRegistro` e `AuditSaveChangesInterceptor` existentes.

## Entradas esperadas
Tipo (`Acesso/Exportacao/Correcao/Eliminacao/Revogacao/Outro`), titular (Pessoa) e Tenant, data da solicitação.

## Processo
1. **Registrar** → `Status = Aberta`, `PrazoLimite = SolicitadoEm + 15 dias`.
2. **Atender** → `EmAtendimento`.
3. **Executar por tipo**:
   - **Eliminação = ANONIMIZAÇÃO** (nunca exclusão física): substituir PII por marcadores, preservando integridade referencial/auditoria.
   - **Acesso/Exportação**: gerar pacote de dados do titular.
   - **Correção**: aplicar e versionar.
   - **Revogação de consentimento**: gravar `RevogadoEm` no `ConsentimentoRegistro` (append-only — não apaga histórico).
4. **Concluir** → `Concluida` (ou `Recusada`) com observação/resultado.
5. **Auditoria**: garantir que as mudanças passam pelo `AuditSaveChangesInterceptor` (AuditLog); zero PII em logs/Sentry.
6. **Papéis**: Igreja = Controladora, VerboPlus = Operadora — agir como operadora.

## Validações
- Eliminação nunca apaga fisicamente (só anonimiza).
- Consentimento append-only (`RevogadoEm`).
- Conclusão dentro de `PrazoLimite`; AuditLog registrou as operações; sem PII.

## Resultado esperado
`SolicitacaoTitular` em `Concluida`/`Recusada`; dados anonimizados/exportados/corrigidos conforme o tipo; entradas em AuditLog.

## Critérios de conclusão
Solicitação fechada dentro do prazo; ação coerente com o tipo; trilha de auditoria presente; nenhuma PII exposta.

## Quando NÃO usar
CRUD comum (→ `backend-feature-crud`); mudar o motor de consentimento/middleware sem alinhar; exclusão física (proibido — é sempre anonimização).

## Exemplos
- "Atender pedido de eliminação de um membro (anonimizar mantendo integridade)."
- "Revogar consentimento parental versionado."
