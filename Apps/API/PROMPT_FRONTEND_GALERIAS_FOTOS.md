# Prompt para Frontend: Gerenciamento de Galerias de Fotos

## 📋 Visão Geral

Este documento contém todas as informações necessárias para implementar as telas de gerenciamento de galerias de fotos no frontend Admin. O sistema permite criar galerias, fazer upload de múltiplas fotos, organizar por categorias e relacionar com eventos.

---

## 🔗 Base URL da API

```
http://localhost:5000/api
```

---

## 📚 Endpoints Disponíveis

### Categorias de Mídia

#### Listar todas as categorias
```
GET /api/categoriasMidias
```

**Resposta:**
```json
[
  {
    "id": 1,
    "nome": "Eventos",
    "descricao": "Fotos de eventos da igreja",
    "dataCriacao": "2025-01-28T10:00:00"
  }
]
```

#### Buscar categoria por ID
```
GET /api/categoriasMidias/{id}
```

#### Criar categoria
```
POST /api/categoriasMidias
Content-Type: application/json

{
  "nome": "Eventos",
  "descricao": "Fotos de eventos da igreja"
}
```

#### Atualizar categoria
```
PUT /api/categoriasMidias/{id}
Content-Type: application/json

{
  "nome": "Eventos Atualizado",
  "descricao": "Nova descrição"
}
```

#### Deletar categoria
```
DELETE /api/categoriasMidias/{id}
```

---

### Galerias de Fotos

#### Listar todas as galerias
```
GET /api/galeriasFotos
```

**Resposta:**
```json
[
  {
    "id": 1,
    "nome": "Retiro de Jovens 2025",
    "descricao": "Fotos do retiro realizado em janeiro",
    "data": "2025-01-15T00:00:00",
    "caminhoDiretorio": "uploads/fotos/abc123-def456",
    "imagemDestaque": "uploads/fotos/abc123-def456/thumbnail/foto1.jpg",
    "quantidadeFotos": 25,
    "ativo": true,
    "eventoId": 5,
    "eventoTitulo": "Retiro de Jovens",
    "categoriaMidiaId": 1,
    "categoriaMidiaNome": "Eventos",
    "dataCriacao": "2025-01-28T10:00:00"
  }
]
```

#### Listar apenas galerias ativas (para Portal)
```
GET /api/galeriasFotos/ativas
```

#### Buscar galeria por ID
```
GET /api/galeriasFotos/{id}
```

#### Buscar galerias por evento
```
GET /api/galeriasFotos/evento/{eventoId}
```

#### Buscar galerias por categoria
```
GET /api/galeriasFotos/categoria/{categoriaMidiaId}
```

#### Criar galeria
```
POST /api/galeriasFotos
Content-Type: application/json

{
  "nome": "Retiro de Jovens 2025",
  "descricao": "Fotos do retiro realizado em janeiro",
  "data": "2025-01-15T00:00:00",
  "eventoId": 5,  // Opcional
  "categoriaMidiaId": 1,  // Opcional
  "ativo": true
}
```

**Resposta:**
```json
{
  "id": 1,
  "nome": "Retiro de Jovens 2025",
  "descricao": "Fotos do retiro realizado em janeiro",
  "data": "2025-01-15T00:00:00",
  "caminhoDiretorio": "uploads/fotos/abc123-def456",
  "imagemDestaque": null,
  "quantidadeFotos": 0,
  "ativo": true,
  "eventoId": 5,
  "eventoTitulo": "Retiro de Jovens",
  "categoriaMidiaId": 1,
  "categoriaMidiaNome": "Eventos",
  "dataCriacao": "2025-01-28T10:00:00"
}
```

#### Atualizar galeria
```
PUT /api/galeriasFotos/{id}
Content-Type: application/json

{
  "nome": "Retiro de Jovens 2025 - Atualizado",
  "descricao": "Nova descrição",
  "data": "2025-01-15T00:00:00",
  "eventoId": 5,
  "categoriaMidiaId": 1,
  "ativo": true
}
```

#### Deletar galeria
```
DELETE /api/galeriasFotos/{id}
```

**⚠️ IMPORTANTE:** Deletar uma galeria também remove TODAS as fotos do servidor (diretório completo).

#### Upload de fotos
```
POST /api/galeriasFotos/{id}/upload
Content-Type: multipart/form-data

FormData:
  arquivos: [File, File, File, ...]  // Múltiplos arquivos
```

**Validações:**
- Formatos aceitos: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- Tamanho máximo por arquivo: 10MB
- Múltiplos arquivos podem ser enviados de uma vez

**Resposta:**
```json
{
  "message": "Fotos enviadas com sucesso"
}
```

