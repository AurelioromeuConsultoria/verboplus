import { useEffect, useState } from 'react';
import { Plus, Edit, Trash2, Filter, UserCheck, UserX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { usuariosApi } from '@/lib/api';
import { toast } from 'sonner';
import UsuarioForm from './UsuarioForm';

const TIPO_USUARIO_LABELS = {
  1: 'Administrador',
  2: 'Portal',
  3: 'Ambos',
};

const TIPO_USUARIO_COLORS = {
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-purple-100 text-purple-800',
};

export default function UsuariosList() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busca, setBusca] = useState('');
  const [tipoFilter, setTipoFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await usuariosApi.getAll();
      setItems(res.data || []);
    } catch (err) {
      setError('Erro ao carregar usuários');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id) => {
    if (!confirm('Tem certeza que deseja excluir este usuário?')) return;
    try {
      await usuariosApi.delete(id);
      toast.success('Usuário excluído com sucesso');
      await load();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Erro ao excluir usuário');
      console.error(err);
    }
  };

  const handleToggleAtivo = async (usuario) => {
    try {
      await usuariosApi.update(usuario.id, {
        nome: usuario.nome,
        email: usuario.email,
        tipoUsuario: usuario.tipoUsuario,
        ativo: !usuario.ativo,
      });
      toast.success(`Usuário ${!usuario.ativo ? 'ativado' : 'desativado'} com sucesso`);
      await load();
    } catch (err) {
      toast.error('Erro ao alterar status do usuário');
      console.error(err);
    }
  };

  const handleEdit = (id) => {
    setEditingId(id);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingId(null);
  };

  const handleFormSuccess = () => {
    handleCloseForm();
    load();
  };

  const filtered = items.filter((u) => {
    if (busca && !u.nome?.toLowerCase().includes(busca.toLowerCase()) && !u.email?.toLowerCase().includes(busca.toLowerCase())) return false;
    if (tipoFilter && String(u.tipoUsuario) !== tipoFilter) return false;
    if (statusFilter !== '' && String(u.ativo) !== statusFilter) return false;
    return true;
  });

  if (loading) return <LoadingPage text="Carregando usuários..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Usuários</h1>
          <p className="text-muted-foreground">Gerencie os usuários do sistema</p>
        </div>
        <Button onClick={() => setShowForm(true)}>
          <Plus className="h-4 w-4 mr-2" /> Novo Usuário
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center gap-2"><Filter className="h-4 w-4" />Buscar</label>
              <input
                className="w-full px-3 py-2 border rounded"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
                placeholder="Nome ou email"
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Tipo de Usuário</label>
              <select className="w-full px-3 py-2 border rounded" value={tipoFilter} onChange={(e) => setTipoFilter(e.target.value)}>
                <option value="">Todos</option>
                <option value="1">Administrador</option>
                <option value="2">Portal</option>
                <option value="3">Ambos</option>
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Status</label>
              <select className="w-full px-3 py-2 border rounded" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">Todos</option>
                <option value="true">Ativo</option>
                <option value="false">Inativo</option>
              </select>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Usuários ({filtered.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {filtered.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">Nenhum usuário encontrado.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Data Criação</TableHead>
                  <TableHead>Último Acesso</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered.map((usuario) => (
                  <TableRow key={usuario.id}>
                    <TableCell className="font-medium">{usuario.nome}</TableCell>
                    <TableCell>{usuario.email}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs font-medium ${TIPO_USUARIO_COLORS[usuario.tipoUsuario] || 'bg-gray-100 text-gray-800'}`}>
                        {TIPO_USUARIO_LABELS[usuario.tipoUsuario] || usuario.tipoUsuarioDescricao}
                      </span>
                    </TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded text-xs font-medium ${usuario.ativo ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {usuario.ativo ? 'Ativo' : 'Inativo'}
                      </span>
                    </TableCell>
                    <TableCell>{usuario.dataCriacao ? new Date(usuario.dataCriacao).toLocaleDateString('pt-BR') : '-'}</TableCell>
                    <TableCell>{usuario.ultimoAcesso ? new Date(usuario.ultimoAcesso).toLocaleString('pt-BR') : 'Nunca'}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-1">
                        <Button variant="ghost" size="sm" onClick={() => handleToggleAtivo(usuario)} title={usuario.ativo ? 'Desativar' : 'Ativar'}>
                          {usuario.ativo ? <UserX className="h-4 w-4 text-red-600" /> : <UserCheck className="h-4 w-4 text-green-600" />}
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleEdit(usuario.id)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(usuario.id)}>
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

      {showForm && (
        <UsuarioForm
          id={editingId}
          onClose={handleCloseForm}
          onSuccess={handleFormSuccess}
        />
      )}
    </div>
  );
}







