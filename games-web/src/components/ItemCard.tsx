import { Link } from 'react-router-dom'
import type { ProductResponse } from '../types'
import { useAuth } from '../context/AuthContext'
import { fmt } from '../lib/format'

interface Props {
  item: ProductResponse
  inWishlist?: boolean
  onToggleWishlist?: (item: ProductResponse) => void
  isStoreFavorite?: boolean
  onToggleStoreFavorite?: (store: string) => void
}

export default function ItemCard({
  item,
  inWishlist,
  onToggleWishlist,
  isStoreFavorite,
  onToggleStoreFavorite,
}: Props) {
  const { user } = useAuth()

  const store = item.attributes?.store ?? ''
  const rating = item.attributes?.rating
  const savings = Number(item.attributes?.savings ?? 0)
  const normalPrice = Number(item.attributes?.normalPrice ?? 0)
  const currency = item.currency || 'USD'
  const hasDiscount = savings >= 1 && normalPrice > item.currentPrice

  return (
    <div className="group relative bg-surface border border-border hover:border-neon/50 hover:shadow-neon transition-all duration-300 clip-hud">
      <Link to={`/items/${item.id}`} className="block">
        <div className="relative aspect-[16/9] overflow-hidden bg-s2">
          {item.imageUrl ? (
            <img
              src={item.imageUrl}
              alt={item.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <span className="font-display font-black text-4xl tracking-widest text-neon/10">
                PLAY
              </span>
            </div>
          )}

          {/* Selo de desconto */}
          {hasDiscount && (
            <span className="absolute top-2 left-2 bg-lime text-bg font-display font-bold text-xs tracking-wider px-2 py-1 leading-none">
              -{Math.round(savings)}%
            </span>
          )}
        </div>

        <div className="p-4">
          {/* Loja + favoritar */}
          <div className="flex items-center gap-1.5 mb-1.5">
            <span className="text-xs font-semibold tracking-widest text-neon uppercase truncate">
              {store}
            </span>
            {user && onToggleStoreFavorite && store && (
              <button
                onClick={(e) => {
                  e.preventDefault()
                  onToggleStoreFavorite(store)
                }}
                className={`text-xs leading-none transition-all ${
                  isStoreFavorite
                    ? 'text-magenta opacity-100 text-glow-magenta'
                    : 'text-muted opacity-0 group-hover:opacity-100 hover:text-magenta'
                }`}
                title={isStoreFavorite ? 'Remover loja dos favoritos' : 'Favoritar loja'}
              >
                {isStoreFavorite ? '★' : '☆'}
              </button>
            )}
          </div>

          <p className="text-base font-semibold text-ink leading-snug mb-3 line-clamp-2 min-h-[2.75rem]">
            {item.name}
          </p>

          {/* Preço */}
          <div className="flex items-baseline gap-2">
            <span className="font-display font-bold text-lg tracking-wide text-lime">
              {fmt(item.currentPrice, currency)}
            </span>
            {hasDiscount && (
              <span className="text-sm text-muted line-through">
                {fmt(normalPrice, currency)}
              </span>
            )}
          </div>

          {rating && rating !== 'Sem avaliação' && (
            <p className="text-xs text-muted mt-1.5 uppercase tracking-wider">▸ {rating}</p>
          )}
        </div>
      </Link>

      {user && onToggleWishlist && (
        <button
          onClick={(e) => {
            e.preventDefault()
            onToggleWishlist(item)
          }}
          className={`absolute top-2 right-2 w-8 h-8 flex items-center justify-center border text-sm transition-all duration-200 ${
            inWishlist
              ? 'border-magenta bg-magenta text-bg'
              : 'border-border bg-bg/70 text-muted hover:border-magenta hover:text-magenta opacity-0 group-hover:opacity-100'
          }`}
          title={inWishlist ? 'Remover da wishlist' : 'Adicionar à wishlist'}
        >
          {inWishlist ? '♥' : '♡'}
        </button>
      )}
    </div>
  )
}
