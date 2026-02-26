import { api } from '@/lib/apiClient';

export const dashboardApi = {
  getEstatisticas: () => api.get('/dashboard/estatisticas'),
};

