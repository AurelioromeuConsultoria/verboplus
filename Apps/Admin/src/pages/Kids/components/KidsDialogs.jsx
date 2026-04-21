import React from 'react';
import { CheckCircle2, PhoneCall } from 'lucide-react';
import Loading from '../../../components/ui/loading';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { useTranslation } from 'react-i18next';
import { EstadoVazio } from './KidsShared';
import { OCORRENCIA_TIPOS, formatOcorrenciaTipo, getOcorrenciaStatusConfig, isOcorrenciaEncerrada } from './kidsHelpers';

export function CriancaDialog({
  open,
  onOpenChange,
  form,
  onChange,
  onSave,
  saving,
  salas,
  turmas,
}) {
  const { t } = useTranslation();
  const turmasDaSala = form.salaId ? turmas.filter((item) => item.salaId === form.salaId) : [];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{t('kids.children.new')}</DialogTitle>
          <DialogDescription>
            {t('kids.children.dialogDescription')}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="criancaNome">{t('kids.common.name')}</Label>
              <Input id="criancaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder={t('kids.children.namePlaceholder')} maxLength={100} />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="criancaNascimento">{t('kids.children.birthDate')}</Label>
              <Input id="criancaNascimento" type="date" value={form.dataNascimento} onChange={(e) => onChange('dataNascimento', e.target.value)} />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="criancaSala">{t('kids.common.room')}</Label>
              <Select value={form.salaId || 'selecionar'} onValueChange={(value) => onChange('salaId', value === 'selecionar' ? '' : value)}>
                <SelectTrigger id="criancaSala">
                  <SelectValue placeholder={t('kids.common.selectRoom')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="selecionar">{t('kids.common.selectRoom')}</SelectItem>
                  {salas.map((sala) => (
                    <SelectItem key={sala.id} value={sala.id}>
                      {sala.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="criancaTurma">{t('kids.common.class')}</Label>
              <Select value={form.turmaId || 'selecionar'} onValueChange={(value) => onChange('turmaId', value === 'selecionar' ? '' : value)}>
                <SelectTrigger id="criancaTurma">
                  <SelectValue placeholder={t('kids.common.selectClass')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="selecionar">{t('kids.common.selectClass')}</SelectItem>
                  {turmasDaSala.map((turma) => (
                    <SelectItem key={turma.id} value={turma.id}>
                      {turma.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="criancaAlergias">{t('kids.children.allergies')}</Label>
              <Textarea id="criancaAlergias" value={form.alergias} onChange={(e) => onChange('alergias', e.target.value)} rows={3} maxLength={500} />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="criancaRestricoes">{t('kids.children.foodRestrictions')}</Label>
              <Textarea id="criancaRestricoes" value={form.restricoesAlimentares} onChange={(e) => onChange('restricoesAlimentares', e.target.value)} rows={3} maxLength={500} />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="criancaObservacoes">{t('kids.common.notes')}</Label>
            <Textarea id="criancaObservacoes" value={form.observacoes} onChange={(e) => onChange('observacoes', e.target.value)} rows={4} maxLength={1000} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            {t('actions.cancel')}
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? t('actions.saving') : t('kids.children.create')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function OcorrenciaDialog({
  open,
  onOpenChange,
  form,
  onChange,
  onSave,
  saving,
  criancasPresentes,
}) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{t('kids.occurrence.register')}</DialogTitle>
          <DialogDescription>
            {t('kids.occurrence.dialogDescription')}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="criancaPessoaId">{t('kids.child')}</Label>
            <Select value={form.criancaPessoaId || 'selecionar'} onValueChange={(value) => onChange('criancaPessoaId', value === 'selecionar' ? '' : value)}>
              <SelectTrigger id="criancaPessoaId">
                <SelectValue placeholder={t('kids.occurrence.selectChild')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="selecionar">{t('kids.occurrence.selectChild')}</SelectItem>
                {criancasPresentes.map((crianca) => (
                  <SelectItem key={crianca.criancaPessoaId} value={String(crianca.criancaPessoaId)}>
                    {crianca.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="tipoOcorrencia">{t('kids.occurrence.type')}</Label>
              <Select value={form.tipo} onValueChange={(value) => onChange('tipo', value)}>
                <SelectTrigger id="tipoOcorrencia">
                  <SelectValue placeholder={t('kids.occurrence.selectType')} />
                </SelectTrigger>
                <SelectContent>
                  {OCORRENCIA_TIPOS.map((tipo) => (
                    <SelectItem key={tipo.value} value={tipo.value}>
                      {formatOcorrenciaTipo(tipo.value, t)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="tituloOcorrencia">{t('kids.occurrence.title')}</Label>
              <Input id="tituloOcorrencia" value={form.titulo} onChange={(e) => onChange('titulo', e.target.value)} placeholder={t('kids.occurrence.titlePlaceholder')} maxLength={200} />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="descricaoOcorrencia">{t('kids.common.description')}</Label>
            <Textarea id="descricaoOcorrencia" value={form.descricao} onChange={(e) => onChange('descricao', e.target.value)} placeholder={t('kids.occurrence.descriptionPlaceholder')} rows={5} maxLength={2000} />
          </div>

          <div className="grid gap-3 rounded-xl border border-border p-4">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">{t('kids.occurrence.requiresGuardianContact')}</p>
                <p className="text-sm text-muted-foreground">{t('kids.occurrence.requiresGuardianContactHint')}</p>
              </div>
              <Switch checked={form.requerContatoResponsavel} onCheckedChange={(checked) => onChange('requerContatoResponsavel', checked)} />
            </div>

            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">{t('kids.occurrence.visibleToGuardian')}</p>
                <p className="text-sm text-muted-foreground">{t('kids.occurrence.visibleToGuardianHint')}</p>
              </div>
              <Switch checked={form.visivelAoResponsavel} onCheckedChange={(checked) => onChange('visivelAoResponsavel', checked)} />
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            {t('actions.cancel')}
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? t('actions.saving') : t('kids.occurrence.register')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function SalaDialog({ open, onOpenChange, form, onChange, onSave, saving }) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('kids.structure.newRoom')}</DialogTitle>
          <DialogDescription>
            {t('kids.structure.roomDialogDescription')}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="salaId">{t('kids.common.identifier')}</Label>
            <Input id="salaId" value={form.id} onChange={(e) => onChange('id', e.target.value)} placeholder={t('kids.structure.roomIdentifierPlaceholder')} maxLength={50} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="salaNome">{t('kids.common.name')}</Label>
            <Input id="salaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder={t('kids.structure.roomNamePlaceholder')} maxLength={120} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="salaCapacidade">{t('kids.structure.maxCapacity')}</Label>
            <Input id="salaCapacidade" type="number" min="0" value={form.capacidadeMaxima} onChange={(e) => onChange('capacidadeMaxima', e.target.value)} placeholder={t('kids.structure.capacityPlaceholder.room')} />
          </div>
          <div className="flex items-center justify-between gap-3 rounded-xl border border-border p-4">
            <div>
              <p className="text-sm font-medium text-foreground">{t('kids.structure.roomActive')}</p>
              <p className="text-sm text-muted-foreground">{t('kids.structure.roomActiveHint')}</p>
            </div>
            <Switch checked={form.ativo} onCheckedChange={(checked) => onChange('ativo', checked)} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            {t('actions.cancel')}
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? t('actions.saving') : t('kids.structure.createRoom')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function TurmaDialog({ open, onOpenChange, form, onChange, onSave, saving, salas }) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('kids.structure.newClass')}</DialogTitle>
          <DialogDescription>
            {t('kids.structure.classDialogDescription')}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="turmaId">{t('kids.common.identifier')}</Label>
            <Input id="turmaId" value={form.id} onChange={(e) => onChange('id', e.target.value)} placeholder={t('kids.structure.classIdentifierPlaceholder')} maxLength={50} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaSala">{t('kids.common.room')}</Label>
            <Select value={form.salaId || 'selecionar'} onValueChange={(value) => onChange('salaId', value === 'selecionar' ? '' : value)}>
              <SelectTrigger id="turmaSala">
                <SelectValue placeholder={t('kids.common.selectRoom')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="selecionar">{t('kids.common.selectRoom')}</SelectItem>
                {salas.map((sala) => (
                  <SelectItem key={sala.id} value={sala.id}>
                    {sala.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaNome">{t('kids.common.name')}</Label>
            <Input id="turmaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder={t('kids.structure.classNamePlaceholder')} maxLength={120} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaCapacidade">{t('kids.structure.maxCapacity')}</Label>
            <Input id="turmaCapacidade" type="number" min="0" value={form.capacidadeMaxima} onChange={(e) => onChange('capacidadeMaxima', e.target.value)} placeholder={t('kids.structure.capacityPlaceholder.class')} />
          </div>
          <div className="flex items-center justify-between gap-3 rounded-xl border border-border p-4">
            <div>
              <p className="text-sm font-medium text-foreground">{t('kids.structure.classActive')}</p>
              <p className="text-sm text-muted-foreground">{t('kids.structure.classActiveHint')}</p>
            </div>
            <Switch checked={form.ativo} onCheckedChange={(checked) => onChange('ativo', checked)} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            {t('actions.cancel')}
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? t('actions.saving') : t('kids.structure.createClass')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function HistoricoDialog({
  open,
  onOpenChange,
  criancaHistorico,
  historicoLoading,
  ocorrenciasHistorico,
  historicoUpdatingId,
  onAtualizarOcorrencia,
  formatDate,
}) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-4xl">
        <DialogHeader>
          <DialogTitle>
            {t('kids.history.occurrenceHistory')}
            {criancaHistorico?.nome ? ` • ${criancaHistorico.nome}` : ''}
          </DialogTitle>
          <DialogDescription>
            {t('kids.history.dialogDescription')}
          </DialogDescription>
        </DialogHeader>

        {historicoLoading ? (
          <Loading text={t('kids.history.loadingOccurrences')} />
        ) : ocorrenciasHistorico.length ? (
          <div className="max-h-[60vh] space-y-4 overflow-y-auto pr-1">
            {ocorrenciasHistorico.map((ocorrencia) => {
              const statusConfig = getOcorrenciaStatusConfig(ocorrencia.status, t);
              const podeMarcarContato = ocorrencia.requerContatoResponsavel && !ocorrencia.contatoResponsavelRealizadoEm;
              const podeEncerrar = !isOcorrenciaEncerrada(ocorrencia.status);

              return (
                <div key={ocorrencia.id} className="rounded-xl border border-border bg-background p-4">
                  <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                    <div className="space-y-2">
                      <div className="flex flex-wrap items-center gap-2">
                        <Badge className={statusConfig.className}>{statusConfig.label}</Badge>
                        <Badge variant="outline">{formatOcorrenciaTipo(ocorrencia.tipo, t)}</Badge>
                        {ocorrencia.visivelAoResponsavel ? <Badge variant="outline">{t('kids.occurrence.visibleToGuardian')}</Badge> : null}
                      </div>
                      <h3 className="font-semibold text-foreground">{ocorrencia.titulo}</h3>
                      <p className="text-sm text-muted-foreground">{ocorrencia.descricao}</p>
                      <div className="flex flex-wrap gap-3 text-xs text-muted-foreground">
                        <span>{t('kids.history.recordedBy', { name: ocorrencia.registradoPorNome })}</span>
                        <span>{formatDate(ocorrencia.dataCriacao)}</span>
                        {ocorrencia.salaId ? <span>{t('kids.history.roomLabel', { room: ocorrencia.salaId })}</span> : null}
                      </div>
                    </div>

                    <div className="flex flex-wrap gap-2">
                      {podeMarcarContato ? (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={historicoUpdatingId === ocorrencia.id}
                          onClick={() => onAtualizarOcorrencia(ocorrencia.id, { contatoResponsavelRealizado: true })}
                        >
                          <PhoneCall className="mr-2 h-4 w-4" />
                          {t('kids.history.markContact')}
                        </Button>
                      ) : null}
                      {podeEncerrar ? (
                        <Button
                          size="sm"
                          disabled={historicoUpdatingId === ocorrencia.id}
                          onClick={() => onAtualizarOcorrencia(ocorrencia.id, { status: 'Encerrada' })}
                        >
                          <CheckCircle2 className="mr-2 h-4 w-4" />
                          {t('kids.history.close')}
                        </Button>
                      ) : null}
                    </div>
                  </div>

                  <div className="mt-4 grid gap-2 rounded-lg bg-muted/30 p-3 text-sm">
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <PhoneCall className="h-4 w-4" />
                      {ocorrencia.requerContatoResponsavel ? (
                        ocorrencia.contatoResponsavelRealizadoEm ? (
                          <span>
                            {t('kids.history.contactDoneOn', { date: formatDate(ocorrencia.contatoResponsavelRealizadoEm) })}
                            {ocorrencia.contatoResponsavelPorNome ? ` ${t('kids.history.by', { name: ocorrencia.contatoResponsavelPorNome })}` : ''}
                          </span>
                        ) : (
                          <span>{t('kids.history.contactPending')}</span>
                        )
                      ) : (
                        <span>{t('kids.history.contactNotRequired')}</span>
                      )}
                    </div>

                    {ocorrencia.encerradoEm ? (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <CheckCircle2 className="h-4 w-4" />
                        <span>
                          {t('kids.history.closedOn', { date: formatDate(ocorrencia.encerradoEm) })}
                          {ocorrencia.encerradoPorNome ? ` ${t('kids.history.by', { name: ocorrencia.encerradoPorNome })}` : ''}
                        </span>
                      </div>
                    ) : null}
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <EstadoVazio texto={t('kids.history.noOccurrencesForChild')} />
        )}
      </DialogContent>
    </Dialog>
  );
}
