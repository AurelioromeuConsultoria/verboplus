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
import { RichTextEditor } from '@/components/RichTextEditor';
import { noticiasApi, categoriasNoticiasApi } from '@/lib/api';
import { toast } from 'sonner';

export default function NoticiaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [categorias, setCategorias] = useState([]);
  const [formData, setFormData] = useState({
    titulo: '',
    descricao: '',
    texto: '',
    data: '',
    url: '',
    imagem: '',
    categoriaNoticiaId: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const categoriasRes = await categoriasNoticiasApi.getAll();
      setCategorias(categoriasRes.data || []);

      if (isEditing) {
        const res = await noticiasApi.getById(id);
        const n = res.data;
        setFormData({
          titulo: n.titulo || '',
          descricao: n.descricao || '',
          texto: n.texto || '',
          data: n.data ? new Date(n.data).toISOString().slice(0, 16) : '',
          url: n.url || '',
          imagem: n.imagem || '',
          categoriaNoticiaId: String(n.categoriaNoticiaId || ''),
        });
      }
    } catch (err) {
      setError('Erro ao carregar dados');
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
    if (!formData.categoriaNoticiaId) {
      toast.error('Categoria é obrigatória');
      return;
    }
    try {
      setLoading(true);
      const payload = {
        titulo: formData.titulo.trim() || null,
        descricao: formData.descricao.trim() || null,
        texto: formData.texto.trim() || null,
        data: formData.data ? new Date(formData.data).toISOString() : null,
        url: normalizeUrl(formData.url),
        imagem: formData.imagem.trim() || null,
        categoriaNoticiaId: Number(formData.categoriaNoticiaId),
      };
      if (isEditing) await noticiasApi.update(id, payload);
      else await noticiasApi.create(payload);
      toast.success(isEditing ? 'Notícia atualizada com sucesso!' : 'Notícia criada com sucesso!');
      navigate('/noticias');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar notícia';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando notícia..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/noticias">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Notícia' : 'Nova Notícia'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da notícia' : 'Cadastre uma nova notícia'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Notícia' : 'Cadastrar Notícia'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="titulo">Título *</Label>
                <Input id="titulo" name="titulo" value={formData.titulo} onChange={handleChange} placeholder="Título da notícia" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="categoriaNoticiaId">Categoria *</Label>
                <select id="categoriaNoticiaId" name="categoriaNoticiaId" value={formData.categoriaNoticiaId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {categorias.map((c) => (
                    <option key={c.id} value={c.id}>{c.nome}</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="descricao">Descrição</Label>
              <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição da notícia" rows={3} />
            </div>

            <div className="space-y-2">
              <RichTextEditor
                label="Texto"
                name="texto"
                value={formData.texto}
                onChange={handleChange}
                placeholder="Texto completo da notícia. Cole o texto e os espaços entre parágrafos serão preservados automaticamente."
              />
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="data">Data</Label>
                <Input id="data" name="data" type="datetime-local" value={formData.data} onChange={handleChange} />
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
                <Link to="/noticias">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}


