import React, { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Play, Mail, MessageSquare, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageRefreshButton } from '@/components/ui/page-state';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { comunicacaoCampanhasApi, comunicacaoDiagnosticoApi, comunicacaoEntregasApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

const getStatusLabel = (status) => {
  switch (Number(status)) {
    case 1: return 'Rascunho';
    case 2: return 'Agendada';
    case 3: return 'Processando';
    case 4: return 'Concluída';
    case 5: return 'Com falhas';
    case 6: return 'Cancelada';
    default: return `Status ${status}`;
  }
};

const getEntregaStatus = (status) => {
  switch (Number(status)) {
    case 1: return 'Pendente';
    case 2: return 'Reservado';
    case 3: return 'Enviado';
    case 4: return 'Entregue';
    case 5: return 'Falhou';
    case 6: return 'Cancelado';
    case 7: return 'Ignorado';
    default: return `Status ${status}`;
  }
};

const getCanalLabel = (canal) => {
  switch (Number(canal)) {
    case 1: return 'WhatsApp';
    case 2: return 'E-mail';
    case 3: return 'Push';
    case 4: return 'Notificação interna';
    default: return `Canal ${canal}`;
  }
};

const getCanalIcon = (canal) => {
  switch (Number(canal)) {
    case 1: return <MessageSquare className="w-4 h-4" />;
    case 2: return <Mail className="w-4 h-4" />;
    default: return <MessageSquare className="w-4 h-4" />;
  }
};

export default function ComunicacaoCampanhaDetails() {
  const { id } = useParams();
  const [campanha, setCampanha] = useState(null);
  const [entregas, setEntregas] = useState([]);
  const [healthChecks, setHealthChecks] = useState({});
  const [statusFilter, setStatusFilter] = useState('todos');
  const [canalFilter, setCanalFilter] = useState('todos');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async ({ silent = false } = {}) => {
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const [campanhaResponse, entregasResponse] = await Promise.all([
        comunicacaoCampanhasApi.getById(id),
        comunicacaoCampanhasApi.getEntregas(id),
      ]);

      setCampanha(campanhaResponse.data || null);
      setEntregas(entregasResponse.data || []);

      try {
        const healthResponse = await comunicacaoDiagnosticoApi.getHealth();
        setHealthChecks(healthResponse.data?.checks || {});
      } catch {
        setHealthChecks({});
      }
    } catch (err) {
      setError(getApiErrorMessage(err, 'Erro ao carregar detalhes da campanha'));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  const processarPendentes = async () => {
    try {
      setProcessing(true);
      const response = await comunicacaoEntregasApi.processarPendentes(100);
      toast.success(`${response.data?.processadas ?? 0} entregas processadas.`);
      await load({ silent: true });
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao processar pendências'));
    } finally {
      setProcessing(false);
    }
  };

  if (loading) return <LoadingPage text="Carregando detalhes da campanha..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!campanha) return <ErrorPage message="Campanha não encontrada." onRetry={load} />;

  const falhas = entregas.filter((entrega) => Number(entrega.status) === 5);
  const entregasFiltradas = entregas.filter((entrega) => {
    const statusOk = statusFilter === 'todos' || String(entrega.status) === statusFilter;
    const canalOk = canalFilter === 'todos' || String(entrega.canal) === canalFilter;
    return statusOk && canalOk;
  });
  const resumoPorCanal = (campanha.canais || []).map((canal) => {
    const doCanal = entregas.filter((item) => Number(item.canal) === Number(canal.canal));
    const falhasCanal = doCanal.filter((item) => Number(item.status) === 5).length;
    const sucessosCanal = doCanal.filter((item) => [3, 4].includes(Number(item.status))).length;
    const healthKey = Number(canal.canal) === 1
      ? 'evolution_api_configuration'
      : Number(canal.canal) === 2
        ? 'email_configuration'
        : Number(canal.canal) === 3
          ? 'push_configuration'
          : null;
    const health = healthKey ? healthChecks[healthKey] : null;

    return {
      ...canal,
      total: doCanal.length,
      falhas: falhasCanal,
      sucessos: sucessosCanal,
      diagnostico: health?.description || null,
      configOk: !health || health.status === 'Healthy',
    };
  });

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <Button variant="ghost" size="sm" asChild>
            <Link to="/comunicacao/campanhas">
              <ArrowLeft className="w-4 h-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">{campanha.nome}</h1>
            <p className="text-muted-foreground mt-1">{campanha.objetivo} • {campanha.publicoAlvo}</p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          <Button onClick={processarPendentes} disabled={processing}>
            <Play className="w-4 h-4 mr-2" />
            {processing ? 'Processando...' : 'Processar pendências'}
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Status</div><div className="text-lg font-semibold mt-1">{getStatusLabel(campanha.status)}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Entregas</div><div className="text-2xl font-bold mt-1">{campanha.totalEntregas}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Falhas</div><div className="text-2xl font-bold mt-1">{campanha.totalFalhas}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Agendamento</div><div className="text-sm font-medium mt-1">{campanha.dataAgendamento ? new Date(campanha.dataAgendamento).toLocaleString('pt-BR') : 'Imediato / rascunho'}</div></CardContent></Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Canais</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {resumoPorCanal.map((canal) => (
            <div key={`${canal.canal}-${canal.prioridade}`} className="rounded-lg border border-border p-4 flex items-center justify-between gap-3">
              <div className="flex items-center gap-2">
                {getCanalIcon(canal.canal)}
                <div>
                  <div className="font-medium">{getCanalLabel(canal.canal)}</div>
                  <div className="text-sm text-muted-foreground">{canal.nomeTemplate || 'Sem template vinculado'}</div>
                  <div className="text-xs text-muted-foreground mt-1">
                    {canal.diagnostico || 'Sem diagnóstico detalhado para este canal.'}
                  </div>
                  <div className="text-xs text-muted-foreground mt-2">
                    {canal.sucessos} com sucesso • {canal.falhas} falhas
                  </div>
                </div>
              </div>
              <div className="flex flex-col items-end gap-2">
                <Badge variant="secondary">Prioridade {canal.prioridade}</Badge>
                <Badge variant={canal.configOk ? 'outline' : 'destructive'}>
                  {canal.configOk ? `${canal.total} entregas` : 'Configuração pendente'}
                </Badge>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      {falhas.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Falhas recentes por canal</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {falhas.slice(0, 10).map((entrega) => (
              <div key={`falha-${entrega.id}`} className="rounded-lg border border-red-200 bg-red-50/50 p-4">
                <div className="flex items-center justify-between gap-3">
                  <div className="font-medium">{getCanalLabel(entrega.canal)}</div>
                  <Badge variant="destructive">{getEntregaStatus(entrega.status)}</Badge>
                </div>
                <div className="text-sm text-muted-foreground mt-1">{entrega.destinoResolvido}</div>
                <div className="text-sm mt-2 whitespace-pre-wrap">{entrega.erro || 'Falha sem mensagem detalhada.'}</div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Entregas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap items-center gap-2 mb-4">
            <Button
              type="button"
              size="sm"
              variant={statusFilter === 'todos' ? 'default' : 'outline'}
              onClick={() => setStatusFilter('todos')}
            >
              Todos
            </Button>
            <Button
              type="button"
              size="sm"
              variant={statusFilter === '5' ? 'destructive' : 'outline'}
              onClick={() => setStatusFilter('5')}
            >
              Falhas
            </Button>
            <Button
              type="button"
              size="sm"
              variant={statusFilter === '1' ? 'secondary' : 'outline'}
              onClick={() => setStatusFilter('1')}
            >
              Pendentes
            </Button>
            <Button
              type="button"
              size="sm"
              variant={statusFilter === '3' ? 'secondary' : 'outline'}
              onClick={() => setStatusFilter('3')}
            >
              Enviadas
            </Button>

            <div className="h-6 w-px bg-border mx-1" />

            <Button
              type="button"
              size="sm"
              variant={canalFilter === 'todos' ? 'default' : 'outline'}
              onClick={() => setCanalFilter('todos')}
            >
              Todos os canais
            </Button>
            {(campanha.canais || []).map((canal) => (
              <Button
                key={`filter-${canal.canal}`}
                type="button"
                size="sm"
                variant={canalFilter === String(canal.canal) ? 'secondary' : 'outline'}
                onClick={() => setCanalFilter(String(canal.canal))}
              >
                {getCanalLabel(canal.canal)}
              </Button>
            ))}
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Canal</TableHead>
                <TableHead>Destino</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Tentativas</TableHead>
                <TableHead>Erro</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {entregasFiltradas.map((entrega) => (
                <TableRow key={entrega.id}>
                  <TableCell>{getCanalLabel(entrega.canal)}</TableCell>
                  <TableCell>{entrega.destinoResolvido}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      {Number(entrega.status) === 5 ? <AlertTriangle className="w-4 h-4 text-red-500" /> : <CheckCircle2 className="w-4 h-4 text-green-500" />}
                      <span>{getEntregaStatus(entrega.status)}</span>
                    </div>
                  </TableCell>
                  <TableCell>{entrega.tentativas}</TableCell>
                  <TableCell className="whitespace-normal">{entrega.erro || '-'}</TableCell>
                </TableRow>
              ))}
              {entregasFiltradas.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                    Nenhuma entrega encontrada para o filtro atual.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
