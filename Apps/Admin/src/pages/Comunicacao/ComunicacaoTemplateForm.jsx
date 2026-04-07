import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { comunicacaoTemplatesApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

const sampleData = {
  Nome: 'Maria Souza',
  PrimeiroNome: 'Maria',
  PublicoAlvo: 'visitantes',
  Campanha: 'Boas-vindas',
  Link: 'https://appigreja.local/convite',
};

function interpolateTemplate(text) {
  if (!text) return '';

  return text
    .replaceAll('{Nome}', sampleData.Nome)
    .replaceAll('{PrimeiroNome}', sampleData.PrimeiroNome)
    .replaceAll('{PublicoAlvo}', sampleData.PublicoAlvo)
    .replaceAll('{Campanha}', sampleData.Campanha)
    .replaceAll('{Link}', sampleData.Link);
}

const canalOptions = [
  { value: 1, label: 'WhatsApp' },
  { value: 2, label: 'E-mail' },
  { value: 3, label: 'Push' },
  { value: 4, label: 'Notificação interna' },
];

export default function ComunicacaoTemplateForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const [loading, setLoading] = useState(false);
  const [pageLoading, setPageLoading] = useState(isEditing);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    nome: '',
    objetivo: '',
    canal: 1,
    assunto: '',
    corpo: '',
    corpoHtml: '',
    variaveisPermitidas: '{Nome},{PrimeiroNome},{Link}',
    status: 1,
  });

  useEffect(() => {
    if (!isEditing) return;

    const load = async () => {
      try {
        setPageLoading(true);
        const response = await comunicacaoTemplatesApi.getById(id);
        const data = response.data;
        setFormData({
          nome: data.nome || '',
          objetivo: data.objetivo || '',
          canal: Number(data.canal || 1),
          assunto: data.assunto || '',
          corpo: data.corpo || '',
          corpoHtml: data.corpoHtml || '',
          variaveisPermitidas: data.variaveisPermitidas || '',
          status: Number(data.status || 1),
        });
      } catch (err) {
        setError(getApiErrorMessage(err, 'Erro ao carregar template'));
      } finally {
        setPageLoading(false);
      }
    };

    load();
  }, [id, isEditing]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.nome.trim() || !formData.objetivo.trim() || !formData.corpo.trim()) {
      toast.error('Preencha nome, objetivo e corpo do template.');
      return;
    }

    if ([2, 3, 4].includes(Number(formData.canal)) && !formData.assunto.trim()) {
      toast.error('Para este canal, informe o assunto ou título do template.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const payload = {
        nome: formData.nome.trim(),
        objetivo: formData.objetivo.trim(),
        canal: Number(formData.canal),
        assunto: formData.assunto.trim() || null,
        corpo: formData.corpo.trim(),
        corpoHtml: formData.corpoHtml.trim() || null,
        variaveisPermitidas: formData.variaveisPermitidas.trim(),
        ...(isEditing ? { status: Number(formData.status) } : {}),
      };

      if (isEditing) await comunicacaoTemplatesApi.update(id, payload);
      else await comunicacaoTemplatesApi.create(payload);

      toast.success(isEditing ? 'Template atualizado com sucesso.' : 'Template criado com sucesso.');
      navigate('/comunicacao/templates');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar template'));
    } finally {
      setLoading(false);
    }
  };

  if (pageLoading) return <LoadingPage text="Carregando template..." />;
  if (error) return <ErrorPage message={error} onRetry={() => window.location.reload()} />;

  const previewAssunto = interpolateTemplate(formData.assunto) || sampleData.Campanha;
  const previewCorpo = interpolateTemplate(formData.corpo);
  const previewHtml = interpolateTemplate(formData.corpoHtml);
  const isEmail = Number(formData.canal) === 2;
  const isContextual = [3, 4].includes(Number(formData.canal));
  const canalBadge = canalOptions.find((item) => item.value === Number(formData.canal))?.label || 'Canal';
  const assuntoLabel = isEmail ? 'Assunto' : isContextual ? 'Título' : 'Assunto';

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/comunicacao/templates">
            <ArrowLeft className="w-4 h-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">{isEditing ? 'Editar Template' : 'Novo Template'}</h1>
          <p className="text-muted-foreground mt-1">Base reutilizável para campanhas e automações.</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do template</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome</Label>
                <Input id="nome" value={formData.nome} onChange={(e) => setFormData((prev) => ({ ...prev, nome: e.target.value }))} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="objetivo">Objetivo</Label>
                <Input id="objetivo" value={formData.objetivo} onChange={(e) => setFormData((prev) => ({ ...prev, objetivo: e.target.value }))} />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="canal">Canal</Label>
                <select id="canal" value={formData.canal} onChange={(e) => setFormData((prev) => ({ ...prev, canal: Number(e.target.value) }))} className="w-full rounded-md border border-input bg-background px-3 py-2">
                  {canalOptions.map((option) => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                  ))}
                </select>
              </div>
              {isEditing && (
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <select id="status" value={formData.status} onChange={(e) => setFormData((prev) => ({ ...prev, status: Number(e.target.value) }))} className="w-full rounded-md border border-input bg-background px-3 py-2">
                    <option value={1}>Rascunho</option>
                    <option value={2}>Ativo</option>
                    <option value={3}>Arquivado</option>
                  </select>
                </div>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="assunto">{assuntoLabel}</Label>
              <Input id="assunto" value={formData.assunto} onChange={(e) => setFormData((prev) => ({ ...prev, assunto: e.target.value }))} placeholder={isEmail || isContextual ? 'Obrigatório para este canal' : 'Opcional para este canal'} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="corpo">{isEmail ? 'Corpo texto' : 'Mensagem'}</Label>
              <Textarea id="corpo" rows={6} value={formData.corpo} onChange={(e) => setFormData((prev) => ({ ...prev, corpo: e.target.value }))} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="corpoHtml">Corpo HTML opcional</Label>
              <Textarea id="corpoHtml" rows={6} value={formData.corpoHtml} onChange={(e) => setFormData((prev) => ({ ...prev, corpoHtml: e.target.value }))} />
              <p className="text-xs text-muted-foreground">
                {isEmail ? 'Use HTML quando quiser uma versão mais rica do e-mail.' : isContextual ? 'Para canais contextuais, o HTML normalmente não será usado no primeiro momento.' : 'Para WhatsApp, normalmente só o corpo texto será usado.'}
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="variaveisPermitidas">Variáveis permitidas</Label>
              <Input id="variaveisPermitidas" value={formData.variaveisPermitidas} onChange={(e) => setFormData((prev) => ({ ...prev, variaveisPermitidas: e.target.value }))} />
            </div>

            <div className="space-y-3">
              <div className="flex items-center justify-between gap-3">
                <div className="space-y-1">
                  <Label>Preview do template</Label>
                  <p className="text-xs text-muted-foreground">
                    A pré-visualização usa valores de exemplo para facilitar a revisão do conteúdo antes do uso em campanha.
                  </p>
                </div>
                <Badge variant="outline">{canalBadge}</Badge>
              </div>

              <div className="rounded-xl border border-border bg-muted/20 p-4 space-y-3">
                {(isEmail || isContextual) && (
                  <div className="space-y-1">
                    <div className="text-xs uppercase tracking-wide text-muted-foreground">{assuntoLabel}</div>
                    <div className="rounded-md border border-border bg-background px-3 py-2 text-sm">{previewAssunto}</div>
                  </div>
                )}

                <div className="space-y-1">
                  <div className="text-xs uppercase tracking-wide text-muted-foreground">{isEmail ? 'Corpo texto' : 'Mensagem'}</div>
                  <div className="rounded-md border border-border bg-background px-3 py-3 text-sm whitespace-pre-wrap">{previewCorpo || 'Sem conteúdo para pré-visualizar.'}</div>
                </div>

                {isEmail && previewHtml && (
                  <div className="space-y-1">
                    <div className="text-xs uppercase tracking-wide text-muted-foreground">Corpo HTML</div>
                    <div className="rounded-md border border-border bg-background px-3 py-3 text-sm whitespace-pre-wrap">{previewHtml}</div>
                  </div>
                )}
              </div>
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-border">
              <Button type="button" variant="outline" asChild>
                <Link to="/comunicacao/templates">Cancelar</Link>
              </Button>
              <Button type="submit" disabled={loading}>
                <Save className="w-4 h-4 mr-2" />
                {loading ? 'Salvando...' : 'Salvar'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
