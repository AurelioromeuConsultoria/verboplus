# Cadastro de Membro — Kingdom

Página independente para cadastro público de membros da igreja.

## Uso

1. **Logo (opcional):** Para exibir o logo da Kingdom, copie `Portal/public/images/logo.png` para esta pasta como `logo.png`. Sem o arquivo, será exibido o texto "Kingdom" em SVG.

2. **API:** A página se conecta automaticamente à API:
   - Em **localhost**: `http://localhost:7000`
   - Em **produção**: mesma origem da página
   - Para sobrescrever: defina `window.CADASTRO_API_BASE` antes do carregamento (ex: em um script inline no HTML)

3. **CORS:** Se a página for hospedada em outro domínio, configure CORS na API para permitir a origem.

## Publicação

- Hospede a pasta `CadastroMembro/` em qualquer servidor de arquivos estáticos (GitHub Pages, Netlify, Azure Static Web Apps, etc.)
- Ou sirva via API: adicione middleware de arquivos estáticos apontando para esta pasta
- QR code: gere um QR code apontando para a URL da página e imprima para uso na igreja

## Endpoint

`POST /api/Membros/cadastro`

Payload:
```json
{
  "nome": "João Silva",
  "whatsApp": "11999999999",
  "email": "joao@email.com",
  "dataNascimento": "1990-01-15"
}
```
