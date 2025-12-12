# Sistema de Autenticação - Implementação Completa

## ✅ O que foi implementado:

### 1. **Entidades e Enums**
- ✅ `TipoUsuario` enum (Admin, Portal, Ambos)
- ✅ `Usuario` entity com todos os campos necessários

### 2. **DTOs**
- ✅ `LoginDto` - Dados de login
- ✅ `LoginResponseDto` - Resposta com token e dados do usuário
- ✅ `RefreshTokenDto` - Renovação de token
- ✅ `AlterarSenhaDto` - Alteração de senha
- ✅ `UsuarioDto` - Dados do usuário
- ✅ `CriarUsuarioDto` - Criação de usuário
- ✅ `AtualizarUsuarioDto` - Atualização de usuário

### 3. **Repositories e Services**
- ✅ `IUsuarioRepository` e `UsuarioRepository`
- ✅ `IUsuarioService` e `UsuarioService`
- ✅ `IAuthService` e `AuthService` (com JWT)

### 4. **Controllers**
- ✅ `AuthController` - Endpoints de autenticação
- ✅ `UsuariosController` - CRUD de usuários (protegido)

### 5. **Configurações**
- ✅ JWT configurado no `Program.cs`
- ✅ Swagger configurado para aceitar JWT
- ✅ `appsettings.json` com configuração JWT
- ✅ Pacotes NuGet adicionados:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `BCrypt.Net-Next`
  - `System.IdentityModel.Tokens.Jwt`

### 6. **Database**
- ✅ `DbContext` atualizado com entidade `Usuario`
- ✅ Índice único em Email
- ⚠️ **Migration pendente** (criar quando API estiver parada)

## 📋 Endpoints Disponíveis

### Autenticação (Públicos)
```
POST   /api/auth/login          - Login
POST   /api/auth/refresh         - Renovar token
GET    /api/auth/me             - Dados do usuário logado (protegido)
PUT    /api/auth/alterar-senha  - Alterar senha (protegido)
```

### Usuários (Protegido - requer autenticação)
```
GET    /api/usuarios            - Listar todos
GET    /api/usuarios/{id}       - Buscar por ID
POST   /api/usuarios            - Criar usuário
PUT    /api/usuarios/{id}       - Atualizar usuário
DELETE /api/usuarios/{id}       - Deletar usuário
```

## 🔐 Como Usar

### 1. Login
```json
POST /api/auth/login
{
  "email": "admin@igreja.com",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "usuario": {
    "id": 1,
    "nome": "Administrador",
    "email": "admin@igreja.com",
    "tipoUsuario": "Admin",
    "tipoUsuarioDescricao": "Administrador",
    "ativo": true
  }
}
```

### 2. Usar Token nas Requisições
```
Authorization: Bearer {token}
```

### 3. Swagger
- Clique no botão "Authorize" no Swagger
- Cole o token (sem "Bearer")
- Todas as requisições protegidas funcionarão

## 🔧 Próximos Passos

1. **Parar a API** e criar a migration:
   ```bash
   dotnet ef migrations add AddUsuario --project .\src\SistemaIgreja.Infrastructure\SistemaIgreja.Infrastructure.csproj --startup-project .\src\SistemaIgreja.API\SistemaIgreja.API.csproj
   ```

2. **Criar primeiro usuário admin** (via Swagger ou script):
   ```json
   POST /api/usuarios
   {
     "nome": "Administrador",
     "email": "admin@igreja.com",
     "senha": "admin123",
     "tipoUsuario": 1
   }
   ```

3. **Proteger endpoints existentes** adicionando `[Authorize]` nos controllers que precisam

4. **Frontend**: Implementar login nos dois React apps usando os endpoints de `/api/auth`

## ⚠️ Importante

- A chave JWT está em `appsettings.json` - **altere em produção**!
- Refresh tokens estão em memória (dicionário) - **use Redis ou banco em produção**
- Senhas são hasheadas com BCrypt
- Email tem índice único (não permite duplicatas)

## 🎯 Estrutura de Permissões

Por enquanto, todos os endpoints protegidos requerem apenas autenticação. Para implementar permissões baseadas em `TipoUsuario`:

1. Criar Policy no `Program.cs`:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireClaim("TipoUsuario", "Admin", "Ambos"));
});
```

2. Usar nos controllers:
```csharp
[Authorize(Policy = "AdminOnly")]
```





