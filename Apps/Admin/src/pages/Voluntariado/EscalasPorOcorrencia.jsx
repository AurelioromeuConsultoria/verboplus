import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, PlusCircle, Settings } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';
import { eventosOcorrenciasApi, escalasApi, equipesApi } from '@/lib/api';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES, ACTIONS } from '@/utils/permissions';

function getEscalaStatusLabel(status) {
  const v = Number(status);
  if (v === 1) return 'Rascunho';
  if (v === 2) return 'Publicada';
  if (v === 3) return 'Fechada';
  return '—';
}

export default function EscalasPorOcorrencia() {
  const { ocorrenciaId } = useParams();
  const { can } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [ocorrencia, setOcorrencia] = useState(null);
  const [escalas, setEscalas] = useState([]);
  const [equipes, setEquipes] = useState([]);

  const canEdit = can(RESOURCES.VOLUNTARIOS, ACTIONS.EDIT);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const [ocRes, escRes, eqRes] = await Promise.all([
        eventosOcorrenciasApi.getById(ocorrenciaId),
        escalasApi.getAllByOcorrencia(ocorrenciaId),
        equipesApi.getAll(),
      ]);
      setOcorrencia(ocRes.data);
      setEscalas(escRes.data || []);
      setEquipes(eqRes.data || []);
    } catch (err) {
      console.error(err);
      setError('Erro ao carregar dados');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [ocorrenciaId]);

  if (loading) return <LoadingPage text="Carregando..." />;
  if (error) return <ErrorPage message={error} onRetry={load} />;
  if (!ocorrencia) return <ErrorPage message="Ocorrência não encontrada" onRetry={load} />;

  const equipesComEscala = new Set(escalas?.map((e) => e.equipeId) || []);
  const equipesSemEscala = (equipes || []).filter((eq) => !equipesComEscala.has(eq.id));

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" asChild>
          <Link to="/voluntariado/escalas">
            <ArrowLeft className="h-4 w-4 mr-2" /> Voltar
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Escalas por equipe</h1>
          <p className="text-muted-foreground">
            {ocorrencia.eventoTitulo} — {new Date(ocorrencia.dataHoraInicio).toLocaleString('pt-BR')}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Equipes</CardTitle>
          <p className="text-sm text-muted-foreground">
            Cada equipe tem sua própria escala para esta ocorrência. Crie ou edite por equipe. Um voluntário não pode estar em mais de uma equipe na mesma data.
          </p>
        </CardHeader>
        <CardContent className="space-y-4">
          {escalas?.length > 0 && (
            <>
              <h3 className="font-medium">Escalas criadas</h3>
              <ul className="space-y-2">
                {escalas.map((esc) => (
                  <li key={esc.id} className="flex items-center justify-between rounded-lg border p-3">
                    <span className="font-medium">{esc.equipeNome || `Equipe ${esc.equipeId}`}</span>
                    <span className="px-2 py-1 rounded text-xs bg-gray-100 text-gray-800">{getEscalaStatusLabel(esc.status)}</span>
                    {canEdit && (
                      <Button variant="outline" size="sm" asChild>
                        <Link to={`/voluntariado/escalas/ocorrencia/${ocorrenciaId}/equipe/${esc.equipeId}`}>
                          <Settings className="h-4 w-4 mr-2" />
                          Editar escala
                        </Link>
                      </Button>
                    )}
                  </li>
                ))}
              </ul>
            </>
          )}
          {equipesSemEscala?.length > 0 && canEdit && (
            <>
              <h3 className="font-medium pt-2">Criar escala para equipe</h3>
              <ul className="space-y-2">
                {equipesSemEscala.map((eq) => (
                  <li key={eq.id} className="flex items-center justify-between rounded-lg border border-dashed p-3">
                    <span>{eq.nome}</span>
                    <Button size="sm" asChild>
                      <Link to={`/voluntariado/escalas/ocorrencia/${ocorrenciaId}/equipe/${eq.id}`}>
                        <PlusCircle className="h-4 w-4 mr-2" />
                        Criar escala
                      </Link>
                    </Button>
                  </li>
                ))}
              </ul>
            </>
          )}
          {(!escalas || escalas.length === 0) && (!equipesSemEscala || equipesSemEscala.length === 0) && (
            <p className="text-muted-foreground">Nenhuma equipe cadastrada. Cadastre equipes em Voluntariado → Equipes.</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
