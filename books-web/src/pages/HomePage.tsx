import { useState, useCallback, useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { items, wishlist, favoriteAuthors } from '../api/client'
import type { ProductResponse, PagedResult, WishlistEntryResponse } from '../types'
import ItemCard from '../components/ItemCard'
import { useAuth } from '../context/AuthContext'

export default function HomePage() {
  const { user } = useAuth()
  const qc = useQueryClient()
  const [searchParams] = useSearchParams()

  const initialAuthor = searchParams.get('author') ?? ''

  const [query, setQuery] = useState('')
  const [author, setAuthor] = useState(initialAuthor)
  const [genre, setGenre] = useState('')
  const [page, setPage] = useState(1)
  const [applied, setApplied] = useState({ query: '', author: initialAuthor, genre: '' })

  useEffect(() => {
    const a = searchParams.get('author') ?? ''
    setAuthor(a)
    setApplied({ query: '', author: a, genre: '' })
    setPage(1)
  }, [searchParams])

  const params = {
    ...(applied.query && { query: applied.query }),
    ...(applied.author && { author: applied.author }),
    ...(applied.genre && { genre: applied.genre }),
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

  const { data: favAuthorsData } = useQuery<string[]>({
    queryKey: ['favorite-authors'],
    queryFn: () => favoriteAuthors.getAll(),
    enabled: !!user,
  })

  const favAuthorsSet = new Set((favAuthorsData ?? []).map((a: string) => a.toLowerCase()))

  const addAuthorMutation = useMutation({
    mutationFn: (author: string) => favoriteAuthors.add(author),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-authors'] }),
  })

  const removeAuthorMutation = useMutation({
    mutationFn: (author: string) => favoriteAuthors.remove(author),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorite-authors'] }),
  })

  const handleToggleAuthor = useCallback(
    (author: string) => {
      if (favAuthorsSet.has(author.toLowerCase())) {
        removeAuthorMutation.mutate(author)
      } else {
        addAuthorMutation.mutate(author)
      }
    },
    [favAuthorsSet, addAuthorMutation, removeAuthorMutation],
  )

  const wishlistIds = new Set(wishlistData?.map((w) => w.productId) ?? [])
  const wishlistEntryMap = Object.fromEntries(
    wishlistData?.map((w) => [w.productId, w.id]) ?? [],
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
    setApplied({ query, author, genre })
    setPage(1)
  }

  function handleClear() {
    setQuery('')
    setAuthor('')
    setGenre('')
    setApplied({ query: '', author: '', genre: '' })
    setPage(1)
  }

  const hasFilters = applied.query || applied.author || applied.genre

  return (
    <main className="max-w-6xl mx-auto px-6 py-12">
      <div className="mb-12">
        <h1 className="font-display text-6xl md:text-7xl tracking-widest text-cream mb-2">
          CATÁLOGO
        </h1>
        <p className="text-xs text-muted tracking-widest uppercase">
          Livros · Monitoramento de preços
        </p>
      </div>

      <form onSubmit={handleSearch} className="flex flex-wrap gap-2 mb-10">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Título ou autor..."
          className="flex-1 min-w-48 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <input
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
          placeholder="Autor"
          className="w-36 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <input
          value={genre}
          onChange={(e) => setGenre(e.target.value)}
          placeholder="Gênero"
          className="w-36 bg-surface border border-border px-4 py-2.5 text-cream text-sm focus:outline-none focus:border-cream/40 transition-colors placeholder:text-muted"
        />
        <button
          type="submit"
          className="bg-cream text-bg font-display tracking-widest px-6 py-2.5 text-sm hover:bg-cream/90 transition-colors"
        >
          BUSCAR
        </button>
        {hasFilters && (
          <button
            type="button"
            onClick={handleClear}
            className="border border-border text-muted font-display tracking-widest px-5 py-2.5 text-sm hover:border-cream hover:text-cream transition-colors"
          >
            LIMPAR
          </button>
        )}
      </form>

      {isLoading ? (
        <div className="flex items-center justify-center py-40">
          <span className="font-display text-2xl tracking-widest text-muted animate-pulse">
            CARREGANDO...
          </span>
        </div>
      ) : data?.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-40 text-center">
          <span className="font-display text-4xl tracking-widest text-muted mb-3">
            NENHUM LIVRO
          </span>
          <p className="text-xs text-muted tracking-wider">Tente outros filtros de busca</p>
        </div>
      ) : (
        <>
          {data && (
            <p className="text-xs text-muted tracking-widest uppercase mb-8">
              {data.totalCount} {data.totalCount === 1 ? 'livro encontrado' : 'livros encontrados'}
            </p>
          )}
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {data?.items.map((item) => (
              <ItemCard
                key={item.id}
                item={item}
                inWishlist={wishlistIds.has(item.id)}
                onToggleWishlist={user ? handleToggleWishlist : undefined}
                isAuthorFavorite={favAuthorsSet.has((item.attributes?.author ?? '').toLowerCase())}
                onToggleAuthorFavorite={user ? handleToggleAuthor : undefined}
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
