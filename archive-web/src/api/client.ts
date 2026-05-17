async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    ...init,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? `Erro ${res.status}`)
  }

  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const auth = {
  register: (name: string, email: string, password: string) =>
    request('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ name, email, password }),
    }),

  login: (email: string, password: string) =>
    request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  logout: () => request('/api/auth/logout', { method: 'POST' }),

  me: () => request('/api/auth/me'),
}

export const items = {
  search: (params: Record<string, unknown> = {}) => {
    const qs = new URLSearchParams()
    for (const [k, v] of Object.entries(params)) {
      if (v != null && v !== '') qs.set(k, String(v))
    }
    return request(`/api/items?${qs}`)
  },

  getById: (id: string) => request(`/api/items/${id}`),

  getPriceHistory: (id: string, from?: string, to?: string) => {
    const qs = new URLSearchParams()
    if (from) qs.set('from', from)
    if (to) qs.set('to', to)
    return request(`/api/items/${id}/price-history?${qs}`)
  },
}

export const favoriteBrands = {
  getAll: () => request<string[]>('/api/favorite-brands'),

  add: (brand: string) =>
    request(`/api/favorite-brands/${encodeURIComponent(brand)}`, { method: 'POST' }),

  remove: (brand: string) =>
    request(`/api/favorite-brands/${encodeURIComponent(brand)}`, { method: 'DELETE' }),
}

export const priceAlerts = {
  getAll: () => request<import('../types').PriceAlertResponse[]>('/api/price-alerts'),

  set: (fashionItemId: string, targetPrice: number) =>
    request('/api/price-alerts', {
      method: 'POST',
      body: JSON.stringify({ fashionItemId, targetPrice }),
    }),

  remove: (id: string) =>
    request(`/api/price-alerts/${id}`, { method: 'DELETE' }),
}

export const wishlist = {
  getAll: () => request('/api/wishlist'),

  add: (fashionItemId: string, note?: string) =>
    request('/api/wishlist', {
      method: 'POST',
      body: JSON.stringify({ fashionItemId, note }),
    }),

  remove: (entryId: string) =>
    request(`/api/wishlist/${entryId}`, { method: 'DELETE' }),
}
