import { clsx } from "clsx";
import { twMerge } from "tailwind-merge"

export function cn(...inputs) {
  return twMerge(clsx(inputs));
}

/**
 * Converte uma URL relativa para absoluta usando a base da API
 * @param {string} url - URL relativa (ex: /uploads/...) ou absoluta
 * @returns {string} URL absoluta
 */
export function getAbsoluteUrl(url) {
  if (!url) return null;
  
  // Se já é uma URL absoluta (http:// ou https://), retorna como está
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }
  
  // Remove barra inicial se existir
  const normalizedUrl = url.startsWith('/') ? url.substring(1) : url;
  
  // Garante que não tenha barras duplas
  const cleanUrl = normalizedUrl.replace(/\/+/g, '/');
  
  // Retorna URL absoluta usando a base da API
  const API_BASE_URL = 'http://localhost:5000';
  return `${API_BASE_URL}/${cleanUrl}`;
}
