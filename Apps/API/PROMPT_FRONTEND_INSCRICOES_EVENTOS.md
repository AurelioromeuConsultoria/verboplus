# Prompt para Frontend - Sistema de Inscrições em Eventos

## Contexto
Você está construindo o frontend de uma API REST para gestão de uma igreja. Agora precisa implementar a funcionalidade completa de **Inscrições em Eventos**, onde pessoas podem se inscrever em eventos e administradores podem gerenciar essas inscrições.

## Estrutura da API

### Base URL
```
http://localhost:5000/api/InscricoesEventos
```

### Endpoints Disponíveis

#### 1. Listar todas as inscrições (Admin)
```
GET /api/InscricoesEventos
```
**Resposta:** Array de `InscricaoEventoDto`
**Uso:** Lista completa de todas as inscrições (ordenadas por data, mais recentes primeiro)

#### 2. Buscar inscrição por ID
```
GET /api/InscricoesEventos/{id}
```
**Resposta:** `InscricaoEventoDto`
**Uso:** Detalhes de uma inscrição específica

#### 3. Buscar inscrições por evento
```
GET /api/InscricoesEventos/evento/{eventoId}
```
**Resposta:** Array de `InscricaoEventoDto`
**Uso:** Listar todas as inscrições de um evento específico

#### 4. Buscar inscrições por status
```
GET /api/InscricoesEventos/status/{status}
```
**Parâmetros:** status (1=Pendente, 2=Confirmada, 3=Cancelada, 4=Presente)
**Resposta:** Array de `InscricaoEventoDto`
**Uso:** Filtrar inscrições por status

#### 5. Obter estatísticas de um evento
```
GET /api/InscricoesEventos/evento/{eventoId}/estatisticas
```
**Resposta:** `EstatisticasInscricaoDto`
**Uso:** Dashboard com números do evento

#### 6. Criar nova inscrição (Público)
```
POST /api/InscricoesEventos
```
**Body:** `CriarInscricaoEventoDto`
**Uso:** Formulário público de inscrição

#### 7. Atualizar inscrição (Admin)
```
PUT /api/InscricoesEventos/{id}
```
**Body:** `AtualizarInscricaoEventoDto`
**Uso:** Editar dados da inscrição

#### 8. Confirmar inscrição (Admin)
```
PUT /api/InscricoesEventos/{id}/confirmar
```
**Uso:** Botão de confirmação rápida

#### 9. Cancelar inscrição (Admin)
```
PUT /api/InscricoesEventos/{id}/cancelar
```
**Uso:** Botão de cancelamento rápido

#### 10. Remover inscrição (Admin)
```
DELETE /api/InscricoesEventos/{id}
```
**Uso:** Excluir inscrição permanentemente

## Estrutura de Dados

### CriarInscricaoEventoDto (POST)
```typescript
interface CriarInscricaoEventoDto {
  eventoId: number;                    // Obrigatório - ID do evento
  nome: string;                         // Obrigatório - Nome do participante
  whatsApp: string;                     // Obrigatório - WhatsApp (formato: "11999999999")
  email?: string;                       // Opcional - Email do participante
  quantidadeAcompanhantes: number;       // Opcional - Quantidade de pessoas que vão junto (padrão: 0)
  observacoes?: string;                 // Opcional - Observações do participante
}
```

### InscricaoEventoDto (Resposta)
```typescript
interface InscricaoEventoDto {
  id: number;
  eventoId: number;
  eventoTitulo?: string;                // Nome do evento (vem do relacionamento)
  nome: string;
  whatsApp: string;
  email?: string;
  status: StatusInscricao;              // 1=Pendente, 2=Confirmada, 3=Cancelada, 4=Presente
  statusDescricao: string;             // "Pendente", "Confirmada", "Cancelada", "Presente"
  quantidadeAcompanhantes: number;
  observacoes?: string;                 // Observações do participante
  observacoesInternas?: string;         // Observações internas (só admin vê)
  dataInscricao: string;                // ISO DateTime
  dataConfirmacao?: string;             // ISO DateTime (null se não confirmada)
  dataCancelamento?: string;           // ISO DateTime (null se não cancelada)
}

enum StatusInscricao {
  Pendente = 1,
  Confirmada = 2,
  Cancelada = 3,
  Presente = 4
}
```

