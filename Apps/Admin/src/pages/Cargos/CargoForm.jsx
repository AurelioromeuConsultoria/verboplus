import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { cargosApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';
import { useTranslation } from 'react-i18next';

export default function CargoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    nome: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await cargosApi.getById(id);
      const c = res.data;
      setFormData({ nome: c.nome || '' });
    } catch (err) {
      setError('Erro ao carregar cargo');
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
      toast.error('Nome é obrigatório');
      return;
    }
    try {
      setLoading(true);
      const payload = { nome: formData.nome.trim() };
      if (isEditing) await cargosApi.update(id, payload);
      else await cargosApi.create(payload);
      toast.success(isEditing ? 'Cargo atualizado com sucesso' : 'Cargo criado com sucesso');
      navigate('/cargos');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar cargo'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando cargo..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/cargos">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? t('volunteer.roles.edit') : t('volunteer.roles.new')}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do cargo' : 'Cadastre um novo cargo'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? t('volunteer.roles.edit') : t('volunteer.roles.create')}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome do cargo" required />
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? t('actions.saving') : (isEditing ? t('volunteer.roles.update') : t('volunteer.roles.create'))}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/cargos">{t('actions.cancel')}</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

