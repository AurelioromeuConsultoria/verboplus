function normalizeBaseUrl(url) {
  if (!url) return '';
  return String(url).replace(/\/+$/, '');
}

export const API_BASE_URL = normalizeBaseUrl(import.meta.env.VITE_API_URL || 'http://localhost:5000');

// Os endpoints do backend usam prefixo /api
export const API_BASE_URL_WITH_API = `${API_BASE_URL}/api`;

