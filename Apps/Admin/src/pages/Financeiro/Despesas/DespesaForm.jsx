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
import { despesasApi, fornecedoresApi, categoriasDespesasApi, contasBancariasApi, centrosCustosApi, projetosApi } from '@/lib/api';
import { toast } from 'sonner';

export default function DespesaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [fornecedores, setFornecedores] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [contas, setContas] = useState([]);
  const [centrosCustos, setCentrosCustos] = useState([]);
  const [projetos, setProjetos] = useState([]);

  const [formData, setFormData] = useState({
    descricao: '',
    valor: '',
    dataVencimento: '',
    status: 'Pendente',
    observacoes: '',
    comprovanteUrl: '',
    fornecedorId: '',
    categoriaDespesaId: '',
    contaBancariaId: '',
    centroCustoId: '',
    projetoId: '',
  });

  const loadDependencies = async () => {
    try {
      const [fornecedoresRes, categoriasRes, contasRes, centrosRes, projetosRes] = await Promise.all([
        fornecedoresApi.getAll(),
        categoriasDespesasApi.getAll(),
        contasBancariasApi.getAll(),
        centrosCustosApi.getAll(),
        projetosApi.getAll(),
      ]);
      setFornecedores(fornecedoresRes.data || []);
      setCategorias(categoriasRes.data || []);
      setContas(contasRes.data || []);
      setCentrosCustos(centrosRes.data || []);
      setProjetos(projetosRes.data || []);
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
      const res = await despesasApi.getById(id);
      const d = res.data || {};
      setFormData({
        descricao: d.descricao || '',
        valor: d.valor !== undefined ? String(d.valor) : '',
        dataVencimento: d.dataVencimento ? new Date(d.dataVencimento).toISOString().slice(0, 10) : '',
        status: d.status || 'Pendente',
        observacoes: d.observacoes || '',
        comprovanteUrl: d.comprovanteUrl || '',
        fornecedorId: d.fornecedorId ? String(d.fornecedorId) : '',
        categoriaDespesaId: d.categoriaDespesaId ? String(d.categoriaDespesaId) : '',
        contaBancariaId: d.contaBancariaId ? String(d.contaBancariaId) : '',
        centroCustoId: d.centroCustoId ? String(d.centroCustoId) : '',
        projetoId: d.projetoId ? String(d.projetoId) : '',
      });
    } catch (err) {
      setError('Erro ao carregar despesa');
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
    if (!formData.dataVencimento) {
      toast.error('Data de vencimento é obrigatória');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        descricao: formData.descricao.trim(),
        valor: parseFloat(formData.valor) || 0,
        dataVencimento: new Date(formData.dataVencimento).toISOString(),
        status: formData.status,
        observacoes: formData.observacoes?.trim() || null,
        comprovanteUrl: formData.comprovanteUrl?.trim() || null,
        fornecedorId: formData.fornecedorId ? Number(formData.fornecedorId) : null,
        categoriaDespesaId: formData.categoriaDespesaId ? Number(formData.categoriaDespesaId) : null,
        contaBancariaId: formData.contaBancariaId ? Number(formData.contaBancariaId) : null,
        centroCustoId: formData.centroCustoId ? Number(formData.centroCustoId) : null,
        projetoId: formData.projetoId ? Number(formData.projetoId) : null,
      };
      if (isEditing) await despesasApi.update(id, payload);
      else await despesasApi.create(payload);
      toast.success(isEditing ? 'Despesa atualizada com sucesso!' : 'Despesa criada com sucesso!');
      navigate('/financeiro/despesas');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar despesa';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando despesa..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/despesas">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Despesa' : 'Nova Despesa'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da despesa' : 'Cadastre uma nova despesa'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Despesa</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="descricao">Descrição *</Label>
                <Input id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição da despesa" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="valor">Valor *</Label>
                <Input id="valor" name="valor" type="number" step="0.01" value={formData.valor} onChange={handleChange} placeholder="0.00" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="dataVencimento">Data de Vencimento *</Label>
                <Input id="dataVencimento" name="dataVencimento" type="date" value={formData.dataVencimento} onChange={handleChange} required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="status">Status *</Label>
                <select id="status" name="status" value={formData.status} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="Pendente">Pendente</option>
                  <option value="Pago">Pago</option>
                  <option value="Cancelado">Cancelado</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="fornecedorId">Fornecedor</Label>
                <select id="fornecedorId" name="fornecedorId" value={formData.fornecedorId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {fornecedores.map((f) => (
                    <option key={f.id} value={f.id}>{f.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="categoriaDespesaId">Categoria de Despesa</Label>
                <select id="categoriaDespesaId" name="categoriaDespesaId" value={formData.categoriaDespesaId} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  {categorias.map((c) => (
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
                <Textarea id="observacoes" name="observacoes" value={formData.observacoes} onChange={handleChange} placeholder="Observações sobre a despesa" rows={3} />
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
            <Link to="/financeiro/despesas">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
