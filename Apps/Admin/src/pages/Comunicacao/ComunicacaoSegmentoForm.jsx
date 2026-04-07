import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { comunicacaoSegmentosApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { toast } from 'sonner';

const publicosOptions = [
  { value: 'visitantes', label: 'Visitantes' },
  { value: 'membros', label: 'Membros' },
  { value: 'voluntarios', label: 'Voluntários' },
  { value: 'responsaveis-kids', label: 'Responsáveis do Kids' },
  { value: 'pessoas', label: 'Pessoas' },
];

export default function ComunicacaoSegmentoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const [loading, setLoading] = useState(false);
  const [pageLoading, setPageLoading] = useState(isEditing);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    nome: '',
    descricao: '',
    publicoAlvo: 'visitantes',
    ativo: true,
  });

  useEffect(() => {
    if (!isEditing) return;

    const load = async () => {
      try {
        const response = await comunicacaoSegmentosApi.getById(id);
        const data = response.data;
        setFormData({
          nome: data.nome || '',
          descricao: data.descricao || '',
          publicoAlvo: data.publicoAlvo || 'visitantes',
          ativo: data.ativo ?? true,
        });
      } catch (err) {
        setError(getApiErrorMessage(err, 'Erro ao carregar segmento'));
      } finally {
        setPageLoading(false);
      }
    };

    load();
  }, [id, isEditing]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.nome.trim() || !formData.publicoAlvo.trim()) {
      toast.error('Preencha nome e público alvo do segmento.');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        nome: formData.nome.trim(),
        descricao: formData.descricao.trim() || null,
        publicoAlvo: formData.publicoAlvo,
        ...(isEditing ? { ativo: formData.ativo } : {}),
      };

      if (isEditing) await comunicacaoSegmentosApi.update(id, payload);
      else await comunicacaoSegmentosApi.create(payload);

      toast.success(isEditing ? 'Segmento atualizado com sucesso.' : 'Segmento criado com sucesso.');
      navigate('/comunicacao/segmentos');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar segmento'));
    } finally {
      setLoading(false);
    }
  };

  if (pageLoading) return <LoadingPage text="Carregando segmento..." />;
  if (error) return <ErrorPage message={error} onRetry={() => window.location.reload()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/comunicacao/segmentos">
            <ArrowLeft className="w-4 h-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">{isEditing ? 'Editar Segmento' : 'Novo Segmento'}</h1>
          <p className="text-muted-foreground mt-1">Salve públicos prioritários para reutilizar em campanhas e automações.</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do segmento</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="nome">Nome</Label>
              <Input id="nome" value={formData.nome} onChange={(e) => setFormData((prev) => ({ ...prev, nome: e.target.value }))} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="descricao">Descrição</Label>
              <Input id="descricao" value={formData.descricao} onChange={(e) => setFormData((prev) => ({ ...prev, descricao: e.target.value }))} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="publicoAlvo">Público alvo</Label>
              <select id="publicoAlvo" value={formData.publicoAlvo} onChange={(e) => setFormData((prev) => ({ ...prev, publicoAlvo: e.target.value }))} className="w-full rounded-md border border-input bg-background px-3 py-2">
                {publicosOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </div>

            {isEditing && (
              <label className="flex items-center gap-3 rounded-lg border border-border p-3 cursor-pointer">
                <input type="checkbox" checked={formData.ativo} onChange={(e) => setFormData((prev) => ({ ...prev, ativo: e.target.checked }))} />
                <span className="text-sm font-medium">Segmento ativo</span>
              </label>
            )}

            <div className="flex justify-end gap-3 pt-4 border-t border-border">
              <Button type="button" variant="outline" asChild>
                <Link to="/comunicacao/segmentos">Cancelar</Link>
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
