import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2, Send, Wand2, CheckCircle2, XCircle, UserCheck, UserX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { equipesApi, escalasApi, eventosOcorrenciasApi, solicitacoesTrocasEscalasApi, voluntariosApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { useTranslation } from 'react-i18next';

function getEscalaStatusLabel(status) {
  const value = Number(status);
  if (value === 1) return 'Rascunho';
  if (value === 2) return 'Publicada';
  if (value === 3) return 'Fechada';
  return 'Desconhecido';
}

function getEscalaStatusClassName(status) {
  const value = Number(status);
  if (value === 1) return 'border border-amber-500/40 bg-amber-500/15 text-amber-200';
  if (value === 2) return 'border border-emerald-500/40 bg-emerald-500/15 text-emerald-200';
  if (value === 3) return 'border border-slate-500/40 bg-slate-500/15 text-slate-200';
  return 'border border-muted bg-muted/20 text-muted-foreground';
}

function getEscalaItemStatusLabel(status) {
  const value = Number(status);
  if (value === 1) return 'Pendente';
  if (value === 2) return 'Confirmado';
  if (value === 3) return 'Recusado';
  if (value === 4) return 'Substituído';
  if (value === 5) return 'Serviu';
  if (value === 6) return 'Faltou';
  return 'Desconhecido';
}

function getActionButtonProps(item, action) {
  const status = Number(item.status);

  if (action === 'confirmar') {
    return status === 2
      ? {
          label: 'Confirmado',
          className: '!border-emerald-600 !bg-emerald-600 !text-white hover:!bg-emerald-700 hover:!text-white',
        }
      : {
          label: 'Confirmar',
          className: '',
        };
  }

  if (action === 'recusar') {
    return status === 3
      ? {
          label: 'Recusado',
          className: '!border-rose-600 !bg-rose-600 !text-white hover:!bg-rose-700 hover:!text-white',
        }
      : {
          label: 'Recusar',
          className: '',
        };
  }

  if (action === 'serviu') {
    return status === 5
      ? {
          label: 'Serviu',
          className: '!border-sky-600 !bg-sky-600 !text-white hover:!bg-sky-700 hover:!text-white',
        }
      : {
          label: 'Serviu',
          className: '',
        };
  }

  if (action === 'faltou') {
    return status === 6
      ? {
          label: 'Faltou',
          className: '!border-amber-500 !bg-amber-400 !text-black hover:!bg-amber-400 hover:!text-black',
        }
      : {
          label: 'Faltou',
          className: '',
        };
  }

  return {
    label: '',
    className: '',
  };
}

export default function EscalaEditor() {
  const { ocorrenciaId, equipeId } = useParams();
  const { usuario } = useAuth();
  const confirmDialog = useConfirmDialog();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const [ocorrencia, setOcorrencia] = useState(null);
  const [escala, setEscala] = useState(null);
  const [equipes, setEquipes] = useState([]);
  const [voluntarios, setVoluntarios] = useState([]);
  const [sugestoes, setSugestoes] = useState([]);
  const [solicitacoesTroca, setSolicitacoesTroca] = useState([]);

  const [formItem, setFormItem] = useState({
    equipeId: '',
    voluntarioId: '',
    forcarConflito: false,
    motivoExcecao: '',
  });
  const [gerandoAuto, setGerandoAuto] = useState(false);
  const [trocaDialogOpen, setTrocaDialogOpen] = useState(false);
  const [trocaSelecionada, setTrocaSelecionada] = useState(null);
  const [substitutosDisponiveis, setSubstitutosDisponiveis] = useState([]);
  const [substitutoSelecionado, setSubstitutoSelecionado] = useState('');

  const isAdmin = Number(usuario?.tipoUsuario) === 1 || Number(usuario?.tipoUsuario) === 3;
  const { t } = useTranslation();
  const escalaRascunho = escala && Number(escala.status) === 1;

  const voluntariosFiltrados = useMemo(() => {
    if (!formItem.equipeId) return [];

    const idsJaEscalados = new Set((escala?.itens || []).map((item) => Number(item.voluntarioId)));

    if (sugestoes.length > 0) {
      return sugestoes
        .filter((s) => s.disponivel && Number(s.equipeId) === Number(formItem.equipeId) && !idsJaEscalados.has(Number(s.voluntarioId)))
        .map((s) => ({
        id: s.voluntarioId,
        nome: s.voluntarioNome,
        equipeId: s.equipeId,
        cargoId: s.cargoId,
        cargoNome: s.cargoNome,
        disponivel: s.disponivel,
        cargaRecente: s.cargaRecente,
        motivoBloqueio: s.motivoBloqueio,
      }));
    }

    return voluntarios
      .filter((v) => String(v.equipeId) === String(formItem.equipeId) && !idsJaEscalados.has(Number(v.id)))
      .map((v) => ({ ...v, disponivel: true, cargaRecente: 0, motivoBloqueio: null }));
  }, [voluntarios, formItem.equipeId, sugestoes, escala?.itens]);

  const voluntarioSelecionado = useMemo(
    () => voluntariosFiltrados.find((item) => String(item.id) === String(formItem.voluntarioId)),
    [voluntariosFiltrados, formItem.voluntarioId]
  );

  const load = async () => {
    try {
      setLoading(true);
      setError(null);

      const [ocorrenciaRes, equipesRes, voluntariosRes] = await Promise.all([
        eventosOcorrenciasApi.getById(ocorrenciaId),
        equipesApi.getAll(),
        voluntariosApi.getAll(),
      ]);

      setOcorrencia(ocorrenciaRes.data);
      setEquipes(equipesRes.data || []);
      setVoluntarios(voluntariosRes.data || []);

      try {
        const escalaRes = await escalasApi.getByOcorrenciaAndEquipe(ocorrenciaId, equipeId);
        setEscala(escalaRes.data);
        const solicitacoesRes = await solicitacoesTrocasEscalasApi.getByEscala(escalaRes.data.id);
        setSolicitacoesTroca(solicitacoesRes.data || []);
      } catch (errEscala) {
        if (errEscala.response?.status === 404) {
          setEscala(null);
          setSolicitacoesTroca([]);
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
  }, [ocorrenciaId, equipeId]);

  useEffect(() => {
    if (equipeId) setFormItem((p) => ({ ...p, equipeId: String(equipeId) }));
  }, [equipeId]);

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
  }, [escala?.id, escala?.itens?.length, formItem.equipeId]);

  const ensureEscala = async () => {
    if (escala) return escala;

    const created = await escalasApi.create({
      eventoOcorrenciaId: Number(ocorrenciaId),
      equipeId: Number(equipeId),
      observacoes: null,
    });
    setEscala(created.data);
    toast.success('Escala criada');
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
        cargoId: voluntarioSelecionado?.cargoId ? Number(voluntarioSelecionado.cargoId) : null,
        voluntarioId: Number(formItem.voluntarioId),
        ordem: 0,
        forcarConflito: !!formItem.forcarConflito,
        motivoExcecao: formItem.forcarConflito ? formItem.motivoExcecao.trim() : null,
      });

      setFormItem({
        equipeId: String(equipeId || formItem.equipeId || ''),
        voluntarioId: '',
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

  const handleGerarAutomatico = async () => {
    try {
      setGerandoAuto(true);
      await escalasApi.gerarAutomatico(ocorrenciaId, equipeId);
      toast.success('Escala preenchida automaticamente');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao preencher escala');
      toast.error(message);
    } finally {
      setGerandoAuto(false);
    }
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

  const handleConfirmarItem = async (item) => {
    if (!escala) return;
    try {
      await escalasApi.confirmarItem(escala.id, item.id);
      toast.success('Item confirmado');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao confirmar item');
      toast.error(message);
    }
  };

  const handleRecusarItem = async (item) => {
    if (!escala) return;

    const motivoRecusa = window.prompt(`Motivo da recusa de ${item.voluntarioNome}:`, item.motivoRecusa || '');
    if (motivoRecusa === null) return;

    try {
      await escalasApi.recusarItem(escala.id, item.id, { motivoRecusa });
      toast.success('Item marcado como recusado');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao recusar item');
      toast.error(message);
    }
  };

  const handleAprovarTroca = async (solicitacao) => {
    if (!escala) return;

    const res = await escalasApi.getSugestoes(escala.id, Number(equipeId));
    const disponiveis = (res.data || []).filter((x) => x.disponivel && x.voluntarioId !== solicitacao.voluntarioSolicitanteId);
    if (!disponiveis.length) {
      toast.error('Nenhum substituto disponível no momento');
      return;
    }
    setTrocaSelecionada(solicitacao);
    setSubstitutosDisponiveis(disponiveis);
    setSubstitutoSelecionado(String(disponiveis[0].voluntarioId));
    setTrocaDialogOpen(true);
  };

  const confirmAprovarTroca = async () => {
    if (!trocaSelecionada || !substitutoSelecionado) return;
    try {
      await solicitacoesTrocasEscalasApi.aprovar(trocaSelecionada.id, {
        voluntarioSubstitutoId: Number(substitutoSelecionado),
        observacaoResposta: null,
      });
      toast.success('Solicitação aprovada');
      setTrocaDialogOpen(false);
      setTrocaSelecionada(null);
      setSubstitutosDisponiveis([]);
      setSubstitutoSelecionado('');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao aprovar solicitação');
      toast.error(message);
    }
  };

  const handleRejeitarTroca = async (solicitacao) => {
    const observacaoResposta = window.prompt(`Motivo da rejeição da solicitação de ${solicitacao.voluntarioSolicitanteNome}:`, '');
    if (observacaoResposta === null) return;

    try {
      await solicitacoesTrocasEscalasApi.rejeitar(solicitacao.id, { observacaoResposta });
      toast.success('Solicitação rejeitada');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao rejeitar solicitação');
      toast.error(message);
    }
  };

  const handleRegistrarPresenca = async (item, compareceu) => {
    if (!escala) return;

    const observacaoOperacional = window.prompt(
      compareceu
        ? `Observação de presença para ${item.voluntarioNome}:`
        : `Observação de falta para ${item.voluntarioNome}:`,
      item.observacaoOperacional || ''
    );

    if (observacaoOperacional === null) return;

    try {
      await escalasApi.registrarPresenca(escala.id, item.id, {
        compareceu,
        observacaoOperacional,
      });
      toast.success(compareceu ? 'Presença registrada' : 'Falta registrada');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao registrar presença');
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
            <Link to={`/voluntariado/escalas/ocorrencia/${ocorrenciaId}`}>
              <ArrowLeft className="h-4 w-4 mr-2" />
              Voltar
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">
              {t('volunteer.schedules.editorTitle')}
              {escala?.equipeNome ? ` — ${escala.equipeNome}` : ''}
            </h1>
            <p className="text-muted-foreground">
              {ocorrencia.eventoTitulo} — {new Date(ocorrencia.dataHoraInicio).toLocaleString('pt-BR')}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className={`inline-flex items-center rounded-full px-3 py-1 text-sm font-semibold ${getEscalaStatusClassName(escala?.status)}`}>
            {escalaStatusLabel}
          </span>
          {escalaRascunho && (
            <Button
              variant="outline"
              onClick={handleGerarAutomatico}
              disabled={gerandoAuto}
            >
              <Wand2 className="h-4 w-4 mr-2" />
              {gerandoAuto ? 'Preenchendo...' : 'Preencher automaticamente'}
            </Button>
          )}
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
            {equipeId && (
              <div className="space-y-2">
                <Label>Equipe</Label>
                <p className="text-sm py-2 text-muted-foreground">
                  {escala?.equipeNome || equipes.find((e) => String(e.id) === String(equipeId))?.nome || `Equipe ${equipeId}`}
                </p>
              </div>
            )}
            {!equipeId && (
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
            )}

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
                      {`${voluntario.nome}${voluntario.cargoNome ? ` - ${voluntario.cargoNome}` : ''}${formItem.equipeId ? ` (carga: ${voluntario.cargaRecente})` : ''}`}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
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
                  <TableHead>Status</TableHead>
                  <TableHead>Resposta</TableHead>
                  <TableHead>Observações</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {escala.itens.map((item) => (
                  <TableRow key={item.id}>
                    {(() => {
                      const confirmarButton = getActionButtonProps(item, 'confirmar');
                      const recusarButton = getActionButtonProps(item, 'recusar');
                      const serviuButton = getActionButtonProps(item, 'serviu');
                      const faltouButton = getActionButtonProps(item, 'faltou');

                      return (
                        <>
                    <TableCell>{item.equipeNome}</TableCell>
                    <TableCell>{item.cargoNome || '-'}</TableCell>
                    <TableCell className="font-medium">{item.voluntarioNome}</TableCell>
                    <TableCell>{getEscalaItemStatusLabel(item.status)}</TableCell>
                    <TableCell>
                      {item.dataConfirmacao
                        ? new Date(item.dataConfirmacao).toLocaleString('pt-BR')
                        : item.dataRecusa
                          ? new Date(item.dataRecusa).toLocaleString('pt-BR')
                          : '-'}
                    </TableCell>
                    <TableCell>
                      {item.observacaoOperacional || item.motivoRecusa || (item.conflitoAprovado ? 'Exceção manual' : '-')}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          className={confirmarButton.className}
                          onClick={() => handleConfirmarItem(item)}
                        >
                          <CheckCircle2 className="h-4 w-4 mr-2" />
                          {confirmarButton.label}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          className={recusarButton.className}
                          onClick={() => handleRecusarItem(item)}
                        >
                          <XCircle className="h-4 w-4 mr-2" />
                          {recusarButton.label}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          className={serviuButton.className}
                          onClick={() => handleRegistrarPresenca(item, true)}
                        >
                          <UserCheck className="h-4 w-4 mr-2" />
                          {serviuButton.label}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          className={faltouButton.className}
                          onClick={() => handleRegistrarPresenca(item, false)}
                        >
                          <UserX className="h-4 w-4 mr-2" />
                          {faltouButton.label}
                        </Button>
                        {escalaRascunho && (
                          <Button variant="ghost" size="sm" onClick={() => handleDeleteItem(item)}>
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                        </>
                      );
                    })()}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {solicitacoesTroca.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Solicitações de troca</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Solicitante</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Motivo</TableHead>
                  <TableHead>Substituto</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {solicitacoesTroca.map((solicitacao) => (
                  <TableRow key={solicitacao.id}>
                    <TableCell className="font-medium">{solicitacao.voluntarioSolicitanteNome}</TableCell>
                    <TableCell>{solicitacao.status === 1 ? 'Pendente' : solicitacao.status === 2 ? 'Aprovada' : 'Rejeitada'}</TableCell>
                    <TableCell>{solicitacao.motivo || '-'}</TableCell>
                    <TableCell>{solicitacao.voluntarioSubstitutoNome || '-'}</TableCell>
                    <TableCell className="text-right">
                      {solicitacao.status === 1 ? (
                        <div className="flex items-center justify-end gap-2">
                          <Button variant="outline" size="sm" onClick={() => handleAprovarTroca(solicitacao)}>
                            Aprovar
                          </Button>
                          <Button variant="outline" size="sm" onClick={() => handleRejeitarTroca(solicitacao)}>
                            Rejeitar
                          </Button>
                        </div>
                      ) : '-'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      <Dialog open={trocaDialogOpen} onOpenChange={setTrocaDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Aprovar solicitação de troca</DialogTitle>
            <DialogDescription>
              Selecione o substituto disponível para assumir esta escala.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            {substitutosDisponiveis.map((sub) => (
              <label key={sub.voluntarioId} className="flex items-center justify-between rounded-lg border p-3 cursor-pointer">
                <div>
                  <div className="font-medium">{sub.voluntarioNome}</div>
                  <div className="text-sm text-muted-foreground">
                    {sub.cargoNome || 'Sem cargo'} • Carga recente: {sub.cargaRecente}
                  </div>
                </div>
                <input
                  type="radio"
                  name="substituto"
                  value={sub.voluntarioId}
                  checked={String(sub.voluntarioId) === substitutoSelecionado}
                  onChange={(e) => setSubstitutoSelecionado(e.target.value)}
                />
              </label>
            ))}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setTrocaDialogOpen(false)}>Cancelar</Button>
            <Button onClick={confirmAprovarTroca}>Confirmar substituto</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

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
