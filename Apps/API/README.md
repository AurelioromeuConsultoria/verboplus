# Sistema para Igrejas - Fase 1: Visitantes

## Visão Geral

Este é um sistema moderno e extensível desenvolvido especificamente para igrejas, focado inicialmente no cadastramento de visitantes e agendamento automático de mensagens de WhatsApp. O sistema foi construído seguindo os princípios da Clean Architecture, garantindo escalabilidade e manutenibilidade.

## Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core** - API REST
- **Entity Framework Core** - ORM para acesso a dados
- **SQLite** - Banco de dados (facilmente substituível por SQL Server)
- **Blazor** - Interface web (preparado para implementação)
- **IHostedService** - Serviço de background para agendamento

## Arquitetura

O projeto segue a **Clean Architecture** com as seguintes camadas:

### 1. Domain (SistemaIgreja.Domain)
- **Entidades**: Visitante, ConfiguracaoMensagem, MensagemAgendada
- **Enums**: StatusMensagem
- Contém as regras de negócio fundamentais

### 2. Application (SistemaIgreja.Application)
- **DTOs**: Objetos de transferência de dados
- **Interfaces**: Contratos para repositórios
- **Services**: Lógica de aplicação e casos de uso

### 3. Infrastructure (SistemaIgreja.Infrastructure)
- **Repositories**: Implementação dos repositórios
- **DbContext**: Configuração do Entity Framework
- **Services**: MessageSchedulerService (agendamento)

### 4. API (SistemaIgreja.API)
- **Controllers**: Endpoints REST
- **Program.cs**: Configuração da aplicação

### 5. Blazor (SistemaIgreja.Blazor)
- Interface web (preparada para implementação futura)

## Funcionalidades Implementadas

### ✅ Cadastro de Visitantes
- Nome, telefone (WhatsApp), data da visita
- Campos opcionais: e-mail, observações
- API REST completa (CRUD)

### ✅ Configuração de Mensagens
- Cadastro de templates de mensagens
- Configuração de dias após visita para envio
- Horário desejado de envio
- Status ativo/inativo

### ✅ Agendamento Automático
- Criação automática de mensagens ao cadastrar visitante
- Verificação periódica (a cada 5 minutos)
- Logs detalhados de processamento
- Simulação de envio (preparado para integração real)

### ✅ API REST Completa
- Endpoints para todas as entidades
- Documentação Swagger automática
- CORS configurado para frontend

## Estrutura do Banco de Dados

### Tabela: Visitantes
- Id, Nome, Telefone, DataVisita, Email, Observacoes, DataCadastro

### Tabela: ConfiguracoesMensagens
- Id, Nome, TextoMensagem, DiasAposVisita, HorarioEnvio, Ativo, DataCriacao

### Tabela: MensagensAgendadas
- Id, VisitanteId, ConfiguracaoMensagemId, DataAgendamento, DataEnvio, Status, TextoFinal, DataProcessamento, LogErro, DataCriacao

## Como Executar

### Pré-requisitos
- .NET 8 SDK
- Git

### Passos para Execução

1. **Clone o repositório**
```bash
git clone <url-do-repositorio>
cd SistemaIgreja
```

2. **Restaurar dependências**
```bash
dotnet restore
```

3. **Executar migrations (se necessário)**
```bash
dotnet ef database update --project src/SistemaIgreja.Infrastructure --startup-project src/SistemaIgreja.API
```

4. **Executar a aplicação**
```bash
cd src/SistemaIgreja.API
dotnet run
```

5. **Acessar a API**
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Endpoints da API

### Visitantes
- `GET /api/visitantes` - Listar todos os visitantes
- `GET /api/visitantes/{id}` - Obter visitante por ID
- `POST /api/visitantes` - Criar novo visitante
- `PUT /api/visitantes/{id}` - Atualizar visitante
- `DELETE /api/visitantes/{id}` - Excluir visitante

### Configurações de Mensagens
- `GET /api/configuracoesMensagens` - Listar todas as configurações
- `GET /api/configuracoesMensagens/ativas` - Listar apenas configurações ativas
- `GET /api/configuracoesMensagens/{id}` - Obter configuração por ID
- `POST /api/configuracoesMensagens` - Criar nova configuração
- `PUT /api/configuracoesMensagens/{id}` - Atualizar configuração
- `DELETE /api/configuracoesMensagens/{id}` - Excluir configuração

### Mensagens Agendadas
- `GET /api/mensagensAgendadas` - Listar todas as mensagens
- `GET /api/mensagensAgendadas/{id}` - Obter mensagem por ID
- `GET /api/mensagensAgendadas/prontas-para-envio` - Mensagens prontas para envio
- `GET /api/mensagensAgendadas/visitante/{visitanteId}` - Mensagens de um visitante
- `POST /api/mensagensAgendadas/{id}/marcar-pronta` - Marcar como pronta
- `POST /api/mensagensAgendadas/{id}/marcar-enviada` - Marcar como enviada
- `POST /api/mensagensAgendadas/{id}/marcar-erro` - Marcar como erro

## Agendamento de Mensagens

O sistema possui um serviço de background (`MessageSchedulerService`) que:

1. **Verifica a cada 5 minutos** se há mensagens para enviar
2. **Processa mensagens** que chegaram no horário de envio
3. **Simula o envio** (logs detalhados)
4. **Atualiza o status** das mensagens
5. **Registra erros** quando necessário

### Fluxo de Agendamento

1. Visitante é cadastrado
2. Sistema busca configurações ativas
3. Cria mensagens agendadas baseadas nas configurações
4. Serviço de background processa mensagens no horário
5. Mensagens são marcadas como enviadas ou erro

## Configurações Padrão

O sistema vem com duas configurações de mensagem pré-cadastradas:

1. **Boas-vindas** - 1 dia após visita às 10:00
2. **Convite para retorno** - 7 dias após visita às 18:00

## Logs e Monitoramento

- Logs detalhados do agendamento
- Status de cada mensagem
- Registro de erros
- Timestamps de processamento

## Próximas Fases (Roadmap)

### Fase 2: Interface Blazor
- Páginas para cadastro de visitantes
- Configuração de mensagens
- Dashboard de agendamentos

### Fase 3: Integração WhatsApp
- Integração com Z-API, Twilio ou similar
- Envio real de mensagens
- Confirmação de entrega

### Fase 4: Expansão
- Módulo de membros
- Células e discipulados
- Eventos e campanhas
- Dashboard administrativo

## Estrutura de Arquivos

```
SistemaIgreja/
├── src/
│   ├── SistemaIgreja.Domain/
│   │   └── Entities/
│   ├── SistemaIgreja.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Services/
│   ├── SistemaIgreja.Infrastructure/
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Services/
│   ├── SistemaIgreja.API/
│   │   └── Controllers/
│   └── SistemaIgreja.Blazor/
├── tests/
└── README.md
```

## Contribuição

Este projeto foi desenvolvido seguindo as melhores práticas de desenvolvimento .NET e está preparado para expansão futura. A arquitetura limpa facilita a manutenção e adição de novas funcionalidades.

## Licença

Este projeto é proprietário e destinado ao uso específico da organização religiosa solicitante.

