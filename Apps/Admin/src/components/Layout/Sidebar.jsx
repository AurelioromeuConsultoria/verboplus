import { Link, useLocation } from 'react-router-dom';
import { useState } from 'react';
import { 
  Home, 
  Users, 
  MessageSquare, 
  Calendar,
  Church,
  Group,
  Briefcase,
  Handshake,
  CalendarDays,
  Gift,
  CalendarOff,
  ArrowRightLeft,
  ClipboardCheck,
  Star,
  Tag,
  Newspaper,
  Contact,
  ClipboardList,
  Globe,
  ChevronDown,
  ChevronRight,
  Network,
  UserCog,
  Images,
  Folder,
  ChevronsUpDown,
  BarChart3,
  Baby,
  LogIn,
  Cog,
  Shield,
  Package
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES } from '@/utils/permissions';
import { useTranslation } from 'react-i18next';

const menuItems = [
  {
    titleKey: 'menu.dashboard',
    href: '/',
    icon: Home,
    permission: RESOURCES.DASHBOARD,
  },
  {
    titleKey: 'menu.mySchedules',
    href: '/minhas-escalas',
    icon: ClipboardCheck,
  },
  {
    titleKey: 'menu.users',
    href: '/usuarios',
    icon: UserCog,
    permission: RESOURCES.USUARIOS,
  },
  {
    titleKey: 'menu.audit',
    href: '/auditoria',
    icon: Shield,
    permission: RESOURCES.USUARIOS,
  },
  {
    titleKey: 'menu.accessProfiles',
    href: '/perfis-acesso',
    icon: UserCog,
    permission: RESOURCES.PERFIS_ACESSO,
  },
];

