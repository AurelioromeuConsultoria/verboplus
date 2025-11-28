import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { equipesApi } from '@/lib/api';

export default function EquipeForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState({
    nome: '',
    area: '1',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await equipesApi.getById(id);
      const e = res.data;
      setFormData({ nome: e.nome || '', area: String(e.area || '1') });
    } catch (err) {
      setError('Erro ao carregar equipe');
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
    if (!formData.nome.trim()) {
      alert('Nome é obrigatório');
      return;
    }
    if (!['1', '2', '3'].includes(formData.area)) {
      alert('Área inválida');
      return;
    }
    try {
      setLoading(true);
      const payload = { nome: formData.nome.trim(), area: Number(formData.area) };
      if (isEditing) await equipesApi.update(id, payload);
      else await equipesApi.create(payload);
      navigate('/equipes');
    } catch (err) {
      alert('Erro ao salvar equipe');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando equipe..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/equipes">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Equipe' : 'Nova Equipe'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da equipe' : 'Cadastre uma nova equipe'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Equipe' : 'Cadastrar Equipe'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome da equipe" required />
              </div>

              <div className="space-y-2">
                <Label htmlFor="area">Área *</Label>
                <select id="area" name="area" value={formData.area} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="1">Verde</option>
                  <option value="2">Vermelha</option>
                  <option value="3">Laranja</option>
                </select>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/equipes">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}


