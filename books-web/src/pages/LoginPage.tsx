import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await login(email, password)
      navigate('/')
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-6">
      <div className="w-full max-w-sm">
        <div className="mb-10">
          <h1 className="font-display text-6xl tracking-widest text-cream mb-2">ENTRAR</h1>
          <p className="text-xs text-muted tracking-widest uppercase">Acesse sua conta</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs tracking-widest text-muted uppercase mb-2">
              E-mail
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
              className="w-full bg-surface border border-border px-4 py-3 text-cream text-sm focus:outline-none focus:border-cream/50 transition-colors placeholder:text-muted"
              placeholder="seu@email.com"
            />
          </div>

          <div>
            <label className="block text-xs tracking-widest text-muted uppercase mb-2">
              Senha
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
              className="w-full bg-surface border border-border px-4 py-3 text-cream text-sm focus:outline-none focus:border-cream/50 transition-colors placeholder:text-muted"
              placeholder="••••••••"
            />
          </div>

          {error && <p className="text-xs text-red-400 tracking-wide">{error}</p>}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-cream text-bg font-display text-xl tracking-widest py-3 hover:bg-cream/90 transition-colors disabled:opacity-50 mt-2"
          >
            {loading ? 'ENTRANDO...' : 'ENTRAR'}
          </button>
        </form>

        <p className="mt-8 text-xs text-muted text-center tracking-wider">
          Não tem conta?{' '}
          <Link to="/register" className="text-cream hover:underline">
            Cadastre-se
          </Link>
        </p>
      </div>
    </div>
  )
}
