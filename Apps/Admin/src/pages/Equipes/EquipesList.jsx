import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { equipesApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

const AREA_LABEL = {
  1: 'Verde',
  2: 'Vermelha',
  3: 'Laranja',
};

export default function EquipesList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [area, setArea] = useState('');
  const confirmDialog = useConfirmDialog();
  const { can } = useAuth();
  const { t } = useTranslation();

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await equipesApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar equipes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const equipe = items.find(e => e.id === id);
    confirmDialog.show({
      title: 'Excluir Equipe',
      description: `Tem certeza que deseja excluir "${equipe?.nome || 'esta equipe'}"? Esta ação não pode ser desfeita. Se houver voluntários vinculados, a exclusão será bloqueada.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await equipesApi.delete(id);
          toast.success('Equipe excluída com sucesso');
          await load();
        } catch (err) {
          const errorMsg = err.response?.data?.message || 'Erro ao excluir equipe. Pode haver voluntários vinculados.';
          toast.error(errorMsg);
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filtered = items.filter((e) => {
    if (busca && !e.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    if (area && String(e.area) !== String(area)) return false;
    return true;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  if (loading) return <LoadingPage text="Carregando equipes..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = can(RESOURCES.EQUIPES, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.EQUIPES, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{t('volunteer.teams.title')}</h1>
          <p className="text-muted-foreground">{t('volunteer.teams.subtitle')}</p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/equipes/novo">
              <Plus className="h-4 w-4 mr-2" /> {t('volunteer.teams.new')}
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
                placeholder="Digite o nome da equipe"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Área</label>
              <Select value={area || 'all'} onValueChange={(value) => setArea(value === 'all' ? '' : value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Todas as áreas" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas as áreas</SelectItem>
                  <SelectItem value="1">Verde</SelectItem>
                  <SelectItem value="2">Vermelha</SelectItem>
                  <SelectItem value="3">Laranja</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('volunteer.teams.listTitle')} ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma equipe encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Área</TableHead>
                  <TableHead>Data de Criação</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((equipe) => (
                  <TableRow key={equipe.id}>
                    <TableCell className="font-medium">{equipe.nome}</TableCell>
                    <TableCell>{AREA_LABEL[equipe.area] || equipe.area}</TableCell>
                    <TableCell>{new Date(equipe.dataCriacao).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/equipes/${equipe.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(equipe.id)}>
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