**Comportamento:**
- Cada foto é salva em `original/` (tamanho completo)
- Um thumbnail de 400x400px é gerado automaticamente em `thumbnail/`
- A primeira foto enviada vira automaticamente a imagem de destaque (usa thumbnail)

#### Definir imagem de destaque
```
PUT /api/galeriasFotos/{id}/destaque
Content-Type: application/json

"foto1.jpg"  // Nome do arquivo (string simples)
```

**Resposta:**
```json
{
  "message": "Imagem de destaque definida com sucesso"
}
```

---

## 📁 Estrutura de Diretórios

Quando uma galeria é criada, a seguinte estrutura é criada no servidor:

```
wwwroot/
  uploads/
    fotos/
      {galeriaId}/          // Ex: abc123-def456
        original/
          foto1.jpg         // Foto original completa
          foto2.jpg
          foto3.jpg
        thumbnail/
          foto1.jpg         // Thumbnail 400x400px
          foto2.jpg
          foto3.jpg
```

---

## 🖼️ Como Acessar as Imagens

### Imagem de Destaque (Thumbnail)
```
http://localhost:5000/{imagemDestaque}
```

Exemplo:
```
http://localhost:5000/uploads/fotos/abc123-def456/thumbnail/foto1.jpg
```

### Foto Original
```
http://localhost:5000/{caminhoDiretorio}/original/{nomeArquivo}
```

Exemplo:
```
http://localhost:5000/uploads/fotos/abc123-def456/original/foto1.jpg
```

### Thumbnail Específico
```
http://localhost:5000/{caminhoDiretorio}/thumbnail/{nomeArquivo}
```

---

## 📊 Estruturas de Dados TypeScript

### CategoriaMidia
```typescript
interface CategoriaMidia {
  id: number;
  nome: string;
  descricao: string | null;
  dataCriacao: string; // ISO 8601
}

interface CriarCategoriaMidiaDto {
  nome: string;
  descricao?: string;
}

interface AtualizarCategoriaMidiaDto {
  nome: string;
  descricao?: string;
}
```

### GaleriaFoto
```typescript
interface GaleriaFoto {
  id: number;
  nome: string;
  descricao: string | null;
  data: string; // ISO 8601
  caminhoDiretorio: string;
  imagemDestaque: string | null; // Caminho relativo para o thumbnail
  quantidadeFotos: number;
  ativo: boolean;
  eventoId: number | null;
  eventoTitulo: string | null;
  categoriaMidiaId: number | null;
  categoriaMidiaNome: string | null;
  dataCriacao: string; // ISO 8601
}

interface CriarGaleriaFotoDto {
  nome: string;
  descricao?: string;
  data: string; // ISO 8601
  eventoId?: number;
  categoriaMidiaId?: number;
  ativo?: boolean;
}

interface AtualizarGaleriaFotoDto {
  nome: string;
  descricao?: string;
  data: string; // ISO 8601
  eventoId?: number | null;
  categoriaMidiaId?: number | null;
  ativo: boolean;
}
```

---

## 🎯 Funcionalidades Detalhadas

### 1. Gerenciamento de Categorias

#### Listar Categorias
- Exibir todas as categorias em uma tabela
- Campos: Nome, Descrição, Data de Criação
- Ações: Editar, Deletar

#### Criar/Editar Categoria
- Formulário com campos:
  - Nome (obrigatório, máximo 100 caracteres)
  - Descrição (opcional, máximo 500 caracteres)
- Validação: Nome não pode estar vazio

#### Deletar Categoria
- Confirmar antes de deletar
- ⚠️ Verificar se há galerias usando a categoria (backend não impede, mas é bom avisar)

---

### 2. Gerenciamento de Galerias

#### Listar Galerias
- Exibir galerias em cards ou tabela
- Mostrar:
  - Imagem de destaque (thumbnail)
  - Nome da galeria
  - Data
  - Quantidade de fotos
  - Categoria (se houver)
  - Evento relacionado (se houver)
  - Status (Ativo/Inativo)
- Ordenação: Por data (mais recente primeiro)
- Filtros sugeridos:
  - Por categoria
  - Por evento
  - Apenas ativas
  - Apenas inativas

#### Criar Galeria
- Formulário com campos:
  - Nome (obrigatório)
  - Descrição (opcional)
  - Data (date picker)
  - Evento (select opcional - buscar de `/api/eventos`)
  - Categoria (select opcional - buscar de `/api/categoriasMidias`)
  - Ativo (checkbox, padrão: true)
- Após criar, redirecionar para tela de upload de fotos

#### Editar Galeria
- Mesmo formulário de criação, pré-preenchido
- Permitir alterar todos os campos exceto `caminhoDiretorio`

