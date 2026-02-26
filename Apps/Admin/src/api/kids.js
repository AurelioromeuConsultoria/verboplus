import { api } from '@/lib/apiClient';

export const kidsApi = {
  getCriancas: () => api.get('/kids/criancas'),
  getCriancaById: (id) => api.get(`/kids/criancas/${id}`),
  createCrianca: (data) => api.post('/kids/criancas', data),
  updateCrianca: (id, data) => api.put(`/kids/criancas/${id}`, data),
  deleteCrianca: (id) => api.delete(`/kids/criancas/${id}`),
  getCheckins: (criancaPessoaId) => {
    const params = criancaPessoaId ? { criancaPessoaId } : {};
    return api.get('/kids/checkins', { params });
  },
  checkin: (data) => api.post('/kids/checkin', data),
  checkout: (data) => api.post('/kids/checkout', data),
};

