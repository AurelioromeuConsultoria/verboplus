# Sistema Igreja - Backend

## Configuração e Execução

### Pré-requisitos
- .NET 8.0 SDK
- SQLite (já incluído no projeto)

### Executar o Backend

1. **Navegar para o diretório da API:**
   ```bash
   cd src/SistemaIgreja.API
   ```

2. **Restaurar dependências:**
   ```bash
   dotnet restore
   ```

3. **Executar a aplicação:**
   ```bash
   dotnet run --launch-profile http
   ```

   Ou simplesmente:
   ```bash
   dotnet run
   ```

### Configurações Aplicadas

#### ✅ Problemas Resolvidos:

1. **Redirecionamento HTTPS removido**: Removido `app.UseHttpsRedirection()` para evitar redirecionamento forçado
2. **CORS configurado**: Permitindo origens do frontend (localhost:5173, 3000, 4173)
3. **Porta fixa**: API configurada para rodar na porta 5000
4. **Configuração flexível**: CORS configurado via appsettings.json

#### 🔧 Configurações:

- **URL da API**: `http://localhost:5000`
- **Swagger**: `http://localhost:5000/swagger`
- **CORS**: Configurado para aceitar requisições do frontend React

### Endpoints Disponíveis

- `GET /api/visitantes` - Listar todos os visitantes
- `GET /api/visitantes/{id}` - Buscar visitante por ID
- `POST /api/visitantes` - Criar novo visitante
- `PUT /api/visitantes/{id}` - Atualizar visitante
- `DELETE /api/visitantes/{id}` - Deletar visitante

### Estrutura do Projeto

```
src/
├── SistemaIgreja.API/          # Camada de apresentação (Controllers)
├── SistemaIgreja.Application/   # Camada de aplicação (Services, DTOs)
├── SistemaIgreja.Domain/        # Camada de domínio (Entities)
└── SistemaIgreja.Infrastructure/ # Camada de infraestrutura (Data, Repositories)
```

### Solução de Problemas

#### Se o frontend não conseguir conectar:

1. **Verificar se a API está rodando:**
   - Acesse `http://localhost:5000/swagger`
   - Deve mostrar a documentação da API

2. **Verificar CORS:**
   - A API está configurada para aceitar requisições de `http://localhost:5173`
   - Se o frontend estiver em outra porta, adicione no `appsettings.json`

3. **Verificar porta:**
   - Certifique-se de que a porta 5000 não está sendo usada por outro processo

#### Logs de Erro Comuns:

- **Erro 404**: Verificar se a rota está correta (`/api/visitantes`)
- **Erro CORS**: Verificar se a origem do frontend está na lista de origens permitidas
- **Erro de conexão**: Verificar se a API está rodando na porta correta

### Desenvolvimento

Para desenvolvimento, a aplicação:
- Usa SQLite como banco de dados
- Cria o banco automaticamente na primeira execução
- Inclui Swagger para documentação da API
- Configura CORS para desenvolvimento local 