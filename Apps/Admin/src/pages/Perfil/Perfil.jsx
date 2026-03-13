import { useEffect, useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { useTheme } from '@/context/ThemeContext';
import { authApi } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { toast } from 'sonner';
import { User, Mail, Shield, Calendar, Clock, Lock, Palette, Sun, Moon } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const TIPO_USUARIO_COLORS = {
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-purple-100 text-purple-800',
};

const TIPO_USUARIO_KEYS = { 1: 'userTypeAdmin', 2: 'userTypePortal', 3: 'userTypeBoth' };

export default function Perfil() {
  const { usuario: usuarioContext, atualizarUsuario } = useAuth();
  const { theme, setTheme } = useTheme();
  const { t } = useTranslation();
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
      setError(t('profile.errorLoad'));
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
      toast.error(t('profile.fillAllFields'));
      return;
    }

    if (senhaData.novaSenha.length < 6) {
      toast.error(t('profile.minPasswordLength'));
      return;
    }

    if (senhaData.novaSenha !== senhaData.confirmarSenha) {
      toast.error(t('profile.passwordsDontMatch'));
      return;
    }

    try {
      setAlterandoSenha(true);
      await authApi.alterarSenha({
        senhaAtual: senhaData.senhaAtual,
        novaSenha: senhaData.novaSenha,
      });
      toast.success(t('profile.passwordChangeSuccess'));
      setSenhaData({
        senhaAtual: '',
        novaSenha: '',
        confirmarSenha: '',
      });
    } catch (err) {
      const errorMessage = err.response?.data?.message || t('profile.passwordChangeError');
      toast.error(errorMessage);
    } finally {
      setAlterandoSenha(false);
    }
  };

  if (loading) return <LoadingPage text={t('profile.loading')} />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!usuario) return <div>{t('profile.userNotFound')}</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('profile.title')}</h1>
        <p className="text-muted-foreground">{t('profile.subtitle')}</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Palette className="h-5 w-5" />
              {t('profile.preferences')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground mb-2 block">
                {t('profile.theme')}
              </label>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant={theme === 'light' ? 'default' : 'outline'}
                  onClick={() => setTheme('light')}
                  className="flex-1"
                >
                  <Sun className="h-4 w-4 mr-2" />
                  {t('profile.light')}
                </Button>
                <Button
                  type="button"
                  variant={theme === 'dark' ? 'default' : 'outline'}
                  onClick={() => setTheme('dark')}
                  className="flex-1"
                >
                  <Moon className="h-4 w-4 mr-2" />
                  {t('profile.dark')}
                </Button>
              </div>
              <p className="text-xs text-muted-foreground mt-2">
                {t('profile.themeHint')}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              {t('profile.personalData')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <User className="h-4 w-4" />
                {t('profile.name')}
              </label>
              <p className="text-base font-medium mt-1">{usuario.nome}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Mail className="h-4 w-4" />
                {t('profile.email')}
              </label>
              <p className="text-base mt-1">{usuario.email}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Shield className="h-4 w-4" />
                {t('profile.userType')}
              </label>
              <div className="mt-1">
                <span className={`px-3 py-1 rounded text-sm font-medium ${TIPO_USUARIO_COLORS[usuario.tipoUsuario] || 'bg-gray-100 text-gray-800'}`}>
                  {(TIPO_USUARIO_KEYS[usuario.tipoUsuario] && t('profile.' + TIPO_USUARIO_KEYS[usuario.tipoUsuario])) || usuario.tipoUsuarioDescricao}
                </span>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                {t('profile.createdAt')}
              </label>
              <p className="text-base mt-1">
                {usuario.dataCriacao ? new Date(usuario.dataCriacao).toLocaleString('pt-BR') : '-'}
              </p>
            </div>
            {usuario.ultimoAcesso && (
              <div>
                <label className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                  <Clock className="h-4 w-4" />
                  {t('profile.lastAccess')}
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
              {t('profile.changePassword')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleAlterarSenha} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="senhaAtual">{t('profile.currentPassword')}</Label>
                <Input
                  id="senhaAtual"
                  name="senhaAtual"
                  type="password"
                  value={senhaData.senhaAtual}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, senhaAtual: e.target.value }))}
                  placeholder={t('profile.currentPasswordPlaceholder')}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="novaSenha">{t('profile.newPassword')}</Label>
                <Input
                  id="novaSenha"
                  name="novaSenha"
                  type="password"
                  value={senhaData.novaSenha}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, novaSenha: e.target.value }))}
                  placeholder={t('profile.newPasswordPlaceholder')}
                  required
                  minLength={6}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="confirmarSenha">{t('profile.confirmPassword')}</Label>
                <Input
                  id="confirmarSenha"
                  name="confirmarSenha"
                  type="password"
                  value={senhaData.confirmarSenha}
                  onChange={(e) => setSenhaData((prev) => ({ ...prev, confirmarSenha: e.target.value }))}
                  placeholder={t('profile.confirmPasswordPlaceholder')}
                  required
                />
              </div>
              <Button type="submit" disabled={alterandoSenha}>
                {alterandoSenha ? t('profile.changing') : t('profile.changePasswordButton')}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}