const menuGroups = [
  {
    titleKey: 'menu.connect',
    icon: Network,
    items: [
      {
        titleKey: 'menu.people',
        href: '/pessoas',
        icon: Users,
        permission: RESOURCES.PESSOAS,
      },
      {
        titleKey: 'menu.birthdays',
        href: '/pessoas/aniversariantes',
        icon: CalendarDays,
        permission: RESOURCES.PESSOAS,
      },
      {
        titleKey: 'menu.birthdayCampaign',
        href: '/pessoas/aniversariantes/campanha',
        icon: Gift,
        permission: RESOURCES.PESSOAS,
      },
      {
        titleKey: 'menu.visitors',
        href: '/visitantes',
        icon: Users,
        permission: RESOURCES.VISITANTES,
      },
      {
        titleKey: 'menu.messageSettings',
        href: '/configuracoes-mensagens',
        icon: MessageSquare,
        permission: RESOURCES.CONFIG_MENSAGENS,
      },
      {
        titleKey: 'menu.scheduledMessages',
        href: '/mensagens-agendadas',
        icon: CalendarDays,
        permission: RESOURCES.MENSAGENS_AGENDADAS,
      },
    ],
  },
  {
    titleKey: 'menu.volunteering',
    icon: Handshake,
    items: [
      {
        titleKey: 'menu.teams',
        href: '/equipes',
        icon: Group,
        permission: RESOURCES.EQUIPES,
      },
      {
        titleKey: 'menu.roles',
        href: '/cargos',
        icon: Briefcase,
        permission: RESOURCES.CARGOS,
      },
      {
        titleKey: 'menu.volunteers',
        href: '/voluntarios',
        icon: Handshake,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.schedules',
        href: '/voluntariado/escalas',
        icon: CalendarDays,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.coveragePanel',
        href: '/voluntariado/painel-cobertura',
        icon: ClipboardCheck,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.scheduleModels',
        href: '/voluntariado/modelos-escala',
        icon: ClipboardList,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.unavailabilities',
        href: '/voluntariado/indisponibilidades',
        icon: CalendarOff,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.linksReport',
        href: '/voluntariado/relatorio-vinculos',
        icon: ArrowRightLeft,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.swapRequests',
        href: '/voluntariado/solicitacoes-troca',
        icon: ClipboardCheck,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        titleKey: 'menu.volunteerHistory',
        href: '/voluntariado/historico',
        icon: ClipboardCheck,
        permission: RESOURCES.VOLUNTARIOS,
      },
    ],
  },
  {
    titleKey: 'menu.hub',
    icon: Home,
    items: [
      {
        titleKey: 'menu.houses',
        href: '/hub/casas',
        icon: Home,
      },
    ],
  },
  {
    titleKey: 'menu.events',
    icon: Calendar,
    items: [
      {
        titleKey: 'menu.events',
        href: '/eventos',
        icon: Calendar,
        permission: RESOURCES.EVENTOS,
      },
      {
        titleKey: 'menu.occurrences',
        href: '/eventos/ocorrencias',
        icon: Calendar,
        permission: RESOURCES.EVENTOS,
      },
      {
        titleKey: 'menu.registrations',
        href: '/inscricoes-eventos',
        icon: ClipboardList,
        permission: RESOURCES.INSCRICOES_EVENTOS,
      },
    ],
  },
  {
    titleKey: 'menu.finance',
    icon: Briefcase,
    items: [
      {
        titleKey: 'menu.suppliers',
        href: '/financeiro/fornecedores',
        icon: Contact,
        permission: RESOURCES.FORNECEDORES,
      },
      {
        titleKey: 'menu.expenseCategories',
        href: '/financeiro/categorias-despesas',
        icon: Tag,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.bankAccounts',
        href: '/financeiro/contas-bancarias',
        icon: Briefcase,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.costCenters',
        href: '/financeiro/centros-custos',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.projects',
        href: '/financeiro/projetos',
        icon: Calendar,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.patrimony',
        href: '/financeiro/patrimonio',
        icon: Package,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.patrimonyCategories',
        href: '/financeiro/patrimonio/categorias',
        icon: Tag,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.patrimonyReport',
        href: '/financeiro/patrimonio/relatorio-geral',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.expenses',
        href: '/financeiro/despesas',
        icon: Briefcase,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.revenues',
        href: '/financeiro/receitas',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.revenueCategories',
        href: '/financeiro/categorias-receitas',
        icon: Tag,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.financeDashboard',
        href: '/financeiro/dashboard',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        titleKey: 'menu.financeReports',
        href: '/financeiro/relatorios',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
    ],
  },
  {
    titleKey: 'menu.portal',
    icon: Globe,
    items: [
      {
        titleKey: 'menu.newsCategories',
        href: '/categorias-noticias',
        icon: Tag,
        permission: RESOURCES.CATEGORIAS_NOTICIAS,
      },
      {
        titleKey: 'menu.news',
        href: '/noticias',
        icon: Newspaper,
        permission: RESOURCES.NOTICIAS,
      },
      {
        titleKey: 'menu.polls',
        href: '/enquetes',
        icon: BarChart3,
        permission: RESOURCES.ENQUETES,
      },
      {
        titleKey: 'menu.contacts',
        href: '/contatos',
        icon: Contact,
        permission: RESOURCES.CONTATOS,
      },
      {
        titleKey: 'menu.siteHighlights',
        href: '/destaques-site',
        icon: Star,
        permission: RESOURCES.DESTAQUES_SITE,
      },
      {
        titleKey: 'menu.portalConfig',
        href: '/configuracao-portal',
        icon: Cog,
        permission: RESOURCES.PORTAL,
      },
    ],
  },
  {
    titleKey: 'menu.media',
    icon: Images,
    items: [
      {
        titleKey: 'menu.mediaCategories',
        href: '/categorias-midias',
        icon: Folder,
        permission: RESOURCES.MIDIA,
      },
      {
        titleKey: 'menu.photoGalleries',
        href: '/galerias-fotos',
        icon: Images,
        permission: RESOURCES.GALERIAS_FOTOS,
      },
    ],
  },
  {
    titleKey: 'menu.kids',
    icon: Baby,
    items: [
      {
        titleKey: 'menu.kidsCheckins',
        href: '/kids/checkins',
        icon: LogIn,
        permission: RESOURCES.KIDS,
      },
    ],
  },
];

export function Sidebar() {
  const location = useLocation();
  const { can } = useAuth();
  const { t } = useTranslation();
  const [openGroups, setOpenGroups] = useState({});

  const toggleGroup = (groupKey) => {
    setOpenGroups((prev) => ({
      ...prev,
      [groupKey]: !prev[groupKey],
    }));
  };

  const expandAllGroups = () => {
    const allGroups = menuGroups.reduce((acc, group) => {
      const groupKey = group.titleKey.split('.').pop();
      acc[groupKey] = true;
      return acc;
    }, {});
    setOpenGroups(allGroups);
  };

  const collapseAllGroups = () => {
    const allGroups = menuGroups.reduce((acc, group) => {
      const groupKey = group.titleKey.split('.').pop();
      acc[groupKey] = false;
      return acc;
    }, {});
    setOpenGroups(allGroups);
  };

  const allExpanded = menuGroups.every(group => {
    const groupKey = group.titleKey.split('.').pop();
    return openGroups[groupKey];
  });

  const toggleAllGroups = () => {
    if (allExpanded) {
      collapseAllGroups();
    } else {
      expandAllGroups();
    }
  };

  const isGroupActive = (items) => {
    return items.some((item) => {
      const isActive = location.pathname === item.href || 
        (item.href !== '/' && location.pathname.startsWith(item.href));
      return isActive;
    });
  };

  return (
    <div className="flex h-full w-64 flex-col bg-sidebar border-r border-sidebar-border">
      {/* Logo */}
      <div className="flex h-16 items-center justify-between px-6 border-b border-sidebar-border">
        <div className="flex items-center space-x-2">
          <Church className="h-8 w-8 text-sidebar-primary" />
          <span className="text-lg font-semibold text-sidebar-foreground">
            {t('app.name')}
          </span>
        </div>
        <Button
          variant="ghost"
          size="sm"
          onClick={toggleAllGroups}
          className="h-8 w-8 p-0 text-sidebar-foreground/60 hover:text-sidebar-foreground"
          title={allExpanded ? t('layout.collapseAll') : t('layout.expandAll')}
        >
          <ChevronsUpDown className="h-4 w-4" />
        </Button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-4 overflow-y-auto">
        {menuItems.filter((item) => !item.permission || can(item.permission, 'view')).map((item) => {
          const Icon = item.icon;
          const isActive = location.pathname === item.href || 
            (item.href !== '/' && location.pathname.startsWith(item.href));

          return (
            <Link
              key={item.href}
              to={item.href}
              className={cn(
                'flex items-center space-x-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
              )}
            >
              <Icon className="h-5 w-5" />
              <span>{t(item.titleKey)}</span>
            </Link>
          );
        })}

        {/* Menu Groups */}
        {menuGroups.map((group, groupIndex) => {
          const visibleItems = group.items.filter((item) => !item.permission || can(item.permission, 'view'));
          if (visibleItems.length === 0) return null;
          const groupKey = group.titleKey.split('.').pop();
          const isOpen = openGroups[groupKey] ?? false;
          const isActiveGroup = isGroupActive(visibleItems);
          const GroupIcon = group.icon;

          return (
            <Collapsible
              key={groupIndex}
              open={isOpen}
              onOpenChange={() => toggleGroup(groupKey)}
              className="mt-2"
            >
              <CollapsibleTrigger
                className={cn(
                  'flex w-full items-center justify-between rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActiveGroup
                    ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                    : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
                )}
              >
                <div className="flex items-center space-x-3">
                  <GroupIcon className="h-5 w-5" />
                  <span>{t(group.titleKey)}</span>
                </div>
                {isOpen ? (
                  <ChevronDown className="h-4 w-4" />
                ) : (
                  <ChevronRight className="h-4 w-4" />
                )}
              </CollapsibleTrigger>
              <CollapsibleContent className="space-y-1 mt-1">
                {visibleItems.map((item) => {
                  const ItemIcon = item.icon;
                  const isActive = location.pathname === item.href || 
                    (item.href !== '/' && location.pathname.startsWith(item.href));

                  return (
                    <Link
                      key={item.href}
                      to={item.href}
                      className={cn(
                        'flex items-center space-x-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ml-6',
                        isActive
                          ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                          : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
                      )}
                    >
                      <ItemIcon className="h-4 w-4" />
                      <span>{t(item.titleKey)}</span>
                    </Link>
                  );
                })}
              </CollapsibleContent>
            </Collapsible>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-sidebar-border">
        <div className="text-xs text-sidebar-foreground/60">
          {t('app.tagline')}
        </div>
        <a
          href="https://malachdigital.com.br/"
          target="_blank"
          rel="noreferrer"
          className="mt-3 flex items-center gap-2 text-xs text-sidebar-foreground/70 hover:text-sidebar-foreground"
        >
          <svg
            viewBox="0 0 24 24"
            aria-hidden="true"
            className="h-4 w-4 text-black dark:text-white"
            fill="currentColor"
          >
            <path d="M3 20V4h4l5 5 5-5h4v16h-4V10l-5 5-5-5v10H3z" />
          </svg>
          <span>{t('app.developedBy')}</span>
        </a>
      </div>
    </div>
  );
}
