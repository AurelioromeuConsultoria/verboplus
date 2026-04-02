import { useEffect, useMemo, useState } from 'react';
import { RefreshCcw, ShieldAlert, UserCheck } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { equipesApi, escalasApi, eventosApi } from '@/lib/api';
import { usePagination } from '@/hooks/usePagination';

export default function HistoricoVoluntarios() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [equipes, setEquipes] = useState([]);
  const [eventos, setEventos] = useState([]);
  const [registros, setRegistros] = useState([]);
  const [busca, setBusca] = useState('');
  const [equipeId, setEquipeId] = useState('all');
  const [eventoId, setEventoId] = useState('all');
  const [dataInicio, setDataInicio] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 6);
    return d.toISOString().slice(0, 10);
  });
  const [dataFim, setDataFim] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() + 1);
    return d.toISOString().slice(0, 10);
  });

  const load = async () => {
    try {
      setLoading(true);
      setError(null);

      const params = {
        equipeId: equipeId === 'all' ? undefined : Number(equipeId),
        eventoId: eventoId === 'all' ? undefined : Number(eventoId),
        dataInicio: `${dataInicio}T00:00:00`,
        dataFim: `${dataFim}T23:59:59`,
      };

      const [equipesRes, eventosRes, historicoRes] = await Promise.all([
        equipesApi.getAll(),
        eventosApi.getAll(),
        escalasApi.getHistoricoVoluntarios(params),
      ]);

      setEquipes(equipesRes.data || []);
      setEventos(eventosRes.data || []);
      setRegistros(historicoRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar histórico operacional');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [equipeId, eventoId, dataInicio, dataFim]);

  const resumo = useMemo(() => {
    return registros.reduce((acc, item) => {
      acc.voluntarios += 1;
      acc.presencas += item.presencas;
      acc.faltas += item.faltas;
      acc.pendentes += item.pendentes;
      return acc;
    }, {
      voluntarios: 0,
      presencas: 0,
      faltas: 0,
      pendentes: 0,
    });
  }, [registros]);

  const filtrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    return registros.filter((item) => {
      if (!termo) return true;
      return item.voluntarioNome?.toLowerCase().includes(termo)
        || item.equipes?.join(', ').toLowerCase().includes(termo);
    });
  }, [registros, busca]);

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtrados, 15);

  if (loading) return <LoadingPage text="Carregando histórico do voluntariado..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Histórico Operacional</h1>
          <p className="text-muted-foreground">
            Acompanhe presença, faltas, pendências e carga recente dos voluntários.
          </p>
        </div>
        <Button variant="outline" onClick={load}>
          <RefreshCcw className="h-4 w-4 mr-2" />
          Atualizar
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader><CardTitle>Voluntários</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.voluntarios}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Presenças</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-green-600">{resumo.presencas}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Faltas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-red-600">{resumo.faltas}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Pendências</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-amber-600">{resumo.pendentes}</CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-5">
          <div className="space-y-2 md:col-span-2">
            <Label>Buscar</Label>
            <Input value={busca} onChange={(e) => setBusca(e.target.value)} placeholder="Nome do voluntário ou equipe" />
          </div>
          <div className="space-y-2">
            <Label>Equipe</Label>
            <Select value={equipeId} onValueChange={setEquipeId}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas</SelectItem>
                {equipes.map((equipe) => (
                  <SelectItem key={equipe.id} value={String(equipe.id)}>{equipe.nome}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Evento</Label>
            <Select value={eventoId} onValueChange={setEventoId}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                {eventos.map((evento) => (
                  <SelectItem key={evento.id} value={String(evento.id)}>{evento.titulo}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid gap-4 md:grid-cols-2 md:col-span-5">
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
          <CardTitle>Voluntários ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtrados.length === 0 ? (
            <div className="py-10 text-center text-muted-foreground">Nenhum voluntário encontrado para os filtros atuais.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Voluntário</TableHead>
                  <TableHead>Equipes</TableHead>
                  <TableHead>Total</TableHead>
                  <TableHead>Presenças</TableHead>
                  <TableHead>Faltas</TableHead>
                  <TableHead>Pendentes</TableHead>
                  <TableHead>Carga no mês</TableHead>
                  <TableHead>Última escala</TableHead>
                  <TableHead>Próxima escala</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((item) => (
                  <TableRow key={item.pessoaId}>
                    <TableCell className="font-medium">
                      <div className="flex items-center gap-2">
                        {item.faltas > 0 ? <ShieldAlert className="h-4 w-4 text-red-600" /> : <UserCheck className="h-4 w-4 text-green-600" />}
                        {item.voluntarioNome}
                      </div>
                    </TableCell>
                    <TableCell>{item.equipes?.join(', ') || '-'}</TableCell>
                    <TableCell>{item.totalEscalas}</TableCell>
                    <TableCell className="text-green-600">{item.presencas}</TableCell>
                    <TableCell className="text-red-600">{item.faltas}</TableCell>
                    <TableCell className="text-amber-600">{item.pendentes}</TableCell>
                    <TableCell>{item.cargaMesAtual}</TableCell>
                    <TableCell>{item.ultimaEscalaEm ? new Date(item.ultimaEscalaEm).toLocaleString('pt-BR') : '-'}</TableCell>
                    <TableCell>{item.proximaEscalaEm ? new Date(item.proximaEscalaEm).toLocaleString('pt-BR') : '-'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          {filtrados.length > 0 && (
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
