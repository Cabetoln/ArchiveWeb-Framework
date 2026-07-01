import { Link } from 'react-router-dom'
import type { ProductResponse } from '../types'
import { useAuth } from '../context/AuthContext'

interface Props {
  item: ProductResponse
  inWishlist?: boolean
  onToggleWishlist?: (item: ProductResponse) => void
  isAuthorFavorite?: boolean
  onToggleAuthorFavorite?: (author: string) => void
}

export default function ItemCard({ item, inWishlist, onToggleWishlist, isAuthorFavorite, onToggleAuthorFavorite }: Props) {
  const { user } = useAuth()

  const author = item.attributes?.author ?? ''
  const genre = item.attributes?.genre

  const price = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: item.currency || 'BRL',
  }).format(item.currentPrice)

  return (
    <div className="group relative bg-surface border border-border hover:border-cream/20 transition-all duration-300">
      <Link to={`/items/${item.id}`} className="block">
        <div className="aspect-[3/4] overflow-hidden bg-s2">
          {item.imageUrl ? (
            <img
              src={item.imageUrl}
              alt={item.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <span className="font-display text-5xl tracking-widest text-cream/10">A</span>
            </div>
          )}
        </div>

        <div className="p-4">
          <div className="flex items-center gap-1 mb-1">
            <p className="text-xs tracking-widest text-muted uppercase">{author}</p>
            {user && onToggleAuthorFavorite && author && (
              <button
                onClick={(e) => {
                  e.preventDefault()
                  onToggleAuthorFavorite(author)
                }}
                className={`text-xs leading-none transition-all ${
                  isAuthorFavorite
                    ? 'text-yellow-400 opacity-100'
                    : 'text-muted opacity-0 group-hover:opacity-100 hover:text-cream'
                }`}
                title={isAuthorFavorite ? 'Remover autor dos favoritos' : 'Favoritar autor'}
              >
                {isAuthorFavorite ? '★' : '☆'}
              </button>
            )}
          </div>
          <p className="text-sm text-cream leading-snug mb-3 line-clamp-2">{item.name}</p>
          <p className="font-display text-lg tracking-wider text-cream">{price}</p>
          {genre && (
            <p className="text-xs text-muted mt-1 uppercase tracking-wider">{genre}</p>
          )}
        </div>
      </Link>

      {user && onToggleWishlist && (
        <button
          onClick={(e) => {
            e.preventDefault()
            onToggleWishlist(item)
          }}
          className={`absolute top-3 right-3 w-8 h-8 flex items-center justify-center border text-sm transition-all duration-200 ${
            inWishlist
              ? 'border-cream bg-cream text-bg'
              : 'border-border bg-surface/80 text-muted hover:border-cream hover:text-cream opacity-0 group-hover:opacity-100'
          }`}
          title={inWishlist ? 'Remover da wishlist' : 'Adicionar à wishlist'}
        >
          {inWishlist ? '♥' : '♡'}
        </button>
      )}
    </div>
  )
}
