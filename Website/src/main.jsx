import React, { useState } from "react";
import { createRoot } from "react-dom/client";
import { motion } from "framer-motion";
import {
  ArrowRight,
  BarChart3,
  CalendarDays,
  CheckCircle2,
  ChevronRight,
  Church,
  ClipboardCheck,
  HeartHandshake,
  LockKeyhole,
  Menu,
  MessageCircle,
  Send,
  ShieldCheck,
  Smartphone,
  Sparkles,
  Users,
  UserRoundCheck,
  Workflow,
  X,
} from "lucide-react";
import "./index.css";

const WHATSAPP_NUMBER = import.meta.env.VITE_WHATSAPP_NUMBER || "";
const APP_URL = (import.meta.env.VITE_APP_URL || "").replace(/\/$/, "");
const WHATSAPP_MESSAGE =
  "Olá, quero conhecer o Verbo+ e entender como ele pode ajudar a organização da minha igreja.";

function getWhatsappUrl() {
  const digits = WHATSAPP_NUMBER.replace(/\D/g, "");
  return digits
    ? `https://wa.me/${digits}?text=${encodeURIComponent(WHATSAPP_MESSAGE)}`
    : "#interesse";
}

function getSignupUrl(plano = "organizacao") {
  return APP_URL ? `${APP_URL}/signup?plano=${plano}` : "#interesse";
}

function Logo({ light = false }) {
  return (
    <img
      src={light ? "/verbo-brand/verbo-logo-light-transparent.png" : "/verbo-brand/verbo-logo-dark-transparent.png"}
      alt="Verbo+ - Organize sua igreja. Transforme vidas."
      className="h-14 w-auto object-contain sm:h-16"
    />
  );
}

const painPoints = [
  "Visitantes chegam, mas nem sempre recebem acompanhamento estruturado.",
  "Escalas, equipes e voluntários ficam espalhados em planilhas e grupos.",
  "Dados de membros, aniversários e contatos se perdem ou ficam desatualizados.",
  "Líderes tomam decisões sem uma visão clara da rotina da igreja.",
];

const productModules = [
  {
    icon: Users,
    title: "Gestão de pessoas",
    text: "Cadastro de membros, visitantes, líderes e famílias com dados organizados para acompanhamento pastoral.",
    items: ["Membros e visitantes", "Histórico de contato", "Aniversariantes", "Perfis e vínculos familiares"],
  },
  {
    icon: HeartHandshake,
    title: "Voluntários e ministérios",
    text: "Organize equipes, ministérios, cargos e disponibilidade para servir com menos ruído operacional.",
    items: ["Equipes e funções", "Disponibilidade", "Líderes responsáveis", "Visão por ministério"],
  },
  {
    icon: ClipboardCheck,
    title: "Escalas inteligentes",
    text: "Planeje escalas, confirme participações e reduza improvisos nos cultos, eventos e departamentos.",
    items: ["Escalas por data", "Confirmação de presença", "Substituições", "Histórico de serviço"],
  },
  {
    icon: CalendarDays,
    title: "Eventos e inscrições",
    text: "Crie eventos, acompanhe inscrições e entenda a participação da igreja em cada programação.",
    items: ["Eventos", "Inscrições", "Check-in", "Relatórios de participação"],
  },
  {
    icon: MessageCircle,
    title: "Comunicação eficiente",
    text: "Prepare lembretes, mensagens e fluxos de contato para manter pessoas próximas e bem cuidadas.",
    items: ["Mensagens por grupo", "Lembretes", "Acompanhamentos", "Integração futura com WhatsApp"],
  },
  {
    icon: BarChart3,
    title: "Decisões inteligentes",
    text: "Dashboards e indicadores para enxergar crescimento, engajamento, visitas, eventos e voluntariado.",
    items: ["Indicadores", "Relatórios", "Tendências", "Visão por igreja ou congregação"],
  },
  {
    icon: ShieldCheck,
    title: "Permissões e segurança",
    text: "Controle quem acessa cada área, mantendo dados sensíveis protegidos e equipes no contexto certo.",
    items: ["Perfis de acesso", "Permissões por módulo", "Ambiente seguro", "Dados centralizados"],
  },
  {
    icon: Workflow,
    title: "Multi-igreja e expansão",
    text: "Estrutura preparada para igrejas, sedes, congregações e redes que precisam crescer com organização.",
    items: ["Multi-tenant", "Congregações", "Painel consolidado", "Base escalável"],
  },
];

