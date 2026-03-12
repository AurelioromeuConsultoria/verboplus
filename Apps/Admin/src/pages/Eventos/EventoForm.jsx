import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, PlusCircle, Save, Trash2, Pencil, ChevronUp, ChevronDown } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ImageUpload } from '@/components/ImageUpload';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { eventosApi, eventosRecorrenciasApi, normalizeEvento } from '@/lib/api';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';

const TIPOS_EVENTO = [
  { value: 1, label: 'Evento' },
  { value: 2, label: 'Culto' },
  { value: 3, label: 'Reunião' },
  { value: 4, label: 'Outro' },
];

const DIAS_SEMANA = [
  { value: 0, label: 'Domingo' },
  { value: 1, label: 'Segunda-feira' },
  { value: 2, label: 'Terça-feira' },
  { value: 3, label: 'Quarta-feira' },
  { value: 4, label: 'Quinta-feira' },
  { value: 5, label: 'Sexta-feira' },
  { value: 6, label: 'Sábado' },
];

const PERIODICIDADE = [
  { value: 1, label: 'Semanal' },
  { value: 2, label: 'Quinzenal' },
  { value: 3, label: 'Mensal' },
];

/** Colunas fixas: nome, whatsApp, email, observacoes. Outros campos ficam em DadosInscricao. */
const CAMPOS_FORMULARIO_PADRAO = [
  { slug: 'nome', label: 'Nome', tipo: 'texto', obrigatorio: true },
  { slug: 'whatsApp', label: 'WhatsApp', tipo: 'texto', obrigatorio: true },
  { slug: 'email', label: 'Email', tipo: 'texto', obrigatorio: false },
  { slug: 'observacoes', label: 'Observações', tipo: 'texto', obrigatorio: false },
];

const TIPOS_CAMPO = [
  { value: 'texto', label: 'Texto' },
  { value: 'numero', label: 'Número' },
  { value: 'email', label: 'Email' },
  { value: 'tel', label: 'Telefone' },
];

