import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { noticiasApi, categoriasNoticiasApi } from '@/lib/api';

export default function NoticiasList() {
  const [items, setItems] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [categoriaId, setCategoriaId] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [noticiasRes, categoriasRes] = await Promise.all([
        noticiasApi.getAll(),
        categoriasNoticiasApi.getAll(),
      ]);
      setItems(noticiasRes.data || []);
      setCategorias(categoriasRes.data || []);
    } catch (err) {
      setError('Erro ao carregar notícias');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta notícia?')) return;
    try {
      await noticiasApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir notícia');
      console.error(err);
    }
  };

  const getCategoriaNome = (id) => {
    const categoria = categorias.find((c) => c.id === id);
    return categoria ? categoria.nome : '-';
  };

  const filtered = items.filter((n) => {
    if (busca && !n.titulo?.toLowerCase().includes(busca.toLowerCase()) && !n.descricao?.toLowerCase().includes(busca.toLowerCase())) return false;
    if (categoriaId && String(n.categoriaNoticiaId) !== String(categoriaId)) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando notícias..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Notícias</h1>
          <p className="text-muted-foreground">Gerencie as notícias da igreja</p>
        </div>
        <Button asChild>
          <Link to="/noticias/novo">
            <Plus className="h-4 w-4 mr-2" /> Nova Notícia
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o título ou descrição"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Categoria</label>
              <select className="w-full px-3 py-2 border rounded" value={categoriaId} onChange={(e) => setCategoriaId(e.target.value)}>
                <option value="">Todas</option>
                {categorias.map((c) => (
                  <option key={c.id} value={c.id}>{c.nome}</option>
                ))}
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Notícias</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma notícia encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Título</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Categoria</TableHead>
                  <TableHead>Data</TableHead>
                  <TableHead>URL</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((noticia) => (
                  <TableRow key={noticia.id}>
                    <TableCell className="font-medium">{noticia.titulo || '-'}</TableCell>
                    <TableCell>{noticia.descricao ? (noticia.descricao.length > 50 ? `${noticia.descricao.substring(0, 50)}...` : noticia.descricao) : '-'}</TableCell>
                    <TableCell>{getCategoriaNome(noticia.categoriaNoticiaId)}</TableCell>
                    <TableCell>{noticia.data ? new Date(noticia.data).toLocaleDateString('pt-BR') : '-'}</TableCell>
                    <TableCell>
                      {noticia.url ? (
                        <a href={noticia.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                          {noticia.url.length > 30 ? `${noticia.url.substring(0, 30)}...` : noticia.url}
                        </a>
                      ) : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/noticias/${noticia.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(noticia.id)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}


