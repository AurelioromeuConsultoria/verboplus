import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { AlertTriangle, CalendarDays, CheckCircle2, Clock3, Filter, Settings, XCircle } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { usePagination } from '@/hooks/usePagination';
import { eventosApi, eventosOcorrenciasApi } from '@/lib/api';
import { escalasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';

function getRiskClassName(nivelRisco) {
  if (nivelRisco === 'high') return 'bg-red-100 text-red-800 hover:bg-red-100';
  if (nivelRisco === 'attention') return 'bg-amber-100 text-amber-800 hover:bg-amber-100';
  if (nivelRisco === 'none') return 'bg-slate-100 text-slate-800 hover:bg-slate-100';
  return 'bg-green-100 text-green-800 hover:bg-green-100';
}

export default function PainelCoberturaVoluntariado() {
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [eventos, setEventos] = useState([]);
  const [cards, setCards] = useState([]);
  const [filtroEventoId, setFiltroEventoId] = useState('all');
  const [filtroRisco, setFiltroRisco] = useState('all');
  const [busca, setBusca] = useState('');
  const [dataInicio, setDataInicio] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - 7);
    return d.toISOString().slice(0, 10);
  });
  const [dataFim, setDataFim] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 30);
    return d.toISOString().slice(0, 10);
  });

  const load = async (options = {}) => {
    const silent = options.silent ?? false;
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const eventoId = filtroEventoId === 'all' ? undefined : Number(filtroEventoId);
      const [eventosRes, ocorrenciasRes] = await Promise.all([
        eventosApi.getAll(),
        eventosOcorrenciasApi.getCoberturaVoluntariado({
          dataInicio: `${dataInicio}T00:00:00`,
          dataFim: `${dataFim}T23:59:59`,
          eventoId,
          nivelRisco: filtroRisco === 'all' ? undefined : filtroRisco,
        }),
      ]);

      setEventos(eventosRes.data || []);
      setCards(ocorrenciasRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar painel de cobertura');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    load();
  }, [filtroEventoId, filtroRisco, dataInicio, dataFim]);

  const resumoGeral = useMemo(() => {
    return cards.reduce((acc, item) => {
      acc.ocorrencias += 1;
      acc.vagas += item.totalVagas;
      acc.confirmados += item.confirmados;
      acc.pendentes += item.pendentes;
      acc.recusados += item.recusados;
      if (item.nivelRisco === 'high') acc.riscoAlto += 1;
      if (item.nivelRisco === 'attention') acc.atencao += 1;
      if (item.nivelRisco === 'none') acc.semEscala += 1;
      return acc;
    }, {
      ocorrencias: 0,
      vagas: 0,
      confirmados: 0,
      pendentes: 0,
      recusados: 0,
      riscoAlto: 0,
      atencao: 0,
      semEscala: 0,
    });
  }, [cards]);

  const filtered = useMemo(() => {
    return cards
      .filter((item) => !busca.trim() || item.eventoTitulo?.toLowerCase().includes(busca.trim().toLowerCase()))
      .sort((a, b) => {
        if (a.ordemRisco !== b.ordemRisco) return a.ordemRisco - b.ordemRisco;
        return new Date(a.dataHoraInicio) - new Date(b.dataHoraInicio);
      });
  }, [busca, cards]);

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtered, 12);

  const handleProcessarLembretes = async () => {
    try {
      const res = await escalasApi.processarLembretes();
      const totalEnviados = res.data?.totalEnviados ?? 0;
      toast.success(totalEnviados > 0 ? `${totalEnviados} lembrete(s) enviados` : 'Nenhum lembrete pendente no momento');
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao processar lembretes');
      toast.error(message);
    }
  };

  if (loading) return <LoadingPage text="Carregando painel de cobertura..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Painel de Cobertura</h1>
          <p className="text-muted-foreground">
            Visão consolidada das ocorrências com mais risco operacional no voluntariado.
          </p>
        </div>
        <div className="flex items-center gap-2">
          {isAdmin && (
            <Button variant="outline" onClick={handleProcessarLembretes}>
              Processar lembretes
            </Button>
          )}
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-4 xl:grid-cols-7">
        <Card>
          <CardHeader><CardTitle>Ocorrências</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumoGeral.ocorrencias}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Risco alto</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-red-600">{resumoGeral.riscoAlto}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Atenção</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-amber-600">{resumoGeral.atencao}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Sem escala</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-slate-600">{resumoGeral.semEscala}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Vagas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumoGeral.vagas}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Pendentes</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-amber-600">{resumoGeral.pendentes}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Recusas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-red-600">{resumoGeral.recusados}</CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-4 w-4" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <Label>Evento</Label>
              <Select value={filtroEventoId} onValueChange={setFiltroEventoId}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os eventos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os eventos</SelectItem>
                  {eventos.map((evento) => (
                    <SelectItem key={evento.id} value={String(evento.id)}>
                      {evento.titulo}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Data início</Label>
              <Input type="date" value={dataInicio} onChange={(e) => setDataInicio(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Data fim</Label>
              <Input type="date" value={dataFim} onChange={(e) => setDataFim(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Risco</Label>
              <Select value={filtroRisco} onValueChange={setFiltroRisco}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="high">Risco alto</SelectItem>
                  <SelectItem value="attention">Atenção</SelectItem>
                  <SelectItem value="none">Sem escala</SelectItem>
                  <SelectItem value="ok">Coberta</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="pt-4">
            <Label>Buscar ocorrência</Label>
            <Input
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="Digite o nome do evento"
            />
          </div>
        </CardContent>
      </Card>

      <div className="space-y-4">
        {paginatedItems.length === 0 ? (
          <Card>
            <CardContent>
              <PageEmptyState
                title="Nenhuma ocorrencia encontrada"
                description="Nao ha ocorrencias no periodo ou com o nivel de risco selecionado. Ajuste os filtros para ampliar o painel."
              />
            </CardContent>
          </Card>
        ) : (
          paginatedItems.map((item) => (
            <Card key={item.ocorrenciaId}>
              <CardHeader>
                <CardTitle className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                  <div>
                    <div>{item.eventoTitulo}</div>
                    <div className="mt-1 flex items-center gap-2 text-sm font-normal text-muted-foreground">
                      <CalendarDays className="h-4 w-4" />
                      {new Date(item.dataHoraInicio).toLocaleString('pt-BR')}
                    </div>
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge className={getRiskClassName(item.nivelRisco)}>{item.rotuloRisco}</Badge>
                    <Button variant="outline" size="sm" asChild>
                      <Link to={`/voluntariado/escalas/ocorrencia/${item.ocorrenciaId}`}>
                        <Settings className="h-4 w-4 mr-2" />
                        Abrir ocorrência
                      </Link>
                    </Button>
                  </div>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex flex-wrap gap-3 text-sm">
                  <span className="inline-flex items-center gap-1"><CheckCircle2 className="h-4 w-4 text-green-600" /> {item.confirmados} confirmados</span>
                  <span className="inline-flex items-center gap-1"><Clock3 className="h-4 w-4 text-amber-600" /> {item.pendentes} pendentes</span>
                  <span className="inline-flex items-center gap-1"><XCircle className="h-4 w-4 text-red-600" /> {item.recusados} recusas</span>
                  <span className="inline-flex items-center gap-1"><AlertTriangle className="h-4 w-4 text-slate-600" /> {item.substituidos} substituições</span>
                </div>

                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                  {item.equipes.map((equipe) => (
                    <div key={equipe.equipeId} className="rounded-lg border p-3">
                      <div className="font-medium">{equipe.equipeNome}</div>
                      <div className="mt-2 flex flex-wrap gap-2 text-xs text-muted-foreground">
                        <span>{equipe.totalVagas} vagas</span>
                        <span>{equipe.confirmados} confirmados</span>
                        <span>{equipe.pendentes} pendentes</span>
                        <span>{equipe.recusados} recusas</span>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>

      {filtered.length > 0 && (
        <DataTablePagination
          page={page}
          pageSize={pageSize}
          total={total}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
      )}
    </div>
  );
}
