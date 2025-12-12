import { useEffect, useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { authApi } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { toast } from 'sonner';
import { User, Mail, Shield, Calendar, Clock, Lock } from 'lucide-react';

const TIPO_USUARIO_LABELS = {
  1: 'Administrador',
  2: 'Portal',
  3: 'Ambos',
};

const TIPO_USUARIO_COLORS = {
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-purple-100 text-purple-800',
};

export default function Perfil() {
  const { usuario: usuarioContext, atualizarUsuario } = useAuth();
  const [usuario, setUsuario] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [senhaData, setSenhaData] = useState({
    senhaAtual: '',
    novaSenha: '',
    confirmarSenha: '',
  });
  const [alterandoSenha, setAlterandoSenha] = useState(false);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await authApi.me();
      setUsuario(res.data);
      atualizarUsuario(res.data);
    } catch (err) {
      setError('Erro ao carregar dados do perfil');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleAlterarSenha = async (e) => {
    e.preventDefault();

    if (!senhaData.senhaAtual || !senhaData.novaSenha) {
      toast.error('Preencha todos os campos');
      return;
    }

    if (senhaData.novaSenha.length < 6) {
      toast.error('A nova senha deve ter no mínimo 6 caracteres');
      return;
    }

    if (senhaData.novaSenha !== senhaData.confirmarSenha) {
      toast.error('As senhas não coincidem');
      return;
    }

    try {
      setAlterandoSenha(true);
      await authApi.alterarSenha({
        senhaAtual: senhaData.senhaAtual,
        novaSenha: senhaData.novaSenha,
      });
      toast.success('Senha alterada com sucesso!');
      setSenhaData({
        senhaAtual: '',
        novaSenha: '',
        confirmarSenha: '',
      });
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Erro ao alterar senha';
      toast.error(errorMessage);
    } finally {
      setAlterandoSenha(false);
    }
  };

  if (loading) return <LoadingPage text="Carregando perfil..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!usuario) return <div>Usuário não encontrado</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Meu Perfil</h1>
        <p className="text-muted-foreground">Gerencie suas informações pessoais</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Dados Pessoais
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <User className="h-4 w-4" />
                Nome
              </label>
              <p className="text-base font-medium mt-1">{usuario.nome}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Mail className="h-4 w-4" />
                Email
              </label>
              <p className="text-base mt-1">{usuario.email}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Shield className="h-4 w-4" />
                Tipo de Usuário
              </label>
              <div className="mt-1">
                <span className={`px-3 py-1 rounded text-sm font-medium ${TIPO_USUARIO_COLORS[usuario.tipoUsuario] || 'bg-gray-100 text-gray-800'}`}>
                  {TIPO_USUARIO_LABELS[usuario.tipoUsuario] || usuario.tipoUsuarioDescricao}
                </span>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Data de Criação
              </label>
              <p className="text-base mt-1">
                {usuario.dataCriacao ? new Date(usuario.dataCriacao).toLocaleString('pt-BR') : '-'}
              </p>
            </div>
            {usuario.ultimoAcesso && (
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                  <Clock className="h-4 w-4" />
                  Último Acesso
                </label>
                <p className="text-base mt-1">
                  {new Date(usuario.ultimoAcesso).toLocaleString('pt-BR')}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Lock className="h-5 w-5" />
              Alterar Senha
            </CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleAlterarSenha} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="senhaAtual">Senha Atual *</Label>
                <Input
                  id="senhaAtual"
                  name="senhaAtual"
                  type="password"
                  value={senhaData.senhaAtual}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, senhaAtual: e.target.value }))}
                  placeholder="Digite sua senha atual"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="novaSenha">Nova Senha *</Label>
                <Input
                  id="novaSenha"
                  name="novaSenha"
                  type="password"
                  value={senhaData.novaSenha}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, novaSenha: e.target.value }))}
                  placeholder="Mínimo 6 caracteres"
                  required
                  minLength={6}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="confirmarSenha">Confirmar Nova Senha *</Label>
                <Input
                  id="confirmarSenha"
                  name="confirmarSenha"
                  type="password"
                  value={senhaData.confirmarSenha}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, confirmarSenha: e.target.value }))}
                  placeholder="Confirme a nova senha"
                  required
                />
              </div>
              <Button type="submit" disabled={alterandoSenha}>
                {alterandoSenha ? 'Alterando...' : 'Alterar Senha'}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}