export default function EventoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    titulo: '',
    descricao: '',
    imagemDestaque: '',
    url: '',
    dataInicio: '',
    dataFim: '',
    tipo: 1,
    ehRecorrente: false,
    ativo: true,
    aceitaInscricoes: false,
  });
  /** Lista de campos do formulário de inscrição: { slug, label, tipo, obrigatorio } */
  const [camposFormulario, setCamposFormulario] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [recorrencias, setRecorrencias] = useState([]);
  const [loadingRecorrencias, setLoadingRecorrencias] = useState(false);
  const [showRecorrenciaForm, setShowRecorrenciaForm] = useState(false);
  const [editingRecorrenciaId, setEditingRecorrenciaId] = useState(null);
  const [recorrenciaForm, setRecorrenciaForm] = useState({
    diaSemana: 0,
    horaInicio: '10:00',
    horaFim: '',
    periodicidade: 1,
    dataInicioVigencia: new Date().toISOString().slice(0, 10),
    dataFimVigencia: '',
    ativo: true,
  });

  // Considera data vazia se for null/undefined ou data default do backend (ex: 0001-01-01)
  const toDateTimeLocal = (value) => {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime()) || d.getFullYear() < 1900) return '';
    return d.toISOString().slice(0, 16);
  };

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await eventosApi.getById(id);
      const e = normalizeEvento(res.data);
      if (!e) {
        setError('Evento não encontrado');
        return;
      }
      setFormData({
        titulo: e.titulo || '',
        descricao: e.descricao || '',
        imagemDestaque: e.imagemDestaque || '',
        url: e.url || '',
        dataInicio: toDateTimeLocal(e.dataInicio),
        dataFim: toDateTimeLocal(e.dataFim),
        tipo: e.tipo ?? 1,
        ehRecorrente: e.ehRecorrente ?? false,
        ativo: e.ativo ?? true,
        aceitaInscricoes: e.aceitaInscricoes ?? false,
      });
      // Campos do formulário de inscrição (JSON)
      let campos = CAMPOS_FORMULARIO_PADRAO;
      if (e.configuracaoFormularioInscricao && typeof e.configuracaoFormularioInscricao === 'string') {
        try {
          const parsed = JSON.parse(e.configuracaoFormularioInscricao);
          if (Array.isArray(parsed) && parsed.length > 0) campos = parsed;
        } catch (_) { /* mantém padrão */ }
      }
      setCamposFormulario(campos.map((c) => ({
        slug: c.slug || '',
        label: c.label || '',
        tipo: c.tipo || 'texto',
        obrigatorio: Boolean(c.obrigatorio),
      })));
    } catch (err) {
      setError('Erro ao carregar evento');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadRecorrencias = async () => {
    if (!id) return;
    try {
      setLoadingRecorrencias(true);
      const res = await eventosRecorrenciasApi.getByEvento(id);
      setRecorrencias(res.data || []);
    } catch (err) {
      console.error(err);
      setRecorrencias([]);
    } finally {
      setLoadingRecorrencias(false);
    }
  };

  useEffect(() => { load(); }, [id]);
  useEffect(() => {
    if (!isEditing) setCamposFormulario([]);
  }, [isEditing]);
  useEffect(() => {
    if (isEditing && formData.ehRecorrente) loadRecorrencias();
    else setRecorrencias([]);
  }, [id, isEditing, formData.ehRecorrente]);

  const handleChange = (e) => {
    const { name, value, type } = e.target;
    const next = type === 'checkbox' ? e.target.checked : value;
    setFormData((prev) => ({ ...prev, [name]: next }));
  };

  const usarCamposPadrao = () => {
    setCamposFormulario(CAMPOS_FORMULARIO_PADRAO.map((c) => ({ ...c })));
  };

  const addCampoFormulario = () => {
    setCamposFormulario((prev) => [...prev, { slug: '', label: '', tipo: 'texto', obrigatorio: false }]);
  };

  const updateCampoFormulario = (index, field, value) => {
    setCamposFormulario((prev) => {
      const next = [...prev];
      next[index] = { ...next[index], [field]: value };
      if (field === 'label' && !next[index].slug) {
        next[index].slug = value.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/gi, '') || '';
      }
      return next;
    });
  };

  const removeCampoFormulario = (index) => {
    setCamposFormulario((prev) => prev.filter((_, i) => i !== index));
  };

  const moveCampoFormulario = (index, dir) => {
    if (dir < 0 && index <= 0) return;
    if (dir > 0 && index >= camposFormulario.length - 1) return;
    const next = [...camposFormulario];
    const swap = index + dir;
    [next[index], next[swap]] = [next[swap], next[index]];
    setCamposFormulario(next);
  };

  const openNovaRecorrencia = () => {
    setEditingRecorrenciaId(null);
    setRecorrenciaForm({
      diaSemana: 0,
      horaInicio: '10:00',
      horaFim: '',
      periodicidade: 1,
      dataInicioVigencia: new Date().toISOString().slice(0, 10),
      dataFimVigencia: '',
      ativo: true,
    });
    setShowRecorrenciaForm(true);
  };

  const openEditRecorrencia = (r) => {
    setEditingRecorrenciaId(r.id);
    setRecorrenciaForm({
      diaSemana: r.diaSemana,
      horaInicio: r.horaInicio || '10:00',
      horaFim: r.horaFim || '',
      periodicidade: r.periodicidade,
      dataInicioVigencia: r.dataInicioVigencia?.slice(0, 10) || new Date().toISOString().slice(0, 10),
      dataFimVigencia: r.dataFimVigencia?.slice(0, 10) || '',
      ativo: r.ativo ?? true,
    });
    setShowRecorrenciaForm(true);
  };

  const cancelRecorrenciaForm = () => {
    setShowRecorrenciaForm(false);
    setEditingRecorrenciaId(null);
  };

  const saveRecorrencia = async () => {
    const base = {
      diaSemana: Number(recorrenciaForm.diaSemana),
      horaInicio: recorrenciaForm.horaInicio || '10:00',
      horaFim: recorrenciaForm.horaFim || null,
      periodicidade: Number(recorrenciaForm.periodicidade),
      dataInicioVigencia: recorrenciaForm.dataInicioVigencia ? new Date(recorrenciaForm.dataInicioVigencia).toISOString() : new Date().toISOString(),
      dataFimVigencia: recorrenciaForm.dataFimVigencia ? new Date(recorrenciaForm.dataFimVigencia).toISOString() : null,
      ativo: recorrenciaForm.ativo,
    };
    try {
      if (editingRecorrenciaId) {
        await eventosRecorrenciasApi.update(id, editingRecorrenciaId, base);
        toast.success('Recorrência atualizada.');
      } else {
        await eventosRecorrenciasApi.create(id, { ...base, eventoId: Number(id) });
        toast.success('Recorrência adicionada.');
      }
      cancelRecorrenciaForm();
      await loadRecorrencias();
    } catch (err) {
      toast.error(err.response?.data || 'Erro ao salvar recorrência');
    }
  };

  const deleteRecorrencia = async (recId) => {
    if (!window.confirm('Excluir esta recorrência?')) return;
    try {
      await eventosRecorrenciasApi.delete(id, recId);
      toast.success('Recorrência excluída.');
      await loadRecorrencias();
    } catch (err) {
      toast.error(err.response?.data || 'Erro ao excluir');
    }
  };

  // Função para normalizar URL (adiciona https:// se não tiver protocolo, mas preserva URLs relativas)
  const normalizeUrl = (url) => {
    if (!url || !url.trim()) return null;
    const trimmed = url.trim();
    // Se já tiver protocolo, retorna como está
    if (trimmed.match(/^https?:\/\//i)) {
      return trimmed;
    }
    // Se começar com /, é URL relativa interna - não adicionar protocolo
    if (trimmed.startsWith('/')) {
      return trimmed;
    }
    // Se não tiver protocolo e não for relativa, adiciona https://
    return `https://${trimmed}`;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const dataInicio = formData.dataInicio ? new Date(formData.dataInicio).toISOString() : null;
      const dataFim = formData.dataFim ? new Date(formData.dataFim).toISOString() : dataInicio;
      const payload = {
        titulo: formData.titulo.trim() || '',
        descricao: formData.descricao.trim() || null,
        imagemDestaque: formData.imagemDestaque.trim() || null,
        url: normalizeUrl(formData.url),
        dataInicio,
        dataFim,
        tipo: Number(formData.tipo),
        ehRecorrente: Boolean(formData.ehRecorrente),
        ativo: Boolean(formData.ativo),
        aceitaInscricoes: Boolean(formData.aceitaInscricoes),
      };
      const camposValidos = camposFormulario.filter((c) => (c.slug || '').trim() && (c.label || '').trim());
      if (formData.aceitaInscricoes && camposValidos.length > 0) {
        payload.configuracaoFormularioInscricao = JSON.stringify(camposValidos.map((c) => ({
          slug: String(c.slug).trim(),
          label: String(c.label).trim(),
          tipo: c.tipo || 'texto',
          obrigatorio: Boolean(c.obrigatorio),
        })));
      } else {
        payload.configuracaoFormularioInscricao = null;
      }
      // Backend espera o body direto (não dentro de "dto"); DataFim obrigatório → usa dataInicio quando vazio
      if (isEditing) await eventosApi.update(id, payload);
      else await eventosApi.create(payload);
      toast.success(isEditing ? 'Evento atualizado com sucesso!' : 'Evento criado com sucesso!');
      navigate('/eventos');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar evento';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando evento..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/eventos">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Evento' : 'Novo Evento'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do evento' : 'Cadastre um novo evento'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Evento' : 'Cadastrar Evento'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="titulo">Título *</Label>
                <Input id="titulo" name="titulo" value={formData.titulo} onChange={handleChange} placeholder="Título do evento" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="url">URL</Label>
                <Input 
                  id="url" 
                  name="url" 
                  type="text" 
                  value={formData.url} 
                  onChange={handleChange} 
                  placeholder="exemplo.com ou https://exemplo.com" 
                />
                {formData.url && !formData.url.match(/^https?:\/\//i) && (
                  <p className="text-xs text-muted-foreground">
                    Será adicionado https:// automaticamente
                  </p>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="descricao">Descrição</Label>
              <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição do evento" rows={4} />
            </div>

            <div className="space-y-2">
              <ImageUpload
                label="Imagem de Destaque"
                value={formData.imagemDestaque}
                onChange={(url) => setFormData((prev) => ({ ...prev, imagemDestaque: url }))}
                accept="image/*"
                type="image"
              />
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="dataInicio">Data e Hora de Início *</Label>
                <Input id="dataInicio" name="dataInicio" type="datetime-local" value={formData.dataInicio} onChange={handleChange} required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="dataFim">Data e Hora de Fim</Label>
                <Input id="dataFim" name="dataFim" type="datetime-local" value={formData.dataFim} onChange={handleChange} />
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              <div className="space-y-2">
                <Label>Tipo de evento</Label>
                <Select value={String(formData.tipo)} onValueChange={(v) => setFormData((p) => ({ ...p, tipo: Number(v) }))}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {TIPOS_EVENTO.map((opt) => (
                      <SelectItem key={opt.value} value={String(opt.value)}>{opt.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-center gap-2 pt-8">
                <input type="checkbox" id="ehRecorrente" name="ehRecorrente" checked={formData.ehRecorrente} onChange={handleChange} className="rounded border-input" />
                <Label htmlFor="ehRecorrente" className="cursor-pointer">É recorrente (ex.: culto dominical)</Label>
              </div>
              <div className="flex items-center gap-2 pt-8">
                <input type="checkbox" id="ativo" name="ativo" checked={formData.ativo} onChange={handleChange} className="rounded border-input" />
                <Label htmlFor="ativo" className="cursor-pointer">Ativo</Label>
              </div>
              <div className="flex items-center gap-2 pt-8">
                <input type="checkbox" id="aceitaInscricoes" name="aceitaInscricoes" checked={formData.aceitaInscricoes} onChange={handleChange} className="rounded border-input" />
                <Label htmlFor="aceitaInscricoes" className="cursor-pointer">Aceitar inscrições (exibir formulário no portal)</Label>
              </div>
            </div>

            {formData.aceitaInscricoes && (
              <div className="rounded-lg border p-4 space-y-3 bg-muted/20">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <h4 className="font-medium">Campos do formulário de inscrição</h4>
                    <p className="text-sm text-muted-foreground">
                      Colunas fixas: Nome, WhatsApp, Email, Observações. Adicione outros campos (ex.: RG, CPF) ou use o padrão.
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <Button type="button" variant="outline" size="sm" onClick={usarCamposPadrao}>
                      Usar campos padrão
                    </Button>
                    <Button type="button" variant="outline" size="sm" onClick={addCampoFormulario}>
                      <PlusCircle className="h-4 w-4 mr-1" /> Adicionar campo
                    </Button>
                  </div>
                </div>
                {camposFormulario.length === 0 ? (
                  <p className="text-sm text-muted-foreground">Nenhum campo definido. Clique em &quot;Usar campos padrão&quot; ou &quot;Adicionar campo&quot;.</p>
                ) : (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="w-10" />
                        <TableHead>Slug (identificador)</TableHead>
                        <TableHead>Rótulo</TableHead>
                        <TableHead>Tipo</TableHead>
                        <TableHead className="w-24">Obrigatório</TableHead>
                        <TableHead className="w-20">Ações</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {camposFormulario.map((campo, idx) => (
                        <TableRow key={idx}>
                          <TableCell className="p-1">
                            <div className="flex flex-col gap-0">
                              <Button type="button" variant="ghost" size="icon" className="h-6 w-6" onClick={() => moveCampoFormulario(idx, -1)} disabled={idx === 0} title="Subir">
                                <ChevronUp className="h-4 w-4" />
                              </Button>
                              <Button type="button" variant="ghost" size="icon" className="h-6 w-6" onClick={() => moveCampoFormulario(idx, 1)} disabled={idx === camposFormulario.length - 1} title="Descer">
                                <ChevronDown className="h-4 w-4" />
                              </Button>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Input
                              className="h-8 font-mono text-sm"
                              placeholder="ex: nome, rg"
                              value={campo.slug}
                              onChange={(e) => updateCampoFormulario(idx, 'slug', e.target.value)}
                            />
                          </TableCell>
                          <TableCell>
                            <Input
                              className="h-8"
                              placeholder="Ex.: Nome completo"
                              value={campo.label}
                              onChange={(e) => updateCampoFormulario(idx, 'label', e.target.value)}
                            />
                          </TableCell>
                          <TableCell>
                            <Select value={campo.tipo} onValueChange={(v) => updateCampoFormulario(idx, 'tipo', v)}>
                              <SelectTrigger className="h-8"><SelectValue /></SelectTrigger>
                              <SelectContent>
                                {TIPOS_CAMPO.map((t) => (
                                  <SelectItem key={t.value} value={t.value}>{t.label}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </TableCell>
                          <TableCell>
                            <input
                              type="checkbox"
                              checked={campo.obrigatorio}
                              onChange={(e) => updateCampoFormulario(idx, 'obrigatorio', e.target.checked)}
                              className="rounded border-input"
                            />
                          </TableCell>
                          <TableCell>
                            <Button type="button" variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => removeCampoFormulario(idx)} title="Remover">
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </div>
            )}

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? t('actions.saving') : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/eventos">{t('actions.cancel')}</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {isEditing && formData.ehRecorrente && (
        <Card>
          <CardHeader>
            <CardTitle>Recorrências</CardTitle>
            <p className="text-sm text-muted-foreground">
              Defina os dias e horários em que este evento se repete (ex.: todo domingo às 10h). Depois use &quot;Gerar Ocorrências&quot; em Voluntariado → Escalas.
            </p>
            <div className="pt-2">
              <Button type="button" variant="outline" size="sm" onClick={openNovaRecorrencia}>
                <PlusCircle className="h-4 w-4 mr-2" /> Nova recorrência
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {showRecorrenciaForm && (
              <div className="rounded-lg border p-4 space-y-4 bg-muted/30">
                <h4 className="font-medium">{editingRecorrenciaId ? 'Editar recorrência' : 'Nova recorrência'}</h4>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                  <div className="space-y-2">
                    <Label>Dia da semana</Label>
                    <Select value={String(recorrenciaForm.diaSemana)} onValueChange={(v) => setRecorrenciaForm((p) => ({ ...p, diaSemana: Number(v) }))}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        {DIAS_SEMANA.map((d) => (
                          <SelectItem key={d.value} value={String(d.value)}>{d.label}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Hora início</Label>
                    <Input type="time" value={recorrenciaForm.horaInicio} onChange={(e) => setRecorrenciaForm((p) => ({ ...p, horaInicio: e.target.value }))} />
                  </div>
                  <div className="space-y-2">
                    <Label>Hora fim (opcional)</Label>
                    <Input type="time" value={recorrenciaForm.horaFim} onChange={(e) => setRecorrenciaForm((p) => ({ ...p, horaFim: e.target.value }))} />
                  </div>
                  <div className="space-y-2">
                    <Label>Periodicidade</Label>
                    <Select value={String(recorrenciaForm.periodicidade)} onValueChange={(v) => setRecorrenciaForm((p) => ({ ...p, periodicidade: Number(v) }))}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        {PERIODICIDADE.map((p) => (
                          <SelectItem key={p.value} value={String(p.value)}>{p.label}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Vigência desde</Label>
                    <Input type="date" value={recorrenciaForm.dataInicioVigencia} onChange={(e) => setRecorrenciaForm((p) => ({ ...p, dataInicioVigencia: e.target.value }))} />
                  </div>
                  <div className="space-y-2">
                    <Label>Vigência até (opcional)</Label>
                    <Input type="date" value={recorrenciaForm.dataFimVigencia} onChange={(e) => setRecorrenciaForm((p) => ({ ...p, dataFimVigencia: e.target.value }))} />
                  </div>
                  <div className="flex items-center gap-2 pt-8">
                    <input type="checkbox" checked={recorrenciaForm.ativo} onChange={(e) => setRecorrenciaForm((p) => ({ ...p, ativo: e.target.checked }))} className="rounded border-input" />
                    <Label>Ativo</Label>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button type="button" size="sm" onClick={saveRecorrencia}>{t('actions.save')}</Button>
                  <Button type="button" size="sm" variant="outline" onClick={cancelRecorrenciaForm}>{t('actions.cancel')}</Button>
                </div>
              </div>
            )}

            {loadingRecorrencias ? (
              <p className="text-sm text-muted-foreground">Carregando recorrências...</p>
            ) : recorrencias.length === 0 ? (
              <p className="text-sm text-muted-foreground">Nenhuma recorrência cadastrada. Clique em &quot;Nova recorrência&quot; para definir dias e horários.</p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Dia</TableHead>
                    <TableHead>Hora início</TableHead>
                    <TableHead>Hora fim</TableHead>
                    <TableHead>Periodicidade</TableHead>
                    <TableHead>Vigência</TableHead>
                    <TableHead>Ativo</TableHead>
                    <TableHead className="w-[100px]">Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {recorrencias.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell>{r.diaSemanaDescricao ?? DIAS_SEMANA.find((d) => d.value === r.diaSemana)?.label}</TableCell>
                      <TableCell>{r.horaInicio}</TableCell>
                      <TableCell>{r.horaFim || '—'}</TableCell>
                      <TableCell>{r.periodicidadeDescricao ?? PERIODICIDADE.find((p) => p.value === r.periodicidade)?.label}</TableCell>
                      <TableCell>
                        {r.dataInicioVigencia?.slice(0, 10)} {r.dataFimVigencia ? `até ${r.dataFimVigencia.slice(0, 10)}` : '(sem fim)'}
                      </TableCell>
                      <TableCell>{r.ativo ? 'Sim' : 'Não'}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          <Button type="button" variant="ghost" size="icon" onClick={() => openEditRecorrencia(r)} title="Editar">
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button type="button" variant="ghost" size="icon" onClick={() => deleteRecorrencia(r.id)} title="Excluir">
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}


