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
import { formatDateTime } from '@/lib/formatters';
import { useTranslation } from 'react-i18next';

const MensagensAgendadas = () => {
  const { t } = useTranslation();
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
      console.warn(t('scheduledMessagesManagement.logs.enrichVisitorsWarning'), err);
      setVisitantes([]);
    }
  }, [t]);

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
      const msg = getApiErrorMessage(err, t('scheduledMessagesManagement.errorLoad'));
      setError(msg);
      console.error(t('scheduledMessagesManagement.logs.fetchError'), err);
      toast.error(msg);
    } finally {
      if (showLoader) setLoading(false);
      setRefreshing(false);
    }
  }, [filters.dataEnvio_from, filters.dataEnvio_to, filters.status, filters.texto, filters.visitanteId, page, pageSize, t]);

  useEffect(() => {
    fetchVisitantes().catch((err) => {
      console.error(t('scheduledMessagesManagement.logs.loadVisitorsError'), err);
    });
  }, [fetchVisitantes, t]);

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
      case 1: return t('scheduledMessagesManagement.status.scheduled');
      case 2: return t('scheduledMessagesManagement.status.ready');
      case 3: return t('scheduledMessagesManagement.status.sent');
      case 4: return t('scheduledMessagesManagement.status.error');
      case 5: return t('scheduledMessagesManagement.status.canceled');
      case 6: return t('scheduledMessagesManagement.status.processing');
      default: return t('scheduledMessagesManagement.status.unknown', { status });
    }
  };

  const getStatusIcon = (status) => {
    const statusText = getStatusText(status);
    switch (statusText) {
      case t('scheduledMessagesManagement.status.scheduled'):
        return <Clock className="w-4 h-4 text-blue-500 dark:text-blue-400" />;
      case t('scheduledMessagesManagement.status.sent'):
        return <CheckCircle className="w-4 h-4 text-green-500 dark:text-green-400" />;
      case t('scheduledMessagesManagement.status.error'):
        return <XCircle className="w-4 h-4 text-red-500 dark:text-red-400" />;
      default:
        return <AlertCircle className="w-4 h-4 text-muted-foreground" />;
    }
  };

  const getStatusBadge = (status) => {
    const statusText = getStatusText(status);
    
    switch (statusText) {
      case t('scheduledMessagesManagement.status.scheduled'):
        return <Badge variant="default" className="bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">{statusText}</Badge>;
      case t('scheduledMessagesManagement.status.ready'):
        return <Badge variant="secondary">{t('scheduledMessagesManagement.status.readyShort')}</Badge>;
      case t('scheduledMessagesManagement.status.processing'):
        return <Badge variant="secondary">{t('scheduledMessagesManagement.status.processingShort')}</Badge>;
      case t('scheduledMessagesManagement.status.sent'):
        return <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">{statusText}</Badge>;
      case t('scheduledMessagesManagement.status.error'):
        return <Badge variant="destructive">{statusText}</Badge>;
      case t('scheduledMessagesManagement.status.canceled'):
        return <Badge variant="outline">{statusText}</Badge>;
      default:
        return <Badge variant="secondary">{statusText}</Badge>;
    }
  };

  const getVisitanteNome = (mensagem) => {
    if (mensagem?.nomeVisitante) return mensagem.nomeVisitante;
    const nome = visitanteNomeById.get(String(mensagem.visitanteId));
    return nome || t('scheduledMessagesManagement.visitorNotFound');
  };

  // Cancelamento não está implementado na API atualmente.

  useEffect(() => {
    setPage(1);
  }, [filters]);

  if (loading) return <LoadingPage text={t('scheduledMessagesManagement.loading')} />;
  if (error) return <ErrorPage message={error} onRetry={() => fetchMensagens({ showLoader: true })} />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-foreground">{t('scheduledMessagesManagement.title')}</h1>
          <p className="text-muted-foreground mt-1">{t('scheduledMessagesManagement.subtitle')}</p>
        </div>

        <div className="flex items-center gap-2">
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground whitespace-nowrap">{t('scheduledMessagesManagement.autoRefresh.label')}</span>
            <Select value={String(autoRefreshSeconds)} onValueChange={(v) => setAutoRefreshSeconds(Number(v))}>
              <SelectTrigger className="w-[140px]" title={t('scheduledMessagesManagement.autoRefresh.triggerTitle')}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">{t('scheduledMessagesManagement.autoRefresh.off')}</SelectItem>
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
                <p className="text-sm font-medium text-muted-foreground">{t('scheduledMessagesManagement.stats.total')}</p>
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
                <p className="text-sm font-medium text-muted-foreground">{t('scheduledMessagesManagement.stats.scheduled')}</p>
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
                <p className="text-sm font-medium text-muted-foreground">{t('scheduledMessagesManagement.stats.sent')}</p>
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
                <p className="text-sm font-medium text-muted-foreground">{t('scheduledMessagesManagement.stats.error')}</p>
                <p className="text-2xl font-bold text-red-500 dark:text-red-400">{stats.erro}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'texto', label: t('scheduledMessagesManagement.filters.messageOrVisitor'), type: 'text', placeholder: t('scheduledMessagesManagement.filters.searchPlaceholder') },
        ]}
        filterFields={[
          {
            key: 'status',
            label: t('scheduledMessagesManagement.filters.status'),
            type: 'select',
            options: [
              { value: '1', label: t('scheduledMessagesManagement.status.scheduled') },
              { value: '2', label: t('scheduledMessagesManagement.status.ready') },
              { value: '6', label: t('scheduledMessagesManagement.status.processing') },
              { value: '3', label: t('scheduledMessagesManagement.status.sent') },
              { value: '4', label: t('scheduledMessagesManagement.status.error') },
              { value: '5', label: t('scheduledMessagesManagement.status.canceled') },
            ],
          },
          {
            key: 'visitanteId',
            label: t('scheduledMessagesManagement.filters.visitor'),
            type: 'select',
            options: visitantes.map((v) => ({ value: String(v.id), label: v.nome })),
          },
          {
            key: 'dataEnvio',
            label: t('scheduledMessagesManagement.filters.sendDate'),
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
            {t('scheduledMessagesManagement.listTitle', { total })}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {mensagens.length === 0 ? (
            <PageEmptyState
              title={total === 0 ? t('scheduledMessagesManagement.empty.title') : t('scheduledMessagesManagement.empty.pageTitle')}
              description={total === 0
                ? t('scheduledMessagesManagement.empty.description')
                : t('scheduledMessagesManagement.empty.pageDescription')}
              icon={MessageSquare}
            />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>{t('scheduledMessagesManagement.table.visitor')}</TableHead>
                    <TableHead>{t('scheduledMessagesManagement.table.message')}</TableHead>
                    <TableHead>{t('scheduledMessagesManagement.table.sendDateTime')}</TableHead>
                    <TableHead>{t('scheduledMessagesManagement.table.status')}</TableHead>
                    <TableHead>{t('scheduledMessagesManagement.table.actions')}</TableHead>
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
                              {t('scheduledMessagesManagement.table.visitorId', { id: mensagem.visitanteId })}
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
                              title={t('scheduledMessagesManagement.actions.viewVisitor')}
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
