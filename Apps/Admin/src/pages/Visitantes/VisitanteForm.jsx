import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save, RefreshCcw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { visitantesApi } from '@/lib/api';
import { toast } from 'sonner';

export function VisitanteForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const [formData, setFormData] = useState({
    nome: '',
    telefone: '',
    whatsApp: '',
    email: '',
    dataNascimento: '',
    dataVisita: new Date().toISOString().split('T')[0],
    observacoes: ''
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [regenerando, setRegenerando] = useState(false);
  const [error, setError] = useState(null);

  const loadVisitante = async () => {
    if (!isEditing) return;

    try {
      setLoading(true);
      setError(null);
      const response = await visitantesApi.getById(id);
      const visitante = response.data;
      setFormData({
        nome: visitante.nome || '',
        telefone: visitante.telefone || '',
        whatsApp: visitante.whatsApp || '',
        email: visitante.email || '',
        dataNascimento: visitante.dataNascimento ? visitante.dataNascimento.split('T')[0] : '',
        dataVisita: visitante.dataVisita ? visitante.dataVisita.split('T')[0] : new Date().toISOString().split('T')[0],
        observacoes: visitante.observacoes || ''
      });
    } catch (err) {
      setError('Erro ao carregar visitante');
      console.error('Erro ao carregar visitante:', err);
    } finally {
      setLoading(false);
    }
  };

  const normalizePhone = (phone) => {
    return phone ? phone.replace(/\D/g, '') : null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.nome || !formData.dataVisita) {
      toast.error('Nome e Data da Visita são obrigatórios');
      return;
    }

    if (formData.email && !/.+@.+\..+/.test(formData.email)) {
      toast.error('Email inválido');
      return;
    }

    try {
      setSaving(true);
      
      const submitData = {
        nome: formData.nome.trim(),
        email: formData.email?.trim() || null,
        telefone: normalizePhone(formData.telefone),
        whatsApp: normalizePhone(formData.whatsApp),
        dataNascimento: formData.dataNascimento 
          ? new Date(formData.dataNascimento + 'T00:00:00').toISOString()
          : null,
        dataVisita: new Date(formData.dataVisita + 'T00:00:00').toISOString(),
        observacoes: formData.observacoes?.trim() || null
      };

      if (isEditing) {
        // Para edição, só atualiza campos da visita
        await visitantesApi.update(id, {
          dataVisita: submitData.dataVisita,
          observacoes: submitData.observacoes
        });
        toast.success('Visita atualizada com sucesso');
      } else {
        await visitantesApi.create(submitData);
        toast.success('Visita registrada com sucesso');
      }

      navigate('/visitantes');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 
                          err.response?.data?.error ||
                          `Erro ao ${isEditing ? 'atualizar' : 'cadastrar'} visita`;
      toast.error(errorMessage);
      console.error(`Erro ao ${isEditing ? 'atualizar' : 'cadastrar'} visita:`, err);
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleRegerarMensagens = async () => {
    if (!isEditing) return;
    try {
      setRegenerando(true);
      const res = await visitantesApi.regerarMensagens(id);
      toast.success(
        `Mensagens regeradas: ${res.data?.mensagensCriadas ?? 0} criadas, ${res.data?.mensagensCanceladas ?? 0} canceladas`
      );
    } catch (err) {
      const msg = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || err.response?.data?.error || 'Erro ao regerar mensagens');
      toast.error(msg);
      console.error(err);
    } finally {
      setRegenerando(false);
    }
  };

  useEffect(() => {
    loadVisitante();
  }, [id]);

  if (loading) {
    return <LoadingPage text="Carregando visitante..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadVisitante} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/visitantes">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">
            {isEditing ? 'Editar Visitante' : 'Novo Visitante'}
          </h1>
          <p className="text-muted-foreground">
            {isEditing ? 'Atualize as informações do visitante' : 'Cadastre um novo visitante'}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>
            {isEditing ? 'Editar Visitante' : 'Cadastrar Visitante'}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input
                  id="nome"
                  name="nome"
                  value={formData.nome}
                  onChange={handleChange}
                  placeholder="Nome completo"
                  required
                  disabled={isEditing}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="email@exemplo.com"
                  disabled={isEditing}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="telefone">Telefone</Label>
                <Input
                  id="telefone"
                  name="telefone"
                  value={formData.telefone}
                  onChange={handleChange}
                  placeholder="11999998888 (apenas números)"
                  disabled={isEditing}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="whatsApp">WhatsApp</Label>
                <Input
                  id="whatsApp"
                  name="whatsApp"
                  value={formData.whatsApp}
                  onChange={handleChange}
                  placeholder="11999998888 (apenas números)"
                  disabled={isEditing}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dataNascimento">Data de Nascimento</Label>
                <Input
                  id="dataNascimento"
                  name="dataNascimento"
                  type="date"
                  value={formData.dataNascimento}
                  onChange={handleChange}
                  disabled={isEditing}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dataVisita">Data da Visita *</Label>
                <Input
                  id="dataVisita"
                  name="dataVisita"
                  type="date"
                  value={formData.dataVisita}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="observacoes">Observações</Label>
              <Textarea
                id="observacoes"
                name="observacoes"
                value={formData.observacoes}
                onChange={handleChange}
                placeholder="Observações sobre o visitante..."
                rows={3}
              />
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={saving}>
                <Save className="h-4 w-4 mr-2" />
                {saving ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
              </Button>
              {isEditing && (
                <Button type="button" variant="outline" onClick={handleRegerarMensagens} disabled={regenerando}>
                  <RefreshCcw className="h-4 w-4 mr-2" />
                  {regenerando ? 'Regerando...' : 'Regerar Mensagens'}
                </Button>
              )}
              <Button type="button" variant="outline" asChild>
                <Link to="/visitantes">Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

