import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { PageEmptyState, PageRefreshButton } from '@/components/ui/page-state';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { escalasModelosApi, equipesApi } from '@/lib/api';
import { toast } from 'sonner';

export default function ModelosEscalaList() {
  const [equipes, setEquipes] = useState([]);
  const [equipeId, setEquipeId] = useState('');
  const [modelos, setModelos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingModelos, setLoadingModelos] = useState(false);
  const [refreshingModelos, setRefreshingModelos] = useState(false);
  const [error, setError] = useState(null);
  const confirmDialog = useConfirmDialog();

  const loadEquipes = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await equipesApi.getAll();
      setEquipes(res.data || []);
      if (res.data?.length && !equipeId) setEquipeId(String(res.data[0].id));
    } catch (err) {
      setError('Erro ao carregar equipes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadModelos = async ({ silent = false } = {}) => {
    if (!equipeId) {
      setModelos([]);
      return;
    }
    try {
      if (silent) {
        setRefreshingModelos(true);
      } else {
        setLoadingModelos(true);
      }
      const res = await escalasModelosApi.getByEquipe(equipeId);
      setModelos(res.data || []);
    } catch (err) {
      console.error(err);
      toast.error('Erro ao carregar modelos');
      setModelos([]);
    } finally {
      if (silent) {
        setRefreshingModelos(false);
      } else {
        setLoadingModelos(false);
      }
    }
  };

  useEffect(() => {
    loadEquipes();
  }, []);

  useEffect(() => {
    loadModelos();
  }, [equipeId]);

  const handleDelete = async (modelo) => {
    confirmDialog.show({
      title: 'Excluir modelo de escala',
      description: `Excluir o modelo "${modelo.nome || (modelo.eventoNome ? modelo.eventoNome : 'Sem nome')}" da equipe ${modelo.equipeNome}?`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await escalasModelosApi.delete(modelo.id);
          toast.success('Modelo excluído');
          await loadModelos();
        } catch (err) {
          const msg = err.response?.data?.message || err.response?.data || 'Erro ao excluir';
          toast.error(typeof msg === 'string' ? msg : 'Erro ao excluir');
          throw err;
        }
      },
    });
  };

  if (loading) return <LoadingPage text="Carregando equipes..." />;
  if (error) return <ErrorPage message={error} onRetry={loadEquipes} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Modelos de Escala</h1>
          <p className="text-muted-foreground">
            Defina quantas pessoas (por cargo) cada equipe precisa por evento. Usado no &quot;Preencher automaticamente&quot;.
          </p>
        </div>
        <Button asChild>
          <Link to={equipeId ? `/voluntariado/modelos-escala/novo?equipeId=${equipeId}` : '/voluntariado/modelos-escala/novo'}>
            <Plus className="h-4 w-4 mr-2" /> Novo modelo
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtrar por equipe</CardTitle>
        </CardHeader>
        <CardContent>
          <Select value={equipeId || 'all'} onValueChange={(v) => setEquipeId(v === 'all' ? '' : v)}>
            <SelectTrigger className="max-w-xs">
              <SelectValue placeholder="Selecione a equipe" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todas</SelectItem>
              {equipes.map((e) => (
                <SelectItem key={e.id} value={String(e.id)}>{e.nome}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between gap-3">
            <CardTitle>Modelos {equipeId ? `— ${equipes.find((e) => String(e.id) === equipeId)?.nome || ''}` : ''}</CardTitle>
            {equipeId ? (
              <PageRefreshButton onClick={() => loadModelos({ silent: true })} refreshing={refreshingModelos} />
            ) : null}
          </div>
        </CardHeader>
        <CardContent>
          {!equipeId ? (
            <PageEmptyState
              title="Selecione uma equipe para listar os modelos."
              description="Os modelos de escala são organizados por equipe."
            />
          ) : loadingModelos ? (
            <LoadingPage text="Carregando modelos..." />
          ) : !modelos.length ? (
            <PageEmptyState
              title="Nenhum modelo para esta equipe."
              description="Crie um modelo para usar o preenchimento automático."
              action={(
                <Button asChild>
                  <Link to={equipeId ? `/voluntariado/modelos-escala/novo?equipeId=${equipeId}` : '/voluntariado/modelos-escala/novo'}>
                    <Plus className="h-4 w-4 mr-2" /> Novo modelo
                  </Link>
                </Button>
              )}
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Evento</TableHead>
                  <TableHead>Nome</TableHead>
                  <TableHead>Equipe</TableHead>
                  <TableHead>Dias folga</TableHead>
                  <TableHead>Itens (cargo × qtd)</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {modelos.map((m) => (
                  <TableRow key={m.id}>
                    <TableCell>{m.eventoNome ?? 'Padrão (qualquer evento)'}</TableCell>
                    <TableCell>{m.nome || '-'}</TableCell>
                    <TableCell>{m.equipeNome}</TableCell>
                    <TableCell>{m.diasFolgaAposEscala ?? '-'}</TableCell>
                    <TableCell>
                      {m.itens?.length
                        ? m.itens.map((i) => `${i.cargoNome || 'Qualquer'} × ${i.quantidade}`).join(', ')
                        : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="ghost" size="sm" asChild>
                        <Link to={`/voluntariado/modelos-escala/${m.id}`}>
                          <Edit className="h-4 w-4" />
                        </Link>
                      </Button>
                      <Button variant="ghost" size="sm" onClick={() => handleDelete(m)}>
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={confirmDialog.hide}
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
}
