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
import { projetosApi } from '@/lib/api';
import { toast } from 'sonner';

export default function ProjetoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    descricao: '',
    orcamento: '',
    ativo: true,
  });

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await projetosApi.getById(id);
      const p = res.data || {};
      setFormData({
        nome: p.nome || '',
        descricao: p.descricao || '',
        orcamento: p.orcamento !== undefined && p.orcamento !== null ? String(p.orcamento) : '',
        ativo: p.ativo !== undefined ? p.ativo : true,
      });
    } catch (err) {
      setError('Erro ao carregar projeto');
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
        orcamento: formData.orcamento ? parseFloat(formData.orcamento) : null,
        ativo: formData.ativo,
      };
      if (isEditing) await projetosApi.update(id, payload);
      else await projetosApi.create(payload);
      toast.success(isEditing ? 'Projeto atualizado com sucesso!' : 'Projeto criado com sucesso!');
      navigate('/financeiro/projetos');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar projeto';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando projeto..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/projetos">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Projeto' : 'Novo Projeto'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do projeto' : 'Cadastre um novo projeto'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados do Projeto</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome do projeto" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="orcamento">Orçamento</Label>
                <Input id="orcamento" name="orcamento" type="number" step="0.01" value={formData.orcamento} onChange={handleChange} placeholder="0.00" />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="descricao">Descrição</Label>
                <Textarea id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição do projeto" rows={3} />
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
                <Label htmlFor="ativo" className="cursor-pointer">Projeto ativo</Label>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/financeiro/projetos">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
