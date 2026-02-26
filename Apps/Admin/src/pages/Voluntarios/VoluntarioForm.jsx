import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { voluntariosApi, equipesApi, cargosApi, pessoasApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const WHATSAPP_REGEX = /^\d{10,13}$/;

export default function VoluntarioForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [pessoas, setPessoas] = useState([]);
  const [pessoaBusca, setPessoaBusca] = useState('');
  const [equipes, setEquipes] = useState([]);
  const [cargos, setCargos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    pessoaId: '',
    whatsApp: '',
    email: '',
    telefone: '',
    equipeId: '',
    cargoId: '',
  });

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [p, e, c] = await Promise.all([
        pessoasApi.getAll(),
        equipesApi.getAll(),
        cargosApi.getAll(),
      ]);
      setPessoas(p.data || []);
      setEquipes(e.data || []);
      setCargos(c.data || []);

      if (isEditing) {
        const res = await voluntariosApi.getById(id);
        const v = res.data;
        setFormData({
          pessoaId: String(v.pessoaId || ''),
          whatsApp: v.whatsApp || '',
          email: v.email || '',
          telefone: v.telefone || '',
          equipeId: String(v.equipeId || ''),
          cargoId: String(v.cargoId || ''),
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

  const pessoasFiltradas = pessoas.filter((p) => {
    if (!pessoaBusca) return true;
    const b = pessoaBusca.toLowerCase();
    const nome = (p.nome || '').toLowerCase();
    const email = (p.email || '').toLowerCase();
    const whats = String(p.whatsApp || '').toLowerCase();
    return nome.includes(b) || email.includes(b) || whats.includes(b);
  });

  const pessoaSelecionada = pessoas.find((p) => String(p.id) === String(formData.pessoaId));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.pessoaId) {
      toast.error('Selecione uma Pessoa');
      return;
    }
    const onlyDigits = String(formData.whatsApp).replace(/\D/g, '');
    if (!formData.equipeId || !formData.cargoId) {
      toast.error('Selecione Equipe e Cargo');
      return;
    }
    if (formData.email && !/.+@.+\..+/.test(formData.email)) {
      toast.error('E-mail inválido');
      return;
    }
    if (formData.whatsApp && !WHATSAPP_REGEX.test(onlyDigits)) {
      toast.error('WhatsApp inválido. Use apenas dígitos (10 a 13).');
      return;
    }
    try {
      setLoading(true);
      const payload = {
        pessoaId: Number(formData.pessoaId),
        whatsApp: formData.whatsApp ? onlyDigits : null,
        email: formData.email?.trim() || null,
        telefone: formData.telefone?.trim() || null,
        equipeId: Number(formData.equipeId),
        cargoId: Number(formData.cargoId),
      };
      if (isEditing) await voluntariosApi.update(id, payload);
      else await voluntariosApi.create(payload);
      toast.success(isEditing ? 'Voluntário atualizado com sucesso' : 'Voluntário criado com sucesso');
      navigate('/voluntarios');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar voluntário'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando voluntário..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/voluntarios">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Voluntário' : 'Novo Voluntário'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do voluntário' : 'Cadastre um novo voluntário'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Voluntário' : 'Cadastrar Voluntário'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="pessoaBusca">Pessoa *</Label>
                <Input
                  id="pessoaBusca"
                  name="pessoaBusca"
                  value={pessoaBusca}
                  onChange={(e) => setPessoaBusca(e.target.value)}
                  placeholder="Buscar pessoa por nome, email ou WhatsApp"
                />
                <select
                  id="pessoaId"
                  name="pessoaId"
                  value={formData.pessoaId}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border rounded"
                  required
                >
                  <option value="">Selecione</option>
                  {pessoasFiltradas.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nome}{p.email ? ` — ${p.email}` : ''}{p.whatsApp ? ` — ${p.whatsApp}` : ''}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="whatsApp">WhatsApp</Label>
                <Input
                  id="whatsApp"
                  name="whatsApp"
                  value={formData.whatsApp}
                  onChange={handleChange}
                  placeholder={pessoaSelecionada?.whatsApp ? `Atual: ${pessoaSelecionada.whatsApp}` : '11999998888 (apenas dígitos)'}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input id="email" name="email" type="email" value={formData.email} onChange={handleChange} placeholder="email@exemplo.com" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="telefone">Telefone</Label>
                <Input id="telefone" name="telefone" value={formData.telefone} onChange={handleChange} placeholder="11999998888 (apenas dígitos)" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="equipeId">Equipe *</Label>
                <select id="equipeId" name="equipeId" value={formData.equipeId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {equipes.map((e) => (
                    <option key={e.id} value={e.id}>{e.nome}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="cargoId">Cargo *</Label>
                <select id="cargoId" name="cargoId" value={formData.cargoId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {cargos.map((c) => (
                    <option key={c.id} value={c.id}>{c.nome}</option>
                  ))}
                </select>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/voluntarios">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}


