# Como Criar o Primeiro Usuário

## 🔓 Status Atual da API

### Endpoints Públicos (sem autenticação):
- ✅ `POST /api/auth/login` - Login
- ✅ `POST /api/auth/refresh` - Renovar token
- ✅ `POST /api/usuarios` - **Criar usuário (apenas se não existir nenhum usuário)**

### Endpoints Protegidos (requerem autenticação):
- 🔒 `GET /api/usuarios` - Listar usuários
- 🔒 `GET /api/usuarios/{id}` - Buscar usuário
- 🔒 `PUT /api/usuarios/{id}` - Atualizar usuário
- 🔒 `DELETE /api/usuarios/{id}` - Deletar usuário
- 🔒 `GET /api/auth/me` - Dados do usuário logado
- 🔒 `PUT /api/auth/alterar-senha` - Alterar senha

## 🎯 Como Funciona

O endpoint `POST /api/usuarios` tem uma lógica especial:

1. **Se NÃO existir nenhum usuário no banco:**
   - ✅ Endpoint é **PÚBLICO** (não precisa autenticação)
   - Permite criar o primeiro usuário admin

2. **Se JÁ existir pelo menos um usuário:**
   - 🔒 Endpoint exige **AUTENTICAÇÃO**
   - Só usuários autenticados podem criar novos usuários

## 📝 Passo a Passo

### 1. Iniciar a API
```bash
cd src/SistemaIgreja.API
dotnet run
```

### 2. Criar o Primeiro Usuário (Público)

**Via Swagger:**
1. Acesse: `http://localhost:5000/swagger`
2. Vá para `POST /api/usuarios`
3. Clique em "Try it out"
4. Cole o JSON:
```json
{
  "nome": "Administrador",
  "email": "admin@igreja.com",
  "senha": "admin123",
  "tipoUsuario": 1
}
```
5. Execute

**Via cURL:**
```bash
curl -X POST "http://localhost:5000/api/usuarios" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Administrador",
    "email": "admin@igreja.com",
    "senha": "admin123",
    "tipoUsuario": 1
  }'
```

### 3. Fazer Login

Agora que o usuário existe, faça login:

**Via Swagger:**
1. Vá para `POST /api/auth/login`
2. Use:
```json
{
  "email": "admin@igreja.com",
  "senha": "admin123"
}
```
3. Copie o `token` da resposta

### 4. Autorizar no Swagger

1. Clique no botão **"Authorize"** (cadeado no topo do Swagger)
2. Cole o token (sem "Bearer")
3. Clique em "Authorize"
4. Agora você pode usar todos os endpoints protegidos!

### 5. Criar Outros Usuários (Agora Requer Autenticação)

Agora que já existe um usuário, o `POST /api/usuarios` exige autenticação:

1. Certifique-se de estar autorizado no Swagger (passo 4)
2. Use `POST /api/usuarios` normalmente
3. O token será validado automaticamente

## 🔐 Valores do TipoUsuario

- `1` = **Admin** - Acesso ao módulo administrativo
- `2` = **Portal** - Acesso ao portal público  
- `3` = **Ambos** - Acesso aos dois módulos

## ⚠️ Importante

- **Apenas o primeiro usuário** pode ser criado sem autenticação
- Após criar o primeiro usuário, **todos os outros** precisam de autenticação
- Isso garante segurança: só quem já está autenticado pode criar novos usuários

## 🛡️ Segurança

- Senhas são hasheadas com BCrypt
- Email tem índice único (não permite duplicatas)
- Tokens JWT expiram em 1 hora
- Use refresh token para renovar sem fazer login novamente


