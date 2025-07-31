# Exemplos de Uso da API

## Cadastrar um Visitante

```bash
curl -X POST http://localhost:5000/api/visitantes \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "telefone": "11999887766",
    "dataVisita": "2025-06-27T10:00:00",
    "email": "joao@email.com",
    "observacoes": "Primeira visita, interessado em conhecer mais sobre a igreja"
  }'
```

## Listar Visitantes

```bash
curl -X GET http://localhost:5000/api/visitantes
```

## Obter Visitante por ID

```bash
curl -X GET http://localhost:5000/api/visitantes/1
```

## Criar Configuração de Mensagem

```bash
curl -X POST http://localhost:5000/api/configuracoesMensagens \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Seguimento 3 dias",
    "textoMensagem": "Olá {Nome}! Como você está? Gostaríamos de saber como foi sua experiência conosco!",
    "diasAposVisita": 3,
    "horarioEnvio": "14:00:00",
    "ativo": true
  }'
```

## Listar Configurações Ativas

```bash
curl -X GET http://localhost:5000/api/configuracoesMensagens/ativas
```

## Listar Mensagens Agendadas

```bash
curl -X GET http://localhost:5000/api/mensagensAgendadas
```

## Listar Mensagens Prontas para Envio

```bash
curl -X GET http://localhost:5000/api/mensagensAgendadas/prontas-para-envio
```

## Listar Mensagens de um Visitante

```bash
curl -X GET http://localhost:5000/api/mensagensAgendadas/visitante/1
```

## Marcar Mensagem como Enviada

```bash
curl -X POST http://localhost:5000/api/mensagensAgendadas/1/marcar-enviada
```

## Marcar Mensagem com Erro

```bash
curl -X POST http://localhost:5000/api/mensagensAgendadas/1/marcar-erro \
  -H "Content-Type: application/json" \
  -d '"Número de telefone inválido"'
```

## Exemplo de Resposta - Visitante

```json
{
  "id": 1,
  "nome": "João Silva",
  "telefone": "11999887766",
  "dataVisita": "2025-06-27T10:00:00",
  "email": "joao@email.com",
  "observacoes": "Primeira visita, interessado em conhecer mais sobre a igreja",
  "dataCadastro": "2025-06-27T15:30:00"
}
```

## Exemplo de Resposta - Mensagem Agendada

```json
{
  "id": 1,
  "visitanteId": 1,
  "nomeVisitante": "João Silva",
  "telefoneVisitante": "11999887766",
  "configuracaoMensagemId": 1,
  "nomeConfiguracao": "Boas-vindas",
  "dataAgendamento": "2025-06-27T15:30:00",
  "dataEnvio": "2025-06-28T10:00:00",
  "status": 2,
  "textoFinal": "Olá João Silva! Que alegria ter você conosco na igreja! Esperamos vê-lo novamente em breve. Deus abençoe!",
  "dataProcessamento": "2025-06-28T10:05:00",
  "logErro": null,
  "dataCriacao": "2025-06-27T15:30:00"
}
```

## Status das Mensagens

- **1** - Agendada
- **2** - Pronta para Envio
- **3** - Enviada
- **4** - Erro
- **5** - Cancelada

## Testando o Agendamento

1. Cadastre um visitante com data de visita recente
2. Aguarde alguns minutos para o serviço processar
3. Verifique os logs da aplicação
4. Consulte as mensagens agendadas via API

## Swagger UI

Acesse http://localhost:5000/swagger para uma interface interativa da API com todos os endpoints documentados.

