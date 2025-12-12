import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { inscricoesEventosApi } from '@/lib/api';

const STATUS_OPTIONS = [
  { value: 1, label: 'Pendente' },
  { value: 2, label: 'Confirmada' },
  { value: 3, label: 'Cancelada' },
  { value: 4, label: 'Presente' },
];

export default function InscricaoEventoForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState({
    status: 1,
    quantidadeAcompanhantes: 0,
    observacoes: '',
    observacoesInternas: '',
  });
  const [inscricao, setInscricao] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = async () => {
    if (!isEditing) return;
    try {
      setLoading(true);
      setError(null);
      const res = await inscricoesEventosApi.getById(id);
      const i = res.data;
      setInscricao(i);
      setFormData({
        status: i.status || 1,
        quantidadeAcompanhantes: i.quantidadeAcompanhantes || 0,
        observacoes: i.observacoes || '',
        observacoesInternas: i.observacoesInternas || '',
      });
    } catch (err) {
      setError('Erro ao carregar inscrição');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: name === 'quantidadeAcompanhantes' || name === 'status' ? Number(value) : value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const payload = {
        status: formData.status,
        quantidadeAcompanhantes: formData.quantidadeAcompanhantes || 0,
        observacoes: formData.observacoes.trim() || null,
        observacoesInternas: formData.observacoesInternas.trim() || null,
      };
      await inscricoesEventosApi.update(id, payload);
      navigate(`/inscricoes-eventos/${id}`);
    } catch (err) {
      alert('Erro ao salvar inscrição');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing && !inscricao) return <LoadingPage text="Carregando inscrição..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (isEditing && !inscricao) return <div>Inscrição não encontrada</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button variant="ghost" asChild>
          <Link to={isEditing ? `/inscricoes-eventos/${id}` : '/inscricoes-eventos'}>
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{isEditing ? 'Editar Inscrição' : 'Nova Inscrição'}</h1>
          <p className="text-muted-foreground">{isEditing ? 'Atualize as informações da inscrição' : 'Cadastre uma nova inscrição'}</p>
        </div>
      </div>

      {isEditing && inscricao && (
        <Card>
          <CardHeader>
            <CardTitle>Informações do Participante</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label className="text-sm font-medium text-muted-foreground">Nome</label>
                <p className="text-base">{inscricao.nome}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">WhatsApp</label>
                <p className="text-base">{inscricao.whatsApp}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Email</label>
                <p className="text-base">{inscricao.email || '-'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-muted-foreground">Evento</label>
                <p className="text-base">{inscricao.eventoTitulo}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>{isEditing ? 'Editar Inscrição' : 'Cadastrar Inscrição'}</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="status">Status *</Label>
                <select id="status" name="status" value={formData.status} onChange={handleChange} className="w-full px-3 py-2 border rounded" required>
                  {STATUS_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="quantidadeAcompanhantes">Quantidade de Acompanhantes</Label>
                <Input
                  id="quantidadeAcompanhantes"
                  name="quantidadeAcompanhantes"
                  type="number"
                  min="0"
                  value={formData.quantidadeAcompanhantes}
                  onChange={handleChange}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="observacoes">Observações do Participante</Label>
              <Textarea
                id="observacoes"
                name="observacoes"
                value={formData.observacoes}
                onChange={handleChange}
                placeholder="Observações do participante"
                rows={3}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="observacoesInternas">Observações Internas</Label>
              <Textarea
                id="observacoesInternas"
                name="observacoesInternas"
                value={formData.observacoesInternas}
                onChange={handleChange}
                placeholder="Observações internas (apenas para administradores)"
                rows={3}
              />
            </div>

            <div className="flex items-center space-x-4">
              <Button type="submit" disabled={loading}>
                <Save className="h-4 w-4 mr-2" /> {loading ? 'Salvando...' : 'Salvar'}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to={isEditing ? `/inscricoes-eventos/${id}` : '/inscricoes-eventos'}>Cancelar</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}







