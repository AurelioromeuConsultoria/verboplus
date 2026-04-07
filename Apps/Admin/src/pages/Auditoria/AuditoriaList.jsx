import { useCallback, useEffect, useMemo, useState } from 'react';
import { Shield, Eye, Search, ArrowUpRight, TriangleAlert } from 'lucide-react';
import { Link, useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { auditLogsApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const ENTITY_OPTIONS = [
  { value: 'Auth', label: 'Autenticação' },
  { value: 'CampanhaAniversario', label: 'Campanha de Aniversário' },
  { value: 'CampanhaAniversarioEnvio', label: 'Envio da Campanha' },
  { value: 'Escala', label: 'Escala' },
  { value: 'EscalaItem', label: 'Item de Escala' },
  { value: 'SolicitacaoTrocaEscala', label: 'Solicitação de Troca' },
  { value: 'MensagemAgendada', label: 'Mensagem Agendada' },
  { value: 'Pessoa', label: 'Pessoa' },
  { value: 'PessoaPerfil', label: 'Perfil de Pessoa' },
  { value: 'Visitante', label: 'Visitante' },
  { value: 'Evento', label: 'Evento' },
  { value: 'Noticia', label: 'Notícia' },
  { value: 'Usuario', label: 'Usuário' },
];

const ACTION_OPTIONS = [
  { value: 'Create', label: 'Criação' },
  { value: 'Update', label: 'Edição' },
  { value: 'Delete', label: 'Exclusão' },
  { value: 'Login', label: 'Login' },
  { value: 'RefreshToken', label: 'Refresh Token' },
  { value: 'AlterarSenha', label: 'Alterar Senha' },
  { value: 'Publicar', label: 'Publicar' },
  { value: 'GerarAutomatico', label: 'Gerar Automático' },
  { value: 'Confirmar', label: 'Confirmar' },
  { value: 'Recusar', label: 'Recusar' },
  { value: 'RegistrarPresenca', label: 'Registrar Presença' },
  { value: 'Aprovar', label: 'Aprovar' },
  { value: 'Rejeitar', label: 'Rejeitar' },
  { value: 'Regerar', label: 'Regerar' },
  { value: 'ProntaParaEnvio', label: 'Pronta para Envio' },
  { value: 'Enviada', label: 'Enviada' },
  { value: 'ErroEnvio', label: 'Erro de Envio' },
  { value: 'AtualizarConfiguracao', label: 'Atualizar Configuração' },
  { value: 'EnviarTeste', label: 'Enviar Teste' },
  { value: 'Reenviar', label: 'Reenviar' },
  { value: 'ProcessarDia', label: 'Processar Dia' },
];

const ACTION_LABELS = Object.fromEntries(ACTION_OPTIONS.map((item) => [item.value, item.label]));
const QUICK_ACTIONS = ['Login', 'AlterarSenha', 'Publicar', 'Confirmar', 'Recusar', 'Aprovar', 'Rejeitar', 'ErroEnvio', 'ProcessarDia'];
const DEFAULT_FILTERS = {
  search: '',
  entityName: undefined,
  entityId: '',
  action: undefined,
  userName: '',
  userEmail: '',
  createdAt_from: '',
  createdAt_to: '',
};

function normalizeFilterValue(value) {
  return value == null ? '' : String(value);
}

function parseFiltersFromSearchParams(searchParams) {
  return {
    search: normalizeFilterValue(searchParams.get('search')),
    entityName: searchParams.get('entityName') || undefined,
    entityId: normalizeFilterValue(searchParams.get('entityId')),
    action: searchParams.get('action') || undefined,
    userName: normalizeFilterValue(searchParams.get('userName')),
    userEmail: normalizeFilterValue(searchParams.get('userEmail')),
    createdAt_from: normalizeFilterValue(searchParams.get('createdAt_from') || searchParams.get('from')),
    createdAt_to: normalizeFilterValue(searchParams.get('createdAt_to') || searchParams.get('to')),
  };
}

function areFiltersEqual(left, right) {
  return Object.keys(DEFAULT_FILTERS).every((key) => (left?.[key] ?? '') === (right?.[key] ?? ''));
}

function buildSearchParamsFromFilters(filters) {
  const nextParams = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      nextParams.set(key, value);
    }
  });

  return nextParams;
}

function getActionLabel(action) {
  return ACTION_LABELS[action] || action;
}

