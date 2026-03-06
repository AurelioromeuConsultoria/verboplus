import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Users, Download } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { SortableTableHeader } from '@/components/ui/sortable-table-header';
import { useTableSort } from '@/hooks/useTableSort';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { exportToCSV } from '@/utils/export';
import { eventosApi, normalizeEvento } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';

export default function EventosList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    titulo: '',
    descricao: '',
    dataInicio_from: '',
    dataInicio_to: '',
  });
  const confirmDialog = useConfirmDialog();
  const { can } = useAuth();

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await eventosApi.getAll();
      const raw = res.data || [];
      setItems(Array.isArray(raw) ? raw.map(normalizeEvento) : raw);
    } catch (err) {
      setError('Erro ao carregar eventos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    const evento = items.find(e => e.id === id);
    confirmDialog.show({
      title: 'Excluir Evento',
      description: `Tem certeza que deseja excluir "${evento?.titulo || 'este evento'}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await eventosApi.delete(id);
          toast.success('Evento excluído com sucesso');
          await load();
        } catch (err) {
          toast.error('Erro ao excluir evento');
          console.error(err);
          throw err;
        }
      },
    });
  };

  const filteredRaw = items.filter((e) => {
    // Busca por título
    if (filters.titulo && !e.titulo?.toLowerCase().includes(filters.titulo.toLowerCase())) {
      return false;
    }

    // Busca por descrição
    if (filters.descricao && !e.descricao?.toLowerCase().includes(filters.descricao.toLowerCase())) {
      return false;
    }

    // Filtro por data de início
    if (filters.dataInicio_from) {
      const dataInicio = new Date(e.dataInicio);
      const dataFrom = new Date(filters.dataInicio_from + 'T00:00:00');
      if (dataInicio < dataFrom) return false;
    }

    if (filters.dataInicio_to) {
      const dataInicio = new Date(e.dataInicio);
      const dataTo = new Date(filters.dataInicio_to + 'T23:59:59');
      if (dataInicio > dataTo) return false;
    }

    return true;
  });

  // Ordenação
  const { sortedData: filtered, sortConfig, handleSort } = useTableSort(filteredRaw, {
    defaultSort: 'titulo',
    defaultDirection: 'asc',
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 20);

  // Exibe data formatada ou '-' se vazia ou data default (ex: 0001-01-01)
  const formatEventDate = (value) => {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime()) || d.getFullYear() < 1900) return '-';
    return d.toLocaleString('pt-BR');
  };

  const getTipoLabel = (tipo, tipoDescricao) => {
    if (tipoDescricao) return tipoDescricao;
    const map = { 1: 'Evento', 2: 'Culto', 3: 'Reunião', 4: 'Outro' };
    return map[tipo] ?? 'Evento';
  };

  // Exportação
  const handleExport = () => {
    const exportData = filtered.map(evento => ({
      Título: evento.titulo || '',
      Descrição: evento.descricao || '',
      'Data Início': formatEventDate(evento.dataInicio),
      'Data Fim': formatEventDate(evento.dataFim),
      URL: evento.url || '',
    }));

    exportToCSV(exportData, 'eventos');
    toast.success('Dados exportados com sucesso!');
  };

  if (loading) return <LoadingPage text="Carregando eventos..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const canEdit = can(RESOURCES.EVENTOS, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.EVENTOS, ACTIONS.DELETE);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Eventos</h1>
          <p className="text-muted-foreground">Gerencie os eventos da igreja</p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/eventos/novo">
              <Plus className="h-4 w-4 mr-2" /> Novo Evento
            </Link>
          </Button>
        )}
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'titulo', label: 'Título', type: 'text', placeholder: 'Buscar por título...' },
          { key: 'descricao', label: 'Descrição', type: 'text', placeholder: 'Buscar por descrição...' },
        ]}
        filterFields={[
          {
            key: 'dataInicio',
            label: 'Data de Início',
            type: 'date-range',
          },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() => {
          setFilters({
            titulo: '',
            descricao: '',
            dataInicio_from: '',
            dataInicio_to: '',
          });
        }}
      />

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Lista de Eventos ({total})</CardTitle>
            {filtered.length > 0 && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 mr-2" />
                Exportar CSV
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum evento encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <SortableTableHeader field="titulo" onSort={handleSort} sortConfig={sortConfig}>
                    Título
                  </SortableTableHeader>
                  <TableHead>Tipo</TableHead>
                  <SortableTableHeader field="descricao" onSort={handleSort} sortConfig={sortConfig}>
                    Descrição
                  </SortableTableHeader>
                  <SortableTableHeader field="dataInicio" onSort={handleSort} sortConfig={sortConfig}>
                    Data Início
                  </SortableTableHeader>
                  <SortableTableHeader field="dataFim" onSort={handleSort} sortConfig={sortConfig}>
                    Data Fim
                  </SortableTableHeader>
                  <TableHead>URL</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((evento) => (
                  <TableRow key={evento.id}>
                    <TableCell className="font-medium">{evento.titulo || '-'}</TableCell>
                    <TableCell>{getTipoLabel(evento.tipo, evento.tipoDescricao)}</TableCell>
                    <TableCell>{evento.descricao ? (evento.descricao.length > 50 ? `${evento.descricao.substring(0, 50)}...` : evento.descricao) : '-'}</TableCell>
                    <TableCell>{formatEventDate(evento.dataInicio)}</TableCell>
                    <TableCell>{formatEventDate(evento.dataFim)}</TableCell>
                    <TableCell>
                      {evento.url ? (
                        <a href={evento.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                          {evento.url.length > 30 ? `${evento.url.substring(0, 30)}...` : evento.url}
                        </a>
                      ) : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild title="Ver Inscrições">
                          <Link to={`/eventos/${evento.id}/inscricoes`}>
                            <Users className="h-4 w-4" />
                          </Link>
                        </Button>
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/eventos/${evento.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(evento.id)}>
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

