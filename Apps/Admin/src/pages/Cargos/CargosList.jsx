import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { cargosApi } from '@/lib/api';

export default function CargosList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await cargosApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar cargos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir este cargo?')) return;
    try {
      await cargosApi.delete(id);
      await load();
    } catch (err) {
      alert('Erro ao excluir. Existe(m) voluntário(s) vinculado(s).');
      console.error(err);
    }
  };

  const filtered = items.filter((c) => {
    if (busca && !c.nome.toLowerCase().includes(busca.toLowerCase())) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando cargos..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Cargos</h1>
          <p className="text-muted-foreground">Gerencie os cargos</p>
        </div>
        <Button asChild>
          <Link to="/cargos/novo">
            <Plus className="h-4 w-4 mr-2" /> Novo Cargo
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
                placeholder="Digite o nome do cargo"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Cargos</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum cargo encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Data de Criação</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((cargo) => (
                  <TableRow key={cargo.id}>
                    <TableCell className="font-medium">{cargo.nome}</TableCell>
                    <TableCell>{new Date(cargo.dataCriacao).toLocaleDateString('pt-BR')}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/cargos/${cargo.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(cargo.id)}>
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


