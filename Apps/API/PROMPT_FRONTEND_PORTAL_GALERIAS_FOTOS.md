# Prompt para Frontend Portal: Galerias de Fotos

## 📋 Visão Geral

Este documento contém todas as informações necessárias para implementar a visualização de galerias de fotos no frontend Portal (público). O Portal permite que visitantes visualizem galerias de fotos ativas, naveguem pelas fotos e vejam imagens em tamanho real.

---

## 🔗 Base URL da API

```
http://localhost:5000/api
```

**⚠️ IMPORTANTE:** Todos os endpoints listados aqui são **PÚBLICOS** e **NÃO requerem autenticação**.

---

## 📚 Endpoints Disponíveis

### 1. Listar Galerias Ativas

```
GET /api/galeriasFotos/ativas
```

**Descrição:** Retorna apenas as galerias que estão marcadas como ativas (publicadas).

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

**Uso:** Exibir lista de galerias na página principal de galerias.

---

### 2. Buscar Galeria por ID

```
GET /api/galeriasFotos/{id}
```

**Descrição:** Retorna os detalhes de uma galeria específica.

**Resposta:**
```json
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
```

**Uso:** Exibir detalhes da galeria na página de visualização.

---

### 3. Buscar Galerias por Evento

```
GET /api/galeriasFotos/evento/{eventoId}
```

**Descrição:** Retorna todas as galerias relacionadas a um evento específico.

**Resposta:** Array de `GaleriaFotoDto` (mesmo formato do endpoint anterior).

**Uso:** Exibir galerias relacionadas a um evento na página de detalhes do evento.

---

### 4. Buscar Galerias por Categoria

```
GET /api/galeriasFotos/categoria/{categoriaMidiaId}
```

**Descrição:** Retorna todas as galerias de uma categoria específica.

**Resposta:** Array de `GaleriaFotoDto`.

**Uso:** Filtrar galerias por categoria.

---

### 5. Listar Fotos de uma Galeria

```
GET /api/galeriasFotos/{id}/fotos
```

**Descrição:** Retorna lista de todas as fotos de uma galeria específica.

**Resposta:**
```json
[
  {
    "nomeArquivo": "foto1.jpg",
    "destaque": true
  },
  {
    "nomeArquivo": "foto2.jpg",
    "destaque": false
  },
  {
    "nomeArquivo": "foto3.jpg",
    "destaque": false
  }
]
```

**Uso:** Exibir grid de thumbnails na página de visualização da galeria.

---

### 6. Listar Categorias de Mídia

```
GET /api/categoriasMidias
```

**Descrição:** Retorna todas as categorias disponíveis (para filtros).

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

**Uso:** Criar filtros por categoria na página de galerias.

---

## 🖼️ Como Acessar as Imagens

### Base URL para Imagens
```
http://localhost:5000
```

### Imagem de Destaque (Thumbnail)
```
http://localhost:5000/{imagemDestaque}
```

**Exemplo:**
```
http://localhost:5000/uploads/fotos/abc123-def456/thumbnail/foto1.jpg
```

### Thumbnail de uma Foto Específica
```
http://localhost:5000/{caminhoDiretorio}/thumbnail/{nomeArquivo}
```

**Exemplo:**
```
http://localhost:5000/uploads/fotos/abc123-def456/thumbnail/foto2.jpg
```

### Foto Original (Tamanho Completo)
```
http://localhost:5000/{caminhoDiretorio}/original/{nomeArquivo}
```

**Exemplo:**
```
http://localhost:5000/uploads/fotos/abc123-def456/original/foto2.jpg
```

---

