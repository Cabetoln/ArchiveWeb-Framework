export interface UserResponse {
  id: string
  name: string
  email: string
  createdAt: string
}

export interface FashionItemResponse {
  id: string
  name: string
  brand: string
  category: string | null
  imageUrl: string | null
  productUrl: string | null
  currentPrice: number
  currency: string
  updatedAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface WishlistEntryResponse {
  id: string
  fashionItemId: string
  itemName: string
  brand: string
  currentPrice: number
  imageUrl: string | null
  addedAt: string
  note: string | null
}

export interface PriceHistoryResponse {
  id: string
  price: number
  currency: string
  recordedAt: string
  source: string | null
}

export interface PriceAlertResponse {
  id: string
  fashionItemId: string
  itemName: string
  brand: string
  targetPrice: number
  currentPrice: number
  currency: string
  createdAt: string
}
