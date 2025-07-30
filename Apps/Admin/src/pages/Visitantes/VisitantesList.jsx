import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { visitantesApi } from '@/lib/api';

export function VisitantesList() {
  const [visitantes, setVisitantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadVisitantes = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await visitantesApi.getAll();
      setVisitantes(response.data);
    } catch (err) {
      setError('Erro ao carregar visitantes');
      console.error('Erro ao carregar visitantes:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir este visitante?')) {
      return;
    }

    try {
      await visitantesApi.delete(id);
      await loadVisitantes();
    } catch (err) {
      alert('Erro ao excluir visitante');
      console.error('Erro ao excluir visitante:', err);
    }
  };

  useEffect(() => {
    loadVisitantes();
  }, []);

  if (loading) {
    return <LoadingPage text="Carregando visitantes..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadVisitantes} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Visitantes</h1>
          <p className="text-muted-foreground">
            Gerencie os visitantes da igreja
          </p>
        </div>
        <Button asChild>
          <Link to="/visitantes/novo">
            <Plus className="h-4 w-4 mr-2" />
            Novo Visitante
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Visitantes</CardTitle>
        </CardHeader>
        <CardContent>
          {visitantes.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                Nenhum visitante cadastrado ainda.
              </p>
              <Button asChild>
                <Link to="/visitantes/novo">
                  <Plus className="h-4 w-4 mr-2" />
                  Cadastrar Primeiro Visitante
                </Link>
              </Button>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Telefone</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Data da Visita</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {visitantes.map((visitante) => (
                  <TableRow key={visitante.id}>
                    <TableCell className="font-medium">
                      {visitante.nome}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{visitante.telefone}</span>
                        {visitante.telefone && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`https://wa.me/55${visitante.telefone.replace(/\D/g, '')}`)}
                          >
                            <Phone className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{visitante.email || '-'}</span>
                        {visitante.email && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`mailto:${visitante.email}`)}
                          >
                            <Mail className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      {new Date(visitante.dataVisita).toLocaleDateString('pt-BR')}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/visitantes/${visitante.id}`}>
                            <Eye className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/visitantes/${visitante.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(visitante.id)}
                        >
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

