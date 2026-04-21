import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Users, MessageSquare, Calendar, CheckCircle, User, CalendarDays, ClipboardList, Handshake } from 'lucide-react';
import { dashboardApi } from '@/lib/api';
import { ErrorPage } from '@/components/ui/error-message';
import { Skeleton } from '@/components/ui/skeleton';
import { useTranslation } from 'react-i18next';
import { formatDate } from '@/lib/formatters';

export default function Dashboard() {
  const { t } = useTranslation();
  const [estatisticas, setEstatisticas] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadEstatisticas();
  }, []);

  const loadEstatisticas = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await dashboardApi.getEstatisticas();
      setEstatisticas(response.data);
    } catch (err) {
      console.error('Error loading dashboard statistics:', err);
      setError(t('dashboard.errorLoad'));
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div>
          <Skeleton className="h-9 w-48 mb-2" />
          <Skeleton className="h-5 w-96" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <Card key={i}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-4 w-4 rounded" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-8 w-16 mb-2" />
                <Skeleton className="h-3 w-24" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return <ErrorPage message={error} onRetry={loadEstatisticas} />;
  }

  const stats = estatisticas || {
    totalVisitantes: 0,
    mensagensAgendadas: 0,
    mensagensEnviadas: 0,
    configuracoesAtivas: 0,
    totalPessoas: 0,
    totalEventos: 0,
    totalInscricoes: 0,
    totalVoluntarios: 0,
    totalAniversariantesProximos: 0,
    proximosAniversariantes: [],
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">{t('dashboard.title')}</h1>
        <p className="text-muted-foreground">
          {t('dashboard.subtitle')}
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.totalVisitors.title')}
            </CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalVisitantes}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.totalVisitors.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.scheduledMessages.title')}
            </CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.mensagensAgendadas}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.scheduledMessages.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.sentMessages.title')}
            </CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.mensagensEnviadas}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.sentMessages.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.activeSettings.title')}
            </CardTitle>
            <MessageSquare className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.configuracoesAtivas}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.activeSettings.description')}
            </p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.totalPeople.title')}
            </CardTitle>
            <User className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalPessoas}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.totalPeople.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.totalEvents.title')}
            </CardTitle>
            <CalendarDays className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalEventos}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.totalEvents.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.totalRegistrations.title')}
            </CardTitle>
            <ClipboardList className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalInscricoes}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.totalRegistrations.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.totalVolunteers.title')}
            </CardTitle>
            <Handshake className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalVoluntarios}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.totalVolunteers.description')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {t('dashboard.cards.upcomingBirthdays.title')}
            </CardTitle>
            <CalendarDays className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalAniversariantesProximos}</div>
            <p className="text-xs text-muted-foreground">
              {t('dashboard.cards.upcomingBirthdays.description')}
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t('dashboard.birthdays.title')}</CardTitle>
        </CardHeader>
        <CardContent>
          {stats.proximosAniversariantes?.length ? (
            <div className="space-y-3">
              {stats.proximosAniversariantes.map((p) => (
                <div key={p.id} className="flex items-center justify-between border-b pb-2 last:border-b-0 last:pb-0">
                  <div>
                    <div className="font-medium">{p.nome}</div>
                    <div className="text-xs text-muted-foreground">
                      {t('dashboard.birthdays.nextBirthday')}: {formatDate(p.proximoAniversario)}
                    </div>
                  </div>
                  <div className="text-sm font-semibold">{t('dashboard.birthdays.days', { count: p.diasParaAniversario })}</div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-sm text-muted-foreground">{t('dashboard.birthdays.empty')}</div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('dashboard.welcome.title')}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">
            {t('dashboard.welcome.description')}
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
