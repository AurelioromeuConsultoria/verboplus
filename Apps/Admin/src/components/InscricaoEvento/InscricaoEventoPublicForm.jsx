import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { inscricoesEventosApi } from '@/lib/api';
import { toast } from 'sonner';

export default function InscricaoEventoPublicForm({ eventoId, onSuccess, onCancel }) {
  const [formData, setFormData] = useState({
    nome: '',
    whatsApp: '',
    email: '',
    quantidadeAcompanhantes: 0,
    observacoes: '',
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: name === 'quantidadeAcompanhantes' ? Number(value) : value }));
  };

  const formatWhatsApp = (value) => {
    const numbers = value.replace(/\D/g, '');
    if (numbers.length <= 11) {
      if (numbers.length <= 2) return numbers;
      if (numbers.length <= 7) return `(${numbers.slice(0, 2)}) ${numbers.slice(2)}`;
      return `(${numbers.slice(0, 2)}) ${numbers.slice(2, 7)}-${numbers.slice(7)}`;
    }
    return value;
  };

  const handleWhatsAppChange = (e) => {
    const formatted = formatWhatsApp(e.target.value);
    setFormData((prev) => ({ ...prev, whatsApp: formatted }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.nome.trim()) {
      toast.error('Nome é obrigatório');
      return;
    }

    if (!formData.whatsApp.trim()) {
      toast.error('WhatsApp é obrigatório');
      return;
    }

    const whatsAppNumbers = formData.whatsApp.replace(/\D/g, '');
    if (whatsAppNumbers.length < 10 || whatsAppNumbers.length > 13) {
      toast.error('WhatsApp inválido. Use o formato (11) 99999-9999');
      return;
    }

    if (formData.email && !/.+@.+\..+/.test(formData.email)) {
      toast.error('Email inválido');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        eventoId: Number(eventoId),
        nome: formData.nome.trim(),
        whatsApp: whatsAppNumbers,
        email: formData.email?.trim() || null,
        quantidadeAcompanhantes: formData.quantidadeAcompanhantes || 0,
        observacoes: formData.observacoes?.trim() || null,
      };

      await inscricoesEventosApi.create(payload);
      toast.success('Inscrição realizada com sucesso! Aguarde a confirmação.');
      
      if (onSuccess) {
        onSuccess();
      }
    } catch (err) {
      let errorMessage = 'Erro ao realizar inscrição';
      if (err.response?.data?.message) {
        const msg = err.response.data.message;
        if (msg.includes('já iniciou')) {
          errorMessage = 'Este evento já iniciou e não aceita mais inscrições';
        } else if (msg.includes('já existe') || msg.includes('duplicada')) {
          errorMessage = 'Você já está inscrito neste evento';
        } else if (msg.includes('não encontrado')) {
          errorMessage = 'Evento não encontrado';
        } else {
          errorMessage = msg;
        }
      }
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Inscrever-se no Evento</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="nome">Nome Completo *</Label>
            <Input
              id="nome"
              name="nome"
              value={formData.nome}
              onChange={handleChange}
              placeholder="Seu nome completo"
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="whatsApp">WhatsApp *</Label>
            <Input
              id="whatsApp"
              name="whatsApp"
              value={formData.whatsApp}
              onChange={handleWhatsAppChange}
              placeholder="(11) 99999-9999"
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              name="email"
              type="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="seu@email.com"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="quantidadeAcompanhantes">Quantidade de Acompanhantes</Label>
            <Input
              id="quantidadeAcompanhantes"
              name="quantidadeAcompanhantes"
              type="number"
              min="0"
              value={formData.quantidadeAcompanhantes}
              onChange={handleChange}
              placeholder="0"
            />
            <p className="text-xs text-muted-foreground">Número de pessoas que virão junto com você</p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="observacoes">Observações</Label>
            <Textarea
              id="observacoes"
              name="observacoes"
              value={formData.observacoes}
              onChange={handleChange}
              placeholder="Alguma informação adicional que gostaria de compartilhar?"
              rows={3}
            />
          </div>

          <div className="flex items-center space-x-4">
            <Button type="submit" disabled={loading}>
              {loading ? 'Enviando...' : 'Inscrever-se'}
            </Button>
            {onCancel && (
              <Button type="button" variant="outline" onClick={onCancel}>
                Cancelar
              </Button>
            )}
          </div>
        </form>
      </CardContent>
    </Card>
  );
}







