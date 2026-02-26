import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Filter, X, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { usePagination } from '@/hooks/usePagination';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { pessoasPerfisApi } from '@/lib/api';
import { toast } from 'sonner';

export default function PerfisList() {
  const [perfis, setPerfis] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filtroPerfil, setFiltroPerfil] = useState('');
  const [filtroStatus, setFiltroStatus] = useState('');
  const confirmDialog = useConfirmDialog();

  const loadPerfis = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await pessoasPerfisApi.getAll();
      setPerfis(response.data || []);
    } catch (err) {
      setError('Erro ao carregar perfis');
      console.error('Erro ao carregar perfis:', err);
      toast.error('Erro ao carregar perfis');
    } finally {
      setLoading(false);
    }
  };

  const handleEncerrarPerfil = async (id) => {
    const perfil = perfis.find(p => p.id === id);
    confirmDialog.show({
      title: 'Encerrar Perfil',
      description: `Tem certeza que deseja encerrar o perfil "${perfil?.perfil || 'este perfil'}" de "${perfil?.pessoa?.nome || 'esta pessoa'}"?`,
      confirmText: 'Encerrar',
      cancelText: 'Cancelar',
      variant: 'default',
      onConfirm: async () => {
        try {
          await pessoasPerfisApi.update(id, {
            dataFim: new Date().toISOString(),
          });
          toast.success('Perfil encerrado com sucesso');
          await loadPerfis();
        } catch (err) {
          toast.error('Erro ao encerrar perfil');
          console.error('Erro ao encerrar perfil:', err);
          throw err;
        }
      },
    });
  };

  const handleDelete = async (id) => {
    const perfil = perfis.find(p => p.id === id);
    confirmDialog.show({
      title: 'Excluir Perfil',
      description: `Tem certeza que deseja excluir o perfil "${perfil?.perfil || 'este perfil'}" de "${perfil?.pessoa?.nome || 'esta pessoa'}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await pessoasPerfisApi.delete(id);
          toast.success('Perfil excluído com sucesso');
          await loadPerfis();
        } catch (err) {
          toast.error('Erro ao excluir perfil');
          console.error('Erro ao excluir perfil:', err);
          throw err;
        }
      },
    });
  };

  useEffect(() => {
    loadPerfis();
  }, []);

  // Obter lista única de perfis para filtro
  const perfisUnicos = [...new Set(
    perfis
      .map(p => p.perfil)
      .filter(perfil => perfil != null && typeof perfil === 'string')
      .map(perfil => perfil.trim())
      .filter(perfil => perfil !== '')
  )];

  // Filtrar perfis
  const perfisFiltrados = perfis.filter((perfil) => {
    const matchPerfil = !filtroPerfil || perfil.perfil === filtroPerfil;
    
    const isAtivo = !perfil.dataFim;
    const matchStatus = !filtroStatus || 
      (filtroStatus === 'ativo' && isAtivo) ||
      (filtroStatus === 'inativo' && !isAtivo);

    return matchPerfil && matchStatus;
  });

  const { page, pageSize, total, paginatedItems, setPage, setPageSize } = usePagination(perfisFiltrados, 20);

  if (loading) {
    return <LoadingPage text="Carregando perfis..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadPerfis} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Perfis</h1>
          <p className="text-muted-foreground">
            Gerencie os perfis das pessoas
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="text-sm font-medium">Perfil</label>
              <Select value={filtroPerfil || "all"} onValueChange={(value) => setFiltroPerfil(value === "all" ? "" : value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os perfis" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os perfis</SelectItem>
                  {perfisUnicos.map((perfil) => (
                    <SelectItem key={perfil} value={perfil}>
                      {perfil}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Status</label>
              <Select value={filtroStatus || "all"} onValueChange={(value) => setFiltroStatus(value === "all" ? "" : value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos os status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os status</SelectItem>
                  <SelectItem value="ativo">Ativo</SelectItem>
                  <SelectItem value="inativo">Inativo</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Perfis ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {perfisFiltrados.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">
                {perfis.length === 0 
                  ? 'Nenhum perfil cadastrado ainda.'
                  : 'Nenhum perfil encontrado com os filtros aplicados.'}
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Pessoa</TableHead>
                  <TableHead>Perfil</TableHead>
                  <TableHead>Data Início</TableHead>
                  <TableHead>Data Fim</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((perfil) => {
                  const isAtivo = !perfil.dataFim;
                  return (
                    <TableRow key={perfil.id}>
                      <TableCell className="font-medium">
                        <Link 
                          to={`/pessoas/${perfil.pessoaId}`}
                          className="text-primary hover:underline"
                        >
                          {perfil.pessoa?.nome || `Pessoa #${perfil.pessoaId}`}
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary">{perfil.perfil}</Badge>
                      </TableCell>
                      <TableCell>
                        {new Date(perfil.dataInicio).toLocaleDateString('pt-BR')}
                      </TableCell>
                      <TableCell>
                        {perfil.dataFim 
                          ? new Date(perfil.dataFim).toLocaleDateString('pt-BR')
                          : '-'}
                      </TableCell>
                      <TableCell>
                        <Badge variant={isAtivo ? 'default' : 'secondary'}>
                          {isAtivo ? 'Ativo' : 'Inativo'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end space-x-2">
                          {isAtivo && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleEncerrarPerfil(perfil.id)}
                              title="Encerrar perfil"
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          )}
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDelete(perfil.id)}
                            title="Excluir perfil"
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
          {perfisFiltrados.length > 0 && (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              total={total}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
            />
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={confirmDialog.hide}
        onConfirm={confirmDialog.handleConfirm}
        title={confirmDialog.config.title}
        description={confirmDialog.config.description}
        confirmText={confirmDialog.config.confirmText}
        cancelText={confirmDialog.config.cancelText}
        variant={confirmDialog.config.variant}
        loading={confirmDialog.loading}
      />
    </div>
  );
}




