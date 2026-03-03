import { api } from '@/lib/apiClient';

export const equipesApi = {
  getAll: () => api.get('/equipes'),
  getById: (id) => api.get(`/equipes/${id}`),
  create: (data) => api.post('/equipes', data),
  update: (id, data) => api.put(`/equipes/${id}`, data),
  delete: (id) => api.delete(`/equipes/${id}`),
};

export const cargosApi = {
  getAll: () => api.get('/cargos'),
  getById: (id) => api.get(`/cargos/${id}`),
  create: (data) => api.post('/cargos', data),
  update: (id, data) => api.put(`/cargos/${id}`, data),
  delete: (id) => api.delete(`/cargos/${id}`),
};

export const voluntariosApi = {
  getAll: () => api.get('/voluntarios'),
  getById: (id) => api.get(`/voluntarios/${id}`),
  getByPessoa: (pessoaId) => api.get(`/voluntarios/pessoa/${pessoaId}`),
  getByEquipe: (equipeId) => api.get(`/voluntarios/equipe/${equipeId}`),
  create: (data) => api.post('/voluntarios', data),
  update: (id, data) => api.put(`/voluntarios/${id}`, data),
  delete: (id) => api.delete(`/voluntarios/${id}`),
};

export const escalasApi = {
  getById: (id) => api.get(`/Escalas/${id}`),
  getByOcorrencia: (eventoOcorrenciaId) => api.get(`/Escalas/ocorrencia/${eventoOcorrenciaId}`),
  getSugestoes: (escalaId, equipeId) => api.get(`/Escalas/${escalaId}/sugestoes`, { params: { equipeId } }),
  create: (data) => api.post('/Escalas', data),
  update: (id, data) => api.put(`/Escalas/${id}`, data),
  delete: (id) => api.delete(`/Escalas/${id}`),
  addItem: (escalaId, data) => api.post(`/Escalas/${escalaId}/itens`, data),
  updateItem: (escalaId, escalaItemId, data) => api.put(`/Escalas/${escalaId}/itens/${escalaItemId}`, data),
  deleteItem: (escalaId, escalaItemId) => api.delete(`/Escalas/${escalaId}/itens/${escalaItemId}`),
  publicar: (escalaId) => api.post(`/Escalas/${escalaId}/publicar`),
};

