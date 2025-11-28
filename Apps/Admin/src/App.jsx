import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout } from './components/Layout/Layout';
import { Dashboard } from './pages/Dashboard';
import { VisitantesList } from './pages/Visitantes/VisitantesList';
import { VisitanteForm } from './pages/Visitantes/VisitanteForm';
import { VisitanteDetails } from './pages/Visitantes/VisitanteDetails';
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
import './App.css';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          
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
        </Route>
      </Routes>
    </Router>
  );
}

export default App;

