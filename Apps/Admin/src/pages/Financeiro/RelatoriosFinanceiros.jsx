import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { FileText, Download, Calendar } from 'lucide-react';
import { relatoriosFinanceirosApi } from '@/lib/api';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';

export default function RelatoriosFinanceiros() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [tipoRelatorio, setTipoRelatorio] = useState('fluxo-caixa');
  const [dataInicio, setDataInicio] = useState(() => {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split('T')[0];
  });
  const [dataFim, setDataFim] = useState(() => {
    return new Date().toISOString().split('T')[0];
  });
  const [relatorio, setRelatorio] = useState(null);

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString('pt-BR');
  };

  const gerarRelatorio = async () => {
    if (!dataInicio || !dataFim) {
      toast.error('Selecione as datas de início e fim');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      let response;

      switch (tipoRelatorio) {
        case 'fluxo-caixa':
          response = await relatoriosFinanceirosApi.getFluxoCaixa(dataInicio, dataFim);
          break;
        case 'por-categoria':
          response = await relatoriosFinanceirosApi.getPorCategoria(dataInicio, dataFim);
          break;
        case 'por-centro-custo':
          response = await relatoriosFinanceirosApi.getPorCentroCusto(dataInicio, dataFim);
          break;
        case 'por-projeto':
          response = await relatoriosFinanceirosApi.getPorProjeto(dataInicio, dataFim);
          break;
        default:
          throw new Error('Tipo de relatório inválido');
      }

      setRelatorio(response.data);
      toast.success('Relatório gerado com sucesso');
    } catch (err) {
      setError('Erro ao gerar relatório');
      console.error(err);
      toast.error('Erro ao gerar relatório');
    } finally {
      setLoading(false);
    }
  };

  const renderRelatorio = () => {
    if (!relatorio) return null;

    switch (tipoRelatorio) {
      case 'fluxo-caixa':
        return (
          <Card>
            <CardHeader>
              <CardTitle>Fluxo de Caixa</CardTitle>
              <p className="text-sm text-muted-foreground">
                Período: {formatDate(relatorio.dataInicio)} a {formatDate(relatorio.dataFim)}
              </p>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Total Receitas</p>
                    <p className="text-2xl font-bold text-green-600">{formatCurrency(relatorio.totalReceitas)}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Despesas</p>
                    <p className="text-2xl font-bold text-red-600">{formatCurrency(relatorio.totalDespesas)}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Saldo</p>
                    <p className={`text-2xl font-bold ${relatorio.saldo >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {formatCurrency(relatorio.saldo)}
                    </p>
                  </div>
                </div>
                {relatorio.movimentacoesDiarias && relatorio.movimentacoesDiarias.length > 0 && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Data</TableHead>
                        <TableHead className="text-right">Receitas</TableHead>
                        <TableHead className="text-right">Despesas</TableHead>
                        <TableHead className="text-right">Saldo do Dia</TableHead>
                        <TableHead className="text-right">Saldo Acumulado</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {relatorio.movimentacoesDiarias.map((item, idx) => (
                        <TableRow key={idx}>
                          <TableCell>{formatDate(item.data)}</TableCell>
                          <TableCell className="text-right text-green-600">{formatCurrency(item.receitas)}</TableCell>
                          <TableCell className="text-right text-red-600">{formatCurrency(item.despesas)}</TableCell>
                          <TableCell className={`text-right font-bold ${item.saldoDia >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {formatCurrency(item.saldoDia)}
                          </TableCell>
                          <TableCell className={`text-right font-bold ${item.saldoAcumulado >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {formatCurrency(item.saldoAcumulado)}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </div>
            </CardContent>
          </Card>
        );

      case 'por-categoria':
        return (
          <Card>
            <CardHeader>
              <CardTitle>Relatório por Categoria</CardTitle>
              <p className="text-sm text-muted-foreground">
                Período: {formatDate(dataInicio)} a {formatDate(dataFim)}
              </p>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                {relatorio.receitas && relatorio.receitas.length > 0 && (
                  <div>
                    <h3 className="text-lg font-semibold mb-3 text-green-600">Receitas por Categoria</h3>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Categoria</TableHead>
                          <TableHead className="text-right">Valor</TableHead>
                          <TableHead className="text-right">Quantidade</TableHead>
                          <TableHead className="text-right">%</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {relatorio.receitas.map((item, idx) => (
                          <TableRow key={idx}>
                            <TableCell className="font-medium">{item.categoriaNome || 'Sem categoria'}</TableCell>
                            <TableCell className="text-right text-green-600">{formatCurrency(item.valor)}</TableCell>
                            <TableCell className="text-right">{item.quantidade}</TableCell>
                            <TableCell className="text-right">{item.percentual.toFixed(1)}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                )}
                {relatorio.despesas && relatorio.despesas.length > 0 && (
                  <div>
                    <h3 className="text-lg font-semibold mb-3 text-red-600">Despesas por Categoria</h3>
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Categoria</TableHead>
                          <TableHead className="text-right">Valor</TableHead>
                          <TableHead className="text-right">Quantidade</TableHead>
                          <TableHead className="text-right">%</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {relatorio.despesas.map((item, idx) => (
                          <TableRow key={idx}>
                            <TableCell className="font-medium">{item.categoriaNome || 'Sem categoria'}</TableCell>
                            <TableCell className="text-right text-red-600">{formatCurrency(item.valor)}</TableCell>
                            <TableCell className="text-right">{item.quantidade}</TableCell>
                            <TableCell className="text-right">{item.percentual.toFixed(1)}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        );

      case 'por-centro-custo':
        return (
          <Card>
            <CardHeader>
              <CardTitle>Relatório por Centro de Custo</CardTitle>
              <p className="text-sm text-muted-foreground">
                Período: {formatDate(dataInicio)} a {formatDate(dataFim)}
              </p>
            </CardHeader>
            <CardContent>
              {relatorio.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">Nenhum dado encontrado</div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Centro de Custo</TableHead>
                      <TableHead className="text-right">Receitas</TableHead>
                      <TableHead className="text-right">Despesas</TableHead>
                      <TableHead className="text-right">Saldo</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {relatorio.map((item, idx) => (
                      <TableRow key={idx}>
                        <TableCell className="font-medium">{item.centroCusto || 'Sem centro de custo'}</TableCell>
                        <TableCell className="text-right text-green-600">{formatCurrency(item.totalReceitas)}</TableCell>
                        <TableCell className="text-right text-red-600">{formatCurrency(item.totalDespesas)}</TableCell>
                        <TableCell className={`text-right font-bold ${item.saldo >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                          {formatCurrency(item.saldo)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        );

      case 'por-projeto':
        return (
          <Card>
            <CardHeader>
              <CardTitle>Relatório por Projeto</CardTitle>
              <p className="text-sm text-muted-foreground">
                Período: {formatDate(dataInicio)} a {formatDate(dataFim)}
              </p>
            </CardHeader>
            <CardContent>
              {relatorio.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">Nenhum dado encontrado</div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Projeto</TableHead>
                      <TableHead className="text-right">Orçamento</TableHead>
                      <TableHead className="text-right">Receitas</TableHead>
                      <TableHead className="text-right">Despesas</TableHead>
                      <TableHead className="text-right">Saldo</TableHead>
                      <TableHead className="text-right">% Utilizado</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {relatorio.map((item, idx) => (
                      <TableRow key={idx}>
                        <TableCell className="font-medium">{item.projeto || 'Sem projeto'}</TableCell>
                        <TableCell className="text-right">{item.orcamento ? formatCurrency(item.orcamento) : '-'}</TableCell>
                        <TableCell className="text-right text-green-600">{formatCurrency(item.totalReceitas)}</TableCell>
                        <TableCell className="text-right text-red-600">{formatCurrency(item.totalDespesas)}</TableCell>
                        <TableCell className={`text-right font-bold ${item.saldo >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                          {formatCurrency(item.saldo)}
                        </TableCell>
                        <TableCell className="text-right">
                          {item.percentualUtilizado !== null && item.percentualUtilizado !== undefined
                            ? `${item.percentualUtilizado.toFixed(1)}%`
                            : '-'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        );

      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Relatórios Financeiros</h1>
        <p className="text-muted-foreground">Gere relatórios detalhados das finanças</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Parâmetros do Relatório
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <Label htmlFor="tipoRelatorio">Tipo de Relatório</Label>
              <Select value={tipoRelatorio} onValueChange={setTipoRelatorio}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="fluxo-caixa">Fluxo de Caixa</SelectItem>
                  <SelectItem value="por-categoria">Por Categoria</SelectItem>
                  <SelectItem value="por-centro-custo">Por Centro de Custo</SelectItem>
                  <SelectItem value="por-projeto">Por Projeto</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="dataInicio">Data Início</Label>
              <Input
                id="dataInicio"
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="dataFim">Data Fim</Label>
              <Input
                id="dataFim"
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
              />
            </div>
            <div className="space-y-2 flex items-end">
              <Button onClick={gerarRelatorio} disabled={loading} className="w-full">
                <Calendar className="h-4 w-4 mr-2" />
                {loading ? 'Gerando...' : 'Gerar Relatório'}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {loading && <LoadingPage text="Gerando relatório..." />}
      {error && <ErrorPage message={error} onRetry={gerarRelatorio} />}
      {relatorio && !loading && renderRelatorio()}
    </div>
  );
}
