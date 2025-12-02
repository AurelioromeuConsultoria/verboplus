import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Users, CheckCircle, Clock, XCircle, UserCheck, Eye, Edit, Trash2, Phone, Mail, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { inscricoesEventosApi, eventosApi } from '@/lib/api';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import InscricaoEventoPublicForm from '@/components/InscricaoEvento/InscricaoEventoPublicForm';

const STATUS_LABELS = {
  1: 'Pendente',
  2: 'Confirmada',
  3: 'Cancelada',
  4: 'Presente',
};

const STATUS_COLORS = {
  1: 'bg-yellow-100 text-yellow-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-blue-100 text-blue-800',
};

export default function EventoInscricoes() {
  const { eventoId } = useParams();
  const [inscricoes, setInscricoes] = useState([]);
  const [evento, setEvento] = useState(null);
  const [estatisticas, setEstatisticas] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [inscricoesRes, eventoRes, statsRes] = await Promise.all([
        inscricoesEventosApi.getByEvento(eventoId),
        eventosApi.getById(eventoId),
        inscricoesEventosApi.getEstatisticas(eventoId),
      ]);
      setInscricoes(inscricoesRes.data || []);
      setEvento(eventoRes.data);
      setEstatisticas(statsRes.data);
    } catch (err) {
      setError('Erro ao carregar dados');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [eventoId]);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta inscrição?')) return;
    try {
      await inscricoesEventosApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir inscrição');
      console.error(err);
    }
  };

  const handleConfirmar = async (id) => {
    try {
      await inscricoesEventosApi.confirmar(id);
      await load();
    } catch (err) {
      alert('Erro ao confirmar inscrição');
      console.error(err);
    }
  };

  const handleCancelar = async (id) => {
    if (!confirm('Tem certeza que deseja cancelar esta inscrição?')) return;
    try {
      await inscricoesEventosApi.cancelar(id);
      await load();
    } catch (err) {
      alert('Erro ao cancelar inscrição');
      console.error(err);
    }
  };

  const handleMarcaPresente = async (id) => {
    try {
      await inscricoesEventosApi.update(id, { status: 4 });
      await load();
    } catch (err) {
      alert('Erro ao marcar presença');
      console.error(err);
    }
  };

  const filtered = inscricoes.filter((i) => {
    if (statusFilter && String(i.status) !== statusFilter) return false;
    return true;
  });

  const podeInscrever = evento && new Date(evento.dataInicio) > new Date();

  if (loading) return <LoadingPage text="Carregando inscrições..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" asChild>
            <Link to="/eventos">
              <ArrowLeft className="h-4 w-4 mr-2" /> Voltar para Eventos
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Inscrições - {evento?.titulo || 'Evento'}</h1>
            <p className="text-muted-foreground">Gerencie as inscrições deste evento</p>
          </div>
        </div>
        {podeInscrever && (
          <Button onClick={() => setShowForm(true)}>
            <Plus className="h-4 w-4 mr-2" /> Nova Inscrição
          </Button>
        )}
      </div>

      {estatisticas && (
        <div className="grid gap-4 md:grid-cols-5">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <Users className="h-4 w-4 text-muted-foreground" />
                <div>
                  <p className="text-sm text-muted-foreground">Total</p>
                  <p className="text-2xl font-bold">{estatisticas.totalInscricoes}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <CheckCircle className="h-4 w-4 text-green-600" />
                <div>
                  <p className="text-sm text-muted-foreground">Confirmadas</p>
                  <p className="text-2xl font-bold">{estatisticas.inscricoesConfirmadas}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <Clock className="h-4 w-4 text-yellow-600" />
                <div>
                  <p className="text-sm text-muted-foreground">Pendentes</p>
                  <p className="text-2xl font-bold">{estatisticas.inscricoesPendentes}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <XCircle className="h-4 w-4 text-red-600" />
                <div>
                  <p className="text-sm text-muted-foreground">Canceladas</p>
                  <p className="text-2xl font-bold">{estatisticas.inscricoesCanceladas}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <Users className="h-4 w-4 text-blue-600" />
                <div>
                  <p className="text-sm text-muted-foreground">Participantes</p>
                  <p className="text-2xl font-bold">{estatisticas.totalParticipantes}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Lista de Inscrições ({filtered.length})</CardTitle>
            <div className="space-y-2">
              <label className="text-sm font-medium">Filtrar por Status</label>
              <select className="px-3 py-2 border rounded" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">Todos</option>
                <option value="1">Pendente</option>
                <option value="2">Confirmada</option>
                <option value="3">Cancelada</option>
                <option value="4">Presente</option>
              </select>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma inscrição encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>WhatsApp</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Acompanhantes</TableHead>
                  <TableHead>Data Inscrição</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((inscricao) => (
                  <TableRow key={inscricao.id}>
                    <TableCell className="font-medium">{inscricao.nome}</TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{inscricao.whatsApp}</span>
                        {inscricao.whatsApp && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`https://wa.me/55${inscricao.whatsApp.replace(/\D/g, '')}`)}
                          >
                            <Phone className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{inscricao.email || '-'}</span>
                        {inscricao.email && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`mailto:${inscricao.email}`)}
                          >
                            <Mail className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs font-medium ${STATUS_COLORS[inscricao.status] || 'bg-gray-100 text-gray-800'}`}>
                        {STATUS_LABELS[inscricao.status] || inscricao.statusDescricao}
                      </span>
                    </TableCell>
                    <TableCell>{inscricao.quantidadeAcompanhantes || 0}</TableCell>
                    <TableCell>{inscricao.dataInscricao ? new Date(inscricao.dataInscricao).toLocaleDateString('pt-BR') : '-'}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-1">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/inscricoes-eventos/${inscricao.id}`}>
                            <Eye className="h-4 w-4" />
                          </Link>
                        </Button>
                        {inscricao.status === 1 && (
                          <Button variant="ghost" size="sm" onClick={() => handleConfirmar(inscricao.id)} title="Confirmar">
                            <CheckCircle className="h-4 w-4 text-green-600" />
                          </Button>
                        )}
                        {(inscricao.status === 1 || inscricao.status === 2) && (
                          <Button variant="ghost" size="sm" onClick={() => handleCancelar(inscricao.id)} title="Cancelar">
                            <XCircle className="h-4 w-4 text-red-600" />
                          </Button>
                        )}
                        {inscricao.status === 2 && (
                          <Button variant="ghost" size="sm" onClick={() => handleMarcaPresente(inscricao.id)} title="Marcar Presença">
                            <UserCheck className="h-4 w-4 text-blue-600" />
                          </Button>
                        )}
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/inscricoes-eventos/${inscricao.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(inscricao.id)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={showForm} onOpenChange={setShowForm}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Nova Inscrição</DialogTitle>
          </DialogHeader>
          <InscricaoEventoPublicForm
            eventoId={eventoId}
            onSuccess={() => {
              setShowForm(false);
              load();
            }}
            onCancel={() => setShowForm(false)}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}



