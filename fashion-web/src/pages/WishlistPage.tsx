import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { wishlist } from '../api/client'
import type { WishlistEntryResponse } from '../types'
import { Link } from 'react-router-dom'

function fmt(price: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(price)
}

export default function WishlistPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery<WishlistEntryResponse[]>({
    queryKey: ['wishlist'],
    queryFn: () => wishlist.getAll() as Promise<WishlistEntryResponse[]>,
  })

  const removeMutation = useMutation({
    mutationFn: (entryId: string) => wishlist.remove(entryId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <div className="mb-12">
        <h1 className="font-display text-6xl md:text-7xl tracking-widest text-cream mb-2">
          WISHLIST
        </h1>
        <p className="text-xs text-muted tracking-widest uppercase">Seus itens salvos</p>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display text-2xl tracking-widest text-muted animate-pulse">
            CARREGANDO...
          </span>
        </div>
      ) : !data?.length ? (
        <div className="flex flex-col items-center justify-center py-40 text-center">
          <span className="font-display text-4xl tracking-widest text-muted mb-4">
            LISTA VAZIA
          </span>
          <p className="text-xs text-muted tracking-wider mb-10">
            Adicione itens do catálogo à sua wishlist
          </p>
          <Link
            to="/"
            className="border border-cream text-cream font-display tracking-widest px-8 py-3 text-sm hover:bg-cream hover:text-bg transition-colors"
          >
            VER CATÁLOGO
          </Link>
        </div>
      ) : (
        <>
          <p className="text-xs text-muted tracking-widest uppercase mb-6">
            {data.length} {data.length === 1 ? 'item' : 'itens'}
          </p>
          <div className="border-t border-border">
            {data.map((entry) => (
              <div
                key={entry.id}
                className="flex items-center gap-6 py-5 border-b border-border hover:bg-surface/40 transition-colors px-2 -mx-2"
              >
                <Link to={`/items/${entry.productId}`} className="shrink-0">
                  <div className="w-14 h-18 w-14 bg-s2 overflow-hidden" style={{ height: '4.5rem' }}>
                    {entry.imageUrl ? (
                      <img
                        src={entry.imageUrl}
                        alt={entry.itemName}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center">
                        <span className="font-display text-muted text-sm">A</span>
                      </div>
                    )}
                  </div>
                </Link>

                <div className="flex-1 min-w-0">
                  <p className="text-xs tracking-widest text-muted uppercase mb-1">{entry.brand}</p>
                  <Link
                    to={`/items/${entry.productId}`}
                    className="text-sm text-cream truncate block hover:text-cream/70 transition-colors"
                  >
                    {entry.itemName}
                  </Link>
                  {entry.note && (
                    <p className="text-xs text-muted mt-1 italic">"{entry.note}"</p>
                  )}
                </div>

                <div className="text-right shrink-0 hidden sm:block">
                  <p className="font-display text-xl tracking-wider text-cream">{fmt(entry.currentPrice)}</p>
                  <p className="text-xs text-muted mt-1">
                    {new Date(entry.addedAt).toLocaleDateString('pt-BR')}
                  </p>
                </div>

                <button
                  onClick={() => removeMutation.mutate(entry.id)}
                  disabled={removeMutation.isPending}
                  className="shrink-0 text-xs tracking-widest text-muted hover:text-cream uppercase border border-transparent hover:border-border px-3 py-2 transition-colors disabled:opacity-40"
                >
                  Remover
                </button>
              </div>
            ))}
          </div>
        </>
      )}
    </main>
  )
}
