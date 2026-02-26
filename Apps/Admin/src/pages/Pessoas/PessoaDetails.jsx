import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Edit, Phone, Mail, Plus, X, UserPlus, Users, CalendarClock, LogIn } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { useConfirmDialog } from '@/hooks/useConfirmDialog';
import { pessoasApi, pessoasPerfisApi, visitantesApi } from '@/lib/api';
import { getApiErrorMessage } from '@/lib/apiError';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';
import { toast } from 'sonner';

export default function PessoaDetails() {
  const { id } = useParams();
  const [dados360, setDados360] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAddPerfil, setShowAddPerfil] = useState(false);
  const [showAddVisita, setShowAddVisita] = useState(false);
  const [saving, setSaving] = useState(false);
  const { can } = useAuth();
  const confirmDialog = useConfirmDialog();

  // Formulário de perfil
  const [perfilForm, setPerfilForm] = useState({
    perfil: '',
    dataInicio: new Date().toISOString().split('T')[0],
  });

  // Formulário de visita
  const [visitaForm, setVisitaForm] = useState({
    dataVisita: new Date().toISOString().split('T')[0],
    observacoes: '',
  });

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await pessoasApi.get360(id);
      setDados360(response.data);
    } catch (err) {
      setError('Erro ao carregar dados da pessoa');
      console.error('Erro ao carregar dados:', err);
      toast.error('Erro ao carregar dados da pessoa');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [id]);

  const handleAddPerfil = async () => {
    if (!perfilForm.perfil) {
      toast.error('Selecione um perfil');
      return;
    }

    try {
      setSaving(true);
      await pessoasPerfisApi.create({
        pessoaId: parseInt(id),
        perfil: perfilForm.perfil,
        dataInicio: new Date(perfilForm.dataInicio + 'T00:00:00').toISOString(),
      });
      toast.success('Perfil adicionado com sucesso');
      setShowAddPerfil(false);
      setPerfilForm({ perfil: '', dataInicio: new Date().toISOString().split('T')[0] });
      await loadData();
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao adicionar perfil'));
      console.error('Erro ao adicionar perfil:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleEncerrarPerfil = async (perfilId) => {
    const perfis = dados360?.pessoa?.perfis ?? [];
    const perfil = perfis.find((p) => p.id === perfilId);
    confirmDialog.show({
      title: 'Encerrar perfil?',
      description: `Deseja encerrar o perfil "${perfil?.perfil || 'este perfil'}" agora?`,
      confirmText: 'Encerrar',
      cancelText: 'Cancelar',
      variant: 'default',
      onConfirm: async () => {
        try {
          await pessoasPerfisApi.update(perfilId, {
            dataFim: new Date().toISOString(),
          });
          toast.success('Perfil encerrado com sucesso');
          await loadData();
        } catch (err) {
          toast.error(getApiErrorMessage(err, 'Erro ao encerrar perfil'));
          console.error('Erro ao encerrar perfil:', err);
          throw err;
        }
      },
    });
  };

  const handleRemoverPerfil = async (perfilId) => {
    const perfis = dados360?.pessoa?.perfis ?? [];
    const perfil = perfis.find((p) => p.id === perfilId);
    confirmDialog.show({
      title: 'Remover perfil?',
      description: `Tem certeza que deseja remover o perfil "${perfil?.perfil || 'este perfil'}"? Essa ação não pode ser desfeita.`,
      confirmText: 'Remover',
      cancelText: 'Cancelar',
      variant: 'destructive',
      onConfirm: async () => {
        try {
          await pessoasPerfisApi.delete(perfilId);
          toast.success('Perfil removido com sucesso');
          await loadData();
        } catch (err) {
          toast.error(getApiErrorMessage(err, 'Erro ao remover perfil'));
          console.error('Erro ao remover perfil:', err);
          throw err;
        }
      },
    });
  };

  const handleAddVisita = async () => {
    if (!visitaForm.dataVisita || !dados360?.pessoa) {
      toast.error('Data da visita é obrigatória');
      return;
    }

    try {
      setSaving(true);
      await visitantesApi.create({
        nome: dados360.pessoa.nome,
        email: dados360.pessoa.email,
        telefone: dados360.pessoa.telefone,
        whatsApp: dados360.pessoa.whatsApp,
        dataNascimento: dados360.pessoa.dataNascimento,
        dataVisita: new Date(visitaForm.dataVisita + 'T00:00:00').toISOString(),
        observacoes: visitaForm.observacoes || null,
      });
      toast.success('Visita registrada com sucesso');
      setShowAddVisita(false);
      setVisitaForm({ dataVisita: new Date().toISOString().split('T')[0], observacoes: '' });
      await loadData();
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'Erro ao registrar visita'));
      console.error('Erro ao registrar visita:', err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <LoadingPage text="Carregando pessoa..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadData} />;
  }

  if (!dados360?.pessoa) {
    return <ErrorPage message="Pessoa não encontrada" />;
  }

  const pessoa = dados360.pessoa;
  const perfis = pessoa.perfis ?? [];
  const perfisAtivos = perfis.filter(p => !p.dataFim);
  const perfisHistorico = perfis.filter(p => p.dataFim);
  const visitantes = dados360.visitantes ?? [];
  const voluntarios = dados360.voluntarios ?? [];
  const usuario = dados360.usuario;
  const canCreateUsuario = can(RESOURCES.USUARIOS, ACTIONS.EDIT);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" asChild>
            <Link to="/pessoas">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Voltar
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">{pessoa.nome}</h1>
            <p className="text-muted-foreground">
              Visão 360° — perfis, visitas, voluntariado e acesso
            </p>
          </div>
        </div>
        <div className="flex items-center space-x-2">
          {canCreateUsuario && !usuario && (
            <Button variant="outline" asChild>
              <Link to={`/usuarios?pessoaId=${id}`}>
                <UserPlus className="h-4 w-4 mr-2" />
                Criar Acesso
              </Link>
            </Button>
          )}
          <Dialog open={showAddVisita} onOpenChange={setShowAddVisita}>
            <DialogTrigger asChild>
              <Button variant="outline">
                <UserPlus className="h-4 w-4 mr-2" />
                Adicionar Visita
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Adicionar Visita (Visitante)</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="dataVisita">Data da Visita *</Label>
                  <Input
                    id="dataVisita"
                    type="date"
                    value={visitaForm.dataVisita}
                    onChange={(e) => setVisitaForm(prev => ({ ...prev, dataVisita: e.target.value }))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="observacoes">Observações</Label>
                  <Input
                    id="observacoes"
                    value={visitaForm.observacoes}
                    onChange={(e) => setVisitaForm(prev => ({ ...prev, observacoes: e.target.value }))}
                    placeholder="Observações sobre a visita..."
                  />
                </div>
                <div className="flex justify-end space-x-2">
                  <Button variant="outline" onClick={() => setShowAddVisita(false)}>
                    Cancelar
                  </Button>
                  <Button onClick={handleAddVisita} disabled={saving}>
                    {saving ? 'Salvando...' : 'Registrar Visita'}
                  </Button>
                </div>
              </div>
            </DialogContent>
          </Dialog>
          <Button asChild>
            <Link to={`/pessoas/${id}/editar`}>
              <Edit className="h-4 w-4 mr-2" />
              Editar
            </Link>
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Informações Pessoais</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium">Nome</p>
              <p className="text-sm text-muted-foreground">{pessoa.nome}</p>
            </div>
            {pessoa.email && (
              <div className="flex items-center space-x-2">
                <div>
                  <p className="text-sm font-medium">Email</p>
                  <p className="text-sm text-muted-foreground">{pessoa.email}</p>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(`mailto:${pessoa.email}`)}
                >
                  <Mail className="h-4 w-4" />
                </Button>
              </div>
            )}
            {pessoa.telefone && (
              <div>
                <p className="text-sm font-medium">Telefone</p>
                <p className="text-sm text-muted-foreground">{pessoa.telefone}</p>
              </div>
            )}
            {pessoa.whatsApp && (
              <div className="flex items-center space-x-2">
                <div>
                  <p className="text-sm font-medium">WhatsApp</p>
                  <p className="text-sm text-muted-foreground">{pessoa.whatsApp}</p>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(`https://wa.me/55${pessoa.whatsApp.replace(/\D/g, '')}`)}
                >
                  <Phone className="h-4 w-4" />
                </Button>
              </div>
            )}
            {pessoa.dataNascimento && (
              <div>
                <p className="text-sm font-medium">Data de Nascimento</p>
                <p className="text-sm text-muted-foreground">
                  {new Date(pessoa.dataNascimento).toLocaleDateString('pt-BR')}
                </p>
              </div>
            )}
            <div>
              <p className="text-sm font-medium">Tipo de Pessoa</p>
              <Badge variant="outline">{pessoa.tipoPessoa || '-'}</Badge>
            </div>
            <div>
              <p className="text-sm font-medium">Status</p>
              <Badge variant={pessoa.ativo ? 'default' : 'secondary'}>
                {pessoa.ativo ? 'Ativo' : 'Inativo'}
              </Badge>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Perfis</span>
              <Dialog open={showAddPerfil} onOpenChange={setShowAddPerfil}>
                <DialogTrigger asChild>
                  <Button size="sm">
                    <Plus className="h-4 w-4 mr-2" />
                    Adicionar Perfil
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Adicionar Perfil</DialogTitle>
                  </DialogHeader>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label htmlFor="perfil">Perfil *</Label>
                      <Select
                        value={perfilForm.perfil}
                        onValueChange={(value) => setPerfilForm(prev => ({ ...prev, perfil: value }))}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Selecione um perfil" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Visitante">Visitante</SelectItem>
                          <SelectItem value="Membro">Membro</SelectItem>
                          <SelectItem value="Voluntario">Voluntário</SelectItem>
                          <SelectItem value="Lider">Líder</SelectItem>
                          <SelectItem value="Pastor">Pastor</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="dataInicio">Data de Início</Label>
                      <Input
                        id="dataInicio"
                        type="date"
                        value={perfilForm.dataInicio}
                        onChange={(e) => setPerfilForm(prev => ({ ...prev, dataInicio: e.target.value }))}
                      />
                    </div>
                    <div className="flex justify-end space-x-2">
                      <Button variant="outline" onClick={() => setShowAddPerfil(false)}>
                        Cancelar
                      </Button>
                      <Button onClick={handleAddPerfil} disabled={saving}>
                        {saving ? 'Salvando...' : 'Adicionar'}
                      </Button>
                    </div>
                  </div>
                </DialogContent>
              </Dialog>
            </CardTitle>
          </CardHeader>
          <CardContent>
            {perfisAtivos.length > 0 && (
              <div className="space-y-4">
                <div>
                  <p className="text-sm font-medium mb-2">Perfis Ativos</p>
                  <div className="space-y-2">
                    {perfisAtivos.map((perfil) => (
                      <div key={perfil.id} className="flex items-center justify-between p-2 border rounded">
                        <div>
                          <Badge variant="default">{perfil.perfil}</Badge>
                          <p className="text-xs text-muted-foreground mt-1">
                            Desde {new Date(perfil.dataInicio).toLocaleDateString('pt-BR')}
                          </p>
                        </div>
                        <div className="flex space-x-1">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleEncerrarPerfil(perfil.id)}
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {perfisHistorico.length > 0 && (
              <div className="mt-4">
                <p className="text-sm font-medium mb-2">Histórico de Perfis</p>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Perfil</TableHead>
                      <TableHead>Início</TableHead>
                      <TableHead>Fim</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {perfisHistorico.map((perfil) => (
                      <TableRow key={perfil.id}>
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
                        <TableCell className="text-right">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleRemoverPerfil(perfil.id)}
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}

            {perfis.length === 0 && (
              <p className="text-sm text-muted-foreground text-center py-4">
                Nenhum perfil cadastrado.
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Histórico de Visitas */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CalendarClock className="h-5 w-5" />
            Histórico de Visitas ({visitantes.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {visitantes.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-4">
              Nenhuma visita registrada.
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Data</TableHead>
                  <TableHead>Observações</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {visitantes.map((v) => (
                  <TableRow key={v.id}>
                    <TableCell>
                      {v.dataVisita ? new Date(v.dataVisita).toLocaleDateString('pt-BR') : '-'}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {v.observacoes ? (v.observacoes.length > 60 ? v.observacoes.slice(0, 60) + '...' : v.observacoes) : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button variant="ghost" size="sm" asChild>
                        <Link to={`/visitantes/${v.id}`}>Ver</Link>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Voluntariado */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Voluntariado ({voluntarios.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {voluntarios.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-4">
              Nenhum vínculo de voluntariado.
            </p>
          ) : (
            <div className="space-y-2">
              {voluntarios.map((v) => (
                <div key={v.id} className="flex items-center justify-between p-3 border rounded">
                  <div>
                    <span className="font-medium">{v.nomeEquipe}</span>
                    <span className="text-muted-foreground mx-2">—</span>
                    <span>{v.nomeCargo}</span>
                  </div>
                  <Button variant="ghost" size="sm" asChild>
                    <Link to={`/voluntarios/${v.id}/editar`}>Editar</Link>
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Acesso ao Sistema */}
      {usuario && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <LogIn className="h-5 w-5" />
              Acesso ao Sistema
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium">{usuario.emailLogin}</p>
                <p className="text-xs text-muted-foreground">
                  {usuario.tipoUsuarioDescricao}
                  {usuario.perfilAcessoNome && ` • ${usuario.perfilAcessoNome}`}
                  {usuario.ultimoAcesso && ` • Último acesso: ${new Date(usuario.ultimoAcesso).toLocaleString('pt-BR')}`}
                </p>
              </div>
              <Badge variant={usuario.ativo ? 'default' : 'secondary'}>
                {usuario.ativo ? 'Ativo' : 'Inativo'}
              </Badge>
            </div>
            {canCreateUsuario && (
              <Button variant="outline" size="sm" asChild>
                <Link to={`/usuarios?pessoaId=${id}`}>
                  Gerenciar usuário
                </Link>
              </Button>
            )}
          </CardContent>
        </Card>
      )}

      <ConfirmDialog
        open={confirmDialog.open}
        onOpenChange={(open) => {
          if (!open) confirmDialog.hide();
        }}
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




