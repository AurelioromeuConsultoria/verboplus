import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { 
  Calendar, 
  Clock, 
  MessageSquare, 
  User, 
  CheckCircle,
  XCircle,
  AlertCircle,
  Eye
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { mensagensAgendadasApi, visitantesApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const MensagensAgendadas = () => {
  const [mensagens, setMensagens] = useState([]);
  const [visitantes, setVisitantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [autoRefreshSeconds, setAutoRefreshSeconds] = useState(30);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [filters, setFilters] = useState({
    texto: '',
    status: undefined,
    visitanteId: undefined,
    dataEnvio_from: '',
    dataEnvio_to: '',
  });

  // Estados para estatísticas
  const [stats, setStats] = useState({
    total: 0,
    agendadas: 0,
    enviadas: 0,
    erro: 0
  });

  const fetchVisitantes = useCallback(async () => {
    try {
      const visitantesResponse = await visitantesApi.getAll();
      setVisitantes(visitantesResponse.data || []);
    } catch (err) {
      console.warn('Nao foi possivel carregar visitantes para enriquecer a listagem.', err);
      setVisitantes([]);
    }
  }, []);

  const fetchMensagens = useCallback(async ({ showLoader = false } = {}) => {
    try {
      setError(null);
      if (showLoader) setLoading(true);
      else setRefreshing(true);

      const [mensagensResponse, statsResponse] = await Promise.all([
        mensagensAgendadasApi.getPaged({
          page,
          pageSize,
          sort: 'dataEnvio',
          direction: 'desc',
          texto: filters.texto || undefined,
          status: filters.status ? Number(filters.status) : undefined,
          visitanteId: filters.visitanteId ? Number(filters.visitanteId) : undefined,
          dataEnvioFrom: filters.dataEnvio_from || undefined,
          dataEnvioTo: filters.dataEnvio_to || undefined,
        }),
        mensagensAgendadasApi.getStats(),
      ]);

      const data = mensagensResponse.data || {};
      setMensagens(data.items || []);
      setTotal(Number(data.total || 0));

      const s = statsResponse.data || {};
      setStats({
        total: Number(s.total || 0),
        agendadas: Number(s.agendadas || 0),
        enviadas: Number(s.enviadas || 0),
        erro: Number(s.erro || 0),
      });
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar dados');
      setError(msg);
      console.error('Erro ao buscar dados:', err);
      toast.error(msg);
    } finally {
      if (showLoader) setLoading(false);
      setRefreshing(false);
    }
  }, [filters.dataEnvio_from, filters.dataEnvio_to, filters.status, filters.texto, filters.visitanteId, page, pageSize]);

  useEffect(() => {
    fetchVisitantes().catch((err) => {
      console.error('Erro ao carregar visitantes:', err);
    });
  }, [fetchVisitantes]);

  useEffect(() => {
    fetchMensagens({ showLoader: true });
  }, [fetchMensagens]);

  useEffect(() => {
    if (!autoRefreshSeconds || autoRefreshSeconds <= 0) return;

    const intervalMs = autoRefreshSeconds * 1000;
    const id = window.setInterval(() => {
      if (document.visibilityState !== 'visible') return;
      if (loading || refreshing) return;
      fetchMensagens();
    }, intervalMs);

    return () => window.clearInterval(id);
  }, [autoRefreshSeconds, fetchMensagens, loading, refreshing]);

  const visitanteNomeById = useMemo(() => {
    const map = new Map();
    visitantes.forEach((v) => map.set(String(v.id), v.nome));
    return map;
  }, [visitantes]);

  const getStatusText = (status) => {
    switch (Number(status)) {
      case 1: return 'Agendada';
      case 2: return 'Pronta para envio';
      case 3: return 'Enviada';
      case 4: return 'Erro';
      case 5: return 'Cancelada';
      case 6: return 'Em processamento';
      default: return `Status ${status}`;
    }
  };

  const getStatusIcon = (status) => {
    const statusText = getStatusText(status);
    switch (statusText) {
      case 'Agendada':
        return <Clock className="w-4 h-4 text-blue-500 dark:text-blue-400" />;
      case 'Enviada':
        return <CheckCircle className="w-4 h-4 text-green-500 dark:text-green-400" />;
      case 'Erro':
        return <XCircle className="w-4 h-4 text-red-500 dark:text-red-400" />;
      default:
        return <AlertCircle className="w-4 h-4 text-muted-foreground" />;
    }
  };

  const getStatusBadge = (status) => {
    const statusText = getStatusText(status);
    
    switch (statusText) {
      case 'Agendada':
        return <Badge variant="default" className="bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">Agendada</Badge>;
      case 'Pronta para envio':
        return <Badge variant="secondary">Pronta</Badge>;
      case 'Em processamento':
        return <Badge variant="secondary">Processando</Badge>;
      case 'Enviada':
        return <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">Enviada</Badge>;
      case 'Erro':
        return <Badge variant="destructive">Erro</Badge>;
      case 'Cancelada':
        return <Badge variant="outline">Cancelada</Badge>;
      default:
        return <Badge variant="secondary">{statusText}</Badge>;
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return 'Data não definida';
    
    const date = new Date(dateString);
    
    // Verifica se a data é válida
    if (isNaN(date.getTime())) {
      console.warn('Data inválida recebida:', dateString);
      return 'Data inválida';
    }
    
    return date.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getVisitanteNome = (mensagem) => {
    if (mensagem?.nomeVisitante) return mensagem.nomeVisitante;
    const nome = visitanteNomeById.get(String(mensagem.visitanteId));
    return nome || 'Visitante não encontrado';
  };

  // Cancelamento não está implementado na API atualmente.

  useEffect(() => {
    setPage(1);
  }, [filters]);

  if (loading) return <LoadingPage text="Carregando mensagens..." />;
  if (error) return <ErrorPage message={error} onRetry={() => fetchMensagens({ showLoader: true })} />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Mensagens Agendadas</h1>
          <p className="text-muted-foreground mt-1">Acompanhe o status das mensagens automáticas</p>
        </div>

        <div className="flex items-center gap-2">
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground whitespace-nowrap">Auto-atualizar</span>
            <Select value={String(autoRefreshSeconds)} onValueChange={(v) => setAutoRefreshSeconds(Number(v))}>
              <SelectTrigger className="w-[140px]" title="Intervalo de atualização automática">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">Desligado</SelectItem>
                <SelectItem value="10">10s</SelectItem>
                <SelectItem value="30">30s</SelectItem>
                <SelectItem value="60">60s</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <PageRefreshButton onClick={() => fetchMensagens()} refreshing={refreshing} />
        </div>
      </div>

      {/* Cards de Estatísticas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <MessageSquare className="w-8 h-8 text-muted-foreground" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Total</p>
                <p className="text-2xl font-bold text-foreground">{stats.total}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Clock className="w-8 h-8 text-blue-500 dark:text-blue-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Agendadas</p>
                <p className="text-2xl font-bold text-blue-500 dark:text-blue-400">{stats.agendadas}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <CheckCircle className="w-8 h-8 text-green-500 dark:text-green-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Enviadas</p>
                <p className="text-2xl font-bold text-green-500 dark:text-green-400">{stats.enviadas}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <XCircle className="w-8 h-8 text-red-500 dark:text-red-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Com Erro</p>
                <p className="text-2xl font-bold text-red-500 dark:text-red-400">{stats.erro}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'texto', label: 'Mensagem/Visitante', type: 'text', placeholder: 'Buscar por texto ou nome do visitante...' },
        ]}
        filterFields={[
          {
            key: 'status',
            label: 'Status',
            type: 'select',
            options: [
              { value: '1', label: 'Agendada' },
              { value: '2', label: 'Pronta para envio' },
              { value: '6', label: 'Em processamento' },
              { value: '3', label: 'Enviada' },
              { value: '4', label: 'Erro' },
              { value: '5', label: 'Cancelada' },
            ],
          },
          {
            key: 'visitanteId',
            label: 'Visitante',
            type: 'select',
            options: visitantes.map((v) => ({ value: String(v.id), label: v.nome })),
          },
          {
            key: 'dataEnvio',
            label: 'Data de Envio',
            type: 'date-range',
          },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() =>
          setFilters({
            texto: '',
            status: undefined,
            visitanteId: undefined,
            dataEnvio_from: '',
            dataEnvio_to: '',
          })
        }
      />

      {/* Tabela de Mensagens */}
      <Card>
        <CardHeader>
          <CardTitle>
            Mensagens ({total})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {mensagens.length === 0 ? (
            <PageEmptyState
              title={total === 0 ? 'Nenhuma mensagem encontrada' : 'Nenhuma mensagem nesta pagina'}
              description={total === 0
                ? 'As mensagens aparecerao aqui quando houver agendamentos compativeis com os filtros atuais.'
                : 'Nao ha mensagens nesta pagina atual. Tente navegar ou ajustar os filtros.'}
              icon={MessageSquare}
            />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Visitante</TableHead>
                    <TableHead>Mensagem</TableHead>
                    <TableHead>Data/Hora Envio</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mensagens.map((mensagem) => (
                    <TableRow key={mensagem.id}>
                      <TableCell>
                        <div className="flex items-center">
                          <User className="w-4 h-4 text-muted-foreground mr-2" />
                          <div>
                            <div className="text-sm font-medium text-foreground">
                              {getVisitanteNome(mensagem)}
                            </div>
                            <div className="text-sm text-muted-foreground">
                              ID: {mensagem.visitanteId}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm text-foreground max-w-xs truncate">
                          {mensagem.nomeConfiguracao ? `${mensagem.nomeConfiguracao}: ` : ''}
                          {mensagem.textoFinal}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center text-sm text-foreground">
                          <Calendar className="w-4 h-4 text-muted-foreground mr-2" />
                          {formatDateTime(mensagem.dataEnvio)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getStatusIcon(mensagem.status)}
                          {getStatusBadge(mensagem.status)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            asChild
                          >
                            <Link
                              to={`/visitantes/${mensagem.visitanteId}`}
                              title="Ver visitante"
                            >
                              <Eye className="w-4 h-4" />
                            </Link>
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
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
    </div>
  );
};

export default MensagensAgendadas;
