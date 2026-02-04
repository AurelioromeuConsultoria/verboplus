import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Save, Eye, Clock, MessageSquare } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
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
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate('/configuracoes-mensagens')}
        >
          <ArrowLeft className="w-5 h-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">
            {isEditing ? 'Editar Configuração' : 'Nova Configuração'}
          </h1>
          <p className="text-muted-foreground mt-1">
            {isEditing ? 'Altere as informações da configuração de mensagem' : 'Configure uma nova mensagem automática'}
          </p>
        </div>
      </div>

      {error && <ErrorMessage message={error} />}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Formulário */}
        <Card>
          <CardContent className="p-6">
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Texto da Mensagem */}
              <div className="space-y-2">
                <Label htmlFor="textoMensagem">
                  Texto da Mensagem *
                </Label>
                <Textarea
                  id="textoMensagem"
                  name="textoMensagem"
                  value={formData.textoMensagem}
                  onChange={handleInputChange}
                  rows={6}
                  placeholder="Digite o texto da mensagem... Use {Nome} para incluir o nome do visitante"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Dica: Use {'{Nome}'} para incluir automaticamente o nome do visitante na mensagem
                </p>
              </div>

              {/* Dias Após Visita */}
              <div className="space-y-2">
                <Label htmlFor="diasAposVisita">
                  Enviar quantos dias após a visita?
                </Label>
                <select
                  id="diasAposVisita"
                  name="diasAposVisita"
                  value={formData.diasAposVisita}
                  onChange={handleInputChange}
                  className="w-full px-3 py-2 bg-background border border-input rounded-lg focus:ring-2 focus:ring-ring focus:border-ring"
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
              <div className="space-y-2">
                <Label htmlFor="horarioEnvio">
                  Horário de Envio
                </Label>
                <Input
                  type="time"
                  id="horarioEnvio"
                  name="horarioEnvio"
                  value={formData.horarioEnvio}
                  onChange={handleInputChange}
                  required
                />
              </div>

              {/* Status Ativo */}
              <div className="flex items-center space-x-2">
                <Switch
                  id="ativo"
                  name="ativo"
                  checked={formData.ativo}
                  onCheckedChange={(checked) => setFormData(prev => ({ ...prev, ativo: checked }))}
                />
                <Label htmlFor="ativo" className="cursor-pointer">
                  Configuração ativa
                </Label>
              </div>

              {/* Botões */}
              <div className="flex justify-end space-x-3 pt-6 border-t border-border">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate('/configuracoes-mensagens')}
                >
                  Cancelar
                </Button>
                <Button
                  type="submit"
                  disabled={loading}
                >
                  <Save className="w-4 h-4 mr-2" />
                  {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Criar')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {/* Preview */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Eye className="w-5 h-5" />
              Preview da Mensagem
            </CardTitle>
          </CardHeader>
          <CardContent>
            {/* Informações de Agendamento */}
            <div className="bg-blue-500/10 dark:bg-blue-500/20 rounded-lg p-4 mb-4 border border-blue-500/20">
              <div className="flex items-center space-x-2 text-blue-600 dark:text-blue-400 mb-2">
                <Clock className="w-4 h-4" />
                <span className="font-medium">Agendamento</span>
              </div>
              <p className="text-sm text-blue-700 dark:text-blue-300">
                Esta mensagem será enviada <strong>{getDiasText()}</strong> às <strong>{formData.horarioEnvio}</strong>
              </p>
            </div>

            {/* Preview da Mensagem */}
            <div className="border border-border rounded-lg p-4">
              <div className="flex items-center space-x-2 mb-3">
                <MessageSquare className="w-4 h-4 text-green-500 dark:text-green-400" />
                <span className="text-sm font-medium text-foreground">WhatsApp</span>
              </div>
            
              {formData.textoMensagem ? (
                <div className="bg-green-500/10 dark:bg-green-500/20 rounded-lg p-3 max-w-xs border border-green-500/20">
                  <p className="text-sm text-foreground whitespace-pre-wrap">
                    {getPreviewMessage()}
                  </p>
                </div>
              ) : (
                <div className="text-muted-foreground text-sm italic">
                  Digite o texto da mensagem para ver o preview
                </div>
              )}
            </div>

            {/* Status */}
            <div className="mt-4 pt-4 border-t border-border">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Status:</span>
                {formData.ativo ? (
                  <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">
                    Ativa
                  </Badge>
                ) : (
                  <Badge variant="secondary">Inativa</Badge>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default ConfiguracaoForm;