#### Deletar Galeria
- ⚠️ **ATENÇÃO:** Confirmar com mensagem clara:
  - "Esta ação irá deletar a galeria e TODAS as fotos. Esta ação não pode ser desfeita."
- Confirmar antes de deletar

---

### 3. Upload de Fotos

#### Tela de Upload
- Exibir informações da galeria (nome, data, etc.)
- Área de drag & drop ou botão para selecionar arquivos
- Suportar múltiplos arquivos
- Validações no frontend:
  - Formatos: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
  - Tamanho máximo: 10MB por arquivo
- Preview das fotos antes de enviar (opcional)
- Barra de progresso durante upload
- Após upload bem-sucedido:
  - Mostrar mensagem de sucesso
  - Atualizar contador de fotos
  - Exibir thumbnails das fotos enviadas

#### Listar Fotos da Galeria
- Grid de thumbnails (ex: 4-6 colunas)
- Cada thumbnail:
  - Imagem (400x400px)
  - Botão para ver original
  - Botão para definir como destaque (se não for a atual)
  - Indicador visual se for a imagem de destaque
- Ordenação: Por nome do arquivo ou data de upload

#### Visualizar Foto Original
- Modal ou nova página
- Exibir foto original em tamanho completo
- Botão para fechar/voltar
- Opção para definir como destaque

#### Definir Imagem de Destaque
- Botão em cada thumbnail: "Definir como destaque"
- Confirmar ação
- Atualizar visualmente após sucesso

---

## 🎨 Sugestões de UI/UX

### Tela de Listagem de Galerias
```
┌─────────────────────────────────────────────────────────┐
│  Galerias de Fotos                    [+ Nova Galeria] │
├─────────────────────────────────────────────────────────┤
│  [Filtros: Categoria ▼] [Evento ▼] [Ativas ▼]          │
├─────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │ [Thumb]  │  │ [Thumb]  │  │ [Thumb]  │              │
│  │ Nome     │  │ Nome     │  │ Nome     │              │
│  │ 25 fotos │  │ 10 fotos │  │ 8 fotos  │              │
│  │ [Editar] │  │ [Editar] │  │ [Editar] │              │
│  └──────────┘  └──────────┘  └──────────┘              │
└─────────────────────────────────────────────────────────┘
```

### Tela de Upload
```
┌─────────────────────────────────────────────────────────┐
│  Upload de Fotos - Retiro de Jovens 2025                │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌─────────────────────────────────────────────┐        │
│  │                                             │        │
│  │      📁 Arraste fotos aqui ou clique        │        │
│  │                                             │        │
│  │      Formatos: JPG, PNG, GIF, WEBP          │        │
│  │      Máximo: 10MB por arquivo              │        │
│  │                                             │        │
│  └─────────────────────────────────────────────┘        │
│                                                           │
│  [Selecionar Arquivos]                                   │
│                                                           │
│  Fotos selecionadas: 3                                   │
│  [Enviar Fotos]                                          │
└─────────────────────────────────────────────────────────┘
```

