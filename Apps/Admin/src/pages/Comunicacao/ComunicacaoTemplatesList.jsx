import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Mail, MessageSquare, Bell, LayoutTemplate } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { comunicacaoTemplatesApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

const getCanalIcon = (canal) => {
  switch (Number(canal)) {
    case 1: return <MessageSquare className="w-4 h-4" />;
    case 2: return <Mail className="w-4 h-4" />;
    case 3: return <Bell className="w-4 h-4" />;
    default: return <LayoutTemplate className="w-4 h-4" />;
  }
};

const getCanalLabel = (canal) => {
  switch (Number(canal)) {
    case 1: return 'WhatsApp';
    case 2: return 'E-mail';
    case 3: return 'Push';
    case 4: return 'Notificação interna';
    default: return `Canal ${canal}`;
  }
};

export default function ComunicacaoTemplatesList() {
  const [templates, setTemplates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async ({ silent = false } = {}) => {
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);
      const response = await comunicacaoTemplatesApi.getAll();
      setTemplates(response.data || []);
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar templates');
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) return <LoadingPage text="Carregando templates..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Templates de Comunicação</h1>
          <p className="text-muted-foreground mt-1">Biblioteca inicial de templates reutilizáveis por canal.</p>
        </div>

        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          <Button variant="outline" asChild>
            <Link to="/comunicacao/campanhas">Campanhas</Link>
          </Button>
          <Button asChild>
            <Link to="/comunicacao/templates/novo">
              <Plus className="w-4 h-4 mr-2" />
              Novo Template
            </Link>
          </Button>
        </div>
      </div>

      {templates.length === 0 ? (
        <PageEmptyState
          title="Nenhum template cadastrado"
          description="Comece criando o primeiro template do módulo de comunicação."
          action={(
            <Button asChild>
              <Link to="/comunicacao/templates/novo">Criar template</Link>
            </Button>
          )}
        />
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
          {templates.map((template) => (
            <Card key={template.id}>
              <CardContent className="p-6 flex items-start justify-between gap-4">
                <div className="space-y-2">
                  <div className="flex items-center gap-2 text-muted-foreground">
                    {getCanalIcon(template.canal)}
                    <span className="text-sm">{getCanalLabel(template.canal)}</span>
                  </div>
                  <div>
                    <h2 className="text-lg font-semibold text-foreground">{template.nome}</h2>
                    <p className="text-sm text-muted-foreground">{template.objetivo}</p>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Badge variant="secondary">v{template.versao}</Badge>
                  <Button variant="outline" size="sm" asChild>
                    <Link to={`/comunicacao/templates/${template.id}/editar`}>Editar</Link>
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
