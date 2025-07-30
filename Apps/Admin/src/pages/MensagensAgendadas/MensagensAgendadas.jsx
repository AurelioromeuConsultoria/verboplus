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
        return <Clock className="w-4 h-4 text-blue-600" />;
      case 'Enviada':
        return <CheckCircle className="w-4 h-4 text-green-600" />;
      case 'Erro':
        return <XCircle className="w-4 h-4 text-red-600" />;
      default:
        return <AlertCircle className="w-4 h-4 text-gray-600" />;
    }
  };

  const getStatusBadge = (status) => {
    const baseClasses = "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium";
    const statusText = getStatusText(status);
    
    switch (statusText) {
      case 'Agendada':
        return `${baseClasses} bg-blue-100 text-blue-800`;
      case 'Enviada':
        return `${baseClasses} bg-green-100 text-green-800`;
      case 'Erro':
        return `${baseClasses} bg-red-100 text-red-800`;
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`;
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
        <h1 className="text-3xl font-bold text-gray-900">Mensagens Agendadas</h1>
        <p className="text-gray-600 mt-1">Acompanhe o status das mensagens automáticas</p>
      </div>

      {error && <ErrorMessage message={error} />}

      {/* Cards de Estatísticas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <MessageSquare className="w-8 h-8 text-gray-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total</p>
              <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <Clock className="w-8 h-8 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Agendadas</p>
              <p className="text-2xl font-bold text-blue-600">{stats.agendadas}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <CheckCircle className="w-8 h-8 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Enviadas</p>
              <p className="text-2xl font-bold text-green-600">{stats.enviadas}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <XCircle className="w-8 h-8 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Com Erro</p>
              <p className="text-2xl font-bold text-red-600">{stats.erro}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Filtros */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="flex items-center space-x-2 mb-4">
          <Filter className="w-5 h-5 text-gray-600" />
          <h3 className="text-lg font-medium text-gray-900">Filtros</h3>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              name="status"
              value={filtros.status}
              onChange={handleFiltroChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Todos os status</option>
              <option value="Agendada">Agendada</option>
              <option value="Enviada">Enviada</option>
              <option value="Erro">Erro</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Visitante</label>
            <select
              name="visitanteId"
              value={filtros.visitanteId}
              onChange={handleFiltroChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Todos os visitantes</option>
              {visitantes.map(visitante => (
                <option key={visitante.id} value={visitante.id}>
                  {visitante.nome}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Data Início</label>
            <input
              type="date"
              name="dataInicio"
              value={filtros.dataInicio}
              onChange={handleFiltroChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Data Fim</label>
            <input
              type="date"
              name="dataFim"
              value={filtros.dataFim}
              onChange={handleFiltroChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>

        {(filtros.status || filtros.visitanteId || filtros.dataInicio || filtros.dataFim) && (
          <div className="mt-4">
            <button
              onClick={limparFiltros}
              className="inline-flex items-center px-3 py-1.5 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded transition-colors"
            >
              <X className="w-4 h-4 mr-1" />
              Limpar filtros
            </button>
          </div>
        )}
      </div>

      {/* Tabela de Mensagens */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">
            Mensagens ({mensagensFiltradas.length})
          </h3>
        </div>

        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Visitante
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Mensagem
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Data/Hora Envio
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {mensagensFiltradas.map((mensagem) => (
                <tr key={mensagem.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <User className="w-4 h-4 text-gray-400 mr-2" />
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {getVisitanteNome(mensagem.visitanteId)}
                        </div>
                        <div className="text-sm text-gray-500">
                          ID: {mensagem.visitanteId}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm text-gray-900 max-w-xs truncate">
                      {mensagem.textoMensagem}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center text-sm text-gray-900">
                      <Calendar className="w-4 h-4 text-gray-400 mr-2" />
                      {formatDateTime(mensagem.dataHoraEnvio)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      {getStatusIcon(mensagem.status)}
                      <span className={`ml-2 ${getStatusBadge(mensagem.status)}`}>
                        {getStatusText(mensagem.status)}
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex space-x-2">
                      <Link
                        to={`/visitantes/${mensagem.visitanteId}`}
                        className="text-blue-600 hover:text-blue-900"
                        title="Ver visitante"
                      >
                        <Eye className="w-4 h-4" />
                      </Link>
                      {mensagem.status === 'Agendada' && (
                        <button
                          onClick={() => cancelarMensagem(mensagem.id)}
                          className="text-red-600 hover:text-red-900"
                          title="Cancelar mensagem"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Empty State */}
        {mensagensFiltradas.length === 0 && !loading && (
          <div className="text-center py-12">
            <MessageSquare className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              {mensagens.length === 0 ? 'Nenhuma mensagem encontrada' : 'Nenhuma mensagem corresponde aos filtros'}
            </h3>
            <p className="text-gray-600">
              {mensagens.length === 0 
                ? 'As mensagens aparecerão aqui quando visitantes forem cadastrados.'
                : 'Tente ajustar os filtros para encontrar as mensagens desejadas.'
              }
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default MensagensAgendadas;

