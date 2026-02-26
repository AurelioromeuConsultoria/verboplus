import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Download } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
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
import { visitantesApi } from '@/lib/api';
import { toast } from 'sonner';

export function VisitantesList() {
  const [visitantes, setVisitantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    nome: '',
    email: '',
    telefone: '',
    whatsApp: '',
    dataVisita_from: '',
    dataVisita_to: '',
  });
  const confirmDialog = useConfirmDialog();

  const loadVisitantes = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await visitantesApi.getAll();
      setVisitantes(response.data || []);
    } catch (err) {
      setError('Erro ao carregar visitantes');
      console.error('Erro ao carregar visitantes:', err);
      toast.error('Erro ao carregar visitantes');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    const visitante = visitantes.find(v => v.id === id);
    const pessoaNome = visitante?.nome || 'esta visita';
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
  }, []);

  // Filtrar visitantes com busca avançada
  const visitantesFiltradosRaw = visitantes.filter((visitante) => {
    // Busca por nome
    if (filters.nome && !visitante.nome?.toLowerCase().includes(filters.nome.toLowerCase())) {
      return false;
    }

    // Busca por email
    if (filters.email && !visitante.email?.toLowerCase().includes(filters.email.toLowerCase())) {
      return false;
    }

    // Busca por telefone
    if (filters.telefone && !visitante.telefone?.includes(filters.telefone)) {
      return false;
    }

    // Busca por WhatsApp
    if (filters.whatsApp && !visitante.whatsApp?.includes(filters.whatsApp)) {
      return false;
    }

    // Filtro por data de visita
    const dataVisita = new Date(visitante.dataVisita);
    if (filters.dataVisita_from) {
      const dataFrom = new Date(filters.dataVisita_from + 'T00:00:00');
      if (dataVisita < dataFrom) return false;
    }
    if (filters.dataVisita_to) {
      const dataTo = new Date(filters.dataVisita_to + 'T23:59:59');
      if (dataVisita > dataTo) return false;
    }

    return true;
  });

  // Ordenação - precisa ordenar por propriedades aninhadas
  const visitantesFiltradosComNome = visitantesFiltradosRaw.map(v => ({
    ...v,
    nome: v.nome || '',
    email: v.email || '',
    telefone: v.telefone || '',
    whatsApp: v.whatsApp || '',
  }));

  const { sortedData: visitantesFiltrados, sortConfig, handleSort } = useTableSort(visitantesFiltradosComNome, {
    defaultSort: 'dataVisita',
    defaultDirection: 'desc',
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(visitantesFiltrados, 20);

  // Exportação
  const handleExport = () => {
    const exportData = visitantesFiltrados.map(v => ({
      Nome: v.nome || '',
      Email: v.email || '',
      Telefone: v.telefone || '',
      WhatsApp: v.whatsApp || '',
      'Data da Visita': v.dataVisita ? new Date(v.dataVisita).toLocaleDateString('pt-BR') : '',
      Observações: v.observacoes || '',
    }));

    exportToCSV(exportData, 'visitantes');
    toast.success('Dados exportados com sucesso!');
  };

  // Reset page when filters change
  useEffect(() => {
    setPage(1);
  }, [filters, setPage]);

  if (loading) {
    return <LoadingPage text="Carregando visitantes..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadVisitantes} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Visitantes</h1>
          <p className="text-muted-foreground">
            Histórico de visitas
          </p>
        </div>
        <Button asChild>
          <Link to="/visitantes/novo">
            <Plus className="h-4 w-4 mr-2" />
            Novo Visitante
          </Link>
        </Button>
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
            <CardTitle>Lista de Visitas ({total})</CardTitle>
            {visitantesFiltrados.length > 0 && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 mr-2" />
                Exportar CSV
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {visitantesFiltrados.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                {visitantes.length === 0 
                  ? 'Nenhuma visita cadastrada ainda.'
                  : 'Nenhuma visita encontrada com os filtros aplicados.'}
              </p>
              {visitantes.length === 0 && (
                <Button asChild>
                  <Link to="/visitantes/novo">
                    <Plus className="h-4 w-4 mr-2" />
                    Cadastrar Primeira Visita
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
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
                {paginatedItems.map((visitante) => {
                  const contato = visitante.email || visitante.whatsApp || visitante.telefone || '-';
                  const perfisAtivos = visitante.perfis || [];
                  
                  return (
                    <TableRow key={visitante.id}>
                      <TableCell>
                        {new Date(visitante.dataVisita).toLocaleDateString('pt-BR')}
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
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/visitantes/${visitante.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDelete(visitante.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
          {visitantesFiltrados.length > 0 && (
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

