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
import { destaquesSiteApi } from '@/lib/api';
import { toast } from 'sonner';

export default function DestaqueSiteForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState({
    texto: '',
    descricao: '',
    url: '',
    imagem: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await destaquesSiteApi.getById(id);
      const d = res.data;
      setFormData({
        texto: d.texto || '',
        descricao: d.descricao || '',
        url: d.url || '',
        imagem: d.imagem || '',
      });
    } catch (err) {
      setError('Erro ao carregar destaque do site');
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
      const payload = {
        texto: formData.texto.trim() || null,
        descricao: formData.descricao.trim() || null,
        url: normalizeUrl(formData.url),
        imagem: formData.imagem.trim() || null,
      };
      if (isEditing) await destaquesSiteApi.update(id, payload);
      else await destaquesSiteApi.create(payload);
      toast.success(isEditing ? 'Destaque atualizado com sucesso!' : 'Destaque criado com sucesso!');
      navigate('/destaques-site');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar destaque do site';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando destaque..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/destaques-site">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Destaque do Site' : 'Novo Destaque do Site'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do destaque' : 'Cadastre um novo destaque do site'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Destaque do Site' : 'Cadastrar Destaque do Site'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="texto">Texto</Label>
                <Input id="texto" name="texto" value={formData.texto} onChange={handleChange} placeholder="Texto do destaque" />
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
              <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição do destaque" rows={3} />
            </div>

            <div className="space-y-2">
              <ImageUpload
                label="Imagem"
                value={formData.imagem}
                onChange={(url) => setFormData((prev) => ({ ...prev, imagem: url }))}
                accept="image/*"
                type="image"
              />
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/destaques-site">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}


