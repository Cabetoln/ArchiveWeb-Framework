import type { SeasonalInsightResponse } from '../types'
import { fmt } from '../lib/format'

interface SeasonalInsightCardProps {
  insight: SeasonalInsightResponse
}

function getStatusStyles(status: string) {
  return status === 'Em baixa'
    ? 'text-lime border-lime bg-lime/5'
    : status === 'Em alta'
    ? 'text-magenta border-magenta bg-magenta/5'
    : 'text-muted border-border bg-surface'
}

export default function SeasonalInsightCard({ insight }: SeasonalInsightCardProps) {
  return (
    <section className="mt-10 border border-border bg-surface p-6 clip-hud">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
        <div>
          <p className="text-xs tracking-widest text-neon uppercase mb-2">Análise sazonal</p>
          <h2 className="font-display font-bold text-xl tracking-wide text-ink">
            Janela provável de desconto
          </h2>
        </div>
        <span
          className={`border px-3 py-1 text-xs tracking-widest uppercase clip-hud ${getStatusStyles(
            insight.status,
          )}`}
        >
          {insight.status}
        </span>
      </div>

      {insight.hasEnoughHistory ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="border border-border bg-bg/50 p-4">
            <p className="text-xs tracking-widest text-muted uppercase mb-2">Preço atual</p>
            <p className="font-display font-bold text-2xl tracking-wide text-ink">
              {fmt(insight.currentPrice)}
            </p>
            <p className="text-xs tracking-widest text-muted mt-2">
              Média do mês: {fmt(insight.currentMonthAverage)}
            </p>
            <p
              className={`text-xs tracking-widest mt-1 ${
                insight.differencePercentage < 0
                  ? 'text-lime'
                  : insight.differencePercentage > 0
                  ? 'text-magenta'
                  : 'text-muted'
              }`}
            >
              {insight.differencePercentage < 0 ? '↓ ' : insight.differencePercentage > 0 ? '↑ +' : '• '}
              {insight.differencePercentage.toFixed(2)}% em relação à média
            </p>
          </div>

          <div className="border border-border bg-bg/50 p-4">
            {insight.bestDiscountMonth ? (
              <>
                <p className="text-xs tracking-widest text-muted uppercase mb-2">Melhor janela</p>
                <p className="font-display font-bold text-2xl tracking-wide text-ink">
                  {insight.bestDiscountMonth.monthName}
                </p>
                <p className="text-xs tracking-widest text-muted mt-2">
                  {insight.bestDiscountMonth.season}
                </p>
                <p className="text-xs tracking-widest mt-3 text-ink/80">
                  {insight.bestDiscountMonth.insight}
                </p>
                <p className="text-xs tracking-widest mt-2 text-muted">
                  Desconto típico: {insight.bestDiscountMonth.discountPercentage.toFixed(2)}%
                </p>
              </>
            ) : (
              <>
                <p className="text-xs tracking-widest text-muted uppercase mb-2">Melhor janela</p>
                <p className="font-display font-bold text-xl tracking-wide text-ink">
                  Sem padrão definido
                </p>
                <p className="text-xs tracking-widest mt-2 text-muted">
                  Não há um mês claro com desconto evidente.
                </p>
              </>
            )}
          </div>

          {insight.hasSimulatedHistory && (
            <div className="sm:col-span-2 border border-border bg-bg/50 p-4">
              <p className="text-xs tracking-widest text-muted uppercase mb-2">Dados simulados</p>
              <p className="text-sm text-ink">
                Esta análise foi gerada com base em histórico simulado para demonstrar o recurso.
              </p>
            </div>
          )}
        </div>
      ) : (
        <div className="border border-border bg-bg/50 p-4">
          <p className="text-xs tracking-widest text-muted uppercase mb-2">Dados insuficientes</p>
          <p className="font-display font-bold text-xl tracking-wide text-ink">
            Ainda não há histórico suficiente.
          </p>
          <p className="text-xs tracking-widest mt-2 text-muted">
            Aguarde novos registros de preço para que a análise seja liberada.
          </p>
        </div>
      )}
    </section>
  )
}