## 📊 Estruturas de Dados TypeScript

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
```

### Foto
```typescript
interface Foto {
  nomeArquivo: string;
  destaque: boolean;
}
```

### CategoriaMidia
```typescript
interface CategoriaMidia {
  id: number;
  nome: string;
  descricao: string | null;
  dataCriacao: string; // ISO 8601
}
```

---

## 🎯 Funcionalidades Detalhadas

### 1. Página de Listagem de Galerias

#### Layout Sugerido
- Grid de cards (3-4 colunas em desktop, 2 em tablet, 1 em mobile)
- Cada card mostra:
  - Thumbnail da imagem de destaque
  - Nome da galeria
  - Data do evento
  - Quantidade de fotos
  - Categoria (se houver)
  - Evento relacionado (se houver)

#### Filtros Sugeridos
- Por categoria (dropdown)
- Por evento (se houver página de eventos)
- Busca por nome (opcional)

#### Ordenação
- Por data (mais recente primeiro) - já vem ordenado da API

---

### 2. Página de Visualização da Galeria

#### Layout Sugerido
- Header com informações da galeria:
  - Nome
  - Descrição
  - Data
  - Categoria
  - Evento relacionado
- Grid de thumbnails (4-6 colunas)
- Cada thumbnail clicável abre modal/lightbox com foto original

#### Funcionalidades
- Grid responsivo de thumbnails
- Lightbox/Modal para visualizar foto original
- Navegação entre fotos no lightbox (anterior/próxima)
- Botão para fechar lightbox
- Indicador visual da foto de destaque (opcional)

---

### 3. Lightbox/Modal de Visualização

#### Funcionalidades
- Exibir foto original em tamanho completo
- Botões de navegação (anterior/próxima)
- Botão de fechar (X ou ESC)
- Indicador de posição (ex: "3 de 25")
- Zoom (opcional)
- Download (opcional)

---

## 🎨 Sugestões de UI/UX

### Página de Listagem

```
┌─────────────────────────────────────────────────────────┐
│  Galerias de Fotos                                      │
├─────────────────────────────────────────────────────────┤
│  [Filtro: Categoria ▼] [Buscar...]                     │
├─────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │ [Thumb]  │  │ [Thumb]  │  │ [Thumb]  │              │
│  │ Nome     │  │ Nome     │  │ Nome     │              │
│  │ 15/01/25 │  │ 10/01/25 │  │ 05/01/25 │              │
│  │ 25 fotos │  │ 10 fotos │  │ 8 fotos  │              │
│  └──────────┘  └──────────┘  └──────────┘              │
└─────────────────────────────────────────────────────────┘
```

### Página de Galeria

```
┌─────────────────────────────────────────────────────────┐
│  Retiro de Jovens 2025                                 │
│  15 de Janeiro de 2025 • 25 fotos • Eventos            │
│  Fotos do retiro realizado em janeiro                 │
├─────────────────────────────────────────────────────────┤
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐                          │
│  │[⭐]│ │    │ │    │ │    │                          │
│  │Foto│ │Foto│ │Foto│ │Foto│                          │
│  └────┘ └────┘ └────┘ └────┘                          │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐                          │
│  │    │ │    │ │    │ │    │                          │
│  │Foto│ │Foto│ │Foto│ │Foto│                          │
│  └────┘ └────┘ └────┘ └────┘                          │
└─────────────────────────────────────────────────────────┘
```

### Lightbox

```
┌─────────────────────────────────────────────────────────┐
│  [X]                                    [<] Foto [>]    │
│                                                          │
│                    [Foto Original]                      │
│                    (Tamanho Completo)                    │
│                                                          │
│                    Foto 3 de 25                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxos de Uso

### Fluxo 1: Visualizar Galerias
1. Usuário acessa página "Galerias de Fotos"
2. Sistema busca galerias ativas: `GET /api/galeriasFotos/ativas`
3. Exibe grid de cards com thumbnails
4. Usuário pode filtrar por categoria
5. Usuário clica em uma galeria

### Fluxo 2: Visualizar Fotos de uma Galeria
1. Usuário clica em uma galeria
2. Sistema busca detalhes: `GET /api/galeriasFotos/{id}`
3. Sistema busca lista de fotos: `GET /api/galeriasFotos/{id}/fotos`
4. Exibe grid de thumbnails
5. Usuário clica em uma foto

### Fluxo 3: Visualizar Foto em Tamanho Real
1. Usuário clica em thumbnail
2. Lightbox abre com foto original
3. Usuário pode navegar (anterior/próxima)
4. Usuário fecha lightbox

---

## 📝 Exemplos de Código

### Buscar Galerias Ativas
```typescript
async function buscarGaleriasAtivas(): Promise<GaleriaFoto[]> {
  const response = await fetch('http://localhost:5000/api/galeriasFotos/ativas');
  
  if (!response.ok) {
    throw new Error('Erro ao buscar galerias');
  }
  
  return await response.json();
}
```

### Buscar Fotos de uma Galeria
```typescript
async function buscarFotos(galeriaId: number): Promise<Foto[]> {
  const response = await fetch(
    `http://localhost:5000/api/galeriasFotos/${galeriaId}/fotos`
  );
  
  if (!response.ok) {
    throw new Error('Erro ao buscar fotos');
  }
  
  return await response.json();
}
```

### Construir URL da Imagem
```typescript
const API_BASE_URL = 'http://localhost:5000';

function getThumbnailUrl(galeria: GaleriaFoto, foto: Foto): string {
  return `${API_BASE_URL}/${galeria.caminhoDiretorio}/thumbnail/${foto.nomeArquivo}`;
}

