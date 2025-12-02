# Prompt para Frontend Admin - Gerenciamento de Usuários

## Contexto
Você está construindo o frontend administrativo (React) de uma API REST para gestão de uma igreja. Agora precisa implementar a funcionalidade completa de **Gerenciamento de Usuários**, onde administradores podem criar, editar, listar e gerenciar usuários do sistema.

## Estrutura da API

### Base URL
```
http://localhost:5000/api
```

### Autenticação
**IMPORTANTE:** Todos os endpoints (exceto login) requerem autenticação JWT.

**Como autenticar:**
1. Fazer login em `POST /api/auth/login`
2. Receber o token JWT na resposta
3. Incluir o token em todas as requisições:
   ```
   Authorization: Bearer {token}
   ```

### Endpoints Disponíveis

#### 1. Login (Público)
```
POST /api/auth/login
```
**Body:** `LoginDto`
**Resposta:** `LoginResponseDto`
**Uso:** Autenticação inicial do administrador

#### 2. Obter dados do usuário logado
```
GET /api/auth/me
```
**Headers:** `Authorization: Bearer {token}`
**Resposta:** `UsuarioDto`
**Uso:** Exibir dados do usuário logado no header/navbar

#### 3. Alterar senha
```
PUT /api/auth/alterar-senha
```
**Headers:** `Authorization: Bearer {token}`
**Body:** `AlterarSenhaDto`
**Uso:** Permite que o usuário logado altere sua própria senha

#### 4. Listar todos os usuários
```
GET /api/usuarios
```
**Headers:** `Authorization: Bearer {token}`
**Resposta:** Array de `UsuarioDto`
**Uso:** Tela de listagem de usuários

#### 5. Buscar usuário por ID
```
GET /api/usuarios/{id}
```
**Headers:** `Authorization: Bearer {token}`
**Resposta:** `UsuarioDto`
**Uso:** Detalhes de um usuário específico

#### 6. Criar novo usuário
```
POST /api/usuarios
```
**Headers:** `Authorization: Bearer {token}`
**Body:** `CriarUsuarioDto`
**Resposta:** `UsuarioDto`
**Uso:** Formulário de criação de usuário

**IMPORTANTE:** Este endpoint só aceita requisições autenticadas. O primeiro usuário deve ser criado via Swagger ou script.

#### 7. Atualizar usuário
```
PUT /api/usuarios/{id}
```
**Headers:** `Authorization: Bearer {token}`
**Body:** `AtualizarUsuarioDto`
**Resposta:** `UsuarioDto`
**Uso:** Formulário de edição de usuário

#### 8. Deletar usuário
```
DELETE /api/usuarios/{id}
```
**Headers:** `Authorization: Bearer {token}`
**Uso:** Remover usuário do sistema

## Estrutura de Dados

### LoginDto (POST /api/auth/login)
```typescript
interface LoginDto {
  email: string;
  senha: string;
}
```

### LoginResponseDto (Resposta do login)
```typescript
interface LoginResponseDto {
  token: string;                    // JWT token - usar em todas as requisições
  refreshToken: string;             // Token para renovação
  expiresIn: number;                // Tempo de expiração em segundos (3600 = 1 hora)
  usuario: UsuarioDto;
}
```

### UsuarioDto (Resposta padrão)
```typescript
interface UsuarioDto {
  id: number;
  nome: string;
  email: string;
  tipoUsuario: TipoUsuario;        // 1=Admin, 2=Portal, 3=Ambos
  tipoUsuarioDescricao: string;    // "Administrador", "Portal", "Administrador e Portal"
  ativo: boolean;
  dataCriacao: string;              // ISO DateTime
  ultimoAcesso?: string;           // ISO DateTime (null se nunca acessou)
}

enum TipoUsuario {
  Admin = 1,        // Acesso ao módulo administrativo
  Portal = 2,       // Acesso ao portal público
  Ambos = 3         // Acesso aos dois módulos
}
```

