import { useCallback, useEffect, useMemo, useState } from 'react';
import { Activity, ShieldAlert, ShieldCheck, Siren, TimerReset } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageRefreshButton } from '@/components/ui/page-state';
import { auditLogsApi, operacaoApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

function getHealthVariant(status) {
  if (status === 'Healthy') return 'default';
  if (status === 'Degraded') return 'secondary';
  return 'destructive';
}

function getSchedulerVariant(status) {
  if (status === 'Success') return 'default';
  if (status === 'Running') return 'secondary';
  return 'destructive';
}

function formatDateTime(value) {
  if (!value) return '-';
  return new Date(value).toLocaleString('pt-BR');
}

function formatDuration(ms) {
  if (ms == null) return '-';
  if (ms < 1000) return `${ms} ms`;
  return `${(ms / 1000).toFixed(1)} s`;
}

function normalizeHealthPayload(payload) {
  if (!payload) {
    return {
      status: '-',
      totalDuration: null,
      checks: [],
    };
  }

  const rawChecks = payload.checks;
  const checks = Array.isArray(rawChecks)
    ? rawChecks
    : Object.entries(rawChecks || {}).map(([name, item]) => ({
        name,
        status: item?.status || 'Unknown',
        duration: item?.duration ?? null,
        description: item?.description || null,
        error: item?.error || null,
        tags: item?.tags || [],
      }));

  return {
    status: payload.status || '-',
    totalDuration: payload.totalDuration ?? null,
    checks,
  };
}

function toAuditDateInput(date) {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function buildAuditLink(patch = {}) {
  const now = new Date();
  const yesterday = new Date(now);
  yesterday.setDate(now.getDate() - 1);

  const params = new URLSearchParams({
    createdAt_from: toAuditDateInput(yesterday),
    createdAt_to: toAuditDateInput(now),
  });

  Object.entries(patch).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    params.set(key, String(value));
  });

  return `/auditoria?${params.toString()}`;
}

function buildCriticalAuditLink() {
  return buildAuditLink({ search: 'Publicar Confirmar Recusar Aprovar Rejeitar ProcessarDia RegistrarPresenca' });
}

