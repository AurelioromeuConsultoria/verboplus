import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Bell, CheckCheck, Clock3 } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { notificacoesApi } from '@/lib/api';
import { toast } from 'sonner';

function getTipoLabel(tipo) {
  const value = Number(tipo);
  if (value === 2) return 'Escala';
  if (value === 3) return 'Troca';
  return 'Geral';
}

export default function NotificacoesList() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notificacoes, setNotificacoes] = useState([]);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await notificacoesApi.getMinhas();
      setNotificacoes(res.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar notificações');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const marcarComoLida = async (id) => {
    try {
      await notificacoesApi.marcarComoLida(id);
      setNotificacoes((current) =>
        current.map((item) => (item.id === id ? { ...item, dataLeitura: new Date().toISOString() } : item))
      );
    } catch (err) {
      console.error(err);
      toast.error('Erro ao marcar notificação como lida');
    }
  };

  const marcarTodas = async () => {
    try {
      await notificacoesApi.marcarTodasComoLidas();
      setNotificacoes((current) => current.map((item) => ({ ...item, dataLeitura: item.dataLeitura || new Date().toISOString() })));
      toast.success('Notificações marcadas como lidas');
    } catch (err) {
      console.error(err);
      toast.error('Erro ao atualizar notificações');
    }
  };

  if (loading) return <LoadingPage text="Carregando notificações..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Notificações</h1>
          <p className="text-muted-foreground">
            Central de avisos operacionais do sistema, com foco em escalas e trocas.
          </p>
        </div>
        <Button variant="outline" onClick={marcarTodas}>
          <CheckCheck className="h-4 w-4 mr-2" />
          Marcar todas como lidas
        </Button>
      </div>

      {notificacoes.length === 0 ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            Nenhuma notificação encontrada.
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {notificacoes.map((item) => (
            <Card key={item.id} className={item.dataLeitura ? 'opacity-80' : 'border-primary/30'}>
              <CardHeader>
                <CardTitle className="flex items-center justify-between gap-3">
                  <div className="flex items-center gap-2">
                    <Bell className="h-5 w-5" />
                    <span>{item.titulo}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={item.dataLeitura ? 'secondary' : 'default'}>
                      {item.dataLeitura ? 'Lida' : 'Nova'}
                    </Badge>
                    <Badge variant="outline">{getTipoLabel(item.tipo)}</Badge>
                  </div>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm">{item.mensagem}</p>
                <div className="flex items-center justify-between gap-3 text-sm text-muted-foreground">
                  <div className="flex items-center gap-2">
                    <Clock3 className="h-4 w-4" />
                    {new Date(item.dataCriacao).toLocaleString('pt-BR')}
                  </div>
                  {!item.dataLeitura && (
                    <Button variant="outline" size="sm" onClick={() => marcarComoLida(item.id)}>
                      Marcar como lida
                    </Button>
                  )}
                </div>
                {item.link && (
                  <div>
                    <Button size="sm" asChild>
                      <Link to={item.link}>Abrir</Link>
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
