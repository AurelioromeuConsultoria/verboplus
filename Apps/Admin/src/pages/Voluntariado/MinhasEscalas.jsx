import { useEffect, useState } from 'react';
import { CalendarDays, CheckCircle2, Clock3, XCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { escalasApi, solicitacoesTrocasEscalasApi } from '@/lib/api';
import { toast } from 'sonner';

function getEscalaItemStatusLabel(status) {
  const value = Number(status);
  if (value === 1) return 'Pendente';
  if (value === 2) return 'Confirmado';
  if (value === 3) return 'Recusado';
  if (value === 4) return 'Substituído';
  if (value === 5) return 'Serviu';
  if (value === 6) return 'Faltou';
  return 'Desconhecido';
}

function getActionButtonProps(item, action) {
  const status = Number(item.status);

  if (action === 'confirmar') {
    return status === 2
      ? {
          label: 'Confirmado',
          className: '!border-emerald-600 !bg-emerald-600 !text-white hover:!bg-emerald-700 hover:!text-white',
        }
      : {
          label: 'Confirmar',
          className: '',
        };
  }

  if (action === 'recusar') {
    return status === 3
      ? {
          label: 'Recusado',
          className: '!border-rose-600 !bg-rose-600 !text-white hover:!bg-rose-700 hover:!text-white',
        }
      : {
          label: 'Recusar',
          className: '',
        };
  }

  return {
    label: '',
    className: '',
  };
}

export default function MinhasEscalas() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [somenteFuturas, setSomenteFuturas] = useState('true');
  const [escalas, setEscalas] = useState([]);
  const [solicitacoes, setSolicitacoes] = useState([]);
  const [trocaModalOpen, setTrocaModalOpen] = useState(false);
  const [trocaItem, setTrocaItem] = useState(null);
  const [trocaMotivo, setTrocaMotivo] = useState('');

  const load = async (futureOnly = somenteFuturas) => {
    try {
      setLoading(true);
      setError(null);
      const [escalasRes, solicitacoesRes] = await Promise.all([
        escalasApi.getMinhas({ somenteFuturas: futureOnly === 'true' }),
        solicitacoesTrocasEscalasApi.getMinhas(),
      ]);
      setEscalas(escalasRes.data || []);
      setSolicitacoes(solicitacoesRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar suas escalas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [somenteFuturas]);

  const handleConfirmar = async (escalaId, itemId) => {
    try {
      await escalasApi.confirmarItem(escalaId, itemId);
      toast.success('Escala confirmada');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao confirmar escala');
      toast.error(message);
    }
  };

  const handleRecusar = async (escalaId, item) => {
    const motivoRecusa = window.prompt(`Motivo da recusa para ${item.equipeNome}:`, item.motivoRecusa || '');
    if (motivoRecusa === null) return;

    try {
      await escalasApi.recusarItem(escalaId, item.id, { motivoRecusa });
      toast.success('Recusa registrada');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao recusar escala');
      toast.error(message);
    }
  };

  const openSolicitarTroca = (escalaId, item) => {
    setTrocaItem({ escalaId, item });
    setTrocaMotivo('');
    setTrocaModalOpen(true);
  };

  const handleSolicitarTroca = async () => {
    if (!trocaItem) return;
    try {
      await solicitacoesTrocasEscalasApi.create(trocaItem.escalaId, trocaItem.item.id, { motivo: trocaMotivo });
      toast.success('Solicitação de troca enviada');
      setTrocaModalOpen(false);
      setTrocaItem(null);
      setTrocaMotivo('');
      await load();
    } catch (err) {
      console.error(err);
      const message = typeof err.response?.data === 'string'
        ? err.response.data
        : (err.response?.data?.message || 'Erro ao solicitar troca');
      toast.error(message);
    }
  };

  if (loading) return <LoadingPage text="Carregando suas escalas..." />;
  if (error) return <ErrorPage message={error} onRetry={() => load()} />;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold">Minhas Escalas</h1>
          <p className="text-muted-foreground">
            Consulte suas escalas e confirme ou recuse sua participação.
          </p>
        </div>

        <div className="w-[220px]">
          <Select value={somenteFuturas} onValueChange={setSomenteFuturas}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="true">Somente futuras</SelectItem>
              <SelectItem value="false">Todas</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {escalas.length === 0 ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            Nenhuma escala encontrada.
          </CardContent>
        </Card>
      ) : (
        escalas.map((escala) => (
          <Card key={escala.id}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CalendarDays className="h-5 w-5" />
                {escala.eventoTitulo} - {escala.equipeNome}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="text-sm text-muted-foreground">
                {new Date(escala.eventoDataHoraInicio).toLocaleString('pt-BR')}
              </div>

              {escala.itens.map((item) => (
                <div key={item.id} className="flex items-center justify-between rounded-lg border p-4 gap-4">
                  {(() => {
                    const confirmarButton = getActionButtonProps(item, 'confirmar');
                    const recusarButton = getActionButtonProps(item, 'recusar');

                    return (
                      <>
                  <div className="space-y-1">
                    <div className="font-medium">{item.cargoNome || 'Sem cargo definido'}</div>
                    <div className="text-sm text-muted-foreground">
                      Status: {getEscalaItemStatusLabel(item.status)}
                    </div>
                    {item.motivoRecusa && (
                      <div className="text-sm text-red-600">
                        Motivo da recusa: {item.motivoRecusa}
                      </div>
                    )}
                  </div>

                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      className={confirmarButton.className}
                      onClick={() => handleConfirmar(escala.id, item.id)}
                    >
                      <CheckCircle2 className="h-4 w-4 mr-2" />
                      {confirmarButton.label}
                    </Button>
                    <Button
                      variant="outline"
                      className={recusarButton.className}
                      onClick={() => handleRecusar(escala.id, item)}
                    >
                      <XCircle className="h-4 w-4 mr-2" />
                      {recusarButton.label}
                    </Button>
                    <Button variant="outline" onClick={() => openSolicitarTroca(escala.id, item)}>
                      Solicitar troca
                    </Button>
                    {Number(item.status) === 1 && (
                      <span className="inline-flex items-center text-sm text-amber-600">
                        <Clock3 className="h-4 w-4 mr-1" />
                        Pendente
                      </span>
                    )}
                  </div>
                      </>
                    );
                  })()}
                </div>
              ))}
            </CardContent>
          </Card>
        ))
      )}

      <Card>
        <CardHeader>
          <CardTitle>Minhas solicitações de troca</CardTitle>
        </CardHeader>
        <CardContent>
          {solicitacoes.length === 0 ? (
            <div className="text-sm text-muted-foreground">Nenhuma solicitação registrada.</div>
          ) : (
            <div className="space-y-3">
              {solicitacoes.map((solicitacao) => (
                <div key={solicitacao.id} className="rounded-lg border p-4">
                  <div className="font-medium">{solicitacao.equipeNome}</div>
                  <div className="text-sm text-muted-foreground">
                    Status: {solicitacao.status === 1 ? 'Pendente' : solicitacao.status === 2 ? 'Aprovada' : 'Rejeitada'}
                  </div>
                  <div className="text-sm">Motivo: {solicitacao.motivo || '-'}</div>
                  {solicitacao.voluntarioSubstitutoNome && (
                    <div className="text-sm">Substituto: {solicitacao.voluntarioSubstitutoNome}</div>
                  )}
                  {solicitacao.observacaoResposta && (
                    <div className="text-sm">Resposta: {solicitacao.observacaoResposta}</div>
                  )}
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={trocaModalOpen} onOpenChange={setTrocaModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Solicitar troca</DialogTitle>
            <DialogDescription>
              Descreva o motivo da troca para ajudar o líder a tomar a decisão.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-2">
            <Textarea
              value={trocaMotivo}
              onChange={(e) => setTrocaMotivo(e.target.value)}
              placeholder="Ex.: estarei viajando / estou indisponível nesse horário"
              rows={4}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setTrocaModalOpen(false)}>Cancelar</Button>
            <Button onClick={handleSolicitarTroca}>Enviar solicitação</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
