import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { perfisAcessoApi } from '@/lib/api';
import { RESOURCES } from '@/utils/permissions';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const RESOURCE_LABELS = {
  [RESOURCES.DASHBOARD]: 'Dashboard',
  [RESOURCES.USUARIOS]: 'Usuários',
  [RESOURCES.PERFIS_ACESSO]: 'Perfis de Acesso',
  [RESOURCES.PESSOAS]: 'Pessoas',
  [RESOURCES.PERFIS]: 'Perfis',
  [RESOURCES.VISITANTES]: 'Visitantes',
  [RESOURCES.CONFIG_MENSAGENS]: 'Configurações de Mensagens',
  [RESOURCES.MENSAGENS_AGENDADAS]: 'Mensagens Agendadas',
  [RESOURCES.EQUIPES]: 'Equipes',
  [RESOURCES.CARGOS]: 'Cargos',
  [RESOURCES.VOLUNTARIOS]: 'Voluntários',
  [RESOURCES.EVENTOS]: 'Eventos',
  [RESOURCES.INSCRICOES_EVENTOS]: 'Inscrições em Eventos',
  [RESOURCES.PORTAL]: 'Portal',
  [RESOURCES.CATEGORIAS_NOTICIAS]: 'Categorias de Notícias',
  [RESOURCES.NOTICIAS]: 'Notícias',
  [RESOURCES.CONTATOS]: 'Contatos',
  [RESOURCES.DESTAQUES_SITE]: 'Destaques do Site',
  [RESOURCES.MIDIA]: 'Categorias de Mídia',
  [RESOURCES.GALERIAS_FOTOS]: 'Galerias de Fotos',
  [RESOURCES.ENQUETES]: 'Enquetes',
  [RESOURCES.KIDS]: 'Kids',
  [RESOURCES.HUB]: 'Hub',
  [RESOURCES.FINANCEIRO]: 'Financeiro',
  [RESOURCES.FORNECEDORES]: 'Fornecedores',
};

const RESOURCE_LIST = Object.values(RESOURCES);

const emptyPermissions = RESOURCE_LIST.map((recurso) => ({
  recurso,
  podeVer: false,
  podeEditar: false,
  podeExcluir: false,
}));

export default function PerfilAcessoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [formData, setFormData] = useState({
    nome: '',
    descricao: '',
    permissoes: emptyPermissions,
  });

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await perfisAcessoApi.getById(id);
      const p = res.data || {};

      const perms = emptyPermissions.map((perm) => {
        const match = (p.permissoes || []).find((x) => String(x.recurso).toLowerCase() === String(perm.recurso).toLowerCase());
        return {
          recurso: perm.recurso,
          podeVer: !!match?.podeVer,
          podeEditar: !!match?.podeEditar,
          podeExcluir: !!match?.podeExcluir,
        };
      });

      setFormData({
        nome: p.nome || '',
        descricao: p.descricao || '',
        permissoes: perms,
      });
    } catch (err) {
      setError('Erro ao carregar perfil');
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

  const updatePermission = (index, field, value) => {
    setFormData((prev) => {
      const updated = [...prev.permissoes];
      updated[index] = { ...updated[index], [field]: value };

      if (field === 'podeEditar' && value) {
        updated[index].podeVer = true;
      }
      if (field === 'podeExcluir' && value) {
        updated[index].podeVer = true;
      }

      return { ...prev, permissoes: updated };
    });
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
        descricao: formData.descricao?.trim() || null,
        permissoes: formData.permissoes,
      };

      if (isEditing) await perfisAcessoApi.update(id, payload);
      else await perfisAcessoApi.create(payload);
      toast.success(isEditing ? 'Perfil atualizado com sucesso' : 'Perfil criado com sucesso');
      navigate('/perfis-acesso');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar perfil'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando perfil..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/perfis-acesso">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Perfil' : 'Novo Perfil'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as permissões do perfil' : 'Cadastre um novo perfil de acesso'}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados do Perfil</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome do perfil" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="descricao">Descrição</Label>
                <Input id="descricao" name="descricao" value={formData.descricao} onChange={handleChange} placeholder="Descrição do perfil" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Permissões por Seção</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4">
              {formData.permissoes.map((perm, index) => (
                <div key={perm.recurso} className="flex flex-col gap-2 border rounded p-3">
                  <div className="font-medium">{RESOURCE_LABELS[perm.recurso] || perm.recurso}</div>
                  <div className="flex flex-wrap items-center gap-4">
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={perm.podeVer}
                        onChange={(e) => updatePermission(index, 'podeVer', e.target.checked)}
                      />
                      Ver
                    </label>
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={perm.podeEditar}
                        onChange={(e) => updatePermission(index, 'podeEditar', e.target.checked)}
                      />
                      Editar/Inserir
                    </label>
                    <label className="flex items-center gap-2 text-sm">
                      <input
                        type="checkbox"
                        checked={perm.podeExcluir}
                        onChange={(e) => updatePermission(index, 'podeExcluir', e.target.checked)}
                      />
                      Excluir
                    </label>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center space-x-4">
          <Button type="submit" disabled={loading}>
            <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : (isEditing ? 'Atualizar' : 'Cadastrar')}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/perfis-acesso">Cancelar</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
