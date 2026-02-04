import React, { useState, useEffect } from 'react';
import { Save, Settings, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { configuracaoPortalApi } from '../../lib/api';
import { toast } from 'sonner';
import Loading from '../../components/ui/loading';

const ConfiguracaoPortal = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formData, setFormData] = useState({
    tempoTransicaoCarrossel: 5,
  });

  useEffect(() => {
    loadConfiguracao();
  }, []);

  const loadConfiguracao = async () => {
    try {
      setLoading(true);
      const response = await configuracaoPortalApi.get();
      const config = response.data;
      setFormData({
        tempoTransicaoCarrossel: config.tempoTransicaoCarrossel || 5,
      });
    } catch (err) {
      console.error('Erro ao carregar configuração:', err);
      toast.error('Erro ao carregar configuração do portal');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (formData.tempoTransicaoCarrossel < 1 || formData.tempoTransicaoCarrossel > 60) {
      toast.error('Tempo de transição deve estar entre 1 e 60 segundos');
      return;
    }

    try {
      setSaving(true);
      await configuracaoPortalApi.update(formData);
      toast.success('Configuração do portal atualizada com sucesso!');
    } catch (err) {
      console.error('Erro ao salvar configuração:', err);
      toast.error('Erro ao salvar configuração do portal');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Loading />;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold flex items-center gap-2">
          <Settings className="h-8 w-8" />
          Configuração do Portal
        </h1>
        <p className="text-muted-foreground mt-2">
          Configure as opções gerais do portal público
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Carrossel de Destaques</CardTitle>
          <CardDescription>
            Configure o tempo de transição automática do carrossel na página inicial
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="tempoTransicaoCarrossel" className="flex items-center gap-2">
                <Clock className="h-4 w-4" />
                Tempo de Transição (segundos)
              </Label>
              <Input
                id="tempoTransicaoCarrossel"
                name="tempoTransicaoCarrossel"
                type="number"
                min="1"
                max="60"
                value={formData.tempoTransicaoCarrossel}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    tempoTransicaoCarrossel: parseInt(e.target.value) || 5,
                  }))
                }
                placeholder="5"
                className="max-w-xs"
              />
              <p className="text-xs text-muted-foreground">
                Tempo em segundos para trocar automaticamente os slides do carrossel na página inicial do portal.
                Valor deve estar entre 1 e 60 segundos. Padrão: 5 segundos.
              </p>
            </div>

            <div className="flex gap-2">
              <Button type="submit" disabled={saving}>
                <Save className="h-4 w-4 mr-2" />
                {saving ? 'Salvando...' : 'Salvar Configuração'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};

export default ConfiguracaoPortal;
