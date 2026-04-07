import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Building2,
  Eye,
  Filter,
  LogOut,
  MessageSquareWarning,
  Pencil,
  PlusCircle,
  Search,
  ShieldAlert,
  TriangleAlert,
  Users,
  Layers3,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import { kidsApi } from '../../lib/api';
import Loading from '../../components/ui/loading';
import ErrorMessage from '../../components/ui/error-message';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { CriancaDialog, HistoricoDialog, OcorrenciaDialog, SalaDialog, TurmaDialog } from './components/KidsDialogs';
import { CheckPanelIcon, EstadoVazio, IndicadorLinha, PainelCriancaCard, ResumoCard } from './components/KidsShared';
import { buildCriticalDescription, formatOcorrenciaTipo, getOcorrenciaStatusConfig } from './components/kidsHelpers';

const FORMULARIO_INICIAL = {
  criancaPessoaId: '',
  checkinId: null,
  tipo: 'OUTRO',
  titulo: '',
  descricao: '',
  requerContatoResponsavel: false,
  visivelAoResponsavel: false,
};

const SALA_FORM_INICIAL = {
  id: '',
  nome: '',
  capacidadeMaxima: '',
  ativo: true,
};

const TURMA_FORM_INICIAL = {
  id: '',
  salaId: '',
  nome: '',
  capacidadeMaxima: '',
  ativo: true,
};

const CRIANCA_FORM_INICIAL = {
  nome: '',
  dataNascimento: '',
  salaId: '',
  turmaId: '',
  alergias: '',
  restricoesAlimentares: '',
  observacoes: '',
};

const SECTION_CONFIG = {
  overview: {
    title: 'Painel Kids',
    subtitle: 'Operação do culto em tempo real com presentes, pendências e alertas críticos.',
  },
  painel: {
    title: 'Painel operacional',
    subtitle: 'Visão ao vivo do culto com presença, retirada, alertas e ocorrências.',
  },
  criancas: {
    title: 'Crianças',
    subtitle: 'Cadastro e acompanhamento da base de crianças e responsáveis do módulo.',
  },
  estrutura: {
    title: 'Estrutura',
    subtitle: 'Organize salas e turmas para sustentar capacidade e operação do Kids.',
  },
  historico: {
    title: 'Histórico e ocorrências',
    subtitle: 'Consulte check-ins, retiradas e situações registradas pela equipe.',
  },
};

