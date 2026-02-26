import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { contasBancariasApi } from '@/lib/api';
import { toast } from 'sonner';

export default function ContaBancariaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    banco: '',
    agencia: '',
    conta: '',
    tipoConta: '',
    saldoInicial: '',
    ativo: true,
  });

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await contasBancariasApi.getById(id);
      const c = res.data || {};
      setFormData({
        nome: c.nome || '',
        banco: c.banco || '',
        agencia: c.agencia || '',
        conta: c.conta || '',
        tipoConta: c.tipoConta || '',
        saldoInicial: c.saldoInicial !== undefined ? String(c.saldoInicial) : '',
        ativo: c.ativo !== undefined ? c.ativo : true,
      });
    } catch (err) {
      setError('Erro ao carregar conta bancária');
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
    if (!formData.saldoInicial) {
      toast.error('Saldo inicial é obrigatório');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        nome: formData.nome.trim(),
        banco: formData.banco?.trim() || null,
        agencia: formData.agencia?.trim() || null,
        conta: formData.conta?.trim() || null,
        tipoConta: formData.tipoConta?.trim() || null,
        saldoInicial: parseFloat(formData.saldoInicial) || 0,
        ativo: formData.ativo,
      };
      if (isEditing) await contasBancariasApi.update(id, payload);
      else await contasBancariasApi.create(payload);
      toast.success(isEditing ? 'Conta bancária atualizada com sucesso!' : 'Conta bancária criada com sucesso!');
      navigate('/financeiro/contas-bancarias');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar conta bancária';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando conta bancária..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/contas-bancarias">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Conta Bancária' : 'Nova Conta Bancária'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da conta' : 'Cadastre uma nova conta bancária'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Conta Bancária</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome da conta" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="banco">Banco</Label>
                <Input id="banco" name="banco" value={formData.banco} onChange={handleChange} placeholder="Nome do banco" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="agencia">Agência</Label>
                <Input id="agencia" name="agencia" value={formData.agencia} onChange={handleChange} placeholder="Número da agência" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="conta">Conta</Label>
                <Input id="conta" name="conta" value={formData.conta} onChange={handleChange} placeholder="Número da conta" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="tipoConta">Tipo de Conta</Label>
                <select id="tipoConta" name="tipoConta" value={formData.tipoConta} onChange={handleChange} className="w-full px-3 py-2 border rounded">
                  <option value="">Selecione</option>
                  <option value="Corrente">Corrente</option>
                  <option value="Poupança">Poupança</option>
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="saldoInicial">Saldo Inicial *</Label>
                <Input id="saldoInicial" name="saldoInicial" type="number" step="0.01" value={formData.saldoInicial} onChange={handleChange} placeholder="0.00" required />
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
                <Label htmlFor="ativo" className="cursor-pointer">Conta ativa</Label>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/financeiro/contas-bancarias">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
