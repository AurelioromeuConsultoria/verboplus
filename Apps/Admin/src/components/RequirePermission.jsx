import { Navigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { LoadingPage } from '@/components/ui/loading';
import { ErrorPage } from '@/components/ui/error-message';

export function RequirePermission({ resource, action = 'view', children }) {
  const { loading, isAuthenticated, can } = useAuth();

  if (loading) return <LoadingPage text="Verificando permissões..." />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  if (!can(resource, action)) {
    return <ErrorPage message="Você não tem permissão para acessar esta seção." />;
  }

  return children;
}
