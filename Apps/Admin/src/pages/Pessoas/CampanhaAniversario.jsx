import { useEffect, useState } from 'react';
import { Gift, Save, RefreshCcw, Send, Clock3, ImageIcon, RotateCcw, Search, CheckCircle2, AlertTriangle, History, Hourglass } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { ImageUpload } from '@/components/ImageUpload';
import { getAbsoluteUrl } from '@/lib/utils';
import { pessoasApi } from '@/lib/api';
import { toast } from 'sonner';

const defaultMessage = `{Nome},

Hoje celebramos a sua vida! 🎂

Que neste novo ciclo, Cristo se manifeste de forma ainda mais clara em cada área da sua vida — nos seus caminhos, decisões e sonhos.

Que você experimente um tempo de crescimento, intimidade com Deus e direção em cada passo. Que a graça te sustente, o amor te envolva e o propósito dEle te conduza todos os dias. 🙏✨

Feliz aniversário! Você é parte importante daquilo que Deus está fazendo! 💛

Equipe Kingdom`;

function formatDateTime(value) {
  if (!value) return '-';
  return new Date(value).toLocaleString('pt-BR');
}

function formatDate(value) {
  if (!value) return '-';
  return new Date(value).toLocaleDateString('pt-BR');
}

function renderPreview(template, nome) {
  return (template || '').replace(/\{Nome\}/gi, nome);
}

function getStatusBadgeVariant(status) {
  if (status === 'Enviado') return 'default';
  if (status === 'Erro') return 'destructive';
  return 'secondary';
}

const initialFilters = {
  busca: '',
  status: 'all',
  limit: '50',
};

