import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { apiClient } from '../services/api'

interface User {
  id: string
  username: string
  email: string
  role: 'User' | 'Moderator' | 'Admin'
}

interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  loginMicrosoft: () => Promise<void>
  loginOffline: (playerName: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (token) {
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`
      checkAuth()
    } else {
      setIsLoading(false)
    }
  }, [])

  const checkAuth = async () => {
    try {
      const response = await apiClient.get('/auth/me')
      setUser(response.data)
    } catch {
      localStorage.removeItem('token')
      delete apiClient.defaults.headers.common['Authorization']
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (email: string, password: string) => {
    const response = await apiClient.post('/auth/login', { email, password })
    const { token, user } = response.data
    localStorage.setItem('token', token)
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`
    setUser(user)
  }

  const loginMicrosoft = async () => {
    window.location.href = '/api/auth/microsoft'
  }

  const loginOffline = async (playerName: string) => {
    localStorage.setItem('offlinePlayer', playerName)
    setUser({
      id: 'offline',
      username: playerName,
      email: 'offline@local',
      role: 'User'
    })
  }

  const register = async (username: string, email: string, password: string) => {
    await apiClient.post('/auth/register', { username, email, password })
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('offlinePlayer')
    delete apiClient.defaults.headers.common['Authorization']
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        loginMicrosoft,
        loginOffline,
        register,
        logout
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
