import axios from 'axios';
import { API_BASE_URL_WITH_API } from './env';

export const api = axios.create({
  baseURL: API_BASE_URL_WITH_API,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    const usuario = JSON.parse(localStorage.getItem('usuario') || 'null');
    const selectedTenantId = localStorage.getItem('selectedTenantId');
    const selectedTenantSlug = localStorage.getItem('selectedTenantSlug');
    const requestUrl = String(config.url || '').toLowerCase();
    const isAuthRequest =
      requestUrl.includes('/auth/login') ||
      requestUrl.includes('/auth/me') ||
      requestUrl.includes('/auth/refresh') ||
      requestUrl.includes('/auth/alterar-senha') ||
      requestUrl.includes('/tenants/contexto-operacional') ||
      requestUrl.includes('/auditoria-administrativa');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Auth endpoints must always resolve the user's home tenant from the token.
    // The operational tenant override is only for business/admin screens.
    if (!isAuthRequest && usuario?.isPlatformAdmin && selectedTenantId) {
      config.headers['X-Tenant-Id'] = selectedTenantId;
      if (selectedTenantSlug) {
        config.headers['X-Tenant-Slug'] = selectedTenantSlug;
      }
    }

    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('Erro na API:', error);

    if (error.response?.status === 401 && !error.config.url?.includes('/auth/login')) {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('usuario');
      window.location.href = '/login';
    }

    return Promise.reject(error);
  }
);
