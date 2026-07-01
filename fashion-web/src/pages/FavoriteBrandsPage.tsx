import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { favoriteBrands } from '../api/client'
import { Link } from 'react-router-dom'

export default function FavoriteBrandsPage() {
  const qc = useQueryClient()

  const { data, isLoading } = useQuery<string[]>({
    queryKey: ['favorite-brands'],
    queryFn: () => favoriteBrands.getAll(),
  })

  const removeMutation = useMutation({
    mutationFn: (brand: string) => favoriteBrands.remove(brand),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-brands'] }),
  })

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <div className="mb-12">
        <h1 className="font-display text-6xl md:text-7xl tracking-widest text-cream mb-2">
          MARCAS
        </h1>
        <p className="text-xs text-muted tracking-widest uppercase">Suas marcas favoritas</p>
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
            NENHUMA MARCA
          </span>
          <p className="text-xs text-muted tracking-wider mb-10">
            Favorite marcas clicando na estrela (☆) nos cards do catálogo
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
            {data.length} {data.length === 1 ? 'marca' : 'marcas'}
          </p>
          <div className="border-t border-border">
            {data.map((brand) => (
              <div
                key={brand}
                className="flex items-center justify-between py-5 border-b border-border hover:bg-surface/40 transition-colors px-2 -mx-2"
              >
                <div className="flex items-center gap-4">
                  <span className="text-cream text-xs tracking-widest">★</span>
                  <Link
                    to={`/?brand=${encodeURIComponent(brand)}`}
                    className="font-display text-xl tracking-widest text-cream hover:opacity-70 transition-opacity uppercase"
                  >
                    {brand}
                  </Link>
                </div>
                <div className="flex items-center gap-4">
                  <Link
                    to={`/?brand=${encodeURIComponent(brand)}`}
                    className="text-xs tracking-widest text-muted uppercase border border-transparent hover:border-border hover:text-cream px-3 py-2 transition-colors"
                  >
                    Ver itens
                  </Link>
                  <button
                    onClick={() => removeMutation.mutate(brand)}
                    disabled={removeMutation.isPending}
                    className="text-xs tracking-widest text-muted uppercase border border-transparent hover:border-border hover:text-cream px-3 py-2 transition-colors disabled:opacity-40"
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
