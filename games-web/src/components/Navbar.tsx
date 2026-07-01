import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import NotificationBell from './NotificationBell'

export default function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  async function handleLogout() {
    await logout()
    navigate('/')
  }

  function isActive(path: string) {
    return location.pathname === path
  }

  const linkCls = (path: string) =>
    `text-sm font-semibold tracking-wide uppercase transition-colors ${
      isActive(path) ? 'text-neon text-glow-neon' : 'text-muted hover:text-ink'
    }`

  return (
    <header className="border-b border-border sticky top-0 z-50 bg-bg/90 backdrop-blur-md">
      <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2 group">
          <span className="font-display font-black text-xl tracking-widest text-ink group-hover:text-neon transition-colors">
            ARCHIVÉ
          </span>
          <span className="font-display font-bold text-xs tracking-[0.3em] text-magenta text-glow-magenta border border-magenta/40 px-1.5 py-0.5">
            PLAY
          </span>
        </Link>

        <nav className="flex items-center gap-7">
          <Link to="/" className={linkCls('/')}>
            Ofertas
          </Link>

          {user && (
            <Link to="/wishlist" className={linkCls('/wishlist')}>
              Wishlist
            </Link>
          )}

          {user && (
            <Link to="/lojas" className={linkCls('/lojas')}>
              Lojas
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-5">
              <NotificationBell />
              <span className="text-sm text-muted hidden sm:block">{user.name}</span>
              <button
                onClick={handleLogout}
                className="text-xs font-semibold tracking-widest border border-border px-4 py-2 text-muted hover:border-neon hover:text-neon transition-colors uppercase clip-hud"
              >
                Sair
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <Link
                to="/login"
                className="text-sm font-semibold tracking-widest text-muted hover:text-ink transition-colors uppercase"
              >
                Entrar
              </Link>
              <Link
                to="/register"
                className="text-xs font-bold tracking-widest bg-neon text-bg px-4 py-2 hover:shadow-neon transition-all uppercase clip-hud"
              >
                Cadastrar
              </Link>
            </div>
          )}
        </nav>
      </div>
    </header>
  )
}
