import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Edit, Phone, Mail, Calendar, MessageSquare } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { visitantesApi, mensagensAgendadasApi } from '@/lib/api';

export function VisitanteDetails() {
  const { id } = useParams();
  const [visitante, setVisitante] = useState(null);
  const [mensagens, setMensagens] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const visitanteResponse = await visitantesApi.getById(id);
      const visitante = visitanteResponse.data;
      
      setVisitante(visitante);
      
      // Tentar carregar mensagens se o endpoint existir
      try {
        const mensagensResponse = await mensagensAgendadasApi.getAll();
        const mensagensDoVisitante = mensagensResponse.data.filter(
          msg => msg.visitanteId === parseInt(id)
        );
        setMensagens(mensagensDoVisitante);
      } catch (err) {
        // Ignorar erro se o endpoint não existir
        console.log('Mensagens não disponíveis');
      }
    } catch (err) {
      setError('Erro ao carregar dados do visitante');
      console.error('Erro ao carregar dados:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status) => {
    const statusConfig = {
      'Agendada': { variant: 'secondary', text: 'Agendada' },
      'Enviada': { variant: 'default', text: 'Enviada' },
      'Erro': { variant: 'destructive', text: 'Erro' },
      'Cancelada': { variant: 'outline', text: 'Cancelada' }
    };

    const config = statusConfig[status] || { variant: 'secondary', text: status };
    return <Badge variant={config.variant}>{config.text}</Badge>;
  };

  useEffect(() => {
    loadData();
  }, [id]);

  if (loading) {
    return <LoadingPage text="Carregando visitante..." />;
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadData} />;
  }

  if (!visitante) {
    return <ErrorPage message="Visitante não encontrado" />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" asChild>
            <Link to="/visitantes">
              <ArrowLeft className="h-4 w-4 mr-2" />
              Voltar
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold">
              {visitante.pessoa?.nome || visitante.nome || 'Visitante'}
            </h1>
            <p className="text-muted-foreground">
              Detalhes da visita
            </p>
          </div>
        </div>
        <Button asChild>
          <Link to={`/visitantes/${id}/editar`}>
            <Edit className="h-4 w-4 mr-2" />
            Editar
          </Link>
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Dados da Visita</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center space-x-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                <Calendar className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-sm font-medium">Data da Visita</p>
                <p className="text-sm text-muted-foreground">
                  {new Date(visitante.dataVisita).toLocaleDateString('pt-BR')}
                </p>
              </div>
            </div>

            {visitante.observacoes && (
              <div>
                <p className="text-sm font-medium mb-2">Observações</p>
                <p className="text-sm text-muted-foreground bg-muted p-3 rounded-lg">
                  {visitante.observacoes}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Dados da Pessoa</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium">Nome</p>
              <p className="text-sm text-muted-foreground">
                {visitante.pessoa?.nome || visitante.nome || '-'}
              </p>
            </div>

            {visitante.pessoa?.email && (
              <div className="flex items-center space-x-2">
                <div>
                  <p className="text-sm font-medium">Email</p>
                  <p className="text-sm text-muted-foreground">{visitante.pessoa.email}</p>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(`mailto:${visitante.pessoa.email}`)}
                >
                  <Mail className="h-4 w-4" />
                </Button>
              </div>
            )}

            {visitante.pessoa?.telefone && (
              <div>
                <p className="text-sm font-medium">Telefone</p>
                <p className="text-sm text-muted-foreground">{visitante.pessoa.telefone}</p>
              </div>
            )}

            {visitante.pessoa?.whatsApp && (
              <div className="flex items-center space-x-2">
                <div>
                  <p className="text-sm font-medium">WhatsApp</p>
                  <p className="text-sm text-muted-foreground">{visitante.pessoa.whatsApp}</p>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => window.open(`https://wa.me/55${visitante.pessoa.whatsApp.replace(/\D/g, '')}`)}
                >
                  <Phone className="h-4 w-4" />
                </Button>
              </div>
            )}

            {visitante.pessoa?.perfis && visitante.pessoa.perfis.length > 0 && (
              <div>
                <p className="text-sm font-medium mb-2">Perfis</p>
                <div className="flex flex-wrap gap-1">
                  {visitante.pessoa.perfis
                    .filter(p => !p.dataFim)
                    .map((perfil, idx) => (
                      <Badge key={idx} variant="secondary">
                        {perfil.perfil}
                      </Badge>
                    ))}
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <MessageSquare className="h-5 w-5" />
              <span>Mensagens Agendadas</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            {mensagens.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">
                Nenhuma mensagem agendada para este visitante.
              </p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Tipo</TableHead>
                    <TableHead>Data de Envio</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mensagens.map((mensagem) => (
                    <TableRow key={mensagem.id}>
                      <TableCell className="font-medium">
                        {mensagem.configuracaoMensagem?.titulo || 'Mensagem'}
                      </TableCell>
                      <TableCell>
                        {new Date(mensagem.dataEnvio).toLocaleDateString('pt-BR')}
                      </TableCell>
                      <TableCell>
                        {getStatusBadge(mensagem.status)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

