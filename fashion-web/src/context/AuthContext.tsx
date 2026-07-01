import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { auth } from '../api/client'
import type { UserResponse } from '../types'

interface AuthContextValue {
  user: UserResponse | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (name: string, email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserResponse | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    ;(auth.me() as Promise<UserResponse>)
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setLoading(false))
  }, [])

  async function login(email: string, password: string) {
    const u = await (auth.login(email, password) as Promise<UserResponse>)
    setUser(u)
  }

  async function register(name: string, email: string, password: string) {
    const u = await (auth.register(name, email, password) as Promise<UserResponse>)
    setUser(u)
  }

  async function logout() {
    await auth.logout()
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be inside AuthProvider')
  return ctx
}
