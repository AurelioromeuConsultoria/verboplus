import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { equipesApi } from '@/lib/api';

const AREA_LABEL = {
  1: 'Verde',
  2: 'Vermelha',
  3: 'Laranja',
};

export default function EquipesList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [area, setArea] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await equipesApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar equipes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta equipe?')) return;
    try {
      await equipesApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir. Existe(m) voluntário(s) vinculado(s).');
      console.error(err);
    }
  };

  const filtered = items.filter((e) => {
    if (busca && !e.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    if (area && String(e.area) !== String(area)) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando equipes..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Equipes</h1>
          <p className="text-muted-foreground">Gerencie as equipes da igreja</p>
        </div>
        <Button asChild>
          <Link to="/equipes/novo">
            <Plus className="h-4 w-4 mr-2" /> Nova Equipe
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
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar por nome</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Digite o nome da equipe"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Área</label>
              <select className="w-full px-3 py-2 border rounded" value={area} onChange={(e) => setArea(e.target.value)}>
                <option value="">Todas</option>
                <option value="1">Verde</option>
                <option value="2">Vermelha</option>
                <option value="3">Laranja</option>
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Equipes</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhuma equipe encontrada.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Área</TableHead>
                  <TableHead>Data de Criação</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((equipe) => (
                  <TableRow key={equipe.id}>
                    <TableCell className="font-medium">{equipe.nome}</TableCell>
                    <TableCell>{AREA_LABEL[equipe.area] || equipe.area}</TableCell>
                    <TableCell>{new Date(equipe.dataCriacao).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/equipes/${equipe.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(equipe.id)}>
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