### EstatisticasInscricaoDto
```typescript
interface EstatisticasInscricaoDto {
  eventoId: number;
  eventoTitulo: string;
  totalInscricoes: number;
  inscricoesConfirmadas: number;
  inscricoesPendentes: number;
  inscricoesCanceladas: number;
  totalParticipantes: number;          // Inclui acompanhantes
}
```

### AtualizarInscricaoEventoDto (PUT)
```typescript
interface AtualizarInscricaoEventoDto {
  status: StatusInscricao;
  quantidadeAcompanhantes: number;
  observacoes?: string;
  observacoesInternas?: string;         // Só admin pode editar
}
```

## Validações da API

A API retorna erros nas seguintes situações:

1. **Evento não encontrado:** `"Evento não encontrado"`
2. **Evento já iniciou:** `"Não é possível se inscrever em um evento que já iniciou"`
3. **Inscrição duplicada:** `"Já existe uma inscrição para este WhatsApp neste evento"`
4. **Inscrição não encontrada:** Retorna 404

## Funcionalidades a Implementar

### 1. Formulário Público de Inscrição
**Localização:** Página de detalhes do evento ou modal

**Campos:**
- Nome (obrigatório, texto)
- WhatsApp (obrigatório, máscara de telefone)
- Email (opcional, validação de email)
- Quantidade de Acompanhantes (número, mínimo 0)
- Observações (opcional, textarea)

**Comportamento:**
- Ao submeter, fazer POST para `/api/InscricoesEventos`
- Mostrar mensagem de sucesso: "Inscrição realizada com sucesso! Aguarde a confirmação."
- Em caso de erro, mostrar mensagem específica:
  - Se evento já iniciou: "Este evento já iniciou e não aceita mais inscrições"
  - Se já existe inscrição: "Você já está inscrito neste evento"
  - Outros erros: Mostrar mensagem genérica

### 2. Painel Administrativo - Listagem de Inscrições

**Filtros:**
- Por Evento (dropdown com lista de eventos)
- Por Status (Pendente, Confirmada, Cancelada, Presente)
- Busca por nome ou WhatsApp

**Tabela de Inscrições:**
Colunas:
- Nome
- WhatsApp
- Email
- Evento (nome do evento)
- Status (badge colorido)
- Quantidade de Acompanhantes
- Data de Inscrição
- Ações (Editar, Confirmar, Cancelar, Excluir)

**Status com cores:**
- Pendente: Amarelo/Laranja
- Confirmada: Verde
- Cancelada: Vermelho
- Presente: Azul

### 3. Painel Administrativo - Detalhes da Inscrição

**Informações exibidas:**
- Dados do participante (Nome, WhatsApp, Email)
- Evento vinculado
- Status atual
- Quantidade de acompanhantes
- Observações do participante
- Observações internas (campo editável)
- Datas (Inscrição, Confirmação, Cancelamento)

**Ações disponíveis:**
- Editar (abrir modal/formulário)
- Confirmar (botão rápido)
- Cancelar (botão rápido)
- Excluir (com confirmação)

### 4. Dashboard de Estatísticas do Evento

**Localização:** Página de detalhes do evento (seção admin)

**Cards/Métricas:**
- Total de Inscrições
- Inscrições Confirmadas
- Inscrições Pendentes
- Inscrições Canceladas
- Total de Participantes (incluindo acompanhantes)

**Gráfico sugerido:**
- Gráfico de pizza ou barras mostrando distribuição por status

### 5. Lista de Inscrições por Evento

**Localização:** Página de detalhes do evento

**Funcionalidades:**
- Lista todas as inscrições do evento
- Filtro por status
- Exportar lista (opcional)
- Marcar presença (mudar status para "Presente")

## Fluxos de Uso

