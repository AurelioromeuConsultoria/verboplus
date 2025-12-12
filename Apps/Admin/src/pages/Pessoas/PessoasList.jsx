import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Search, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { pessoasApi } from '@/lib/api';
import { toast } from 'sonner';

export function PessoasList() {
  const [pessoas, setPessoas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [filtroPerfil, setFiltroPerfil] = useState('');
  const [filtroTipoPessoa, setFiltroTipoPessoa] = useState('');

  const loadPessoas = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await pessoasApi.getAll();
      setPessoas(response.data || []);
    } catch (err) {
      setError('Erro ao carregar pessoas');
      console.error('Erro ao carregar pessoas:', err);
      toast.error('Erro ao carregar pessoas');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta pessoa?')) {
      return;
    }

    try {
      await pessoasApi.delete(id);
      toast.success('Pessoa excluída com sucesso');
      await loadPessoas();
    } catch (err) {
      toast.error('Erro ao excluir pessoa');
      console.error('Erro ao excluir pessoa:', err);
    }
  };

  useEffect(() => {
    loadPessoas();
  }, []);

  // Obter lista única de perfis para filtro
  const perfisUnicos = [...new Set(
    pessoas.flatMap(p => p.perfis?.map(perf => perf.perfil) || [])
  )];

  // Filtrar pessoas
  const pessoasFiltradas = pessoas.filter((pessoa) => {
    const buscaLower = busca.toLowerCase();
    const matchBusca = !busca || 
      pessoa.nome?.toLowerCase().includes(buscaLower) ||
      pessoa.email?.toLowerCase().includes(buscaLower) ||
      pessoa.telefone?.includes(busca) ||
      pessoa.whatsApp?.includes(busca);

    const matchPerfil = !filtroPerfil || 
      pessoa.perfis?.some(p => p.perfil === filtroPerfil);

    const matchTipo = !filtroTipoPessoa || 
      pessoa.tipoPessoa === filtroTipoPessoa;

    return matchBusca && matchPerfil && matchTipo;
  });

  if (loading) {
    return <LoadingPage text="Carregando pessoas..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadPessoas} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Pessoas</h1>
          <p className="text-muted-foreground">
            Gerencie as pessoas cadastradas no sistema
          </p>
        </div>
        <Button asChild>
          <Link to="/pessoas/novo">
            <Plus className="h-4 w-4 mr-2" />
            Nova Pessoa
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
              <label className="text-sm font-medium">Perfil</label>
              <Select value={filtroPerfil} onValueChange={setFiltroPerfil}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os perfis" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos os perfis</SelectItem>
                  {perfisUnicos.map((perfil) => (
                    <SelectItem key={perfil} value={perfil}>
                      {perfil}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Tipo de Pessoa</label>
              <Select value={filtroTipoPessoa} onValueChange={setFiltroTipoPessoa}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os tipos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos os tipos</SelectItem>
                  <SelectItem value="Adulto">Adulto</SelectItem>
                  <SelectItem value="Crianca">Criança</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Pessoas ({pessoasFiltradas.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {pessoasFiltradas.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                {pessoas.length === 0 
                  ? 'Nenhuma pessoa cadastrada ainda.'
                  : 'Nenhuma pessoa encontrada com os filtros aplicados.'}
              </p>
              {pessoas.length === 0 && (
                <Button asChild>
                  <Link to="/pessoas/novo">
                    <Plus className="h-4 w-4 mr-2" />
                    Cadastrar Primeira Pessoa
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Telefone</TableHead>
                  <TableHead>WhatsApp</TableHead>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Perfis</TableHead>
                  <TableHead>Ativo</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {pessoasFiltradas.map((pessoa) => (
                  <TableRow key={pessoa.id}>
                    <TableCell className="font-medium">
                      {pessoa.nome}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{pessoa.email || '-'}</span>
                        {pessoa.email && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => window.open(`mailto:${pessoa.email}`)}
                          >
                            <Mail className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      {pessoa.telefone || '-'}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <span>{pessoa.whatsApp || '-'}</span>
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
                      <Badge variant="outline">
                        {pessoa.tipoPessoa || '-'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">
                        {pessoa.perfis && pessoa.perfis.length > 0 ? (
                          pessoa.perfis
                            .filter(p => !p.dataFim) // Apenas perfis ativos
                            .map((perfil, idx) => (
                              <Badge key={idx} variant="secondary">
                                {perfil.perfil}
                              </Badge>
                            ))
                        ) : (
                          <span className="text-muted-foreground text-sm">-</span>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={pessoa.ativo ? 'default' : 'secondary'}>
                        {pessoa.ativo ? 'Sim' : 'Não'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/pessoas/${pessoa.id}`}>
                            <Eye className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button variant="ghost" size="sm" asChild>
                          <Link to={`/pessoas/${pessoa.id}/editar`}>
                            <Edit className="h-4 w-4" />
                          </Link>
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(pessoa.id)}
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


