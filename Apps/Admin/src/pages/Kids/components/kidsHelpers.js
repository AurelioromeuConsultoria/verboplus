export const OCORRENCIA_TIPOS = [
  { value: 'FEBRE', label: 'Febre' },
  { value: 'QUEDA', label: 'Queda' },
  { value: 'CHORO', label: 'Choro persistente' },
  { value: 'TROCA_SALA', label: 'Troca de sala' },
  { value: 'MEDICACAO', label: 'Medicação' },
  { value: 'COMPORTAMENTO', label: 'Comportamento' },
  { value: 'OUTRO', label: 'Outro' },
];

export const OCORRENCIA_STATUS = {
  aberta: { label: 'Aberta', className: 'bg-amber-500 hover:bg-amber-600' },
  em_andamento: { label: 'Em andamento', className: 'bg-blue-500 hover:bg-blue-600' },
  encerrada: { label: 'Encerrada', className: 'bg-emerald-600 hover:bg-emerald-700' },
};

export function formatOcorrenciaTipo(tipo) {
  const item = OCORRENCIA_TIPOS.find((entry) => entry.value === tipo);
  return item?.label || tipo;
}

export function getOcorrenciaStatusConfig(status) {
  const chave = (status || '').toLowerCase();
  return OCORRENCIA_STATUS[chave] || {
    label: status || 'Sem status',
    className: 'bg-slate-500 hover:bg-slate-600',
  };
}

export function isOcorrenciaEncerrada(status) {
  return (status || '').toLowerCase() === 'encerrada';
}

export function buildCriticalDescription(crianca, t) {
  const itens = [];
  if (crianca.temAlergia) itens.push(t('kids.panel.allergy', 'Alergia'));
  if (crianca.temRestricao) itens.push(t('kids.panel.restriction', 'Restrição'));
  if (crianca.temObservacaoCritica) itens.push(t('kids.panel.criticalNote', 'Observação crítica'));
  return itens.join(' • ');
}