function getActionVariant(action) {
  if (action === 'Delete' || action === 'ErroEnvio' || action === 'Recusar' || action === 'Rejeitar') {
    return 'destructive';
  }

  if (action === 'Create' || action === 'Login' || action === 'Confirmar' || action === 'Aprovar' || action === 'Enviada') {
    return 'default';
  }

  return 'secondary';
}

function formatAuditJson(value) {
  if (!value) return null;

  try {
    const obj = JSON.parse(value);
    return JSON.stringify(obj, null, 2);
  } catch {
    return value;
  }
}

function parseAuditJson(value) {
  if (!value) return null;

  try {
    return JSON.parse(value);
  } catch {
    return null;
  }
}

function toLocalDateInput(date) {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function buildPeriodFilters(days) {
  const now = new Date();
  const from = new Date(now);
  from.setDate(now.getDate() - days);

  return {
    createdAt_from: toLocalDateInput(from),
    createdAt_to: toLocalDateInput(now),
  };
}

function formatTimeBucket(date) {
  const source = new Date(date);
  const day = `${source.getDate()}`.padStart(2, '0');
  const month = `${source.getMonth() + 1}`.padStart(2, '0');
  const hour = `${source.getHours()}`.padStart(2, '0');
  return `${day}/${month} ${hour}:00`;
}

function getAuditDestination(item) {
  if (!item) return null;

  const entityId = item.entityId;
  const parsed = parseAuditJson(item.changesJson);

  if (item.entityName === 'Pessoa' && entityId) {
    return { to: `/pessoas/${entityId}`, label: 'Abrir pessoa' };
  }

  if (item.entityName === 'Visitante' && entityId) {
    return { to: `/visitantes/${entityId}`, label: 'Abrir visitante' };
  }

  if (item.entityName === 'Usuario') {
    return { to: '/usuarios', label: 'Abrir usuários' };
  }

  if (item.entityName === 'PessoaPerfil') {
    const pessoaId = parsed?.newValues?.PessoaId
      ?? parsed?.changes?.PessoaId?.newValue
      ?? parsed?.changes?.PessoaId?.oldValue;

    if (pessoaId) {
      return { to: `/pessoas/${pessoaId}`, label: 'Abrir pessoa' };
    }
  }

  if (item.entityName === 'MensagemAgendada') {
    const visitanteId = parsed?.VisitanteId
      ?? parsed?.newValues?.VisitanteId
      ?? parsed?.changes?.VisitanteId?.newValue
      ?? parsed?.changes?.VisitanteId?.oldValue;

    if (visitanteId) {
      return { to: `/visitantes/${visitanteId}`, label: 'Abrir visitante' };
    }

    return { to: '/mensagens-agendadas', label: 'Abrir mensagens' };
  }

  if (item.entityName === 'Escala') {
    const ocorrenciaId = parsed?.EventoOcorrenciaId
      ?? parsed?.newValues?.EventoOcorrenciaId
      ?? parsed?.changes?.EventoOcorrenciaId?.newValue
      ?? parsed?.changes?.EventoOcorrenciaId?.oldValue;
    const equipeId = parsed?.EquipeId
      ?? parsed?.newValues?.EquipeId
      ?? parsed?.changes?.EquipeId?.newValue
      ?? parsed?.changes?.EquipeId?.oldValue;

    if (ocorrenciaId && equipeId) {
      return { to: `/voluntariado/escalas/ocorrencia/${ocorrenciaId}/equipe/${equipeId}`, label: 'Abrir escala' };
    }

    if (ocorrenciaId) {
      return { to: `/voluntariado/escalas/ocorrencia/${ocorrenciaId}`, label: 'Abrir ocorrência' };
    }

    return { to: '/voluntariado/escalas', label: 'Abrir escalas' };
  }

  if (item.entityName === 'EscalaItem' || item.entityName === 'SolicitacaoTrocaEscala') {
    return { to: '/voluntariado/solicitacoes-troca', label: 'Abrir trocas' };
  }

  if (item.entityName === 'CampanhaAniversario' || item.entityName === 'CampanhaAniversarioEnvio') {
    return { to: '/pessoas/aniversariantes/campanha', label: 'Abrir campanha' };
  }

  return null;
}

export default function AuditoriaList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [metrics, setMetrics] = useState({
    totalLogs: 0,
    criticalActions: 0,
    failureActions: 0,
    distinctUsers: 0,
    topUserLabel: '-',
    topUserCount: 0,
    topEntityName: '-',
    topEntityCount: 0,
    topActionName: '-',
    topActionCount: 0,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const [filters, setFilters] = useState(() => ({
    ...DEFAULT_FILTERS,
    ...parseFiltersFromSearchParams(searchParams),
  }));

  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selected, setSelected] = useState(null);

  const applyAlertFilters = useCallback((patch) => {
    setFilters((current) => ({ ...current, ...patch }));
    setPage(1);
  }, []);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const params = {
        page,
        pageSize,
        search: filters.search || undefined,
        entityName: filters.entityName || undefined,
        entityId: filters.entityId || undefined,
        action: filters.action || undefined,
        userName: filters.userName || undefined,
        userEmail: filters.userEmail || undefined,
        from: filters.createdAt_from || undefined,
        to: filters.createdAt_to || undefined,
      };

      const [resp, metricsResp] = await Promise.all([
        auditLogsApi.getPaged(params),
        auditLogsApi.getMetrics(params),
      ]);

      const data = resp.data || {};
      setItems(data.items || []);
      setTotal(Number(data.total || 0));
      setMetrics({
        totalLogs: Number(metricsResp.data?.totalLogs || 0),
        criticalActions: Number(metricsResp.data?.criticalActions || 0),
        failureActions: Number(metricsResp.data?.failureActions || 0),
        distinctUsers: Number(metricsResp.data?.distinctUsers || 0),
        topUserLabel: metricsResp.data?.topUserLabel || '-',
        topUserCount: Number(metricsResp.data?.topUserCount || 0),
        topEntityName: metricsResp.data?.topEntityName || '-',
        topEntityCount: Number(metricsResp.data?.topEntityCount || 0),
        topActionName: metricsResp.data?.topActionName || '-',
        topActionCount: Number(metricsResp.data?.topActionCount || 0),
      });
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar auditoria');
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  }, [filters.action, filters.createdAt_from, filters.createdAt_to, filters.entityId, filters.entityName, filters.search, filters.userEmail, filters.userName, page, pageSize]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    setPage(1);
  }, [filters]);

  useEffect(() => {
    const nextFilters = {
      ...DEFAULT_FILTERS,
      ...parseFiltersFromSearchParams(searchParams),
    };

    setFilters((current) => (areFiltersEqual(current, nextFilters) ? current : nextFilters));
  }, [searchParams]);

  useEffect(() => {
    const nextParams = buildSearchParamsFromFilters(filters);
    const currentParams = searchParams.toString();
    const targetParams = nextParams.toString();

    if (currentParams !== targetParams) {
      setSearchParams(nextParams, { replace: true });
    }
  }, [filters, searchParams, setSearchParams]);

  const prettyJson = useMemo(() => {
    return formatAuditJson(selected?.changesJson);
  }, [selected]);

  const selectedDestination = useMemo(() => getAuditDestination(selected), [selected]);

  const activeFilterCount = useMemo(() => {
    return Object.values(filters).filter((value) => value !== undefined && value !== '').length;
  }, [filters]);

  const periodLabel = useMemo(() => {
    if (filters.createdAt_from && filters.createdAt_to) {
      return `${filters.createdAt_from} ate ${filters.createdAt_to}`;
    }

    if (filters.createdAt_from) {
      return `desde ${filters.createdAt_from}`;
    }

    if (filters.createdAt_to) {
      return `ate ${filters.createdAt_to}`;
    }

    return 'todo o periodo';
  }, [filters.createdAt_from, filters.createdAt_to]);

  const currentActionLabel = filters.action ? getActionLabel(filters.action) : 'todas as ações';
  const recentCriticalItems = useMemo(() => {
    return items
      .filter((item) => QUICK_ACTIONS.includes(item.action) || ['Delete', 'ErroEnvio', 'Recusar', 'Rejeitar'].includes(item.action))
      .slice(0, 6);
  }, [items]);
  const trendBuckets = useMemo(() => {
    const map = new Map();

    for (const item of items) {
      if (!item.createdAt) continue;
      const bucket = formatTimeBucket(item.createdAt);
      map.set(bucket, (map.get(bucket) || 0) + 1);
    }

    return [...map.entries()]
      .slice(0, 8)
      .map(([label, count]) => ({ label, count }))
      .reverse();
  }, [items]);
  const maxTrendCount = useMemo(() => {
    return trendBuckets.reduce((max, item) => Math.max(max, item.count), 1);
  }, [trendBuckets]);
  const alerts = useMemo(() => {
    const nextAlerts = [];

    if (metrics.failureActions >= 5) {
      nextAlerts.push({
        id: 'failure-spike',
        title: 'Volume elevado de falhas ou recusas',
        description: `${metrics.failureActions} eventos negativos encontrados no conjunto filtrado.`,
        variant: 'destructive',
        actionLabel: 'Filtrar falhas',
        action: () => applyAlertFilters({ action: 'ErroEnvio' }),
      });
    }

    if (metrics.topActionName === 'ErroEnvio' && metrics.topActionCount >= 3) {
      nextAlerts.push({
        id: 'send-errors',
        title: 'Erros de envio em destaque',
        description: `${metrics.topActionCount} registros com erro de envio dominam o resultado atual.`,
        variant: 'destructive',
        actionLabel: 'Ver erros de envio',
        action: () => applyAlertFilters({ action: 'ErroEnvio', entityName: 'MensagemAgendada' }),
      });
    }

    if (metrics.topActionName === 'AlterarSenha' && metrics.topActionCount >= 3) {
      nextAlerts.push({
        id: 'password-changes',
        title: 'Muitas alterações de senha',
        description: `${metrics.topActionCount} alterações de senha aparecem como ação mais frequente.`,
        variant: 'secondary',
        actionLabel: 'Filtrar senhas',
        action: () => applyAlertFilters({ action: 'AlterarSenha', entityName: 'Usuario' }),
      });
    }

    if (metrics.topActionName === 'Login' && metrics.topActionCount >= 10) {
      nextAlerts.push({
        id: 'login-activity',
        title: 'Pico de login no período',
        description: `${metrics.topActionCount} logins foram registrados com os filtros atuais.`,
        variant: 'secondary',
        actionLabel: 'Ver logins',
        action: () => applyAlertFilters({ action: 'Login', entityName: 'Auth' }),
      });
    }

    if (metrics.criticalActions >= 12) {
      nextAlerts.push({
        id: 'critical-actions',
        title: 'Alta concentração de ações críticas',
        description: `${metrics.criticalActions} ações críticas foram encontradas no conjunto filtrado.`,
        variant: 'default',
        actionLabel: 'Isolar críticas',
        action: () => applyAlertFilters({ action: undefined }),
      });
    }

    return nextAlerts;
  }, [applyAlertFilters, metrics.criticalActions, metrics.failureActions, metrics.topActionCount, metrics.topActionName]);

  if (loading) return <LoadingPage text="Carregando auditoria..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Auditoria</h1>
          <p className="text-muted-foreground mt-1">Histórico de alterações no sistema</p>
        </div>
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'search', label: 'Busca geral', type: 'text', placeholder: 'Entidade, ação, usuário, e-mail ou ID...' },
          { key: 'entityId', label: 'ID da entidade', type: 'text', placeholder: 'Ex.: 10, teste, 2026-04-02...' },
          { key: 'userName', label: 'Nome do usuário', type: 'text', placeholder: 'Buscar por nome...' },
          { key: 'userEmail', label: 'E-mail do usuário', type: 'text', placeholder: 'Buscar por e-mail...' },
        ]}
        filterFields={[
          { key: 'entityName', label: 'Entidade', type: 'select', options: ENTITY_OPTIONS },
          { key: 'action', label: 'Ação', type: 'select', options: ACTION_OPTIONS },
          { key: 'createdAt', label: 'Data', type: 'date-range' },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() => setFilters(DEFAULT_FILTERS)}
      />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total no resultado</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{metrics.totalLogs}</div>
            <p className="text-xs text-muted-foreground mt-1">Todos os logs do conjunto filtrado</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Ocorrências críticas</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{metrics.criticalActions}</div>
            <p className="text-xs text-muted-foreground mt-1">Ações sensíveis no conjunto filtrado</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Falhas e recusas</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{metrics.failureActions}</div>
            <p className="text-xs text-muted-foreground mt-1">Erros operacionais e respostas negativas</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Maior volume atual</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div>
              <div className="text-xs uppercase tracking-wide text-muted-foreground">Usuário</div>
              <div className="font-semibold">{metrics.topUserLabel}</div>
              <div className="text-xs text-muted-foreground">{metrics.topUserCount} registro(s)</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-muted-foreground">Entidade</div>
              <div className="font-semibold">{metrics.topEntityName}</div>
              <div className="text-xs text-muted-foreground">{metrics.topEntityCount} registro(s)</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-muted-foreground">Ação</div>
              <div className="font-semibold">{getActionLabel(metrics.topActionName)}</div>
              <div className="text-xs text-muted-foreground">{metrics.topActionCount} registro(s)</div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Alertas operacionais</CardTitle>
        </CardHeader>
        <CardContent>
          {alerts.length === 0 ? (
            <div className="flex items-center gap-3 rounded-lg border border-dashed p-4 text-sm text-muted-foreground">
              <Shield className="h-4 w-4" />
              Nenhum alerta automático disparado com os filtros atuais.
            </div>
          ) : (
            <div className="space-y-3">
              {alerts.map((alert) => (
                <div key={alert.id} className="flex items-start gap-3 rounded-lg border p-4">
                  <TriangleAlert className="mt-0.5 h-4 w-4 text-amber-600" />
                  <div className="flex-1 space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{alert.title}</span>
                      <Badge variant={alert.variant}>{alert.variant === 'destructive' ? 'Alta' : alert.variant === 'default' ? 'Média' : 'Atenção'}</Badge>
                    </div>
                    <p className="text-sm text-muted-foreground">{alert.description}</p>
                    {alert.actionLabel ? (
                      <div className="pt-1">
                        <Button type="button" variant="outline" size="sm" onClick={alert.action}>
                          {alert.actionLabel}
                        </Button>
                      </div>
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-4 xl:grid-cols-[1.3fr_0.7fr]">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Eventos críticos recentes</CardTitle>
          </CardHeader>
          <CardContent>
            {recentCriticalItems.length === 0 ? (
              <p className="text-sm text-muted-foreground">Nenhum evento crítico no resultado atual.</p>
            ) : (
              <div className="space-y-3">
                {recentCriticalItems.map((item) => {
                  const destination = getAuditDestination(item);
                  return (
                    <div key={item.id} className="flex items-start justify-between gap-3 rounded-lg border p-3">
                      <div className="space-y-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <Badge variant={getActionVariant(item.action)}>{getActionLabel(item.action)}</Badge>
                          <span className="text-sm font-medium">{item.entityName}</span>
                          <span className="text-xs text-muted-foreground">#{item.entityId}</span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {item.userEmail || item.userName || (item.userId ? `User ${item.userId}` : 'Sistema')}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {item.createdAt ? new Date(item.createdAt).toLocaleString('pt-BR') : '-'}
                        </div>
                      </div>
                      <div className="flex items-center gap-1">
                        {destination ? (
                          <Button type="button" variant="ghost" size="sm" asChild>
                            <Link to={destination.to} title={destination.label}>
                              <ArrowUpRight className="h-4 w-4" />
                            </Link>
                          </Button>
                        ) : null}
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => {
                            setSelected(item);
                            setDetailsOpen(true);
                          }}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Tendência recente</CardTitle>
          </CardHeader>
          <CardContent>
            {trendBuckets.length === 0 ? (
              <p className="text-sm text-muted-foreground">Sem dados suficientes para tendência.</p>
            ) : (
              <div className="space-y-3">
                {trendBuckets.map((bucket) => (
                  <div key={bucket.label} className="space-y-1">
                    <div className="flex items-center justify-between text-xs text-muted-foreground">
                      <span>{bucket.label}</span>
                      <span>{bucket.count}</span>
                    </div>
                    <div className="h-2 rounded-full bg-muted">
                      <div
                        className="h-2 rounded-full bg-primary"
                        style={{ width: `${Math.max((bucket.count / maxTrendCount) * 100, 8)}%` }}
                      />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Atalhos de investigação</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-wrap gap-2">
            <Button
              type="button"
              variant={!filters.createdAt_from && !filters.createdAt_to ? 'default' : 'outline'}
              size="sm"
              onClick={() => setFilters((current) => ({ ...current, createdAt_from: '', createdAt_to: '' }))}
            >
              Todo o período
            </Button>
            <Button
              type="button"
              variant={filters.createdAt_from === toLocalDateInput(new Date()) && filters.createdAt_to === toLocalDateInput(new Date()) ? 'default' : 'outline'}
              size="sm"
              onClick={() => {
                const today = toLocalDateInput(new Date());
                setFilters((current) => ({ ...current, createdAt_from: today, createdAt_to: today }));
              }}
            >
              Hoje
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setFilters((current) => ({ ...current, ...buildPeriodFilters(1) }))}
            >
              Últimas 24h
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setFilters((current) => ({ ...current, ...buildPeriodFilters(7) }))}
            >
              Últimos 7 dias
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setFilters((current) => ({ ...current, ...buildPeriodFilters(30) }))}
            >
              Últimos 30 dias
            </Button>
          </div>

          <div className="flex flex-wrap gap-2">
            <Button
              type="button"
              variant={!filters.action ? 'default' : 'outline'}
              size="sm"
              onClick={() => setFilters((current) => ({ ...current, action: undefined }))}
            >
              Todas as ações
            </Button>
            {QUICK_ACTIONS.map((action) => (
              <Button
                key={action}
                type="button"
                variant={filters.action === action ? 'default' : 'outline'}
                size="sm"
                onClick={() => setFilters((current) => ({ ...current, action }))}
              >
                {getActionLabel(action)}
              </Button>
            ))}
          </div>

          <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
            <Badge variant="secondary">Filtros ativos: {activeFilterCount}</Badge>
            <Badge variant="outline">Período: {periodLabel}</Badge>
            <Badge variant="outline">Ação: {currentActionLabel}</Badge>
            {filters.entityName ? <Badge variant="outline">Entidade: {filters.entityName}</Badge> : null}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Logs ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {items.length === 0 ? (
            <div className="text-center py-12">
              <Shield className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium text-foreground mb-2">Nenhum log encontrado</h3>
              <p className="text-muted-foreground">Ajuste os filtros ou aguarde novas alterações no sistema.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data</TableHead>
                    <TableHead>Ação</TableHead>
                    <TableHead>Entidade</TableHead>
                    <TableHead>ID</TableHead>
                    <TableHead>Usuário</TableHead>
                    <TableHead>IP</TableHead>
                    <TableHead className="text-right">Detalhes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {items.map((it) => (
                    <TableRow key={it.id}>
                      <TableCell>{it.createdAt ? new Date(it.createdAt).toLocaleString('pt-BR') : '-'}</TableCell>
                      <TableCell>
                        <Badge variant={getActionVariant(it.action)}>
                          {getActionLabel(it.action)}
                        </Badge>
                      </TableCell>
                      <TableCell>{it.entityName}</TableCell>
                      <TableCell>{it.entityId}</TableCell>
                      <TableCell className="max-w-[260px] truncate">
                        {it.userEmail || it.userName || (it.userId ? `User ${it.userId}` : '-') }
                      </TableCell>
                      <TableCell>{it.ipAddress || '-'}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          {getAuditDestination(it) ? (
                            <Button type="button" variant="ghost" size="sm" asChild>
                              <Link to={getAuditDestination(it).to} title={getAuditDestination(it).label}>
                                <ArrowUpRight className="h-4 w-4" />
                              </Link>
                            </Button>
                          ) : null}
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => {
                              setSelected(it);
                              setDetailsOpen(true);
                            }}
                          >
                            <Eye className="h-4 w-4" />
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

      <Dialog open={detailsOpen} onOpenChange={setDetailsOpen}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Detalhes do log</DialogTitle>
            <DialogDescription>
              {selected ? `${selected.entityName} ${selected.entityId} — ${getActionLabel(selected.action)}` : ''}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div className="grid grid-cols-1 gap-2 text-sm">
              <div><span className="text-muted-foreground">Entidade:</span> {selected?.entityName || '-'}</div>
              <div><span className="text-muted-foreground">ID da entidade:</span> {selected?.entityId || '-'}</div>
              <div><span className="text-muted-foreground">Ação:</span> {selected?.action ? getActionLabel(selected.action) : '-'}</div>
              <div><span className="text-muted-foreground">Usuário:</span> {selected?.userEmail || selected?.userName || selected?.userId || '-'}</div>
              <div><span className="text-muted-foreground">IP:</span> {selected?.ipAddress || '-'}</div>
              <div><span className="text-muted-foreground">Quando:</span> {selected?.createdAt ? new Date(selected.createdAt).toLocaleString('pt-BR') : '-'}</div>
            </div>
            {selectedDestination ? (
              <div className="flex justify-end">
                <Button type="button" variant="outline" asChild>
                  <Link to={selectedDestination.to} onClick={() => setDetailsOpen(false)}>
                    <ArrowUpRight className="mr-2 h-4 w-4" />
                    {selectedDestination.label}
                  </Link>
                </Button>
              </div>
            ) : null}
            <div className="rounded-md border bg-muted/30 p-3">
              <div className="mb-2 flex items-center gap-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                <Search className="h-3.5 w-3.5" />
                Mudanças registradas
              </div>
              <pre className="whitespace-pre-wrap text-xs overflow-auto max-h-[380px]">
                {prettyJson || 'Sem detalhes de mudanças.'}
              </pre>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
