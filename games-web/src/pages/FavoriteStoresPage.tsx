import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { favoriteStores } from '../api/client'
import { Link } from 'react-router-dom'

export default function FavoriteStoresPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery<string[]>({
    queryKey: ['favorite-stores'],
    queryFn: () => favoriteStores.getAll(),
  })

  const removeMutation = useMutation({
    mutationFn: (store: string) => favoriteStores.remove(store),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-stores'] }),
  })

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <div className="mb-10">
        <h1 className="font-display font-black text-5xl md:text-6xl tracking-widest text-ink mb-2">
          LOJAS<span className="text-neon text-glow-neon">.</span>
        </h1>
        <p className="text-xs text-muted tracking-widest uppercase">Suas lojas favoritas</p>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display font-bold text-xl tracking-widest text-neon animate-pulse">
            CARREGANDO...
          </span>
        </div>
      ) : !data?.length ? (
        <div className="flex flex-col items-center justify-center py-40 text-center">
          <span className="font-display font-bold text-3xl tracking-widest text-muted mb-4">
            NENHUMA LOJA
          </span>
          <p className="text-xs text-muted tracking-wider mb-10">
            Favorite lojas clicando na estrela (☆) nos cards do catálogo
          </p>
          <Link
            to="/"
            className="border border-neon text-neon font-display font-bold tracking-widest px-8 py-3 text-sm hover:shadow-neon transition-all clip-hud"
          >
            VER OFERTAS
          </Link>
        </div>
      ) : (
        <>
          <p className="text-xs text-muted tracking-widest uppercase mb-6">
            {data.length} {data.length === 1 ? 'loja' : 'lojas'}
          </p>
          <div className="border-t border-border">
            {data.map((store) => (
              <div
                key={store}
                className="flex items-center justify-between py-5 border-b border-border hover:bg-surface/40 transition-colors px-2 -mx-2"
              >
                <div className="flex items-center gap-4">
                  <span className="text-magenta text-xs tracking-widest text-glow-magenta">★</span>
                  <Link
                    to={`/?store=${encodeURIComponent(store)}`}
                    className="font-display font-bold text-lg tracking-widest text-ink hover:text-neon transition-colors uppercase"
                  >
                    {store}
                  </Link>
                </div>
                <div className="flex items-center gap-4">
                  <Link
                    to={`/?store=${encodeURIComponent(store)}`}
                    className="text-xs tracking-widest text-muted uppercase border border-transparent hover:border-border hover:text-neon px-3 py-2 transition-colors"
                  >
                    Ver ofertas
                  </Link>
                  <button
                    onClick={() => removeMutation.mutate(store)}
                    disabled={removeMutation.isPending}
                    className="text-xs tracking-widest text-muted uppercase border border-transparent hover:border-border hover:text-magenta px-3 py-2 transition-colors disabled:opacity-40"
                  >
                    Remover
                  </button>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </main>
  )
}
