import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { eventosApi } from '@/lib/api';

export default function EventosList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await eventosApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar eventos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir este evento?')) return;
    try {
      await eventosApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir evento');
      console.error(err);
    }
  };

  const filtered = items.filter((e) => {
    if (busca && !e.titulo?.toLowerCase().includes(busca.toLowerCase()) && !e.descricao?.toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando eventos..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Eventos</h1>
          <p className="text-muted-foreground">Gerencie os eventos da igreja</p>
        </div>
        <Button asChild>
          <Link to="/eventos/novo">
            <Plus className="h-4 w-4 mr-2" /> Novo Evento
          </Link>
        </Button>
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
                placeholder="Digite o título ou descrição"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Eventos</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum evento encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Título</TableHead>
                  <TableHead>Descrição</TableHead>
                  <TableHead>Data Início</TableHead>
                  <TableHead>Data Fim</TableHead>
                  <TableHead>URL</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((evento) => (
                  <TableRow key={evento.id}>
                    <TableCell className="font-medium">{evento.titulo || '-'}</TableCell>
                    <TableCell>{evento.descricao ? (evento.descricao.length > 50 ? `${evento.descricao.substring(0, 50)}...` : evento.descricao) : '-'}</TableCell>
                    <TableCell>{evento.dataInicio ? new Date(evento.dataInicio).toLocaleString('pt-BR') : '-'}</TableCell>
                    <TableCell>{evento.dataFim ? new Date(evento.dataFim).toLocaleString('pt-BR') : '-'}</TableCell>
                    <TableCell>
                      {evento.url ? (
                        <a href={evento.url} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                          {evento.url.length > 30 ? `${evento.url.substring(0, 30)}...` : evento.url}
                        </a>
                      ) : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild title="Ver Inscrições">
                          <Link to={`/eventos/${evento.id}/inscricoes`}>
                            <Users className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/eventos/${evento.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(evento.id)}>
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
    </div>
  );
}


