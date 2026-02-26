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
import { receitasApi, contasBancariasApi, centrosCustosApi, projetosApi, categoriasReceitasApi } from '@/lib/api';
import { toast } from 'sonner';

export default function ReceitaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [contas, setContas] = useState([]);
  const [centrosCustos, setCentrosCustos] = useState([]);
  const [projetos, setProjetos] = useState([]);
  const [categoriasReceitas, setCategoriasReceitas] = useState([]);

  const [formData, setFormData] = useState({
    descricao: '',
    valor: '',
    dataRecebimento: '',
    status: 'Pendente',
    observacoes: '',
    comprovanteUrl: '',
    categoriaReceitaId: '',
    contaBancariaId: '',
    centroCustoId: '',
    projetoId: '',
  });

  const loadDependencies = async () => {
    try {
      const [contasRes, centrosRes, projetosRes, categoriasRes] = await Promise.all([
        contasBancariasApi.getAll(),
        centrosCustosApi.getAll(),
        projetosApi.getAll(),
        categoriasReceitasApi.getAll(),
      ]);
      setContas(contasRes.data || []);
      setCentrosCustos(centrosRes.data || []);
      setProjetos(projetosRes.data || []);
      setCategoriasReceitas(categoriasRes.data || []);
    } catch (err) {
      console.error('Erro ao carregar dependências:', err);
    }
  };

  const load = async () => {
    await loadDependencies();
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await receitasApi.getById(id);
      const r = res.data || {};
      setFormData({
        descricao: r.descricao || '',
        valor: r.valor !== undefined ? String(r.valor) : '',
        dataRecebimento: r.dataRecebimento ? new Date(r.dataRecebimento).toISOString().slice(0, 10) : '',
        status: r.status || 'Pendente',
        observacoes: r.observacoes || '',
        comprovanteUrl: r.comprovanteUrl || '',
        categoriaReceitaId: r.categoriaReceitaId ? String(r.categoriaReceitaId) : '',
        contaBancariaId: r.contaBancariaId ? String(r.contaBancariaId) : '',
        centroCustoId: r.centroCustoId ? String(r.centroCustoId) : '',
        projetoId: r.projetoId ? String(r.projetoId) : '',
      });
    } catch (err) {
      setError('Erro ao carregar receita');
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
    if (!formData.descricao.trim()) {
      toast.error('Descrição é obrigatória');
      return;
    }
    if (!formData.valor) {
      toast.error('Valor é obrigatório');
      return;
    }
    if (!formData.dataRecebimento) {
      toast.error('Data de recebimento é obrigatória');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        descricao: formData.descricao.trim(),
        valor: parseFloat(formData.valor) || 0,
        dataRecebimento: new Date(formData.dataRecebimento).toISOString(),
        status: formData.status,
        observacoes: formData.observacoes?.trim() || null,
        comprovanteUrl: formData.comprovanteUrl?.trim() || null,
        categoriaReceitaId: formData.categoriaReceitaId ? Number(formData.categoriaReceitaId) : null,
        contaBancariaId: formData.contaBancariaId ? Number(formData.contaBancariaId) : null,
        centroCustoId: formData.centroCustoId ? Number(formData.centroCustoId) : null,
        projetoId: formData.projetoId ? Number(formData.projetoId) : null,
      };
      if (isEditing) await receitasApi.update(id, payload);
      else await receitasApi.create(payload);
      toast.success(isEditing ? 'Receita atualizada com sucesso!' : 'Receita criada com sucesso!');
      navigate('/financeiro/receitas');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar receita';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando receita..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/receitas">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Receita' : 'Nova Receita'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da receita' : 'Cadastre uma nova receita'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Receita</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="descricao">Descrição *</Label>
                <Input id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição da receita" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="valor">Valor *</Label>
                <Input id="valor" name="valor" type="number" step="0.01" value={formData.valor} onChange={handleChange} placeholder="0.00" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="dataRecebimento">Data de Recebimento *</Label>
                <Input id="dataRecebimento" name="dataRecebimento" type="date" value={formData.dataRecebimento} onChange={handleChange} required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="status">Status *</Label>
                <select id="status" name="status" value={formData.status} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="Pendente">Pendente</option>
                  <option value="Recebida">Recebida</option>
                  <option value="Cancelada">Cancelada</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="categoriaReceitaId">Categoria de Receita</Label>
                <select id="categoriaReceitaId" name="categoriaReceitaId" value={formData.categoriaReceitaId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {categoriasReceitas.filter(c => c.ativo).map((c) => (
                    <option key={c.id} value={c.id}>{c.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="contaBancariaId">Conta Bancária</Label>
                <select id="contaBancariaId" name="contaBancariaId" value={formData.contaBancariaId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {contas.map((c) => (
                    <option key={c.id} value={c.id}>{c.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="centroCustoId">Centro de Custo</Label>
                <select id="centroCustoId" name="centroCustoId" value={formData.centroCustoId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {centrosCustos.map((c) => (
                    <option key={c.id} value={c.id}>{c.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="projetoId">Projeto</Label>
                <select id="projetoId" name="projetoId" value={formData.projetoId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {projetos.map((p) => (
                    <option key={p.id} value={p.id}>{p.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="observacoes">Observações</Label>
                <Textarea id="observacoes" name="observacoes" value={formData.observacoes} onChange={handleChange} placeholder="Observações sobre a receita" rows={3} />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="comprovanteUrl">URL do Comprovante</Label>
                <Input id="comprovanteUrl" name="comprovanteUrl" value={formData.comprovanteUrl} onChange={handleChange} placeholder="https://..." />
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/financeiro/receitas">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
