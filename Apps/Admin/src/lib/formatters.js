export function formatDateBr(value, fallback = '-') {
  if (!value) return fallback;

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return fallback;

  return date.toLocaleDateString('pt-BR');
}

export function formatDateTimeBr(value, fallback = '-') {
  if (!value) return fallback;

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return fallback;

  return date.toLocaleString('pt-BR');
}

export function formatShortTime(value, fallback = 'Nao definido') {
  if (!value) return fallback;
  return String(value).slice(0, 5);
}
