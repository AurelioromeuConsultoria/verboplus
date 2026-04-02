import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRightLeft, CalendarDays, CheckCircle2, Clock3, RefreshCcw, Search, XCircle } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { equipesApi, solicitacoesTrocasEscalasApi } from '@/lib/api';
import { usePagination } from '@/hooks/usePagination';

function getStatusBadge(status) {
  const value = Number(status);

  if (value === 1) {
    return (
      <Badge className="bg-amber-100 text-amber-800 hover:bg-amber-100">
        <Clock3 className="h-3 w-3" />
        Pendente
      </Badge>
    );
  }

  if (value === 2) {
    return (
      <Badge className="bg-green-100 text-green-800 hover:bg-green-100">
        <CheckCircle2 className="h-3 w-3" />
        Aprovada
      </Badge>
    );
  }

  if (value === 3) {
    return (
      <Badge className="bg-red-100 text-red-800 hover:bg-red-100">
        <XCircle className="h-3 w-3" />
        Rejeitada
      </Badge>
    );
  }

  return <Badge variant="secondary">Cancelada</Badge>;
}

export default function SolicitacoesTrocaList() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [equipes, setEquipes] = useState([]);
  const [solicitacoes, setSolicitacoes] = useState([]);
  const [busca, setBusca] = useState('');
  const [filtroEquipeId, setFiltroEquipeId] = useState('all');
  const [filtroStatus, setFiltroStatus] = useState('all');

  const load = async (options = {}) => {
    const equipeId = options.equipeId ?? filtroEquipeId;
    const status = options.status ?? filtroStatus;

    try {
      setLoading(true);
      setError(null);

      const [equipesRes, solicitacoesRes] = await Promise.all([
        equipesApi.getAll(),
        solicitacoesTrocasEscalasApi.getAll({
          equipeId: equipeId === 'all' ? undefined : Number(equipeId),
          status: status === 'all' ? undefined : Number(status),
        }),
      ]);

      setEquipes(equipesRes.data || []);
      setSolicitacoes(solicitacoesRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar solicitações de troca');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [filtroEquipeId, filtroStatus]);

  const resumo = useMemo(() => {
    const pendentes = solicitacoes.filter((item) => Number(item.status) === 1).length;
    const aprovadas = solicitacoes.filter((item) => Number(item.status) === 2).length;
    const rejeitadas = solicitacoes.filter((item) => Number(item.status) === 3).length;

    return {
      total: solicitacoes.length,
      pendentes,
      aprovadas,
      rejeitadas,
    };
  }, [solicitacoes]);

  const filtradas = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    if (!termo) return solicitacoes;

    return solicitacoes.filter((item) =>
      (item.eventoTitulo || '').toLowerCase().includes(termo)
      || (item.equipeNome || '').toLowerCase().includes(termo)
      || (item.voluntarioSolicitanteNome || '').toLowerCase().includes(termo)
      || (item.voluntarioSubstitutoNome || '').toLowerCase().includes(termo)
      || (item.motivo || '').toLowerCase().includes(termo)
      || (item.observacaoResposta || '').toLowerCase().includes(termo)
    );
  }, [solicitacoes, busca]);

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtradas, 20);

  if (loading) return <LoadingPage text="Carregando solicitações de troca..." />;
  if (error) return <ErrorPage message={error} onRetry={() => load()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Solicitações de Troca</h1>
          <p className="text-muted-foreground">
            Acompanhe pedidos de substituição das equipes e entre rápido nas pendências mais urgentes.
          </p>
        </div>
        <Button variant="outline" onClick={() => load()}>
          <RefreshCcw className="h-4 w-4 mr-2" />
          Atualizar
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader><CardTitle>Total</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.total}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Pendentes</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-amber-600">{resumo.pendentes}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Aprovadas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-green-600">{resumo.aprovadas}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Rejeitadas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold text-red-600">{resumo.rejeitadas}</CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Buscar</Label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="pl-9"
                  value={busca}
                  onChange={(e) => setBusca(e.target.value)}
                  placeholder="Evento, equipe, voluntário ou motivo"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Equipe</Label>
              <Select value={filtroEquipeId} onValueChange={setFiltroEquipeId}>
                <SelectTrigger>
                  <SelectValue placeholder="Todas as equipes" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas as equipes</SelectItem>
                  {equipes.map((equipe) => (
                    <SelectItem key={equipe.id} value={String(equipe.id)}>
                      {equipe.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Status</Label>
              <Select value={filtroStatus} onValueChange={setFiltroStatus}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="1">Pendentes</SelectItem>
                  <SelectItem value="2">Aprovadas</SelectItem>
                  <SelectItem value="3">Rejeitadas</SelectItem>
                  <SelectItem value="4">Canceladas</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Gestão de trocas ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtradas.length === 0 ? (
            <div className="py-10 text-center text-muted-foreground">
              Nenhuma solicitação encontrada para os filtros atuais.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Evento</TableHead>
                  <TableHead>Equipe</TableHead>
                  <TableHead>Solicitante</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Substituto</TableHead>
                  <TableHead>Motivo</TableHead>
                  <TableHead>Resposta</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="font-medium">{item.eventoTitulo || `Ocorrência #${item.eventoOcorrenciaId}`}</div>
                        {item.eventoDataHoraInicio && (
                          <div className="flex items-center gap-2 text-sm text-muted-foreground">
                            <CalendarDays className="h-4 w-4" />
                            {new Date(item.eventoDataHoraInicio).toLocaleString('pt-BR')}
                          </div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{item.equipeNome}</TableCell>
                    <TableCell>{item.voluntarioSolicitanteNome}</TableCell>
                    <TableCell>{getStatusBadge(item.status)}</TableCell>
                    <TableCell>{item.voluntarioSubstitutoNome || '-'}</TableCell>
                    <TableCell className="max-w-[280px]">
                      <div className="line-clamp-3 text-sm">{item.motivo || '-'}</div>
                    </TableCell>
                    <TableCell className="max-w-[280px]">
                      <div className="text-sm">
                        {item.observacaoResposta || (Number(item.status) === 1 ? 'Aguardando análise' : '-')}
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="outline" size="sm" asChild>
                        <Link to={`/voluntariado/escalas/ocorrencia/${item.eventoOcorrenciaId}/equipe/${item.equipeId}`}>
                          <ArrowRightLeft className="h-4 w-4 mr-2" />
                          Abrir escala
                        </Link>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {filtradas.length > 0 && (
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
