# Comandos CURL para testar o módulo Kids

## 1. Primeiro, faça login para obter o token JWT

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "seu-email@exemplo.com",
    "senha": "sua-senha"
  }'
```

**Copie o token do campo `token` da resposta.**

---

## 2. Criar criança (versão simples - sem responsáveis)

```bash
curl -X POST http://localhost:5000/api/kids/criancas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "nome": "Arthur Sanches Soares",
    "dataNascimento": "2023-03-04T00:00:00Z",
    "email": "arthur@exemplo.com",
    "telefone": "11999999999",
    "whatsApp": "11999999999",
    "alergias": "teste",
    "restricoesAlimentares": "teste",
    "observacoes": "teste",
    "salaId": "Sala-01"
  }'
```

---

## 3. Criar criança com responsável existente

```bash
curl -X POST http://localhost:5000/api/kids/criancas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "nome": "Maria Silva",
    "dataNascimento": "2020-05-15T00:00:00Z",
    "email": "maria@exemplo.com",
    "telefone": "11988888888",
    "whatsApp": "11988888888",
    "alergias": "Nenhuma",
    "restricoesAlimentares": "Vegetariana",
    "observacoes": "Criança muito ativa",
    "salaId": "Sala-02",
    "responsaveis": [
      {
        "responsavelPessoaId": 1,
        "podeRetirar": true,
        "parentesco": "Mãe"
      }
    ]
  }'
```

---

## 4. Criar criança com novo responsável (cria pessoa do responsável automaticamente)

```bash
curl -X POST http://localhost:5000/api/kids/criancas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "nome": "João Pedro",
    "dataNascimento": "2019-08-20T00:00:00Z",
    "email": "joao@exemplo.com",
    "telefone": "11977777777",
    "whatsApp": "11977777777",
    "alergias": "Amendoim",
    "restricoesAlimentares": null,
    "observacoes": "Cuidado com amendoim",
    "salaId": "Sala-03",
    "responsaveis": [
      {
        "nome": "Ana Paula",
        "telefone": "11966666666",
        "whatsApp": "11966666666",
        "email": "ana@exemplo.com",
        "podeRetirar": true,
        "parentesco": "Mãe"
      },
      {
        "nome": "Carlos Silva",
        "telefone": "11955555555",
        "whatsApp": "11955555555",
        "email": "carlos@exemplo.com",
        "podeRetirar": true,
        "parentesco": "Pai"
      }
    ]
  }'
```

---

## 5. Listar todas as crianças

```bash
curl -X GET http://localhost:5000/api/kids/criancas \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

---

## 6. Buscar criança por ID

```bash
curl -X GET http://localhost:5000/api/kids/criancas/1 \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

---

## 7. Realizar Check-in

```bash
curl -X POST http://localhost:5000/api/kids/checkin \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "criancaPessoaId": 1,
    "metodo": "ADMIN",
    "checkinByPessoaId": 1,
    "observacoes": "Check-in realizado pelo sistema"
  }'
```

---

## 8. Realizar Check-out

```bash
curl -X POST http://localhost:5000/api/kids/checkout \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "criancaPessoaId": 1,
    "codigoSessao": "CODIGO_SESSAO_DO_CHECKIN",
    "checkoutByPessoaId": 1,
    "metodo": "ADMIN"
  }'
```

---

## 9. Listar histórico de check-ins

```bash
# Todos os check-ins
curl -X GET http://localhost:5000/api/kids/checkins \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"

# Check-ins de uma criança específica
curl -X GET "http://localhost:5000/api/kids/checkins?criancaPessoaId=1" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

---

## IMPORTANTE:

1. **Substitua `SEU_TOKEN_AQUI`** pelo token JWT obtido no login
2. **Ajuste a porta** se sua API estiver rodando em outra porta (verifique no console da aplicação)
3. **Ajuste os IDs** conforme seus dados reais do banco


