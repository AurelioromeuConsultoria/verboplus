import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { formatDateBr } from '@/lib/formatters';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { categoriasMidiasApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

export default function CategoriasMidiasList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const confirmDialog = useConfirmDialog();

  const load = async ({ silent = false } = {}) => {
    try {
      if (silent) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      if (!silent) {
        setError(null);
      }
      const res = await categoriasMidiasApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar categorias de mídia');
      console.error(err);
    } finally {
      if (silent) {
        setRefreshing(false);
      } else {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const categoria = items.find((c) => c.id === id);
    confirmDialog.show({
      title: 'Excluir categoria de mídia?',
      description: `Tem certeza que deseja excluir "${categoria?.nome || 'esta categoria'}"?`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await categoriasMidiasApi.delete(id);
          toast.success('Categoria excluída com sucesso');
          await load();
        } catch (err) {
          toast.error(
            getApiErrorMessage(err, 'Erro ao excluir. Pode haver galerias usando esta categoria.')
          );
          console.error(err);
          throw err;
        }
      },
    });
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
          <div className="flex items-center justify-between gap-3">
            <CardTitle>Lista de Categorias de Mídia</CardTitle>
            <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          </div>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <PageEmptyState
              title="Nenhuma categoria encontrada."
              description="Ajuste a busca ou crie uma nova categoria de mídia."
              action={(
                <Button asChild>
                  <Link to="/categorias-midias/novo">
                    <Plus className="h-4 w-4 mr-2" /> Nova Categoria
                  </Link>
                </Button>
              )}
            />
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
                    <TableCell>{formatDateBr(categoria.dataCriacao)}</TableCell>
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

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={(open) => {
          if (!open) confirmDialog.hide();
        }}
        onConfirm={confirmDialog.handleConfirm}
        title={confirmDialog.config.title}
        description={confirmDialog.config.description}
        confirmText={confirmDialog.config.confirmText}
        cancelText={confirmDialog.config.cancelText}
        variant={confirmDialog.config.variant}
        loading={confirmDialog.loading}
      />
    </div>
  );
}



