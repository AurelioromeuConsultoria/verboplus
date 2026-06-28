# Checklist operacional de entrega com qualidade

Use este checklist em toda funcionalidade nova, mudança relevante ou estabilização sensível no AppIgreja.

## Como usar

- mudança pequena: preencher a seção mínima
- fluxo sensível: preencher seção mínima + seção crítica
- entrega transversal: revisar também observabilidade, auditoria e permissão

## 1. Entendimento da entrega

- [ ] O objetivo da funcionalidade está claro em linguagem de negócio.
- [ ] O fluxo principal e os cenários de erro foram identificados.
- [ ] Está claro quem usa essa funcionalidade.
- [ ] Está claro quem não deve conseguir usar essa funcionalidade.

## 2. Engenharia e modelagem

- [ ] A solução respeita responsabilidade única de forma pragmática.
- [ ] Regra de negócio não ficou espalhada em controller ou componente de UI sem necessidade.
- [ ] Dependências relevantes foram abstraídas de forma simples e clara.
- [ ] Não houve duplicação desnecessária de lógica.
- [ ] Nomes de classes, funções, campos e componentes comunicam intenção real.
- [ ] O código ficou legível para manutenção futura.

## 3. Backend

- [ ] O endpoint ou serviço trata sucesso, bloqueio e erro de forma coerente.
- [ ] Permissão e escopo da ação foram revisados.
- [ ] Contratos de entrada e saída estão claros e consistentes.
- [ ] Falhas retornam mensagem útil sem expor detalhe indevido.
- [ ] A ação sensível gera log e, quando aplicável, auditoria.

## 4. Frontend, portal ou app

- [ ] Existe estado de loading.
- [ ] Existe estado de erro.
- [ ] Existe estado vazio, quando fizer sentido.
- [ ] Mensagens e labels usam linguagem de negócio.
- [ ] Não há resíduo visível de depuração, texto técnico ou comportamento improvisado.
- [ ] A tela orienta a tarefa do usuário com clareza.

## 5. Testes e regressão

- [ ] Regra principal está coberta por teste automatizado ou checklist de regressão.
- [ ] Foram considerados cenários de negação, conflito ou exceção.
- [ ] Existe forma objetiva de revalidar a funcionalidade depois.

## 6. Observabilidade

- [ ] Logs permitem entender o que aconteceu sem depender só do relato do usuário.
- [ ] Erros críticos são distinguíveis de warnings e rejeições de negócio.
- [ ] Integrações externas têm diagnóstico mínimo viável.

## 7. Auditoria e segurança

- [ ] Ações administrativas ou sensíveis deixam trilha.
- [ ] O backend não depende do frontend para impor segurança.
- [ ] O escopo por pessoa, equipe ou contexto foi validado.
- [ ] Não há rota sensível aberta sem intenção explícita.

## 8. Homologação

- [ ] Existe um roteiro curto para validar o fluxo principal.
- [ ] Existe uma forma clara de confirmar que a entrega ficou pronta.
- [ ] O impacto em módulos vizinhos foi considerado.

## Checklist reforçado para fluxos críticos

Aplicar obrigatoriamente em:
- Voluntariado
- Kids / AppKids
- autenticação e permissões
- mensagens, campanhas e jobs
- financeiro e patrimônio
- cadastro público

- [ ] O fluxo tem dono de negócio claro.
- [ ] O fluxo tem risco operacional identificado.
- [ ] Existe trilha de quem executou a ação.
- [ ] Existe revisão de permissão contextual.
- [ ] Existe validação de regressão do caminho feliz.
- [ ] Existe validação de regressão do caminho de bloqueio.
- [ ] Existe diagnóstico mínimo para ambiente e integração.

## Encerramento da entrega

Uma funcionalidade só deve ser considerada pronta quando:
- funciona
- pode ser entendida
- pode ser observada
- pode ser auditada, quando necessário
- pode ser mantida sem medo excessivo de regressão
