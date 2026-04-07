import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Phone, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { voluntariosApi, equipesApi, cargosApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

export default function VoluntariosList() {
  const [items, setItems] = useState([]);
  const [equipes, setEquipes] = useState([]);
  const [cargos, setCargos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [equipeId, setEquipeId] = useState('');
  const [cargoId, setCargoId] = useState('');
  const confirmDialog = useConfirmDialog();
  const { can, isAdmin } = useAuth();
  const { t } = useTranslation();

  const load = async (options = {}) => {
    const silent = options.silent ?? false;
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);
      const [v, e, c] = await Promise.all([
        voluntariosApi.getAll(),
        equipesApi.getAll(),
        cargosApi.getAll(),
      ]);
      setItems(v.data || []);
      setEquipes(e.data || []);
      setCargos(c.data || []);
    } catch (err) {
      setError('Erro ao carregar voluntários');
      console.error(err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const voluntario = items.find(v => v.id === id);
    confirmDialog.show({
      title: 'Excluir Voluntário',
      description: `Tem certeza que deseja excluir "${voluntario?.nome || 'este voluntário'}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await voluntariosApi.delete(id);
          toast.success('Voluntário excluído com sucesso');
          await load();
        } catch (err) {
          toast.error('Erro ao excluir voluntário');
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filtered = items.filter((v) => {
    if (busca && !v.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    if (equipeId && String(v.equipeId) !== String(equipeId)) return false;
    if (cargoId && String(v.cargoId) !== String(cargoId)) return false;
    return true;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  if (loading) return <LoadingPage text="Carregando voluntários..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = isAdmin && can(RESOURCES.VOLUNTARIOS, ACTIONS.EDIT);
  const canDelete = isAdmin && can(RESOURCES.VOLUNTARIOS, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-3xl font-bold">{t('volunteer.volunteers.title')}</h1>
          <p className="text-muted-foreground">{t('volunteer.volunteers.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          {canEdit && (
            <Button asChild>
              <Link to="/voluntarios/novo">
                <Plus className="h-4 w-4 mr-2" /> {t('volunteer.volunteers.new')}
              </Link>
            </Button>
          )}
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Search className="h-4 w-4" />Buscar por nome</label>
              <Input
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o nome"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Equipe</label>
              <Select value={equipeId || 'all'} onValueChange={(value) => setEquipeId(value === 'all' ? '' : value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Todas as equipes" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas as equipes</SelectItem>
                  {equipes.map((e) => (
                    <SelectItem key={e.id} value={String(e.id)}>{e.nome}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Cargo</label>
              <Select value={cargoId || 'all'} onValueChange={(value) => setCargoId(value === 'all' ? '' : value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os cargos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os cargos</SelectItem>
                  {cargos.map((c) => (
                    <SelectItem key={c.id} value={String(c.id)}>{c.nome}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('volunteer.volunteers.listTitle')} ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <PageEmptyState
              title="Nenhum voluntario encontrado"
              description="Nao ha voluntarios para os filtros atuais. Ajuste equipe, cargo ou busca para ampliar a lista."
              action={canEdit ? (
                <Button asChild>
                  <Link to="/voluntarios/novo">
                    <Plus className="mr-2 h-4 w-4" />
                    {t('volunteer.volunteers.new')}
                  </Link>
                </Button>
              ) : null}
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>WhatsApp</TableHead>
                  <TableHead>Equipe</TableHead>
                  <TableHead>Cargo</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((vol) => (
                  <TableRow key={vol.id}>
                    <TableCell className="font-medium">{vol.nome}</TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{vol.whatsApp}</span>
                        {vol.whatsApp && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`https://wa.me/55${String(vol.whatsApp).replace(/\D/g, '')}`)}
                          >
                            <Phone className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{vol.nomeEquipe}</TableCell>
                    <TableCell>{vol.nomeCargo}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/voluntarios/${vol.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(vol.id)}>
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
