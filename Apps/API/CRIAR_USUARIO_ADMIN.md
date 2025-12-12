# Como Criar o Primeiro Usuário Admin

Após iniciar a API, você precisa criar o primeiro usuário administrador. Existem duas formas:

## Opção 1: Via Swagger (Recomendado)

1. Inicie a API:
   ```bash
   cd src/SistemaIgreja.API
   dotnet run
   ```

2. Acesse o Swagger: `http://localhost:5000/swagger`

3. Encontre o endpoint `POST /api/usuarios`

4. Clique em "Try it out"

5. Cole o JSON abaixo e execute:
   ```json
   {
     "nome": "Administrador",
     "email": "admin@igreja.com",
     "senha": "admin123",
     "tipoUsuario": 1
   }
   ```

6. Você receberá a resposta com o usuário criado (sem a senha, claro)

## Opção 2: Via cURL ou Postman

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

## Valores do TipoUsuario

- `1` = Admin (Acesso ao módulo administrativo)
- `2` = Portal (Acesso ao portal público)
- `3` = Ambos (Acesso aos dois módulos)

## Testar Login

Após criar o usuário, teste o login:

**Via Swagger:**
1. Vá para `POST /api/auth/login`
2. Use:
   ```json
   {
     "email": "admin@igreja.com",
     "senha": "admin123"
   }
   ```
3. Você receberá o token JWT

**Copiar o Token:**
- Copie o valor do campo `token` da resposta
- No Swagger, clique no botão "Authorize" (cadeado no topo)
- Cole o token (sem "Bearer")
- Agora você pode testar os endpoints protegidos!

## Importante

⚠️ **Altere a senha padrão após o primeiro login!**

Use o endpoint `PUT /api/auth/alterar-senha` para alterar a senha.





