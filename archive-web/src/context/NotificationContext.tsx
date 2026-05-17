import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react'
import { useQuery } from '@tanstack/react-query'
import { wishlist, priceAlerts } from '../api/client'
import type { WishlistEntryResponse, PriceAlertResponse } from '../types'
import { useAuth } from './AuthContext'

export interface PriceNotification {
  id: string
  fashionItemId: string
  itemName: string
  brand: string
  oldPrice: number
  newPrice: number
  currency: string
  changedAt: string
  read: boolean
  type: 'change' | 'alert'
}

interface NotificationContextValue {
  notifications: PriceNotification[]
  unreadCount: number
  markAllRead: () => void
  clearAll: () => void
}

const STORAGE_PRICES = 'archive_last_prices'
const STORAGE_NOTIFS = 'archive_notifications'
const STORAGE_TRIGGERED_ALERTS = 'archive_triggered_alerts'

const NotificationContext = createContext<NotificationContextValue | null>(null)

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth()

  const [notifications, setNotifications] = useState<PriceNotification[]>(() => {
    try {
      return JSON.parse(localStorage.getItem(STORAGE_NOTIFS) ?? '[]')
    } catch {
      return []
    }
  })

  // Poll wishlist every 30s
  const { data: wishlistData } = useQuery<WishlistEntryResponse[]>({
    queryKey: ['wishlist'],
    queryFn: () => wishlist.getAll() as Promise<WishlistEntryResponse[]>,
    enabled: !!user,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })

  // Poll price alerts every 30s
  const { data: alertsData } = useQuery<PriceAlertResponse[]>({
    queryKey: ['price-alerts'],
    queryFn: () => priceAlerts.getAll(),
    enabled: !!user,
    refetchInterval: 30_000,
    refetchIntervalInBackground: false,
  })

  // Detecta mudanças de preço na wishlist
  useEffect(() => {
    if (!wishlistData || wishlistData.length === 0) return

    const storedRaw = localStorage.getItem(STORAGE_PRICES)
    const lastPrices: Record<string, number> = storedRaw ? JSON.parse(storedRaw) : {}

    const newNotifs: PriceNotification[] = []
    const updatedPrices = { ...lastPrices }

    for (const entry of wishlistData) {
      const last = lastPrices[entry.fashionItemId]

      if (last === undefined) {
        updatedPrices[entry.fashionItemId] = entry.currentPrice
        continue
      }

      if (last !== entry.currentPrice) {
        newNotifs.push({
          id: `change-${entry.fashionItemId}-${Date.now()}`,
          fashionItemId: entry.fashionItemId,
          itemName: entry.itemName,
          brand: entry.brand,
          oldPrice: last,
          newPrice: entry.currentPrice,
          currency: 'BRL',
          changedAt: new Date().toISOString(),
          read: false,
          type: 'change',
        })
        updatedPrices[entry.fashionItemId] = entry.currentPrice
      }
    }

    localStorage.setItem(STORAGE_PRICES, JSON.stringify(updatedPrices))

    if (newNotifs.length > 0) {
      setNotifications((prev) => {
        const merged = [...newNotifs, ...prev].slice(0, 50)
        localStorage.setItem(STORAGE_NOTIFS, JSON.stringify(merged))
        return merged
      })
    }
  }, [wishlistData])

  // Verifica alertas de preço atingidos
  useEffect(() => {
    if (!alertsData || alertsData.length === 0) return

    const triggeredRaw = localStorage.getItem(STORAGE_TRIGGERED_ALERTS)
    const alreadyTriggered: string[] = triggeredRaw ? JSON.parse(triggeredRaw) : []

    const newNotifs: PriceNotification[] = []
    const newTriggered = [...alreadyTriggered]

    for (const alert of alertsData) {
      if (alert.currentPrice <= alert.targetPrice && !alreadyTriggered.includes(alert.id)) {
        newNotifs.push({
          id: `alert-${alert.id}`,
          fashionItemId: alert.fashionItemId,
          itemName: alert.itemName,
          brand: alert.brand,
          oldPrice: alert.targetPrice,
          newPrice: alert.currentPrice,
          currency: alert.currency,
          changedAt: new Date().toISOString(),
          read: false,
          type: 'alert',
        })
        newTriggered.push(alert.id)
      }
    }

    localStorage.setItem(STORAGE_TRIGGERED_ALERTS, JSON.stringify(newTriggered))

    if (newNotifs.length > 0) {
      setNotifications((prev) => {
        const merged = [...newNotifs, ...prev].slice(0, 50)
        localStorage.setItem(STORAGE_NOTIFS, JSON.stringify(merged))
        return merged
      })
    }
  }, [alertsData])

  // Limpa dados locais ao fazer logout
  useEffect(() => {
    if (!user) {
      localStorage.removeItem(STORAGE_PRICES)
      localStorage.removeItem(STORAGE_TRIGGERED_ALERTS)
    }
  }, [user])

  const markAllRead = useCallback(() => {
    setNotifications((prev) => {
      const updated = prev.map((n) => ({ ...n, read: true }))
      localStorage.setItem(STORAGE_NOTIFS, JSON.stringify(updated))
      return updated
    })
  }, [])

  const clearAll = useCallback(() => {
    setNotifications([])
    localStorage.removeItem(STORAGE_NOTIFS)
  }, [])

  const unreadCount = notifications.filter((n) => !n.read).length

  return (
    <NotificationContext.Provider value={{ notifications, unreadCount, markAllRead, clearAll }}>
      {children}
    </NotificationContext.Provider>
  )
}

export function useNotifications() {
  const ctx = useContext(NotificationContext)
  if (!ctx) throw new Error('useNotifications must be inside NotificationProvider')
  return ctx
}
