import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, BarChart3, Calendar, CheckCircle, XCircle, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { enquetesApi } from '@/lib/api';
import { toast } from 'sonner';

export default function EnquetesList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const confirmDialog = useConfirmDialog();

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await enquetesApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar enquetes');
      console.error(err);
      toast.error('Erro ao carregar enquetes');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const enquete = items.find(e => e.id === id);
    confirmDialog.show({
      title: 'Excluir Enquete',
      description: `Tem certeza que deseja excluir "${enquete?.titulo || 'esta enquete'}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await enquetesApi.delete(id);
          toast.success('Enquete excluída com sucesso');
          await load();
        } catch (err) {
          toast.error('Erro ao excluir enquete');
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filtered = items.filter((e) => {
    if (busca && !e.titulo?.toLowerCase().includes(busca.toLowerCase()) && !e.descricao?.toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('pt-BR');
  };

  const isAtiva = (enquete) => {
    const agora = new Date();
    const inicio = new Date(enquete.dataInicio);
    const fim = new Date(enquete.dataFim);
    return enquete.ativo && inicio <= agora && fim >= agora;
  };

  if (loading) return <LoadingPage text="Carregando enquetes..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Enquetes</h1>
          <p className="text-muted-foreground">Gerencie as enquetes do sistema</p>
        </div>
        <Button asChild>
          <Link to="/enquetes/novo">
            <Plus className="h-4 w-4 mr-2" /> Nova Enquete
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground flex items-center gap-2"><Search className="h-4 w-4" />Buscar</label>
              <Input
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o título ou descrição"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Enquetes ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              Nenhuma enquete encontrada.
              {items.length === 0 && (
                <div className="mt-4">
                  <Button asChild>
                    <Link to="/enquetes/novo">
                      <Plus className="h-4 w-4 mr-2" />
                      Criar Primeira Enquete
                    </Link>
                  </Button>
                </div>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Título</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Período</TableHead>
                  <TableHead>Opções</TableHead>
                  <TableHead>Votos</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((enquete) => (
                  <TableRow key={enquete.id}>
                    <TableCell className="font-medium text-foreground">
                      {enquete.titulo}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {enquete.descricao ? (
                        <span className="line-clamp-2">{enquete.descricao}</span>
                      ) : (
                        '-'
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Calendar className="h-4 w-4" />
                        <span>{formatDate(enquete.dataInicio)} - {formatDate(enquete.dataFim)}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{enquete.opcoes?.length || 0} opções</Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <BarChart3 className="h-4 w-4 text-muted-foreground" />
                        <span className="text-sm text-foreground">{enquete.totalVotos || 0}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      {isAtiva(enquete) ? (
                        <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">
                          <CheckCircle className="h-3 w-3 mr-1" />
                          Ativa
                        </Badge>
                      ) : (
                        <Badge variant="secondary">
                          <XCircle className="h-3 w-3 mr-1" />
                          {enquete.ativo ? 'Aguardando' : 'Inativa'}
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/enquetes/${enquete.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(enquete.id)}
                          className="text-destructive hover:text-destructive"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {filtered.length > 0 && (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              total={total}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
            />
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={confirmDialog.hide}
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
