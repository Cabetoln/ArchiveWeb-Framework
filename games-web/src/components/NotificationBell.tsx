import { useState, useRef, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useNotifications, type PriceNotification } from '../context/NotificationContext'
import { fmt } from '../lib/format'

function NotifItem({ n }: { n: PriceNotification }) {
  const down = n.newPrice < n.oldPrice
  const diff = Math.abs(n.newPrice - n.oldPrice)

  return (
    <Link
      to={`/items/${n.productId}`}
      className="block px-4 py-3 border-b border-border hover:bg-s2 transition-colors"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-xs text-neon uppercase tracking-widest mb-0.5">{n.store}</p>
          <p className="text-sm text-ink truncate leading-snug">{n.itemName}</p>
          <p className="text-xs text-muted mt-1">
            {fmt(n.oldPrice, n.currency)} →{' '}
            <span className={down ? 'text-lime' : 'text-magenta'}>{fmt(n.newPrice, n.currency)}</span>
          </p>
        </div>
        <div className="shrink-0 text-right">
          <span
            className={`font-display font-bold text-sm tracking-wider ${
              down ? 'text-lime' : 'text-magenta'
            }`}
          >
            {down ? '↓' : '↑'} {fmt(diff, n.currency)}
          </span>
          <p className="text-xs text-muted mt-1">
            {new Date(n.changedAt).toLocaleTimeString('pt-BR', {
              hour: '2-digit',
              minute: '2-digit',
            })}
          </p>
        </div>
      </div>
    </Link>
  )
}

export default function NotificationBell() {
  const { notifications, unreadCount, markAllRead, clearAll } = useNotifications()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  function handleOpen() {
    setOpen((v) => !v)
    if (!open && unreadCount > 0) markAllRead()
  }

  return (
    <div ref={ref} className="relative">
      <button
        onClick={handleOpen}
        className="relative text-muted hover:text-neon transition-colors p-1"
        title="Notificações"
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>

        {unreadCount > 0 && (
          <span className="absolute -top-0.5 -right-0.5 w-4 h-4 bg-magenta text-bg text-[9px] font-bold flex items-center justify-center rounded-full leading-none">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-8 w-80 bg-surface border border-border shadow-xl z-50">
          <div className="flex items-center justify-between px-4 py-3 border-b border-border">
            <span className="font-display font-bold text-xs tracking-widest text-ink">
              NOTIFICAÇÕES
            </span>
            {notifications.length > 0 && (
              <button
                onClick={clearAll}
                className="text-xs text-muted hover:text-neon transition-colors tracking-wider uppercase"
              >
                Limpar
              </button>
            )}
          </div>

          <div className="max-h-80 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="px-4 py-8 text-center">
                <p className="text-xs text-muted tracking-widest uppercase">Sem notificações</p>
                <p className="text-xs text-muted mt-1 opacity-60">
                  Avisamos quando um preço mudar
                </p>
              </div>
            ) : (
              notifications.map((n) => <NotifItem key={n.id} n={n} />)
            )}
          </div>
        </div>
      )}
    </div>
  )
}
