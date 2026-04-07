import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Clock, MessageSquare, ToggleLeft, ToggleRight } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { StatusBadge } from '@/components/ui/status-badge';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { configuracoesMensagensApi } from '@/lib/api';
import { formatShortTime } from '@/lib/formatters';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const ConfiguracoesList = () => {
  const [configuracoes, setConfiguracoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const confirmDialog = useConfirmDialog();

  useEffect(() => {
    fetchConfiguracoes();
  }, []);

  const fetchConfiguracoes = async ({ silent = false } = {}) => {
    try {
      if (silent) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      if (!silent) {
        setError(null);
      }
      const response = await configuracoesMensagensApi.getAll();
      setConfiguracoes(response.data || []);
    } catch (err) {
      setError('Erro ao carregar configurações de mensagens');
      console.error('Erro ao buscar configurações:', err);
      toast.error(getApiErrorMessage(err, 'Erro ao carregar configurações de mensagens'));
    } finally {
      if (silent) {
        setRefreshing(false);
      } else {
        setLoading(false);
      }
    }
  };

  const handleDelete = async (id) => {
    const config = configuracoes.find((c) => c.id === id);
    confirmDialog.show({
      title: 'Excluir configuração?',
      description: `Tem certeza que deseja excluir a configuração da mensagem #${config?.id ?? id}?`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await configuracoesMensagensApi.delete(id);
          toast.success('Configuração excluída com sucesso');
          setConfiguracoes((prev) => prev.filter((c) => c.id !== id));
        } catch (err) {
          setError('Erro ao excluir configuração');
          console.error('Erro ao excluir configuração:', err);
          toast.error(getApiErrorMessage(err, 'Erro ao excluir configuração'));
          throw err;
        }
      },
    });
  };

  const toggleStatus = async (id, currentStatus) => {
    try {
      const configuracao = configuracoes.find(c => c.id === id);
      const updatedConfig = { ...configuracao, ativo: !currentStatus };
      
      await configuracoesMensagensApi.update(id, updatedConfig);
      
      setConfiguracoes(configuracoes.map(config => 
        config.id === id ? { ...config, ativo: !currentStatus } : config
      ));
      toast.success(updatedConfig.ativo ? 'Configuração ativada' : 'Configuração desativada');
    } catch (err) {
      setError('Erro ao alterar status da configuração');
      console.error('Erro ao alterar status:', err);
      toast.error(getApiErrorMessage(err, 'Erro ao alterar status da configuração'));
    }
  };

  if (loading) return <LoadingPage text="Carregando configurações..." />;
  if (error) return <ErrorPage message={error} onRetry={fetchConfiguracoes} />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Configurações de Mensagens</h1>
          <p className="text-muted-foreground mt-1">Gerencie as mensagens automáticas enviadas aos visitantes</p>
        </div>
        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => fetchConfiguracoes({ silent: true })} refreshing={refreshing} />
          <Button asChild>
            <Link to="/configuracoes-mensagens/novo">
              <Plus className="w-4 h-4 mr-2" />
              Nova Configuração
            </Link>
          </Button>
        </div>
      </div>

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
                  <StatusBadge tone="success">Ativa</StatusBadge>
                ) : (
                  <StatusBadge tone="neutral">Inativa</StatusBadge>
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
                  <span>As {formatShortTime(config.horarioEnvio)}</span>
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
        <PageEmptyState
          title="Nenhuma configuração encontrada"
          description="Comece criando sua primeira configuração de mensagem automática."
          action={(
            <Button asChild>
              <Link to="/configuracoes-mensagens/novo">
                <Plus className="w-4 h-4 mr-2" />
                Nova Configuração
              </Link>
            </Button>
          )}
        />
      )}

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={(open) => {
          if (!open) confirmDialog.hide();
        }}
        onConfirm={confirmDialog.handleConfirm}
        title={confirmDialog.config.title}
        description={confirmDialog.config.description}
        confirmText={confirmDialog.config.confirmText}
        cancelText={confirmDialog.config.cancelText}
        variant={confirmDialog.config.variant}
        loading={confirmDialog.loading}
      />
    </div>
  );
};

export default ConfiguracoesList;
