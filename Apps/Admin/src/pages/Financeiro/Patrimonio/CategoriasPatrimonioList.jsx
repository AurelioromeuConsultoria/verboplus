import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Search } from 'lucide-react';
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
import { categoriasPatrimonioApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

export default function CategoriasPatrimonioList() {
  const { t } = useTranslation();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const confirmDialog = useConfirmDialog();
  const { can } = useAuth();

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await categoriasPatrimonioApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError(t('finance.patrimonyCategories.errorLoad'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const categoria = items.find((item) => item.id === id);
    confirmDialog.show({
      title: t('finance.patrimonyCategories.deleteTitle'),
      description: t('finance.patrimonyCategories.deleteDescription', { name: categoria?.nome || t('finance.patrimonyCategories.emptyMessage') }),
      confirmText: t('finance.expenses.delete.confirm'),
      cancelText: t('actions.cancel'),
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await categoriasPatrimonioApi.delete(id);
          toast.success(t('finance.patrimonyCategories.deleteSuccess'));
          await load();
        } catch (err) {
          toast.error(err.response?.data?.message || t('finance.patrimonyCategories.deleteError'));
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filtered = items.filter((item) => !busca || String(item.nome || '').toLowerCase().includes(busca.toLowerCase()));
  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  if (loading) return <LoadingPage text={t('finance.patrimonyCategories.loading')} />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = can(RESOURCES.FINANCEIRO, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.FINANCEIRO, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('finance.patrimonyCategories.title')}</h1>
          <p className="text-muted-foreground">{t('finance.patrimonyCategories.subtitle')}</p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/financeiro/patrimonio/categorias/nova">
              <Plus className="h-4 w-4 mr-2" /> {t('finance.patrimonyCategories.new')}
            </Link>
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t('finance.common.filters')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <label className="text-sm font-medium flex items-center gap-2">
              <Search className="h-4 w-4" />
              {t('finance.common.searchByName')}
            </label>
            <Input value={busca} onChange={(e) => setBusca(e.target.value)} placeholder={t('finance.common.searchByName')} />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('finance.patrimonyCategories.listTitle')} ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">{t('finance.patrimonyCategories.emptyMessage')}</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('finance.common.name')}</TableHead>
                  <TableHead>{t('finance.common.description')}</TableHead>
                  <TableHead>{t('finance.common.status')}</TableHead>
                  <TableHead className="text-right">{t('finance.expenses.table.actions')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.nome}</TableCell>
                    <TableCell>{item.descricao || '-'}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs ${item.ativo ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                        {item.ativo ? t('finance.patrimonyCategories.statusActive') : t('finance.patrimonyCategories.statusInactive')}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/financeiro/patrimonio/categorias/${item.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(item.id)}>
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
