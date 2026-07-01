import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await register(name, email, password)
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
          <h1 className="font-display text-6xl tracking-widest text-cream mb-2">CADASTRO</h1>
          <p className="text-xs text-muted tracking-widest uppercase">Crie sua conta Archivé</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs tracking-widest text-muted uppercase mb-2">Nome</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoComplete="name"
              className="w-full bg-surface border border-border px-4 py-3 text-cream text-sm focus:outline-none focus:border-cream/50 transition-colors placeholder:text-muted"
              placeholder="Seu nome"
            />
          </div>

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
            <label className="block text-xs tracking-widest text-muted uppercase mb-2">Senha</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={6}
              autoComplete="new-password"
              className="w-full bg-surface border border-border px-4 py-3 text-cream text-sm focus:outline-none focus:border-cream/50 transition-colors placeholder:text-muted"
              placeholder="Mínimo 6 caracteres"
            />
          </div>

          {error && <p className="text-xs text-red-400 tracking-wide">{error}</p>}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-cream text-bg font-display text-xl tracking-widest py-3 hover:bg-cream/90 transition-colors disabled:opacity-50 mt-2"
          >
            {loading ? 'CRIANDO...' : 'CRIAR CONTA'}
          </button>
        </form>

        <p className="mt-8 text-xs text-muted text-center tracking-wider">
          Já tem conta?{' '}
          <Link to="/login" className="text-cream hover:underline">
            Entrar
          </Link>
        </p>
      </div>
    </div>
  )
}
