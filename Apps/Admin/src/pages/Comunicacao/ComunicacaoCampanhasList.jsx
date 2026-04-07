import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Radio, Send, AlertTriangle } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { comunicacaoCampanhasApi } from '@/lib/api';
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

export default function ComunicacaoCampanhasList() {
  const [campanhas, setCampanhas] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async ({ silent = false } = {}) => {
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const [campanhasResponse, statsResponse] = await Promise.all([
        comunicacaoCampanhasApi.getPaged({ page: 1, pageSize: 20 }),
        comunicacaoCampanhasApi.getStats(),
      ]);

      setCampanhas(campanhasResponse.data?.items || []);
      setStats(statsResponse.data || null);
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar campanhas de comunicação');
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

  if (loading) return <LoadingPage text="Carregando campanhas de comunicação..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Campanhas de Comunicação</h1>
          <p className="text-muted-foreground mt-1">Gerencie campanhas omnichannel e acompanhe o status do núcleo de entregas.</p>
        </div>

        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          <Button variant="outline" asChild>
            <Link to="/comunicacao/templates">Templates</Link>
          </Button>
          <Button asChild>
            <Link to="/comunicacao/campanhas/nova">
              <Plus className="w-4 h-4 mr-2" />
              Nova Campanha
            </Link>
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Total</div><div className="text-2xl font-bold">{stats?.totalCampanhas ?? 0}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Rascunhos</div><div className="text-2xl font-bold">{stats?.campanhasRascunho ?? 0}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Pendentes</div><div className="text-2xl font-bold">{stats?.entregasPendentes ?? 0}</div></CardContent></Card>
        <Card><CardContent className="p-5"><div className="text-sm text-muted-foreground">Falhas</div><div className="text-2xl font-bold">{stats?.entregasComFalha ?? 0}</div></CardContent></Card>
      </div>

      {campanhas.length === 0 ? (
        <PageEmptyState
          title="Nenhuma campanha cadastrada"
          description="Comece criando a primeira campanha do módulo de comunicação."
          action={(
            <Button asChild>
              <Link to="/comunicacao/campanhas/nova">Criar campanha</Link>
            </Button>
          )}
        />
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
          {campanhas.map((campanha) => (
            <Card key={campanha.id}>
              <CardContent className="p-6 space-y-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <Link to={`/comunicacao/campanhas/${campanha.id}`} className="text-lg font-semibold text-foreground hover:underline">
                      {campanha.nome}
                    </Link>
                    <p className="text-sm text-muted-foreground">{campanha.objetivo} • {campanha.publicoAlvo}</p>
                  </div>
                  <Badge variant="secondary">{getStatusLabel(campanha.status)}</Badge>
                </div>

                <div className="grid grid-cols-3 gap-3 text-sm">
                  <div className="rounded-lg border border-border p-3">
                    <div className="flex items-center gap-2 text-muted-foreground"><Radio className="w-4 h-4" /> Entregas</div>
                    <div className="text-xl font-semibold mt-1">{campanha.totalEntregas}</div>
                  </div>
                  <div className="rounded-lg border border-border p-3">
                    <div className="flex items-center gap-2 text-muted-foreground"><Send className="w-4 h-4" /> Agendamento</div>
                    <div className="text-sm font-medium mt-1">{campanha.dataAgendamento ? new Date(campanha.dataAgendamento).toLocaleString('pt-BR') : 'Imediato / rascunho'}</div>
                  </div>
                  <div className="rounded-lg border border-border p-3">
                    <div className="flex items-center gap-2 text-muted-foreground"><AlertTriangle className="w-4 h-4" /> Falhas</div>
                    <div className="text-xl font-semibold mt-1">{campanha.totalFalhas}</div>
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/comunicacao/campanhas/${campanha.id}`}>Abrir detalhes</Link>
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
