import { useEffect, useState } from 'react';
import { CalendarDays, Search } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { usePagination } from '@/hooks/usePagination';
import { pessoasApi } from '@/lib/api';

export default function Aniversariantes() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [dias, setDias] = useState('30');
  const [mes, setMes] = useState(''); // 1-12

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const diasNum = Number(dias) || 30;
      const mesNum = mes ? Number(mes) : null;
      const res = await pessoasApi.getAniversariantes(diasNum, 500, mesNum);
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar aniversariantes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(items, 20);

  if (loading) return <LoadingPage text="Carregando aniversariantes..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  const formatDate = (value) => {
    if (!value) return '-';
    const d = new Date(value);
    return d.toLocaleDateString('pt-BR');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Aniversariantes</h1>
          <p className="text-muted-foreground">Próximos aniversários das pessoas cadastradas</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtro</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4 md:items-end">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Search className="h-4 w-4" />Dias</label>
              <Input
                value={dias}
                onChange={(e) => setDias(e.target.value)}
                placeholder="Ex: 30"
                inputMode="numeric"
                disabled={!!mes}
              />
              {mes && (
                <p className="text-xs text-muted-foreground">
                  Com mês selecionado, o filtro de dias é ignorado.
                </p>
              )}
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Mês do ano</label>
              <select
                value={mes || ''}
                onChange={(e) => setMes(e.target.value)}
                className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
              >
                <option value="">Todos (próximos dias)</option>
                <option value="1">Janeiro</option>
                <option value="2">Fevereiro</option>
                <option value="3">Março</option>
                <option value="4">Abril</option>
                <option value="5">Maio</option>
                <option value="6">Junho</option>
                <option value="7">Julho</option>
                <option value="8">Agosto</option>
                <option value="9">Setembro</option>
                <option value="10">Outubro</option>
                <option value="11">Novembro</option>
                <option value="12">Dezembro</option>
              </select>
            </div>
            <div className="flex md:justify-end">
              <Button onClick={load} className="w-full md:w-auto">
                <CalendarDays className="h-4 w-4 mr-2" /> Atualizar
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Aniversariantes ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {items.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum aniversariante encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Data de Nascimento</TableHead>
                  <TableHead>Próximo Aniversário</TableHead>
                  <TableHead>Dias</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((p) => (
                  <TableRow key={p.id}>
                    <TableCell className="font-medium">{p.nome}</TableCell>
                    <TableCell>{formatDate(p.dataNascimento)}</TableCell>
                    <TableCell>{formatDate(p.proximoAniversario)}</TableCell>
                    <TableCell>{p.diasParaAniversario}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {items.length > 0 && (
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
