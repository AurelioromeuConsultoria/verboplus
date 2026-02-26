import { api } from '@/lib/apiClient';

export const despesasApi = {
  getAll: () => api.get('/despesas'),
  getById: (id) => api.get(`/despesas/${id}`),
  create: (data) => api.post('/despesas', data),
  update: (id, data) => api.put(`/despesas/${id}`, data),
  delete: (id) => api.delete(`/despesas/${id}`),
};

export const receitasApi = {
  getAll: () => api.get('/receitas'),
  getById: (id) => api.get(`/receitas/${id}`),
  create: (data) => api.post('/receitas', data),
  update: (id, data) => api.put(`/receitas/${id}`, data),
  delete: (id) => api.delete(`/receitas/${id}`),
};

