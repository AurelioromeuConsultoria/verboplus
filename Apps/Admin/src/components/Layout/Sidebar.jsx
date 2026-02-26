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
  ArrowRightLeft,
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
  User,
  ChevronsUpDown,
  BarChart3,
  Baby,
  LogIn,
  Cog,
  Shield
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { RESOURCES } from '@/utils/permissions';

const menuItems = [
  {
    title: 'Dashboard',
    href: '/',
    icon: Home,
    permission: RESOURCES.DASHBOARD,
  },
  {
    title: 'Usuários',
    href: '/usuarios',
    icon: UserCog,
    permission: RESOURCES.USUARIOS,
  },
  {
    title: 'Auditoria',
    href: '/auditoria',
    icon: Shield,
    permission: RESOURCES.USUARIOS,
  },
  {
    title: 'Perfis de Acesso',
    href: '/perfis-acesso',
    icon: UserCog,
    permission: RESOURCES.PERFIS_ACESSO,
  },
];

const menuGroups = [
  {
    title: 'Connect',
    icon: Network,
    items: [
      {
        title: 'Pessoas',
        href: '/pessoas',
        icon: Users,
        permission: RESOURCES.PESSOAS,
      },
      {
        title: 'Aniversariantes',
        href: '/pessoas/aniversariantes',
        icon: CalendarDays,
        permission: RESOURCES.PESSOAS,
      },
      {
        title: 'Perfis',
        href: '/perfis',
        icon: User,
        permission: RESOURCES.PERFIS,
      },
      {
        title: 'Visitantes',
        href: '/visitantes',
        icon: Users,
        permission: RESOURCES.VISITANTES,
      },
      {
        title: 'Configurações de Mensagens',
        href: '/configuracoes-mensagens',
        icon: MessageSquare,
        permission: RESOURCES.CONFIG_MENSAGENS,
      },
      {
        title: 'Mensagens Agendadas',
        href: '/mensagens-agendadas',
        icon: CalendarDays,
        permission: RESOURCES.MENSAGENS_AGENDADAS,
      },
    ],
  },
  {
    title: 'Voluntariado',
    icon: Handshake,
    items: [
      {
        title: 'Equipes',
        href: '/equipes',
        icon: Group,
        permission: RESOURCES.EQUIPES,
      },
      {
        title: 'Cargos',
        href: '/cargos',
        icon: Briefcase,
        permission: RESOURCES.CARGOS,
      },
      {
        title: 'Voluntários',
        href: '/voluntarios',
        icon: Handshake,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        title: 'Escalas',
        href: '/voluntariado/escalas',
        icon: CalendarDays,
        permission: RESOURCES.VOLUNTARIOS,
      },
      {
        title: 'Relatório Vínculos',
        href: '/voluntariado/relatorio-vinculos',
        icon: ArrowRightLeft,
        permission: RESOURCES.VOLUNTARIOS,
      },
    ],
  },
  {
    title: 'Hub',
    icon: Home,
    items: [
      {
        title: 'Casas',
        href: '/hub/casas',
        icon: Home,
      },
    ],
  },
  {
    title: 'Eventos',
    icon: Calendar,
    items: [
      {
        title: 'Eventos',
        href: '/eventos',
        icon: Calendar,
        permission: RESOURCES.EVENTOS,
      },
      {
        title: 'Inscrições',
        href: '/inscricoes-eventos',
        icon: ClipboardList,
        permission: RESOURCES.INSCRICOES_EVENTOS,
      },
    ],
  },
  {
    title: 'Financeiro',
    icon: Briefcase,
    items: [
      {
        title: 'Fornecedores',
        href: '/financeiro/fornecedores',
        icon: Contact,
        permission: RESOURCES.FORNECEDORES,
      },
      {
        title: 'Categorias de Despesas',
        href: '/financeiro/categorias-despesas',
        icon: Tag,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Contas Bancárias',
        href: '/financeiro/contas-bancarias',
        icon: Briefcase,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Centros de Custos',
        href: '/financeiro/centros-custos',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Projetos',
        href: '/financeiro/projetos',
        icon: Calendar,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Despesas',
        href: '/financeiro/despesas',
        icon: Briefcase,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Receitas',
        href: '/financeiro/receitas',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Categorias de Receitas',
        href: '/financeiro/categorias-receitas',
        icon: Tag,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Dashboard Financeiro',
        href: '/financeiro/dashboard',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
      {
        title: 'Relatórios Financeiros',
        href: '/financeiro/relatorios',
        icon: BarChart3,
        permission: RESOURCES.FINANCEIRO,
      },
    ],
  },
  {
    title: 'Portal',
    icon: Globe,
    items: [
      {
        title: 'Categorias de Notícias',
        href: '/categorias-noticias',
        icon: Tag,
        permission: RESOURCES.CATEGORIAS_NOTICIAS,
      },
      {
        title: 'Notícias',
        href: '/noticias',
        icon: Newspaper,
        permission: RESOURCES.NOTICIAS,
      },
      {
        title: 'Enquetes',
        href: '/enquetes',
        icon: BarChart3,
        permission: RESOURCES.ENQUETES,
      },
      {
        title: 'Contatos',
        href: '/contatos',
        icon: Contact,
        permission: RESOURCES.CONTATOS,
      },
      {
        title: 'Destaques do Site',
        href: '/destaques-site',
        icon: Star,
        permission: RESOURCES.DESTAQUES_SITE,
      },
      {
        title: 'Configuração',
        href: '/configuracao-portal',
        icon: Cog,
        permission: RESOURCES.PORTAL,
      },
    ],
  },
  {
    title: 'Mídia',
    icon: Images,
    items: [
      {
        title: 'Categorias de Mídia',
        href: '/categorias-midias',
        icon: Folder,
        permission: RESOURCES.MIDIA,
      },
      {
        title: 'Galerias de Fotos',
        href: '/galerias-fotos',
        icon: Images,
        permission: RESOURCES.GALERIAS_FOTOS,
      },
    ],
  },
  {
    title: 'Kids',
    icon: Baby,
    items: [
      {
        title: 'Check-ins',
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
  const [openGroups, setOpenGroups] = useState({
    connect: true,
    voluntariado: true,
    eventos: true,
    financeiro: true,
    portal: true,
    mídia: true,
    kids: true,
    hub: true,
  });

  const toggleGroup = (groupKey) => {
    setOpenGroups((prev) => ({
      ...prev,
      [groupKey]: !prev[groupKey],
    }));
  };

  const expandAllGroups = () => {
    const allGroups = menuGroups.reduce((acc, group) => {
      const groupKey = group.title.toLowerCase().replace(/\s+/g, '');
      acc[groupKey] = true;
      return acc;
    }, {});
    setOpenGroups(allGroups);
  };

  const collapseAllGroups = () => {
    const allGroups = menuGroups.reduce((acc, group) => {
      const groupKey = group.title.toLowerCase().replace(/\s+/g, '');
      acc[groupKey] = false;
      return acc;
    }, {});
    setOpenGroups(allGroups);
  };

  const allExpanded = menuGroups.every(group => {
    const groupKey = group.title.toLowerCase().replace(/\s+/g, '');
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
            Sistema Igreja
          </span>
        </div>
        <Button
          variant="ghost"
          size="sm"
          onClick={toggleAllGroups}
          className="h-8 w-8 p-0 text-sidebar-foreground/60 hover:text-sidebar-foreground"
          title={allExpanded ? 'Colapsar todos os menus' : 'Expandir todos os menus'}
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
              <span>{item.title}</span>
            </Link>
          );
        })}

        {/* Menu Groups */}
        {menuGroups.map((group, groupIndex) => {
          const visibleItems = group.items.filter((item) => !item.permission || can(item.permission, 'view'));
          if (visibleItems.length === 0) return null;
          const groupKey = group.title.toLowerCase().replace(/\s+/g, '');
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
                  <span>{group.title}</span>
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
                      <span>{item.title}</span>
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
          Sistema de Gestão para Igrejas
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
          <span>Desenvolvido por Malach Digital</span>
        </a>
      </div>
    </div>
  );
}
