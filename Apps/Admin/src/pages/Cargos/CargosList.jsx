import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { cargosApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

export default function CargosList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const confirmDialog = useConfirmDialog();
  const { can } = useAuth();
  const { t } = useTranslation();

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await cargosApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar cargos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const cargo = items.find(c => c.id === id);
    confirmDialog.show({
      title: 'Excluir Cargo',
      description: `Tem certeza que deseja excluir "${cargo?.nome || 'este cargo'}"? Esta ação não pode ser desfeita. Se houver voluntários vinculados, a exclusão será bloqueada.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await cargosApi.delete(id);
          toast.success('Cargo excluído com sucesso');
          await load();
        } catch (err) {
          const errorMsg = err.response?.data?.message || 'Erro ao excluir cargo. Pode haver voluntários vinculados.';
          toast.error(errorMsg);
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filtered = items.filter((c) => {
    if (busca && !c.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  if (loading) return <LoadingPage text="Carregando cargos..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = can(RESOURCES.CARGOS, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.CARGOS, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('volunteer.roles.title')}</h1>
          <p className="text-muted-foreground">{t('volunteer.roles.subtitle')}</p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/cargos/novo">
              <Plus className="h-4 w-4 mr-2" /> {t('volunteer.roles.new')}
            </Link>
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Search className="h-4 w-4" />Buscar por nome</label>
              <Input
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o nome do cargo"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('volunteer.roles.listTitle')} ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum cargo encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Data de Criação</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((cargo) => (
                  <TableRow key={cargo.id}>
                    <TableCell className="font-medium">{cargo.nome}</TableCell>
                    <TableCell>{new Date(cargo.dataCriacao).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/cargos/${cargo.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(cargo.id)}>
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
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

