import { useEffect, useState } from 'react';
import { X, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { usuariosApi } from '@/lib/api';
import { toast } from 'sonner';

const TIPO_USUARIO_OPTIONS = [
  { value: 1, label: 'Administrador' },
  { value: 2, label: 'Portal' },
  { value: 3, label: 'Ambos' },
];

export default function UsuarioForm({ id, onClose, onSuccess }) {
  const isEditing = Boolean(id);
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    senha: '',
    confirmarSenha: '',
    tipoUsuario: 1,
    ativo: true,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await usuariosApi.getById(id);
      const u = res.data;
      setFormData({
        nome: u.nome || '',
        email: u.email || '',
        senha: '',
        confirmarSenha: '',
        tipoUsuario: u.tipoUsuario || 1,
        ativo: u.ativo !== undefined ? u.ativo : true,
      });
    } catch (err) {
      setError('Erro ao carregar usuário');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : name === 'tipoUsuario' ? Number(value) : value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.nome.trim()) {
      toast.error('Nome é obrigatório');
      return;
    }

    if (!formData.email.trim()) {
      toast.error('Email é obrigatório');
      return;
    }

    if (!/.+@.+\..+/.test(formData.email)) {
      toast.error('Email inválido');
      return;
    }

    if (!isEditing && !formData.senha) {
      toast.error('Senha é obrigatória');
      return;
    }

    if (!isEditing && formData.senha.length < 6) {
      toast.error('Senha deve ter no mínimo 6 caracteres');
      return;
    }

    if (!isEditing && formData.senha !== formData.confirmarSenha) {
      toast.error('As senhas não coincidem');
      return;
    }

    try {
      setLoading(true);
      if (isEditing) {
        await usuariosApi.update(id, {
          nome: formData.nome.trim(),
          email: formData.email.trim(),
          tipoUsuario: formData.tipoUsuario,
          ativo: formData.ativo,
        });
        toast.success('Usuário atualizado com sucesso');
      } else {
        await usuariosApi.create({
          nome: formData.nome.trim(),
          email: formData.email.trim(),
          senha: formData.senha,
          tipoUsuario: formData.tipoUsuario,
        });
        toast.success('Usuário criado com sucesso');
      }
      if (onSuccess) onSuccess();
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao salvar usuário';
      toast.error(errorMessage);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing && !formData.nome) return <LoadingPage text="Carregando usuário..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <Card className="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{isEditing ? 'Editar Usuário' : 'Novo Usuário'}</CardTitle>
          <Button variant="ghost" size="sm" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
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
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email *</Label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="email@exemplo.com"
                  required
                />
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="tipoUsuario">Tipo de Usuário *</Label>
                <select
                  id="tipoUsuario"
                  name="tipoUsuario"
                  value={formData.tipoUsuario}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border rounded"
                  required
                >
                  {TIPO_USUARIO_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
              </div>
              {isEditing && (
                <div className="space-y-2 flex items-center space-x-3 pt-6">
                  <input
                    type="checkbox"
                    id="ativo"
                    name="ativo"
                    checked={formData.ativo}
                    onChange={handleChange}
                    className="h-4 w-4"
                  />
                  <Label htmlFor="ativo" className="cursor-pointer">Usuário ativo</Label>
                </div>
              )}
            </div>

            {!isEditing && (
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="senha">Senha *</Label>
                  <Input
                    id="senha"
                    name="senha"
                    type="password"
                    value={formData.senha}
                    onChange={handleChange}
                    placeholder="Mínimo 6 caracteres"
                    required={!isEditing}
                    minLength={6}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="confirmarSenha">Confirmar Senha *</Label>
                  <Input
                    id="confirmarSenha"
                    name="confirmarSenha"
                    type="password"
                    value={formData.confirmarSenha}
                    onChange={handleChange}
                    placeholder="Confirme a senha"
                    required={!isEditing}
                  />
                </div>
              </div>
            )}

            <div className="flex items-center space-x-4 pt-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : 'Salvar'}
              </Button>
              <Button type="button" variant="outline" onClick={onClose}>
                Cancelar
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}





