import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Save, Eye, Clock, MessageSquare } from 'lucide-react';
import api from '../../lib/api';
import Loading from '../../components/ui/loading';
import ErrorMessage from '../../components/ui/error-message';

const ConfiguracaoForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    textoMensagem: '',
    diasAposVisita: 0,
    horarioEnvio: '09:00',
    ativo: true
  });

  useEffect(() => {
    if (isEditing) {
      fetchConfiguracao();
    }
  }, [id, isEditing]);

  const fetchConfiguracao = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/configuracoesMensagens/${id}`);
      const config = response.data;
      
      setFormData({
        textoMensagem: config.textoMensagem || '',
        diasAposVisita: config.diasAposVisita || 0,
        horarioEnvio: config.horarioEnvio ? config.horarioEnvio.substring(0, 5) : '09:00',
        ativo: config.ativo !== undefined ? config.ativo : true
      });
    } catch (err) {
      setError('Erro ao carregar configuração');
      console.error('Erro ao buscar configuração:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.textoMensagem.trim()) {
      setError('O texto da mensagem é obrigatório');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const payload = {
        textoMensagem: formData.textoMensagem.trim(),
        diasAposVisita: parseInt(formData.diasAposVisita),
        horarioEnvio: formData.horarioEnvio + ':00', // Adiciona segundos
        ativo: formData.ativo
      };

      if (isEditing) {
        await api.put(`/configuracoesMensagens/${id}`, payload);
      } else {
        await api.post('/configuracoesMensagens', payload);
      }

      navigate('/configuracoes-mensagens');
    } catch (err) {
      setError(isEditing ? 'Erro ao atualizar configuração' : 'Erro ao criar configuração');
      console.error('Erro ao salvar configuração:', err);
    } finally {
      setLoading(false);
    }
  };

  const getPreviewMessage = () => {
    if (!formData.textoMensagem) return '';
    
    // Substitui variáveis por valores de exemplo
    return formData.textoMensagem
      .replace(/{Nome}/g, 'João Silva')
      .replace(/{nome}/g, 'João Silva');
  };

  const getDiasText = () => {
    if (formData.diasAposVisita === 0) return 'no mesmo dia da visita';
    if (formData.diasAposVisita === 1) return '1 dia após a visita';
    return `${formData.diasAposVisita} dias após a visita`;
  };

  if (loading && isEditing) return <Loading />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center space-x-4">
        <button
          onClick={() => navigate('/configuracoes-mensagens')}
          className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {isEditing ? 'Editar Configuração' : 'Nova Configuração'}
          </h1>
          <p className="text-gray-600 mt-1">
            {isEditing ? 'Altere as informações da configuração de mensagem' : 'Configure uma nova mensagem automática'}
          </p>
        </div>
      </div>

      {error && <ErrorMessage message={error} />}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Formulário */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Texto da Mensagem */}
            <div>
              <label htmlFor="textoMensagem" className="block text-sm font-medium text-gray-700 mb-2">
                Texto da Mensagem *
              </label>
              <textarea
                id="textoMensagem"
                name="textoMensagem"
                value={formData.textoMensagem}
                onChange={handleInputChange}
                rows={6}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Digite o texto da mensagem... Use {Nome} para incluir o nome do visitante"
                required
              />
              <p className="text-xs text-gray-500 mt-1">
                Dica: Use {'{Nome}'} para incluir automaticamente o nome do visitante na mensagem
              </p>
            </div>

            {/* Dias Após Visita */}
            <div>
              <label htmlFor="diasAposVisita" className="block text-sm font-medium text-gray-700 mb-2">
                Enviar quantos dias após a visita?
              </label>
              <select
                id="diasAposVisita"
                name="diasAposVisita"
                value={formData.diasAposVisita}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value={0}>No mesmo dia</option>
                <option value={1}>1 dia depois</option>
                <option value={2}>2 dias depois</option>
                <option value={3}>3 dias depois</option>
                <option value={7}>1 semana depois</option>
                <option value={14}>2 semanas depois</option>
                <option value={30}>1 mês depois</option>
              </select>
            </div>

            {/* Horário de Envio */}
            <div>
              <label htmlFor="horarioEnvio" className="block text-sm font-medium text-gray-700 mb-2">
                Horário de Envio
              </label>
              <input
                type="time"
                id="horarioEnvio"
                name="horarioEnvio"
                value={formData.horarioEnvio}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              />
            </div>

            {/* Status Ativo */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="ativo"
                name="ativo"
                checked={formData.ativo}
                onChange={handleInputChange}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="ativo" className="ml-2 block text-sm text-gray-700">
                Configuração ativa
              </label>
            </div>

            {/* Botões */}
            <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={() => navigate('/configuracoes-mensagens')}
                className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={loading}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
              >
                <Save className="w-4 h-4 mr-2" />
                {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Criar')}
              </button>
            </div>
          </form>
        </div>

        {/* Preview */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center space-x-2 mb-4">
            <Eye className="w-5 h-5 text-blue-600" />
            <h3 className="text-lg font-medium text-gray-900">Preview da Mensagem</h3>
          </div>

          {/* Informações de Agendamento */}
          <div className="bg-blue-50 rounded-lg p-4 mb-4">
            <div className="flex items-center space-x-2 text-blue-800 mb-2">
              <Clock className="w-4 h-4" />
              <span className="font-medium">Agendamento</span>
            </div>
            <p className="text-sm text-blue-700">
              Esta mensagem será enviada <strong>{getDiasText()}</strong> às <strong>{formData.horarioEnvio}</strong>
            </p>
          </div>

          {/* Preview da Mensagem */}
          <div className="border border-gray-200 rounded-lg p-4">
            <div className="flex items-center space-x-2 mb-3">
              <MessageSquare className="w-4 h-4 text-green-600" />
              <span className="text-sm font-medium text-gray-700">WhatsApp</span>
            </div>
            
            {formData.textoMensagem ? (
              <div className="bg-green-100 rounded-lg p-3 max-w-xs">
                <p className="text-sm text-gray-800 whitespace-pre-wrap">
                  {getPreviewMessage()}
                </p>
              </div>
            ) : (
              <div className="text-gray-400 text-sm italic">
                Digite o texto da mensagem para ver o preview
              </div>
            )}
          </div>

          {/* Status */}
          <div className="mt-4 pt-4 border-t border-gray-200">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Status:</span>
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                formData.ativo 
                  ? 'bg-green-100 text-green-800' 
                  : 'bg-gray-100 text-gray-800'
              }`}>
                {formData.ativo ? 'Ativa' : 'Inativa'}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ConfiguracaoForm;

