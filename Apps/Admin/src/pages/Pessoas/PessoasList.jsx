import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Eye, Edit, Trash2, Phone, Mail, Download, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { SortableTableHeader } from '@/components/ui/sortable-table-header';
import { exportToCSV } from '@/utils/export';
import { pessoasApi } from '@/lib/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

const PERFIS_OPTIONS = [
  { value: 'Visitante', label: 'Visitante' },
  { value: 'Membro', label: 'Membro' },
  { value: 'Voluntario', label: 'Voluntário' },
  { value: 'Lider', label: 'Líder' },
  { value: 'Kids', label: 'Kids' },
  { value: 'Admin', label: 'Administrador' },
];

export default function PessoasList() {
  const [pessoas, setPessoas] = useState([]);
  const [total, setTotal] = useState(0);
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
  const [sortConfig, setSortConfig] = useState({ field: 'nome', direction: 'asc' });
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [pessoaToDelete, setPessoaToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [bulkDeleteDialogOpen, setBulkDeleteDialogOpen] = useState(false);
  const [bulkDeleting, setBulkDeleting] = useState(false);
  const { can } = useAuth();
  const { t } = useTranslation();

  const loadPessoas = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const ativoParam =
        filters.ativo === undefined ? undefined : (filters.ativo === true || filters.ativo === 'true');
      const response = await pessoasApi.getPaged({
        page,
        pageSize,
        sort: sortConfig.field,
        direction: sortConfig.direction,
        nome: filters.nome || undefined,
        email: filters.email || undefined,
        telefone: filters.telefone || undefined,
        whatsApp: filters.whatsApp || undefined,
        perfil: filters.perfil || undefined,
        tipoPessoa: filters.tipoPessoa || undefined,
        ativo: ativoParam,
      });

      const data = response.data || {};
      setPessoas(data.items || []);
      setTotal(Number(data.total || 0));
    } catch (err) {
      const errorMessage = err.response?.data?.message || err.message || 'Erro ao carregar pessoas';
      setError(errorMessage);
      console.error('Erro ao carregar pessoas:', err);
      toast.error(`Erro ao carregar pessoas: ${errorMessage}`);
    } finally {
      setLoading(false);
    }
  }, [filters, page, pageSize, sortConfig.direction, sortConfig.field]);

  const handleDeleteClick = (pessoa) => {
    setPessoaToDelete(pessoa);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!pessoaToDelete) return;
    const currentPageCount = pessoas.length;

    try {
      setDeleting(true);
      await pessoasApi.delete(pessoaToDelete.id);
      toast.success('Pessoa excluída com sucesso');
      setDeleteDialogOpen(false);
      setPessoaToDelete(null);
      // Recarrega a página atual; se ficar vazia, volta uma página.
      await loadPessoas();
      if (page > 1 && currentPageCount === 1) {
        setPage((p) => Math.max(1, p - 1));
      }
    } catch (err) {
      toast.error('Erro ao excluir pessoa');
      console.error('Erro ao excluir pessoa:', err);
    } finally {
      setDeleting(false);
    }
  };

  useEffect(() => {
    loadPessoas();
  }, [loadPessoas]);

  useEffect(() => {
    setSelectedIds(new Set());
  }, [page, filters]);

  const pageIds = pessoas.map((p) => p.id);
  const allPageSelected = pageIds.length > 0 && pageIds.every((id) => selectedIds.has(id));

  const toggleSelectAll = () => {
    if (allPageSelected) {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        pageIds.forEach((id) => next.delete(id));
        return next;
      });
    } else {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        pageIds.forEach((id) => next.add(id));
        return next;
      });
    }
  };

  const toggleSelect = (id) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleBulkDeleteClick = () => {
    if (selectedIds.size === 0) return;
    setBulkDeleteDialogOpen(true);
  };

  const handleBulkDeleteConfirm = async () => {
    const ids = Array.from(selectedIds);
    if (ids.length === 0) return;

    try {
      setBulkDeleting(true);
      let ok = 0;
      let fail = 0;
      for (const id of ids) {
        try {
          await pessoasApi.delete(id);
          ok += 1;
        } catch {
          fail += 1;
        }
      }
      setSelectedIds(new Set());
      setBulkDeleteDialogOpen(false);
      await loadPessoas();
      if (page > 1 && pessoas.length === ids.length) setPage((p) => Math.max(1, p - 1));
      if (fail > 0) {
        toast.warning(`${ok} excluída(s), ${fail} falha(s).`);
      } else {
        toast.success(`${ok} pessoa(s) excluída(s) com sucesso`);
      }
    } catch (err) {
      toast.error('Erro ao excluir em lote');
    } finally {
      setBulkDeleting(false);
    }
  };

  const handleSort = (field) => {
    setSortConfig((prev) => {
      if (prev.field === field) {
        return { field, direction: prev.direction === 'asc' ? 'desc' : 'asc' };
      }
      return { field, direction: 'asc' };
    });
    setPage(1);
  };

  // Exportação
  const handleExport = async () => {
    try {
      // Exporta TODOS os itens do filtro atual buscando páginas sequencialmente.
      const all = [];
      let p = 1;
      let totalItems = Infinity;
      const exportPageSize = 200;
      const ativoParam =
        filters.ativo === undefined ? undefined : (filters.ativo === true || filters.ativo === 'true');

      while (all.length < totalItems) {
        const resp = await pessoasApi.getPaged({
          page: p,
          pageSize: exportPageSize,
          sort: sortConfig.field,
          direction: sortConfig.direction,
          nome: filters.nome || undefined,
          email: filters.email || undefined,
          telefone: filters.telefone || undefined,
          whatsApp: filters.whatsApp || undefined,
          perfil: filters.perfil || undefined,
          tipoPessoa: filters.tipoPessoa || undefined,
          ativo: ativoParam,
        });

        const data = resp.data || {};
        const items = data.items || [];
        totalItems = Number(data.total || 0);
        all.push(...items);
        if (items.length === 0) break;
        p += 1;
        if (p > 200) break; // trava de segurança
      }

      const exportData = all.map(pessoa => ({
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
    } catch (err) {
      console.error('Erro ao exportar pessoas:', err);
      toast.error('Erro ao exportar dados');
    }
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
          <h1 className="text-3xl font-bold">{t('people.title')}</h1>
          <p className="text-muted-foreground">
            {t('people.subtitle')}
          </p>
        </div>
        {canEdit && (
          <Button asChild>
            <Link to="/pessoas/novo">
              <Plus className="h-4 w-4 mr-2" />
              {t('people.new')}
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
            options: PERFIS_OPTIONS,
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
            <CardTitle>{t('people.listTitle')} ({total})</CardTitle>
            {total > 0 && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 mr-2" />
                Exportar CSV
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {selectedIds.size > 0 && canDelete && (
            <div className="flex items-center justify-between rounded-md border bg-muted/50 px-4 py-2 mb-4">
              <span className="text-sm font-medium">
                {selectedIds.size} selecionada(s)
              </span>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" onClick={() => setSelectedIds(new Set())}>
                  Limpar seleção
                </Button>
                <Button variant="destructive" size="sm" onClick={handleBulkDeleteClick}>
                  <Trash2 className="h-4 w-4 mr-2" />
                  Excluir selecionadas
                </Button>
              </div>
            </div>
          )}
          {pessoas.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground mb-4">
                {total === 0 ? t('people.emptyMessage') : t('people.emptyPageMessage')}
              </p>
              {total === 0 && canEdit && (
                <Button asChild>
                  <Link to="/pessoas/novo">
                    <Plus className="h-4 w-4 mr-2" />
                    {t('people.emptyCta')}
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  {canDelete && (
                    <TableHead className="w-12">
                      <Checkbox
                        checked={allPageSelected}
                        onCheckedChange={toggleSelectAll}
                        aria-label="Selecionar todas"
                      />
                    </TableHead>
                  )}
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
                {pessoas.map((pessoa) => (
                  <TableRow key={pessoa.id}>
                    {canDelete && (
                      <TableCell>
                        <Checkbox
                          checked={selectedIds.has(pessoa.id)}
                          onCheckedChange={() => toggleSelect(pessoa.id)}
                          aria-label={`Selecionar ${pessoa.nome}`}
                        />
                      </TableCell>
                    )}
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
          {total > 0 && (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              total={total}
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
        cancelText={t('actions.cancel')}
        variant="destructive"
        loading={deleting}
      />

      <ConfirmDialog
        open={bulkDeleteDialogOpen}
        onOpenChange={setBulkDeleteDialogOpen}
        onConfirm={handleBulkDeleteConfirm}
        title="Excluir em lote"
        description={`Tem certeza que deseja excluir ${selectedIds.size} pessoa(s) selecionada(s)? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText={t('actions.cancel')}
        variant="destructive"
        loading={bulkDeleting}
      />
    </div>
  );
}



