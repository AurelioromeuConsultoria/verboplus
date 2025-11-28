import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Edit, Trash2, CheckCircle, XCircle, Phone, Mail, Calendar, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { inscricoesEventosApi } from '@/lib/api';
import { useNavigate } from 'react-router-dom';

const STATUS_LABELS = {
  1: 'Pendente',
  2: 'Confirmada',
  3: 'Cancelada',
  4: 'Presente',
};

const STATUS_COLORS = {
  1: 'bg-yellow-100 text-yellow-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-blue-100 text-blue-800',
};

export default function InscricaoEventoDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [inscricao, setInscricao] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await inscricoesEventosApi.getById(id);
      setInscricao(res.data);
    } catch (err) {
      setError('Erro ao carregar inscrição');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);

  const handleDelete = async () => {
    if (!confirm('Tem certeza que deseja excluir esta inscrição?')) return;
    try {
      await inscricoesEventosApi.delete(id);
      navigate('/inscricoes-eventos');
    } catch (err) {
      alert('Erro ao excluir inscrição');
      console.error(err);
    }
  };

  const handleConfirmar = async () => {
    try {
      await inscricoesEventosApi.confirmar(id);
      await load();
    } catch (err) {
      alert('Erro ao confirmar inscrição');
      console.error(err);
    }
  };

  const handleCancelar = async () => {
    if (!confirm('Tem certeza que deseja cancelar esta inscrição?')) return;
    try {
      await inscricoesEventosApi.cancelar(id);
      await load();
    } catch (err) {
      alert('Erro ao cancelar inscrição');
      console.error(err);
    }
  };

  if (loading) return <LoadingPage text="Carregando inscrição..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!inscricao) return <div>Inscrição não encontrada</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" asChild>
            <Link to="/inscricoes-eventos">
              <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Detalhes da Inscrição</h1>
            <p className="text-muted-foreground">Informações completas da inscrição</p>
          </div>
        </div>
        <div className="flex items-center space-x-2">
          {inscricao.status === 1 && (
            <Button onClick={handleConfirmar} variant="outline">
              <CheckCircle className="h-4 w-4 mr-2" /> Confirmar
            </Button>
          )}
          {(inscricao.status === 1 || inscricao.status === 2) && (
            <Button onClick={handleCancelar} variant="outline">
              <XCircle className="h-4 w-4 mr-2" /> Cancelar
            </Button>
          )}
          <Button variant="outline" asChild>
            <Link to={`/inscricoes-eventos/${id}/editar`}>
              <Edit className="h-4 w-4 mr-2" /> Editar
            </Link>
          </Button>
          <Button variant="destructive" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" /> Excluir
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Dados do Participante</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Nome</label>
              <p className="text-base font-medium">{inscricao.nome}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">WhatsApp</label>
              <div className="flex items-center space-x-2">
                <p className="text-base">{inscricao.whatsApp}</p>
                {inscricao.whatsApp && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => window.open(`https://wa.me/55${inscricao.whatsApp.replace(/\D/g, '')}`)}
                  >
                    <Phone className="h-4 w-4" />
                  </Button>
                )}
              </div>
            </div>
            {inscricao.email && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Email</label>
                <div className="flex items-center space-x-2">
                  <p className="text-base">{inscricao.email}</p>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => window.open(`mailto:${inscricao.email}`)}
                  >
                    <Mail className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            )}
            <div>
              <label className="text-sm font-medium text-muted-foreground">Status</label>
              <div>
                <span className={`px-3 py-1 rounded text-sm font-medium ${STATUS_COLORS[inscricao.status] || 'bg-gray-100 text-gray-800'}`}>
                  {STATUS_LABELS[inscricao.status] || inscricao.statusDescricao}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Informações do Evento</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Evento</label>
              <p className="text-base font-medium">{inscricao.eventoTitulo || '-'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Quantidade de Acompanhantes</label>
              <div className="flex items-center space-x-2">
                <Users className="h-4 w-4" />
                <p className="text-base">{inscricao.quantidadeAcompanhantes || 0}</p>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Data de Inscrição</label>
              <div className="flex items-center space-x-2">
                <Calendar className="h-4 w-4" />
                <p className="text-base">{inscricao.dataInscricao ? new Date(inscricao.dataInscricao).toLocaleString('pt-BR') : '-'}</p>
              </div>
            </div>
            {inscricao.dataConfirmacao && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Data de Confirmação</label>
                <p className="text-base">{new Date(inscricao.dataConfirmacao).toLocaleString('pt-BR')}</p>
              </div>
            )}
            {inscricao.dataCancelamento && (
              <div>
                <label className="text-sm font-medium text-muted-foreground">Data de Cancelamento</label>
                <p className="text-base">{new Date(inscricao.dataCancelamento).toLocaleString('pt-BR')}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {inscricao.observacoes && (
        <Card>
          <CardHeader>
            <CardTitle>Observações do Participante</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-base whitespace-pre-wrap">{inscricao.observacoes}</p>
          </CardContent>
        </Card>
      )}

      {inscricao.observacoesInternas && (
        <Card>
          <CardHeader>
            <CardTitle>Observações Internas</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-base whitespace-pre-wrap">{inscricao.observacoesInternas}</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

