# Nginx + CORS na VPS

Quando a API está na VPS atrás de um **reverse proxy nginx**, o proxy precisa repassar os headers corretamente. Se o nginx bloquear ou modificar requisições, o CORS falha.

## Configuração recomendada do nginx

O nginx **deve repassar** as requisições (incluindo OPTIONS/preflight) para a API .NET. **Não** adicione headers CORS no nginx – a API .NET já trata isso.

```nginx
server {
    listen 80;
    server_name api.verboplus.com.br;

    location / {
        proxy_pass http://localhost:7000;  # ou a porta da sua API
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        # IMPORTANTE: preservar Origin e outros headers para CORS
        proxy_set_header Origin $http_origin;
        proxy_pass_request_headers on;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Verificações

1. **Headers preservados**: `proxy_pass_request_headers on` (geralmente já é o padrão) garante que o header `Origin` chegue na API.
2. **HTTPS**: Se usar HTTPS no nginx, inclua o bloco `listen 443 ssl` e os certificados.
3. **Firewall**: Portas 80/443 abertas na VPS.

## Debug

Se o CORS continuar falhando, confira no navegador (F12 > Network):

- **Request URL**: deve ser `https://api.verboplus.com.br/api/...`
- **Origin**: deve ser a URL do cliente (ex.: admin `https://app.verboplus.com.br`; Portal continua em `https://portal.kingdombr.com.br`)
- **Access-Control-Allow-Origin** na resposta: deve ter a mesma origem do Portal
