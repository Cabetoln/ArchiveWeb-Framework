import type { SeasonalInsightResponse } from '../types'

interface SeasonalInsightCardProps {
  insight: SeasonalInsightResponse
}

function fmt(price: number, currency = 'BRL') {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency }).format(price)
}

function getStatusStyles(status: string) {
  return status === 'Em baixa'
    ? 'text-green-400 border-green-400 bg-green-400/5'
    : status === 'Em alta'
    ? 'text-red-400 border-red-400 bg-red-400/5'
    : 'text-muted border-border bg-surface'
}

export default function SeasonalInsightCard({ insight }: SeasonalInsightCardProps) {
  return (
    <section className="mt-10 rounded-xl border border-border bg-surface p-6">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
        <div>
          <p className="text-xs tracking-widest text-muted uppercase mb-2">Análise sazonal</p>
          <h2 className="font-display text-2xl tracking-wider text-cream">Janela provável de desconto</h2>
        </div>
        <span className={`rounded-full border px-3 py-1 text-xs tracking-widest uppercase ${getStatusStyles(insight.status)}`}>
          {insight.status}
        </span>
      </div>

      {insight.hasEnoughHistory ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="rounded-xl border border-border bg-bg/50 p-4">
            <p className="text-xs tracking-widest text-muted uppercase mb-2">Preço atual</p>
            <p className="font-display text-3xl tracking-wider text-cream">{fmt(insight.currentPrice)}</p>
            <p className="text-xs tracking-widest text-muted mt-2">
              Média do mês: {fmt(insight.currentMonthAverage)}
            </p>
            <p
              className={`text-xs tracking-widest mt-1 ${
                insight.differencePercentage < 0
                  ? 'text-green-400'
                  : insight.differencePercentage > 0
                  ? 'text-red-400'
                  : 'text-muted'
              }`}
            >
              {insight.differencePercentage < 0 ? '• ' : insight.differencePercentage > 0 ? '• ' : '• '}
              {insight.differencePercentage.toFixed(2)}% em relação à média
            </p>
          </div>

          <div className="rounded-xl border border-border bg-bg/50 p-4">
            {insight.bestDiscountMonth ? (
              <>
                <p className="text-xs tracking-widest text-muted uppercase mb-2">Melhor janela</p>
                <p className="font-display text-3xl tracking-wider text-cream">
                  {insight.bestDiscountMonth.monthName}
                </p>
                <p className="text-xs tracking-widest text-muted mt-2">
                  {insight.bestDiscountMonth.season}
                </p>
                <p className="text-xs tracking-widest mt-3 text-cream/80">
                  {insight.bestDiscountMonth.insight}
                </p>
                <p className="text-xs tracking-widest mt-2 text-muted">
                  Desconto típico: {insight.bestDiscountMonth.discountPercentage.toFixed(2)}%
                </p>
              </>
            ) : (
              <>
                <p className="text-xs tracking-widest text-muted uppercase mb-2">Melhor janela</p>
                <p className="font-display text-2xl tracking-wider text-cream">Sem padrão definido</p>
                <p className="text-xs tracking-widest mt-2 text-muted">
                  Não há um mês claro com desconto evidente.
                </p>
              </>
            )}
          </div>

          {insight.hasSimulatedHistory && (
            <div className="sm:col-span-2 rounded-xl border border-border bg-bg/50 p-4">
              <p className="text-xs tracking-widest text-muted uppercase mb-2">Dados simulados</p>
              <p className="text-sm text-cream">
                Esta análise foi gerada com base em histórico simulado para demonstrar o recurso.
              </p>
            </div>
          )}
        </div>
      ) : (
        <div className="rounded-xl border border-border bg-bg/50 p-4">
          <p className="text-xs tracking-widest text-muted uppercase mb-2">Dados insuficientes</p>
          <p className="font-display text-2xl tracking-wider text-cream">Ainda não há histórico suficiente.</p>
          <p className="text-xs tracking-widest mt-2 text-muted">
            Aguarde novos registros de preço para que a análise seja liberada.
          </p>
        </div>
      )}
    </section>
  )
}
