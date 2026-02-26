import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { RequirePermission } from './components/RequirePermission';
import { Layout } from './components/Layout/Layout';
import Login from './pages/Login/Login';

const Dashboard = lazy(() => import('./pages/Dashboard'));
const VisitantesList = lazy(() => import('./pages/Visitantes/VisitantesList'));
const VisitanteForm = lazy(() => import('./pages/Visitantes/VisitanteForm'));
const VisitanteDetails = lazy(() => import('./pages/Visitantes/VisitanteDetails'));
const PessoasList = lazy(() => import('./pages/Pessoas/PessoasList'));
const PessoaForm = lazy(() => import('./pages/Pessoas/PessoaForm'));
const PessoaDetails = lazy(() => import('./pages/Pessoas/PessoaDetails'));
const Aniversariantes = lazy(() => import('./pages/Pessoas/Aniversariantes'));
const PerfisList = lazy(() => import('./pages/Perfis/PerfisList'));
const ConfiguracoesList = lazy(() => import('./pages/ConfiguracoesMensagens/ConfiguracoesList'));
const ConfiguracaoForm = lazy(() => import('./pages/ConfiguracoesMensagens/ConfiguracaoForm'));
const MensagensAgendadas = lazy(() => import('./pages/MensagensAgendadas/MensagensAgendadas'));
const EquipesList = lazy(() => import('./pages/Equipes/EquipesList'));
const EquipeForm = lazy(() => import('./pages/Equipes/EquipeForm'));
const CargosList = lazy(() => import('./pages/Cargos/CargosList'));
const CargoForm = lazy(() => import('./pages/Cargos/CargoForm'));
const VoluntariosList = lazy(() => import('./pages/Voluntarios/VoluntariosList'));
const VoluntarioForm = lazy(() => import('./pages/Voluntarios/VoluntarioForm'));
const EscalasList = lazy(() => import('./pages/Voluntariado/EscalasList'));
const EscalaEditor = lazy(() => import('./pages/Voluntariado/EscalaEditor'));
const RelatorioVinculosVoluntariado = lazy(() => import('./pages/Voluntariado/RelatorioVinculosVoluntariado'));
const EventosList = lazy(() => import('./pages/Eventos/EventosList'));
const EventoForm = lazy(() => import('./pages/Eventos/EventoForm'));
const DestaquesSiteList = lazy(() => import('./pages/DestaquesSite/DestaquesSiteList'));
const DestaqueSiteForm = lazy(() => import('./pages/DestaquesSite/DestaqueSiteForm'));
const CategoriasNoticiasList = lazy(() => import('./pages/CategoriasNoticias/CategoriasNoticiasList'));
const CategoriaNoticiaForm = lazy(() => import('./pages/CategoriasNoticias/CategoriaNoticiaForm'));
const NoticiasList = lazy(() => import('./pages/Noticias/NoticiasList'));
const NoticiaForm = lazy(() => import('./pages/Noticias/NoticiaForm'));
const ContatosList = lazy(() => import('./pages/Contatos/ContatosList'));
const ContatoForm = lazy(() => import('./pages/Contatos/ContatoForm'));
const InscricoesEventosList = lazy(() => import('./pages/InscricoesEventos/InscricoesEventosList'));
const InscricaoEventoDetails = lazy(() => import('./pages/InscricoesEventos/InscricaoEventoDetails'));
const InscricaoEventoForm = lazy(() => import('./pages/InscricoesEventos/InscricaoEventoForm'));
const EventoInscricoes = lazy(() => import('./pages/InscricoesEventos/EventoInscricoes'));
const UsuariosList = lazy(() => import('./pages/Usuarios/UsuariosList'));
const AuditoriaList = lazy(() => import('./pages/Auditoria/AuditoriaList'));
const Perfil = lazy(() => import('./pages/Perfil/Perfil'));
const CategoriasMidiasList = lazy(() => import('./pages/CategoriasMidias/CategoriasMidiasList'));
const CategoriaMidiaForm = lazy(() => import('./pages/CategoriasMidias/CategoriaMidiaForm'));
const GaleriasFotosList = lazy(() => import('./pages/GaleriasFotos/GaleriasFotosList'));
const GaleriaFotoForm = lazy(() => import('./pages/GaleriasFotos/GaleriaFotoForm'));
const GaleriaFotos = lazy(() => import('./pages/GaleriasFotos/GaleriaFotos'));
const EnquetesList = lazy(() => import('./pages/Enquetes/EnquetesList'));
const EnqueteForm = lazy(() => import('./pages/Enquetes/EnqueteForm'));
const KidsCheckinsList = lazy(() => import('./pages/Kids/KidsCheckinsList'));
const ConfiguracaoPortal = lazy(() => import('./pages/ConfiguracaoPortal/ConfiguracaoPortal'));
const CasasList = lazy(() => import('./pages/Hub/CasasList'));
const CasaForm = lazy(() => import('./pages/Hub/CasaForm'));
const FornecedoresList = lazy(() => import('./pages/Fornecedores/FornecedoresList'));
const FornecedorForm = lazy(() => import('./pages/Fornecedores/FornecedorForm'));
const CategoriasDespesasList = lazy(() => import('./pages/Financeiro/CategoriasDespesas/CategoriasDespesasList'));
const CategoriaDespesaForm = lazy(() => import('./pages/Financeiro/CategoriasDespesas/CategoriaDespesaForm'));
const ContasBancariasList = lazy(() => import('./pages/Financeiro/ContasBancarias/ContasBancariasList'));
const ContaBancariaForm = lazy(() => import('./pages/Financeiro/ContasBancarias/ContaBancariaForm'));
const CentrosCustosList = lazy(() => import('./pages/Financeiro/CentrosCustos/CentrosCustosList'));
const CentroCustoForm = lazy(() => import('./pages/Financeiro/CentrosCustos/CentroCustoForm'));
const ProjetosList = lazy(() => import('./pages/Financeiro/Projetos/ProjetosList'));
const ProjetoForm = lazy(() => import('./pages/Financeiro/Projetos/ProjetoForm'));
const DespesasList = lazy(() => import('./pages/Financeiro/Despesas/DespesasList'));
const DespesaForm = lazy(() => import('./pages/Financeiro/Despesas/DespesaForm'));
const ReceitasList = lazy(() => import('./pages/Financeiro/Receitas/ReceitasList'));
const ReceitaForm = lazy(() => import('./pages/Financeiro/Receitas/ReceitaForm'));
const CategoriasReceitasList = lazy(() => import('./pages/Financeiro/CategoriasReceitas/CategoriasReceitasList'));
const CategoriaReceitaForm = lazy(() => import('./pages/Financeiro/CategoriasReceitas/CategoriaReceitaForm'));
const DashboardFinanceiro = lazy(() => import('./pages/Financeiro/DashboardFinanceiro'));
const RelatoriosFinanceiros = lazy(() => import('./pages/Financeiro/RelatoriosFinanceiros'));
const PerfisAcessoList = lazy(() => import('./pages/PerfisAcesso/PerfisAcessoList'));
const PerfilAcessoForm = lazy(() => import('./pages/PerfisAcesso/PerfilAcessoForm'));
import { RESOURCES, ACTIONS } from './utils/permissions';
import './App.css';

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Router>
        <Suspense fallback={<div className="p-6 text-muted-foreground">Carregando...</div>}>
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
            <Route index element={
              <RequirePermission resource={RESOURCES.DASHBOARD}>
                <Dashboard />
              </RequirePermission>
            } />
          
          {/* Rotas de Pessoas */}
          <Route path="pessoas" element={
            <RequirePermission resource={RESOURCES.PESSOAS}>
              <PessoasList />
            </RequirePermission>
          } />
          <Route path="pessoas/aniversariantes" element={
            <RequirePermission resource={RESOURCES.PESSOAS}>
              <Aniversariantes />
            </RequirePermission>
          } />
          <Route path="pessoas/novo" element={
            <RequirePermission resource={RESOURCES.PESSOAS} action={ACTIONS.EDIT}>
              <PessoaForm />
            </RequirePermission>
          } />
          <Route path="pessoas/:id/editar" element={
            <RequirePermission resource={RESOURCES.PESSOAS} action={ACTIONS.EDIT}>
              <PessoaForm />
            </RequirePermission>
          } />
          <Route path="pessoas/:id" element={
            <RequirePermission resource={RESOURCES.PESSOAS}>
              <PessoaDetails />
            </RequirePermission>
          } />

          {/* Rotas de Perfis */}
          <Route path="perfis" element={
            <RequirePermission resource={RESOURCES.PERFIS}>
              <PerfisList />
            </RequirePermission>
          } />

          {/* Rotas de Visitantes */}
          <Route path="visitantes" element={
            <RequirePermission resource={RESOURCES.VISITANTES}>
              <VisitantesList />
            </RequirePermission>
          } />
          <Route path="visitantes/novo" element={
            <RequirePermission resource={RESOURCES.VISITANTES} action={ACTIONS.EDIT}>
              <VisitanteForm />
            </RequirePermission>
          } />
          <Route path="visitantes/:id/editar" element={
            <RequirePermission resource={RESOURCES.VISITANTES} action={ACTIONS.EDIT}>
              <VisitanteForm />
            </RequirePermission>
          } />
          <Route path="visitantes/:id" element={
            <RequirePermission resource={RESOURCES.VISITANTES}>
              <VisitanteDetails />
            </RequirePermission>
          } />
          
          {/* Rotas de Configurações de Mensagens */}
          <Route path="configuracoes-mensagens" element={
            <RequirePermission resource={RESOURCES.CONFIG_MENSAGENS}>
              <ConfiguracoesList />
            </RequirePermission>
          } />
          <Route path="configuracoes-mensagens/novo" element={
            <RequirePermission resource={RESOURCES.CONFIG_MENSAGENS} action={ACTIONS.EDIT}>
              <ConfiguracaoForm />
            </RequirePermission>
          } />
          <Route path="configuracoes-mensagens/editar/:id" element={
            <RequirePermission resource={RESOURCES.CONFIG_MENSAGENS} action={ACTIONS.EDIT}>
              <ConfiguracaoForm />
            </RequirePermission>
          } />
          
          {/* Rotas de Mensagens Agendadas */}
          <Route path="mensagens-agendadas" element={
            <RequirePermission resource={RESOURCES.MENSAGENS_AGENDADAS}>
              <MensagensAgendadas />
            </RequirePermission>
          } />

          {/* Rotas de Equipes */}
          <Route path="equipes" element={
            <RequirePermission resource={RESOURCES.EQUIPES}>
              <EquipesList />
            </RequirePermission>
          } />
          <Route path="equipes/novo" element={
            <RequirePermission resource={RESOURCES.EQUIPES} action={ACTIONS.EDIT}>
              <EquipeForm />
            </RequirePermission>
          } />
          <Route path="equipes/:id/editar" element={
            <RequirePermission resource={RESOURCES.EQUIPES} action={ACTIONS.EDIT}>
              <EquipeForm />
            </RequirePermission>
          } />

          {/* Rotas de Cargos */}
          <Route path="cargos" element={
            <RequirePermission resource={RESOURCES.CARGOS}>
              <CargosList />
            </RequirePermission>
          } />
          <Route path="cargos/novo" element={
            <RequirePermission resource={RESOURCES.CARGOS} action={ACTIONS.EDIT}>
              <CargoForm />
            </RequirePermission>
          } />
          <Route path="cargos/:id/editar" element={
            <RequirePermission resource={RESOURCES.CARGOS} action={ACTIONS.EDIT}>
              <CargoForm />
            </RequirePermission>
          } />

          {/* Rotas de Voluntários */}
          <Route path="voluntarios" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS}>
              <VoluntariosList />
            </RequirePermission>
          } />
          <Route path="voluntarios/novo" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS} action={ACTIONS.EDIT}>
              <VoluntarioForm />
            </RequirePermission>
          } />
          <Route path="voluntarios/:id/editar" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS} action={ACTIONS.EDIT}>
              <VoluntarioForm />
            </RequirePermission>
          } />
          <Route path="voluntariado/escalas" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS}>
              <EscalasList />
            </RequirePermission>
          } />
          <Route path="voluntariado/escalas/ocorrencia/:ocorrenciaId" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS} action={ACTIONS.EDIT}>
              <EscalaEditor />
            </RequirePermission>
          } />
          <Route path="voluntariado/relatorio-vinculos" element={
            <RequirePermission resource={RESOURCES.VOLUNTARIOS}>
              <RelatorioVinculosVoluntariado />
            </RequirePermission>
          } />

          {/* Rotas de Eventos */}
          <Route path="eventos" element={
            <RequirePermission resource={RESOURCES.EVENTOS}>
              <EventosList />
            </RequirePermission>
          } />
          <Route path="eventos/novo" element={
            <RequirePermission resource={RESOURCES.EVENTOS} action={ACTIONS.EDIT}>
              <EventoForm />
            </RequirePermission>
          } />
          <Route path="eventos/:id/editar" element={
            <RequirePermission resource={RESOURCES.EVENTOS} action={ACTIONS.EDIT}>
              <EventoForm />
            </RequirePermission>
          } />

          {/* Rotas de Destaques Site */}
          <Route path="destaques-site" element={
            <RequirePermission resource={RESOURCES.DESTAQUES_SITE}>
              <DestaquesSiteList />
            </RequirePermission>
          } />
          <Route path="destaques-site/novo" element={
            <RequirePermission resource={RESOURCES.DESTAQUES_SITE} action={ACTIONS.EDIT}>
              <DestaqueSiteForm />
            </RequirePermission>
          } />
          <Route path="destaques-site/:id/editar" element={
            <RequirePermission resource={RESOURCES.DESTAQUES_SITE} action={ACTIONS.EDIT}>
              <DestaqueSiteForm />
            </RequirePermission>
          } />
          <Route path="configuracao-portal" element={
            <RequirePermission resource={RESOURCES.PORTAL} action={ACTIONS.EDIT}>
              <ConfiguracaoPortal />
            </RequirePermission>
          } />

          {/* Rotas de Categorias de Notícias */}
          <Route path="categorias-noticias" element={
            <RequirePermission resource={RESOURCES.CATEGORIAS_NOTICIAS}>
              <CategoriasNoticiasList />
            </RequirePermission>
          } />
          <Route path="categorias-noticias/novo" element={
            <RequirePermission resource={RESOURCES.CATEGORIAS_NOTICIAS} action={ACTIONS.EDIT}>
              <CategoriaNoticiaForm />
            </RequirePermission>
          } />
          <Route path="categorias-noticias/:id/editar" element={
            <RequirePermission resource={RESOURCES.CATEGORIAS_NOTICIAS} action={ACTIONS.EDIT}>
              <CategoriaNoticiaForm />
            </RequirePermission>
          } />

          {/* Rotas de Notícias */}
          <Route path="noticias" element={
            <RequirePermission resource={RESOURCES.NOTICIAS}>
              <NoticiasList />
            </RequirePermission>
          } />
          <Route path="noticias/novo" element={
            <RequirePermission resource={RESOURCES.NOTICIAS} action={ACTIONS.EDIT}>
              <NoticiaForm />
            </RequirePermission>
          } />
          <Route path="noticias/:id/editar" element={
            <RequirePermission resource={RESOURCES.NOTICIAS} action={ACTIONS.EDIT}>
              <NoticiaForm />
            </RequirePermission>
          } />

          {/* Rotas de Contatos */}
          <Route path="contatos" element={
            <RequirePermission resource={RESOURCES.CONTATOS}>
              <ContatosList />
            </RequirePermission>
          } />
          <Route path="contatos/novo" element={
            <RequirePermission resource={RESOURCES.CONTATOS} action={ACTIONS.EDIT}>
              <ContatoForm />
            </RequirePermission>
          } />
          <Route path="contatos/:id/editar" element={
            <RequirePermission resource={RESOURCES.CONTATOS} action={ACTIONS.EDIT}>
              <ContatoForm />
            </RequirePermission>
          } />

          {/* Rotas de Inscrições em Eventos */}
          <Route path="inscricoes-eventos" element={
            <RequirePermission resource={RESOURCES.INSCRICOES_EVENTOS}>
              <InscricoesEventosList />
            </RequirePermission>
          } />
          <Route path="inscricoes-eventos/:id" element={
            <RequirePermission resource={RESOURCES.INSCRICOES_EVENTOS}>
              <InscricaoEventoDetails />
            </RequirePermission>
          } />
          <Route path="inscricoes-eventos/:id/editar" element={
            <RequirePermission resource={RESOURCES.INSCRICOES_EVENTOS} action={ACTIONS.EDIT}>
              <InscricaoEventoForm />
            </RequirePermission>
          } />
          <Route path="eventos/:eventoId/inscricoes" element={
            <RequirePermission resource={RESOURCES.INSCRICOES_EVENTOS}>
              <EventoInscricoes />
            </RequirePermission>
          } />

          {/* Rotas de Usuários */}
          <Route path="usuarios" element={
            <RequirePermission resource={RESOURCES.USUARIOS}>
              <UsuariosList />
            </RequirePermission>
          } />
          <Route path="auditoria" element={
            <RequirePermission resource={RESOURCES.USUARIOS}>
              <AuditoriaList />
            </RequirePermission>
          } />
          <Route path="perfis-acesso" element={
            <RequirePermission resource={RESOURCES.PERFIS_ACESSO}>
              <PerfisAcessoList />
            </RequirePermission>
          } />
          <Route path="perfis-acesso/novo" element={
            <RequirePermission resource={RESOURCES.PERFIS_ACESSO} action={ACTIONS.EDIT}>
              <PerfilAcessoForm />
            </RequirePermission>
          } />
          <Route path="perfis-acesso/:id/editar" element={
            <RequirePermission resource={RESOURCES.PERFIS_ACESSO} action={ACTIONS.EDIT}>
              <PerfilAcessoForm />
            </RequirePermission>
          } />

          {/* Rota de Perfil */}
          <Route path="perfil" element={<Perfil />} />

          {/* Rotas de Categorias de Mídia */}
          <Route path="categorias-midias" element={
            <RequirePermission resource={RESOURCES.MIDIA}>
              <CategoriasMidiasList />
            </RequirePermission>
          } />
          <Route path="categorias-midias/novo" element={
            <RequirePermission resource={RESOURCES.MIDIA} action={ACTIONS.EDIT}>
              <CategoriaMidiaForm />
            </RequirePermission>
          } />
          <Route path="categorias-midias/:id/editar" element={
            <RequirePermission resource={RESOURCES.MIDIA} action={ACTIONS.EDIT}>
              <CategoriaMidiaForm />
            </RequirePermission>
          } />

          {/* Rotas de Galerias de Fotos */}
          <Route path="galerias-fotos" element={
            <RequirePermission resource={RESOURCES.GALERIAS_FOTOS}>
              <GaleriasFotosList />
            </RequirePermission>
          } />
          <Route path="galerias-fotos/novo" element={
            <RequirePermission resource={RESOURCES.GALERIAS_FOTOS} action={ACTIONS.EDIT}>
              <GaleriaFotoForm />
            </RequirePermission>
          } />
          <Route path="galerias-fotos/:id/editar" element={
            <RequirePermission resource={RESOURCES.GALERIAS_FOTOS} action={ACTIONS.EDIT}>
              <GaleriaFotoForm />
            </RequirePermission>
          } />
          <Route path="galerias-fotos/:id/fotos" element={
            <RequirePermission resource={RESOURCES.GALERIAS_FOTOS}>
              <GaleriaFotos />
            </RequirePermission>
          } />

          {/* Rotas de Enquetes */}
          <Route path="enquetes" element={
            <RequirePermission resource={RESOURCES.ENQUETES}>
              <EnquetesList />
            </RequirePermission>
          } />
          <Route path="enquetes/novo" element={
            <RequirePermission resource={RESOURCES.ENQUETES} action={ACTIONS.EDIT}>
              <EnqueteForm />
            </RequirePermission>
          } />
          <Route path="enquetes/:id/editar" element={
            <RequirePermission resource={RESOURCES.ENQUETES} action={ACTIONS.EDIT}>
              <EnqueteForm />
            </RequirePermission>
          } />

          {/* Rotas de Kids */}
          <Route path="kids/checkins" element={
            <RequirePermission resource={RESOURCES.KIDS}>
              <KidsCheckinsList />
            </RequirePermission>
          } />

          {/* Rotas de Hub - Casas */}
          <Route path="hub/casas" element={
            <RequirePermission resource={RESOURCES.HUB}>
              <CasasList />
            </RequirePermission>
          } />
          <Route path="hub/casas/novo" element={
            <RequirePermission resource={RESOURCES.HUB} action={ACTIONS.EDIT}>
              <CasaForm />
            </RequirePermission>
          } />
          <Route path="hub/casas/:id/editar" element={
            <RequirePermission resource={RESOURCES.HUB} action={ACTIONS.EDIT}>
              <CasaForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Fornecedores */}
          <Route path="financeiro/fornecedores" element={
            <RequirePermission resource={RESOURCES.FORNECEDORES}>
              <FornecedoresList />
            </RequirePermission>
          } />
          <Route path="financeiro/fornecedores/novo" element={
            <RequirePermission resource={RESOURCES.FORNECEDORES} action={ACTIONS.EDIT}>
              <FornecedorForm />
            </RequirePermission>
          } />
          <Route path="financeiro/fornecedores/:id/editar" element={
            <RequirePermission resource={RESOURCES.FORNECEDORES} action={ACTIONS.EDIT}>
              <FornecedorForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Categorias de Despesas */}
          <Route path="financeiro/categorias-despesas" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <CategoriasDespesasList />
            </RequirePermission>
          } />
          <Route path="financeiro/categorias-despesas/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CategoriaDespesaForm />
            </RequirePermission>
          } />
          <Route path="financeiro/categorias-despesas/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CategoriaDespesaForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Contas Bancárias */}
          <Route path="financeiro/contas-bancarias" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <ContasBancariasList />
            </RequirePermission>
          } />
          <Route path="financeiro/contas-bancarias/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ContaBancariaForm />
            </RequirePermission>
          } />
          <Route path="financeiro/contas-bancarias/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ContaBancariaForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Centros de Custos */}
          <Route path="financeiro/centros-custos" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <CentrosCustosList />
            </RequirePermission>
          } />
          <Route path="financeiro/centros-custos/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CentroCustoForm />
            </RequirePermission>
          } />
          <Route path="financeiro/centros-custos/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CentroCustoForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Projetos */}
          <Route path="financeiro/projetos" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <ProjetosList />
            </RequirePermission>
          } />
          <Route path="financeiro/projetos/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ProjetoForm />
            </RequirePermission>
          } />
          <Route path="financeiro/projetos/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ProjetoForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Despesas */}
          <Route path="financeiro/despesas" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <DespesasList />
            </RequirePermission>
          } />
          <Route path="financeiro/despesas/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <DespesaForm />
            </RequirePermission>
          } />
          <Route path="financeiro/despesas/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <DespesaForm />
            </RequirePermission>
          } />

          {/* Rotas de Financeiro - Receitas */}
          <Route path="financeiro/receitas" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <ReceitasList />
            </RequirePermission>
          } />
          <Route path="financeiro/receitas/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ReceitaForm />
            </RequirePermission>
          } />
          <Route path="financeiro/receitas/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <ReceitaForm />
            </RequirePermission>
          } />
          {/* Rotas de Financeiro - Categorias de Receitas */}
          <Route path="financeiro/categorias-receitas" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <CategoriasReceitasList />
            </RequirePermission>
          } />
          <Route path="financeiro/categorias-receitas/novo" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CategoriaReceitaForm />
            </RequirePermission>
          } />
          <Route path="financeiro/categorias-receitas/:id/editar" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO} action={ACTIONS.EDIT}>
              <CategoriaReceitaForm />
            </RequirePermission>
          } />
          {/* Rotas de Financeiro - Dashboard */}
          <Route path="financeiro/dashboard" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <DashboardFinanceiro />
            </RequirePermission>
          } />
          {/* Rotas de Financeiro - Relatórios */}
          <Route path="financeiro/relatorios" element={
            <RequirePermission resource={RESOURCES.FINANCEIRO}>
              <RelatoriosFinanceiros />
            </RequirePermission>
          } />
        </Route>
        
        {/* Redirecionar rotas não encontradas para login ou dashboard */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      </Suspense>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
