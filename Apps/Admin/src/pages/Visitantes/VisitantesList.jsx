import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Download } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { SortableTableHeader } from '@/components/ui/sortable-table-header';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { exportToCSV } from '@/utils/export';
import { visitantesApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { useTranslation } from 'react-i18next';

export default function VisitantesList() {
  const [visitantes, setVisitantes] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    nome: '',
    email: '',
    telefone: '',
    whatsApp: '',
    dataVisita_from: '',
    dataVisita_to: '',
  });
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [sortConfig, setSortConfig] = useState({ field: 'dataVisita', direction: 'desc' });
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [bulkDeleteDialogOpen, setBulkDeleteDialogOpen] = useState(false);
  const [bulkDeleting, setBulkDeleting] = useState(false);
  const confirmDialog = useConfirmDialog();
  const { isAdmin } = useAuth();
  const { t } = useTranslation();

  const loadVisitantes = useCallback(async (options = {}) => {
    const silent = options.silent ?? false;
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);
      const response = await visitantesApi.getPaged({
        page,
        pageSize,
        sort: sortConfig.field,
        direction: sortConfig.direction,
        nome: filters.nome || undefined,
        email: filters.email || undefined,
        telefone: filters.telefone || undefined,
        whatsApp: filters.whatsApp || undefined,
        dataVisitaFrom: filters.dataVisita_from || undefined,
        dataVisitaTo: filters.dataVisita_to || undefined,
      });

      const data = response.data || {};
      setVisitantes(data.items || []);
      setTotal(Number(data.total || 0));
    } catch (err) {
      setError('Erro ao carregar visitantes');
      console.error('Erro ao carregar visitantes:', err);
      toast.error('Erro ao carregar visitantes');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [filters, page, pageSize, sortConfig.direction, sortConfig.field]);

  const handleDelete = async (id) => {
    const visitante = visitantes.find(v => v.id === id);
    const pessoaNome = visitante?.nome || 'esta visita';
    const currentPageCount = visitantes.length;
    confirmDialog.show({
      title: 'Excluir Visita',
      description: `Tem certeza que deseja excluir a visita de "${pessoaNome}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await visitantesApi.delete(id);
          toast.success('Visita excluída com sucesso');
          await loadVisitantes();
          if (page > 1 && currentPageCount === 1) {
            setPage((p) => Math.max(1, p - 1));
          }
        } catch (err) {
          toast.error('Erro ao excluir visita');
          console.error('Erro ao excluir visita:', err);
          throw err;
        }
      },
    });
  };

  useEffect(() => {
    loadVisitantes();
  }, [loadVisitantes]);

  useEffect(() => {
    setSelectedIds(new Set());
  }, [page, filters]);

  const pageIds = visitantes.map((v) => v.id);
  const allPageSelected = pageIds.length > 0 && pageIds.every((id) => selectedIds.has(id));

  const toggleSelectAll = () => {
    if (allPageSelected) {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        pageIds.forEach((id) => next.delete(id));
        return next;
      });
    } else {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        pageIds.forEach((id) => next.add(id));
        return next;
      });
    }
  };

  const toggleSelect = (id) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleBulkDeleteClick = () => {
    if (selectedIds.size === 0) return;
    setBulkDeleteDialogOpen(true);
  };

  const handleBulkDeleteConfirm = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;

    try {
      setBulkDeleting(true);
      let ok = 0;
      let fail = 0;
      for (const id of ids) {
        try {
          await visitantesApi.delete(id);
          ok += 1;
        } catch {
          fail += 1;
        }
      }
      setSelectedIds(new Set());
      setBulkDeleteDialogOpen(false);
      await loadVisitantes();
      if (page > 1 && visitantes.length === ids.length) setPage((p) => Math.max(1, p - 1));
      if (fail > 0) {
        toast.warning(`${ok} excluída(s), ${fail} falha(s).`);
      } else {
        toast.success(`${ok} visita(s) excluída(s) com sucesso`);
      }
    } catch {
      toast.error('Erro ao excluir em lote');
    } finally {
      setBulkDeleting(false);
    }
  };

  const handleSort = (field) => {
    setSortConfig((prev) => {
      if (prev.field === field) {
        return { field, direction: prev.direction === 'asc' ? 'desc' : 'asc' };
      }
      return { field, direction: 'asc' };
    });
    setPage(1);
  };

  // Exportação
  const handleExport = async () => {
    try {
      const all = [];
      let p = 1;
      let totalItems = Infinity;
      const exportPageSize = 200;

      while (all.length < totalItems) {
        const resp = await visitantesApi.getPaged({
          page: p,
          pageSize: exportPageSize,
          sort: sortConfig.field,
          direction: sortConfig.direction,
          nome: filters.nome || undefined,
          email: filters.email || undefined,
          telefone: filters.telefone || undefined,
          whatsApp: filters.whatsApp || undefined,
          dataVisitaFrom: filters.dataVisita_from || undefined,
          dataVisitaTo: filters.dataVisita_to || undefined,
        });

        const data = resp.data || {};
        const items = data.items || [];
        totalItems = Number(data.total || 0);
        all.push(...items);
        if (items.length === 0) break;
        p += 1;
        if (p > 200) break;
      }

      const exportData = all.map(v => ({
        Nome: v.nome || '',
        Email: v.email || '',
        Telefone: v.telefone || '',
        WhatsApp: v.whatsApp || '',
        'Data da Visita': v.dataVisita ? new Date(v.dataVisita).toLocaleDateString('pt-BR') : '',
        Observações: v.observacoes || '',
      }));

      exportToCSV(exportData, 'visitantes');
      toast.success('Dados exportados com sucesso!');
    } catch (err) {
      console.error('Erro ao exportar visitantes:', err);
      toast.error('Erro ao exportar dados');
    }
  };

  // Reset page when filters change
  useEffect(() => {
    setPage(1);
  }, [filters]);

  if (loading) {
    return <LoadingPage text="Carregando visitantes..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadVisitantes} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-3xl font-bold">{t('visitors.title')}</h1>
          <p className="text-muted-foreground">
            {t('visitors.subtitle')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => loadVisitantes({ silent: true })} refreshing={refreshing} />
          {isAdmin && (
            <Button asChild>
              <Link to="/visitantes/novo">
                <Plus className="h-4 w-4 mr-2" />
                {t('visitors.new')}
              </Link>
            </Button>
          )}
        </div>
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'nome', label: 'Nome', type: 'text', placeholder: 'Buscar por nome...' },
          { key: 'email', label: 'Email', type: 'text', placeholder: 'Buscar por email...' },
          { key: 'telefone', label: 'Telefone', type: 'text', placeholder: 'Buscar por telefone...' },
          { key: 'whatsApp', label: 'WhatsApp', type: 'text', placeholder: 'Buscar por WhatsApp...' },
        ]}
        filterFields={[
          {
            key: 'dataVisita',
            label: 'Data da Visita',
            type: 'date-range',
          },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() => {
          setFilters({
            nome: '',
            email: '',
            telefone: '',
            whatsApp: '',
            dataVisita_from: '',
            dataVisita_to: '',
          });
        }}
      />

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>{t('visitors.listTitle')} ({total})</CardTitle>
            {total > 0 && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 mr-2" />
                Exportar CSV
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {selectedIds.size > 0 && (
            <div className="flex items-center justify-between rounded-md border bg-muted/50 px-4 py-2 mb-4">
              <span className="text-sm font-medium">
                {selectedIds.size} selecionada(s)
              </span>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" onClick={() => setSelectedIds(new Set())}>
                  Limpar seleção
                </Button>
                <Button variant="destructive" size="sm" onClick={handleBulkDeleteClick}>
                  <Trash2 className="h-4 w-4 mr-2" />
                  Excluir selecionadas
                </Button>
              </div>
            </div>
          )}
          {visitantes.length === 0 ? (
            <PageEmptyState
              title="Nenhum visitante encontrado"
              description={total === 0 ? t('visitors.emptyMessage') : t('visitors.emptyPageMessage')}
              action={total === 0 && isAdmin ? (
                <Button asChild>
                  <Link to="/visitantes/novo">
                    <Plus className="h-4 w-4 mr-2" />
                    {t('visitors.emptyCta')}
                  </Link>
                </Button>
              ) : null}
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={allPageSelected}
                      onCheckedChange={toggleSelectAll}
                      aria-label="Selecionar todas"
                    />
                  </TableHead>
                  <SortableTableHeader field="dataVisita" onSort={handleSort} sortConfig={sortConfig}>
                    Data da Visita
                  </SortableTableHeader>
                  <SortableTableHeader field="nome" onSort={handleSort} sortConfig={sortConfig}>
                    Pessoa
                  </SortableTableHeader>
                  <TableHead>Contato</TableHead>
                  <TableHead>Observações</TableHead>
                  <TableHead>Perfis</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {visitantes.map((visitante) => {
                  const contato = visitante.email || visitante.whatsApp || visitante.telefone || '-';
                  const perfisAtivos = visitante.perfis || [];
                  
                  return (
                    <TableRow key={visitante.id}>
                      <TableCell>
                        <Checkbox
                          checked={selectedIds.has(visitante.id)}
                          onCheckedChange={() => toggleSelect(visitante.id)}
                          aria-label={`Selecionar ${visitante.nome || 'visita'}`}
                        />
                      </TableCell>
                      <TableCell>
                        {visitante.dataVisita ? new Date(visitante.dataVisita).toLocaleDateString('pt-BR') : '-'}
                      </TableCell>
                      <TableCell className="font-medium">
                        {visitante.nome || '-'}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <span className="text-sm">{contato}</span>
                          {visitante.email && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => window.open(`mailto:${visitante.email}`)}
                            >
                              <Mail className="h-4 w-4" />
                            </Button>
                          )}
                          {visitante.whatsApp && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => window.open(`https://wa.me/55${visitante.whatsApp.replace(/\D/g, '')}`)}
                            >
                              <Phone className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="text-sm text-muted-foreground">
                          {visitante.observacoes 
                            ? (visitante.observacoes.length > 50 
                                ? visitante.observacoes.substring(0, 50) + '...'
                                : visitante.observacoes)
                            : '-'}
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {perfisAtivos.length > 0 ? (
                            perfisAtivos.map((perfil, idx) => (
                              <Badge key={idx} variant="secondary" className="text-xs">
                                {perfil}
                              </Badge>
                            ))
                          ) : (
                            <span className="text-muted-foreground text-sm">-</span>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end space-x-2">
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/visitantes/${visitante.id}`}>
                              <Eye className="h-4 w-4" />
                            </Link>
                          </Button>
                          {isAdmin && (
                            <Button variant="ghost" size="sm" asChild>
                              <Link to={`/visitantes/${visitante.id}/editar`}>
                                <Edit className="h-4 w-4" />
                              </Link>
                            </Button>
                          )}
                          {isAdmin && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDelete(visitante.id)}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
          {total > 0 && (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              total={total}
              onPageChange={setPage}
              onPageSizeChange={(newSize) => {
                setPageSize(newSize);
                setPage(1);
              }}
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

      <ConfirmDialog
        open={bulkDeleteDialogOpen}
        onOpenChange={setBulkDeleteDialogOpen}
        onConfirm={handleBulkDeleteConfirm}
        title="Excluir em lote"
        description={`Tem certeza que deseja excluir ${selectedIds.size} visita(s) selecionada(s)? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText={t('actions.cancel')}
        variant="destructive"
        loading={bulkDeleting}
      />
    </div>
  );
}
