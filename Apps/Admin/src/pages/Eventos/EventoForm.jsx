import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ImageUpload } from '@/components/ImageUpload';
import { eventosApi, normalizeEvento } from '@/lib/api';
import { toast } from 'sonner';

export default function EventoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState({
    titulo: '',
    descricao: '',
    imagemDestaque: '',
    url: '',
    dataInicio: '',
    dataFim: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

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
      });
    } catch (err) {
      setError('Erro ao carregar evento');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
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
      };
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

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/eventos">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}


