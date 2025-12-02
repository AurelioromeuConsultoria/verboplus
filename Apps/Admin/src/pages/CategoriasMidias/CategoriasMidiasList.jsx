import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { categoriasMidiasApi } from '@/lib/api';

export default function CategoriasMidiasList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await categoriasMidiasApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar categorias de mídia');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta categoria de mídia?')) return;
    try {
      await categoriasMidiasApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir. Pode haver galerias usando esta categoria.');
      console.error(err);
    }
  };

  const filtered = items.filter((c) => {
    if (busca && !c.nome?.toLowerCase().includes(busca.toLowerCase()) && !c.descricao?.toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando categorias de mídia..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Categorias de Mídia</h1>
          <p className="text-muted-foreground">Gerencie as categorias para organizar as galerias</p>
        </div>
        <Button asChild>
          <Link to="/categorias-midias/novo">
            <Plus className="h-4 w-4 mr-2" /> Nova Categoria
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
                placeholder="Nome ou descrição"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Categorias de Mídia</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma categoria encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Data de Criação</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((categoria) => (
                  <TableRow key={categoria.id}>
                    <TableCell className="font-medium">{categoria.nome}</TableCell>
                    <TableCell>{categoria.descricao || '-'}</TableCell>
                    <TableCell>{categoria.dataCriacao ? new Date(categoria.dataCriacao).toLocaleDateString('pt-BR') : '-'}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/categorias-midias/${categoria.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(categoria.id)}>
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