export default function OperacaoDashboard() {
  const [health, setHealth] = useState(null);
  const [schedulers, setSchedulers] = useState([]);
  const [auditMetrics, setAuditMetrics] = useState({
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
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async ({ silent = false } = {}) => {
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const periodFilters = {
        from: new Date(Date.now() - (24 * 60 * 60 * 1000)).toISOString(),
        to: new Date().toISOString(),
      };

      const [healthRes, schedulersRes, auditMetricsRes] = await Promise.all([
        operacaoApi.getHealth(),
        operacaoApi.getSchedulers(),
        auditLogsApi.getMetrics(periodFilters),
      ]);

      setHealth(normalizeHealthPayload(healthRes.data));
      setSchedulers(schedulersRes.data || []);
      setAuditMetrics({
        totalLogs: Number(auditMetricsRes.data?.totalLogs || 0),
        criticalActions: Number(auditMetricsRes.data?.criticalActions || 0),
        failureActions: Number(auditMetricsRes.data?.failureActions || 0),
        distinctUsers: Number(auditMetricsRes.data?.distinctUsers || 0),
        topUserLabel: auditMetricsRes.data?.topUserLabel || '-',
        topUserCount: Number(auditMetricsRes.data?.topUserCount || 0),
        topEntityName: auditMetricsRes.data?.topEntityName || '-',
        topEntityCount: Number(auditMetricsRes.data?.topEntityCount || 0),
        topActionName: auditMetricsRes.data?.topActionName || '-',
        topActionCount: Number(auditMetricsRes.data?.topActionCount || 0),
      });
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar operação');
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const operationSummary = useMemo(() => {
    const checks = Array.isArray(health?.checks) ? health.checks : [];
    const degradedChecks = checks.filter((item) => item.status === 'Degraded').length;
    const unhealthyChecks = checks.filter((item) => item.status === 'Unhealthy').length;
    const failedSchedulers = schedulers.filter((item) => item.lastStatus !== 'Success').length;
    const auditAttention = auditMetrics.failureActions + auditMetrics.criticalActions;

    return {
      degradedChecks,
      unhealthyChecks,
      failedSchedulers,
      auditAttention,
    };
  }, [auditMetrics.criticalActions, auditMetrics.failureActions, health, schedulers]);

  if (loading) return <LoadingPage text="Carregando operação..." />;
  if (error) return <ErrorPage message={error} onRetry={() => load()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Operação</h1>
          <p className="mt-1 text-muted-foreground">Visão consolidada de saúde, schedulers e acesso rápido à investigação.</p>
        </div>
        <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Saúde geral</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-3">
              <ShieldCheck className="h-5 w-5 text-primary" />
              <div className="text-2xl font-bold">{health?.status || '-'}</div>
            </div>
            <p className="mt-1 text-xs text-muted-foreground">Duração total: {formatDuration(health?.totalDuration)}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Checks degradados</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{operationSummary.degradedChecks}</div>
            <p className="mt-1 text-xs text-muted-foreground">Dependências com atenção</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Checks indisponíveis</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{operationSummary.unhealthyChecks}</div>
            <p className="mt-1 text-xs text-muted-foreground">Falhas críticas de ambiente</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Schedulers com atenção</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{operationSummary.failedSchedulers}</div>
            <p className="mt-1 text-xs text-muted-foreground">Última execução fora de sucesso</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Auditoria 24h</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-3">
              <ShieldAlert className="h-5 w-5 text-primary" />
              <div className="text-2xl font-bold">{auditMetrics.totalLogs}</div>
            </div>
            <p className="mt-1 text-xs text-muted-foreground">Eventos auditados no ultimo dia</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Acoes criticas</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{auditMetrics.criticalActions}</div>
            <p className="mt-1 text-xs text-muted-foreground">Operacoes sensiveis nas ultimas 24h</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Falhas e recusas</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{auditMetrics.failureActions}</div>
            <p className="mt-1 text-xs text-muted-foreground">Sinais de erro ou excecao operacional</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Usuarios ativos</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{auditMetrics.distinctUsers}</div>
            <p className="mt-1 text-xs text-muted-foreground">
              Topo: {auditMetrics.topUserLabel} ({auditMetrics.topUserCount})
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Alertas operacionais</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {operationSummary.unhealthyChecks === 0 && operationSummary.failedSchedulers === 0 ? (
            <div className="flex items-center gap-3 rounded-lg border border-dashed p-4 text-sm text-muted-foreground">
              <Activity className="h-4 w-4" />
              Nenhum alerta crítico nesta leitura.
            </div>
          ) : null}

          {operationSummary.unhealthyChecks > 0 ? (
            <div className="flex items-start gap-3 rounded-lg border p-4">
              <Siren className="mt-0.5 h-4 w-4 text-red-600" />
              <div>
                <div className="font-medium">Health check em estado crítico</div>
                <div className="text-sm text-muted-foreground">Há {operationSummary.unhealthyChecks} verificação(ões) em estado `Unhealthy`.</div>
              </div>
            </div>
          ) : null}

          {operationSummary.failedSchedulers > 0 ? (
            <div className="flex items-start gap-3 rounded-lg border p-4">
              <TimerReset className="mt-0.5 h-4 w-4 text-amber-600" />
              <div>
                <div className="font-medium">Schedulers exigem revisão</div>
                <div className="text-sm text-muted-foreground">Há {operationSummary.failedSchedulers} scheduler(s) sem sucesso na última execução.</div>
              </div>
            </div>
          ) : null}

          {auditMetrics.failureActions > 0 ? (
            <div className="flex items-start gap-3 rounded-lg border p-4">
              <ShieldAlert className="mt-0.5 h-4 w-4 text-red-600" />
              <div className="flex-1">
                <div className="font-medium">Auditoria indica falhas recentes</div>
                <div className="text-sm text-muted-foreground">
                  Foram detectados {auditMetrics.failureActions} evento(s) de falha, recusa ou rejeição nas últimas 24 horas.
                </div>
              </div>
              <Button type="button" variant="outline" size="sm" asChild>
                <Link to={buildAuditLink({ action: 'ErroEnvio', entityName: 'MensagemAgendada' })}>Investigar</Link>
              </Button>
            </div>
          ) : null}

          {auditMetrics.criticalActions >= 10 ? (
            <div className="flex items-start gap-3 rounded-lg border p-4">
              <Activity className="mt-0.5 h-4 w-4 text-amber-600" />
              <div className="flex-1">
                <div className="font-medium">Volume alto de ações críticas</div>
                <div className="text-sm text-muted-foreground">
                  A auditoria registrou {auditMetrics.criticalActions} ações críticas no período recente. Vale revisar o contexto operacional.
                </div>
              </div>
              <Button type="button" variant="outline" size="sm" asChild>
                <Link to={buildCriticalAuditLink()}>Abrir auditoria</Link>
              </Button>
            </div>
          ) : null}
        </CardContent>
      </Card>

      <div className="grid gap-4 xl:grid-cols-[1fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Health checks</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {(Array.isArray(health?.checks) ? health.checks : []).map((item) => (
                <div key={item.name} className="rounded-lg border p-3">
                  <div className="flex items-center justify-between gap-3">
                    <div className="font-medium">{item.name}</div>
                    <Badge variant={getHealthVariant(item.status)}>{item.status}</Badge>
                  </div>
                  <div className="mt-1 text-sm text-muted-foreground">{item.description || 'Sem descrição adicional.'}</div>
                  <div className="mt-2 text-xs text-muted-foreground">Duração: {formatDuration(item.duration)}</div>
                  {item.error ? (
                    <div className="mt-2 text-xs text-red-600">Erro: {item.error}</div>
                  ) : null}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Acesso rápido</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink()}>Abrir auditoria</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to="/mensagens-agendadas">Abrir mensagens agendadas</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'Login', entityName: 'Auth' })}>Abrir logins recentes</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'AlterarSenha', entityName: 'Usuario' })}>Abrir alteracoes de senha</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'ErroEnvio', entityName: 'MensagemAgendada' })}>Abrir erros de envio</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to="/pessoas/aniversariantes/campanha">Abrir campanha de aniversário</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to="/voluntariado/solicitacoes-troca">Abrir trocas de escala</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'ErroEnvio', entityName: 'MensagemAgendada' })}>Abrir falhas e sinais operacionais</Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 xl:grid-cols-[1.1fr_0.9fr]">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Sinais da auditoria nas ultimas 24h</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 md:grid-cols-2">
            <div className="rounded-lg border p-4">
              <div className="text-sm font-medium text-muted-foreground">Entidade dominante</div>
              <div className="mt-2 text-xl font-semibold">{auditMetrics.topEntityName}</div>
              <div className="mt-1 text-xs text-muted-foreground">{auditMetrics.topEntityCount} registro(s)</div>
            </div>
            <div className="rounded-lg border p-4">
              <div className="text-sm font-medium text-muted-foreground">Acao dominante</div>
              <div className="mt-2 text-xl font-semibold">{auditMetrics.topActionName}</div>
              <div className="mt-1 text-xs text-muted-foreground">{auditMetrics.topActionCount} registro(s)</div>
            </div>
            <div className="rounded-lg border p-4 md:col-span-2">
              <div className="text-sm font-medium text-muted-foreground">Leitura operacional</div>
              <div className="mt-2 text-sm text-muted-foreground">
                {operationSummary.auditAttention === 0
                  ? 'Sem sinais relevantes de auditoria nas ultimas 24 horas.'
                  : `A soma de acoes criticas e falhas chegou a ${operationSummary.auditAttention} evento(s), com destaque para ${auditMetrics.topActionName} em ${auditMetrics.topEntityName}.`}
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Investigacao rapida</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink()}>Auditoria geral</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'ErroEnvio', entityName: 'MensagemAgendada' })}>Erros de envio</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'Login', entityName: 'Auth' })}>Logins</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ action: 'AlterarSenha', entityName: 'Usuario' })}>Alteracoes de senha</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildAuditLink({ entityName: 'Escala' })}>Escalas e operacao voluntaria</Link>
            </Button>
            <Button type="button" variant="outline" className="w-full justify-start" asChild>
              <Link to={buildCriticalAuditLink()}>Acoes criticas</Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Schedulers</CardTitle>
        </CardHeader>
        <CardContent>
          {schedulers.length === 0 ? (
            <p className="text-sm text-muted-foreground">Nenhum scheduler monitorado no momento.</p>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Scheduler</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Início</TableHead>
                    <TableHead>Fim</TableHead>
                    <TableHead>Duração</TableHead>
                    <TableHead>Detalhes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {schedulers.map((item) => (
                    <TableRow key={item.schedulerName}>
                      <TableCell className="font-medium">{item.schedulerName}</TableCell>
                      <TableCell>
                        <Badge variant={getSchedulerVariant(item.lastStatus)}>{item.lastStatus || '-'}</Badge>
                      </TableCell>
                      <TableCell>{formatDateTime(item.lastStartedAt)}</TableCell>
                      <TableCell>{formatDateTime(item.lastFinishedAt)}</TableCell>
                      <TableCell>{formatDuration(item.lastDurationMs)}</TableCell>
                      <TableCell className="max-w-[320px] whitespace-pre-wrap text-sm text-muted-foreground">
                        {item.lastError || item.details || '-'}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
