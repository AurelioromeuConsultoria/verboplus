import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { equipesApi, voluntariosApi, pessoasApi, cargosApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';
import { useTranslation } from 'react-i18next';

export default function EquipeForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const { t } = useTranslation();

  const [formData, setFormData] = useState({
    nome: '',
    area: '1',
  });
  const [voluntarios, setVoluntarios] = useState([]);
  const [pessoas, setPessoas] = useState([]);
  const [cargos, setCargos] = useState([]);
  const [vinculoPessoaId, setVinculoPessoaId] = useState('');
  const [vinculoCargoId, setVinculoCargoId] = useState('');
  const [loading, setLoading] = useState(false);
  const [loadingVinculo, setLoadingVinculo] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      if (isEditing) {
        const [resEquipe, resPessoas, resCargos, resVol] = await Promise.all([
          equipesApi.getById(id),
          pessoasApi.getAll(),
          cargosApi.getAll(),
          voluntariosApi.getByEquipe(id),
        ]);
        const e = resEquipe.data;
        setFormData({ nome: e.nome || '', area: String(e.area || '1') });
        setPessoas(resPessoas.data || []);
        setCargos(resCargos.data || []);
        setVoluntarios(resVol.data || []);
      } else {
        setPessoas([]);
        setCargos([]);
      }
    } catch (err) {
      setError('Erro ao carregar equipe');
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

  const handleVincular = async () => {
    if (!vinculoPessoaId || !vinculoCargoId) {
      toast.error('Selecione Pessoa e Cargo');
      return;
    }
    try {
      setLoadingVinculo(true);
      await voluntariosApi.create({
        pessoaId: Number(vinculoPessoaId),
        equipeId: Number(id),
        cargoId: Number(vinculoCargoId),
      });
      toast.success('Voluntário vinculado');
      setVinculoPessoaId('');
      setVinculoCargoId('');
      const res = await voluntariosApi.getByEquipe(id);
      setVoluntarios(res.data || []);
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao vincular'));
    } finally {
      setLoadingVinculo(false);
    }
  };

  const handleRemoverVinculo = async (voluntarioId) => {
    if (!confirm('Remover este voluntário da equipe?')) return;
    try {
      await voluntariosApi.delete(voluntarioId);
      toast.success('Voluntário removido');
      const res = await voluntariosApi.getByEquipe(id);
      setVoluntarios(res.data || []);
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao remover'));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.nome.trim()) {
      toast.error('Nome é obrigatório');
      return;
    }
    if (!['1', '2', '3'].includes(formData.area)) {
      toast.error('Área inválida');
      return;
    }
    try {
      setLoading(true);
      const payload = { nome: formData.nome.trim(), area: Number(formData.area) };
      if (isEditing) await equipesApi.update(id, payload);
      else await equipesApi.create(payload);
      toast.success(isEditing ? 'Equipe atualizada com sucesso' : 'Equipe criada com sucesso');
      navigate('/equipes');
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao salvar equipe'));
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) return <LoadingPage text="Carregando equipe..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to="/equipes">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? t('volunteer.teams.edit') : t('volunteer.teams.new')}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da equipe' : 'Cadastre uma nova equipe'}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? t('volunteer.teams.edit') : t('volunteer.teams.create')}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="nome">Nome *</Label>
                <Input id="nome" name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome da equipe" required />
              </div>

              <div className="space-y-2">
                <Label htmlFor="area">Área *</Label>
                <select id="area" name="area" value={formData.area} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  <option value="1">Verde</option>
                  <option value="2">Vermelha</option>
                  <option value="3">Laranja</option>
                </select>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? t('actions.saving') : (isEditing ? t('volunteer.teams.update') : t('volunteer.teams.create'))}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to="/equipes">{t('actions.cancel')}</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {isEditing && (
        <Card>
          <CardHeader>
            <CardTitle>Voluntários desta equipe</CardTitle>
          </CardHeader>
          <CardContent>
              <div className="space-y-4">
                <div className="flex flex-wrap gap-2 items-end">
                  <div className="space-y-1 min-w-[200px]">
                    <Label htmlFor="vinculoPessoa">Pessoa</Label>
                    <select
                      id="vinculoPessoa"
                      value={vinculoPessoaId}
                      onChange={(e) => setVinculoPessoaId(e.target.value)}
                      className="w-full px-3 py-2 border rounded"
                    >
                      <option value="">Selecione</option>
                      {pessoas.map((p) => (
                        <option key={p.id} value={p.id}>{p.nome}{p.email ? ` — ${p.email}` : ''}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-1 min-w-[160px]">
                    <Label htmlFor="vinculoCargo">Cargo</Label>
                    <select
                      id="vinculoCargo"
                      value={vinculoCargoId}
                      onChange={(e) => setVinculoCargoId(e.target.value)}
                      className="w-full px-3 py-2 border rounded"
                    >
                      <option value="">Selecione</option>
                      {cargos.map((c) => (
                        <option key={c.id} value={c.id}>{c.nome}</option>
                      ))}
                    </select>
                  </div>
                  <Button type="button" onClick={handleVincular} disabled={loadingVinculo}>
                    <Plus className="h-4 w-4 mr-2" /> Vincular
                  </Button>
                </div>
                {voluntarios.length > 0 ? (
                  <div className="border rounded overflow-hidden">
                    <table className="w-full text-sm">
                      <thead className="bg-muted/50">
                        <tr>
                          <th className="text-left p-2">Nome</th>
                          <th className="text-left p-2">Cargo</th>
                          <th className="w-20 p-2"></th>
                        </tr>
                      </thead>
                      <tbody>
                        {voluntarios.map((v) => (
                          <tr key={v.id} className="border-t">
                            <td className="p-2">{v.nome}</td>
                            <td className="p-2">{v.nomeCargo}</td>
                            <td className="p-2">
                              <Button type="button" variant="ghost" size="sm" onClick={() => handleRemoverVinculo(v.id)}>
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-muted-foreground text-sm">Nenhum voluntário vinculado ainda.</p>
                )}
              </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}


