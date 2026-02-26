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
import { receitasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';

export default function ReceitasList() {
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
      const res = await receitasApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar receitas');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const receita = items.find((r) => r.id === id);
    confirmDialog.show({
      title: 'Excluir Receita',
      description: `Tem certeza que deseja excluir "${receita?.descricao || 'esta receita'}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await receitasApi.delete(id);
          toast.success('Receita excluída com sucesso');
          await load();
        } catch (err) {
          toast.error('Erro ao excluir receita');
          console.error(err);
          throw err;
        }
      },
    });
  };

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('pt-BR');
  };

  const getStatusLabel = (status) => {
    const statusMap = {
      'Pendente': 'Pendente',
      'Recebido': 'Recebido',
      'Cancelado': 'Cancelado',
    };
    return statusMap[status] || status;
  };

  const getStatusColor = (status) => {
    const colorMap = {
      'Pendente': 'bg-yellow-100 text-yellow-800',
      'Recebido': 'bg-green-100 text-green-800',
      'Cancelado': 'bg-gray-100 text-gray-800',
    };
    return colorMap[status] || 'bg-gray-100 text-gray-800';
  };

  const filtered = items.filter((r) => {
    if (busca && !String(r.descricao || '').toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  if (loading) return <LoadingPage text="Carregando receitas..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = can(RESOURCES.FINANCEIRO, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.FINANCEIRO, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Receitas</h1>
          <p className="text-muted-foreground">Gerencie as receitas</p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/financeiro/receitas/novo">
              <Plus className="h-4 w-4 mr-2" /> Nova Receita
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
              <label className="text-sm font-medium flex items-center gap-2"><Search className="h-4 w-4" />Buscar por descrição</label>
              <Input
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite a descrição da receita"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Receitas ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma receita encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Valor</TableHead>
                  <TableHead>Data Recebimento</TableHead>
                  <TableHead>Conta Bancária</TableHead>
                  <TableHead>Centro de Custo</TableHead>
                  <TableHead>Projeto</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="font-medium">{r.descricao}</TableCell>
                    <TableCell>{formatCurrency(r.valor)}</TableCell>
                    <TableCell>{formatDate(r.dataRecebimento)}</TableCell>
                    <TableCell>{r.contaBancaria?.nome || '-'}</TableCell>
                    <TableCell>{r.centroCusto?.nome || '-'}</TableCell>
                    <TableCell>{r.projeto?.nome || '-'}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs ${getStatusColor(r.status)}`}>
                        {getStatusLabel(r.status)}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/financeiro/receitas/${r.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(r.id)}>
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
