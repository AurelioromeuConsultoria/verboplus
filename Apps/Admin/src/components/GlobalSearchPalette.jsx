import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Calendar, FileText, Search, User, Users } from 'lucide-react';
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import { useDebouncedCallback } from '@/hooks/useDebouncedCallback';
import { searchApi } from '@/lib/api';

const TYPE_META = {
  Pessoa: { label: 'Pessoas', icon: Users, to: (id) => `/pessoas/${id}` },
  Visitante: { label: 'Visitantes', icon: User, to: (id) => `/visitantes/${id}` },
  Evento: { label: 'Eventos', icon: Calendar, to: (id) => `/eventos/${id}/editar` },
  Noticia: { label: 'Notícias', icon: FileText, to: (id) => `/noticias/${id}/editar` },
  Usuario: { label: 'Usuários', icon: User, to: () => `/usuarios` },
};

export function GlobalSearchPalette() {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState([]);

  const grouped = useMemo(() => {
    const map = new Map();
    for (const item of items) {
      const key = item.type || 'Outros';
      if (!map.has(key)) map.set(key, []);
      map.get(key).push(item);
    }
    return [...map.entries()];
  }, [items]);

  const runSearch = useCallback(
    async (q) => {
      const s = String(q || '').trim();
      if (s.length < 2) {
        setItems([]);
        return;
      }

      setLoading(true);
      try {
        const resp = await searchApi.search(s, 20);
        setItems(resp.data?.items || []);
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const debouncedSearch = useDebouncedCallback(runSearch, 250);

  useEffect(() => {
    if (!open) return;
    debouncedSearch(query);
  }, [open, query, debouncedSearch]);

  useEffect(() => {
    const onKeyDown = (e) => {
      const isK = e.key?.toLowerCase() === 'k';
      if ((e.metaKey || e.ctrlKey) && isK) {
        e.preventDefault();
        setOpen((v) => !v);
      }
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);

  const handleSelect = (item) => {
    const meta = TYPE_META[item.type];
    const to = meta?.to?.(item.id);
    if (to) navigate(to);
    setOpen(false);
  };

  return (
    <CommandDialog
      open={open}
      onOpenChange={(v) => {
        setOpen(v);
        if (!v) {
          setQuery('');
          setItems([]);
          setLoading(false);
        }
      }}
      title="Busca global"
      description="Busque pessoas, visitantes, eventos e mais."
    >
      <CommandInput
        placeholder="Buscar (ex: João, 1199999, culto, notícia...)"
        value={query}
        onValueChange={setQuery}
      />
      <CommandList>
        {loading ? (
          <div className="px-3 py-2 text-sm text-muted-foreground flex items-center gap-2">
            <Search className="h-4 w-4" />
            Buscando...
          </div>
        ) : (
          <CommandEmpty>Nenhum resultado.</CommandEmpty>
        )}

        {grouped.map(([type, list]) => {
          const meta = TYPE_META[type];
          const Icon = meta?.icon ?? Search;
          const heading = meta?.label ?? type;
          return (
            <CommandGroup key={type} heading={heading}>
              {list.map((it) => (
                <CommandItem key={`${it.type}:${it.id}`} value={`${it.title} ${it.subtitle || ''}`} onSelect={() => handleSelect(it)}>
                  <Icon className="h-4 w-4" />
                  <div className="flex flex-col">
                    <span className="text-sm">{it.title}</span>
                    {it.subtitle ? (
                      <span className="text-xs text-muted-foreground">{it.subtitle}</span>
                    ) : null}
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          );
        })}
      </CommandList>
    </CommandDialog>
  );
}