const stats = [
  ["32", "Total de Pessoas"],
  ["22", "Total de Voluntários"],
  ["5", "Total de Eventos"],
  ["3", "Aniversariantes (30 dias)"],
];

const planIdeas = [
  {
    name: "Essencial",
    slug: "essencial",
    description: "Para igrejas que estão saindo das planilhas e precisam organizar o básico com clareza e sem complexidade.",
    features: ["Pessoas e visitantes", "Eventos e inscrições", "Aniversariantes", "Relatórios iniciais"],
  },
  {
    name: "Organização",
    slug: "organizacao",
    description: "Para igrejas com ministérios ativos, equipes escaladas e comunicação que não pode falhar no dia a dia.",
    features: ["Tudo do Essencial", "Voluntários e ministérios", "Escalas inteligentes", "Comunicação estruturada", "Dashboards e indicadores"],
    highlighted: true,
  },
  {
    name: "Crescimento",
    slug: "crescimento",
    description: "Para igrejas com múltiplas congregações ou necessidade de controle avançado de acesso e relatórios.",
    features: ["Tudo do Organização", "Multi-congregação", "Permissões avançadas", "Relatórios consolidados", "Suporte prioritário"],
  },
];

const faqs = [
  {
    q: "O Verbo+ já está disponível para contratação?",
    a: "Sim. Você pode criar sua conta agora e experimentar gratuitamente por 7 dias, sem precisar cadastrar cartão de crédito. Após o período de trial, escolha o plano que melhor se encaixa na sua igreja.",
  },
  {
    q: "Preciso decidir um plano agora?",
    a: "Não. Os 7 dias de trial oferecem acesso completo à plataforma. Após esse período, você escolhe o plano que melhor atende o tamanho e as necessidades da sua equipe. É possível cancelar a qualquer momento.",
  },
  {
    q: "Vai funcionar para uma igreja pequena?",
    a: "Sim. O Verbo+ atende igrejas locais que querem sair das planilhas e igrejas maiores com múltiplas congregações e equipes. O plano certo depende do tamanho e dos módulos que sua equipe vai usar.",
  },
  {
    q: "Como funciona o pagamento?",
    a: "A assinatura é mensal ou anual, renovada automaticamente. O pagamento pode ser feito por cartão de crédito, Pix ou boleto bancário.",
  },
  {
    q: "O Verbo+ tem integração com WhatsApp?",
    a: "A integração com WhatsApp está no roadmap para lembretes, confirmações de escala e acompanhamento pastoral. Hoje já usamos o WhatsApp como canal de atendimento para facilitar sua avaliação da plataforma.",
  },
];

