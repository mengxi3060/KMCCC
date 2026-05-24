import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      delete apiClient.defaults.headers.common['Authorization']
    }
    return Promise.reject(error)
  }
)

export interface GameVersion {
  id: string
  name: string
  gameRootPath: string
  installDate: string
  size: number
  isValid: boolean
}

export interface JavaInfo {
  path: string
  version: string
}

export interface LaunchOptions {
  versionId: string
  maxMemory: number
  minMemory?: number
  javaPath?: string
  server?: {
    address: string
    port: number
  }
  windowSize?: {
    width: number
    height: number
    fullScreen: boolean
  }
}

export interface LaunchResult {
  success: boolean
  errorMessage?: string
  handle?: {
    processId: number
    isRunning: boolean
  }
}

export interface Resource {
  id: string
  name: string
  type: 'Mod' | 'Modpack' | 'Shader' | 'TexturePack'
  description: string
  authorId: string
  tags: string[]
  screenshots: string[]
  downloadCount: number
  likeCount: number
  status: 'Pending' | 'Approved' | 'Rejected' | 'Frozen'
  createdAt: string
  compatibilities: {
    gameVersion: string
    loaderType: 'None' | 'Forge' | 'Fabric' | 'Quilt'
  }[]
}
