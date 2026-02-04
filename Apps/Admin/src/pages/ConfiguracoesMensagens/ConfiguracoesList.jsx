import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Clock, MessageSquare, ToggleLeft, ToggleRight } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
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
          <h1 className="text-3xl font-bold text-foreground">Configurações de Mensagens</h1>
          <p className="text-muted-foreground mt-1">Gerencie as mensagens automáticas enviadas aos visitantes</p>
        </div>
        <Button asChild>
          <Link to="/configuracoes-mensagens/novo">
            <Plus className="w-4 h-4 mr-2" />
            Nova Configuração
          </Link>
        </Button>
      </div>

      {error && <ErrorMessage message={error} />}

      {/* Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {configuracoes.map((config) => (
          <Card key={config.id}>
            <CardContent className="p-6">
              {/* Header do Card */}
              <div className="flex justify-between items-start mb-4">
                <div className="flex items-center space-x-2">
                  <MessageSquare className="w-5 h-5 text-blue-500 dark:text-blue-400" />
                  <span className="font-medium text-foreground">Mensagem #{config.id}</span>
                </div>
                <div className="flex items-center space-x-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => toggleStatus(config.id, config.ativo)}
                    title={config.ativo ? 'Desativar' : 'Ativar'}
                    className={config.ativo ? 'text-green-500 hover:text-green-600' : 'text-muted-foreground'}
                  >
                    {config.ativo ? <ToggleRight className="w-6 h-6" /> : <ToggleLeft className="w-6 h-6" />}
                  </Button>
                </div>
              </div>

              {/* Status Badge */}
              <div className="mb-3">
                {config.ativo ? (
                  <Badge variant="default" className="bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700">
                    Ativa
                  </Badge>
                ) : (
                  <Badge variant="secondary">Inativa</Badge>
                )}
              </div>

              {/* Conteúdo da Mensagem */}
              <div className="mb-4">
                <p className="text-sm text-muted-foreground line-clamp-3">
                  {config.textoMensagem}
                </p>
              </div>

              {/* Informações de Agendamento */}
              <div className="space-y-2 mb-4">
                <div className="flex items-center text-sm text-muted-foreground">
                  <Clock className="w-4 h-4 mr-2" />
                  <span>
                    {config.diasAposVisita === 0 
                      ? 'No mesmo dia' 
                      : `${config.diasAposVisita} dia${config.diasAposVisita > 1 ? 's' : ''} após a visita`
                    }
                  </span>
                </div>
                <div className="flex items-center text-sm text-muted-foreground">
                  <Clock className="w-4 h-4 mr-2" />
                  <span>Às {formatHorario(config.horarioEnvio)}</span>
                </div>
              </div>

              {/* Ações */}
              <div className="flex justify-end space-x-2 pt-4 border-t border-border">
                <Button variant="ghost" size="sm" asChild>
                  <Link to={`/configuracoes-mensagens/editar/${config.id}`}>
                    <Edit className="w-4 h-4 mr-1" />
                    Editar
                  </Link>
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleDelete(config.id)}
                  className="text-destructive hover:text-destructive"
                >
                  <Trash2 className="w-4 h-4 mr-1" />
                  Excluir
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Empty State */}
      {configuracoes.length === 0 && !loading && (
        <div className="text-center py-12">
          <MessageSquare className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Nenhuma configuração encontrada</h3>
          <p className="text-muted-foreground mb-4">Comece criando sua primeira configuração de mensagem automática.</p>
          <Button asChild>
            <Link to="/configuracoes-mensagens/novo">
              <Plus className="w-4 h-4 mr-2" />
              Nova Configuração
            </Link>
          </Button>
        </div>
      )}
    </div>
  );
};

export default ConfiguracoesList;

