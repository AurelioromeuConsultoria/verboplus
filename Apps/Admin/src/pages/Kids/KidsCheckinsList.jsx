import React, { useState, useEffect } from 'react';
import { 
  Calendar, 
  Clock, 
  User, 
  Filter, 
  Search,
  CheckCircle,
  XCircle,
  LogIn,
  LogOut,
  Users,
  TrendingUp
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { kidsApi } from '../../lib/api';
import Loading from '../../components/ui/loading';
import ErrorMessage from '../../components/ui/error-message';
import { toast } from 'sonner';

const KidsCheckinsList = () => {
  const [checkins, setCheckins] = useState([]);
  const [criancas, setCriancas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filtros, setFiltros] = useState({
    criancaPessoaId: '',
    status: '',
    dataInicio: '',
    dataFim: '',
    busca: ''
  });

  // Estados para estatísticas
  const [stats, setStats] = useState({
    total: 0,
    ativos: 0,
    finalizados: 0,
    hoje: 0
  });

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    calcularEstatisticas();
  }, [checkins]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [checkinsResponse, criancasResponse] = await Promise.all([
        kidsApi.getCheckins(),
        kidsApi.getCriancas()
      ]);
      
      setCheckins(checkinsResponse.data);
      setCriancas(criancasResponse.data);
    } catch (err) {
      setError('Erro ao carregar dados');
      console.error('Erro ao buscar dados:', err);
      toast.error('Erro ao carregar check-ins');
    } finally {
      setLoading(false);
    }
  };

  const calcularEstatisticas = () => {
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    
    const total = checkins.length;
    const ativos = checkins.filter(c => c.status === 'CHECKED_IN' || c.status === 'Ativo').length;
    const finalizados = checkins.filter(c => c.status === 'CHECKED_OUT' || c.status === 'Finalizado').length;
    const hojeCount = checkins.filter(c => {
      const checkinDate = new Date(c.checkinTime);
      checkinDate.setHours(0, 0, 0, 0);
      return checkinDate.getTime() === hoje.getTime();
    }).length;

    setStats({ total, ativos, finalizados, hoje: hojeCount });
  };

  const handleFiltroChange = (name, value) => {
    setFiltros(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const limparFiltros = () => {
    setFiltros({
      criancaPessoaId: '',
      status: '',
      dataInicio: '',
      dataFim: '',
      busca: ''
    });
  };

  const checkinsFiltrados = checkins.filter(checkin => {
    // Filtro por criança
    if (filtros.criancaPessoaId && checkin.criancaPessoaId !== parseInt(filtros.criancaPessoaId)) {
      return false;
    }

    // Filtro por status
    if (filtros.status) {
      if (filtros.status === 'ativo' && checkin.status !== 'CHECKED_IN' && checkin.status !== 'Ativo') {
        return false;
      }
      if (filtros.status === 'finalizado' && checkin.status !== 'CHECKED_OUT' && checkin.status !== 'Finalizado') {
        return false;
      }
    }

    // Filtro por data de início
    if (filtros.dataInicio) {
      const dataInicio = new Date(filtros.dataInicio);
      dataInicio.setHours(0, 0, 0, 0);
      const checkinDate = new Date(checkin.checkinTime);
      checkinDate.setHours(0, 0, 0, 0);
      if (checkinDate < dataInicio) {
        return false;
      }
    }

    // Filtro por data de fim
    if (filtros.dataFim) {
      const dataFim = new Date(filtros.dataFim);
      dataFim.setHours(23, 59, 59, 999);
      const checkinDate = new Date(checkin.checkinTime);
      if (checkinDate > dataFim) {
        return false;
      }
    }

    // Filtro por busca (nome da criança)
    if (filtros.busca) {
      const buscaLower = filtros.busca.toLowerCase();
      if (!checkin.criancaNome?.toLowerCase().includes(buscaLower)) {
        return false;
      }
    }

    return true;
  });

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatDuration = (checkinTime, checkoutTime) => {
    if (!checkoutTime) return '-';
    const inicio = new Date(checkinTime);
    const fim = new Date(checkoutTime);
    const diff = fim - inicio;
    const horas = Math.floor(diff / (1000 * 60 * 60));
    const minutos = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    return `${horas}h ${minutos}m`;
  };

  const getStatusBadge = (status) => {
    const statusLower = status?.toLowerCase() || '';
    if (statusLower === 'checked_in' || statusLower === 'ativo') {
      return <Badge className="bg-green-500 hover:bg-green-600">Ativo</Badge>;
    }
    if (statusLower === 'checked_out' || statusLower === 'finalizado') {
      return <Badge className="bg-gray-500 hover:bg-gray-600">Finalizado</Badge>;
    }
    return <Badge variant="secondary">{status || 'Desconhecido'}</Badge>;
  };

  const getMetodoBadge = (metodo) => {
    const metodoLower = metodo?.toLowerCase() || '';
    const cores = {
      'qr': 'bg-blue-500',
      'pin': 'bg-purple-500',
      'admin': 'bg-orange-500'
    };
    const cor = cores[metodoLower] || 'bg-gray-500';
    return <Badge className={cor}>{metodo?.toUpperCase() || 'N/A'}</Badge>;
  };

  if (loading) {
    return <Loading />;
  }

  if (error) {
    return <ErrorMessage message={error} />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Check-ins Kids</h1>
          <p className="text-muted-foreground mt-1">
            Histórico de check-ins e check-outs das crianças
          </p>
        </div>
      </div>

      {/* Estatísticas */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total de Check-ins
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-foreground">{stats.total}</div>
            <p className="text-xs text-muted-foreground">
              Registros no sistema
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Check-ins Ativos
            </CardTitle>
            <LogIn className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-500">{stats.ativos}</div>
            <p className="text-xs text-muted-foreground">
              Crianças no local
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Check-outs Realizados
            </CardTitle>
            <LogOut className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-500">{stats.finalizados}</div>
            <p className="text-xs text-muted-foreground">
              Sessões finalizadas
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Check-ins Hoje
            </CardTitle>
            <Calendar className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-500">{stats.hoje}</div>
            <p className="text-xs text-muted-foreground">
              Registros de hoje
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground">Buscar</label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Nome da criança..."
                  value={filtros.busca}
                  onChange={(e) => handleFiltroChange('busca', e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground">Criança</label>
              <Select
                value={filtros.criancaPessoaId || 'all'}
                onValueChange={(value) => handleFiltroChange('criancaPessoaId', value === 'all' ? '' : value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todas as crianças" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas as crianças</SelectItem>
                  {criancas.map((crianca) => (
                    <SelectItem key={crianca.pessoaId} value={crianca.pessoaId.toString()}>
                      {crianca.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground">Status</label>
              <Select
                value={filtros.status}
                onValueChange={(value) => handleFiltroChange('status', value === 'all' ? '' : value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos os status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os status</SelectItem>
                  <SelectItem value="ativo">Ativo</SelectItem>
                  <SelectItem value="finalizado">Finalizado</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground">Data Início</label>
              <Input
                type="date"
                value={filtros.dataInicio}
                onChange={(e) => handleFiltroChange('dataInicio', e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-foreground">Data Fim</label>
              <Input
                type="date"
                value={filtros.dataFim}
                onChange={(e) => handleFiltroChange('dataFim', e.target.value)}
              />
            </div>
          </div>

          <div className="mt-4 flex justify-end">
            <Button variant="outline" onClick={limparFiltros}>
              Limpar Filtros
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tabela */}
      <Card>
        <CardHeader>
          <CardTitle>
            Histórico de Check-ins ({checkinsFiltrados.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {checkinsFiltrados.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              Nenhum check-in encontrado com os filtros aplicados.
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Criança</TableHead>
                    <TableHead>Check-in</TableHead>
                    <TableHead>Check-out</TableHead>
                    <TableHead>Duração</TableHead>
                    <TableHead>Método</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Check-in por</TableHead>
                    <TableHead>Check-out por</TableHead>
                    <TableHead>Observações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {checkinsFiltrados.map((checkin) => (
                    <TableRow key={checkin.id}>
                      <TableCell className="font-medium">
                        {checkin.criancaNome || `ID: ${checkin.criancaPessoaId}`}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <LogIn className="h-4 w-4 text-green-500" />
                          <span>{formatDate(checkin.checkinTime)}</span>
                        </div>
                      </TableCell>
                      <TableCell>
                        {checkin.checkoutTime ? (
                          <div className="flex items-center gap-2">
                            <LogOut className="h-4 w-4 text-gray-500" />
                            <span>{formatDate(checkin.checkoutTime)}</span>
                          </div>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {formatDuration(checkin.checkinTime, checkin.checkoutTime)}
                      </TableCell>
                      <TableCell>
                        {getMetodoBadge(checkin.metodo)}
                      </TableCell>
                      <TableCell>
                        {getStatusBadge(checkin.status)}
                      </TableCell>
                      <TableCell>
                        {checkin.checkinByNome || '-'}
                      </TableCell>
                      <TableCell>
                        {checkin.checkoutByNome || '-'}
                      </TableCell>
                      <TableCell>
                        <div className="max-w-xs truncate" title={checkin.observacoes}>
                          {checkin.observacoes || '-'}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default KidsCheckinsList;
