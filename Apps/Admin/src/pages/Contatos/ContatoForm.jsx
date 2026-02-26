import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { contatosApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

export default function ContatoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState({
    nome: '',
    whatsApp: '',
    email: '',
    membro: false,
    mensagem: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await contatosApi.getById(id);
      const c = res.data;
      setFormData({
        nome: c.nome || '',
        whatsApp: c.whatsApp || '',
        email: c.email || '',
        membro: c.membro || false,
        mensagem: c.mensagem || '',
      });
    } catch (err) {
      setError('Erro ao carregar contato');
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

  const handleSwitchChange = (checked) => {
    setFormData((prev) => ({ ...prev, membro: checked }));
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
        whatsApp: formData.whatsApp.trim() || null,
        email: formData.email.trim() || null,
        membro: formData.membro,
        mensagem: formData.mensagem.trim() || null,
      };
      if (isEditing) await contatosApi.update(id, payload);
      else await contatosApi.create(payload);
      toast.success(isEditing ? 'Contato atualizado com sucesso' : 'Contato criado com sucesso');
      navigate('/contatos');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar contato'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando contato..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/contatos">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Contato' : 'Novo Contato'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do contato' : 'Cadastre um novo contato'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Contato' : 'Cadastrar Contato'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome completo" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="whatsApp">WhatsApp</Label>
                <Input id="whatsApp" name="whatsApp" value={formData.whatsApp} onChange={handleChange} placeholder="(11) 99999-9999" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input id="email" name="email" type="email" value={formData.email} onChange={handleChange} placeholder="email@exemplo.com" />
              </div>
              <div className="space-y-2 flex items-center space-x-3 pt-6">
                <Switch id="membro" checked={formData.membro} onCheckedChange={handleSwitchChange} />
                <Label htmlFor="membro" className="cursor-pointer">É membro da igreja?</Label>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="mensagem">Mensagem</Label>
              <Textarea id="mensagem" name="mensagem" value={formData.mensagem} onChange={handleChange} placeholder="Mensagem do contato" rows={4} />
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/contatos">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}







