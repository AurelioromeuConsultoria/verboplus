import { api } from '@/lib/apiClient';

export const eventosApi = {
  getAll: () => api.get('/Eventos'),
  getById: (id) => api.get(`/Eventos/${id}`),
  create: (data) => api.post('/Eventos', data),
  update: (id, data) => api.put(`/Eventos/${id}`, data),
  delete: (id) => api.delete(`/Eventos/${id}`),
  getByPeriodo: () => api.get('/Eventos/periodo'),
};

export const eventosOcorrenciasApi = {
  getByEvento: (eventoId) => api.get(`/EventosOcorrencias/evento/${eventoId}`),
  getByPeriodo: (dataInicio, dataFim, eventoId) =>
    api.get('/EventosOcorrencias/periodo', { params: { dataInicio, dataFim, eventoId } }),
  getById: (id) => api.get(`/EventosOcorrencias/${id}`),
  create: (data) => api.post('/EventosOcorrencias', data),
  update: (id, data) => api.put(`/EventosOcorrencias/${id}`),
  delete: (id) => api.delete(`/EventosOcorrencias/${id}`),
  gerarRecorrencia: (eventoId, dataInicio, dataFim) =>
    api.post('/EventosOcorrencias/gerar-recorrencia', null, { params: { eventoId, dataInicio, dataFim } }),
};

export const inscricoesEventosApi = {
  getAll: () => api.get('/InscricoesEventos'),
  getById: (id) => api.get(`/InscricoesEventos/${id}`),
  getByEvento: (eventoId) => api.get(`/InscricoesEventos/evento/${eventoId}`),
  getByStatus: (status) => api.get(`/InscricoesEventos/status/${status}`),
  getEstatisticas: (eventoId) => api.get(`/InscricoesEventos/evento/${eventoId}/estatisticas`),
  create: (data) => api.post('/InscricoesEventos', data),
  update: (id, data) => api.put(`/InscricoesEventos/${id}`),
  confirmar: (id) => api.put(`/InscricoesEventos/${id}/confirmar`),
  cancelar: (id) => api.put(`/InscricoesEventos/${id}/cancelar`),
  delete: (id) => api.delete(`/InscricoesEventos/${id}`),
};

