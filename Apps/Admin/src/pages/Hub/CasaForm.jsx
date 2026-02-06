import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { hubCasasApi, usuariosApi } from '@/lib/api';

export default function CasaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    abertoPorId: '',
    liderId: '',
    timoteoId: '',
    enderecoCompleto: '',
    anfitriao: '',
  });

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const usersRes = await usuariosApi.getAll();
      setUsuarios(usersRes.data || []);

      if (isEditing) {
        const res = await hubCasasApi.getById(id);
        const casa = res.data || {};
        setFormData({
          nome: casa.nome || '',
          abertoPorId: String(casa.abertoPorId ?? casa.abertoPor?.id ?? ''),
          liderId: String(casa.liderId ?? casa.lider?.id ?? ''),
          timoteoId: String(casa.timoteoId ?? casa.timoteo?.id ?? ''),
          enderecoCompleto: casa.enderecoCompleto || casa.endereco || '',
          anfitriao: casa.anfitriao || '',
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.nome.trim()) {
      alert('Nome é obrigatório');
      return;
    }
    if (!formData.enderecoCompleto.trim()) {
      alert('Endereço completo é obrigatório');
      return;
    }
    if (!formData.anfitriao.trim()) {
      alert('Anfitrião é obrigatório');
      return;
    }
    if (!formData.abertoPorId || !formData.liderId || !formData.timoteoId) {
      alert('Selecione Aberto por, Líder e Timóteo');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        nome: formData.nome.trim(),
        enderecoCompleto: formData.enderecoCompleto.trim(),
        anfitriao: formData.anfitriao.trim(),
        abertoPorId: Number(formData.abertoPorId),
        liderId: Number(formData.liderId),
        timoteoId: Number(formData.timoteoId),
      };
      if (isEditing) await hubCasasApi.update(id, payload);
      else await hubCasasApi.create(payload);
      navigate('/hub/casas');
    } catch (err) {
      alert('Erro ao salvar casa');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando casa..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/hub/casas">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Casa' : 'Nova Casa'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da casa' : 'Cadastre uma nova casa'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Casa' : 'Cadastrar Casa'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome da casa" required />
              </div>

              <div className="space-y-2">
                <Label htmlFor="anfitriao">Anfitrião *</Label>
                <Input id="anfitriao" name="anfitriao" value={formData.anfitriao} onChange={handleChange} placeholder="Nome do anfitrião" required />
              </div>

              <div className="space-y-2">
                <Label htmlFor="abertoPorId">Aberto por (usuário) *</Label>
                <select id="abertoPorId" name="abertoPorId" value={formData.abertoPorId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {usuarios.map((u) => (
                    <option key={u.id} value={u.id}>{u.nome || u.email || u.id}</option>
                  ))}
                </select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="liderId">Líder (usuário) *</Label>
                <select id="liderId" name="liderId" value={formData.liderId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {usuarios.map((u) => (
                    <option key={u.id} value={u.id}>{u.nome || u.email || u.id}</option>
                  ))}
                </select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="timoteoId">Timóteo (usuário) *</Label>
                <select id="timoteoId" name="timoteoId" value={formData.timoteoId} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="">Selecione</option>
                  {usuarios.map((u) => (
                    <option key={u.id} value={u.id}>{u.nome || u.email || u.id}</option>
                  ))}
                </select>
              </div>

              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="enderecoCompleto">Endereço Completo *</Label>
                <Input id="enderecoCompleto" name="enderecoCompleto" value={formData.enderecoCompleto} onChange={handleChange} placeholder="Rua, número, bairro, cidade, estado" required />
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/hub/casas">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
