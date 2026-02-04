import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout/Layout';
import Login from './pages/Login/Login';
import { Dashboard } from './pages/Dashboard';
import { VisitantesList } from './pages/Visitantes/VisitantesList';
import { VisitanteForm } from './pages/Visitantes/VisitanteForm';
import { VisitanteDetails } from './pages/Visitantes/VisitanteDetails';
import { PessoasList } from './pages/Pessoas/PessoasList';
import { PessoaForm } from './pages/Pessoas/PessoaForm';
import { PessoaDetails } from './pages/Pessoas/PessoaDetails';
import { PerfisList } from './pages/Perfis/PerfisList';
import ConfiguracoesList from './pages/ConfiguracoesMensagens/ConfiguracoesList';
import ConfiguracaoForm from './pages/ConfiguracoesMensagens/ConfiguracaoForm';
import MensagensAgendadas from './pages/MensagensAgendadas/MensagensAgendadas';
import EquipesList from './pages/Equipes/EquipesList';
import EquipeForm from './pages/Equipes/EquipeForm';
import CargosList from './pages/Cargos/CargosList';
import CargoForm from './pages/Cargos/CargoForm';
import VoluntariosList from './pages/Voluntarios/VoluntariosList';
import VoluntarioForm from './pages/Voluntarios/VoluntarioForm';
import EventosList from './pages/Eventos/EventosList';
import EventoForm from './pages/Eventos/EventoForm';
import DestaquesSiteList from './pages/DestaquesSite/DestaquesSiteList';
import DestaqueSiteForm from './pages/DestaquesSite/DestaqueSiteForm';
import CategoriasNoticiasList from './pages/CategoriasNoticias/CategoriasNoticiasList';
import CategoriaNoticiaForm from './pages/CategoriasNoticias/CategoriaNoticiaForm';
import NoticiasList from './pages/Noticias/NoticiasList';
import NoticiaForm from './pages/Noticias/NoticiaForm';
import ContatosList from './pages/Contatos/ContatosList';
import ContatoForm from './pages/Contatos/ContatoForm';
import InscricoesEventosList from './pages/InscricoesEventos/InscricoesEventosList';
import InscricaoEventoDetails from './pages/InscricoesEventos/InscricaoEventoDetails';
import InscricaoEventoForm from './pages/InscricoesEventos/InscricaoEventoForm';
import EventoInscricoes from './pages/InscricoesEventos/EventoInscricoes';
import UsuariosList from './pages/Usuarios/UsuariosList';
import Perfil from './pages/Perfil/Perfil';
import CategoriasMidiasList from './pages/CategoriasMidias/CategoriasMidiasList';
import CategoriaMidiaForm from './pages/CategoriasMidias/CategoriaMidiaForm';
import GaleriasFotosList from './pages/GaleriasFotos/GaleriasFotosList';
import GaleriaFotoForm from './pages/GaleriasFotos/GaleriaFotoForm';
import GaleriaFotos from './pages/GaleriasFotos/GaleriaFotos';
import EnquetesList from './pages/Enquetes/EnquetesList';
import EnqueteForm from './pages/Enquetes/EnqueteForm';
import KidsCheckinsList from './pages/Kids/KidsCheckinsList';
import ConfiguracaoPortal from './pages/ConfiguracaoPortal/ConfiguracaoPortal';
import './App.css';

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Router>
        <Routes>
          {/* Rota pública de login */}
          <Route path="/login" element={<Login />} />
          
          {/* Rotas protegidas */}
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Dashboard />} />
          
          {/* Rotas de Pessoas */}
          <Route path="pessoas" element={<PessoasList />} />
          <Route path="pessoas/novo" element={<PessoaForm />} />
          <Route path="pessoas/:id/editar" element={<PessoaForm />} />
          <Route path="pessoas/:id" element={<PessoaDetails />} />

          {/* Rotas de Perfis */}
          <Route path="perfis" element={<PerfisList />} />

          {/* Rotas de Visitantes */}
          <Route path="visitantes" element={<VisitantesList />} />
          <Route path="visitantes/novo" element={<VisitanteForm />} />
          <Route path="visitantes/:id/editar" element={<VisitanteForm />} />
          <Route path="visitantes/:id" element={<VisitanteDetails />} />
          
          {/* Rotas de Configurações de Mensagens */}
          <Route path="configuracoes-mensagens" element={<ConfiguracoesList />} />
          <Route path="configuracoes-mensagens/novo" element={<ConfiguracaoForm />} />
          <Route path="configuracoes-mensagens/editar/:id" element={<ConfiguracaoForm />} />
          
          {/* Rotas de Mensagens Agendadas */}
          <Route path="mensagens-agendadas" element={<MensagensAgendadas />} />

          {/* Rotas de Equipes */}
          <Route path="equipes" element={<EquipesList />} />
          <Route path="equipes/novo" element={<EquipeForm />} />
          <Route path="equipes/:id/editar" element={<EquipeForm />} />

          {/* Rotas de Cargos */}
          <Route path="cargos" element={<CargosList />} />
          <Route path="cargos/novo" element={<CargoForm />} />
          <Route path="cargos/:id/editar" element={<CargoForm />} />

          {/* Rotas de Voluntários */}
          <Route path="voluntarios" element={<VoluntariosList />} />
          <Route path="voluntarios/novo" element={<VoluntarioForm />} />
          <Route path="voluntarios/:id/editar" element={<VoluntarioForm />} />

          {/* Rotas de Eventos */}
          <Route path="eventos" element={<EventosList />} />
          <Route path="eventos/novo" element={<EventoForm />} />
          <Route path="eventos/:id/editar" element={<EventoForm />} />

          {/* Rotas de Destaques Site */}
          <Route path="destaques-site" element={<DestaquesSiteList />} />
          <Route path="destaques-site/novo" element={<DestaqueSiteForm />} />
          <Route path="destaques-site/:id/editar" element={<DestaqueSiteForm />} />
          <Route path="configuracao-portal" element={<ConfiguracaoPortal />} />

          {/* Rotas de Categorias de Notícias */}
          <Route path="categorias-noticias" element={<CategoriasNoticiasList />} />
          <Route path="categorias-noticias/novo" element={<CategoriaNoticiaForm />} />
          <Route path="categorias-noticias/:id/editar" element={<CategoriaNoticiaForm />} />

          {/* Rotas de Notícias */}
          <Route path="noticias" element={<NoticiasList />} />
          <Route path="noticias/novo" element={<NoticiaForm />} />
          <Route path="noticias/:id/editar" element={<NoticiaForm />} />

          {/* Rotas de Contatos */}
          <Route path="contatos" element={<ContatosList />} />
          <Route path="contatos/novo" element={<ContatoForm />} />
          <Route path="contatos/:id/editar" element={<ContatoForm />} />

          {/* Rotas de Inscrições em Eventos */}
          <Route path="inscricoes-eventos" element={<InscricoesEventosList />} />
          <Route path="inscricoes-eventos/:id" element={<InscricaoEventoDetails />} />
          <Route path="inscricoes-eventos/:id/editar" element={<InscricaoEventoForm />} />
          <Route path="eventos/:eventoId/inscricoes" element={<EventoInscricoes />} />

          {/* Rotas de Usuários */}
          <Route path="usuarios" element={<UsuariosList />} />

          {/* Rota de Perfil */}
          <Route path="perfil" element={<Perfil />} />

          {/* Rotas de Categorias de Mídia */}
          <Route path="categorias-midias" element={<CategoriasMidiasList />} />
          <Route path="categorias-midias/novo" element={<CategoriaMidiaForm />} />
          <Route path="categorias-midias/:id/editar" element={<CategoriaMidiaForm />} />

          {/* Rotas de Galerias de Fotos */}
          <Route path="galerias-fotos" element={<GaleriasFotosList />} />
          <Route path="galerias-fotos/novo" element={<GaleriaFotoForm />} />
          <Route path="galerias-fotos/:id/editar" element={<GaleriaFotoForm />} />
          <Route path="galerias-fotos/:id/fotos" element={<GaleriaFotos />} />

          {/* Rotas de Enquetes */}
          <Route path="enquetes" element={<EnquetesList />} />
          <Route path="enquetes/novo" element={<EnqueteForm />} />
          <Route path="enquetes/:id/editar" element={<EnqueteForm />} />

          {/* Rotas de Kids */}
          <Route path="kids/checkins" element={<KidsCheckinsList />} />
        </Route>
        
        {/* Redirecionar rotas não encontradas para login ou dashboard */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;

