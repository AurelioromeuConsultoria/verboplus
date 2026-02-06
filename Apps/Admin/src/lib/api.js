import axios from 'axios';

// Configuração base da API
const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar token nas requisições
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratamento de erros
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('Erro na API:', error);
    
    // Se token expirou ou inválido, redirecionar para login
    if (error.response?.status === 401 && !error.config.url?.includes('/auth/login')) {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('usuario');
      window.location.href = '/login';
    }
    
    return Promise.reject(error);
  }
);

// Serviços da API

// Pessoas
export const pessoasApi = {
  getAll: () => api.get('/pessoas'),
  getById: (id) => api.get(`/pessoas/${id}`),
  create: (data) => api.post('/pessoas', data),
  update: (id, data) => api.put(`/pessoas/${id}`, data),
  delete: (id) => api.delete(`/pessoas/${id}`),
};

// Pessoas Perfis
export const pessoasPerfisApi = {
  getAll: () => api.get('/pessoasperfis'),
  getById: (id) => api.get(`/pessoasperfis/${id}`),
  getByPessoa: (pessoaId) => api.get(`/pessoasperfis/pessoa/${pessoaId}`),
  create: (data) => api.post('/pessoasperfis', data),
  update: (id, data) => api.put(`/pessoasperfis/${id}`, data),
  delete: (id) => api.delete(`/pessoasperfis/${id}`),
};

// Visitantes
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

// Equipes
export const equipesApi = {
  getAll: () => api.get('/equipes'),
  getById: (id) => api.get(`/equipes/${id}`),
  create: (data) => api.post('/equipes', data),
  update: (id, data) => api.put(`/equipes/${id}`, data),
  delete: (id) => api.delete(`/equipes/${id}`),
};

// Cargos
export const cargosApi = {
  getAll: () => api.get('/cargos'),
  getById: (id) => api.get(`/cargos/${id}`),
  create: (data) => api.post('/cargos', data),
  update: (id, data) => api.put(`/cargos/${id}`, data),
  delete: (id) => api.delete(`/cargos/${id}`),
};

// Voluntários
export const voluntariosApi = {
  getAll: () => api.get('/voluntarios'),
  getById: (id) => api.get(`/voluntarios/${id}`),
  create: (data) => api.post('/voluntarios', data),
  update: (id, data) => api.put(`/voluntarios/${id}`, data),
  delete: (id) => api.delete(`/voluntarios/${id}`),
};

// Hub - Casas
export const hubCasasApi = {
  getAll: () => api.get('/hub/casas'),
  getById: (id) => api.get(`/hub/casas/${id}`),
  create: (data) => api.post('/hub/casas', data),
  update: (id, data) => api.put(`/hub/casas/${id}`, data),
  delete: (id) => api.delete(`/hub/casas/${id}`),
};

// Eventos
export const eventosApi = {
  getAll: () => api.get('/Eventos'),
  getById: (id) => api.get(`/Eventos/${id}`),
  create: (data) => api.post('/Eventos', data),
  update: (id, data) => api.put(`/Eventos/${id}`, data),
  delete: (id) => api.delete(`/Eventos/${id}`),
  getByPeriodo: () => api.get('/Eventos/periodo'),
};

// Destaques Site
export const destaquesSiteApi = {
  getAll: () => api.get('/DestaquesSite'),
  getById: (id) => api.get(`/DestaquesSite/${id}`),
  create: (data) => api.post('/DestaquesSite', data),
  update: (id, data) => api.put(`/DestaquesSite/${id}`, data),
  delete: (id) => api.delete(`/DestaquesSite/${id}`),
};

// Categorias de Notícias
export const categoriasNoticiasApi = {
  getAll: () => api.get('/CategoriasNoticias'),
  getById: (id) => api.get(`/CategoriasNoticias/${id}`),
  create: (data) => api.post('/CategoriasNoticias', data),
  update: (id, data) => api.put(`/CategoriasNoticias/${id}`, data),
  delete: (id) => api.delete(`/CategoriasNoticias/${id}`),
};

// Notícias
export const noticiasApi = {
  getAll: () => api.get('/Noticias'),
  getById: (id) => api.get(`/Noticias/${id}`),
  create: (data) => api.post('/Noticias', data),
  update: (id, data) => api.put(`/Noticias/${id}`, data),
  delete: (id) => api.delete(`/Noticias/${id}`),
  getByCategoria: (categoriaId) => api.get(`/Noticias/categoria/${categoriaId}`),
};