### Fluxo 1: Inscrição Pública
1. Usuário acessa página do evento
2. Clica em "Inscrever-se"
3. Preenche formulário
4. Submete
5. Recebe confirmação de sucesso
6. Status inicial: "Pendente"

### Fluxo 2: Gestão Administrativa
1. Admin acessa painel de inscrições
2. Visualiza lista com filtros
3. Pode:
   - Ver detalhes
   - Confirmar inscrição (muda status para "Confirmada")
   - Cancelar inscrição (muda status para "Cancelada")
   - Editar observações internas
   - Excluir inscrição

### Fluxo 3: Ver Estatísticas
1. Admin acessa página do evento
2. Visualiza seção de estatísticas
3. Vê números atualizados em tempo real

## Observações Importantes

1. **Status Inicial:** Todas as inscrições criadas começam com status "Pendente"
2. **Validação de Duplicatas:** A API impede que o mesmo WhatsApp se inscreva duas vezes no mesmo evento
3. **Datas Automáticas:** 
   - `dataInscricao` é preenchida automaticamente
   - `dataConfirmacao` é preenchida quando status muda para "Confirmada"
   - `dataCancelamento` é preenchida quando status muda para "Cancelada"
4. **Relacionamento com Evento:** Sempre que possível, mostrar o nome do evento junto com a inscrição
5. **Observações Internas:** Campo visível e editável apenas para administradores

## Exemplo de Requisição POST (Criar Inscrição)

```json
{
  "eventoId": 1,
  "nome": "João Silva",
  "whatsApp": "11999999999",
  "email": "joao@email.com",
  "quantidadeAcompanhantes": 2,
  "observacoes": "Vou levar minha esposa e filho"
}
```

## Exemplo de Resposta (InscricaoEventoDto)

```json
{
  "id": 1,
  "eventoId": 1,
  "eventoTitulo": "Retiro de Fim de Ano",
  "nome": "João Silva",
  "whatsApp": "11999999999",
  "email": "joao@email.com",
  "status": 1,
  "statusDescricao": "Pendente",
  "quantidadeAcompanhantes": 2,
  "observacoes": "Vou levar minha esposa e filho",
  "observacoesInternas": null,
  "dataInscricao": "2025-11-27T19:59:41",
  "dataConfirmacao": null,
  "dataCancelamento": null
}
```

## Exemplo de Resposta (Estatísticas)

```json
{
  "eventoId": 1,
  "eventoTitulo": "Retiro de Fim de Ano",
  "totalInscricoes": 50,
  "inscricoesConfirmadas": 35,
  "inscricoesPendentes": 10,
  "inscricoesCanceladas": 5,
  "totalParticipantes": 87
}
```

## Sugestões de UI/UX

1. **Formulário de Inscrição:**
   - Design limpo e intuitivo
   - Validação em tempo real
   - Máscara para WhatsApp
   - Campo de acompanhantes com botões +/- ou input numérico

2. **Lista de Inscrições:**
   - Tabela responsiva
   - Paginação se houver muitas inscrições
   - Ações rápidas (confirmar/cancelar) sem abrir modal
   - Badges coloridos para status

3. **Dashboard:**
   - Cards com números grandes e destacados
   - Gráficos visuais
   - Atualização em tempo real ou com botão de refresh

4. **Feedback ao Usuário:**
   - Mensagens de sucesso claras
   - Mensagens de erro específicas
   - Loading states durante requisições
   - Confirmações para ações destrutivas (excluir, cancelar)

## Integração com Eventos

**Importante:** Você também terá acesso ao endpoint de Eventos:
```
GET /api/Eventos
GET /api/Eventos/{id}
```

Use esses endpoints para:
- Popular dropdown de seleção de eventos
- Mostrar informações do evento na página de inscrição
- Validar se o evento ainda aceita inscrições (verificar DataInicio)

## Tratamento de Erros

Sempre trate os seguintes cenários:
- Erro de rede (API offline)
- Erro 400 (Bad Request) - mostrar mensagem específica
- Erro 404 (Not Found) - mostrar "Inscrição não encontrada"
- Erro 500 (Server Error) - mostrar mensagem genérica de erro

