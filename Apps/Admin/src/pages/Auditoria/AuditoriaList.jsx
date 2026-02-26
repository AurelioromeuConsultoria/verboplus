import { useCallback, useEffect, useMemo, useState } from 'react';
import { Shield, Eye } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { DataTablePagination } from '@/components/ui/data-table-pagination';
import { AdvancedSearch } from '@/components/ui/advanced-search';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { auditLogsApi } from '@/lib/api';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/apiError';

const ENTITY_OPTIONS = [
  { value: 'Pessoa', label: 'Pessoa' },
  { value: 'Visitante', label: 'Visitante' },
  { value: 'Evento', label: 'Evento' },
  { value: 'Noticia', label: 'Notícia' },
  { value: 'Usuario', label: 'Usuário' },
  { value: 'MensagemAgendada', label: 'Mensagem Agendada' },
];

const ACTION_OPTIONS = [
  { value: 'Create', label: 'Criação' },
  { value: 'Update', label: 'Edição' },
  { value: 'Delete', label: 'Exclusão' },
];

export default function AuditoriaList() {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const [filters, setFilters] = useState({
    entityName: undefined,
    action: undefined,
    userEmail: '',
    createdAt_from: '',
    createdAt_to: '',
  });

  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selected, setSelected] = useState(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const resp = await auditLogsApi.getPaged({
        page,
        pageSize,
        entityName: filters.entityName || undefined,
        action: filters.action || undefined,
        userEmail: filters.userEmail || undefined,
        from: filters.createdAt_from || undefined,
        to: filters.createdAt_to || undefined,
      });

      const data = resp.data || {};
      setItems(data.items || []);
      setTotal(Number(data.total || 0));
    } catch (err) {
      const msg = getApiErrorMessage(err, 'Erro ao carregar auditoria');
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  }, [filters.action, filters.createdAt_from, filters.createdAt_to, filters.entityName, filters.userEmail, page, pageSize]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    setPage(1);
  }, [filters]);

  const prettyJson = useMemo(() => {
    if (!selected?.changesJson) return null;
    try {
      const obj = JSON.parse(selected.changesJson);
      return JSON.stringify(obj, null, 2);
    } catch {
      return selected.changesJson;
    }
  }, [selected]);

  if (loading) return <LoadingPage text="Carregando auditoria..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Auditoria</h1>
          <p className="text-muted-foreground mt-1">Histórico de alterações no sistema</p>
        </div>
      </div>

      <AdvancedSearch
        searchFields={[
          { key: 'userEmail', label: 'E-mail do usuário', type: 'text', placeholder: 'Buscar por e-mail...' },
        ]}
        filterFields={[
          { key: 'entityName', label: 'Entidade', type: 'select', options: ENTITY_OPTIONS },
          { key: 'action', label: 'Ação', type: 'select', options: ACTION_OPTIONS },
          { key: 'createdAt', label: 'Data', type: 'date-range' },
        ]}
        values={filters}
        onChange={setFilters}
        onReset={() =>
          setFilters({
            entityName: undefined,
            action: undefined,
            userEmail: '',
            createdAt_from: '',
            createdAt_to: '',
          })
        }
      />

      <Card>
        <CardHeader>
          <CardTitle>Logs ({total})</CardTitle>
        </CardHeader>
        <CardContent>
          {items.length === 0 ? (
            <div className="text-center py-12">
              <Shield className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium text-foreground mb-2">Nenhum log encontrado</h3>
              <p className="text-muted-foreground">Ajuste os filtros ou aguarde novas alterações no sistema.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data</TableHead>
                    <TableHead>Ação</TableHead>
                    <TableHead>Entidade</TableHead>
                    <TableHead>ID</TableHead>
                    <TableHead>Usuário</TableHead>
                    <TableHead>IP</TableHead>
                    <TableHead className="text-right">Detalhes</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {items.map((it) => (
                    <TableRow key={it.id}>
                      <TableCell>{it.createdAt ? new Date(it.createdAt).toLocaleString('pt-BR') : '-'}</TableCell>
                      <TableCell>
                        <Badge variant={it.action === 'Delete' ? 'destructive' : it.action === 'Create' ? 'default' : 'secondary'}>
                          {it.action}
                        </Badge>
                      </TableCell>
                      <TableCell>{it.entityName}</TableCell>
                      <TableCell>{it.entityId}</TableCell>
                      <TableCell className="max-w-[260px] truncate">
                        {it.userEmail || it.userName || (it.userId ? `User ${it.userId}` : '-') }
                      </TableCell>
                      <TableCell>{it.ipAddress || '-'}</TableCell>
                      <TableCell className="text-right">
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => {
                            setSelected(it);
                            setDetailsOpen(true);
                          }}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
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

      <Dialog open={detailsOpen} onOpenChange={setDetailsOpen}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Detalhes do log</DialogTitle>
            <DialogDescription>
              {selected ? `${selected.entityName} ${selected.entityId} — ${selected.action}` : ''}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div className="grid grid-cols-1 gap-2 text-sm">
              <div><span className="text-muted-foreground">Usuário:</span> {selected?.userEmail || selected?.userName || selected?.userId || '-'}</div>
              <div><span className="text-muted-foreground">IP:</span> {selected?.ipAddress || '-'}</div>
              <div><span className="text-muted-foreground">Quando:</span> {selected?.createdAt ? new Date(selected.createdAt).toLocaleString('pt-BR') : '-'}</div>
            </div>
            <div className="rounded-md border bg-muted/30 p-3">
              <pre className="whitespace-pre-wrap text-xs overflow-auto max-h-[380px]">
                {prettyJson || 'Sem detalhes de mudanças.'}
              </pre>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

