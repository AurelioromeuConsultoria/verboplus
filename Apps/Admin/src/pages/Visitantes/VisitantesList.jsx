import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Search, Filter, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { visitantesApi } from '@/lib/api';
import { toast } from 'sonner';

export function VisitantesList() {
  const [visitantes, setVisitantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [dataInicio, setDataInicio] = useState('');
  const [dataFim, setDataFim] = useState('');

  const loadVisitantes = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await visitantesApi.getAll();
      setVisitantes(response.data || []);
    } catch (err) {
      setError('Erro ao carregar visitantes');
      console.error('Erro ao carregar visitantes:', err);
      toast.error('Erro ao carregar visitantes');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta visita?')) {
      return;
    }

    try {
      await visitantesApi.delete(id);
      toast.success('Visita excluída com sucesso');
      await loadVisitantes();
    } catch (err) {
      toast.error('Erro ao excluir visita');
      console.error('Erro ao excluir visita:', err);
    }
  };

  useEffect(() => {
    loadVisitantes();
  }, []);

  // Filtrar visitantes
  const visitantesFiltrados = visitantes.filter((visitante) => {
    const buscaLower = busca.toLowerCase();
    const matchBusca = !busca || 
      visitante.pessoa?.nome?.toLowerCase().includes(buscaLower) ||
      visitante.pessoa?.email?.toLowerCase().includes(buscaLower) ||
      visitante.pessoa?.telefone?.includes(busca) ||
      visitante.pessoa?.whatsApp?.includes(busca);

    const dataVisita = new Date(visitante.dataVisita);
    const matchDataInicio = !dataInicio || dataVisita >= new Date(dataInicio + 'T00:00:00');
    const matchDataFim = !dataFim || dataVisita <= new Date(dataFim + 'T23:59:59');

    return matchBusca && matchDataInicio && matchDataFim;
  });

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
            Histórico de visitas
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
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2">
                <Search className="h-4 w-4" />
                Buscar
              </label>
              <Input
                placeholder="Nome, Email, Telefone ou WhatsApp"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Data Início
              </label>
              <Input
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Data Fim
              </label>
              <Input
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Visitas ({visitantesFiltrados.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {visitantesFiltrados.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                {visitantes.length === 0 
                  ? 'Nenhuma visita cadastrada ainda.'
                  : 'Nenhuma visita encontrada com os filtros aplicados.'}
              </p>
              {visitantes.length === 0 && (
                <Button asChild>
                  <Link to="/visitantes/novo">
                    <Plus className="h-4 w-4 mr-2" />
                    Cadastrar Primeira Visita
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Data da Visita</TableHead>
                  <TableHead>Pessoa</TableHead>
                  <TableHead>Contato</TableHead>
                  <TableHead>Observações</TableHead>
                  <TableHead>Perfis</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {visitantesFiltrados.map((visitante) => {
                  const pessoa = visitante.pessoa || {};
                  const contato = pessoa.email || pessoa.whatsApp || pessoa.telefone || '-';
                  const perfis = pessoa.perfis || [];
                  const perfisAtivos = perfis.filter(p => !p.dataFim);
                  
                  return (
                    <TableRow key={visitante.id}>
                      <TableCell>
                        {new Date(visitante.dataVisita).toLocaleDateString('pt-BR')}
                      </TableCell>
                      <TableCell className="font-medium">
                        {pessoa.nome || '-'}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <span className="text-sm">{contato}</span>
                          {pessoa.email && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => window.open(`mailto:${pessoa.email}`)}
                            >
                              <Mail className="h-4 w-4" />
                            </Button>
                          )}
                          {pessoa.whatsApp && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => window.open(`https://wa.me/55${pessoa.whatsApp.replace(/\D/g, '')}`)}
                            >
                              <Phone className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="text-sm text-muted-foreground">
                          {visitante.observacoes 
                            ? (visitante.observacoes.length > 50 
                                ? visitante.observacoes.substring(0, 50) + '...'
                                : visitante.observacoes)
                            : '-'}
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {perfisAtivos.length > 0 ? (
                            perfisAtivos.map((perfil, idx) => (
                              <Badge key={idx} variant="secondary" className="text-xs">
                                {perfil.perfil}
                              </Badge>
                            ))
                          ) : (
                            <span className="text-muted-foreground text-sm">-</span>
                          )}
                        </div>
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
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

