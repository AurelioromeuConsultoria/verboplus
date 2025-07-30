import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Layout } from './components/Layout/Layout';
import { Dashboard } from './pages/Dashboard';
import { VisitantesList } from './pages/Visitantes/VisitantesList';
import { VisitanteForm } from './pages/Visitantes/VisitanteForm';
import { VisitanteDetails } from './pages/Visitantes/VisitanteDetails';
import ConfiguracoesList from './pages/ConfiguracoesMensagens/ConfiguracoesList';
import ConfiguracaoForm from './pages/ConfiguracoesMensagens/ConfiguracaoForm';
import MensagensAgendadas from './pages/MensagensAgendadas/MensagensAgendadas';
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
        </Route>
      </Routes>
    </Router>
  );
}

export default App;

