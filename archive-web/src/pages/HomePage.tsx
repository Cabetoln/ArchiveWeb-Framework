import { useState, useCallback, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { items, wishlist } from '../api/client'
import type { FashionItemResponse, PagedResult, WishlistEntryResponse } from '../types'
import ItemCard from '../components/ItemCard'
import { useAuth } from '../context/AuthContext'

export default function HomePage() {
  const { user } = useAuth()
  const qc = useQueryClient()

  const [query, setQuery] = useState('')
  const [brand, setBrand] = useState('')
  const [category, setCategory] = useState('')
  const [page, setPage] = useState(1)
  const [applied, setApplied] = useState({ query: '', brand: '', category: '' })

  const [imageResults, setImageResults] = useState<FashionItemResponse[] | null>(null)
  const [imageLoading, setImageLoading] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const params = {
    ...(applied.query && { query: applied.query }),
    ...(applied.brand && { brand: applied.brand }),
    ...(applied.category && { category: applied.category }),
    page,
    pageSize: 20,
  }

  const { data, isLoading } = useQuery<PagedResult<FashionItemResponse>>({
    queryKey: ['items', params],
    queryFn: () => items.search(params) as Promise<PagedResult<FashionItemResponse>>,
  })

  const { data: wishlistData } = useQuery<WishlistEntryResponse[]>({
    queryKey: ['wishlist'],
    queryFn: () => wishlist.getAll() as Promise<WishlistEntryResponse[]>,
    enabled: !!user,
  })

  const wishlistIds = new Set(wishlistData?.map((w) => w.fashionItemId) ?? [])
  const wishlistEntryMap = Object.fromEntries(
    wishlistData?.map((w) => [w.fashionItemId, w.id]) ?? [],
  )

  const addMutation = useMutation({
    mutationFn: (id: string) => wishlist.add(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const removeMutation = useMutation({
    mutationFn: (entryId: string) => wishlist.remove(entryId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wishlist'] }),
  })

  const handleToggleWishlist = useCallback(
    (item: FashionItemResponse) => {
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
    setApplied({ query, brand, category })
    setPage(1)
    setImageResults(null)
  }

  function handleClear() {
    setQuery('')
    setBrand('')
    setCategory('')
    setApplied({ query: '', brand: '', category: '' })
    setPage(1)
    setImageResults(null)
  }

  async function handleImageSearch(e: React.ChangeEvent<HTMLInputElement>) {
  const file = e.target.files?.[0]
  if (!file) return

  setImageLoading(true)
  setImageResults(null)

  const form = new FormData()
  form.append('image', file)

  try {
    const res = await fetch('http://localhost:8001/search', { method: 'POST', body: form })
    const data = await res.json()
    const normalized = data.map((item: any) => ({
      ...item,
      id: item.Id,
      name: item.Name,
      brand: item.Brand,
      category: item.Category,
      imageUrl: item.ImageUrl,
      productUrl: item.ProductUrl,
      currentPrice: item.CurrentPrice,
      currency: item.Currency,
    }))
    setImageResults(normalized)
  } finally {
    setImageLoading(false)
    e.target.value = ''
  }
}

  const hasFilters = applied.query || applied.brand || applied.category

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <div className="mb-12">
        <h1 className="font-display text-6xl md:text-7xl tracking-widest text-cream mb-2">
          CATÁLOGO
        </h1>
        <p className="text-xs text-muted tracking-widest uppercase">
          Itens de moda · Monitoramento de preços
        </p>
      </div>

      <form onSubmit={handleSearch} className="flex flex-wrap gap-2 mb-10">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Nome ou marca..."
          className="flex-1 min-w-48 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <input
          value={brand}
          onChange={(e) => setBrand(e.target.value)}
          placeholder="Marca"
          className="w-36 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <input
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          placeholder="Categoria"
          className="w-36 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <button
          type="submit"
          className="bg-cream text-bg font-display tracking-widest px-6 py-2.5 text-sm hover:bg-cream/90 transition-colors"
        >
          BUSCAR
        </button>
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleImageSearch}
        />
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="border border-border text-muted font-display tracking-widest px-5 py-2.5 text-sm hover:border-cream hover:text-cream transition-colors"
          title="Buscar por imagem"
        >
          IMAGEM
        </button>
        {(hasFilters || imageResults) && (
          <button
            type="button"
            onClick={handleClear}
            className="border border-border text-muted font-display tracking-widest px-5 py-2.5 text-sm hover:border-cream hover:text-cream transition-colors"
          >
            LIMPAR
          </button>
        )}
      </form>

      {imageLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display text-2xl tracking-widest text-muted animate-pulse">
            ANALISANDO...
          </span>
        </div>
      ) : imageResults ? (
        <>
          <p className="text-xs text-muted tracking-widest uppercase mb-8">
            {imageResults.length} {imageResults.length === 1 ? 'item similar' : 'itens similares'}
          </p>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {imageResults.map((item) => (
              <ItemCard
                key={item.id ?? item.id}
                item={item}
                inWishlist={wishlistIds.has(item.id)}
                onToggleWishlist={user ? handleToggleWishlist : undefined}
              />
            ))}
          </div>
        </>
      ) : isLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display text-2xl tracking-widest text-muted animate-pulse">
            CARREGANDO...
          </span>
        </div>
      ) : data?.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-40 text-center">
          <span className="font-display text-4xl tracking-widest text-muted mb-3">
            NENHUM ITEM
          </span>
          <p className="text-xs text-muted tracking-wider">Tente outros filtros de busca</p>
        </div>
      ) : (
        <>
          {data && (
            <p className="text-xs text-muted tracking-widest uppercase mb-8">
              {data.totalCount} {data.totalCount === 1 ? 'item encontrado' : 'itens encontrados'}
            </p>
          )}
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {data?.items.map((item) => (
              <ItemCard
                key={item.id}
                item={item}
                inWishlist={wishlistIds.has(item.id)}
                onToggleWishlist={user ? handleToggleWishlist : undefined}
              />
            ))}
          </div>
        </>
      )}

      {!imageResults && data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-3 mt-14">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="border border-border px-5 py-2 text-xs tracking-widest text-muted uppercase hover:border-cream hover:text-cream transition-colors disabled:opacity-25 disabled:cursor-not-allowed"
          >
            Anterior
          </button>
          <span className="text-xs tracking-widest text-muted uppercase px-4">
            {page} / {data.totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
            disabled={page === data.totalPages}
            className="border border-border px-5 py-2 text-xs tracking-widest text-muted uppercase hover:border-cream hover:text-cream transition-colors disabled:opacity-25 disabled:cursor-not-allowed"
          >
            Próxima
          </button>
        </div>
      )}
    </main>
  )
}