### CriarUsuarioDto (POST /api/usuarios)
```typescript
interface CriarUsuarioDto {
  nome: string;                     // Obrigatório
  email: string;                    // Obrigatório, único
  senha: string;                     // Obrigatório
  tipoUsuario: TipoUsuario;          // 1, 2 ou 3
}
```

### AtualizarUsuarioDto (PUT /api/usuarios/{id})
```typescript
interface AtualizarUsuarioDto {
  nome: string;
  email: string;                    // Deve ser único (exceto o próprio usuário)
  tipoUsuario: TipoUsuario;
  ativo: boolean;                   // Permite ativar/desativar usuário
}
```

### AlterarSenhaDto (PUT /api/auth/alterar-senha)
```typescript
interface AlterarSenhaDto {
  senhaAtual: string;
  novaSenha: string;
}
```

## Validações da API

A API retorna erros nas seguintes situações:

1. **Email já cadastrado:** `"Email já cadastrado"` (400 Bad Request)
2. **Usuário não encontrado:** 404 Not Found
3. **Não autenticado:** 401 Unauthorized
4. **Token inválido/expirado:** 401 Unauthorized
5. **Senha atual incorreta:** `"Senha atual incorreta"` (401 Unauthorized)

## Funcionalidades a Implementar

### 1. Tela de Login
**Rota:** `/login` ou `/auth/login`

**Campos:**
- Email (obrigatório, tipo email)
- Senha (obrigatório, tipo password)

**Comportamento:**
- Ao submeter, fazer POST para `/api/auth/login`
- Em caso de sucesso:
  - Armazenar `token` e `refreshToken` (localStorage ou sessionStorage)
  - Armazenar dados do `usuario`
  - Redirecionar para dashboard/home
- Em caso de erro:
  - Mostrar mensagem: "Email ou senha inválidos"
  - Limpar campos de senha

**Validações:**
- Email deve ser válido
- Senha não pode estar vazia

### 2. Layout Principal com Autenticação
**Componentes necessários:**
- Header/Navbar com:
  - Nome do usuário logado
  - Botão de logout
  - Menu de navegação
- Verificação de autenticação em todas as rotas protegidas
- Redirecionar para `/login` se não autenticado

**Gerenciamento de Token:**
- Verificar se token existe antes de cada requisição
- Se token expirou (401), redirecionar para login
- Opcional: Implementar refresh token automático

### 3. Tela de Listagem de Usuários
**Rota:** `/usuarios` ou `/admin/usuarios`

**Funcionalidades:**
- Tabela com colunas:
  - Nome
  - Email
  - Tipo de Usuário (badge colorido)
  - Status (Ativo/Inativo - badge)
  - Data de Criação
  - Último Acesso
  - Ações (Editar, Excluir, Ativar/Desativar)

**Filtros:**
- Busca por nome ou email
- Filtro por tipo de usuário (dropdown)
- Filtro por status (Ativo/Inativo)

**Ações:**
- Botão "Novo Usuário" (abre modal/formulário)
- Botão "Editar" (abre modal/formulário com dados preenchidos)
- Botão "Excluir" (com confirmação)
- Toggle "Ativo/Inativo" (mudança rápida de status)

**Badges:**
- Tipo de Usuário:
  - Admin: Azul
  - Portal: Verde
  - Ambos: Roxo
- Status:
  - Ativo: Verde
  - Inativo: Cinza/Vermelho

### 4. Modal/Formulário de Criar/Editar Usuário
**Componente reutilizável para criar e editar**

**Campos:**
- Nome (texto, obrigatório)
- Email (email, obrigatório)
- Tipo de Usuário (select/dropdown):
  - Administrador (1)
  - Portal (2)
  - Ambos (3)
- Senha (password, obrigatório apenas na criação)
- Confirmar Senha (password, apenas na criação)
- Ativo (checkbox, apenas na edição)

**Validações:**
- Nome: obrigatório, mínimo 3 caracteres
- Email: obrigatório, formato válido
- Senha (criação): obrigatória, mínimo 6 caracteres
- Confirmar Senha: deve ser igual à senha
- Email único: validar se já existe (mostrar erro se duplicado)