### Grid de Fotos
```
┌─────────────────────────────────────────────────────────┐
│  Fotos da Galeria - Retiro de Jovens 2025               │
├─────────────────────────────────────────────────────────┤
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐                          │
│  │[⭐]│ │    │ │    │ │    │                          │
│  │Foto│ │Foto│ │Foto│ │Foto│                          │
│  │[👁]│ │[👁]│ │[👁]│ │[👁]│                          │
│  └────┘ └────┘ └────┘ └────┘                          │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐                          │
│  │    │ │    │ │    │ │    │                          │
│  │Foto│ │Foto│ │Foto│ │Foto│                          │
│  │[👁]│ │[👁]│ │[👁]│ │[👁]│                          │
│  └────┘ └────┘ └────┘ └────┘                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxos de Uso

### Fluxo 1: Criar Nova Galeria e Fazer Upload
1. Usuário clica em "Nova Galeria"
2. Preenche formulário (nome, data, categoria, evento)
3. Salva galeria
4. Redireciona para tela de upload
5. Seleciona múltiplas fotos
6. Faz upload
7. Visualiza thumbnails das fotos enviadas
8. Pode definir imagem de destaque

### Fluxo 2: Editar Galeria Existente
1. Usuário clica em "Editar" na listagem
2. Formulário pré-preenchido é exibido
3. Altera campos desejados
4. Salva alterações
5. Retorna para listagem

### Fluxo 3: Adicionar Fotos a Galeria Existente
1. Usuário clica em galeria na listagem
2. Vê grid de fotos existentes
3. Clica em "Adicionar Fotos"
4. Seleciona novas fotos
5. Faz upload
6. Grid é atualizado com novas fotos

### Fluxo 4: Visualizar Foto em Tamanho Real
1. Usuário clica em thumbnail
2. Modal abre com foto original
3. Pode definir como destaque
4. Fecha modal

---

## ⚠️ Tratamento de Erros

### Erros Comuns

#### Upload falhou
```json
{
  "message": "Nenhum arquivo enviado"
}
```
**Ação:** Mostrar mensagem de erro, permitir tentar novamente

#### Arquivo muito grande
- Validação no frontend antes de enviar
- Se passar, backend retorna erro
- Mostrar: "Arquivo excede 10MB. Por favor, escolha outro arquivo."

#### Formato inválido
- Validação no frontend
- Mostrar: "Formato não suportado. Use JPG, PNG, GIF ou WEBP."

#### Galeria não encontrada
```json
404 Not Found
```
**Ação:** Redirecionar para listagem com mensagem de erro

---

## 📝 Exemplos de Código

### Upload de Fotos (JavaScript/TypeScript)
```typescript
async function uploadFotos(galeriaId: number, arquivos: File[]): Promise<void> {
  const formData = new FormData();
  
  arquivos.forEach(arquivo => {
    formData.append('arquivos', arquivo);
  });

  const response = await fetch(
    `http://localhost:5000/api/galeriasFotos/${galeriaId}/upload`,
    {
      method: 'POST',
      body: formData,
      headers: {
        // NÃO adicionar Content-Type - o browser define automaticamente para FormData
        'Authorization': `Bearer ${token}` // Se necessário
      }
    }
  );

  if (!response.ok) {
    throw new Error('Erro ao fazer upload');
  }

  const data = await response.json();
  console.log(data.message); // "Fotos enviadas com sucesso"
}
```

### Construir URL da Imagem
```typescript
function getImagemUrl(caminho: string): string {
  const baseUrl = 'http://localhost:5000';
  return `${baseUrl}/${caminho}`;
}

// Exemplo de uso
const thumbnailUrl = getImagemUrl(galeria.imagemDestaque);
// Resultado: http://localhost:5000/uploads/fotos/abc123/thumbnail/foto1.jpg
```

### Definir Imagem de Destaque
```typescript
async function definirDestaque(galeriaId: number, nomeArquivo: string): Promise<void> {
  const response = await fetch(
    `http://localhost:5000/api/galeriasFotos/${galeriaId}/destaque`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}` // Se necessário
      },
      body: JSON.stringify(nomeArquivo) // String simples, não objeto
    }
  );

  if (!response.ok) {
    throw new Error('Erro ao definir destaque');
  }
}
```

---

## 🎯 Checklist de Implementação

### Categorias de Mídia
- [ ] Listar categorias
- [ ] Criar categoria
- [ ] Editar categoria
- [ ] Deletar categoria
- [ ] Validações de formulário

### Galerias de Fotos
- [ ] Listar galerias (cards/tabela)
- [ ] Criar galeria
- [ ] Editar galeria
- [ ] Deletar galeria (com confirmação)
- [ ] Filtros (categoria, evento, status)
- [ ] Buscar eventos para select
- [ ] Buscar categorias para select

### Upload e Gerenciamento de Fotos
- [ ] Tela de upload (drag & drop)
- [ ] Validação de arquivos no frontend
- [ ] Preview antes de enviar
- [ ] Barra de progresso
- [ ] Grid de thumbnails
- [ ] Visualizar foto original (modal)
- [ ] Definir imagem de destaque
- [ ] Indicador visual de destaque
- [ ] Contador de fotos

### UI/UX
- [ ] Design responsivo
- [ ] Loading states
- [ ] Mensagens de erro
- [ ] Mensagens de sucesso
- [ ] Confirmações para ações destrutivas

---

## 🔐 Autenticação

**Nota:** Atualmente os endpoints de galerias de fotos NÃO estão protegidos com `[Authorize]`. Se você quiser proteger no futuro, será necessário:

1. Fazer login primeiro: `POST /api/auth/login`
2. Obter token JWT
3. Incluir token no header: `Authorization: Bearer {token}`

---

## 📌 Observações Importantes

1. **Thumbnails são gerados automaticamente** - Não é necessário fazer nada especial, apenas fazer upload das fotos
2. **Primeira foto vira destaque automaticamente** - Mas pode ser alterada depois
3. **Deletar galeria remove todas as fotos** - Confirmar sempre antes de deletar
4. **Caminhos são relativos** - Sempre prefixar com `http://localhost:5000/` para acessar as imagens
5. **Upload suporta múltiplos arquivos** - Enviar todos de uma vez é mais eficiente
6. **Thumbnails são 400x400px** - Mantém proporção original (não corta)

---

**Última atualização:** 2025-01-28




