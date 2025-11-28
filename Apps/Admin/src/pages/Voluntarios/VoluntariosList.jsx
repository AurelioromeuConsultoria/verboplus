import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter, Phone, Mail } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { voluntariosApi, equipesApi, cargosApi } from '@/lib/api';

export default function VoluntariosList() {
  const [items, setItems] = useState([]);
  const [equipes, setEquipes] = useState([]);
  const [cargos, setCargos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [equipeId, setEquipeId] = useState('');
  const [cargoId, setCargoId] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [v, e, c] = await Promise.all([
        voluntariosApi.getAll(),
        equipesApi.getAll(),
        cargosApi.getAll(),
      ]);
      setItems(v.data || []);
      setEquipes(e.data || []);
      setCargos(c.data || []);
    } catch (err) {
      setError('Erro ao carregar voluntários');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir este voluntário?')) return;
    try {
      await voluntariosApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir voluntário');
      console.error(err);
    }
  };

  const filtered = items.filter((v) => {
    if (busca && !v.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    if (equipeId && String(v.equipeId) !== String(equipeId)) return false;
    if (cargoId && String(v.cargoId) !== String(cargoId)) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando voluntários..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Voluntários</h1>
          <p className="text-muted-foreground">Gerencie os voluntários</p>
        </div>
        <Button asChild>
          <Link to="/voluntarios/novo">
            <Plus className="h-4 w-4 mr-2" /> Novo Voluntário
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar por nome</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o nome"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Equipe</label>
              <select className="w-full px-3 py-2 border rounded" value={equipeId} onChange={(e) => setEquipeId(e.target.value)}>
                <option value="">Todas</option>
                {equipes.map((e) => (
                  <option key={e.id} value={e.id}>{e.nome}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Cargo</label>
              <select className="w-full px-3 py-2 border rounded" value={cargoId} onChange={(e) => setCargoId(e.target.value)}>
                <option value="">Todos</option>
                {cargos.map((c) => (
                  <option key={c.id} value={c.id}>{c.nome}</option>
                ))}
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Voluntários</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum voluntário encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>WhatsApp</TableHead>
                  <TableHead>Equipe</TableHead>
                  <TableHead>Cargo</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((vol) => (
                  <TableRow key={vol.id}>
                    <TableCell className="font-medium">{vol.nome}</TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{vol.whatsApp}</span>
                        {vol.whatsApp && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`https://wa.me/55${String(vol.whatsApp).replace(/\D/g, '')}`)}
                          >
                            <Phone className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{vol.nomeEquipe}</TableCell>
                    <TableCell>{vol.nomeCargo}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/voluntarios/${vol.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(vol.id)}>
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