**Comportamento:**
- Ao criar: POST `/api/usuarios`
- Ao editar: PUT `/api/usuarios/{id}`
- Fechar modal após sucesso
- Atualizar lista de usuários

### 5. Tela de Perfil do Usuário Logado
**Rota:** `/perfil` ou `/meu-perfil`

**Seções:**
1. **Dados Pessoais** (somente leitura):
   - Nome
   - Email
   - Tipo de Usuário
   - Data de Criação
   - Último Acesso

2. **Alterar Senha**:
   - Senha Atual
   - Nova Senha
   - Confirmar Nova Senha
   - Botão "Alterar Senha"

**Validações:**
- Senha atual: obrigatória
- Nova senha: obrigatória, mínimo 6 caracteres
- Confirmar senha: deve ser igual à nova senha

### 6. Logout
**Funcionalidade:**
- Botão no header/navbar
- Ao clicar:
  - Limpar token do storage
  - Limpar dados do usuário
  - Redirecionar para `/login`

## Fluxos de Uso

### Fluxo 1: Login e Acesso
1. Usuário acessa `/login`
2. Preenche email e senha
3. Submete formulário
4. Recebe token e dados do usuário
5. Redireciona para dashboard
6. Todas as requisições incluem token no header

### Fluxo 2: Criar Novo Usuário
1. Admin acessa `/usuarios`
2. Clica em "Novo Usuário"
3. Preenche formulário
4. Submete
5. Usuário é criado
6. Lista é atualizada

### Fluxo 3: Editar Usuário
1. Admin acessa `/usuarios`
2. Clica em "Editar" em um usuário
3. Modal abre com dados preenchidos
4. Altera campos desejados
5. Submete
6. Usuário é atualizado
7. Lista é atualizada

### Fluxo 4: Desativar Usuário
1. Admin acessa `/usuarios`
2. Clica no toggle "Ativo/Inativo"
3. Confirma ação
4. Status é atualizado
5. Lista é atualizada

### Fluxo 5: Alterar Própria Senha
1. Usuário acessa `/perfil`
2. Vai para seção "Alterar Senha"
3. Preenche senha atual e nova senha
4. Submete
5. Senha é alterada
6. Mostra mensagem de sucesso

## Exemplos de Requisições

### Login
```json
POST /api/auth/login
{
  "email": "admin@igreja.com",
  "senha": "admin123"
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
    "tipoUsuario": 1,
    "tipoUsuarioDescricao": "Administrador",
    "ativo": true,
    "dataCriacao": "2025-11-28T02:12:11",
    "ultimoAcesso": "2025-11-28T10:30:00"
  }
}
```

### Criar Usuário
```json
POST /api/usuarios
Headers: {
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
Body: {
  "nome": "João Silva",
  "email": "joao@igreja.com",
  "senha": "senha123",
  "tipoUsuario": 2
}
```

### Atualizar Usuário
```json
PUT /api/usuarios/2
Headers: {
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
Body: {
  "nome": "João Silva Santos",
  "email": "joao.silva@igreja.com",
  "tipoUsuario": 3,
  "ativo": true
}
```

### Alterar Senha
```json
PUT /api/auth/alterar-senha
Headers: {
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
Body: {
  "senhaAtual": "senha123",
  "novaSenha": "novaSenha456"
}
```

## Sugestões de UI/UX

### 1. Tela de Login
- Design limpo e profissional
- Logo da igreja (se disponível)
- Campos com labels claros
- Botão de submit destacado
- Link "Esqueci minha senha" (opcional, para futuro)
- Mensagens de erro claras e visíveis

### 2. Listagem de Usuários
- Tabela responsiva
- Paginação se houver muitos usuários
- Busca em tempo real (debounce)
- Filtros visíveis e fáceis de usar
- Ações rápidas (sem precisar abrir modal para ativar/desativar)
- Confirmação para ações destrutivas (excluir)