export default function CampanhaAniversario() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [sendingTest, setSendingTest] = useState(false);
  const [resendingId, setResendingId] = useState(null);
  const [confirmResend, setConfirmResend] = useState(null);
  const [formData, setFormData] = useState({
    ativo: true,
    imagemUrl: '',
    mensagemTemplate: defaultMessage,
    horarioEnvio: '09:00',
  });
  const [metricas, setMetricas] = useState({
    totalHistorico: 0,
    totalEnviadosAnoAtual: 0,
    totalFalhasAnoAtual: 0,
    totalPendentesAnoAtual: 0,
    totalEnviadosHoje: 0,
    totalFalhasHoje: 0,
  });
  const [recentes, setRecentes] = useState([]);
  const [filters, setFilters] = useState(initialFilters);
  const [testData, setTestData] = useState({
    nome: 'Teste Kingdom',
    whatsApp: '',
  });

  const loadConfiguracao = async (filtersToUse = filters) => {
    try {
      setLoading(true);
      const response = await pessoasApi.getCampanhaAniversario({
        busca: filtersToUse.busca || undefined,
        status: filtersToUse.status === 'all' ? undefined : filtersToUse.status,
        limit: Number(filtersToUse.limit || 50),
      });
      const data = response.data;
      setFormData({
        ativo: data.ativo ?? true,
        imagemUrl: data.imagemUrl || '',
        mensagemTemplate: data.mensagemTemplate || defaultMessage,
        horarioEnvio: data.horarioEnvio || '09:00',
      });
      setMetricas(data.metricas || {});
      setRecentes(data.enviosRecentes || []);
      setFilters({
        busca: data.filtros?.busca || filtersToUse.busca || '',
        status: data.filtros?.status || filtersToUse.status || 'all',
        limit: String(data.filtros?.limit || filtersToUse.limit || '50'),
      });
    } catch (error) {
      console.error(error);
      toast.error('Erro ao carregar a campanha de aniversário.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadConfiguracao();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!formData.imagemUrl) {
      toast.error('Envie a imagem da campanha antes de salvar.');
      return;
    }

    if (!formData.mensagemTemplate.trim()) {
      toast.error('A legenda da campanha é obrigatória.');
      return;
    }

    try {
      setSaving(true);
      const response = await pessoasApi.updateCampanhaAniversario(formData);
      const data = response.data;
      setMetricas(data.metricas || {});
      setRecentes(data.enviosRecentes || []);
      toast.success('Campanha de aniversário salva com sucesso.');
    } catch (error) {
      console.error(error);
      toast.error(error.response?.data?.message || 'Erro ao salvar a campanha.');
    } finally {
      setSaving(false);
    }
  };

  const handleSendTest = async (event) => {
    event?.preventDefault?.();
    event?.stopPropagation?.();

    if (!testData.whatsApp.trim()) {
      toast.error('Informe um WhatsApp para teste.');
      return;
    }

    try {
      setSendingTest(true);
      const response = await pessoasApi.sendCampanhaAniversarioTeste(testData);
      toast.success(response.data?.mensagem || 'Teste enviado com sucesso.');
    } catch (error) {
      console.error(error);
      const mensagem = error.response?.data?.mensagem || 'Erro ao enviar mensagem de teste.';
      const detalhes = error.response?.data?.detalhes;
      toast.error(detalhes ? `${mensagem} - ${detalhes}` : mensagem);
    } finally {
      setSendingTest(false);
    }
  };

  const handleApplyFilters = async (event) => {
    event.preventDefault();
    await loadConfiguracao(filters);
  };

  const handleResend = async (item) => {
    try {
      setResendingId(item.id);
      const response = await pessoasApi.resendCampanhaAniversarioHistorico(item.id);
      toast.success(response.data?.mensagem || 'Mensagem reenviada com sucesso.');
      await loadConfiguracao(filters);
    } catch (error) {
      console.error(error);
      const mensagem = error.response?.data?.mensagem || 'Erro ao reenviar mensagem.';
      const detalhes = error.response?.data?.detalhes;
      toast.error(detalhes ? `${mensagem} - ${detalhes}` : mensagem);
    } finally {
      setResendingId(null);
      setConfirmResend(null);
    }
  };

  if (loading) {
    return <LoadingPage text="Carregando campanha de aniversário..." />;
  }

  const previewNome = testData.nome?.trim() || 'Fulano';
  const previewMessage = renderPreview(formData.mensagemTemplate, previewNome);
  const previewImage = formData.imagemUrl ? getAbsoluteUrl(formData.imagemUrl) : null;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold flex items-center gap-2">
            <Gift className="h-8 w-8" />
            Campanha de Aniversário
          </h1>
          <p className="text-muted-foreground mt-2">
            Configure a arte, a legenda e o horário do envio automático de aniversário pelo WhatsApp.
          </p>
        </div>
        <Button type="button" variant="outline" onClick={loadConfiguracao}>
          <RefreshCcw className="h-4 w-4 mr-2" />
          Atualizar
        </Button>
      </div>

      <Alert>
        <Gift className="h-4 w-4" />
        <AlertTitle>Como funciona</AlertTitle>
        <AlertDescription>
          O job roda em segundo plano, procura aniversariantes do dia com WhatsApp preenchido e envia a imagem com a legenda configurada no horário definido. Cada pessoa recebe no máximo uma vez por ano.
        </AlertDescription>
      </Alert>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <Card>
          <CardHeader>
            <CardTitle>Configuração da campanha</CardTitle>
            <CardDescription>
              Use o placeholder <code>{'{Nome}'}</code> para personalizar a mensagem.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="flex items-center justify-between rounded-lg border p-4">
                <div className="space-y-1">
                  <Label className="text-base">Campanha ativa</Label>
                  <p className="text-sm text-muted-foreground">
                    Quando ativa, o envio automático entra na rotina diária.
                  </p>
                </div>
                <Switch
                  checked={formData.ativo}
                  onCheckedChange={(checked) =>
                    setFormData((current) => ({ ...current, ativo: checked }))
                  }
                />
              </div>

              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <ImageIcon className="h-4 w-4" />
                  Imagem do WhatsApp
                </Label>
                <ImageUpload
                  label="Imagem"
                  value={formData.imagemUrl}
                  onChange={(value) => setFormData((current) => ({ ...current, imagemUrl: value }))}
                  accept="image/*"
                  type="image"
                />
                <p className="text-xs text-muted-foreground">
                  Prefira uma arte vertical, como a que será exibida no WhatsApp.
                </p>
              </div>

              <div className="grid gap-4 md:grid-cols-[220px_1fr]">
                <div className="space-y-2">
                  <Label htmlFor="horarioEnvio" className="flex items-center gap-2">
                    <Clock3 className="h-4 w-4" />
                    Horário de envio
                  </Label>
                  <Input
                    id="horarioEnvio"
                    type="time"
                    value={formData.horarioEnvio}
                    onChange={(event) =>
                      setFormData((current) => ({ ...current, horarioEnvio: event.target.value }))
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="mensagemTemplate">Legenda</Label>
                  <Textarea
                    id="mensagemTemplate"
                    value={formData.mensagemTemplate}
                    onChange={(event) =>
                      setFormData((current) => ({ ...current, mensagemTemplate: event.target.value }))
                    }
                    rows={14}
                  />
                </div>
              </div>

              <div className="flex justify-end">
                <Button type="submit" disabled={saving}>
                  <Save className="h-4 w-4 mr-2" />
                  {saving ? 'Salvando...' : 'Salvar campanha'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Prévia</CardTitle>
              <CardDescription>
                Visualização com o nome que será usado no teste.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {previewImage ? (
                <img
                  src={previewImage}
                  alt="Prévia da campanha"
                  className="w-full rounded-xl border object-cover"
                />
              ) : (
                <div className="flex h-56 items-center justify-center rounded-xl border border-dashed text-sm text-muted-foreground">
                  Envie uma imagem para ver a prévia.
                </div>
              )}

              <div className="rounded-xl border bg-muted/20 p-4 whitespace-pre-wrap text-sm leading-6">
                {previewMessage}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Enviar teste</CardTitle>
              <CardDescription>
                Dispare a arte atual para validar imagem, legenda e integração do WhatsApp.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <form onSubmit={handleSendTest} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="nomeTeste">Nome para prévia</Label>
                  <Input
                    id="nomeTeste"
                    value={testData.nome}
                    onChange={(event) =>
                      setTestData((current) => ({ ...current, nome: event.target.value }))
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="whatsAppTeste">WhatsApp de teste</Label>
                  <Input
                    id="whatsAppTeste"
                    value={testData.whatsApp}
                    onChange={(event) =>
                      setTestData((current) => ({ ...current, whatsApp: event.target.value }))
                    }
                    placeholder="11999999999"
                  />
                </div>
                <Button type="submit" disabled={sendingTest} className="w-full">
                  <Send className="h-4 w-4 mr-2" />
                  {sendingTest ? 'Enviando...' : 'Enviar teste por WhatsApp'}
                </Button>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <History className="h-4 w-4" />
              Histórico total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{metricas.totalHistorico || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4" />
              Enviados no ano
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{metricas.totalEnviadosAnoAtual || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <AlertTriangle className="h-4 w-4" />
              Falhas no ano
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{metricas.totalFalhasAnoAtual || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Hourglass className="h-4 w-4" />
              Pendentes no ano
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{metricas.totalPendentesAnoAtual || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Gift className="h-4 w-4" />
              Hoje
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-1">
            <div className="text-lg font-semibold">{metricas.totalEnviadosHoje || 0} enviados</div>
            <div className="text-sm text-muted-foreground">{metricas.totalFalhasHoje || 0} falhas</div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Histórico recente</CardTitle>
          <CardDescription>
            Acompanhe o resultado dos disparos, filtre rapidamente e reenvie mensagens quando necessário.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleApplyFilters} className="grid gap-4 mb-6 lg:grid-cols-[1fr_220px_180px_auto]">
            <div className="space-y-2">
              <Label htmlFor="buscaHistorico" className="flex items-center gap-2">
                <Search className="h-4 w-4" />
                Buscar pessoa ou WhatsApp
              </Label>
              <Input
                id="buscaHistorico"
                value={filters.busca}
                onChange={(event) => setFilters((current) => ({ ...current, busca: event.target.value }))}
                placeholder="Nome ou WhatsApp"
              />
            </div>
            <div className="space-y-2">
              <Label>Status</Label>
              <Select
                value={filters.status}
                onValueChange={(value) => setFilters((current) => ({ ...current, status: value }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos os status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os status</SelectItem>
                  <SelectItem value="Enviado">Enviado</SelectItem>
                  <SelectItem value="Erro">Erro</SelectItem>
                  <SelectItem value="Pendente">Pendente</SelectItem>
                  <SelectItem value="EmProcessamento">Em processamento</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Quantidade</Label>
              <Select
                value={filters.limit}
                onValueChange={(value) => setFilters((current) => ({ ...current, limit: value }))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="20">20 registros</SelectItem>
                  <SelectItem value="50">50 registros</SelectItem>
                  <SelectItem value="100">100 registros</SelectItem>
                  <SelectItem value="200">200 registros</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-end gap-2">
              <Button type="submit" variant="outline" className="w-full lg:w-auto">
                <Search className="h-4 w-4 mr-2" />
                Filtrar
              </Button>
              <Button
                type="button"
                variant="ghost"
                className="w-full lg:w-auto"
                onClick={() => {
                  const resetFilters = { ...initialFilters };
                  setFilters(resetFilters);
                  loadConfiguracao(resetFilters);
                }}
              >
                Limpar
              </Button>
            </div>
          </form>

          {recentes.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              Ainda não há envios registrados para esta campanha.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Pessoa</TableHead>
                  <TableHead>WhatsApp</TableHead>
                  <TableHead>Aniversário</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Tentativas</TableHead>
                  <TableHead>Última tentativa</TableHead>
                  <TableHead>Enviado em</TableHead>
                  <TableHead className="w-[120px] text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recentes.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>
                      <div className="font-medium">{item.nomePessoa}</div>
                      {item.logErro ? (
                        <div className="text-xs text-muted-foreground mt-1 line-clamp-2">{item.logErro}</div>
                      ) : null}
                    </TableCell>
                    <TableCell>{item.whatsApp || '-'}</TableCell>
                    <TableCell>{formatDate(item.dataAniversario)}</TableCell>
                    <TableCell>
                      <Badge variant={getStatusBadgeVariant(item.status)}>
                        {item.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{item.tentativas}</TableCell>
                    <TableCell>{formatDateTime(item.dataUltimaTentativa)}</TableCell>
                    <TableCell>{formatDateTime(item.dataEnvioSucesso)}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setConfirmResend(item)}
                        disabled={resendingId === item.id}
                      >
                        <RotateCcw className="h-4 w-4 mr-2" />
                        {resendingId === item.id ? 'Reenviando...' : 'Reenviar'}
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
        open={Boolean(confirmResend)}
        onOpenChange={(open) => {
          if (!open) setConfirmResend(null);
        }}
        title="Reenviar mensagem"
        description={
          confirmResend
            ? `Deseja reenviar agora a campanha de aniversário para ${confirmResend.nomePessoa}?`
            : ''
        }
        confirmText={resendingId ? 'Reenviando...' : 'Reenviar agora'}
        cancelText="Cancelar"
        loading={Boolean(resendingId)}
        onConfirm={() => confirmResend && handleResend(confirmResend)}
      />
    </div>
  );
}
