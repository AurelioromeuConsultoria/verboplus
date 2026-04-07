import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2, Send, Wand2, CheckCircle2, XCircle, UserCheck, UserX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { equipesApi, escalasApi, escalasModelosApi, eventosOcorrenciasApi, solicitacoesTrocasEscalasApi, voluntariosApi } from '@/lib/api';
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

function getEscalaItemStatusClassName(status) {
  const value = Number(status);
  if (value === 1) return 'border-amber-500/40 bg-amber-500/15 text-amber-200';
  if (value === 2) return 'border-emerald-500/40 bg-emerald-500/15 text-emerald-200';
  if (value === 3) return 'border-rose-500/40 bg-rose-500/15 text-rose-200';
  if (value === 4) return 'border-slate-500/40 bg-slate-500/15 text-slate-200';
  if (value === 5) return 'border-sky-500/40 bg-sky-500/15 text-sky-200';
  if (value === 6) return 'border-amber-500/50 bg-amber-400/20 text-amber-100';
  return 'border-muted bg-muted/20 text-muted-foreground';
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
  const { isAdmin } = useAuth();
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
  const [modeloEscala, setModeloEscala] = useState(null);
  const [gerandoAuto, setGerandoAuto] = useState(false);
  const [trocaDialogOpen, setTrocaDialogOpen] = useState(false);
  const [trocaSelecionada, setTrocaSelecionada] = useState(null);
  const [substitutosDisponiveis, setSubstitutosDisponiveis] = useState([]);
  const [substitutoSelecionado, setSubstitutoSelecionado] = useState('');

  const { t } = useTranslation();
  const escalaRascunho = escala && Number(escala.status) === 1;

  const voluntariosElegiveis = useMemo(() => {
    const idsJaEscalados = new Set((escala?.itens || []).map((item) => Number(item.voluntarioId)));

    if (sugestoes.length > 0) {
      return sugestoes
        .filter((s) => Number(s.equipeId) === Number(equipeId))
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
      .filter((v) => String(v.equipeId) === String(equipeId))
      .map((v) => ({
        ...v,
        disponivel: !idsJaEscalados.has(Number(v.id)),
        cargaRecente: 0,
        motivoBloqueio: idsJaEscalados.has(Number(v.id)) ? 'Já está na escala' : null,
      }));
  }, [voluntarios, sugestoes, escala?.itens, equipeId]);

  const voluntariosDisponiveis = useMemo(
    () => voluntariosElegiveis.filter((item) => item.disponivel && !(escala?.itens || []).some((escalaItem) => Number(escalaItem.voluntarioId) === Number(item.id))),
    [voluntariosElegiveis, escala?.itens]
  );

  const voluntariosBloqueados = useMemo(
    () => voluntariosElegiveis.filter((item) => !item.disponivel),
    [voluntariosElegiveis]
  );

  const coberturaModelo = useMemo(() => {
    if (!modeloEscala?.itens?.length) return [];

    const itensEscala = escala?.itens || [];

    return modeloEscala.itens.map((itemModelo) => {
      const preenchidos = itensEscala.filter((itemEscala) => {
        if (itemModelo.cargoId == null) return itemEscala.cargoId == null;
        return Number(itemEscala.cargoId) === Number(itemModelo.cargoId);
      }).length;

      const necessario = Number(itemModelo.quantidade || 0);
      const faltando = Math.max(0, necessario - preenchidos);

      return {
        id: itemModelo.id,
        cargoNome: itemModelo.cargoNome || 'Sem cargo definido',
        necessario,
        preenchidos,
        faltando,
        completo: faltando === 0,
      };
    });
  }, [modeloEscala, escala?.itens]);

  const resumoEscala = useMemo(() => {
    const itens = escala?.itens || [];
    return itens.reduce((acc, item) => {
      acc.total += 1;
      const status = Number(item.status);
      if (status === 1) acc.pendentes += 1;
      if (status === 2) acc.confirmados += 1;
      if (status === 3) acc.recusados += 1;
      if (status === 5) acc.serviram += 1;
      if (status === 6) acc.faltaram += 1;
      return acc;
    }, { total: 0, pendentes: 0, confirmados: 0, recusados: 0, serviram: 0, faltaram: 0 });
  }, [escala?.itens]);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);

      const [ocorrenciaRes, equipesRes, voluntariosRes] = await Promise.all([
        eventosOcorrenciasApi.getById(ocorrenciaId),
        equipesApi.getAll(),
        voluntariosApi.getAll(),
      ]);

      const ocorrenciaData = ocorrenciaRes.data;
      setOcorrencia(ocorrenciaData);
      setEquipes(equipesRes.data || []);
      setVoluntarios(voluntariosRes.data || []);

      try {
        const modeloRes = await escalasModelosApi.getByEventoAndEquipe(ocorrenciaData.eventoId, Number(equipeId));
        setModeloEscala(modeloRes.data || null);
      } catch (errModelo) {
        if (errModelo.response?.status === 404) {
          setModeloEscala(null);
        } else {
          console.error('Erro ao carregar modelo da escala:', errModelo);
          setModeloEscala(null);
        }
      }

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
    const carregarSugestoes = async () => {
      if (!escala?.id || !equipeId) {
        setSugestoes([]);
        return;
      }
      try {
        const res = await escalasApi.getSugestoes(escala.id, Number(equipeId));
        setSugestoes(res.data || []);
      } catch (err) {
        console.error('Erro ao carregar sugestões:', err);
        setSugestoes([]);
      }
    };

    carregarSugestoes();
  }, [escala?.id, escala?.itens?.length, equipeId]);

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

  const handleAddVoluntario = async (voluntario, forcarConflito = false) => {
    try {
      setSaving(true);
      const escalaAtual = await ensureEscala();
      let motivoExcecao = null;

      if (forcarConflito) {
        motivoExcecao = window.prompt(`Motivo da exceção para ${voluntario.nome}:`, '')?.trim();
        if (!motivoExcecao) {
          return;
        }
      }

      await escalasApi.addItem(escalaAtual.id, {
        equipeId: Number(equipeId),
        cargoId: voluntario?.cargoId ? Number(voluntario.cargoId) : null,
        voluntarioId: Number(voluntario.id),
        ordem: 0,
        forcarConflito,
        motivoExcecao,
      });

      await load();
      toast.success(forcarConflito ? 'Voluntário adicionado com exceção manual' : 'Voluntário adicionado à escala');
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

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <Card><CardHeader><CardTitle>Total</CardTitle></CardHeader><CardContent className="text-2xl font-bold">{resumoEscala.total}</CardContent></Card>
        <Card><CardHeader><CardTitle>Confirmados</CardTitle></CardHeader><CardContent className="text-2xl font-bold text-emerald-400">{resumoEscala.confirmados}</CardContent></Card>
        <Card><CardHeader><CardTitle>Pendentes</CardTitle></CardHeader><CardContent className="text-2xl font-bold text-amber-400">{resumoEscala.pendentes}</CardContent></Card>
        <Card><CardHeader><CardTitle>Recusas</CardTitle></CardHeader><CardContent className="text-2xl font-bold text-rose-400">{resumoEscala.recusados}</CardContent></Card>
        <Card><CardHeader><CardTitle>Dia do evento</CardTitle></CardHeader><CardContent className="text-sm text-muted-foreground">{new Date(ocorrencia.dataHoraInicio).toLocaleString('pt-BR')}</CardContent></Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Cobertura do modelo</CardTitle>
        </CardHeader>
        <CardContent>
          {!modeloEscala ? (
            <div className="flex items-center justify-between gap-4 rounded-lg border border-dashed p-4">
              <div>
                <div className="font-medium">Nenhum modelo configurado para esta equipe neste evento.</div>
                <div className="text-sm text-muted-foreground">
                  O preenchimento manual funciona, mas você fica sem uma referência clara do mínimo necessário.
                </div>
              </div>
              <Button variant="outline" asChild>
                <Link to={`/voluntariado/modelos-escala/novo?equipeId=${equipeId}`}>
                  Criar modelo
                </Link>
              </Button>
            </div>
          ) : coberturaModelo.length === 0 ? (
            <div className="text-sm text-muted-foreground">Este modelo não tem itens definidos.</div>
          ) : (
            <div className="space-y-3">
              {modeloEscala.nome && (
                <div className="text-sm text-muted-foreground">
                  Modelo: <span className="font-medium text-foreground">{modeloEscala.nome}</span>
                </div>
              )}
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                {coberturaModelo.map((item) => (
                  <div
                    key={item.id}
                    className={`rounded-lg border p-4 ${item.completo ? 'border-emerald-500/30 bg-emerald-500/10' : 'border-amber-500/30 bg-amber-500/10'}`}
                  >
                    <div className="font-medium">{item.cargoNome}</div>
                    <div className="mt-2 text-sm text-muted-foreground">
                      Preenchido: <span className="font-semibold text-foreground">{item.preenchidos}/{item.necessario}</span>
                    </div>
                    <div className={`mt-1 text-sm font-medium ${item.completo ? 'text-emerald-300' : 'text-amber-300'}`}>
                      {item.completo ? 'Cobertura completa' : `Faltam ${item.faltando}`}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_1.4fr]">
        <Card>
          <CardHeader>
            <CardTitle>Quem pode servir agora</CardTitle>
            <p className="text-sm text-muted-foreground">
              Escolha direto na lista. A tela já prioriza quem está disponível para esta equipe e ocorrência.
            </p>
          </CardHeader>
          <CardContent className="space-y-3">
            {voluntariosDisponiveis.length === 0 ? (
              <div className="rounded-lg border border-dashed p-6 text-sm text-muted-foreground">
                Nenhum voluntário disponível no momento para esta equipe.
              </div>
            ) : (
              voluntariosDisponiveis.map((voluntario) => (
                <div key={voluntario.id} className="flex items-center justify-between rounded-xl border p-4 gap-4">
                  <div className="space-y-1">
                    <div className="font-medium">{voluntario.nome}</div>
                    <div className="text-sm text-muted-foreground">
                      {voluntario.cargoNome || 'Sem cargo'} • Histórico recente: {voluntario.cargaRecente}
                    </div>
                  </div>
                  <Button size="sm" onClick={() => handleAddVoluntario(voluntario)} disabled={saving || Boolean(escala && !escalaRascunho)}>
                    <Plus className="h-4 w-4 mr-2" />
                    Adicionar
                  </Button>
                </div>
              ))
            )}

            {isAdmin && voluntariosBloqueados.length > 0 && (
              <div className="space-y-3 pt-2">
                <div className="text-sm font-medium text-muted-foreground">Bloqueados para esta ocorrência</div>
                {voluntariosBloqueados.map((voluntario) => (
                  <div key={voluntario.id} className="flex items-center justify-between rounded-xl border border-dashed p-4 gap-4 opacity-80">
                    <div className="space-y-1">
                      <div className="font-medium">{voluntario.nome}</div>
                      <div className="text-sm text-muted-foreground">
                        {voluntario.cargoNome || 'Sem cargo'} • {voluntario.motivoBloqueio || 'Indisponível'}
                      </div>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleAddVoluntario(voluntario, true)}
                      disabled={saving || Boolean(escala && !escalaRascunho)}
                    >
                      Adicionar com exceção
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Escala montada ({escala?.itens?.length || 0})</CardTitle>
            <p className="text-sm text-muted-foreground">
              Acompanhe respostas e faça ajustes sem sair da mesma visão.
            </p>
          </CardHeader>
          <CardContent className="space-y-3">
            {!escala || !escala.itens?.length ? (
              <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
                Nenhum voluntário adicionado à escala ainda.
              </div>
            ) : (
              escala.itens.map((item) => {
                const confirmarButton = getActionButtonProps(item, 'confirmar');
                const recusarButton = getActionButtonProps(item, 'recusar');
                const serviuButton = getActionButtonProps(item, 'serviu');
                const faltouButton = getActionButtonProps(item, 'faltou');

                return (
                  <div key={item.id} className="rounded-2xl border p-4 space-y-4">
                    <div className="flex items-start justify-between gap-4">
                      <div className="space-y-1">
                        <div className="font-medium text-base">{item.voluntarioNome}</div>
                        <div className="text-sm text-muted-foreground">
                          {item.cargoNome || 'Sem cargo'} • {item.equipeNome}
                        </div>
                      </div>
                      <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${getEscalaItemStatusClassName(item.status)}`}>
                        {getEscalaItemStatusLabel(item.status)}
                      </span>
                    </div>

                    <div className="grid gap-3 md:grid-cols-2">
                      <div className="text-sm text-muted-foreground">
                        Resposta: {item.dataConfirmacao
                          ? new Date(item.dataConfirmacao).toLocaleString('pt-BR')
                          : item.dataRecusa
                            ? new Date(item.dataRecusa).toLocaleString('pt-BR')
                            : 'Aguardando resposta'}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Observações: {item.observacaoOperacional || item.motivoRecusa || (item.conflitoAprovado ? 'Exceção manual' : '—')}
                      </div>
                    </div>

                    <div className="flex flex-wrap gap-2">
                      <Button variant="outline" size="sm" className={confirmarButton.className} onClick={() => handleConfirmarItem(item)}>
                        <CheckCircle2 className="h-4 w-4 mr-2" />
                        {confirmarButton.label}
                      </Button>
                      <Button variant="outline" size="sm" className={recusarButton.className} onClick={() => handleRecusarItem(item)}>
                        <XCircle className="h-4 w-4 mr-2" />
                        {recusarButton.label}
                      </Button>
                      <Button variant="outline" size="sm" className={serviuButton.className} onClick={() => handleRegistrarPresenca(item, true)}>
                        <UserCheck className="h-4 w-4 mr-2" />
                        {serviuButton.label}
                      </Button>
                      <Button variant="outline" size="sm" className={faltouButton.className} onClick={() => handleRegistrarPresenca(item, false)}>
                        <UserX className="h-4 w-4 mr-2" />
                        {faltouButton.label}
                      </Button>
                      {escalaRascunho && (
                        <Button variant="ghost" size="sm" onClick={() => handleDeleteItem(item)}>
                          <Trash2 className="h-4 w-4 mr-2" />
                          Remover
                        </Button>
                      )}
                    </div>
                  </div>
                );
              })
            )}
          </CardContent>
        </Card>
      </div>

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