### 3. Formulário de Usuário
- Modal ou página dedicada
- Validação em tempo real
- Mensagens de erro específicas
- Botões de ação claros (Salvar/Cancelar)
- Loading state durante submissão

### 4. Perfil
- Layout em cards ou seções
- Informações organizadas
- Formulário de senha separado e destacado
- Feedback visual claro

### 5. Header/Navbar
- Nome do usuário visível
- Dropdown com opções (Perfil, Logout)
- Indicador visual de autenticação
- Logo/branding

## Tratamento de Erros

Sempre trate os seguintes cenários:

1. **Erro 401 (Unauthorized):**
   - Token expirado ou inválido
   - Limpar storage
   - Redirecionar para `/login`
   - Mostrar mensagem: "Sua sessão expirou. Faça login novamente."

2. **Erro 400 (Bad Request):**
   - Mostrar mensagem específica retornada pela API
   - Exemplo: "Email já cadastrado"

3. **Erro 404 (Not Found):**
   - Usuário não encontrado
   - Mostrar mensagem apropriada

4. **Erro 500 (Server Error):**
   - Erro genérico do servidor
   - Mostrar: "Erro ao processar solicitação. Tente novamente."

5. **Erro de Rede:**
   - API offline ou inacessível
   - Mostrar: "Não foi possível conectar ao servidor."

## Gerenciamento de Estado

**Recomendações:**
- Usar Context API ou Redux para:
  - Estado de autenticação
  - Dados do usuário logado
  - Token JWT
- Persistir token em localStorage ou sessionStorage
- Verificar autenticação em rotas protegidas

## Interceptor de Requisições

**Implementar interceptor HTTP que:**
- Adiciona token automaticamente em todas as requisições
- Trata erros 401 automaticamente
- Renova token quando necessário (opcional)

## Exemplo de Estrutura de Componentes

```
src/
├── pages/
│   ├── Login.tsx
│   ├── Usuarios/
│   │   ├── ListaUsuarios.tsx
│   │   ├── FormularioUsuario.tsx
│   │   └── index.ts
│   └── Perfil.tsx
├── components/
│   ├── Layout/
│   │   ├── Header.tsx
│   │   ├── Navbar.tsx
│   │   └── ProtectedRoute.tsx
│   └── Usuarios/
│       ├── TabelaUsuarios.tsx
│       ├── ModalUsuario.tsx
│       └── BadgeTipoUsuario.tsx
├── services/
│   ├── api.ts (configuração axios/fetch)
│   ├── authService.ts
│   └── usuarioService.ts
├── context/
│   └── AuthContext.tsx
└── types/
    └── usuario.ts
```

## Observações Importantes

1. **Primeiro Usuário:** O primeiro usuário deve ser criado via Swagger ou script. O endpoint `POST /api/usuarios` só aceita requisições autenticadas após o primeiro usuário existir.

2. **Senha na Edição:** O campo senha NÃO aparece no formulário de edição. Para alterar senha, o usuário deve usar a tela de perfil.

3. **Ativar/Desativar:** Pode ser feito via toggle rápido na lista ou no formulário de edição.

4. **Último Acesso:** Campo informativo, não editável.

5. **Tipo de Usuário:** 
   - Admin = Acesso total ao sistema administrativo
   - Portal = Acesso ao portal público (membros)
   - Ambos = Acesso completo

6. **Segurança:**
   - Nunca exibir senhas
   - Sempre usar HTTPS em produção
   - Validar tokens antes de cada requisição
   - Implementar logout automático em caso de token inválido

## Integração com Outros Módulos

Este módulo de usuários deve se integrar com:
- Sistema de permissões (futuro)
- Logs de auditoria (futuro)
- Notificações (futuro)

## Testes Recomendados

- Login com credenciais válidas
- Login com credenciais inválidas
- Criar usuário com sucesso
- Criar usuário com email duplicado
- Editar usuário
- Desativar usuário
- Excluir usuário
- Alterar própria senha
- Token expirado
- Logout


