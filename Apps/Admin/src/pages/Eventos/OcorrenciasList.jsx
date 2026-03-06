import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { CalendarDays, PlusCircle, RefreshCcw, Settings } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { usePagination } from '@/hooks/usePagination';
import { eventosApi, eventosOcorrenciasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';

function getStatusOcorrenciaLabel(status) {
  const value = Number(status);
  if (value === 1) return 'Confirmado';
  if (value === 2) return 'Cancelado';
  if (value === 3) return 'Realizado';
  return 'Desconhecido';
}

export default function OcorrenciasList() {
  const { can } = useAuth();
  const [initialLoad, setInitialLoad] = useState(true);
  const [loadingOcorrencias, setLoadingOcorrencias] = useState(false);
  const [error, setError] = useState(null);
  const [eventos, setEventos] = useState([]);
  const [ocorrencias, setOcorrencias] = useState([]);

  const [filtroEventoId, setFiltroEventoId] = useState('all');
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

  const canEdit = can(RESOURCES.EVENTOS, ACTIONS.EDIT);

  const loadBase = async () => {
    try {
      const eventosRes = await eventosApi.getAll();
      setEventos(eventosRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar eventos');
    }
  };

  const loadOcorrencias = async () => {
    try {
      setLoadingOcorrencias(true);
      setError(null);
      const eventoId = filtroEventoId === 'all' ? undefined : Number(filtroEventoId);
      const res = await eventosOcorrenciasApi.getByPeriodo(
        `${dataInicio}T00:00:00`,
        `${dataFim}T23:59:59`,
        eventoId
      );
      setOcorrencias(res.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar ocorrências');
    } finally {
      setLoadingOcorrencias(false);
      setInitialLoad(false);
    }
  };

  useEffect(() => {
    loadBase();
  }, []);

  useEffect(() => {
    loadOcorrencias();
  }, [filtroEventoId, dataInicio, dataFim]);

  const handleGerarOcorrencias = async () => {
    if (filtroEventoId === 'all') {
      toast.error('Selecione um evento para gerar ocorrências');
      return;
    }

    try {
      const res = await eventosOcorrenciasApi.gerarRecorrencia(
        Number(filtroEventoId),
        `${dataInicio}T00:00:00`,
        `${dataFim}T23:59:59`
      );
      const total = res.data?.totalCriadas ?? 0;
      if (total > 0) {
        toast.success(`${total} ocorrência(s) criada(s)`);
        await loadOcorrencias();
      } else {
        toast.warning(
          'Nenhuma ocorrência nova foi criada. Verifique se o evento tem recorrências configuradas (Editar evento → Recorrências) e se a vigência cobre o período escolhido.'
        );
        await loadOcorrencias();
      }
    } catch (err) {
      console.error(err);
      const msg = err.response?.data?.message ?? err.response?.data ?? 'Erro ao gerar ocorrências';
      toast.error(typeof msg === 'string' ? msg : 'Erro ao gerar ocorrências');
    }
  };

  const sorted = [...ocorrencias].sort((a, b) => new Date(a.dataHoraInicio) - new Date(b.dataHoraInicio));
  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(sorted, 20);

  if (initialLoad) return <LoadingPage text="Carregando ocorrências..." />;
  if (error) return <ErrorPage message={error} onRetry={loadOcorrencias} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Ocorrências de Eventos</h1>
          <p className="text-muted-foreground">Gere e liste ocorrências (datas) dos eventos. Depois monte as escalas em Voluntariado → Escalas.</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={loadOcorrencias}>
            <RefreshCcw className="h-4 w-4 mr-2" />
            Atualizar
          </Button>
          {canEdit && (
            <Button onClick={handleGerarOcorrencias}>
              <PlusCircle className="h-4 w-4 mr-2" />
              Gerar Ocorrências
            </Button>
          )}
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
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
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Ocorrências ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {loadingOcorrencias ? (
            <div className="text-center py-8 text-muted-foreground">Carregando ocorrências...</div>
          ) : sorted.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma ocorrência encontrada para o período.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Evento</TableHead>
                  <TableHead>Data/Hora</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Escalas</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.eventoTitulo}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <CalendarDays className="h-4 w-4 text-muted-foreground" />
                        {new Date(item.dataHoraInicio).toLocaleString('pt-BR')}
                      </div>
                    </TableCell>
                    <TableCell>{getStatusOcorrenciaLabel(item.status)}</TableCell>
                    <TableCell>
                      {item.possuiEscala ? (
                        <span className="px-2 py-1 rounded text-xs bg-green-100 text-green-800">Há escala(s)</span>
                      ) : (
                        <span className="px-2 py-1 rounded text-xs bg-gray-100 text-gray-800">Nenhuma</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="outline" size="sm" asChild>
                        <Link to={`/voluntariado/escalas/ocorrencia/${item.id}`}>
                          <Settings className="h-4 w-4 mr-2" />
                          Montar escalas
                        </Link>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {sorted.length > 0 && (
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
    </div>
  );
}
