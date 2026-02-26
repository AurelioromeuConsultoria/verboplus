import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Download, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { SortableTableHeader } from '@/components/ui/sortable-table-header';
import { useTableSort } from '@/hooks/useTableSort';
import { exportToCSV } from '@/utils/export';
import { pessoasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';

export function PessoasList() {
  const [pessoas, setPessoas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    nome: '',
    email: '',
    telefone: '',
    whatsApp: '',
    perfil: undefined,
    tipoPessoa: undefined,
    ativo: undefined,
  });
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [pessoaToDelete, setPessoaToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const { can } = useAuth();

  const loadPessoas = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await pessoasApi.getAll();
      setPessoas(response.data || []);
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'Erro ao carregar pessoas';
      setError(errorMessage);
      console.error('Erro ao carregar pessoas:', err);
      toast.error(`Erro ao carregar pessoas: ${errorMessage}`);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = (pessoa) => {
    setPessoaToDelete(pessoa);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!pessoaToDelete) return;

    try {
      setDeleting(true);
      await pessoasApi.delete(pessoaToDelete.id);
      toast.success('Pessoa excluída com sucesso');
      setDeleteDialogOpen(false);
      setPessoaToDelete(null);
      await loadPessoas();
      // Reset to first page if current page becomes empty
      setPage(1);
    } catch (err) {
      toast.error('Erro ao excluir pessoa');
      console.error('Erro ao excluir pessoa:', err);
    } finally {
      setDeleting(false);
    }
  };

  useEffect(() => {
    loadPessoas();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Obter lista única de perfis para filtro
  const perfisUnicos = [...new Set(
    pessoas.flatMap(p => p.perfis?.map(perf => perf.perfil) || [])
      .filter(perfil => perfil != null && typeof perfil === 'string')
      .map(perfil => perfil.trim())
      .filter(perfil => perfil !== '')
  )];

  // Filtrar pessoas com busca avançada
  const pessoasFiltradasRaw = pessoas.filter((pessoa) => {
    // Busca por nome
    if (filters.nome && !pessoa.nome?.toLowerCase().includes(filters.nome.toLowerCase())) {
      return false;
    }

    // Busca por email
    if (filters.email && !pessoa.email?.toLowerCase().includes(filters.email.toLowerCase())) {
      return false;
    }

    // Busca por telefone
    if (filters.telefone && !pessoa.telefone?.includes(filters.telefone)) {
      return false;
    }

    // Busca por WhatsApp
    if (filters.whatsApp && !pessoa.whatsApp?.includes(filters.whatsApp)) {
      return false;
    }

    // Filtro por perfil
    if (filters.perfil && !pessoa.perfis?.some(p => p.perfil === filters.perfil)) {
      return false;
    }

    // Filtro por tipo de pessoa
    if (filters.tipoPessoa && pessoa.tipoPessoa !== filters.tipoPessoa) {
      return false;
    }

    // Filtro por status ativo
    if (filters.ativo !== undefined) {
      const isAtivo = filters.ativo === 'true' || filters.ativo === true;
      if (pessoa.ativo !== isAtivo) {
        return false;
      }
    }

    return true;
  });

  // Ordenação
  const { sortedData: pessoasFiltradas, sortConfig, handleSort } = useTableSort(pessoasFiltradasRaw, {
    defaultSort: 'nome',
    defaultDirection: 'asc',
  });

  // Paginação client-side
  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const pessoasPaginadas = pessoasFiltradas.slice(startIndex, endIndex);

  // Exportação
  const handleExport = () => {
    const exportData = pessoasFiltradas.map(pessoa => ({
      Nome: pessoa.nome || '',
      Email: pessoa.email || '',
      Telefone: pessoa.telefone || '',
      WhatsApp: pessoa.whatsApp || '',
      'Tipo de Pessoa': pessoa.tipoPessoa || '',
      Perfis: pessoa.perfis?.filter(p => !p.dataFim).map(p => p.perfil).join('; ') || '',
      Ativo: pessoa.ativo ? 'Sim' : 'Não',
      'Data de Criação': pessoa.dataCriacao ? new Date(pessoa.dataCriacao).toLocaleDateString('pt-BR') : '',
    }));

    exportToCSV(exportData, 'pessoas', [
      { key: 'Nome', label: 'Nome' },
      { key: 'Email', label: 'Email' },
      { key: 'Telefone', label: 'Telefone' },
      { key: 'WhatsApp', label: 'WhatsApp' },
      { key: 'Tipo de Pessoa', label: 'Tipo de Pessoa' },
      { key: 'Perfis', label: 'Perfis' },
      { key: 'Ativo', label: 'Ativo' },
      { key: 'Data de Criação', label: 'Data de Criação' },
    ]);

    toast.success('Dados exportados com sucesso!');
  };

  // Reset page when filters change
  useEffect(() => {
    setPage(1);
  }, [filters]);

  if (loading) {
    return <LoadingPage text="Carregando pessoas..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadPessoas} />;
  }

  const canEdit = can(RESOURCES.PESSOAS, ACTIONS.EDIT);
  const canDelete = can(RESOURCES.PESSOAS, ACTIONS.DELETE);
  const canCreateUsuario = can(RESOURCES.USUARIOS, ACTIONS.EDIT);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Pessoas</h1>
          <p className="text-muted-foreground">
            Gerencie as pessoas cadastradas no sistema
          </p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/pessoas/novo">
              <Plus className="h-4 w-4 mr-2" />
              Nova Pessoa
            </Link>
          </Button>
        )}
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'nome', label: 'Nome', type: 'text', placeholder: 'Buscar por nome...' },
          { key: 'email', label: 'Email', type: 'text', placeholder: 'Buscar por email...' },
          { key: 'telefone', label: 'Telefone', type: 'text', placeholder: 'Buscar por telefone...' },
          { key: 'whatsApp', label: 'WhatsApp', type: 'text', placeholder: 'Buscar por WhatsApp...' },
        ]}
        filterFields={[
          {
            key: 'perfil',
            label: 'Perfil',
            type: 'select',
            options: perfisUnicos.map(perfil => ({ value: perfil, label: perfil })),
          },
          {
            key: 'tipoPessoa',
            label: 'Tipo de Pessoa',
            type: 'select',
            options: [
              { value: 'Adulto', label: 'Adulto' },
              { value: 'Crianca', label: 'Criança' },
            ],
          },
          {
            key: 'ativo',
            label: 'Status',
            type: 'boolean',
            trueLabel: 'Ativo',
            falseLabel: 'Inativo',
          },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() => {
          setFilters({
            nome: '',
            email: '',
            telefone: '',
            whatsApp: '',
            perfil: undefined,
            tipoPessoa: undefined,
            ativo: undefined,
          });
        }}
      />

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Lista de Pessoas ({pessoasFiltradas.length})</CardTitle>
            {pessoasFiltradas.length > 0 && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 mr-2" />
                Exportar CSV
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {pessoasFiltradas.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                {pessoas.length === 0 
                  ? 'Nenhuma pessoa cadastrada ainda.'
                  : 'Nenhuma pessoa encontrada com os filtros aplicados.'}
              </p>
              {pessoas.length === 0 && canEdit && (
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
                  <SortableTableHeader field="nome" onSort={handleSort} sortConfig={sortConfig}>
                    Nome
                  </SortableTableHeader>
                  <SortableTableHeader field="email" onSort={handleSort} sortConfig={sortConfig}>
                    Email
                  </SortableTableHeader>
                  <SortableTableHeader field="telefone" onSort={handleSort} sortConfig={sortConfig}>
                    Telefone
                  </SortableTableHeader>
                  <SortableTableHeader field="whatsApp" onSort={handleSort} sortConfig={sortConfig}>
                    WhatsApp
                  </SortableTableHeader>
                  <SortableTableHeader field="tipoPessoa" onSort={handleSort} sortConfig={sortConfig}>
                    Tipo
                  </SortableTableHeader>
                  <TableHead>Perfis</TableHead>
                  <SortableTableHeader field="ativo" onSort={handleSort} sortConfig={sortConfig}>
                    Ativo
                  </SortableTableHeader>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {pessoasPaginadas.map((pessoa) => (
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
                        {canEdit && (
                          <Button variant="ghost" size="sm" asChild>
                            <Link to={`/pessoas/${pessoa.id}/editar`}>
                              <Edit className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canCreateUsuario && (
                          <Button variant="ghost" size="sm" asChild title="Criar acesso para esta pessoa">
                            <Link to={`/usuarios?pessoaId=${pessoa.id}`}>
                              <UserPlus className="h-4 w-4" />
                            </Link>
                          </Button>
                        )}
                        {canDelete && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteClick(pessoa)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {pessoasFiltradas.length > 0 && (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              total={pessoasFiltradas.length}
              onPageChange={setPage}
              onPageSizeChange={(newSize) => {
                setPageSize(newSize);
                setPage(1);
              }}
            />
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        onConfirm={handleDeleteConfirm}
        title="Excluir Pessoa"
        description={`Tem certeza que deseja excluir "${pessoaToDelete?.nome}"? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText="Cancelar"
        variant="destructive"
        loading={deleting}
      />
    </div>
  );
}



