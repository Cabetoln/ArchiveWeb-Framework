import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { items, wishlist, priceAlerts } from '../api/client'
import type {
  ProductResponse,
  PriceHistoryResponse,
  WishlistEntryResponse,
  PriceAlertResponse,
  SeasonalInsightResponse,
} from '../types'
import SeasonalInsightCard from '../components/SeasonalInsightCard'
import { useAuth } from '../context/AuthContext'
import { fmt } from '../lib/format'

export default function ItemDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const qc = useQueryClient()

  const { data: item, isLoading } = useQuery<ProductResponse>({
    queryKey: ['item', id],
    queryFn: () => items.getById(id!) as Promise<ProductResponse>,
    enabled: !!id,
  })

  const { data: history } = useQuery<PriceHistoryResponse[]>({
    queryKey: ['price-history', id],
    queryFn: () => items.getPriceHistory(id!) as Promise<PriceHistoryResponse[]>,
    enabled: !!id,
  })

  const { data: seasonalInsight } = useQuery<SeasonalInsightResponse>({
    queryKey: ['seasonal-insight', id],
    queryFn: () => items.getSeasonalInsight(id!) as Promise<SeasonalInsightResponse>,
    enabled: !!id,
  })

  const { data: wishlistData } = useQuery<WishlistEntryResponse[]>({
    queryKey: ['wishlist'],
    queryFn: () => wishlist.getAll() as Promise<WishlistEntryResponse[]>,
    enabled: !!user,
  })

  const wishlistEntry = wishlistData?.find((w) => w.productId === id)

  const addMutation = useMutation({
    mutationFn: () => wishlist.add(id!),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const removeMutation = useMutation({
    mutationFn: () => wishlist.remove(wishlistEntry!.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const [targetInput, setTargetInput] = useState('')

  const { data: alertsData } = useQuery<PriceAlertResponse[]>({
    queryKey: ['price-alerts'],
    queryFn: () => priceAlerts.getAll(),
    enabled: !!user,
  })

  const currentAlert = alertsData?.find((a) => a.productId === id)

  const setAlertMutation = useMutation({
    mutationFn: (targetPrice: number) => priceAlerts.set(id!, targetPrice),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['price-alerts'] })
      setTargetInput('')
    },
  })

  const removeAlertMutation = useMutation({
    mutationFn: () => priceAlerts.remove(currentAlert!.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['price-alerts'] }),
  })

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[calc(100vh-4rem)]">
        <span className="font-display font-bold text-xl tracking-widest text-neon animate-pulse">
          CARREGANDO...
        </span>
      </div>
    )
  }

  if (!item) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[calc(100vh-4rem)]">
        <span className="font-display font-bold text-3xl tracking-widest text-muted mb-6">
          ITEM NÃO ENCONTRADO
        </span>
        <button
          onClick={() => navigate(-1)}
          className="text-xs text-muted hover:text-neon tracking-widest uppercase transition-colors"
        >
          ← Voltar
        </button>
      </div>
    )
  }

  const currency = item.currency || 'USD'
  const prices = history?.map((h) => h.price) ?? []
  const minPrice = prices.length ? Math.min(...prices) : item.currentPrice
  const maxPrice = prices.length ? Math.max(...prices) : item.currentPrice
  const priceChange = history && history.length >= 2 ? item.currentPrice - history[0].price : null

  const store = item.attributes?.store
  const rating = item.attributes?.rating
  const discount = item.attributes?.discount
  const savings = Number(item.attributes?.savings ?? 0)
  const normalPrice = Number(item.attributes?.normalPrice ?? 0)
  const hasDiscount = savings >= 1 && normalPrice > item.currentPrice

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <button
        onClick={() => navigate(-1)}
        className="text-xs tracking-widest text-muted uppercase hover:text-neon transition-colors mb-10 flex items-center gap-2"
      >
        ← Voltar
      </button>

      <div className="grid md:grid-cols-2 gap-12 lg:gap-16">
        {/* Imagem */}
        <div>
          <div className="relative aspect-[16/9] bg-surface border border-border overflow-hidden clip-hud">
            {item.imageUrl ? (
              <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                <span className="font-display font-black text-5xl tracking-widest text-neon/10">
                  PLAY
                </span>
              </div>
            )}
            {hasDiscount && (
              <span className="absolute top-3 left-3 bg-lime text-bg font-display font-bold text-sm tracking-wider px-3 py-1 leading-none">
                -{Math.round(savings)}%
              </span>
            )}
          </div>

          {/* chips de atributos */}
          <div className="flex flex-wrap gap-2 mt-4">
            {store && (
              <span className="text-xs tracking-widest uppercase border border-neon/40 text-neon px-3 py-1.5">
                {store}
              </span>
            )}
            {discount && (
              <span className="text-xs tracking-widest uppercase border border-lime/40 text-lime px-3 py-1.5">
                {discount}
              </span>
            )}
            {rating && rating !== 'Sem avaliação' && (
              <span className="text-xs tracking-widest uppercase border border-border text-muted px-3 py-1.5">
                {rating}
              </span>
            )}
          </div>
        </div>

        {/* Info */}
        <div className="flex flex-col">
          <p className="text-xs tracking-widest text-neon uppercase mb-3">{store}</p>
          <h1 className="font-display font-bold text-3xl lg:text-4xl tracking-wide text-ink leading-tight mb-6">
            {item.name}
          </h1>

          <div className="mb-8">
            <div className="flex items-baseline gap-3">
              <p className="font-display font-black text-5xl tracking-wide text-lime">
                {fmt(item.currentPrice, currency)}
              </p>
              {hasDiscount && (
                <p className="text-xl text-muted line-through">{fmt(normalPrice, currency)}</p>
              )}
            </div>
            <p className="text-xs text-muted mt-2 tracking-wider">
              Atualizado em{' '}
              {new Date(item.updatedAt).toLocaleDateString('pt-BR', {
                day: '2-digit',
                month: 'long',
                year: 'numeric',
              })}
            </p>
            {priceChange !== null && (
              <p
                className={`text-xs mt-1 tracking-wider font-medium ${
                  priceChange < 0 ? 'text-lime' : priceChange > 0 ? 'text-magenta' : 'text-muted'
                }`}
              >
                {priceChange < 0 ? '↓' : priceChange > 0 ? '↑' : '='}{' '}
                {fmt(Math.abs(priceChange), currency)} em relação ao primeiro registro
              </p>
            )}
          </div>

          {prices.length > 1 && (
            <div className="flex gap-10 pb-8 mb-8 border-b border-border">
              <div>
                <p className="text-xs text-muted tracking-widest uppercase mb-1">Mínimo</p>
                <p className="font-display font-bold text-lg tracking-wide text-ink">
                  {fmt(minPrice, currency)}
                </p>
              </div>
              <div>
                <p className="text-xs text-muted tracking-widest uppercase mb-1">Máximo</p>
                <p className="font-display font-bold text-lg tracking-wide text-ink">
                  {fmt(maxPrice, currency)}
                </p>
              </div>
              <div>
                <p className="text-xs text-muted tracking-widest uppercase mb-1">Registros</p>
                <p className="font-display font-bold text-lg tracking-wide text-ink">{prices.length}</p>
              </div>
            </div>
          )}

          {/* Ações */}
          <div className="flex gap-3">
            {item.productUrl && (
              <a
                href={item.productUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="flex-1 bg-neon text-bg font-display font-bold tracking-widest text-center py-3 text-sm hover:shadow-neon transition-all clip-hud"
              >
                PEGAR OFERTA
              </a>
            )}
            {user && (
              <button
                onClick={() => (wishlistEntry ? removeMutation.mutate() : addMutation.mutate())}
                disabled={addMutation.isPending || removeMutation.isPending}
                className={`px-6 py-3 font-display font-bold tracking-widest text-sm transition-all border disabled:opacity-40 ${
                  wishlistEntry
                    ? 'bg-magenta text-bg border-magenta hover:shadow-magenta'
                    : 'border-border text-muted hover:border-magenta hover:text-magenta'
                }`}
              >
                {wishlistEntry ? '♥ SALVO' : '♡ SALVAR'}
              </button>
            )}
          </div>

          {/* Alerta de preço */}
          {user && (
            <div className="mt-8 pt-8 border-t border-border">
              <p className="text-xs tracking-widest text-muted uppercase mb-4">Alerta de Preço</p>

              {currentAlert ? (
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-xs text-muted tracking-wider mb-1">Avise-me quando chegar em</p>
                    <p className="font-display font-bold text-xl tracking-wide text-lime">
                      {fmt(currentAlert.targetPrice, currency)}
                    </p>
                  </div>
                  <button
                    onClick={() => removeAlertMutation.mutate()}
                    disabled={removeAlertMutation.isPending}
                    className="text-xs tracking-widest text-muted uppercase border border-border px-4 py-2 hover:border-neon hover:text-neon transition-colors disabled:opacity-40"
                  >
                    Remover
                  </button>
                </div>
              ) : (
                <form
                  onSubmit={(e) => {
                    e.preventDefault()
                    const val = parseFloat(targetInput.replace(',', '.'))
                    if (!isNaN(val) && val > 0) setAlertMutation.mutate(val)
                  }}
                  className="flex gap-2"
                >
                  <input
                    value={targetInput}
                    onChange={(e) => setTargetInput(e.target.value)}
                    placeholder="Preço alvo (ex: 19.90)"
                    className="flex-1 bg-surface border border-border px-4 py-2.5 text-ink text-sm focus:outline-none focus:border-neon/60 transition-colors placeholder:text-muted"
                  />
                  <button
                    type="submit"
                    disabled={setAlertMutation.isPending || !targetInput}
                    className="bg-neon text-bg font-display font-bold tracking-widest px-5 py-2.5 text-sm hover:shadow-neon transition-all disabled:opacity-40 clip-hud"
                  >
                    DEFINIR
                  </button>
                </form>
              )}
            </div>
          )}

          {/* Histórico de preços */}
          {history && history.length > 0 && (
            <div className="mt-12">
              <h2 className="font-display font-bold text-xl tracking-widest text-ink mb-5">
                HISTÓRICO DE PREÇOS
              </h2>
              <div className="border-t border-border">
                {[...history]
                  .sort((a, b) => new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime())
                  .slice(0, 12)
                  .map((entry) => (
                    <div
                      key={entry.id}
                      className="flex items-center justify-between py-3 border-b border-border"
                    >
                      <span className="text-xs text-muted tracking-wider">
                        {new Date(entry.recordedAt).toLocaleDateString('pt-BR', {
                          day: '2-digit',
                          month: 'short',
                          year: 'numeric',
                        })}
                      </span>
                      <span className="text-xs text-muted uppercase tracking-widest">
                        {entry.source ?? '—'}
                      </span>
                      <span className="font-display font-bold text-base tracking-wide text-ink">
                        {fmt(entry.price, entry.currency)}
                      </span>
                    </div>
                  ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {seasonalInsight && <SeasonalInsightCard insight={seasonalInsight} />}
    </main>
  )
}
