import { api } from '@/lib/apiClient';

export const auditLogsApi = {
  getPaged: (params) => api.get('/auditLogs/paged', { params }),
};

