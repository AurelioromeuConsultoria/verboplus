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
  const turmasDaSala = form.salaId ? turmas.filter((item) => item.salaId === form.salaId) : [];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Nova criança</DialogTitle>
          <DialogDescription>
            Cadastre a criança já usando a estrutura formal de sala e turma do Kids.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="criancaNome">Nome</Label>
              <Input id="criancaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder="Nome da criança" maxLength={100} />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="criancaNascimento">Data de nascimento</Label>
              <Input id="criancaNascimento" type="date" value={form.dataNascimento} onChange={(e) => onChange('dataNascimento', e.target.value)} />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="criancaSala">Sala</Label>
              <Select value={form.salaId || 'selecionar'} onValueChange={(value) => onChange('salaId', value === 'selecionar' ? '' : value)}>
                <SelectTrigger id="criancaSala">
                  <SelectValue placeholder="Selecione a sala" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="selecionar">Selecione a sala</SelectItem>
                  {salas.map((sala) => (
                    <SelectItem key={sala.id} value={sala.id}>
                      {sala.nome}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="criancaTurma">Turma</Label>
              <Select value={form.turmaId || 'selecionar'} onValueChange={(value) => onChange('turmaId', value === 'selecionar' ? '' : value)}>
                <SelectTrigger id="criancaTurma">
                  <SelectValue placeholder="Selecione a turma" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="selecionar">Selecione a turma</SelectItem>
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
              <Label htmlFor="criancaAlergias">Alergias</Label>
              <Textarea id="criancaAlergias" value={form.alergias} onChange={(e) => onChange('alergias', e.target.value)} rows={3} maxLength={500} />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="criancaRestricoes">Restrições alimentares</Label>
              <Textarea id="criancaRestricoes" value={form.restricoesAlimentares} onChange={(e) => onChange('restricoesAlimentares', e.target.value)} rows={3} maxLength={500} />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="criancaObservacoes">Observações</Label>
            <Textarea id="criancaObservacoes" value={form.observacoes} onChange={(e) => onChange('observacoes', e.target.value)} rows={4} maxLength={1000} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            Cancelar
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? 'Salvando...' : 'Cadastrar criança'}
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
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Registrar ocorrência</DialogTitle>
          <DialogDescription>
            Registre fatos relevantes do culto para manter a operação segura e rastreável.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="criancaPessoaId">Criança</Label>
            <Select value={form.criancaPessoaId || 'selecionar'} onValueChange={(value) => onChange('criancaPessoaId', value === 'selecionar' ? '' : value)}>
              <SelectTrigger id="criancaPessoaId">
                <SelectValue placeholder="Selecione a criança" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="selecionar">Selecione a criança</SelectItem>
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
              <Label htmlFor="tipoOcorrencia">Tipo</Label>
              <Select value={form.tipo} onValueChange={(value) => onChange('tipo', value)}>
                <SelectTrigger id="tipoOcorrencia">
                  <SelectValue placeholder="Selecione o tipo" />
                </SelectTrigger>
                <SelectContent>
                  {OCORRENCIA_TIPOS.map((tipo) => (
                    <SelectItem key={tipo.value} value={tipo.value}>
                      {tipo.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="tituloOcorrencia">Título</Label>
              <Input id="tituloOcorrencia" value={form.titulo} onChange={(e) => onChange('titulo', e.target.value)} placeholder="Resumo curto da ocorrência" maxLength={200} />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="descricaoOcorrencia">Descrição</Label>
            <Textarea id="descricaoOcorrencia" value={form.descricao} onChange={(e) => onChange('descricao', e.target.value)} placeholder="Descreva o que aconteceu, ações tomadas e contexto relevante." rows={5} maxLength={2000} />
          </div>

          <div className="grid gap-3 rounded-xl border border-border p-4">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">Requer contato com responsável</p>
                <p className="text-sm text-muted-foreground">Marque quando a equipe precisar avisar o responsável ainda durante o culto.</p>
              </div>
              <Switch checked={form.requerContatoResponsavel} onCheckedChange={(checked) => onChange('requerContatoResponsavel', checked)} />
            </div>

            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">Visível ao responsável</p>
                <p className="text-sm text-muted-foreground">Use apenas quando a informação fizer sentido no histórico do responsável.</p>
              </div>
              <Switch checked={form.visivelAoResponsavel} onCheckedChange={(checked) => onChange('visivelAoResponsavel', checked)} />
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            Cancelar
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? 'Salvando...' : 'Registrar ocorrência'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function SalaDialog({ open, onOpenChange, form, onChange, onSave, saving }) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Nova sala</DialogTitle>
          <DialogDescription>
            Cadastre uma sala formal para organizar capacidade e distribuição do Kids.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="salaId">Identificador</Label>
            <Input id="salaId" value={form.id} onChange={(e) => onChange('id', e.target.value)} placeholder="Ex.: SALA_BERCARIO" maxLength={50} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="salaNome">Nome</Label>
            <Input id="salaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder="Ex.: Berçário" maxLength={120} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="salaCapacidade">Capacidade máxima</Label>
            <Input id="salaCapacidade" type="number" min="0" value={form.capacidadeMaxima} onChange={(e) => onChange('capacidadeMaxima', e.target.value)} placeholder="Ex.: 12" />
          </div>
          <div className="flex items-center justify-between gap-3 rounded-xl border border-border p-4">
            <div>
              <p className="text-sm font-medium text-foreground">Sala ativa</p>
              <p className="text-sm text-muted-foreground">Salas inativas não entram no fluxo operacional padrão.</p>
            </div>
            <Switch checked={form.ativo} onCheckedChange={(checked) => onChange('ativo', checked)} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            Cancelar
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? 'Salvando...' : 'Cadastrar sala'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function TurmaDialog({ open, onOpenChange, form, onChange, onSave, saving, salas }) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Nova turma</DialogTitle>
          <DialogDescription>
            Vincule a turma a uma sala para preparar capacidade e organização por culto.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="turmaId">Identificador</Label>
            <Input id="turmaId" value={form.id} onChange={(e) => onChange('id', e.target.value)} placeholder="Ex.: MATERNAL_A" maxLength={50} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaSala">Sala</Label>
            <Select value={form.salaId || 'selecionar'} onValueChange={(value) => onChange('salaId', value === 'selecionar' ? '' : value)}>
              <SelectTrigger id="turmaSala">
                <SelectValue placeholder="Selecione a sala" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="selecionar">Selecione a sala</SelectItem>
                {salas.map((sala) => (
                  <SelectItem key={sala.id} value={sala.id}>
                    {sala.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaNome">Nome</Label>
            <Input id="turmaNome" value={form.nome} onChange={(e) => onChange('nome', e.target.value)} placeholder="Ex.: Maternal A" maxLength={120} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="turmaCapacidade">Capacidade máxima</Label>
            <Input id="turmaCapacidade" type="number" min="0" value={form.capacidadeMaxima} onChange={(e) => onChange('capacidadeMaxima', e.target.value)} placeholder="Ex.: 8" />
          </div>
          <div className="flex items-center justify-between gap-3 rounded-xl border border-border p-4">
            <div>
              <p className="text-sm font-medium text-foreground">Turma ativa</p>
              <p className="text-sm text-muted-foreground">Turmas inativas ficam fora da estrutura principal.</p>
            </div>
            <Switch checked={form.ativo} onCheckedChange={(checked) => onChange('ativo', checked)} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            Cancelar
          </Button>
          <Button onClick={onSave} disabled={saving}>
            {saving ? 'Salvando...' : 'Cadastrar turma'}
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
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-4xl">
        <DialogHeader>
          <DialogTitle>
            Histórico de ocorrências
            {criancaHistorico?.nome ? ` • ${criancaHistorico.nome}` : ''}
          </DialogTitle>
          <DialogDescription>
            Acompanhe o que foi registrado, se houve contato com responsável e o encerramento de cada item.
          </DialogDescription>
        </DialogHeader>

        {historicoLoading ? (
          <Loading text="Carregando ocorrências..." />
        ) : ocorrenciasHistorico.length ? (
          <div className="max-h-[60vh] space-y-4 overflow-y-auto pr-1">
            {ocorrenciasHistorico.map((ocorrencia) => {
              const statusConfig = getOcorrenciaStatusConfig(ocorrencia.status);
              const podeMarcarContato = ocorrencia.requerContatoResponsavel && !ocorrencia.contatoResponsavelRealizadoEm;
              const podeEncerrar = !isOcorrenciaEncerrada(ocorrencia.status);

              return (
                <div key={ocorrencia.id} className="rounded-xl border border-border bg-background p-4">
                  <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                    <div className="space-y-2">
                      <div className="flex flex-wrap items-center gap-2">
                        <Badge className={statusConfig.className}>{statusConfig.label}</Badge>
                        <Badge variant="outline">{formatOcorrenciaTipo(ocorrencia.tipo)}</Badge>
                        {ocorrencia.visivelAoResponsavel ? <Badge variant="outline">Visível ao responsável</Badge> : null}
                      </div>
                      <h3 className="font-semibold text-foreground">{ocorrencia.titulo}</h3>
                      <p className="text-sm text-muted-foreground">{ocorrencia.descricao}</p>
                      <div className="flex flex-wrap gap-3 text-xs text-muted-foreground">
                        <span>Registrado por {ocorrencia.registradoPorNome}</span>
                        <span>{formatDate(ocorrencia.dataCriacao)}</span>
                        {ocorrencia.salaId ? <span>Sala {ocorrencia.salaId}</span> : null}
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
                          Marcar contato
                        </Button>
                      ) : null}
                      {podeEncerrar ? (
                        <Button
                          size="sm"
                          disabled={historicoUpdatingId === ocorrencia.id}
                          onClick={() => onAtualizarOcorrencia(ocorrencia.id, { status: 'Encerrada' })}
                        >
                          <CheckCircle2 className="mr-2 h-4 w-4" />
                          Encerrar
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
                            Contato com responsável realizado em {formatDate(ocorrencia.contatoResponsavelRealizadoEm)}
                            {ocorrencia.contatoResponsavelPorNome ? ` por ${ocorrencia.contatoResponsavelPorNome}` : ''}
                          </span>
                        ) : (
                          <span>Contato com responsável ainda pendente.</span>
                        )
                      ) : (
                        <span>Esta ocorrência não exige contato com responsável.</span>
                      )}
                    </div>

                    {ocorrencia.encerradoEm ? (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <CheckCircle2 className="h-4 w-4" />
                        <span>
                          Encerrada em {formatDate(ocorrencia.encerradoEm)}
                          {ocorrencia.encerradoPorNome ? ` por ${ocorrencia.encerradoPorNome}` : ''}
                        </span>
                      </div>
                    ) : null}
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <EstadoVazio texto="Nenhuma ocorrência registrada para esta criança." />
        )}
      </DialogContent>
    </Dialog>
  );
}
