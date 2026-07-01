export interface UserResponse {
  id: string
  name: string
  email: string
  createdAt: string
}

export interface ProductResponse {
  id: string
  name: string
  imageUrl: string | null
  productUrl: string | null
  currentPrice: number
  currency: string
  updatedAt: string
  attributes: Record<string, string | null>
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
  productId: string
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
  productId: string
  itemName: string
  brand: string
  targetPrice: number
  currentPrice: number
  currency: string
  createdAt: string
}

export interface BestDiscountMonthResponse {
  month: number
  monthName: string
  season: string
  discountPercentage: number
  isDiscountPeriod: boolean
  insight: string
}

export interface SeasonalPatternResponse {
  month: number
  monthName: string
  season: string
  averagePrice: number | null
  discountPercentage: number | null
  isDiscountPeriod: boolean
  hasData: boolean
}

export interface SeasonalInsightResponse {
  itemId: string
  hasEnoughHistory: boolean
  hasSimulatedHistory: boolean
  currentPrice: number
  currentMonthAverage: number
  differencePercentage: number
  status: string
  recommendation: string
  bestDiscountMonth: BestDiscountMonthResponse | null
  monthlyPatterns: SeasonalPatternResponse[]
}
