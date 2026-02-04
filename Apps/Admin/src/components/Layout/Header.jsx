import { Bell, User, LogOut, Settings, Sun, Moon } from 'lucide-react';
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

// Mapeamento de rotas para labels de breadcrumb
const routeLabels = {
  '': 'Dashboard',
  'pessoas': 'Pessoas',
  'visitantes': 'Visitantes',
  'perfis': 'Perfis',
  'configuracoes-mensagens': 'Configurações de Mensagens',
  'mensagens-agendadas': 'Mensagens Agendadas',
  'equipes': 'Equipes',
  'cargos': 'Cargos',
  'voluntarios': 'Voluntários',
  'eventos': 'Eventos',
  'inscricoes-eventos': 'Inscrições em Eventos',
  'destaques-site': 'Destaques do Site',
  'categorias-noticias': 'Categorias de Notícias',
  'noticias': 'Notícias',
  'contatos': 'Contatos',
  'usuarios': 'Usuários',
  'perfil': 'Meu Perfil',
  'categorias-midias': 'Categorias de Mídia',
  'galerias-fotos': 'Galerias de Fotos',
  'novo': 'Novo',
  'editar': 'Editar',
};

function generateBreadcrumbs(pathname) {
  const paths = pathname.split('/').filter(Boolean);
  const breadcrumbs = [{ label: 'Dashboard', path: '/' }];

  if (paths.length === 0) {
    return breadcrumbs;
  }

  let currentPath = '';
  paths.forEach((segment, index) => {
    currentPath += `/${segment}`;
    const isLast = index === paths.length - 1;
    
    // Se for um ID numérico, tentar manter o label anterior
    if (/^\d+$/.test(segment)) {
      const prevLabel = routeLabels[paths[index - 1]] || segment;
      breadcrumbs.push({
        label: prevLabel,
        path: isLast ? null : currentPath,
      });
    } else {
      const label = routeLabels[segment] || segment.charAt(0).toUpperCase() + segment.slice(1);
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
  const breadcrumbs = generateBreadcrumbs(location.pathname);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const truncateText = (text, maxLength = 20) => {
    if (!text) return '';
    return text.length > maxLength ? `${text.substring(0, maxLength)}...` : text;
  };

  return (
    <header className="flex h-16 items-center justify-between border-b border-border bg-background px-6">
      <div className="flex items-center space-x-4 flex-1 min-w-0">
        <h1 className="text-xl font-semibold text-foreground hidden md:block">
          Sistema Igreja
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
        <Tooltip>
          <TooltipTrigger asChild>
            <Button variant="ghost" size="icon" onClick={toggleTheme}>
              {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            {isDark ? 'Usar tema claro' : 'Usar tema escuro'}
          </TooltipContent>
        </Tooltip>
        
        <Button variant="ghost" size="icon">
          <Bell className="h-5 w-5" />
        </Button>
        
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
                  Meu Perfil
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout} className="text-red-600">
                <LogOut className="h-4 w-4 mr-2" />
                Sair
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  );
}

