import { Navigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';

export function RequirePermission({ resource, action = 'view', requireAdmin = false, children }) {
  const { loading, isAuthenticated, isAdmin, can } = useAuth();

  if (loading) return <LoadingPage text="Verificando permissões..." />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  if (requireAdmin && !isAdmin) {
    return <ErrorPage message="Esta área é restrita a administradores." />;
  }

  if (requireAdmin && isAdmin) {
    return children;
  }

  if (!can(resource, action)) {
    return <ErrorPage message="Você não tem permissão para acessar esta seção." />;
  }

  return children;
}
