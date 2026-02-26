import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { TrendingUp, TrendingDown, DollarSign, Calendar, PieChart, BarChart3 } from 'lucide-react';
import { dashboardFinanceiroApi } from '@/lib/api';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';

export default function DashboardFinanceiro() {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await dashboardFinanceiroApi.getDashboard();
      setDashboard(response.data);
    } catch (err) {
      console.error('Erro ao carregar dashboard financeiro:', err);
      setError('Erro ao carregar dashboard financeiro');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div>
          <Skeleton className="h-9 w-48 mb-2" />
          <Skeleton className="h-5 w-96" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <Card key={i}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-4 rounded" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-8 w-16 mb-2" />
                <Skeleton className="h-3 w-24" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadDashboard} />;
  }

  const data = dashboard || {
    totalReceitasMes: 0,
    totalDespesasMes: 0,
    saldoMes: 0,
    totalReceitasAno: 0,
    totalDespesasAno: 0,
    saldoAno: 0,
    fluxoCaixaMensal: [],
    receitasPorCategoria: [],
    despesasPorCategoria: [],
    ultimasMovimentacoes: [],
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard Financeiro</h1>
        <p className="text-muted-foreground">Visão geral das finanças</p>
      </div>

      {/* Cards de Resumo */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Receitas do Mês</CardTitle>
            <TrendingUp className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{formatCurrency(data.totalReceitasMes)}</div>
            <p className="text-xs text-muted-foreground">Total recebido este mês</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Despesas do Mês</CardTitle>
            <TrendingDown className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">{formatCurrency(data.totalDespesasMes)}</div>
            <p className="text-xs text-muted-foreground">Total pago este mês</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Saldo do Mês</CardTitle>
            <DollarSign className="h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${data.saldoMes >= 0 ? 'text-green-600' : 'text-red-600'}`}>
              {formatCurrency(data.saldoMes)}
            </div>
            <p className="text-xs text-muted-foreground">Receitas - Despesas</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Saldo do Ano</CardTitle>
            <Calendar className="h-4 w-4" />
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${data.saldoAno >= 0 ? 'text-green-600' : 'text-red-600'}`}>
              {formatCurrency(data.saldoAno)}
            </div>
            <p className="text-xs text-muted-foreground">Receitas: {formatCurrency(data.totalReceitasAno)} | Despesas: {formatCurrency(data.totalDespesasAno)}</p>
          </CardContent>
        </Card>
      </div>

      {/* Fluxo de Caixa Mensal */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            Fluxo de Caixa - Últimos 12 Meses
          </CardTitle>
        </CardHeader>
        <CardContent>
          {data.fluxoCaixaMensal.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum dado disponível</div>
          ) : (
            <div className="space-y-2">
              {data.fluxoCaixaMensal.map((item, idx) => (
                <div key={idx} className="flex items-center justify-between p-2 border rounded">
                  <div className="font-medium">{item.mesAno}</div>
                  <div className="flex items-center gap-4">
                    <span className="text-green-600">+{formatCurrency(item.totalReceitas)}</span>
                    <span className="text-red-600">-{formatCurrency(item.totalDespesas)}</span>
                    <span className={`font-bold ${item.saldo >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {formatCurrency(item.saldo)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Receitas por Categoria */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <PieChart className="h-5 w-5" />
              Receitas por Categoria (Mês)
            </CardTitle>
          </CardHeader>
          <CardContent>
            {data.receitasPorCategoria.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">Nenhuma receita este mês</div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Categoria</TableHead>
                    <TableHead className="text-right">Valor</TableHead>
                    <TableHead className="text-right">%</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.receitasPorCategoria.map((item, idx) => (
                    <TableRow key={idx}>
                      <TableCell className="font-medium">{item.categoriaNome || 'Sem categoria'}</TableCell>
                      <TableCell className="text-right text-green-600">{formatCurrency(item.total)}</TableCell>
                      <TableCell className="text-right">{item.percentual.toFixed(1)}%</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>

        {/* Despesas por Categoria */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <PieChart className="h-5 w-5" />
              Despesas por Categoria (Mês)
            </CardTitle>
          </CardHeader>
          <CardContent>
            {data.despesasPorCategoria.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">Nenhuma despesa este mês</div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Categoria</TableHead>
                    <TableHead className="text-right">Valor</TableHead>
                    <TableHead className="text-right">%</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.despesasPorCategoria.map((item, idx) => (
                    <TableRow key={idx}>
                      <TableCell className="font-medium">{item.categoriaNome || 'Sem categoria'}</TableCell>
                      <TableCell className="text-right text-red-600">{formatCurrency(item.total)}</TableCell>
                      <TableCell className="text-right">{item.percentual.toFixed(1)}%</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Últimas Movimentações */}
      <Card>
        <CardHeader>
          <CardTitle>Últimas Movimentações</CardTitle>
        </CardHeader>
        <CardContent>
          {data.ultimasMovimentacoes.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma movimentação recente</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Data</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Valor</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.ultimasMovimentacoes.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs ${item.tipo === 'Receita' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {item.tipo}
                      </span>
                    </TableCell>
                    <TableCell className="font-medium">{item.descricao}</TableCell>
                    <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell>
                      <span className="px-2 py-1 rounded text-xs bg-gray-100 text-gray-800">{item.status}</span>
                    </TableCell>
                    <TableCell className={`text-right font-bold ${item.tipo === 'Receita' ? 'text-green-600' : 'text-red-600'}`}>
                      {item.tipo === 'Receita' ? '+' : '-'}{formatCurrency(item.valor)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
