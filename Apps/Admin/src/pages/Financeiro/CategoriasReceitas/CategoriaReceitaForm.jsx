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
import { categoriasReceitasApi } from '@/lib/api';
import { toast } from 'sonner';

export default function CategoriaReceitaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    descricao: '',
    ativo: true,
  });

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await categoriasReceitasApi.getById(id);
      const c = res.data || {};
      setFormData({
        nome: c.nome || '',
        descricao: c.descricao || '',
        ativo: c.ativo !== undefined ? c.ativo : true,
      });
    } catch (err) {
      setError('Erro ao carregar categoria de receita');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.nome.trim()) {
      toast.error('Nome é obrigatório');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        nome: formData.nome.trim(),
        descricao: formData.descricao?.trim() || null,
        ativo: formData.ativo,
      };
      if (isEditing) await categoriasReceitasApi.update(id, payload);
      else await categoriasReceitasApi.create(payload);
      toast.success(isEditing ? 'Categoria atualizada com sucesso!' : 'Categoria criada com sucesso!');
      navigate('/financeiro/categorias-receitas');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar categoria de receita';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando categoria..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/categorias-receitas">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Categoria de Receita' : 'Nova Categoria de Receita'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da categoria' : 'Cadastre uma nova categoria de receita'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Categoria</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome da categoria" required />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="descricao">Descrição</Label>
                <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição da categoria" rows={3} />
              </div>
              <div className="space-y-2 flex items-center space-x-3">
                <input
                  type="checkbox"
                  id="ativo"
                  name="ativo"
                  checked={formData.ativo}
                  onChange={handleChange}
                  className="h-4 w-4"
                />
                <Label htmlFor="ativo" className="cursor-pointer">Categoria ativa</Label>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/financeiro/categorias-receitas">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
