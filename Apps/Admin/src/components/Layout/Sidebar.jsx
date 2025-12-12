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
  User
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';

const menuItems = [
  {
    title: 'Dashboard',
    href: '/',
    icon: Home,
  },
  {
    title: 'Usuários',
    href: '/usuarios',
    icon: UserCog,
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
      },
      {
        title: 'Perfis',
        href: '/perfis',
        icon: User,
      },
      {
        title: 'Visitantes',
        href: '/visitantes',
        icon: Users,
      },
      {
        title: 'Configurações de Mensagens',
        href: '/configuracoes-mensagens',
        icon: MessageSquare,
      },
      {
        title: 'Mensagens Agendadas',
        href: '/mensagens-agendadas',
        icon: CalendarDays,
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
      },
      {
        title: 'Cargos',
        href: '/cargos',
        icon: Briefcase,
      },
      {
        title: 'Voluntários',
        href: '/voluntarios',
        icon: Handshake,
      },
    ],
  },
  {
    title: 'Notícias',
    icon: Newspaper,
    items: [
      {
        title: 'Categorias',
        href: '/categorias-noticias',
        icon: Tag,
      },
      {
        title: 'Notícias',
        href: '/noticias',
        icon: Newspaper,
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
      },
      {
        title: 'Inscrições',
        href: '/inscricoes-eventos',
        icon: ClipboardList,
      },
    ],
  },
  {
    title: 'Portal',
    icon: Globe,
    items: [
      {
        title: 'Contatos',
        href: '/contatos',
        icon: Contact,
      },
      {
        title: 'Destaques do Site',
        href: '/destaques-site',
        icon: Star,
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
      },
      {
        title: 'Galerias de Fotos',
        href: '/galerias-fotos',
        icon: Images,
      },
    ],
  },
];

export function Sidebar() {
  const location = useLocation();
  const [openGroups, setOpenGroups] = useState({
    connect: true,
    voluntariado: true,
    noticias: true,
    eventos: true,
    portal: true,
    mídia: true,
  });

  const toggleGroup = (groupKey) => {
    setOpenGroups((prev) => ({
      ...prev,
      [groupKey]: !prev[groupKey],
    }));
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
      <div className="flex h-16 items-center px-6 border-b border-sidebar-border">
        <div className="flex items-center space-x-2">
          <Church className="h-8 w-8 text-sidebar-primary" />
          <span className="text-lg font-semibold text-sidebar-foreground">
            Sistema Igreja
          </span>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-4 overflow-y-auto">
        {menuItems.map((item) => {
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
          const groupKey = group.title.toLowerCase().replace(/\s+/g, '');
          const isOpen = openGroups[groupKey] ?? false;
          const isActiveGroup = isGroupActive(group.items);
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
                {group.items.map((item) => {
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
      </div>
    </div>
  );
}

