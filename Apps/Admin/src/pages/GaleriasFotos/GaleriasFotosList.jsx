import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Image, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { galeriasFotosApi, eventosApi, categoriasMidiasApi } from '@/lib/api';

const API_BASE_URL = 'http://localhost:5000';

export default function GaleriasFotosList() {
  const [items, setItems] = useState([]);
  const [eventos, setEventos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [eventoFilter, setEventoFilter] = useState('');
  const [categoriaFilter, setCategoriaFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [galeriasRes, eventosRes, categoriasRes] = await Promise.all([
        galeriasFotosApi.getAll(),
        eventosApi.getAll(),
        categoriasMidiasApi.getAll(),
      ]);
      setItems(galeriasRes.data || []);
      setEventos(eventosRes.data || []);
      setCategorias(categoriasRes.data || []);
    } catch (err) {
      setError('Erro ao carregar galerias de fotos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('⚠️ ATENÇÃO: Esta ação irá deletar a galeria e TODAS as fotos. Esta ação não pode ser desfeita.\n\nTem certeza que deseja continuar?')) return;
    try {
      await galeriasFotosApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir galeria');
      console.error(err);
    }
  };

  const filtered = items.filter((g) => {
    if (busca && !g.nome?.toLowerCase().includes(busca.toLowerCase()) && !g.descricao?.toLowerCase().includes(busca.toLowerCase())) return false;
    if (eventoFilter && String(g.eventoId) !== eventoFilter) return false;
    if (categoriaFilter && String(g.categoriaMidiaId) !== categoriaFilter) return false;
    if (statusFilter !== '' && String(g.ativo) !== statusFilter) return false;
    return true;
  });

  const getImagemUrl = (caminho) => {
    if (!caminho) return null;
    // Normalizar o caminho (remover barra inicial se existir para evitar duplicação)
    const caminhoNormalizado = caminho.startsWith('/') ? caminho.substring(1) : caminho;
    return `${API_BASE_URL}/${caminhoNormalizado}`;
  };

  if (loading) return <LoadingPage text="Carregando galerias..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Galerias de Fotos</h1>
          <p className="text-muted-foreground">Gerencie as galerias de fotos da igreja</p>
        </div>
        <Button asChild>
          <Link to="/galerias-fotos/novo">
            <Plus className="h-4 w-4 mr-2" /> Nova Galeria
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Nome ou descrição"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Evento</label>
              <select className="w-full px-3 py-2 border rounded" value={eventoFilter} onChange={(e) => setEventoFilter(e.target.value)}>
                <option value="">Todos</option>
                {eventos.map((e) => (
                  <option key={e.id} value={e.id}>{e.titulo}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Categoria</label>
              <select className="w-full px-3 py-2 border rounded" value={categoriaFilter} onChange={(e) => setCategoriaFilter(e.target.value)}>
                <option value="">Todas</option>
                {categorias.map((c) => (
                  <option key={c.id} value={c.id}>{c.nome}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Status</label>
              <select className="w-full px-3 py-2 border rounded" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">Todos</option>
                <option value="true">Ativo</option>
                <option value="false">Inativo</option>
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      {filtered.length === 0 ? (
        <Card>
          <CardContent className="py-8">
            <div className="text-center text-muted-foreground">Nenhuma galeria encontrada.</div>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {filtered.map((galeria) => {
            const imagemUrl = getImagemUrl(galeria.imagemDestaque);
            return (
              <Card key={galeria.id} className="overflow-hidden">
                <div className="relative h-48 bg-gray-100">
                  {imagemUrl ? (
                    <img
                      src={imagemUrl}
                      alt={galeria.nome}
                      className="w-full h-full object-cover"
                      onError={(e) => {
                        e.target.style.display = 'none';
                        e.target.nextSibling.style.display = 'flex';
                      }}
                    />
                  ) : null}
                  <div className={`absolute inset-0 flex items-center justify-center ${imagemUrl ? 'hidden' : ''}`}>
                    <Image className="h-16 w-16 text-gray-400" />
                  </div>
                  {!galeria.ativo && (
                    <div className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded text-xs font-medium">
                      Inativo
                    </div>
                  )}
                </div>
                <CardContent className="p-4">
                  <h3 className="font-semibold text-lg mb-2">{galeria.nome}</h3>
                  <p className="text-sm text-muted-foreground mb-3 line-clamp-2">{galeria.descricao || 'Sem descrição'}</p>
                  
                  <div className="space-y-2 mb-4 text-sm">
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <span>{galeria.data ? new Date(galeria.data).toLocaleDateString('pt-BR') : '-'}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Image className="h-4 w-4 text-muted-foreground" />
                      <span>{galeria.quantidadeFotos || 0} foto(s)</span>
                    </div>
                    {galeria.eventoTitulo && (
                      <div className="text-muted-foreground">Evento: {galeria.eventoTitulo}</div>
                    )}
                    {galeria.categoriaMidiaNome && (
                      <div className="text-muted-foreground">Categoria: {galeria.categoriaMidiaNome}</div>
                    )}
                  </div>

                  <div className="flex items-center space-x-2">
                    <Button variant="outline" size="sm" asChild className="flex-1">
                      <Link to={`/galerias-fotos/${galeria.id}/fotos`}>
                        <Image className="h-4 w-4 mr-2" /> Ver Fotos
                      </Link>
                    </Button>
                    <Button variant="ghost" size="sm" asChild>
                      <Link to={`/galerias-fotos/${galeria.id}/editar`}>
                        <Edit className="h-4 w-4" />
                      </Link>
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => handleDelete(galeria.id)}>
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}

