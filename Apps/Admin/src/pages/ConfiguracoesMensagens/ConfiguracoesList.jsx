import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Clock, MessageSquare, ToggleLeft, ToggleRight } from 'lucide-react';
import api from '../../lib/api';
import Loading from '../../components/ui/loading';
import ErrorMessage from '../../components/ui/error-message';

const ConfiguracoesList = () => {
  const [configuracoes, setConfiguracoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchConfiguracoes();
  }, []);

  const fetchConfiguracoes = async () => {
    try {
      setLoading(true);
      const response = await api.get('/configuracoesMensagens');
      setConfiguracoes(response.data);
    } catch (err) {
      setError('Erro ao carregar configurações de mensagens');
      console.error('Erro ao buscar configurações:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Tem certeza que deseja excluir esta configuração?')) {
      try {
        await api.delete(`/configuracoesMensagens/${id}`);
        setConfiguracoes(configuracoes.filter(config => config.id !== id));
      } catch (err) {
        setError('Erro ao excluir configuração');
        console.error('Erro ao excluir configuração:', err);
      }
    }
  };

  const toggleStatus = async (id, currentStatus) => {
    try {
      const configuracao = configuracoes.find(c => c.id === id);
      const updatedConfig = { ...configuracao, ativo: !currentStatus };
      
      await api.put(`/configuracoesMensagens/${id}`, updatedConfig);
      
      setConfiguracoes(configuracoes.map(config => 
        config.id === id ? { ...config, ativo: !currentStatus } : config
      ));
    } catch (err) {
      setError('Erro ao alterar status da configuração');
      console.error('Erro ao alterar status:', err);
    }
  };

  const formatHorario = (horario) => {
    if (!horario) return 'Não definido';
    return horario.substring(0, 5); // HH:MM
  };

  if (loading) return <Loading />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Configurações de Mensagens</h1>
          <p className="text-gray-600 mt-1">Gerencie as mensagens automáticas enviadas aos visitantes</p>
        </div>
        <Link
          to="/configuracoes-mensagens/novo"
          className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <Plus className="w-4 h-4 mr-2" />
          Nova Configuração
        </Link>
      </div>

      {error && <ErrorMessage message={error} />}

      {/* Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {configuracoes.map((config) => (
          <div key={config.id} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            {/* Header do Card */}
            <div className="flex justify-between items-start mb-4">
              <div className="flex items-center space-x-2">
                <MessageSquare className="w-5 h-5 text-blue-600" />
                <span className="font-medium text-gray-900">Mensagem #{config.id}</span>
              </div>
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => toggleStatus(config.id, config.ativo)}
                  className={`p-1 rounded transition-colors ${
                    config.ativo ? 'text-green-600 hover:text-green-700' : 'text-gray-400 hover:text-gray-500'
                  }`}
                  title={config.ativo ? 'Desativar' : 'Ativar'}
                >
                  {config.ativo ? <ToggleRight className="w-6 h-6" /> : <ToggleLeft className="w-6 h-6" />}
                </button>
              </div>
            </div>

            {/* Status Badge */}
            <div className="mb-3">
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                config.ativo 
                  ? 'bg-green-100 text-green-800' 
                  : 'bg-gray-100 text-gray-800'
              }`}>
                {config.ativo ? 'Ativa' : 'Inativa'}
              </span>
            </div>

            {/* Conteúdo da Mensagem */}
            <div className="mb-4">
              <p className="text-sm text-gray-600 line-clamp-3">
                {config.textoMensagem}
              </p>
            </div>

            {/* Informações de Agendamento */}
            <div className="space-y-2 mb-4">
              <div className="flex items-center text-sm text-gray-600">
                <Clock className="w-4 h-4 mr-2" />
                <span>
                  {config.diasAposVisita === 0 
                    ? 'No mesmo dia' 
                    : `${config.diasAposVisita} dia${config.diasAposVisita > 1 ? 's' : ''} após a visita`
                  }
                </span>
              </div>
              <div className="flex items-center text-sm text-gray-600">
                <Clock className="w-4 h-4 mr-2" />
                <span>Às {formatHorario(config.horarioEnvio)}</span>
              </div>
            </div>

            {/* Ações */}
            <div className="flex justify-end space-x-2 pt-4 border-t border-gray-100">
              <Link
                to={`/configuracoes-mensagens/editar/${config.id}`}
                className="inline-flex items-center px-3 py-1.5 text-sm text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded transition-colors"
              >
                <Edit className="w-4 h-4 mr-1" />
                Editar
              </Link>
              <button
                onClick={() => handleDelete(config.id)}
                className="inline-flex items-center px-3 py-1.5 text-sm text-red-600 hover:text-red-700 hover:bg-red-50 rounded transition-colors"
              >
                <Trash2 className="w-4 h-4 mr-1" />
                Excluir
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Empty State */}
      {configuracoes.length === 0 && !loading && (
        <div className="text-center py-12">
          <MessageSquare className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">Nenhuma configuração encontrada</h3>
          <p className="text-gray-600 mb-4">Comece criando sua primeira configuração de mensagem automática.</p>
          <Link
            to="/configuracoes-mensagens/novo"
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4 mr-2" />
            Nova Configuração
          </Link>
        </div>
      )}
    </div>
  );
};

export default ConfiguracoesList;