function getOriginalUrl(galeria: GaleriaFoto, foto: Foto): string {
  return `${API_BASE_URL}/${galeria.caminhoDiretorio}/original/${foto.nomeArquivo}`;
}

function getDestaqueUrl(galeria: GaleriaFoto): string | null {
  if (!galeria.imagemDestaque) return null;
  return `${API_BASE_URL}/${galeria.imagemDestaque}`;
}
```

### Exemplo de Componente React (Listagem)
```typescript
import { useEffect, useState } from 'react';

function GaleriasPage() {
  const [galerias, setGalerias] = useState<GaleriaFoto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function carregarGalerias() {
      try {
        const response = await fetch('http://localhost:5000/api/galeriasFotos/ativas');
        const data = await response.json();
        setGalerias(data);
      } catch (error) {
        console.error('Erro ao carregar galerias:', error);
      } finally {
        setLoading(false);
      }
    }

    carregarGalerias();
  }, []);

  if (loading) return <div>Carregando...</div>;

  return (
    <div className="galerias-grid">
      {galerias.map(galeria => (
        <div key={galeria.id} className="galeria-card">
          {galeria.imagemDestaque && (
            <img 
              src={`http://localhost:5000/${galeria.imagemDestaque}`}
              alt={galeria.nome}
            />
          )}
          <h3>{galeria.nome}</h3>
          <p>{new Date(galeria.data).toLocaleDateString('pt-BR')}</p>
          <p>{galeria.quantidadeFotos} fotos</p>
        </div>
      ))}
    </div>
  );
}
```

### Exemplo de Componente React (Visualização)
```typescript
import { useEffect, useState } from 'react';

