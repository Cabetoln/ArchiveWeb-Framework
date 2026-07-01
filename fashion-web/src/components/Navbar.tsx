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

  return (
    <header className="border-b border-border sticky top-0 z-50 bg-bg/95 backdrop-blur-sm">
      <div className="max-w-6xl mx-auto px-6 h-16 flex items-center justify-between">
        <Link
          to="/"
          className="font-display text-2xl tracking-widest text-cream hover:opacity-70 transition-opacity"
        >
          ARCHIVÉ
        </Link>

        <nav className="flex items-center gap-8">
          <Link
            to="/"
            className={`text-xs tracking-widest uppercase transition-colors ${
              isActive('/') ? 'text-cream' : 'text-muted hover:text-cream'
            }`}
          >
            Catálogo
          </Link>

          {user && (
            <Link
              to="/wishlist"
              className={`text-xs tracking-widest uppercase transition-colors ${
                isActive('/wishlist') ? 'text-cream' : 'text-muted hover:text-cream'
              }`}
            >
              Wishlist
            </Link>
          )}

          {user && (
            <Link
              to="/marcas"
              className={`text-xs tracking-widest uppercase transition-colors ${
                isActive('/marcas') ? 'text-cream' : 'text-muted hover:text-cream'
              }`}
            >
              Marcas
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-5">
              <NotificationBell />
              <span className="text-xs text-muted hidden sm:block">{user.name}</span>
              <button
                onClick={handleLogout}
                className="text-xs tracking-widest border border-border px-4 py-2 text-muted hover:border-cream hover:text-cream transition-colors uppercase"
              >
                Sair
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <Link
                to="/login"
                className="text-xs tracking-widest text-muted hover:text-cream transition-colors uppercase"
              >
                Entrar
              </Link>
              <Link
                to="/register"
                className="text-xs tracking-widest border border-cream text-cream px-4 py-2 hover:bg-cream hover:text-bg transition-colors uppercase"
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
