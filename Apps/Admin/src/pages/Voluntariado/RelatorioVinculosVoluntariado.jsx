import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRightLeft, RefreshCcw } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { pessoasApi, usuariosApi, voluntariosApi } from '@/lib/api';
import { usePagination } from '@/hooks/usePagination';

function getStatusVinculo(temUsuario, qtdVoluntario) {
  if (temUsuario && qtdVoluntario > 0) return 'Usuário + Voluntário';
  if (temUsuario && qtdVoluntario === 0) return 'Usuário sem vínculo de voluntário';
  if (!temUsuario && qtdVoluntario > 0) return 'Voluntário sem usuário';
  return 'Somente pessoa';
}

export default function RelatorioVinculosVoluntariado() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [filtroStatus, setFiltroStatus] = useState('all');
  const [linhas, setLinhas] = useState([]);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);

      const [pessoasRes, usuariosRes, voluntariosRes] = await Promise.all([
        pessoasApi.getAll(),
        usuariosApi.getAll(),
        voluntariosApi.getAll(),
      ]);

      const pessoas = pessoasRes.data || [];
      const usuarios = usuariosRes.data || [];
      const voluntarios = voluntariosRes.data || [];

      const usuarioByPessoaId = new Map(usuarios.map((u) => [u.pessoaId, u]));
      const voluntariosByPessoaId = voluntarios.reduce((acc, item) => {
        const list = acc.get(item.pessoaId) || [];
        list.push(item);
        acc.set(item.pessoaId, list);
        return acc;
      }, new Map());

      const resultado = pessoas.map((p) => {
        const usuario = usuarioByPessoaId.get(p.id);
        const voluntariosDaPessoa = voluntariosByPessoaId.get(p.id) || [];
        const statusVinculo = getStatusVinculo(!!usuario, voluntariosDaPessoa.length);

        return {
          pessoaId: p.id,
          nomePessoa: p.nome || '-',
          emailPessoa: p.email || '-',
          temUsuario: !!usuario,
          emailLogin: usuario?.emailLogin || '-',
          tipoUsuario: usuario?.tipoUsuarioDescricao || '-',
          ativoUsuario: usuario?.ativo ?? null,
          qtdVinculosVoluntario: voluntariosDaPessoa.length,
          equipesVoluntario: voluntariosDaPessoa.map((v) => v.nomeEquipe).filter(Boolean).join(', ') || '-',
          cargosVoluntario: voluntariosDaPessoa.map((v) => v.nomeCargo).filter(Boolean).join(', ') || '-',
          statusVinculo,
        };
      });

      setLinhas(resultado);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar relatório de vínculos');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const resumo = useMemo(() => {
    const total = linhas.length;
    const comUsuario = linhas.filter((l) => l.temUsuario).length;
    const comVoluntario = linhas.filter((l) => l.qtdVinculosVoluntario > 0).length;
    const comAmbos = linhas.filter((l) => l.temUsuario && l.qtdVinculosVoluntario > 0).length;
    return { total, comUsuario, comVoluntario, comAmbos };
  }, [linhas]);

  const filtradas = useMemo(() => {
    return linhas.filter((linha) => {
      const matchBusca = !busca
        || linha.nomePessoa.toLowerCase().includes(busca.toLowerCase())
        || linha.emailPessoa.toLowerCase().includes(busca.toLowerCase())
        || linha.emailLogin.toLowerCase().includes(busca.toLowerCase());

      if (!matchBusca) return false;

      if (filtroStatus === 'com-usuario' && !linha.temUsuario) return false;
      if (filtroStatus === 'sem-usuario' && linha.temUsuario) return false;
      if (filtroStatus === 'com-voluntario' && linha.qtdVinculosVoluntario === 0) return false;
      if (filtroStatus === 'sem-voluntario' && linha.qtdVinculosVoluntario > 0) return false;
      if (filtroStatus === 'com-ambos' && !(linha.temUsuario && linha.qtdVinculosVoluntario > 0)) return false;

      return true;
    });
  }, [linhas, busca, filtroStatus]);

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(filtradas, 20);

  if (loading) return <LoadingPage text="Carregando relatório..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Relatório Usuários x Pessoas x Voluntários</h1>
          <p className="text-muted-foreground">
            Visão consolidada dos vínculos entre pessoa, acesso ao sistema e voluntariado
          </p>
        </div>
        <Button variant="outline" onClick={load}>
          <RefreshCcw className="h-4 w-4 mr-2" />
          Atualizar
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader><CardTitle>Total de Pessoas</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.total}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Com Usuário</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.comUsuario}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Com Voluntário</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.comVoluntario}</CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Com Ambos</CardTitle></CardHeader>
          <CardContent className="text-2xl font-bold">{resumo.comAmbos}</CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="text-sm font-medium">Buscar</label>
              <Input
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Nome, email da pessoa ou email de login"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Status</label>
              <Select value={filtroStatus} onValueChange={setFiltroStatus}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="com-usuario">Com usuário</SelectItem>
                  <SelectItem value="sem-usuario">Sem usuário</SelectItem>
                  <SelectItem value="com-voluntario">Com voluntário</SelectItem>
                  <SelectItem value="sem-voluntario">Sem voluntário</SelectItem>
                  <SelectItem value="com-ambos">Com usuário e voluntário</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Vínculos ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtradas.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum registro encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Pessoa</TableHead>
                  <TableHead>Email Pessoa</TableHead>
                  <TableHead>Usuário</TableHead>
                  <TableHead>Email Login</TableHead>
                  <TableHead>Tipo Usuário</TableHead>
                  <TableHead>Vínculos Voluntário</TableHead>
                  <TableHead>Equipes</TableHead>
                  <TableHead>Cargos</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((linha) => (
                  <TableRow key={linha.pessoaId}>
                    <TableCell className="font-medium">
                      <Link to={`/pessoas/${linha.pessoaId}`} className="hover:underline">
                        {linha.nomePessoa}
                      </Link>
                    </TableCell>
                    <TableCell>{linha.emailPessoa}</TableCell>
                    <TableCell>{linha.temUsuario ? 'Sim' : 'Não'}</TableCell>
                    <TableCell>{linha.emailLogin}</TableCell>
                    <TableCell>{linha.tipoUsuario}</TableCell>
                    <TableCell>{linha.qtdVinculosVoluntario}</TableCell>
                    <TableCell>{linha.equipesVoluntario}</TableCell>
                    <TableCell>{linha.cargosVoluntario}</TableCell>
                    <TableCell>
                      <div className="inline-flex items-center gap-1 rounded px-2 py-1 text-xs bg-muted">
                        <ArrowRightLeft className="h-3 w-3" />
                        {linha.statusVinculo}
                      </div>
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
