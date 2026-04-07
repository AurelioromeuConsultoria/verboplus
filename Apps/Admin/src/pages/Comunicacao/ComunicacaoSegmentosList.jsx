import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Users, MessageSquareMore } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { comunicacaoSegmentosApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

export default function ComunicacaoSegmentosList() {
  const [segmentos, setSegmentos] = useState([]);
  const [estimativas, setEstimativas] = useState({});
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async ({ silent = false } = {}) => {
    try {
      if (silent) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const response = await comunicacaoSegmentosApi.getAll();
      const items = response.data || [];
      setSegmentos(items);

      const estimativasEntries = await Promise.all(items.map(async (segmento) => {
        try {
          const estimativaResponse = await comunicacaoSegmentosApi.getEstimativa({ segmentoId: segmento.id });
          return [segmento.id, estimativaResponse.data];
        } catch {
          return [segmento.id, null];
        }
      }));

      setEstimativas(Object.fromEntries(estimativasEntries));
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar segmentos');
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

  if (loading) return <LoadingPage text="Carregando segmentos..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Segmentos de Comunicação</h1>
          <p className="text-muted-foreground mt-1">Salve públicos prioritários e entenda a audiência prevista antes do disparo.</p>
        </div>

        <div className="flex items-center gap-2">
          <PageRefreshButton onClick={() => load({ silent: true })} refreshing={refreshing} />
          <Button variant="outline" asChild>
            <Link to="/comunicacao/campanhas">Campanhas</Link>
          </Button>
          <Button asChild>
            <Link to="/comunicacao/segmentos/novo">
              <Plus className="w-4 h-4 mr-2" />
              Novo Segmento
            </Link>
          </Button>
        </div>
      </div>

      {segmentos.length === 0 ? (
        <PageEmptyState
          title="Nenhum segmento cadastrado"
          description="Crie segmentos básicos para reutilizar públicos prioritários do módulo."
          action={<Button asChild><Link to="/comunicacao/segmentos/novo">Criar segmento</Link></Button>}
        />
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
          {segmentos.map((segmento) => {
            const estimativa = estimativas[segmento.id];

            return (
              <Card key={segmento.id}>
                <CardContent className="p-6 space-y-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <div className="text-lg font-semibold text-foreground">{segmento.nome}</div>
                      <p className="text-sm text-muted-foreground">{segmento.descricao || segmento.publicoAlvo}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      {segmento.padrao && <Badge variant="secondary">Padrão</Badge>}
                      <Badge variant={segmento.ativo ? 'outline' : 'destructive'}>{segmento.ativo ? 'Ativo' : 'Inativo'}</Badge>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
                    <div className="rounded-lg border border-border p-3">
                      <div className="flex items-center gap-2 text-muted-foreground"><Users className="w-4 h-4" /> Total</div>
                      <div className="text-xl font-semibold mt-1">{estimativa?.totalDestinatarios ?? '-'}</div>
                    </div>
                    <div className="rounded-lg border border-border p-3">
                      <div className="text-muted-foreground">WhatsApp</div>
                      <div className="text-xl font-semibold mt-1">{estimativa?.comWhatsApp ?? '-'}</div>
                    </div>
                    <div className="rounded-lg border border-border p-3">
                      <div className="text-muted-foreground">E-mail</div>
                      <div className="text-xl font-semibold mt-1">{estimativa?.comEmail ?? '-'}</div>
                    </div>
                    <div className="rounded-lg border border-border p-3">
                      <div className="flex items-center gap-2 text-muted-foreground"><MessageSquareMore className="w-4 h-4" /> Contextual</div>
                      <div className="text-xl font-semibold mt-1">{estimativa ? estimativa.comPush + estimativa.comNotificacaoInterna : '-'}</div>
                    </div>
                  </div>

                  <div className="flex justify-end">
                    <Button variant="outline" size="sm" asChild>
                      <Link to={`/comunicacao/segmentos/${segmento.id}/editar`}>Editar</Link>
                    </Button>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
