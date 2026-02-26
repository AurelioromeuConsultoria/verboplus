import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { fornecedoresApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const EMAIL_REGEX = /.+@.+\..+/;

export default function FornecedorForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    razaoSocial: '',
    cnpjCpf: '',
    inscricaoEstadual: '',
    endereco: '',
    telefone: '',
    site: '',
    contatoNome: '',
    contatoCpf: '',
    contatoWhatsApp: '',
    contatoEmail: '',
  });

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await fornecedoresApi.getById(id);
      const f = res.data || {};
      setFormData({
        nome: f.nome || '',
        razaoSocial: f.razaoSocial || '',
        cnpjCpf: f.cnpjCpf || '',
        inscricaoEstadual: f.inscricaoEstadual || '',
        endereco: f.endereco || '',
        telefone: f.telefone || '',
        site: f.site || '',
        contatoNome: f.contatoNome || '',
        contatoCpf: f.contatoCpf || '',
        contatoWhatsApp: f.contatoWhatsApp || '',
        contatoEmail: f.contatoEmail || '',
      });
    } catch (err) {
      setError('Erro ao carregar fornecedor');
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
    if (formData.contatoEmail && !EMAIL_REGEX.test(formData.contatoEmail)) {
      toast.error('E-mail do responsável inválido');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        nome: formData.nome.trim(),
        razaoSocial: formData.razaoSocial?.trim() || null,
        cnpjCpf: formData.cnpjCpf?.trim() || null,
        inscricaoEstadual: formData.inscricaoEstadual?.trim() || null,
        endereco: formData.endereco?.trim() || null,
        telefone: formData.telefone?.trim() || null,
        site: formData.site?.trim() || null,
        contatoNome: formData.contatoNome?.trim() || null,
        contatoCpf: formData.contatoCpf?.trim() || null,
        contatoWhatsApp: formData.contatoWhatsApp?.trim() || null,
        contatoEmail: formData.contatoEmail?.trim() || null,
      };
      if (isEditing) await fornecedoresApi.update(id, payload);
      else await fornecedoresApi.create(payload);
      toast.success(isEditing ? 'Fornecedor atualizado com sucesso' : 'Fornecedor criado com sucesso');
      navigate('/financeiro/fornecedores');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar fornecedor'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando fornecedor..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/financeiro/fornecedores">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Fornecedor' : 'Novo Fornecedor'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações do fornecedor' : 'Cadastre um novo fornecedor'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Empresa</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome do fornecedor" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="razaoSocial">Razão Social</Label>
                <Input id="razaoSocial" name="razaoSocial" value={formData.razaoSocial} onChange={handleChange} placeholder="Razão social" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="cnpjCpf">CNPJ/CPF</Label>
                <Input id="cnpjCpf" name="cnpjCpf" value={formData.cnpjCpf} onChange={handleChange} placeholder="00.000.000/0000-00 ou 000.000.000-00" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="inscricaoEstadual">Inscrição Estadual</Label>
                <Input id="inscricaoEstadual" name="inscricaoEstadual" value={formData.inscricaoEstadual} onChange={handleChange} placeholder="Inscrição estadual" />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="endereco">Endereço</Label>
                <Input id="endereco" name="endereco" value={formData.endereco} onChange={handleChange} placeholder="Rua, número, bairro, cidade, estado" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="telefone">Telefone</Label>
                <Input id="telefone" name="telefone" value={formData.telefone} onChange={handleChange} placeholder="(11) 99999-9999" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="site">Site</Label>
                <Input id="site" name="site" value={formData.site} onChange={handleChange} placeholder="https://" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Dados do Contato Responsável</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="contatoNome">Nome</Label>
                <Input id="contatoNome" name="contatoNome" value={formData.contatoNome} onChange={handleChange} placeholder="Nome do responsável" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="contatoCpf">CPF</Label>
                <Input id="contatoCpf" name="contatoCpf" value={formData.contatoCpf} onChange={handleChange} placeholder="000.000.000-00" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="contatoWhatsApp">WhatsApp</Label>
                <Input id="contatoWhatsApp" name="contatoWhatsApp" value={formData.contatoWhatsApp} onChange={handleChange} placeholder="11999998888" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="contatoEmail">E-mail</Label>
                <Input id="contatoEmail" name="contatoEmail" type="email" value={formData.contatoEmail} onChange={handleChange} placeholder="email@exemplo.com" />
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/financeiro/fornecedores">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
