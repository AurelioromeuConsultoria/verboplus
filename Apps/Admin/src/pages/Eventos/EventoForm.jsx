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
import { eventosApi } from '@/lib/api';

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

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await eventosApi.getById(id);
      const e = res.data;
      setFormData({
        titulo: e.titulo || '',
        descricao: e.descricao || '',
        imagemDestaque: e.imagemDestaque || '',
        url: e.url || '',
        dataInicio: e.dataInicio ? new Date(e.dataInicio).toISOString().slice(0, 16) : '',
        dataFim: e.dataFim ? new Date(e.dataFim).toISOString().slice(0, 16) : '',
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const payload = {
        titulo: formData.titulo.trim() || null,
        descricao: formData.descricao.trim() || null,
        imagemDestaque: formData.imagemDestaque.trim() || null,
        url: formData.url.trim() || null,
        dataInicio: formData.dataInicio ? new Date(formData.dataInicio).toISOString() : null,
        dataFim: formData.dataFim ? new Date(formData.dataFim).toISOString() : null,
      };
      if (isEditing) await eventosApi.update(id, payload);
      else await eventosApi.create(payload);
      navigate('/eventos');
    } catch (err) {
      alert('Erro ao salvar evento');
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
                <Input id="url" name="url" type="url" value={formData.url} onChange={handleChange} placeholder="https://exemplo.com" />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="descricao">Descrição</Label>
              <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição do evento" rows={4} />
            </div>

            <div className="space-y-2">
              <Label htmlFor="imagemDestaque">URL da Imagem de Destaque</Label>
              <Input id="imagemDestaque" name="imagemDestaque" type="url" value={formData.imagemDestaque} onChange={handleChange} placeholder="https://exemplo.com/imagem.jpg" />
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="dataInicio">Data e Hora de Início</Label>
                <Input id="dataInicio" name="dataInicio" type="datetime-local" value={formData.dataInicio} onChange={handleChange} />
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


