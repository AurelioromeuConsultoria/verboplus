import { api } from '@/lib/apiClient';

export const kidsApi = {
  getCriancas: () => api.get('/kids/criancas'),
  getCriancaById: (id) => api.get(`/kids/criancas/${id}`),
  createCrianca: (data) => api.post('/kids/criancas', data),
  updateCrianca: (id, data) => api.put(`/kids/criancas/${id}`, data),
  deleteCrianca: (id) => api.delete(`/kids/criancas/${id}`),
  getIndicadores: (params = {}) => api.get('/kids/indicadores', { params }),
  getSalas: (params = {}) => api.get('/kids/salas', { params }),
  createSala: (data) => api.post('/kids/salas', data),
  updateSala: (id, data) => api.put(`/kids/salas/${id}`, data),
  getTurmas: (params = {}) => api.get('/kids/turmas', { params }),
  createTurma: (data) => api.post('/kids/turmas', data),
  updateTurma: (id, data) => api.put(`/kids/turmas/${id}`, data),
  getPainelOperacional: (params = {}) => api.get('/kids/painel-operacional', { params }),
  getOcorrenciasAbertas: () => api.get('/kids/ocorrencias/abertas'),
  getOcorrenciasByCrianca: (criancaPessoaId) => api.get(`/kids/criancas/${criancaPessoaId}/ocorrencias`),
  createOcorrencia: (data) => api.post('/kids/ocorrencias', data),
  updateOcorrencia: (id, data) => api.patch(`/kids/ocorrencias/${id}`, data),
  getCheckins: (criancaPessoaId) => {
    const params = criancaPessoaId ? { criancaPessoaId } : {};
    return api.get('/kids/checkins', { params });
  },
  checkin: (data) => api.post('/kids/checkin', data),
  checkout: (data) => api.post('/kids/checkout', data),
};
