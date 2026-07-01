import { useState, useCallback, useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { items, wishlist, favoriteStores } from '../api/client'
import type { ProductResponse, PagedResult, WishlistEntryResponse } from '../types'
import ItemCard from '../components/ItemCard'
import { useAuth } from '../context/AuthContext'

const DISCOUNT_TIERS = ['Grátis', '75%+', '50–75%', '25–50%', 'Até 25%']

export default function HomePage() {
  const { user } = useAuth()
  const qc = useQueryClient()
  const [searchParams] = useSearchParams()

  const initialStore = searchParams.get('store') ?? ''

  const [query, setQuery] = useState('')
  const [store, setStore] = useState(initialStore)
  const [discount, setDiscount] = useState('')
  const [page, setPage] = useState(1)
  const [applied, setApplied] = useState({ query: '', store: initialStore, discount: '' })

  useEffect(() => {
    const s = searchParams.get('store') ?? ''
    setStore(s)
    setApplied({ query: '', store: s, discount: '' })
    setPage(1)
  }, [searchParams])

  const params = {
    ...(applied.query && { query: applied.query }),
    ...(applied.store && { store: applied.store }),
    ...(applied.discount && { discount: applied.discount }),
    page,
    pageSize: 20,
  }

  const { data, isLoading } = useQuery<PagedResult<ProductResponse>>({
    queryKey: ['items', params],
    queryFn: () => items.search(params) as Promise<PagedResult<ProductResponse>>,
  })

  const { data: wishlistData } = useQuery<WishlistEntryResponse[]>({
    queryKey: ['wishlist'],
    queryFn: () => wishlist.getAll() as Promise<WishlistEntryResponse[]>,
    enabled: !!user,
  })

  const { data: favStoresData } = useQuery<string[]>({
    queryKey: ['favorite-stores'],
    queryFn: () => favoriteStores.getAll(),
    enabled: !!user,
  })

  const favStoresSet = new Set((favStoresData ?? []).map((s: string) => s.toLowerCase()))

  const addStoreMutation = useMutation({
    mutationFn: (s: string) => favoriteStores.add(s),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-stores'] }),
  })

  const removeStoreMutation = useMutation({
    mutationFn: (s: string) => favoriteStores.remove(s),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-stores'] }),
  })

  const handleToggleStore = useCallback(
    (s: string) => {
      if (favStoresSet.has(s.toLowerCase())) {
        removeStoreMutation.mutate(s)
      } else {
        addStoreMutation.mutate(s)
      }
    },
    [favStoresSet, addStoreMutation, removeStoreMutation],
  )

  const wishlistIds = new Set(wishlistData?.map((w) => w.productId) ?? [])
  const wishlistEntryMap = Object.fromEntries(wishlistData?.map((w) => [w.productId, w.id]) ?? [])

  const addMutation = useMutation({
    mutationFn: (id: string) => wishlist.add(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const removeMutation = useMutation({
    mutationFn: (entryId: string) => wishlist.remove(entryId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const handleToggleWishlist = useCallback(
    (item: ProductResponse) => {
      if (wishlistIds.has(item.id)) {
        removeMutation.mutate(wishlistEntryMap[item.id])
      } else {
        addMutation.mutate(item.id)
      }
    },
    [wishlistIds, wishlistEntryMap, addMutation, removeMutation],
  )

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    setApplied({ query, store, discount })
    setPage(1)
  }

  function handleClear() {
    setQuery('')
    setStore('')
    setDiscount('')
    setApplied({ query: '', store: '', discount: '' })
    setPage(1)
  }

  const hasFilters = applied.query || applied.store || applied.discount

  const inputCls =
    'bg-surface border border-border px-4 py-2.5 text-ink text-sm focus:outline-none focus:border-neon/60 transition-colors placeholder:text-muted'

  return (
    <main className="max-w-7xl mx-auto px-6 py-12">
      <div className="mb-10">
        <h1 className="font-display font-black text-5xl md:text-6xl tracking-widest text-ink mb-2">
          OFERTAS<span className="text-neon text-glow-neon">.</span>
        </h1>
        <p className="text-sm text-muted tracking-widest uppercase">
          Games · Melhores preços entre as lojas
        </p>
      </div>

      <form onSubmit={handleSearch} className="flex flex-wrap gap-2 mb-10">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Título ou loja..."
          className={`flex-1 min-w-48 ${inputCls}`}
        />
        <input
          value={store}
          onChange={(e) => setStore(e.target.value)}
          placeholder="Loja"
          className={`w-40 ${inputCls}`}
        />
        <select value={discount} onChange={(e) => setDiscount(e.target.value)} className={`w-40 ${inputCls}`}>
          <option value="">Desconto</option>
          {DISCOUNT_TIERS.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
        <button
          type="submit"
          className="bg-neon text-bg font-display font-bold tracking-widest px-6 py-2.5 text-sm hover:shadow-neon transition-all clip-hud"
        >
          BUSCAR
        </button>
        {hasFilters && (
          <button
            type="button"
            onClick={handleClear}
            className="border border-border text-muted font-display font-bold tracking-widest px-5 py-2.5 text-sm hover:border-neon hover:text-neon transition-colors"
          >
            LIMPAR
          </button>
        )}
      </form>

      {isLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display font-bold text-xl tracking-widest text-neon animate-pulse">
            CARREGANDO...
          </span>
        </div>
      ) : data?.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-40 text-center">
          <span className="font-display font-bold text-3xl tracking-widest text-muted mb-3">
            NENHUMA OFERTA
          </span>
          <p className="text-sm text-muted tracking-wider">Tente outros filtros de busca</p>
        </div>
      ) : (
        <>
          {data && (
            <p className="text-xs text-muted tracking-widest uppercase mb-8">
              {data.totalCount} {data.totalCount === 1 ? 'oferta encontrada' : 'ofertas encontradas'}
            </p>
          )}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
            {data?.items.map((item) => (
              <ItemCard
                key={item.id}
                item={item}
                inWishlist={wishlistIds.has(item.id)}
                onToggleWishlist={user ? handleToggleWishlist : undefined}
                isStoreFavorite={favStoresSet.has((item.attributes?.store ?? '').toLowerCase())}
                onToggleStoreFavorite={user ? handleToggleStore : undefined}
              />
            ))}
          </div>
        </>
      )}

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-3 mt-14">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="border border-border px-5 py-2 text-xs tracking-widest text-muted uppercase hover:border-neon hover:text-neon transition-colors disabled:opacity-25 disabled:cursor-not-allowed"
          >
            Anterior
          </button>
          <span className="text-xs tracking-widest text-muted uppercase px-4">
            {page} / {data.totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page === data.totalPages}
            className="border border-border px-5 py-2 text-xs tracking-widest text-muted uppercase hover:border-neon hover:text-neon transition-colors disabled:opacity-25 disabled:cursor-not-allowed"
          >
            Próxima
          </button>
        </div>
      )}
    </main>
  )
}
