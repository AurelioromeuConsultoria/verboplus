import { Inbox, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/button';

export function PageEmptyState({
  title = 'Nada encontrado',
  description = 'Nao ha dados para exibir com os filtros atuais.',
  icon: Icon = Inbox,
  action,
  className,
}) {
  return (
    <div className={`rounded-lg border border-dashed bg-muted/20 px-6 py-10 text-center ${className || ''}`}>
      <div className="mx-auto flex max-w-md flex-col items-center gap-3">
        <div className="rounded-full bg-background p-3 shadow-sm">
          <Icon className="h-5 w-5 text-muted-foreground" />
        </div>
        <div className="space-y-1">
          <h3 className="text-lg font-semibold text-foreground">{title}</h3>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        {action ? (
          <div className="pt-1">
            {action}
          </div>
        ) : null}
      </div>
    </div>
  );
}

export function PageRefreshButton({
  onClick,
  refreshing = false,
  label = 'Atualizar',
  className,
}) {
  return (
    <Button type="button" variant="outline" onClick={onClick} disabled={refreshing} className={className}>
      <RefreshCw className={`mr-2 h-4 w-4 ${refreshing ? 'animate-spin' : ''}`} />
      {refreshing ? 'Atualizando...' : label}
    </Button>
  );
}
