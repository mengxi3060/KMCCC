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

const isElectron = () => {
  return typeof window !== 'undefined' && (window as any).electronAPI?.isElectron === true
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (token) {
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`
      checkAuth()
    } else {
      const offlinePlayer = localStorage.getItem('offlinePlayer')
      if (offlinePlayer) {
        setUser({
          id: 'offline',
          username: offlinePlayer,
          email: 'offline@local',
          role: 'User'
        })
      }
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
    const microsoftAuthUrl = `${apiClient.defaults.baseURL}/auth/microsoft`

    if (isElectron()) {
      const electronAPI = (window as any).electronAPI
      await electronAPI.openExternal(microsoftAuthUrl)
      try {
        const response = await apiClient.get('/auth/microsoft/status')
        if (response.data?.token && response.data?.user) {
          localStorage.setItem('token', response.data.token)
          apiClient.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`
          setUser(response.data.user)
        }
      } catch {
        alert('请在浏览器中完成微软账号登录后，返回启动器刷新页面')
      }
    } else {
      const width = 600
      const height = 700
      const left = window.screenX + (window.outerWidth - width) / 2
      const top = window.screenY + (window.outerHeight - height) / 2

      const authWindow = window.open(
        microsoftAuthUrl,
        'Microsoft Login',
        `width=${width},height=${height},left=${left},top=${top},toolbar=no,menubar=no,location=no,status=no`
      )

      if (!authWindow) {
        window.open(microsoftAuthUrl, '_blank')
      }

      const checkClosed = setInterval(() => {
        if (authWindow?.closed) {
          clearInterval(checkClosed)
          checkAuth()
        }
      }, 500)
    }
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