const KidsCheckinsList = ({ section = 'overview' }) => {
  const { t } = useTranslation();
  const [painel, setPainel] = useState(null);
  const [indicadores, setIndicadores] = useState(null);
  const [checkins, setCheckins] = useState([]);
  const [criancas, setCriancas] = useState([]);
  const [salas, setSalas] = useState([]);
  const [turmas, setTurmas] = useState([]);
  const [ocorrenciasAbertas, setOcorrenciasAbertas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [abaAtiva, setAbaAtiva] = useState('painel');
  const [filtros, setFiltros] = useState({
    criancaPessoaId: '',
    status: '',
    dataInicio: '',
    dataFim: '',
    busca: '',
    salaId: 'todas',
  });
  const [ocorrenciaDialogOpen, setOcorrenciaDialogOpen] = useState(false);
  const [historicoDialogOpen, setHistoricoDialogOpen] = useState(false);
  const [ocorrenciaForm, setOcorrenciaForm] = useState(FORMULARIO_INICIAL);
  const [ocorrenciaSaving, setOcorrenciaSaving] = useState(false);
  const [historicoLoading, setHistoricoLoading] = useState(false);
  const [historicoUpdatingId, setHistoricoUpdatingId] = useState(null);
  const [criancaHistorico, setCriancaHistorico] = useState(null);
  const [ocorrenciasHistorico, setOcorrenciasHistorico] = useState([]);
  const [salaDialogOpen, setSalaDialogOpen] = useState(false);
  const [turmaDialogOpen, setTurmaDialogOpen] = useState(false);
  const [criancaDialogOpen, setCriancaDialogOpen] = useState(false);
  const [salaForm, setSalaForm] = useState(SALA_FORM_INICIAL);
  const [turmaForm, setTurmaForm] = useState(TURMA_FORM_INICIAL);
  const [criancaForm, setCriancaForm] = useState(CRIANCA_FORM_INICIAL);
  const [salaSaving, setSalaSaving] = useState(false);
  const [turmaSaving, setTurmaSaving] = useState(false);
  const [criancaSaving, setCriancaSaving] = useState(false);

  const sectionConfig = SECTION_CONFIG[section] || SECTION_CONFIG.overview;
  const isOverview = section === 'overview';
  const showPainel = isOverview || section === 'painel';
  const showCriancas = isOverview || section === 'criancas';
  const showEstrutura = isOverview || section === 'estrutura';
  const showHistorico = isOverview || section === 'historico';

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const salaFiltro = filtros.salaId && filtros.salaId !== 'todas' ? filtros.salaId : undefined;
      const [painelResponse, indicadoresResponse, checkinsResponse, criancasResponse, ocorrenciasAbertasResponse, salasResponse, turmasResponse] = await Promise.all([
        kidsApi.getPainelOperacional(salaFiltro ? { salaId: salaFiltro } : {}),
        kidsApi.getIndicadores({ dias: 30 }),
        kidsApi.getCheckins(),
        kidsApi.getCriancas(),
        kidsApi.getOcorrenciasAbertas(),
        kidsApi.getSalas(),
        kidsApi.getTurmas(),
      ]);

      setPainel(painelResponse.data);
      setIndicadores(indicadoresResponse.data);
      setCheckins(checkinsResponse.data);
      setCriancas(criancasResponse.data);
      setOcorrenciasAbertas(ocorrenciasAbertasResponse.data || []);
      setSalas(salasResponse.data || []);
      setTurmas(turmasResponse.data || []);
    } catch (err) {
      setError(t('kids.errorLoad', 'Erro ao carregar dados do Kids'));
      console.error('Erro ao buscar dados do painel Kids:', err);
      toast.error(t('kids.toastError', 'Erro ao carregar painel de Kids'));
    } finally {
      setLoading(false);
    }
  }, [filtros.salaId, t]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (section === 'historico' && abaAtiva !== 'historico') {
      setAbaAtiva('historico');
    }
    if (section !== 'historico' && abaAtiva !== 'painel') {
      setAbaAtiva('painel');
    }
  }, [section, abaAtiva]);

  const handleFiltroChange = (name, value) => {
    setFiltros((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const limparFiltros = () => {
    setFiltros({
      criancaPessoaId: '',
      status: '',
      dataInicio: '',
      dataFim: '',
      busca: '',
      salaId: 'todas',
    });
  };

  const salasDisponiveis = useMemo(() => {
    return salas.map((sala) => sala.id);
  }, [salas]);

  const turmasPorSala = useMemo(() => {
    return turmas.reduce((acc, turma) => {
      if (!acc[turma.salaId]) {
        acc[turma.salaId] = [];
      }
      acc[turma.salaId].push(turma);
      return acc;
    }, {});
  }, [turmas]);

  const checkinsFiltrados = useMemo(() => {
    return checkins.filter((checkin) => {
      if (filtros.criancaPessoaId && checkin.criancaPessoaId !== parseInt(filtros.criancaPessoaId, 10)) {
        return false;
      }

      if (filtros.status) {
        const statusLower = (checkin.status || '').toLowerCase();
        if (filtros.status === 'ativo' && statusLower !== 'checkedin' && statusLower !== 'checked_in' && statusLower !== 'ativo') {
          return false;
        }
        if (filtros.status === 'finalizado' && statusLower !== 'checkedout' && statusLower !== 'checked_out' && statusLower !== 'finalizado') {
          return false;
        }
      }

      if (filtros.dataInicio) {
        const dataInicio = new Date(filtros.dataInicio);
        dataInicio.setHours(0, 0, 0, 0);
        const checkinDate = new Date(checkin.checkinTime);
        checkinDate.setHours(0, 0, 0, 0);
        if (checkinDate < dataInicio) {
          return false;
        }
      }

      if (filtros.dataFim) {
        const dataFim = new Date(filtros.dataFim);
        dataFim.setHours(23, 59, 59, 999);
        if (new Date(checkin.checkinTime) > dataFim) {
          return false;
        }
      }

      if (filtros.busca) {
        const buscaLower = filtros.busca.toLowerCase();
        if (!checkin.criancaNome?.toLowerCase().includes(buscaLower)) {
          return false;
        }
      }

      return true;
    });
  }, [checkins, filtros]);

  const criancasOrdenadas = useMemo(() => {
    return [...criancas].sort((a, b) => (a.nome || '').localeCompare(b.nome || ''));
  }, [criancas]);

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatDuration = (checkinTime, checkoutTime) => {
    if (!checkoutTime) return '-';
    const inicio = new Date(checkinTime);
    const fim = new Date(checkoutTime);
    const diff = fim - inicio;
    const horas = Math.floor(diff / (1000 * 60 * 60));
    const minutos = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    return `${horas}h ${minutos}m`;
  };

  const getStatusBadge = (status) => {
    const statusLower = status?.toLowerCase() || '';
    if (statusLower === 'checked_in' || statusLower === 'checkedin' || statusLower === 'ativo') {
      return <Badge className="bg-green-500 hover:bg-green-600">{t('kids.active', 'Ativo')}</Badge>;
    }
    if (statusLower === 'checked_out' || statusLower === 'checkedout' || statusLower === 'finalizado') {
      return <Badge className="bg-gray-500 hover:bg-gray-600">{t('kids.finished', 'Finalizado')}</Badge>;
    }
    return <Badge variant="secondary">{status || t('kids.unknown', 'Desconhecido')}</Badge>;
  };

  const getMetodoBadge = (metodo) => {
    const metodoLower = metodo?.toLowerCase() || '';
    const cores = {
      qr: 'bg-blue-500',
      pin: 'bg-amber-500',
      admin: 'bg-orange-500',
      excecao: 'bg-red-500',
    };
    return <Badge className={cores[metodoLower] || 'bg-gray-500'}>{metodo?.toUpperCase() || 'N/A'}</Badge>;
  };

  const abrirRegistroOcorrencia = (crianca) => {
    const checkinAtivo = checkins.find((item) => item.criancaPessoaId === crianca.criancaPessoaId && !item.checkoutTime);

    setOcorrenciaForm({
      criancaPessoaId: String(crianca.criancaPessoaId),
      checkinId: checkinAtivo?.id ?? null,
      tipo: 'OUTRO',
      titulo: '',
      descricao: '',
      requerContatoResponsavel: false,
      visivelAoResponsavel: false,
    });
    setOcorrenciaDialogOpen(true);
  };

  const carregarHistoricoCrianca = async (crianca) => {
    try {
      setHistoricoLoading(true);
      setCriancaHistorico(crianca);
      setHistoricoDialogOpen(true);
      const response = await kidsApi.getOcorrenciasByCrianca(crianca.criancaPessoaId);
      setOcorrenciasHistorico(response.data || []);
    } catch (err) {
      console.error('Erro ao carregar histórico de ocorrências:', err);
      toast.error('Não foi possível carregar o histórico de ocorrências.');
    } finally {
      setHistoricoLoading(false);
    }
  };

  const handleOcorrenciaFormChange = (name, value) => {
    setOcorrenciaForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSalaFormChange = (name, value) => {
    setSalaForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleTurmaFormChange = (name, value) => {
    setTurmaForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleCriancaFormChange = (name, value) => {
    setCriancaForm((prev) => {
      if (name === 'salaId') {
        const turmaAtual = prev.turmaId;
        const turmaAindaValida = turmas.some((item) => item.id === turmaAtual && item.salaId === value);
        return {
          ...prev,
          salaId: value,
          turmaId: turmaAindaValida ? turmaAtual : '',
        };
      }

      return {
        ...prev,
        [name]: value,
      };
    });
  };

  const handleCriarOcorrencia = async () => {
    if (!ocorrenciaForm.criancaPessoaId || !ocorrenciaForm.titulo.trim() || !ocorrenciaForm.descricao.trim()) {
      toast.error('Preencha criança, título e descrição para registrar a ocorrência.');
      return;
    }

    try {
      setOcorrenciaSaving(true);
      await kidsApi.createOcorrencia({
        criancaPessoaId: Number(ocorrenciaForm.criancaPessoaId),
        checkinId: ocorrenciaForm.checkinId,
        tipo: ocorrenciaForm.tipo,
        titulo: ocorrenciaForm.titulo.trim(),
        descricao: ocorrenciaForm.descricao.trim(),
        requerContatoResponsavel: ocorrenciaForm.requerContatoResponsavel,
        visivelAoResponsavel: ocorrenciaForm.visivelAoResponsavel,
      });

      toast.success('Ocorrência registrada com sucesso.');
      setOcorrenciaDialogOpen(false);
      setOcorrenciaForm(FORMULARIO_INICIAL);
      await fetchData();

      if (criancaHistorico && Number(ocorrenciaForm.criancaPessoaId) === criancaHistorico.criancaPessoaId) {
        await carregarHistoricoCrianca(criancaHistorico);
      }
    } catch (err) {
      console.error('Erro ao criar ocorrência:', err);
      toast.error('Não foi possível registrar a ocorrência.');
    } finally {
      setOcorrenciaSaving(false);
    }
  };

  const handleAtualizarOcorrencia = async (ocorrenciaId, payload) => {
    try {
      setHistoricoUpdatingId(ocorrenciaId);
      await kidsApi.updateOcorrencia(ocorrenciaId, payload);
      toast.success('Ocorrência atualizada.');

      if (criancaHistorico) {
        await carregarHistoricoCrianca(criancaHistorico);
      }
      await fetchData();
    } catch (err) {
      console.error('Erro ao atualizar ocorrência:', err);
      toast.error('Não foi possível atualizar a ocorrência.');
    } finally {
      setHistoricoUpdatingId(null);
    }
  };

  const handleCriarSala = async () => {
    if (!salaForm.id.trim() || !salaForm.nome.trim()) {
      toast.error('Preencha identificador e nome da sala.');
      return;
    }

    try {
      setSalaSaving(true);
      await kidsApi.createSala({
        id: salaForm.id.trim(),
        nome: salaForm.nome.trim(),
        capacidadeMaxima: salaForm.capacidadeMaxima ? Number(salaForm.capacidadeMaxima) : null,
        ativo: salaForm.ativo,
      });
      toast.success('Sala cadastrada com sucesso.');
      setSalaDialogOpen(false);
      setSalaForm(SALA_FORM_INICIAL);
      await fetchData();
    } catch (err) {
      console.error('Erro ao criar sala:', err);
      toast.error('Não foi possível cadastrar a sala.');
    } finally {
      setSalaSaving(false);
    }
  };

  const handleCriarTurma = async () => {
    if (!turmaForm.id.trim() || !turmaForm.nome.trim() || !turmaForm.salaId) {
      toast.error('Preencha identificador, sala e nome da turma.');
      return;
    }

    try {
      setTurmaSaving(true);
      await kidsApi.createTurma({
        id: turmaForm.id.trim(),
        salaId: turmaForm.salaId,
        nome: turmaForm.nome.trim(),
        capacidadeMaxima: turmaForm.capacidadeMaxima ? Number(turmaForm.capacidadeMaxima) : null,
        ativo: turmaForm.ativo,
      });
      toast.success('Turma cadastrada com sucesso.');
      setTurmaDialogOpen(false);
      setTurmaForm(TURMA_FORM_INICIAL);
      await fetchData();
    } catch (err) {
      console.error('Erro ao criar turma:', err);
      toast.error('Não foi possível cadastrar a turma.');
    } finally {
      setTurmaSaving(false);
    }
  };

  const handleCriarCrianca = async () => {
    if (!criancaForm.nome.trim() || !criancaForm.dataNascimento || !criancaForm.salaId) {
      toast.error('Preencha nome, data de nascimento e sala da criança.');
      return;
    }

    try {
      setCriancaSaving(true);
      await kidsApi.createCrianca({
        nome: criancaForm.nome.trim(),
        dataNascimento: new Date(criancaForm.dataNascimento).toISOString(),
        salaId: criancaForm.salaId || null,
        turmaId: criancaForm.turmaId || null,
        alergias: criancaForm.alergias.trim() || null,
        restricoesAlimentares: criancaForm.restricoesAlimentares.trim() || null,
        observacoes: criancaForm.observacoes.trim() || null,
      });
      toast.success('Criança cadastrada com sucesso.');
      setCriancaDialogOpen(false);
      setCriancaForm(CRIANCA_FORM_INICIAL);
      await fetchData();
    } catch (err) {
      console.error('Erro ao cadastrar criança:', err);
      toast.error('Não foi possível cadastrar a criança.');
    } finally {
      setCriancaSaving(false);
    }
  };

  if (loading) {
    return <Loading text={t('kids.loading', 'Carregando...')} />;
  }

  if (error) {
    return <ErrorMessage message={error} onRetry={fetchData} />;
  }

  const painelContent = (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <ResumoCard
          title={t('kids.panel.presentNow', 'Presentes agora')}
          value={painel?.totalPresentes ?? 0}
          description={t('kids.panel.presentNowHint', 'Crianças atualmente em check-in')}
          icon={Users}
          valueClassName="text-blue-600"
        />
        <ResumoCard
          title={t('kids.panel.pendingPickup', 'Pendentes de retirada')}
          value={painel?.totalPendentesRetirada ?? 0}
          description={t('kids.panel.pendingPickupHint', 'Crianças ainda aguardando saída')}
          icon={LogOut}
          valueClassName="text-amber-600"
        />
        <ResumoCard
          title={t('kids.panel.completedToday', 'Retiradas hoje')}
          value={painel?.totalRetiradasHoje ?? 0}
          description={t('kids.panel.completedTodayHint', 'Saídas concluídas no dia')}
          icon={CheckPanelIcon}
          valueClassName="text-emerald-600"
        />
        <ResumoCard
          title="Ocorrências abertas"
          value={ocorrenciasAbertas.length}
          description="Situações em acompanhamento pela equipe"
          icon={MessageSquareWarning}
          valueClassName="text-rose-600"
        />
      </div>

      <div className="grid gap-4 xl:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Indicadores de 30 dias</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-3 text-sm">
            <IndicadorLinha label="Check-ins no período" value={indicadores?.totalCheckinsPeriodo ?? 0} />
            <IndicadorLinha label="Média por dia" value={indicadores?.mediaCheckinsPorDia ?? 0} />
            <IndicadorLinha label="Presentes agora" value={indicadores?.totalCriancasPresentesAgora ?? 0} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Métodos de retirada</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-3 text-sm">
            <IndicadorLinha label="QR" value={indicadores?.totalRetiradasQr ?? 0} />
            <IndicadorLinha label="PIN" value={indicadores?.totalRetiradasPin ?? 0} />
            <IndicadorLinha label="Exceção" value={indicadores?.totalRetiradasExcecao ?? 0} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Base cadastrada</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-3 text-sm">
            <IndicadorLinha label="Crianças ativas" value={indicadores?.totalCriancasAtivas ?? 0} />
            <IndicadorLinha label="Responsáveis ativos" value={indicadores?.totalResponsaveisAtivos ?? 0} />
            <IndicadorLinha label="Salas / turmas" value={`${indicadores?.totalSalasAtivas ?? 0} / ${indicadores?.totalTurmasAtivas ?? 0}`} />
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
        <Card>
          <CardHeader>
            <CardTitle>{t('kids.panel.presentList', 'Presentes agora')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {painel?.criancasPresentes?.length ? (
              painel.criancasPresentes.map((crianca) => (
                <PainelCriancaCard
                  key={crianca.criancaPessoaId}
                  crianca={crianca}
                  onRegistrarOcorrencia={() => abrirRegistroOcorrencia(crianca)}
                  onVerHistorico={() => carregarHistoricoCrianca(crianca)}
                />
              ))
            ) : (
              <EstadoVazio texto={t('kids.panel.noChildrenPresent', 'Nenhuma criança presente neste momento.')} />
            )}
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>{t('kids.panel.byRoom', 'Distribuição por sala')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {painel?.salas?.length ? (
                painel.salas.map((sala) => (
                  <div
                    key={sala.salaId}
                    className="rounded-xl border border-border bg-muted/20 p-4"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h3 className="font-semibold text-foreground">{sala.salaId}</h3>
                        <p className="text-sm text-muted-foreground">
                          {t('kids.panel.roomSummary', {
                            defaultValue: '{{presentes}} presentes • {{pendentes}} pendentes',
                            presentes: sala.totalPresentes,
                            pendentes: sala.totalPendentesRetirada,
                          })}
                        </p>
                      </div>
                      {sala.totalAlertasCriticos > 0 && (
                        <Badge className="bg-red-500 hover:bg-red-600">
                          {t('kids.panel.alertCount', {
                            defaultValue: '{{count}} alertas',
                            count: sala.totalAlertasCriticos,
                          })}
                        </Badge>
                      )}
                    </div>
                  </div>
                ))
              ) : (
                <EstadoVazio texto={t('kids.panel.noRooms', 'Nenhuma sala com presença no momento.')} />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>{t('kids.panel.criticalList', 'Alertas críticos')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {painel?.alertasCriticos?.length ? (
                painel.alertasCriticos.map((crianca) => (
                  <div
                    key={crianca.criancaPessoaId}
                    className="rounded-xl border border-red-200 bg-red-50 p-4"
                  >
                    <div className="flex items-center gap-2">
                      <TriangleAlert className="h-4 w-4 text-red-600" />
                      <span className="font-semibold text-red-900">{crianca.nome}</span>
                    </div>
                    <p className="mt-2 text-sm text-red-800">
                      {buildCriticalDescription(crianca, t)}
                    </p>
                  </div>
                ))
              ) : (
                <EstadoVazio texto={t('kids.panel.noCriticalAlerts', 'Nenhum alerta crítico entre as crianças presentes.')} />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Ocorrências abertas</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {ocorrenciasAbertas.length ? (
                ocorrenciasAbertas.map((ocorrencia) => (
                  <div key={ocorrencia.id} className="rounded-xl border border-border bg-background p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="flex items-center gap-2">
                          <Badge className={getOcorrenciaStatusConfig(ocorrencia.status).className}>
                            {getOcorrenciaStatusConfig(ocorrencia.status).label}
                          </Badge>
                          <span className="font-semibold text-foreground">{ocorrencia.criancaNome}</span>
                        </div>
                        <p className="mt-2 text-sm text-muted-foreground">
                          {formatOcorrenciaTipo(ocorrencia.tipo)} • {formatDate(ocorrencia.dataCriacao)}
                        </p>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => carregarHistoricoCrianca({
                          criancaPessoaId: ocorrencia.criancaPessoaId,
                          nome: ocorrencia.criancaNome,
                        })}
                      >
                        <Eye className="mr-2 h-4 w-4" />
                        Ver
                      </Button>
                    </div>
                  </div>
                ))
              ) : (
                <EstadoVazio texto="Nenhuma ocorrência aberta neste momento." />
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );

  const criancasContent = (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-3">
        <ResumoCard
          title="Crianças cadastradas"
          value={criancas.length}
          description="Base ativa disponível para operação"
          icon={Users}
          valueClassName="text-blue-600"
        />
        <ResumoCard
          title="Em check-in agora"
          value={criancas.filter((crianca) => crianca.estaCheckedIn).length}
          description="Crianças com presença ativa neste momento"
          icon={ShieldAlert}
          valueClassName="text-amber-600"
        />
        <ResumoCard
          title="Com alerta crítico"
          value={criancas.filter((crianca) => crianca.alergias || crianca.restricoesAlimentares || crianca.observacoes).length}
          description="Crianças com dados sensíveis para atenção da equipe"
          icon={TriangleAlert}
          valueClassName="text-rose-600"
        />
      </div>

      <Card>
        <CardHeader className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <CardTitle>Base de crianças</CardTitle>
            <p className="text-sm text-muted-foreground">
              Cadastro administrativo com sala, turma, responsáveis e alertas de cuidado.
            </p>
          </div>
          <Button onClick={() => setCriancaDialogOpen(true)}>
            <PlusCircle className="mr-2 h-4 w-4" />
            Nova criança
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          {criancasOrdenadas.length ? (
            criancasOrdenadas.map((crianca) => (
              <div key={crianca.pessoaId} className="rounded-xl border border-border bg-background p-4">
                <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                  <div className="space-y-2">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-semibold text-foreground">{crianca.nome}</span>
                      {crianca.estaCheckedIn ? (
                        <Badge className="bg-blue-600 hover:bg-blue-700">Em check-in</Badge>
                      ) : (
                        <Badge variant="outline">Fora da sala</Badge>
                      )}
                      {crianca.alergias && <Badge variant="destructive">Alergia</Badge>}
                      {crianca.restricoesAlimentares && <Badge className="bg-amber-600 hover:bg-amber-700">Restrição</Badge>}
                      {crianca.observacoes && <Badge className="bg-rose-600 hover:bg-rose-700">Observação</Badge>}
                    </div>
                    <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
                      <span>Sala: {crianca.salaId || 'Sem sala'}</span>
                      <span>Turma: {crianca.turmaId || 'Sem turma'}</span>
                      <span>Responsáveis: {crianca.responsaveis?.length || 0}</span>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <Button variant="outline" size="sm" onClick={() => carregarHistoricoCrianca({ criancaPessoaId: crianca.pessoaId, nome: crianca.nome })}>
                      <Eye className="mr-2 h-4 w-4" />
                      Ver histórico
                    </Button>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <EstadoVazio texto="Nenhuma criança cadastrada no módulo Kids." />
          )}
        </CardContent>
      </Card>
    </div>
  );

  const estruturaContent = (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-3">
        <ResumoCard
          title="Salas ativas"
          value={salas.filter((sala) => sala.ativo).length}
          description="Salas disponíveis para operação"
          icon={Building2}
          valueClassName="text-blue-600"
        />
        <ResumoCard
          title="Turmas ativas"
          value={turmas.filter((turma) => turma.ativo).length}
          description="Turmas vinculadas à estrutura formal"
          icon={Layers3}
          valueClassName="text-emerald-600"
        />
        <ResumoCard
          title="Capacidade monitorada"
          value={salas.filter((sala) => sala.capacidadeMaxima).length}
          description="Salas com limite configurado"
          icon={ShieldAlert}
          valueClassName="text-amber-600"
        />
      </div>

      <Card>
        <CardHeader className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <CardTitle>Estrutura atual</CardTitle>
            <p className="text-sm text-muted-foreground">
              Cadastre salas e turmas formais para organizar capacidade, distribuição e segurança.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" onClick={() => setSalaDialogOpen(true)}>
              <Building2 className="mr-2 h-4 w-4" />
              Nova sala
            </Button>
            <Button onClick={() => setTurmaDialogOpen(true)}>
              <Layers3 className="mr-2 h-4 w-4" />
              Nova turma
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-3">
          {salas.length ? (
            salas.map((sala) => (
              <div key={sala.id} className="rounded-xl border border-border bg-background p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-foreground">{sala.nome}</span>
                      <Badge variant="outline">{sala.id}</Badge>
                    </div>
                    <p className="mt-1 text-sm text-muted-foreground">
                      Capacidade {sala.capacidadeMaxima || 'não definida'}
                    </p>
                  </div>
                  <Badge className={sala.ativo ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-slate-500 hover:bg-slate-600'}>
                    {sala.ativo ? 'Ativa' : 'Inativa'}
                  </Badge>
                </div>

                <div className="mt-3 space-y-2">
                  {(turmasPorSala[sala.id] || []).length ? (
                    turmasPorSala[sala.id].map((turma) => (
                      <div key={turma.id} className="flex items-center justify-between rounded-lg bg-muted/30 px-3 py-2 text-sm">
                        <div className="flex items-center gap-2">
                          <Pencil className="h-3.5 w-3.5 text-muted-foreground" />
                          <span className="font-medium text-foreground">{turma.nome}</span>
                          <span className="text-muted-foreground">({turma.id})</span>
                        </div>
                        <span className="text-muted-foreground">
                          Cap. {turma.capacidadeMaxima || '-'}
                        </span>
                      </div>
                    ))
                  ) : (
                    <EstadoVazio texto="Nenhuma turma cadastrada para esta sala." />
                  )}
                </div>
              </div>
            ))
          ) : (
            <EstadoVazio texto="Nenhuma sala cadastrada na estrutura do Kids." />
          )}
        </CardContent>
      </Card>
    </div>
  );

  const historicoContent = (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <CardTitle>Ocorrências abertas</CardTitle>
            <p className="text-sm text-muted-foreground">
              Acompanhe registros ativos e abra o histórico da criança quando precisar aprofundar.
            </p>
          </div>
          <Button variant="outline" onClick={() => setOcorrenciaDialogOpen(true)}>
            <PlusCircle className="mr-2 h-4 w-4" />
            Registrar ocorrência
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          {ocorrenciasAbertas.length ? (
            ocorrenciasAbertas.map((ocorrencia) => (
              <div key={ocorrencia.id} className="rounded-xl border border-border bg-background p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="flex items-center gap-2">
                      <Badge className={getOcorrenciaStatusConfig(ocorrencia.status).className}>
                        {getOcorrenciaStatusConfig(ocorrencia.status).label}
                      </Badge>
                      <span className="font-semibold text-foreground">{ocorrencia.criancaNome}</span>
                    </div>
                    <p className="mt-2 text-sm text-muted-foreground">
                      {formatOcorrenciaTipo(ocorrencia.tipo)} • {formatDate(ocorrencia.dataCriacao)}
                    </p>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => carregarHistoricoCrianca({
                      criancaPessoaId: ocorrencia.criancaPessoaId,
                      nome: ocorrencia.criancaNome,
                    })}
                  >
                    <Eye className="mr-2 h-4 w-4" />
                    Ver
                  </Button>
                </div>
              </div>
            ))
          ) : (
            <EstadoVazio texto="Nenhuma ocorrência aberta neste momento." />
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-4 w-4" />
            {t('kids.filters', 'Filtros')}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
            <div className="space-y-2">
              <label className="text-sm font-medium">{t('kids.search', 'Buscar')}</label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={filtros.busca}
                  onChange={(e) => handleFiltroChange('busca', e.target.value)}
                  placeholder={t('kids.searchPlaceholder', 'Nome da criança...')}
                  className="pl-9"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('kids.child', 'Criança')}</label>
              <Select value={filtros.criancaPessoaId || 'todas'} onValueChange={(value) => handleFiltroChange('criancaPessoaId', value === 'todas' ? '' : value)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('kids.allChildren', 'Todas as crianças')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="todas">{t('kids.allChildren', 'Todas as crianças')}</SelectItem>
                  {criancas.map((crianca) => (
                    <SelectItem key={crianca.pessoaId} value={String(crianca.pessoaId)}>
                      {crianca.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('kids.status', 'Status')}</label>
              <Select value={filtros.status || 'todos'} onValueChange={(value) => handleFiltroChange('status', value === 'todos' ? '' : value)}>
                <SelectTrigger>
                  <SelectValue placeholder={t('kids.allStatus', 'Todos os status')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="todos">{t('kids.allStatus', 'Todos os status')}</SelectItem>
                  <SelectItem value="ativo">{t('kids.active', 'Ativo')}</SelectItem>
                  <SelectItem value="finalizado">{t('kids.finished', 'Finalizado')}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('kids.dateStart', 'Data Início')}</label>
              <Input
                type="date"
                value={filtros.dataInicio}
                onChange={(e) => handleFiltroChange('dataInicio', e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">{t('kids.dateEnd', 'Data Fim')}</label>
              <Input
                type="date"
                value={filtros.dataFim}
                onChange={(e) => handleFiltroChange('dataFim', e.target.value)}
              />
            </div>
          </div>

          <div className="mt-4 flex justify-end">
            <Button variant="outline" onClick={limparFiltros}>
              {t('kids.clearFilters', 'Limpar Filtros')}
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('kids.historyTitle', 'Histórico de Check-ins')}</CardTitle>
        </CardHeader>
        <CardContent>
          {checkinsFiltrados.length ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('kids.table.child', 'Criança')}</TableHead>
                  <TableHead>{t('kids.table.checkin', 'Check-in')}</TableHead>
                  <TableHead>{t('kids.table.checkout', 'Check-out')}</TableHead>
                  <TableHead>{t('kids.table.duration', 'Duração')}</TableHead>
                  <TableHead>{t('kids.table.method', 'Método')}</TableHead>
                  <TableHead>{t('kids.table.status', 'Status')}</TableHead>
                  <TableHead>{t('kids.table.checkinBy', 'Check-in por')}</TableHead>
                  <TableHead>{t('kids.table.checkoutBy', 'Check-out por')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {checkinsFiltrados.map((checkin) => (
                  <TableRow key={checkin.id}>
                    <TableCell className="font-medium">{checkin.criancaNome}</TableCell>
                    <TableCell>{formatDate(checkin.checkinTime)}</TableCell>
                    <TableCell>{formatDate(checkin.checkoutTime)}</TableCell>
                    <TableCell>{formatDuration(checkin.checkinTime, checkin.checkoutTime)}</TableCell>
                    <TableCell>{getMetodoBadge(checkin.retiradaMetodo || checkin.metodo)}</TableCell>
                    <TableCell>{getStatusBadge(checkin.status)}</TableCell>
                    <TableCell>{checkin.checkinByNome || '-'}</TableCell>
                    <TableCell>{checkin.checkoutByNome || checkin.retiradaPessoaNome || '-'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <EstadoVazio texto={t('kids.emptyMessage', 'Nenhum check-in encontrado com os filtros aplicados.')} />
          )}
        </CardContent>
      </Card>
    </div>
  );

  return (
    <>
      <div className="space-y-6 p-6">
        <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-foreground">
              {sectionConfig.title}
            </h1>
            <p className="mt-1 text-muted-foreground">
              {sectionConfig.subtitle}
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            {showPainel && (
              <Select value={filtros.salaId} onValueChange={(value) => handleFiltroChange('salaId', value)}>
                <SelectTrigger className="w-[220px]">
                  <SelectValue placeholder={t('kids.panel.selectRoom', 'Filtrar por sala')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="todas">{t('kids.panel.allRooms', 'Todas as salas')}</SelectItem>
                  {salasDisponiveis.map((sala) => (
                    <SelectItem key={sala} value={sala}>
                      {sala}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
            {showEstrutura && (
              <>
                <Button variant="outline" onClick={() => setSalaDialogOpen(true)}>
                  <Building2 className="mr-2 h-4 w-4" />
                  Nova sala
                </Button>
                <Button variant="outline" onClick={() => setTurmaDialogOpen(true)}>
                  <Layers3 className="mr-2 h-4 w-4" />
                  Nova turma
                </Button>
              </>
            )}
            {showCriancas && (
              <Button onClick={() => setCriancaDialogOpen(true)}>
                <PlusCircle className="mr-2 h-4 w-4" />
                Nova criança
              </Button>
            )}
            <Button variant="outline" onClick={fetchData}>
              {t('kids.panel.refresh', 'Atualizar painel')}
            </Button>
          </div>
        </div>

        {isOverview ? (
          <Tabs value={abaAtiva} onValueChange={setAbaAtiva} className="space-y-4">
            <TabsList>
              <TabsTrigger value="painel">{t('kids.panel.currentTab', 'Painel atual')}</TabsTrigger>
              <TabsTrigger value="historico">{t('kids.panel.historyTab', 'Histórico')}</TabsTrigger>
            </TabsList>

            <TabsContent value="painel" className="space-y-6">
              {painelContent}
            </TabsContent>

            <TabsContent value="historico" className="space-y-6">
              {historicoContent}
            </TabsContent>
          </Tabs>
        ) : (
          <div className="space-y-6">
            {showPainel && painelContent}
            {showCriancas && criancasContent}
            {showEstrutura && estruturaContent}
            {showHistorico && historicoContent}
          </div>
        )}
      </div>

      <CriancaDialog
        open={criancaDialogOpen}
        onOpenChange={setCriancaDialogOpen}
        form={criancaForm}
        onChange={handleCriancaFormChange}
        onSave={handleCriarCrianca}
        saving={criancaSaving}
        salas={salas}
        turmas={turmas}
      />

      <OcorrenciaDialog
        open={ocorrenciaDialogOpen}
        onOpenChange={setOcorrenciaDialogOpen}
        form={ocorrenciaForm}
        onChange={handleOcorrenciaFormChange}
        onSave={handleCriarOcorrencia}
        saving={ocorrenciaSaving}
        criancasPresentes={painel?.criancasPresentes || []}
      />

      <SalaDialog
        open={salaDialogOpen}
        onOpenChange={setSalaDialogOpen}
        form={salaForm}
        onChange={handleSalaFormChange}
        onSave={handleCriarSala}
        saving={salaSaving}
      />

      <TurmaDialog
        open={turmaDialogOpen}
        onOpenChange={setTurmaDialogOpen}
        form={turmaForm}
        onChange={handleTurmaFormChange}
        onSave={handleCriarTurma}
        saving={turmaSaving}
        salas={salas}
      />

      <HistoricoDialog
        open={historicoDialogOpen}
        onOpenChange={setHistoricoDialogOpen}
        criancaHistorico={criancaHistorico}
        historicoLoading={historicoLoading}
        ocorrenciasHistorico={ocorrenciasHistorico}
        historicoUpdatingId={historicoUpdatingId}
        onAtualizarOcorrencia={handleAtualizarOcorrencia}
        formatDate={formatDate}
      />
    </>
  );
};

export default KidsCheckinsList;