function AppWindow() {
  return (
    <div className="overflow-hidden rounded-2xl border border-white/10 bg-[#0F172A] shadow-2xl shadow-blue-950/40">
      <div className="grid gap-4 p-4 lg:grid-cols-[155px_1fr]">
        <aside className="hidden rounded-xl border border-white/10 bg-white/[0.04] p-3 lg:block">
          {["Dashboard", "Pessoas", "Voluntariado", "Escalas", "Eventos", "Comunicação", "Financeiro"].map((item, index) => (
            <div
              key={item}
              className={`mb-1.5 rounded-lg px-3 py-2 text-sm ${
                index === 0
                  ? "bg-gradient-to-r from-[#7C3AED] to-[#2563EB] font-semibold text-white"
                  : "text-slate-400"
              }`}
            >
              {item}
            </div>
          ))}
        </aside>

        <div>
          <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-xs font-medium text-violet-400">Visão geral</p>
              <h3 className="text-lg font-bold text-white">Cuidado visível, dados claros.</h3>
            </div>
            <span className="w-fit rounded-full bg-emerald-400/10 px-3 py-1 text-xs font-semibold text-emerald-300">
              +12% engajamento
            </span>
          </div>

          <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
            {stats.map(([value, label]) => (
              <div key={label} className="rounded-xl border border-white/10 bg-white/[0.06] p-3">
                <p className="text-xl font-bold text-white">{value}</p>
                <p className="mt-0.5 text-xs text-slate-400">{label}</p>
              </div>
            ))}
          </div>

          <div className="mt-3 grid gap-3 lg:grid-cols-[1.4fr_1fr]">
            <div className="rounded-xl border border-white/10 bg-white/[0.06] p-4">
              <div className="mb-3 flex items-center justify-between">
                <p className="text-sm font-semibold text-white">Comunicação (WhatsApp)</p>
                <p className="text-xs text-slate-400">Este mês</p>
              </div>
              <div className="mb-3 flex gap-4 text-xs">
                <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-violet-400 inline-block" />7 Entregues</span>
                <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-blue-400 inline-block" />0 Pendentes</span>
                <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-red-400 inline-block" />0 Falhas</span>
              </div>
              <div className="flex h-20 items-end gap-1.5">
                {[30, 45, 38, 55, 50, 70, 62, 88].map((height, index) => (
                  <div
                    key={index}
                    className="flex-1 rounded-t-sm bg-gradient-to-t from-[#7C3AED] to-[#2563EB]"
                    style={{ height: `${height}%` }}
                  />
                ))}
              </div>
            </div>
            <div className="rounded-xl border border-white/10 bg-white/[0.06] p-4">
              <p className="mb-3 text-sm font-semibold text-white">Próximos aniversariantes</p>
              {[
                ["M", "Marta Silva", "27/06"],
                ["P", "Pedro Borges", "23/07"],
                ["T", "Tatiane Souza", "23/07"],
              ].map(([inicial, nome, data]) => (
                <div key={nome} className="mb-2 flex items-center gap-2 text-xs text-slate-300">
                  <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-violet-500/30 text-xs font-bold text-violet-300">{inicial}</span>
                  <span className="flex-1 truncate">{nome}</span>
                  <span className="text-slate-500">{data}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function InterestForm() {
  const [sent, setSent] = useState(false);

  if (sent) {
    return (
      <div className="rounded-2xl border border-emerald-400/30 bg-emerald-400/10 p-8 text-center">
        <CheckCircle2 className="mx-auto h-12 w-12 text-emerald-300" />
        <h3 className="mt-4 text-2xl font-bold text-white">Interesse recebido!</h3>
        <p className="mt-2 text-slate-300">
          Entraremos em contato pelo WhatsApp ou e-mail para uma conversa sobre o Verbo+ e o que ele pode fazer pela sua igreja.
        </p>
      </div>
    );
  }

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        setSent(true);
      }}
      className="rounded-2xl border border-white/10 bg-white/[0.06] p-6 shadow-2xl shadow-blue-950/30 backdrop-blur"
    >
      <div className="grid gap-4 md:grid-cols-2">
        {[
          ["Nome", "text"],
          ["Igreja", "text"],
          ["Cargo ou função", "text"],
          ["WhatsApp", "tel"],
          ["E-mail", "email"],
          ["Cidade/UF", "text"],
        ].map(([label, type]) => (
          <label key={label} className="block">
            <span className="mb-2 block text-sm font-medium text-slate-300">{label}</span>
            <input
              type={type}
              required
              className="w-full rounded-xl border border-white/10 bg-[#0F172A]/80 px-4 py-3 text-white outline-none transition focus:border-violet-400"
            />
          </label>
        ))}
        <label className="block">
          <span className="mb-2 block text-sm font-medium text-slate-300">Tamanho aproximado</span>
          <select className="w-full rounded-xl border border-white/10 bg-[#0F172A]/80 px-4 py-3 text-white outline-none transition focus:border-violet-400">
            <option>Até 100 pessoas</option>
            <option>100 a 300 pessoas</option>
            <option>300 a 800 pessoas</option>
            <option>Mais de 800 pessoas</option>
          </select>
        </label>
        <label className="block">
          <span className="mb-2 block text-sm font-medium text-slate-300">Principal interesse</span>
          <select className="w-full rounded-xl border border-white/10 bg-[#0F172A]/80 px-4 py-3 text-white outline-none transition focus:border-violet-400">
            <option>Gestão de pessoas</option>
            <option>Voluntários e escalas</option>
            <option>Eventos e inscrições</option>
            <option>Comunicação</option>
            <option>Multi-congregação</option>
          </select>
        </label>
        <label className="block md:col-span-2">
          <span className="mb-2 block text-sm font-medium text-slate-300">O que mais precisa organizar hoje?</span>
          <textarea
            rows={4}
            className="w-full rounded-xl border border-white/10 bg-[#0F172A]/80 px-4 py-3 text-white outline-none transition focus:border-violet-400"
            placeholder="Ex.: visitantes, membros, escalas, eventos, comunicação, relatórios..."
          />
        </label>
      </div>
      <button className="mt-5 flex w-full items-center justify-center gap-2 rounded-xl bg-gradient-to-r from-[#7C3AED] to-[#2563EB] px-6 py-4 font-bold text-white shadow-lg shadow-blue-500/20 transition hover:scale-[1.01]">
        Quero conhecer o Verbo+
        <Send className="h-4 w-4" />
      </button>
    </form>
  );
}

function VerboLandingPage() {
  const [open, setOpen] = useState(false);
  const whatsappUrl = getWhatsappUrl();
  const signupUrl = getSignupUrl();

  return (
    <div className="min-h-screen bg-[#0F172A] text-white">
      <header className="sticky top-0 z-50 border-b border-white/10 bg-[#030817]/95 backdrop-blur-xl">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-5 py-3">
          <a href="#inicio" aria-label="Verbo+ início">
            <Logo light />
          </a>
          <nav className="hidden items-center gap-7 text-sm text-slate-300 lg:flex">
            <a href="#problema" className="hover:text-white">Por que</a>
            <a href="#modulos" className="hover:text-white">Módulos</a>
            <a href="#planos" className="hover:text-white">Planos</a>
            <a href="#interesse" className="hover:text-white">Contato</a>
          </nav>
          <a
            href={signupUrl}
            className="hidden rounded-xl bg-white px-5 py-2.5 text-sm font-bold text-[#0F172A] transition hover:bg-violet-50 lg:inline-flex"
          >
            Começar grátis
          </a>
          <button onClick={() => setOpen(!open)} className="lg:hidden" aria-label="Abrir menu">
            {open ? <X /> : <Menu />}
          </button>
        </div>
        {open && (
          <div className="border-t border-white/10 px-5 py-4 lg:hidden">
            <div className="flex flex-col gap-4 text-slate-300">
              <a href="#problema" onClick={() => setOpen(false)}>Por que</a>
              <a href="#modulos" onClick={() => setOpen(false)}>Módulos</a>
              <a href="#planos" onClick={() => setOpen(false)}>Planos</a>
              <a href="#interesse" onClick={() => setOpen(false)}>Contato</a>
            </div>
          </div>
        )}
      </header>

      <main>
        <section id="inicio" className="mx-auto grid max-w-7xl items-center gap-12 px-5 py-16 lg:grid-cols-[0.88fr_1.12fr] lg:py-20">
          <motion.div initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-violet-400/30 bg-violet-500/10 px-4 py-2 text-sm font-medium text-violet-200">
              <Sparkles className="h-4 w-4" />
              7 dias grátis · Sem cartão de crédito
            </div>
            <h1 className="max-w-4xl text-5xl font-black leading-[1.02] tracking-tight sm:text-6xl lg:text-7xl">
              Organize sua igreja.
              <span className="block bg-gradient-to-r from-[#7C3AED] to-[#2563EB] bg-clip-text text-transparent">
                Transforme vidas.
              </span>
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-300">
              O Verbo+ centraliza pessoas, visitantes, voluntários, escalas, eventos, comunicação e indicadores para que sua igreja cresça com ordem, clareza e cuidado.
            </p>
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <a href={signupUrl} className="inline-flex items-center justify-center gap-2 rounded-xl bg-gradient-to-r from-[#7C3AED] to-[#2563EB] px-7 py-4 font-bold text-white shadow-lg shadow-blue-500/20 transition hover:scale-[1.02]">
                Começar grátis <ArrowRight className="h-4 w-4" />
              </a>
              <a href={whatsappUrl} target={whatsappUrl.startsWith("http") ? "_blank" : undefined} rel="noreferrer" className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/15 bg-white/5 px-7 py-4 font-bold text-white transition hover:bg-white/10">
                <MessageCircle className="h-4 w-4" /> Falar pelo WhatsApp
              </a>
            </div>
            <div className="mt-8 grid max-w-xl gap-3 sm:grid-cols-3">
              {["Gestão completa", "Comunicação eficiente", "Decisões inteligentes"].map((item) => (
                <div key={item} className="flex items-center gap-2 text-sm font-medium text-slate-300">
                  <CheckCircle2 className="h-4 w-4 shrink-0 text-violet-400" />
                  {item}
                </div>
              ))}
            </div>
          </motion.div>

          <motion.div initial={{ opacity: 0, scale: 0.97 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 0.6, delay: 0.1 }}>
            <AppWindow />
          </motion.div>
        </section>

        <section id="problema" className="border-y border-white/10 bg-[#111827] px-5 py-16">
          <div className="mx-auto grid max-w-7xl gap-10 lg:grid-cols-[0.8fr_1.2fr] lg:items-center">
            <div>
              <p className="mb-3 text-sm font-bold uppercase tracking-[0.28em] text-violet-400">O desafio</p>
              <h2 className="text-3xl font-black tracking-tight sm:text-4xl">
                A igreja cuida de pessoas. A gestão precisa ajudar, não atrapalhar.
              </h2>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              {painPoints.map((item) => (
                <div key={item} className="rounded-xl border border-white/10 bg-white/[0.04] p-5">
                  <ChevronRight className="mb-4 h-5 w-5 text-violet-400" />
                  <p className="leading-7 text-slate-300">{item}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section id="modulos" className="mx-auto max-w-7xl px-5 py-20">
          <div className="mb-10 max-w-3xl">
            <p className="mb-3 text-sm font-bold uppercase tracking-[0.28em] text-violet-400">O que está incluído</p>
            <h2 className="text-4xl font-black tracking-tight sm:text-5xl">
              Módulos pensados para a rotina real da sua equipe.
            </h2>
            <p className="mt-5 text-lg leading-8 text-slate-300">
              Do cadastro de membros à confirmação de voluntários — cada módulo resolve um desafio real da sua equipe, sem curva de aprendizado desnecessária.
            </p>
          </div>

          <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-4">
            {productModules.map((module, index) => (
              <motion.article
                key={module.title}
                initial={{ opacity: 0, y: 14 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.35, delay: index * 0.03 }}
                className="rounded-2xl border border-white/10 bg-white/[0.055] p-5 transition hover:-translate-y-1 hover:bg-white/[0.08]"
              >
                <module.icon className="h-8 w-8 text-violet-400" />
                <h3 className="mt-5 text-xl font-bold">{module.title}</h3>
                <p className="mt-3 text-sm leading-6 text-slate-300">{module.text}</p>
                <ul className="mt-5 space-y-2">
                  {module.items.map((item) => (
                    <li key={item} className="flex items-start gap-2 text-sm text-slate-300">
                      <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-blue-300" />
                      {item}
                    </li>
                  ))}
                </ul>
              </motion.article>
            ))}
          </div>
        </section>

        <section className="bg-white px-5 py-20 text-[#0F172A]">
          <div className="mx-auto grid max-w-7xl gap-12 lg:grid-cols-[0.9fr_1.1fr] lg:items-center">
            <div>
              <Logo />
              <p className="mt-8 text-sm font-bold uppercase tracking-[0.28em] text-blue-600">Proposta de valor</p>
              <h2 className="mt-3 text-4xl font-black tracking-tight sm:text-5xl">
                Uma plataforma para organizar o cuidado, a operação e o crescimento.
              </h2>
              <p className="mt-5 text-lg leading-8 text-slate-600">
                O Verbo+ não precisa prometer complexidade para parecer robusto. A força está em reunir o essencial da igreja em uma experiência simples, clara e preparada para escalar.
              </p>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              {[
                [Church, "Igrejas locais", "Organização diária sem depender de controles soltos."],
                [UserRoundCheck, "Pastores e líderes", "Visão clara para acompanhar pessoas e equipes."],
                [Smartphone, "Secretarias", "Rotina mais simples para cadastro, eventos e comunicação."],
                [LockKeyhole, "Redes e congregações", "Permissões e estrutura para crescimento ordenado."],
              ].map(([Icon, title, text]) => (
                <div key={title} className="rounded-2xl border border-slate-200 bg-slate-50 p-6">
                  <Icon className="h-8 w-8 text-blue-600" />
                  <h3 className="mt-5 text-xl font-bold">{title}</h3>
                  <p className="mt-2 leading-7 text-slate-600">{text}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section id="planos" className="mx-auto max-w-7xl px-5 py-20">
          <div className="mb-10 grid gap-6 lg:grid-cols-[0.85fr_1.15fr] lg:items-end">
            <div>
              <p className="mb-3 text-sm font-bold uppercase tracking-[0.28em] text-violet-400">Planos</p>
              <h2 className="text-4xl font-black tracking-tight sm:text-5xl">Um plano para cada estágio da sua igreja.</h2>
            </div>
            <p className="text-lg leading-8 text-slate-300">
              Comece com o que sua igreja precisa agora e expanda conforme o crescimento exigir. Todos os planos incluem 7 dias gratuitos para explorar a plataforma completa.
            </p>
          </div>

          <div className="grid gap-5 lg:grid-cols-3">
            {planIdeas.map((plan) => (
              <article
                key={plan.name}
                className={`flex flex-col rounded-2xl border p-6 ${
                  plan.highlighted
                    ? "border-violet-500/40 bg-gradient-to-br from-[#7C3AED]/30 to-[#2563EB]/20"
                    : "border-white/10 bg-white/[0.055]"
                }`}
              >
                <div className="flex items-center justify-between gap-4">
                  <h3 className="text-2xl font-bold">{plan.name}</h3>
                  {plan.highlighted && (
                    <span className="rounded-full bg-violet-400/20 px-3 py-1 text-xs font-bold text-violet-200">
                      mais escolhido
                    </span>
                  )}
                </div>
                <p className="mt-4 leading-7 text-slate-300">{plan.description}</p>
                <p className="mt-6 text-sm font-semibold uppercase tracking-[0.2em] text-violet-400">Inclui</p>
                <ul className="mt-4 grow space-y-3">
                  {plan.features.map((feature) => (
                    <li key={feature} className="flex items-start gap-3 text-slate-300">
                      <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-violet-400" />
                      {feature}
                    </li>
                  ))}
                </ul>
                <a
                  href={getSignupUrl(plan.slug)}
                  className={`mt-6 flex items-center justify-center gap-2 rounded-xl px-5 py-3 font-bold transition ${
                    plan.highlighted
                      ? "bg-gradient-to-r from-[#7C3AED] to-[#2563EB] text-white shadow-lg shadow-blue-500/20 hover:scale-[1.02]"
                      : "border border-white/20 text-white hover:bg-white/10"
                  }`}
                >
                  Começar grátis <ArrowRight className="h-4 w-4" />
                </a>
              </article>
            ))}
          </div>
        </section>

        <section id="interesse" className="mx-auto grid max-w-7xl gap-10 px-5 py-20 lg:grid-cols-[0.9fr_1.1fr]">
          <div>
            <p className="mb-3 text-sm font-bold uppercase tracking-[0.28em] text-violet-400">Próxima ação</p>
            <h2 className="text-4xl font-black tracking-tight sm:text-5xl">
              Comece agora ou fale com a gente.
            </h2>
            <p className="mt-5 text-lg leading-8 text-slate-300">
              Crie sua conta e experimente por 7 dias sem compromisso. Se preferir, preencha o formulário para receber uma apresentação personalizada.
            </p>
            <a
              href={signupUrl}
              className="mt-6 inline-flex items-center gap-2 rounded-xl bg-gradient-to-r from-[#7C3AED] to-[#2563EB] px-7 py-4 font-bold text-white shadow-lg shadow-blue-500/20 transition hover:scale-[1.02]"
            >
              Criar conta grátis <ArrowRight className="h-4 w-4" />
            </a>
            <div className="mt-6 rounded-2xl border border-white/10 bg-white/[0.05] p-6">
              <MessageCircle className="h-8 w-8 text-violet-400" />
              <h3 className="mt-4 text-xl font-bold">Prefere conversar antes?</h3>
              <p className="mt-2 leading-7 text-slate-300">
                Fale pelo WhatsApp para tirar dúvidas, conhecer os módulos e entender qual plano faz mais sentido para a sua igreja.
              </p>
              <a href={whatsappUrl} target={whatsappUrl.startsWith("http") ? "_blank" : undefined} rel="noreferrer" className="mt-5 inline-flex items-center gap-2 rounded-xl border border-white/15 px-5 py-3 font-bold transition hover:bg-white/10">
                Falar agora <ArrowRight className="h-4 w-4" />
              </a>
            </div>
          </div>
          <InterestForm />
        </section>

        <section className="mx-auto max-w-5xl px-5 pb-20">
          <h2 className="mb-8 text-center text-4xl font-black">Perguntas frequentes</h2>
          <div className="space-y-4">
            {faqs.map((faq) => (
              <details key={faq.q} className="rounded-2xl border border-white/10 bg-white/[0.055] p-5">
                <summary className="cursor-pointer list-none font-bold text-white">{faq.q}</summary>
                <p className="mt-3 leading-7 text-slate-300">{faq.a}</p>
              </details>
            ))}
          </div>
        </section>
      </main>

      <footer className="border-t border-white/10 bg-[#030817] px-5 py-10">
        <div className="mx-auto flex max-w-7xl flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
          <Logo light />
          <p className="text-sm text-slate-400">© 2026 Verbo+. Organize sua igreja. Transforme vidas.</p>
        </div>
      </footer>

      <a
        href={whatsappUrl}
        target={whatsappUrl.startsWith("http") ? "_blank" : undefined}
        rel="noreferrer"
        className="fixed bottom-5 right-5 z-50 flex items-center gap-2 rounded-full bg-gradient-to-r from-[#7C3AED] to-[#2563EB] px-5 py-4 font-bold text-white shadow-2xl shadow-violet-500/30 transition hover:scale-105"
      >
        <MessageCircle className="h-5 w-5" />
        <span className="hidden sm:inline">WhatsApp</span>
      </a>
    </div>
  );
}

createRoot(document.getElementById("root")).render(<VerboLandingPage />);
