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

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const diasNum = Number(dias) || 30;
      const res = await pessoasApi.getAniversariantes(diasNum, 200);
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
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Search className="h-4 w-4" />Dias</label>
              <Input
                value={dias}
                onChange={(e) => setDias(e.target.value)}
                placeholder="Ex: 30"
                inputMode="numeric"
              />
            </div>
            <div className="flex items-end">
              <Button onClick={load}>
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
