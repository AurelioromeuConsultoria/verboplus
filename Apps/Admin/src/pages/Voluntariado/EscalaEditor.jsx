import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2, Send } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { cargosApi, equipesApi, escalasApi, eventosOcorrenciasApi, voluntariosApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';

function getEscalaStatusLabel(status) {
  const value = Number(status);
  if (value === 1) return 'Rascunho';
  if (value === 2) return 'Publicada';
  if (value === 3) return 'Fechada';
  return 'Desconhecido';
}

export default function EscalaEditor() {
  const { ocorrenciaId } = useParams();
  const { usuario } = useAuth();
  const confirmDialog = useConfirmDialog();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const [ocorrencia, setOcorrencia] = useState(null);
  const [escala, setEscala] = useState(null);
  const [equipes, setEquipes] = useState([]);
  const [cargos, setCargos] = useState([]);
  const [voluntarios, setVoluntarios] = useState([]);
  const [sugestoes, setSugestoes] = useState([]);

  const [formItem, setFormItem] = useState({
    equipeId: '',
    cargoId: '',
    voluntarioId: '',
    ordem: '0',
    forcarConflito: false,
    motivoExcecao: '',
  });

  const isAdmin = Number(usuario?.tipoUsuario) === 1 || Number(usuario?.tipoUsuario) === 3;

  const voluntariosFiltrados = useMemo(() => {
    if (!formItem.equipeId) return voluntarios;
    if (sugestoes.length > 0) {
      return sugestoes.map((s) => ({
        id: s.voluntarioId,
        nome: s.voluntarioNome,
        equipeId: s.equipeId,
        disponivel: s.disponivel,
        cargaRecente: s.cargaRecente,
        motivoBloqueio: s.motivoBloqueio,
      }));
    }
    return voluntarios
      .filter((v) => String(v.equipeId) === String(formItem.equipeId))
      .map((v) => ({ ...v, disponivel: true, cargaRecente: 0, motivoBloqueio: null }));
  }, [voluntarios, formItem.equipeId, sugestoes]);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);

      const [ocorrenciaRes, equipesRes, cargosRes, voluntariosRes] = await Promise.all([
        eventosOcorrenciasApi.getById(ocorrenciaId),
        equipesApi.getAll(),
        cargosApi.getAll(),
        voluntariosApi.getAll(),
      ]);

      setOcorrencia(ocorrenciaRes.data);
      setEquipes(equipesRes.data || []);
      setCargos(cargosRes.data || []);
      setVoluntarios(voluntariosRes.data || []);

      try {
        const escalaRes = await escalasApi.getByOcorrencia(ocorrenciaId);
        setEscala(escalaRes.data);
      } catch (errEscala) {
        if (errEscala.response?.status === 404) {
          setEscala(null);
        } else {
          throw errEscala;
        }
      }
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar dados da escala');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [ocorrenciaId]);

  useEffect(() => {
    const carregarSugestoes = async () => {
      if (!escala?.id || !formItem.equipeId) {
        setSugestoes([]);
        return;
      }
      try {
        const res = await escalasApi.getSugestoes(escala.id, Number(formItem.equipeId));
        setSugestoes(res.data || []);
      } catch (err) {
        console.error('Erro ao carregar sugestões:', err);
        setSugestoes([]);
      }
    };

    carregarSugestoes();
  }, [escala?.id, formItem.equipeId]);

  const ensureEscala = async () => {
    if (escala) return escala;

    const created = await escalasApi.create({
      eventoOcorrenciaId: Number(ocorrenciaId),
      observacoes: null,
    });
    setEscala(created.data);
    toast.success('Escala criada com sucesso');
    return created.data;
  };

  const handleAddItem = async (e) => {
    e.preventDefault();
    if (!formItem.equipeId || !formItem.voluntarioId) {
      toast.error('Selecione equipe e voluntário');
      return;
    }

    if (formItem.forcarConflito && isAdmin && !formItem.motivoExcecao.trim()) {
      toast.error('Informe o motivo da exceção manual');
      return;
    }

    try {
      setSaving(true);
      const escalaAtual = await ensureEscala();
      await escalasApi.addItem(escalaAtual.id, {
        equipeId: Number(formItem.equipeId),
        cargoId: formItem.cargoId ? Number(formItem.cargoId) : null,
        voluntarioId: Number(formItem.voluntarioId),
        ordem: Number(formItem.ordem || 0),
        forcarConflito: !!formItem.forcarConflito,
        motivoExcecao: formItem.forcarConflito ? formItem.motivoExcecao.trim() : null,
      });

      setFormItem({
        equipeId: '',
        cargoId: '',
        voluntarioId: '',
        ordem: '0',
        forcarConflito: false,
        motivoExcecao: '',
      });

      await load();
      toast.success('Voluntário adicionado à escala');
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao adicionar item na escala');
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteItem = async (item) => {
    confirmDialog.show({
      title: 'Remover da escala',
      description: `Deseja remover "${item.voluntarioNome}" da equipe "${item.equipeNome}"?`,
      confirmText: 'Remover',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await escalasApi.deleteItem(escala.id, item.id);
          toast.success('Item removido da escala');
          await load();
        } catch (err) {
          console.error(err);
          toast.error('Erro ao remover item da escala');
          throw err;
        }
      },
    });
  };

  const handlePublicar = async () => {
    if (!escala) return;
    try {
      await escalasApi.publicar(escala.id);
      toast.success('Escala publicada com sucesso');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao publicar escala');
      toast.error(message);
    }
  };

  if (loading) return <LoadingPage text="Carregando editor de escala..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!ocorrencia) return <ErrorPage message="Ocorrência não encontrada" onRetry={load} />;

  const escalaStatusLabel = escala ? getEscalaStatusLabel(escala.status) : 'Não criada';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" asChild>
            <Link to="/voluntariado/escalas">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Voltar
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Montagem de Escala</h1>
            <p className="text-muted-foreground">
              {ocorrencia.eventoTitulo} - {new Date(ocorrencia.dataHoraInicio).toLocaleString('pt-BR')}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="px-2 py-1 rounded text-xs bg-gray-100 text-gray-800">Status: {escalaStatusLabel}</span>
          <Button
            onClick={handlePublicar}
            disabled={!escala || !escala.itens?.length}
          >
            <Send className="h-4 w-4 mr-2" />
            Publicar Escala
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Adicionar Voluntário</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleAddItem} className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Equipe *</Label>
              <Select
                value={formItem.equipeId || 'all'}
                onValueChange={(value) => setFormItem((p) => ({ ...p, equipeId: value === 'all' ? '' : value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecione a equipe" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Selecione</SelectItem>
                  {equipes.map((equipe) => (
                    <SelectItem key={equipe.id} value={String(equipe.id)}>
                      {equipe.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Cargo</Label>
              <Select
                value={formItem.cargoId || 'all'}
                onValueChange={(value) => setFormItem((p) => ({ ...p, cargoId: value === 'all' ? '' : value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecione o cargo" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Sem cargo</SelectItem>
                  {cargos.map((cargo) => (
                    <SelectItem key={cargo.id} value={String(cargo.id)}>
                      {cargo.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Voluntário *</Label>
              <Select
                value={formItem.voluntarioId || 'all'}
                onValueChange={(value) => setFormItem((p) => ({ ...p, voluntarioId: value === 'all' ? '' : value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecione o voluntário" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Selecione</SelectItem>
                  {voluntariosFiltrados.map((voluntario) => (
                    <SelectItem key={voluntario.id} value={String(voluntario.id)}>
                      {voluntario.nome}
                      {formItem.equipeId && (
                        ` ${voluntario.disponivel ? `(carga: ${voluntario.cargaRecente})` : '(já escalado no evento)'}`
                      )}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Ordem</Label>
              <Input
                type="number"
                min="0"
                value={formItem.ordem}
                onChange={(e) => setFormItem((p) => ({ ...p, ordem: e.target.value }))}
              />
            </div>

            {isAdmin && (
              <>
                <div className="space-y-2">
                  <Label>Exceção manual</Label>
                  <div className="flex items-center gap-2 h-9">
                    <input
                      id="forcarConflito"
                      type="checkbox"
                      checked={formItem.forcarConflito}
                      onChange={(e) => setFormItem((p) => ({ ...p, forcarConflito: e.target.checked }))}
                    />
                    <Label htmlFor="forcarConflito">Forçar alocação com conflito</Label>
                  </div>
                </div>
                <div className="space-y-2 md:col-span-2">
                  <Label>Motivo da exceção</Label>
                  <Input
                    value={formItem.motivoExcecao}
                    onChange={(e) => setFormItem((p) => ({ ...p, motivoExcecao: e.target.value }))}
                    placeholder="Obrigatório se marcar exceção manual"
                    disabled={!formItem.forcarConflito}
                  />
                </div>
              </>
            )}

            <div className="md:col-span-3">
              <Button type="submit" disabled={saving}>
                <Plus className="h-4 w-4 mr-2" />
                {saving ? 'Adicionando...' : 'Adicionar à Escala'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Itens da Escala ({escala?.itens?.length || 0})</CardTitle>
        </CardHeader>
        <CardContent>
          {!escala || !escala.itens?.length ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum voluntário adicionado à escala.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Equipe</TableHead>
                  <TableHead>Cargo</TableHead>
                  <TableHead>Voluntário</TableHead>
                  <TableHead>Ordem</TableHead>
                  <TableHead>Exceção</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {escala.itens.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>{item.equipeNome}</TableCell>
                    <TableCell>{item.cargoNome || '-'}</TableCell>
                    <TableCell className="font-medium">{item.voluntarioNome}</TableCell>
                    <TableCell>{item.ordem}</TableCell>
                    <TableCell>
                      {item.conflitoAprovado ? (
                        <span className="px-2 py-1 rounded text-xs bg-amber-100 text-amber-800">Exceção manual</span>
                      ) : (
                        '-'
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="ghost" size="sm" onClick={() => handleDeleteItem(item)}>
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
