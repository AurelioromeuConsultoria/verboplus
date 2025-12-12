# 🔧 Configuração do Backend para Servir Imagens

## Problema
As imagens estão sendo salvas corretamente na pasta `uploads/fotos/` do backend, mas não estão sendo servidas como arquivos estáticos, causando erro 404 quando o frontend tenta acessá-las.

## Solução

### 1. Configurar Servidor de Arquivos Estáticos

No arquivo `Program.cs` ou `Startup.cs` do seu backend ASP.NET Core, adicione a configuração para servir arquivos estáticos da pasta `uploads`:

#### Opção 1: Usando `UseStaticFiles()` com mapeamento customizado

```csharp
// No Program.cs, após var app = builder.Build();

// Servir arquivos da pasta wwwroot (padrão)
app.UseStaticFiles();

// Servir arquivos da pasta uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});
```

**Importante:** Certifique-se de que a pasta `uploads` esteja na raiz do projeto do backend, não dentro de `wwwroot`.

#### Opção 2: Se as imagens estão dentro de wwwroot

Se as imagens estão sendo salvas dentro de `wwwroot/uploads/`, então apenas `app.UseStaticFiles()` já deve funcionar. Nesse caso, verifique se a URL no frontend está correta.

### 2. Estrutura de Pastas Esperada

```
Backend/
└── src/
    └── SistemaIgreja.API/
        ├── uploads/              ← Pasta para uploads
        │   └── fotos/
        │       └── {galeriaId}/
        │           ├── original/
        │           └── thumbnail/
        └── wwwroot/              ← Se usar esta pasta
            └── uploads/
                └── fotos/
```

### 3. Verificar CORS (se necessário)

Se você encontrar erros de CORS ao carregar imagens, adicione ao `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL do frontend
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Depois de app = builder.Build()
app.UseCors("AllowFrontend");
```

### 4. Testar a Configuração

Após configurar, teste acessando diretamente no navegador:

```
http://localhost:5000/uploads/fotos/b0f2ca3d-9528-4f0c-86e5-f569ed42da2a/thumbnail/e98cc523-daf7-4b5d-987c-1abfc21116ed.jpg
```

Se a imagem aparecer, a configuração está correta!

## Exemplo Completo de Program.cs

```csharp
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ... outras configurações ...

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Servir arquivos estáticos da pasta wwwroot
app.UseStaticFiles();

// Servir arquivos da pasta uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Notas Importantes

1. **ContentRootPath vs WebRootPath:**
   - `ContentRootPath` = Raiz do projeto (onde está o `.csproj`)
   - `WebRootPath` = Pasta `wwwroot` (padrão para arquivos estáticos)

2. **Se as imagens estão em `wwwroot/uploads`:**
   - Use apenas `app.UseStaticFiles()` (já serve tudo dentro de `wwwroot`)
   - A URL será: `http://localhost:5000/uploads/fotos/...`

3. **Se as imagens estão fora de `wwwroot` (raiz do projeto):**
   - Use `PhysicalFileProvider` com `ContentRootPath`
   - Configure o `RequestPath` para mapear a rota

## Troubleshooting

### Erro 404 ao acessar imagem
- Verifique se a pasta `uploads` existe no local correto
- Verifique se `UseStaticFiles()` está sendo chamado antes de `UseRouting()` ou `UseEndpoints()`
- Verifique se o caminho no banco de dados está correto

### Imagem aparece no navegador mas não no frontend
- Pode ser problema de CORS - configure o CORS adequadamente
- Verifique se a URL está sendo construída corretamente no frontend

### Erro de permissões
- Verifique as permissões da pasta `uploads` no sistema operacional
- Certifique-se de que o servidor tem permissão de leitura na pasta


