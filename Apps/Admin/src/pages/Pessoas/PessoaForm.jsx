import { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { useFormValidation } from '@/hooks/useFormValidation';
import { pessoasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';

export default function PessoaForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const { t } = useTranslation();

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const validationRules = {
    nome: {
      required: true,
      requiredMessage: 'Nome é obrigatório',
      minLength: 2,
      minLengthMessage: 'Nome deve ter pelo menos 2 caracteres',
      maxLength: 100,
    },
    email: {
      email: true,
      emailMessage: 'Email inválido',
    },
    telefone: {
      maxLength: 20,
    },
    whatsApp: {
      maxLength: 20,
    },
  };

  const {
    values: formData,
    errors,
    touched,
    handleChange: handleValidationChange,
    handleBlur,
    validate,
    reset: resetValidation,
    setValues: setFormData,
  } = useFormValidation(validationRules, {
    nome: '',
    email: '',
    telefone: '',
    whatsApp: '',
    dataNascimento: '',
    tipoPessoa: 'Adulto',
    ativo: true,
  });

  const loadPessoa = async () => {
    if (!isEditing) return;

    try {
      setLoading(true);
      setError(null);
      const response = await pessoasApi.getById(id);
      const pessoa = response.data;
      
      const loadedData = {
        nome: pessoa.nome || '',
        email: pessoa.email || '',
        telefone: pessoa.telefone || '',
        whatsApp: pessoa.whatsApp || '',
        dataNascimento: pessoa.dataNascimento 
          ? pessoa.dataNascimento.split('T')[0] 
          : '',
        tipoPessoa: pessoa.tipoPessoa || 'Adulto',
        ativo: pessoa.ativo !== undefined ? pessoa.ativo : true,
      };
      setFormData(loadedData);
      resetValidation(loadedData);
    } catch (err) {
      setError('Erro ao carregar pessoa');
      console.error('Erro ao carregar pessoa:', err);
      toast.error('Erro ao carregar pessoa');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPessoa();
  }, [id]);

  const normalizePhone = (phone) => {
    return phone.replace(/\D/g, '');
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    const finalValue = type === 'checkbox' ? checked : value;
    handleValidationChange(name, finalValue);
  };

  const handlePhoneChange = (name, value) => {
    const normalized = normalizePhone(value);
    handleValidationChange(name, normalized);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validate()) {
      const firstError = Object.values(errors)[0];
      if (firstError) {
        toast.error(firstError);
      }
      return;
    }

    try {
      setLoading(true);
      
      const payload = {
        nome: formData.nome.trim(),
        email: formData.email?.trim() || null,
        telefone: formData.telefone ? normalizePhone(formData.telefone) : null,
        whatsApp: formData.whatsApp ? normalizePhone(formData.whatsApp) : null,
        dataNascimento: formData.dataNascimento 
          ? new Date(formData.dataNascimento + 'T00:00:00').toISOString()
          : null,
        tipoPessoa: formData.tipoPessoa,
        ativo: formData.ativo,
      };

      if (isEditing) {
        await pessoasApi.update(id, payload);
        toast.success('Pessoa atualizada com sucesso');
      } else {
        await pessoasApi.create(payload);
        toast.success('Pessoa cadastrada com sucesso');
      }

      navigate('/pessoas');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 
                          err.response?.data?.error ||
                          'Erro ao salvar pessoa';
      toast.error(errorMessage);
      console.error('Erro ao salvar pessoa:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) {
    return <LoadingPage text="Carregando pessoa..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadPessoa} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/pessoas">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">
            {isEditing ? t('people.edit') : t('people.new')}
          </h1>
          <p className="text-muted-foreground">
            {isEditing ? 'Atualize as informações da pessoa' : 'Cadastre uma nova pessoa'}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>
            {isEditing ? t('people.edit') : t('people.create')}
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
                  onBlur={() => handleBlur('nome')}
                  placeholder="Nome completo"
                  className={touched.nome && errors.nome ? 'border-destructive' : ''}
                />
                {touched.nome && errors.nome && (
                  <p className="text-sm text-destructive mt-1">{errors.nome}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  onBlur={() => handleBlur('email')}
                  placeholder="email@exemplo.com"
                  className={touched.email && errors.email ? 'border-destructive' : ''}
                />
                {touched.email && errors.email && (
                  <p className="text-sm text-destructive mt-1">{errors.email}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="telefone">Telefone</Label>
                <Input
                  id="telefone"
                  name="telefone"
                  value={formData.telefone}
                  onChange={(e) => handlePhoneChange('telefone', e.target.value)}
                  onBlur={() => handleBlur('telefone')}
                  placeholder="11999998888 (apenas números)"
                  className={touched.telefone && errors.telefone ? 'border-destructive' : ''}
                />
                {touched.telefone && errors.telefone && (
                  <p className="text-sm text-destructive mt-1">{errors.telefone}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="whatsApp">WhatsApp</Label>
                <Input
                  id="whatsApp"
                  name="whatsApp"
                  value={formData.whatsApp}
                  onChange={(e) => handlePhoneChange('whatsApp', e.target.value)}
                  onBlur={() => handleBlur('whatsApp')}
                  placeholder="11999998888 (apenas números)"
                  className={touched.whatsApp && errors.whatsApp ? 'border-destructive' : ''}
                />
                {touched.whatsApp && errors.whatsApp && (
                  <p className="text-sm text-destructive mt-1">{errors.whatsApp}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="dataNascimento">Data de Nascimento</Label>
                <Input
                  id="dataNascimento"
                  name="dataNascimento"
                  type="date"
                  value={formData.dataNascimento}
                  onChange={handleChange}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="tipoPessoa">Tipo de Pessoa</Label>
                <Select
                  value={formData.tipoPessoa}
                  onValueChange={(value) => setFormData(prev => ({ ...prev, tipoPessoa: value }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Adulto">Adulto</SelectItem>
                    <SelectItem value="Crianca">Criança</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2 flex items-center space-x-2">
                <Switch
                  id="ativo"
                  checked={formData.ativo}
                  onCheckedChange={(checked) => 
                    setFormData(prev => ({ ...prev, ativo: checked }))
                  }
                />
                <Label htmlFor="ativo" className="cursor-pointer">
                  Pessoa ativa
                </Label>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" />
                {loading ? t('actions.saving') : (isEditing ? t('people.update') : t('people.create'))}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/pessoas">{t('actions.cancel')}</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}




