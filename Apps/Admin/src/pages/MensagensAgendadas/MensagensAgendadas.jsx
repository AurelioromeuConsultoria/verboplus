import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { 
  Calendar, 
  Clock, 
  MessageSquare, 
  User, 
  Filter, 
  Search,
  CheckCircle,
  XCircle,
  AlertCircle,
  Eye,
  X
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import api from '../../lib/api';
import Loading from '../../components/ui/loading';
import ErrorMessage from '../../components/ui/error-message';

const MensagensAgendadas = () => {
  const [mensagens, setMensagens] = useState([]);
  const [visitantes, setVisitantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filtros, setFiltros] = useState({
    status: '',
    visitanteId: '',
    dataInicio: '',
    dataFim: ''
  });

  // Estados para estatísticas
  const [stats, setStats] = useState({
    total: 0,
    agendadas: 0,
    enviadas: 0,
    erro: 0
  });

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    calcularEstatisticas();
  }, [mensagens]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [mensagensResponse, visitantesResponse] = await Promise.all([
        api.get('/mensagensAgendadas'),
        api.get('/visitantes')
      ]);
      
      console.log('Dados das mensagens recebidos:', mensagensResponse.data);
      setMensagens(mensagensResponse.data);
      setVisitantes(visitantesResponse.data);
    } catch (err) {
      setError('Erro ao carregar dados');
      console.error('Erro ao buscar dados:', err);
    } finally {
      setLoading(false);
    }
  };

  const calcularEstatisticas = () => {
    const total = mensagens.length;
    const agendadas = mensagens.filter(m => m.status === 'Agendada' || m.status === 1).length;
    const enviadas = mensagens.filter(m => m.status === 'Enviada' || m.status === 2).length;
    const erro = mensagens.filter(m => m.status === 'Erro' || m.status === 3).length;

    setStats({ total, agendadas, enviadas, erro });
  };

  const handleFiltroChange = (e) => {
    const { name, value } = e.target;
    setFiltros(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const limparFiltros = () => {
    setFiltros({
      status: '',
      visitanteId: '',
      dataInicio: '',
      dataFim: ''
    });
  };

  const mensagensFiltradas = mensagens.filter(mensagem => {
    if (filtros.status && mensagem.status !== filtros.status) return false;
    if (filtros.visitanteId && mensagem.visitanteId.toString() !== filtros.visitanteId) return false;
    
    if (filtros.dataInicio) {
      const dataEnvio = new Date(mensagem.dataHoraEnvio);
      const dataInicio = new Date(filtros.dataInicio);
      if (dataEnvio < dataInicio) return false;
    }
    
    if (filtros.dataFim) {
      const dataEnvio = new Date(mensagem.dataHoraEnvio);
      const dataFim = new Date(filtros.dataFim);
      dataFim.setHours(23, 59, 59, 999); // Final do dia
      if (dataEnvio > dataFim) return false;
    }
    
    return true;
  });

  const getStatusText = (status) => {
    switch (status) {
      case 'Agendada':
      case 1:
        return 'Agendada';
      case 'Enviada':
      case 2:
        return 'Enviada';
      case 'Erro':
      case 3:
        return 'Erro';
      default:
        return `Status ${status}`;
    }
  };

  const getStatusIcon = (status) => {
    const statusText = getStatusText(status);
    switch (statusText) {
      case 'Agendada':
        return <Clock className="w-4 h-4 text-blue-500 dark:text-blue-400" />;
      case 'Enviada':
        return <CheckCircle className="w-4 h-4 text-green-500 dark:text-green-400" />;
      case 'Erro':
        return <XCircle className="w-4 h-4 text-red-500 dark:text-red-400" />;
      default:
        return <AlertCircle className="w-4 h-4 text-muted-foreground" />;
    }
  };

  const getStatusBadge = (status) => {
    const statusText = getStatusText(status);
    
    switch (statusText) {
      case 'Agendada':
        return <Badge variant="default" className="bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700">Agendada</Badge>;
      case 'Enviada':
        return <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">Enviada</Badge>;
      case 'Erro':
        return <Badge variant="destructive">Erro</Badge>;
      default:
        return <Badge variant="secondary">{statusText}</Badge>;
    }
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return 'Data não definida';
    
    const date = new Date(dateString);
    
    // Verifica se a data é válida
    if (isNaN(date.getTime())) {
      console.warn('Data inválida recebida:', dateString);
      return 'Data inválida';
    }
    
    return date.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getVisitanteNome = (visitanteId) => {
    const visitante = visitantes.find(v => v.id === visitanteId);
    return visitante ? visitante.nome : 'Visitante não encontrado';
  };

  const cancelarMensagem = async (id) => {
    if (window.confirm('Tem certeza que deseja cancelar esta mensagem?')) {
      try {
        await api.delete(`/mensagensAgendadas/${id}`);
        setMensagens(mensagens.filter(m => m.id !== id));
      } catch (err) {
        setError('Erro ao cancelar mensagem');
        console.error('Erro ao cancelar mensagem:', err);
      }
    }
  };

  if (loading) return <Loading />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-foreground">Mensagens Agendadas</h1>
        <p className="text-muted-foreground mt-1">Acompanhe o status das mensagens automáticas</p>
      </div>

      {error && <ErrorMessage message={error} />}

      {/* Cards de Estatísticas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <MessageSquare className="w-8 h-8 text-muted-foreground" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Total</p>
                <p className="text-2xl font-bold text-foreground">{stats.total}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Clock className="w-8 h-8 text-blue-500 dark:text-blue-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Agendadas</p>
                <p className="text-2xl font-bold text-blue-500 dark:text-blue-400">{stats.agendadas}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <CheckCircle className="w-8 h-8 text-green-500 dark:text-green-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Enviadas</p>
                <p className="text-2xl font-bold text-green-500 dark:text-green-400">{stats.enviadas}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <XCircle className="w-8 h-8 text-red-500 dark:text-red-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-muted-foreground">Com Erro</p>
                <p className="text-2xl font-bold text-red-500 dark:text-red-400">{stats.erro}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="w-5 h-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <label className="block text-sm font-medium text-foreground">Status</label>
              <select
                name="status"
                value={filtros.status}
                onChange={handleFiltroChange}
                className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
              >
                <option value="">Todos os status</option>
                <option value="Agendada">Agendada</option>
                <option value="Enviada">Enviada</option>
                <option value="Erro">Erro</option>
              </select>
            </div>

            <div className="space-y-2">
              <label className="block text-sm font-medium text-foreground">Visitante</label>
              <select
                name="visitanteId"
                value={filtros.visitanteId}
                onChange={handleFiltroChange}
                className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
              >
                <option value="">Todos os visitantes</option>
                {visitantes.map(visitante => (
                  <option key={visitante.id} value={visitante.id}>
                    {visitante.nome}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <label className="block text-sm font-medium text-foreground">Data Início</label>
              <input
                type="date"
                name="dataInicio"
                value={filtros.dataInicio}
                onChange={handleFiltroChange}
                className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
              />
            </div>

            <div className="space-y-2">
              <label className="block text-sm font-medium text-foreground">Data Fim</label>
              <input
                type="date"
                name="dataFim"
                value={filtros.dataFim}
                onChange={handleFiltroChange}
                className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
              />
            </div>
          </div>

          {(filtros.status || filtros.visitanteId || filtros.dataInicio || filtros.dataFim) && (
            <div className="mt-4">
              <Button
                variant="ghost"
                size="sm"
                onClick={limparFiltros}
              >
                <X className="w-4 h-4 mr-1" />
                Limpar filtros
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Tabela de Mensagens */}
      <Card>
        <CardHeader>
          <CardTitle>
            Mensagens ({mensagensFiltradas.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {mensagensFiltradas.length === 0 ? (
            <div className="text-center py-12">
              <MessageSquare className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium text-foreground mb-2">
                {mensagens.length === 0 ? 'Nenhuma mensagem encontrada' : 'Nenhuma mensagem corresponde aos filtros'}
              </h3>
              <p className="text-muted-foreground">
                {mensagens.length === 0 
                  ? 'As mensagens aparecerão aqui quando visitantes forem cadastrados.'
                  : 'Tente ajustar os filtros para encontrar as mensagens desejadas.'
                }
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Visitante</TableHead>
                    <TableHead>Mensagem</TableHead>
                    <TableHead>Data/Hora Envio</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mensagensFiltradas.map((mensagem) => (
                    <TableRow key={mensagem.id}>
                      <TableCell>
                        <div className="flex items-center">
                          <User className="w-4 h-4 text-muted-foreground mr-2" />
                          <div>
                            <div className="text-sm font-medium text-foreground">
                              {getVisitanteNome(mensagem.visitanteId)}
                            </div>
                            <div className="text-sm text-muted-foreground">
                              ID: {mensagem.visitanteId}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm text-foreground max-w-xs truncate">
                          {mensagem.textoMensagem}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center text-sm text-foreground">
                          <Calendar className="w-4 h-4 text-muted-foreground mr-2" />
                          {formatDateTime(mensagem.dataHoraEnvio)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getStatusIcon(mensagem.status)}
                          {getStatusBadge(mensagem.status)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex space-x-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            asChild
                          >
                            <Link
                              to={`/visitantes/${mensagem.visitanteId}`}
                              title="Ver visitante"
                            >
                              <Eye className="w-4 h-4" />
                            </Link>
                          </Button>
                          {mensagem.status === 'Agendada' && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => cancelarMensagem(mensagem.id)}
                              title="Cancelar mensagem"
                            >
                              <X className="w-4 h-4 text-destructive" />
                            </Button>
                          )}
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

export default MensagensAgendadas;

