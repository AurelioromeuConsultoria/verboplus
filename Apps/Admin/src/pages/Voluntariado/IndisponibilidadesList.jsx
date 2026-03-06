import { useEffect, useState } from 'react';
import { CalendarOff, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { indisponibilidadesVoluntariosApi, voluntariosApi } from '@/lib/api';
import { toast } from 'sonner';

export default function IndisponibilidadesList() {
  const [voluntarios, setVoluntarios] = useState([]);
  const [voluntarioId, setVoluntarioId] = useState('');
  const [itens, setItens] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingItens, setLoadingItens] = useState(false);
  const [error, setError] = useState(null);
  const [novaData, setNovaData] = useState('');
  const [novoMotivo, setNovoMotivo] = useState('');
  const [saving, setSaving] = useState(false);
  const confirmDialog = useConfirmDialog();

  const loadVoluntarios = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await voluntariosApi.getAll();
      setVoluntarios(res.data || []);
      if (res.data?.length && !voluntarioId) setVoluntarioId(String(res.data[0].id));
    } catch (err) {
      setError('Erro ao carregar voluntários');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadItens = async () => {
    if (!voluntarioId) {
      setItens([]);
      return;
    }
    try {
      setLoadingItens(true);
      const res = await indisponibilidadesVoluntariosApi.getByVoluntario(voluntarioId);
      setItens(res.data || []);
    } catch (err) {
      console.error(err);
      toast.error('Erro ao carregar indisponibilidades');
      setItens([]);
    } finally {
      setLoadingItens(false);
    }
  };

  useEffect(() => {
    loadVoluntarios();
  }, []);

  useEffect(() => {
    loadItens();
  }, [voluntarioId]);

  const handleAdd = async (e) => {
    e.preventDefault();
    if (!voluntarioId || !novaData) {
      toast.error('Selecione o voluntário e informe a data');
      return;
    }
    try {
      setSaving(true);
      await indisponibilidadesVoluntariosApi.create({
        voluntarioId: Number(voluntarioId),
        data: novaData,
        motivo: novoMotivo.trim() || null,
      });
      toast.success('Indisponibilidade registrada');
      setNovaData('');
      setNovoMotivo('');
      await loadItens();
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data || 'Erro ao salvar';
      toast.error(typeof msg === 'string' ? msg : 'Erro ao salvar');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (item) => {
    confirmDialog.show({
      title: 'Remover indisponibilidade',
      description: `Remover a data ${new Date(item.data).toLocaleDateString('pt-BR')}${item.motivo ? ` (${item.motivo})` : ''}?`,
      confirmText: 'Remover',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await indisponibilidadesVoluntariosApi.delete(item.id);
          toast.success('Removido');
          await loadItens();
        } catch (err) {
          toast.error('Erro ao remover');
          throw err;
        }
      },
    });
  };

  if (loading) return <LoadingPage text="Carregando voluntários..." />;
  if (error) return <ErrorPage message={error} onRetry={loadVoluntarios} />;

  const voluntario = voluntarios.find((v) => String(v.id) === voluntarioId);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Indisponibilidades</h1>
        <p className="text-muted-foreground">
          Marque datas em que o voluntário não está disponível para escala. O preenchimento automático não os incluirá nesses dias.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Voluntário</CardTitle>
        </CardHeader>
        <CardContent>
          <Select value={voluntarioId || 'all'} onValueChange={setVoluntarioId}>
            <SelectTrigger className="max-w-md">
              <SelectValue placeholder="Selecione o voluntário" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Selecione</SelectItem>
              {voluntarios.map((v) => (
                <SelectItem key={v.id} value={String(v.id)}>
                  {v.nome} {v.nomeEquipe ? `— ${v.nomeEquipe}` : ''}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {voluntarioId && (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Adicionar data indisponível</CardTitle>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleAdd} className="flex flex-wrap items-end gap-4">
                <div className="space-y-2">
                  <Label>Data *</Label>
                  <Input
                    type="date"
                    value={novaData}
                    onChange={(e) => setNovaData(e.target.value)}
                    required
                  />
                </div>
                <div className="space-y-2 flex-1 min-w-[200px]">
                  <Label>Motivo (opcional)</Label>
                  <Input
                    value={novoMotivo}
                    onChange={(e) => setNovoMotivo(e.target.value)}
                    placeholder="Ex: Viagem"
                  />
                </div>
                <Button type="submit" disabled={saving}>
                  <Plus className="h-4 w-4 mr-2" />
                  {saving ? 'Salvando...' : 'Adicionar'}
                </Button>
              </form>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Datas indisponíveis {voluntario ? `— ${voluntario.nome}` : ''}</CardTitle>
            </CardHeader>
            <CardContent>
              {loadingItens ? (
                <LoadingPage text="Carregando..." />
              ) : !itens.length ? (
                <div className="flex items-center gap-2 text-muted-foreground py-4">
                  <CalendarOff className="h-5 w-5" />
                  Nenhuma data cadastrada.
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Data</TableHead>
                      <TableHead>Motivo</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {itens.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                        <TableCell>{item.motivo || '-'}</TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm" onClick={() => handleDelete(item)}>
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
        </>
      )}

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
