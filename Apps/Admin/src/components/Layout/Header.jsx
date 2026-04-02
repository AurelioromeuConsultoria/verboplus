import { Bell, User, LogOut, Settings, Sun, Moon, Globe, CheckCheck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { useTheme } from '@/context/ThemeContext';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Breadcrumb,
  BreadcrumbList,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { notificacoesApi } from '@/lib/api';

// Mapeamento de rotas para chaves de i18n
const routeLabelKeys = {
  '': 'header.dashboard',
  'pessoas': 'header.people',
  'aniversarios-campanha': 'header.aniversarios-campanha',
  'visitantes': 'header.visitors',
  'perfis': 'header.profiles',
  'configuracoes-mensagens': 'header.messageSettings',
  'mensagens-agendadas': 'header.scheduledMessages',
  'equipes': 'header.teams',
  'cargos': 'header.roles',
  'voluntarios': 'header.volunteers',
  'eventos': 'header.events',
  'inscricoes-eventos': 'header.eventRegistrations',
  'patrimonio': 'header.patrimony',
  'categorias': 'header.patrimonyCategories',
  'relatorio-geral': 'header.patrimonyReport',
  'destaques-site': 'header.siteHighlights',
  'categorias-noticias': 'header.newsCategories',
  'noticias': 'header.news',
  'contatos': 'header.contacts',
  'usuarios': 'header.users',
  'perfil': 'header.profile',
  'categorias-midias': 'header.mediaCategories',
  'galerias-fotos': 'header.photoGalleries',
  'novo': 'header.new',
  'editar': 'header.edit',
};

function generateBreadcrumbs(pathname, t, locationState) {
  const paths = pathname.split('/').filter(Boolean);
  const breadcrumbs = [{ label: t('header.dashboard'), path: '/' }];
  const breadcrumbLabels = locationState?.breadcrumbLabels || {};

  if (paths.length === 0) {
    return breadcrumbs;
  }

  let currentPath = '';
  paths.forEach((segment, index) => {
    currentPath += `/${segment}`;
    const isLast = index === paths.length - 1;

    const customLabel = breadcrumbLabels[currentPath];
    if (customLabel) {
      breadcrumbs.push({
        label: customLabel,
        path: isLast ? null : currentPath,
      });
      return;
    }

    if (/^\d+$/.test(segment)) {
      return;
    } else {
      const key = routeLabelKeys[segment];
      const label = key ? t(key) : segment.charAt(0).toUpperCase() + segment.slice(1);
      breadcrumbs.push({
        label,
        path: isLast ? null : currentPath,
      });
    }
  });

  return breadcrumbs;
}

export function Header() {
  const { usuario, logout } = useAuth();
  const { theme, toggleTheme, isDark } = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const { t, i18n } = useTranslation();
  const breadcrumbs = generateBreadcrumbs(location.pathname, t, location.state);
  const [notificacoes, setNotificacoes] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);

  const handleChangeLanguage = (lng) => {
    i18n.changeLanguage(lng);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const truncateText = (text, maxLength = 20) => {
    if (!text) return '';
    return text.length > maxLength ? `${text.substring(0, maxLength)}...` : text;
  };

  const loadNotificacoes = async () => {
    if (!usuario) return;

    try {
      const [itemsRes, countRes] = await Promise.all([
        notificacoesApi.getMinhas({ limit: 5 }),
        notificacoesApi.getUnreadCount(),
      ]);

      setNotificacoes(itemsRes.data || []);
      setUnreadCount(countRes.data?.count || 0);
    } catch (error) {
      console.error('Erro ao carregar notificações', error);
    }
  };

  useEffect(() => {
    loadNotificacoes();
  }, [usuario, location.pathname]);

  const handleNotificationClick = async (notificacao) => {
    try {
      if (!notificacao.dataLeitura) {
        await notificacoesApi.marcarComoLida(notificacao.id);
      }
    } catch (error) {
      console.error('Erro ao marcar notificação como lida', error);
    } finally {
      setNotificacoes((current) =>
        current.map((item) => (item.id === notificacao.id ? { ...item, dataLeitura: item.dataLeitura || new Date().toISOString() } : item))
      );
      setUnreadCount((current) => (notificacao.dataLeitura ? current : Math.max(0, current - 1)));

      if (notificacao.link) {
        navigate(notificacao.link);
      }
    }
  };

  const handleMarkAllNotifications = async () => {
    try {
      await notificacoesApi.marcarTodasComoLidas();
      setNotificacoes((current) => current.map((item) => ({ ...item, dataLeitura: item.dataLeitura || new Date().toISOString() })));
      setUnreadCount(0);
    } catch (error) {
      console.error('Erro ao marcar notificações como lidas', error);
    }
  };

  return (
    <header className="flex h-16 items-center justify-between border-b border-border bg-background px-6">
      <div className="flex items-center space-x-4 flex-1 min-w-0">
        <h1 className="text-xl font-semibold text-foreground hidden md:block">
          {t('app.name')}
        </h1>
        
        {breadcrumbs.length > 1 && (
          <Breadcrumb className="hidden md:flex">
            <BreadcrumbList>
              {breadcrumbs.map((crumb, index) => (
                <div key={index} className="flex items-center">
                  {index > 0 && <BreadcrumbSeparator />}
                  <BreadcrumbItem>
                    {crumb.path ? (
                      <BreadcrumbLink asChild>
                        <Link to={crumb.path}>{crumb.label}</Link>
                      </BreadcrumbLink>
                    ) : (
                      <BreadcrumbPage>{crumb.label}</BreadcrumbPage>
                    )}
                  </BreadcrumbItem>
                </div>
              ))}
            </BreadcrumbList>
          </Breadcrumb>
        )}
      </div>

      <div className="flex items-center space-x-2">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon">
              <Globe className="h-5 w-5" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-40">
            <DropdownMenuLabel>{t('language.label')}</DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => handleChangeLanguage('pt-BR')}>
              {t('language.ptBR')}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => handleChangeLanguage('en-US')}>
              {t('language.enUS')}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => handleChangeLanguage('es-ES')}>
              {t('language.esES')}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button variant="ghost" size="icon" onClick={toggleTheme}>
              {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            {isDark ? t('header.useLightTheme') : t('header.useDarkTheme')}
          </TooltipContent>
        </Tooltip>
        
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="relative">
              <Bell className="h-5 w-5" />
              {unreadCount > 0 && (
                <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-96">
            <DropdownMenuLabel className="flex items-center justify-between gap-2">
              <span>Notificações</span>
              {unreadCount > 0 && (
                <Button variant="ghost" size="sm" className="h-7 px-2 text-xs" onClick={handleMarkAllNotifications}>
                  <CheckCheck className="h-3.5 w-3.5 mr-1" />
                  Ler todas
                </Button>
              )}
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            {notificacoes.length === 0 ? (
              <div className="px-2 py-6 text-center text-sm text-muted-foreground">
                Nenhuma notificação por enquanto.
              </div>
            ) : (
              notificacoes.map((item) => (
                <DropdownMenuItem
                  key={item.id}
                  className="items-start py-3"
                  onClick={() => handleNotificationClick(item)}
                >
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{item.titulo}</span>
                      {!item.dataLeitura && <span className="h-2 w-2 rounded-full bg-red-500" />}
                    </div>
                    <p className="max-w-[300px] text-xs text-muted-foreground whitespace-normal">
                      {item.mensagem}
                    </p>
                    <p className="text-[11px] text-muted-foreground">
                      {new Date(item.dataCriacao).toLocaleString('pt-BR')}
                    </p>
                  </div>
                </DropdownMenuItem>
              ))
            )}
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => navigate('/notificacoes')}>
              Ver todas
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
        
        {usuario && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="flex items-center space-x-2">
                <User className="h-5 w-5" />
                <span className="hidden md:inline max-w-[150px] truncate">
                  {truncateText(usuario.nome)}
                </span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuLabel>
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium truncate">{usuario.nome}</p>
                  <p className="text-xs text-muted-foreground truncate">{usuario.email}</p>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem asChild>
                <Link to="/perfil" className="flex items-center">
                  <Settings className="h-4 w-4 mr-2" />
                  {t('userMenu.myProfile')}
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout} className="text-red-600">
                <LogOut className="h-4 w-4 mr-2" />
                {t('userMenu.logout')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  );
}
