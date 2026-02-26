import { api } from '@/lib/apiClient';

export const categoriasMidiasApi = {
  getAll: () => api.get('/categoriasMidias'),
  getById: (id) => api.get(`/categoriasMidias/${id}`),
  create: (data) => api.post('/categoriasMidias', data),
  update: (id, data) => api.put(`/categoriasMidias/${id}`),
  delete: (id) => api.delete(`/categoriasMidias/${id}`),
};

export const galeriasFotosApi = {
  getAll: () => api.get('/galeriasFotos'),
  getAtivas: () => api.get('/galeriasFotos/ativas'),
  getById: (id) => api.get(`/galeriasFotos/${id}`),
  getByEvento: (eventoId) => api.get(`/galeriasFotos/evento/${eventoId}`),
  getByCategoria: (categoriaId) => api.get(`/galeriasFotos/categoria/${categoriaId}`),
  create: (data) => api.post('/galeriasFotos', data),
  update: (id, data) => api.put(`/galeriasFotos/${id}`, data),
  delete: (id) => api.delete(`/galeriasFotos/${id}`),
  upload: (id, formData) =>
    api.post(`/galeriasFotos/${id}/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }),
  definirDestaque: (id, nomeArquivo) => api.put(`/galeriasFotos/${id}/destaque`, nomeArquivo),
  listarFotos: (id) => api.get(`/galeriasFotos/${id}/fotos`),
};