function GaleriaDetalhesPage({ galeriaId }: { galeriaId: number }) {
  const [galeria, setGaleria] = useState<GaleriaFoto | null>(null);
  const [fotos, setFotos] = useState<Foto[]>([]);
  const [fotoSelecionada, setFotoSelecionada] = useState<number | null>(null);

  useEffect(() => {
    async function carregarDados() {
      // Buscar detalhes da galeria
      const galeriaResponse = await fetch(
        `http://localhost:5000/api/galeriasFotos/${galeriaId}`
      );
      const galeriaData = await galeriaResponse.json();
      setGaleria(galeriaData);

      // Buscar fotos
      const fotosResponse = await fetch(
        `http://localhost:5000/api/galeriasFotos/${galeriaId}/fotos`
      );
      const fotosData = await fotosResponse.json();
      setFotos(fotosData);
    }

    carregarDados();
  }, [galeriaId]);

  if (!galeria) return <div>Carregando...</div>;

  return (
    <div>
      <h1>{galeria.nome}</h1>
      <p>{galeria.descricao}</p>
      
      <div className="fotos-grid">
        {fotos.map((foto, index) => (
          <div 
            key={foto.nomeArquivo}
            className="foto-thumbnail"
            onClick={() => setFotoSelecionada(index)}
          >
            <img 
              src={`http://localhost:5000/${galeria.caminhoDiretorio}/thumbnail/${foto.nomeArquivo}`}
              alt={foto.nomeArquivo}
            />
            {foto.destaque && <span className="badge-destaque">⭐</span>}
          </div>
        ))}
      </div>

      {/* Lightbox */}
      {fotoSelecionada !== null && (
        <Lightbox
          foto={fotos[fotoSelecionada]}
          galeria={galeria}
          totalFotos={fotos.length}
          fotoAtual={fotoSelecionada}
          onClose={() => setFotoSelecionada(null)}
          onAnterior={() => setFotoSelecionada(
            fotoSelecionada > 0 ? fotoSelecionada - 1 : fotos.length - 1
          )}
          onProxima={() => setFotoSelecionada(
            fotoSelecionada < fotos.length - 1 ? fotoSelecionada + 1 : 0
          )}
        />
      )}
    </div>
  );
}
```

### Exemplo de Lightbox
```typescript
function Lightbox({
  foto,
  galeria,
  totalFotos,
  fotoAtual,
  onClose,
  onAnterior,
  onProxima
}: {
  foto: Foto;
  galeria: GaleriaFoto;
  totalFotos: number;
  fotoAtual: number;
  onClose: () => void;
  onAnterior: () => void;
  onProxima: () => void;
}) {
  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') onAnterior();
      if (e.key === 'ArrowRight') onProxima();
    };

    window.addEventListener('keydown', handleKeyPress);
    return () => window.removeEventListener('keydown', handleKeyPress);
  }, [onClose, onAnterior, onProxima]);

  return (
    <div className="lightbox-overlay" onClick={onClose}>
      <div className="lightbox-content" onClick={(e) => e.stopPropagation()}>
        <button className="lightbox-close" onClick={onClose}>×</button>
        <button className="lightbox-prev" onClick={onAnterior}>‹</button>
        <img 
          src={`http://localhost:5000/${galeria.caminhoDiretorio}/original/${foto.nomeArquivo}`}
          alt={foto.nomeArquivo}
        />
        <button className="lightbox-next" onClick={onProxima}>›</button>
        <div className="lightbox-counter">
          {fotoAtual + 1} de {totalFotos}
        </div>
      </div>
    </div>
  );
}
```

---

## ⚠️ Tratamento de Erros

### Erro 404 - Galeria não encontrada
```typescript
if (response.status === 404) {
  // Redirecionar para página 404 ou listagem
  navigate('/galerias');
}
```

### Erro de rede
```typescript
try {
  const response = await fetch(url);
  // ...
} catch (error) {
  console.error('Erro de rede:', error);
  // Mostrar mensagem ao usuário
  alert('Erro ao carregar dados. Tente novamente.');
}
```

### Galeria sem fotos
```typescript
if (fotos.length === 0) {
  return <div>Esta galeria ainda não possui fotos.</div>;
}
```

---

## 🎯 Checklist de Implementação

### Página de Listagem
- [ ] Buscar galerias ativas
- [ ] Exibir grid de cards
- [ ] Mostrar thumbnail de destaque
- [ ] Mostrar informações (nome, data, quantidade)
- [ ] Filtro por categoria
- [ ] Busca por nome (opcional)
- [ ] Responsividade (mobile, tablet, desktop)
- [ ] Loading state
- [ ] Tratamento de erros
- [ ] Link para página de detalhes

### Página de Visualização
- [ ] Buscar detalhes da galeria
- [ ] Buscar lista de fotos
- [ ] Exibir informações da galeria
- [ ] Grid de thumbnails
- [ ] Indicador de foto de destaque
- [ ] Abrir lightbox ao clicar
- [ ] Responsividade
- [ ] Loading state
- [ ] Tratamento de erros

### Lightbox/Modal
- [ ] Exibir foto original
- [ ] Navegação (anterior/próxima)
- [ ] Fechar (botão X e ESC)
- [ ] Indicador de posição
- [ ] Navegação por teclado (setas)
- [ ] Prevenir scroll do body quando aberto
- [ ] Animações suaves

### Funcionalidades Extras (Opcional)
- [ ] Zoom na foto
- [ ] Download da foto
- [ ] Compartilhar galeria
- [ ] Lazy loading de imagens
- [ ] Infinite scroll (se muitas galerias)

---

## 📌 Observações Importantes

1. **Todas as APIs são públicas** - Não é necessário autenticação
2. **Apenas galerias ativas** - Use `/api/galeriasFotos/ativas` para listagem
3. **Thumbnails são 400x400px** - Use para grid/listagem
4. **Fotos originais** - Use para lightbox/visualização completa
5. **Ordenação** - As galerias já vêm ordenadas por data (mais recente primeiro)
6. **Fotos ordenadas** - As fotos vêm ordenadas por nome do arquivo
7. **Foto de destaque** - Campo `destaque: true` na lista de fotos indica qual é a foto de destaque

---

## 🔗 Relacionamento com Eventos

Se o Portal tiver página de eventos, você pode:

1. Na página de detalhes do evento, buscar galerias relacionadas:
   ```
   GET /api/galeriasFotos/evento/{eventoId}
   ```

2. Exibir cards de galerias na página do evento

3. Linkar para a página de visualização da galeria

---

## 📱 Responsividade

### Desktop (> 1024px)
- Grid de 3-4 colunas para galerias
- Grid de 4-6 colunas para fotos

### Tablet (768px - 1024px)
- Grid de 2-3 colunas para galerias
- Grid de 3-4 colunas para fotos

### Mobile (< 768px)
- Grid de 1 coluna para galerias
- Grid de 2 colunas para fotos
- Lightbox em tela cheia

---

## 🎨 Sugestões de Estilo

### Cards de Galeria
- Border radius: 8-12px
- Box shadow sutil
- Hover effect (scale ou shadow)
- Imagem com object-fit: cover

### Thumbnails
- Border radius: 4-8px
- Hover effect (opacity ou scale)
- Cursor pointer
- Indicador visual de destaque (badge ou borda)

### Lightbox
- Overlay escuro (rgba(0,0,0,0.9))
- Foto centralizada
- Botões de navegação grandes e visíveis
- Animações suaves (fade in/out)

---

**Última atualização:** 2025-01-28




