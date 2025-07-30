import axios from 'axios';

// Configuração base da API
const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para tratamento de erros
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('Erro na API:', error);
    return Promise.reject(error);
  }
);

// Serviços da API
export const visitantesApi = {
  getAll: () => api.get('/visitantes'),
  getById: (id) => api.get(`/visitantes/${id}`),
  create: (data) => api.post('/visitantes', data),
  update: (id, data) => api.put(`/visitantes/${id}`, data),
  delete: (id) => api.delete(`/visitantes/${id}`),
};

export const configuracoesMensagensApi = {
  getAll: () => api.get('/configuracoesMensagens'),
  getById: (id) => api.get(`/configuracoesMensagens/${id}`),
  create: (data) => api.post('/configuracoesMensagens', data),
  update: (id, data) => api.put(`/configuracoesMensagens/${id}`, data),
  delete: (id) => api.delete(`/configuracoesMensagens/${id}`),
};

export const mensagensAgendadasApi = {
  getAll: () => api.get('/mensagensAgendadas'),
  getById: (id) => api.get(`/mensagensAgendadas/${id}`),
  cancelar: (id) => api.patch(`/mensagensAgendadas/${id}/cancelar`),
};

export default api;