// Contatos
export const contatosApi = {
  getAll: () => api.get('/Contatos'),
  getById: (id) => api.get(`/Contatos/${id}`),
  create: (data) => api.post('/Contatos', data),
  update: (id, data) => api.put(`/Contatos/${id}`, data),
  delete: (id) => api.delete(`/Contatos/${id}`),
};

// Inscrições em Eventos
export const inscricoesEventosApi = {
  getAll: () => api.get('/InscricoesEventos'),
  getById: (id) => api.get(`/InscricoesEventos/${id}`),
  getByEvento: (eventoId) => api.get(`/InscricoesEventos/evento/${eventoId}`),
  getByStatus: (status) => api.get(`/InscricoesEventos/status/${status}`),
  getEstatisticas: (eventoId) => api.get(`/InscricoesEventos/evento/${eventoId}/estatisticas`),
  create: (data) => api.post('/InscricoesEventos', data),
  update: (id, data) => api.put(`/InscricoesEventos/${id}`, data),
  confirmar: (id) => api.put(`/InscricoesEventos/${id}/confirmar`),
  cancelar: (id) => api.put(`/InscricoesEventos/${id}/cancelar`),
  delete: (id) => api.delete(`/InscricoesEventos/${id}`),
};

// Autenticação
export const authApi = {
  login: (data) => api.post('/auth/login', data),
  me: () => api.get('/auth/me'),
  alterarSenha: (data) => api.post('/auth/alterar-senha', data),
};

// Uploads
export const uploadApi = {
  uploadImage: (formData) => api.post('/admin/upload/images', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  }),
  uploadVideo: (formData) => api.post('/admin/upload/videos', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  }),
  uploadAudio: (formData) => api.post('/admin/upload/audios', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  }),
  uploadFile: (formData) => api.post('/admin/upload/files', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  }),
};

// Usuários
export const usuariosApi = {
  getAll: () => api.get('/usuarios'),
  getById: (id) => api.get(`/usuarios/${id}`),
  create: (data) => api.post('/usuarios', data),
  update: (id, data) => api.put(`/usuarios/${id}`, data),
  delete: (id) => api.delete(`/usuarios/${id}`),
};

// Categorias de Mídia
export const categoriasMidiasApi = {
  getAll: () => api.get('/categoriasMidias'),
  getById: (id) => api.get(`/categoriasMidias/${id}`),
  create: (data) => api.post('/categoriasMidias', data),
  update: (id, data) => api.put(`/categoriasMidias/${id}`, data),
  delete: (id) => api.delete(`/categoriasMidias/${id}`),
};

// Galerias de Fotos
export const galeriasFotosApi = {
  getAll: () => api.get('/galeriasFotos'),
  getAtivas: () => api.get('/galeriasFotos/ativas'),
  getById: (id) => api.get(`/galeriasFotos/${id}`),
  getByEvento: (eventoId) => api.get(`/galeriasFotos/evento/${eventoId}`),
  getByCategoria: (categoriaId) => api.get(`/galeriasFotos/categoria/${categoriaId}`),
  create: (data) => api.post('/galeriasFotos', data),
  update: (id, data) => api.put(`/galeriasFotos/${id}`, data),
  delete: (id) => api.delete(`/galeriasFotos/${id}`),
  upload: (id, formData) => api.post(`/galeriasFotos/${id}/upload`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  }),
  definirDestaque: (id, nomeArquivo) => api.put(`/galeriasFotos/${id}/destaque`, nomeArquivo),
  listarFotos: (id) => api.get(`/galeriasFotos/${id}/fotos`),
};

// Configuração Portal
export const configuracaoPortalApi = {
  get: () => api.get('/configuracaoPortal'),
  update: (data) => api.put('/configuracaoPortal', data),
};

// Enquetes
export const enquetesApi = {
  getAll: () => api.get('/Enquetes'),
  getAtivas: () => api.get('/Enquetes/ativas'),
  getById: (id) => api.get(`/Enquetes/${id}`),
  create: (data) => api.post('/Enquetes', data),
  update: (id, data) => api.put(`/Enquetes/${id}`, data),
  delete: (id) => api.delete(`/Enquetes/${id}`),
};

// Kids
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

// Dashboard
export const dashboardApi = {
  getEstatisticas: () => api.get('/dashboard/estatisticas'),
};

export default api;
