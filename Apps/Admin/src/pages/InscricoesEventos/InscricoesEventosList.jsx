import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Edit, Trash2, Filter, Eye, CheckCircle, XCircle, Phone, Mail, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { inscricoesEventosApi, eventosApi } from '@/lib/api';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

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

export default function InscricoesEventosList() {
  const [items, setItems] = useState([]);
  const [eventos, setEventos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [eventoFilter, setEventoFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [confirmState, setConfirmState] = useState({ open: false, action: null, id: null });
  const [confirmLoading, setConfirmLoading] = useState(false);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [inscricoesRes, eventosRes] = await Promise.all([
        inscricoesEventosApi.getAll(),
        eventosApi.getAll(),
      ]);
      setItems(inscricoesRes.data || []);
      setEventos(eventosRes.data || []);
    } catch (err) {
      setError('Erro ao carregar inscrições');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    setConfirmState({ open: true, action: 'delete', id });
  };

  const handleConfirmar = async (id) => {
    try {
      await inscricoesEventosApi.confirmar(id);
      toast.success('Inscrição confirmada!');
      await load();
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao confirmar inscrição'));
    }
  };

  const handleCancelar = async (id) => {
    setConfirmState({ open: true, action: 'cancel', id });
  };

  const runConfirmedAction = async () => {
    const { action, id } = confirmState;
    if (!action || !id) return;
    try {
      setConfirmLoading(true);
      if (action === 'delete') {
        await inscricoesEventosApi.delete(id);
        toast.success('Inscrição excluída!');
      } else if (action === 'cancel') {
        await inscricoesEventosApi.cancelar(id);
        toast.success('Inscrição cancelada!');
      }
      setConfirmState({ open: false, action: null, id: null });
      await load();
    } catch (err) {
      toast.error(
        getApiErrorMessage(
          err,
          action === 'delete' ? 'Erro ao excluir inscrição' : 'Erro ao cancelar inscrição'
        )
      );
    } finally {
      setConfirmLoading(false);
    }
  };

  const filtered = items.filter((i) => {
    if (busca && !i.nome?.toLowerCase().includes(busca.toLowerCase()) && !i.whatsApp?.includes(busca)) return false;
    if (eventoFilter && String(i.eventoId) !== eventoFilter) return false;
    if (statusFilter && String(i.status) !== statusFilter) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando inscrições..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Inscrições em Eventos</h1>
          <p className="text-muted-foreground">Gerencie as inscrições nos eventos</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Nome ou WhatsApp"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Evento</label>
              <select className="w-full px-3 py-2 border rounded" value={eventoFilter} onChange={(e) => setEventoFilter(e.target.value)}>
                <option value="">Todos</option>
                {eventos.map((e) => (
                  <option key={e.id} value={e.id}>{e.titulo}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Status</label>
              <select className="w-full px-3 py-2 border rounded" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">Todos</option>
                <option value="1">Pendente</option>
                <option value="2">Confirmada</option>
                <option value="3">Cancelada</option>
                <option value="4">Presente</option>
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Inscrições ({filtered.length})</CardTitle>
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
                  <TableHead>Evento</TableHead>
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
                    <TableCell>{inscricao.eventoTitulo || '-'}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs font-medium ${STATUS_COLORS[inscricao.status] || 'bg-gray-100 text-gray-800'}`}>
                        {STATUS_LABELS[inscricao.status] || inscricao.statusDescricao}
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Users className="h-4 w-4" />
                        {inscricao.quantidadeAcompanhantes || 0}
                      </div>
                    </TableCell>
                    <TableCell>{inscricao.dataInscricao ? new Date(inscricao.dataInscricao).toLocaleString('pt-BR') : '-'}</TableCell>
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

      <ConfirmDialog
        open={confirmState.open}
        onOpenChange={(open) => {
          if (!open) setConfirmState({ open: false, action: null, id: null });
          else setConfirmState((s) => ({ ...s, open: true }));
        }}
        onConfirm={runConfirmedAction}
        loading={confirmLoading}
        variant={confirmState.action === 'delete' ? 'destructive' : 'default'}
        title={confirmState.action === 'delete' ? 'Excluir inscrição?' : 'Cancelar inscrição?'}
        description={
          confirmState.action === 'delete'
            ? 'Essa ação não pode ser desfeita.'
            : 'A inscrição ficará com status cancelada.'
        }
        confirmText={confirmState.action === 'delete' ? 'Excluir' : 'Cancelar'}
      />
    </div>
  );
}







