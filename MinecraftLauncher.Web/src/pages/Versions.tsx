import { useState, useEffect } from 'react'
import { apiClient, GameVersion } from '../services/api'
import axios from 'axios'
import { Gamepad2, Download, FolderOpen, CheckCircle, AlertCircle, Plus, Search, Filter, Loader2, Box, Wrench, Layers } from 'lucide-react'

interface MojangVersion {
  id: string
  type: string
  url: string
  time: string
  releaseTime: string
}

interface ForgeVersion {
  version: string
  mcversion: string
  downloadUrl: string
}

interface FabricVersion {
  version: string
  stable: boolean
}

type LoaderFilter = 'vanilla' | 'forge' | 'fabric'

export default function Versions() {
  const [installedVersions, setInstalledVersions] = useState<GameVersion[]>([])
  const [loading, setLoading] = useState(true)
  const [showInstallModal, setShowInstallModal] = useState(false)
  const [activeTab, setActiveTab] = useState<'installed' | 'download'>('installed')

  const [mojangVersions, setMojangVersions] = useState<MojangVersion[]>([])
  const [forgeVersions, setForgeVersions] = useState<ForgeVersion[]>([])
  const [fabricVersions, setFabricVersions] = useState<FabricVersion[]>([])
  const [loaderFilter, setLoaderFilter] = useState<LoaderFilter>('vanilla')
  const [versionSearch, setVersionSearch] = useState('')
  const [releaseFilter, setReleaseFilter] = useState<'all' | 'release' | 'snapshot'>('release')
  const [loadingRemote, setLoadingRemote] = useState(false)
  const [downloading, setDownloading] = useState<string | null>(null)
  const [downloadProgress, setDownloadProgress] = useState(0)

  useEffect(() => {
    loadInstalledVersions()
  }, [])

  const loadInstalledVersions = async () => {
    try {
      const response = await apiClient.get('/launcher/versions')
      setInstalledVersions(response.data)
    } catch {
      setInstalledVersions([])
    } finally {
      setLoading(false)
    }
  }

  const loadRemoteVersions = async () => {
    setLoadingRemote(true)
    try {
      if (loaderFilter === 'vanilla') {
        const res = await axios.get('https://piston-meta.mojang.com/mc/game/version_manifest_v2.json')
        setMojangVersions(res.data.versions || [])
      } else if (loaderFilter === 'forge') {
        const res = await axios.get('https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json')
        const promos = res.data.promos || {}
        const versions: ForgeVersion[] = Object.entries(promos).map(([key, val]: [string, any]) => ({
          version: key,
          mcversion: key.split('-')[0],
          downloadUrl: `https://files.minecraftforge.net/net/minecraftforge/forge/${key}/forge-${key}-installer.jar`
        }))
        setForgeVersions(versions)
      } else if (loaderFilter === 'fabric') {
        const res = await axios.get('https://meta.fabricmc.net/v2/versions/loader')
        setFabricVersions(res.data || [])
      }
    } catch (err) {
      console.error('加载远程版本失败:', err)
    } finally {
      setLoadingRemote(false)
    }
  }

  useEffect(() => {
    if (showInstallModal) {
      loadRemoteVersions()
    }
  }, [showInstallModal, loaderFilter])

  const handleInstall = async (versionId: string, loader: LoaderFilter) => {
    setDownloading(versionId)
    setDownloadProgress(0)
    try {
      const interval = setInterval(() => {
        setDownloadProgress(prev => {
          if (prev >= 90) {
            clearInterval(interval)
            return 90
          }
          return prev + Math.random() * 15
        })
      }, 500)

      await apiClient.post('/launcher/versions/install', {
        versionId,
        loaderType: loader === 'vanilla' ? 'None' : loader === 'forge' ? 'Forge' : 'Fabric'
      })

      clearInterval(interval)
      setDownloadProgress(100)
      setTimeout(() => {
        loadInstalledVersions()
        setDownloading(null)
        setDownloadProgress(0)
      }, 1000)
    } catch (err: any) {
      alert('安装失败: ' + (err.response?.data?.message || err.message))
      setDownloading(null)
      setDownloadProgress(0)
    }
  }

  const formatSize = (bytes: number) => {
    if (bytes === 0) return '0 B'
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  const filteredMojangVersions = mojangVersions.filter(v => {
    const matchesSearch = v.id.toLowerCase().includes(versionSearch.toLowerCase())
    const matchesType = releaseFilter === 'all' || v.type === releaseFilter
    return matchesSearch && matchesType
  })

  const filteredForgeVersions = forgeVersions.filter(v =>
    v.mcversion.toLowerCase().includes(versionSearch.toLowerCase())
  )

  const filteredFabricVersions = fabricVersions.filter(v =>
    v.version.toLowerCase().includes(versionSearch.toLowerCase())
  )

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">版本管理</h1>
          <p className="text-gray-500">管理你的 Minecraft 游戏版本</p>
        </div>
      </div>

      <div className="flex gap-2 border-b border-gray-200">
        <button
          onClick={() => setActiveTab('installed')}
          className={`px-6 py-3 font-medium border-b-2 transition-colors ${
            activeTab === 'installed'
              ? 'text-primary-600 border-primary-600'
              : 'text-gray-500 border-transparent hover:text-gray-700'
          }`}
        >
          <span className="flex items-center gap-2">
            <Gamepad2 className="w-4 h-4" />
            已安装
            {installedVersions.length > 0 && (
              <span className="px-2 py-0.5 bg-primary-100 text-primary-600 rounded-full text-xs">
                {installedVersions.length}
              </span>
            )}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('download')}
          className={`px-6 py-3 font-medium border-b-2 transition-colors ${
            activeTab === 'download'
              ? 'text-primary-600 border-primary-600'
              : 'text-gray-500 border-transparent hover:text-gray-700'
          }`}
        >
          <span className="flex items-center gap-2">
            <Download className="w-4 h-4" />
            下载版本
          </span>
        </button>
      </div>

      {activeTab === 'installed' && (
        <div className="card overflow-hidden">
          <div className="divide-y divide-gray-100">
            {loading ? (
              <div className="p-8 text-center">
                <Loader2 className="w-8 h-8 text-primary-500 animate-spin mx-auto mb-4" />
                <p className="text-gray-500">加载中...</p>
              </div>
            ) : installedVersions.length === 0 ? (
              <div className="p-8 text-center">
                <Gamepad2 className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 mb-4">还没有安装任何版本</p>
                <button
                  onClick={() => setActiveTab('download')}
                  className="btn-primary flex items-center gap-2 mx-auto"
                >
                  <Download className="w-5 h-5" />
                  下载第一个版本
                </button>
              </div>
            ) : (
              installedVersions.map((version) => (
                <div key={version.id} className="p-6 flex items-center justify-between hover:bg-gray-50 transition-colors">
                  <div className="flex items-center gap-4">
                    <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${
                      version.isValid
                        ? 'bg-accent-100 text-accent-600'
                        : 'bg-red-100 text-red-600'
                    }`}>
                      {version.isValid ? (
                        <CheckCircle className="w-6 h-6" />
                      ) : (
                        <AlertCircle className="w-6 h-6" />
                      )}
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-800">{version.name}</h3>
                      <div className="flex items-center gap-4 text-sm text-gray-500">
                        <span>{version.id}</span>
                        <span>•</span>
                        <span>{formatSize(version.size)}</span>
                        <span>•</span>
                        <span>{new Date(version.installDate).toLocaleDateString()}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <button className="btn-secondary flex items-center gap-2">
                      <FolderOpen className="w-4 h-4" />
                      打开文件夹
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}

      {activeTab === 'download' && (
        <div className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex gap-2">
              {([
                { key: 'vanilla', label: '原版', icon: Box, color: 'primary' },
                { key: 'forge', label: 'Forge', icon: Wrench, color: 'orange' },
                { key: 'fabric', label: 'Fabric', icon: Layers, color: 'purple' }
              ] as const).map(({ key, label, icon: Icon, color }) => (
                <button
                  key={key}
                  onClick={() => { setLoaderFilter(key); setVersionSearch(''); }}
                  className={`flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    loaderFilter === key
                      ? `bg-${color}-600 text-white shadow-sm`
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  <Icon className="w-4 h-4" />
                  {label}
                </button>
              ))}
            </div>
            <div className="flex-1 relative">
              <Search className="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="搜索版本号..."
                value={versionSearch}
                onChange={(e) => setVersionSearch(e.target.value)}
                className="input-field pl-12"
              />
            </div>
            {loaderFilter === 'vanilla' && (
              <select
                value={releaseFilter}
                onChange={(e) => setReleaseFilter(e.target.value as any)}
                className="input-field w-auto"
              >
                <option value="release">正式版</option>
                <option value="snapshot">快照</option>
                <option value="all">全部</option>
              </select>
            )}
          </div>

          <div className="card overflow-hidden">
            {loadingRemote ? (
              <div className="p-8 text-center">
                <Loader2 className="w-8 h-8 text-primary-500 animate-spin mx-auto mb-4" />
                <p className="text-gray-500">正在获取版本列表...</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-100 max-h-[500px] overflow-y-auto">
                {loaderFilter === 'vanilla' && filteredMojangVersions.length === 0 && (
                  <div className="p-8 text-center text-gray-500">没有找到匹配的版本</div>
                )}
                {loaderFilter === 'vanilla' && filteredMojangVersions.slice(0, 50).map((v) => {
                  const isInstalled = installedVersions.some(iv => iv.id === v.id)
                  const isDownloading = downloading === v.id
                  return (
                    <div key={v.id} className="p-4 flex items-center justify-between hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-3">
                        <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${
                          v.type === 'release' ? 'bg-primary-100 text-primary-600' : 'bg-yellow-100 text-yellow-600'
                        }`}>
                          <Box className="w-5 h-5" />
                        </div>
                        <div>
                          <p className="font-medium text-gray-800">{v.id}</p>
                          <p className="text-xs text-gray-500">
                            {v.type === 'release' ? '正式版' : '快照'} • {new Date(v.releaseTime).toLocaleDateString()}
                          </p>
                        </div>
                      </div>
                      {isInstalled ? (
                        <span className="flex items-center gap-1 text-accent-600 text-sm font-medium">
                          <CheckCircle className="w-4 h-4" />
                          已安装
                        </span>
                      ) : isDownloading ? (
                        <div className="flex items-center gap-3 min-w-[150px]">
                          <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                            <div
                              className="h-full bg-primary-500 rounded-full transition-all duration-300"
                              style={{ width: `${downloadProgress}%` }}
                            />
                          </div>
                          <span className="text-xs text-gray-500">{Math.round(downloadProgress)}%</span>
                        </div>
                      ) : (
                        <button
                          onClick={() => handleInstall(v.id, 'vanilla')}
                          className="btn-primary text-sm py-2 px-4 flex items-center gap-1"
                        >
                          <Download className="w-4 h-4" />
                          安装
                        </button>
                      )}
                    </div>
                  )
                })}

                {loaderFilter === 'forge' && filteredForgeVersions.map((v, i) => {
                  const isDownloading = downloading === v.version
                  return (
                    <div key={i} className="p-4 flex items-center justify-between hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl flex items-center justify-center bg-orange-100 text-orange-600">
                          <Wrench className="w-5 h-5" />
                        </div>
                        <div>
                          <p className="font-medium text-gray-800">Forge {v.version}</p>
                          <p className="text-xs text-gray-500">Minecraft {v.mcversion}</p>
                        </div>
                      </div>
                      {isDownloading ? (
                        <div className="flex items-center gap-3 min-w-[150px]">
                          <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                            <div
                              className="h-full bg-orange-500 rounded-full transition-all duration-300"
                              style={{ width: `${downloadProgress}%` }}
                            />
                          </div>
                          <span className="text-xs text-gray-500">{Math.round(downloadProgress)}%</span>
                        </div>
                      ) : (
                        <button
                          onClick={() => handleInstall(v.version, 'forge')}
                          className="btn-primary text-sm py-2 px-4 flex items-center gap-1"
                        >
                          <Download className="w-4 h-4" />
                          安装
                        </button>
                      )}
                    </div>
                  )
                })}

                {loaderFilter === 'fabric' && filteredFabricVersions.map((v, i) => {
                  const isDownloading = downloading === v.version
                  return (
                    <div key={i} className="p-4 flex items-center justify-between hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl flex items-center justify-center bg-purple-100 text-purple-600">
                          <Layers className="w-5 h-5" />
                        </div>
                        <div>
                          <p className="font-medium text-gray-800">Fabric Loader {v.version}</p>
                          <p className="text-xs text-gray-500">{v.stable ? '稳定版' : '测试版'}</p>
                        </div>
                      </div>
                      {isDownloading ? (
                        <div className="flex items-center gap-3 min-w-[150px]">
                          <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                            <div
                              className="h-full bg-purple-500 rounded-full transition-all duration-300"
                              style={{ width: `${downloadProgress}%` }}
                            />
                          </div>
                          <span className="text-xs text-gray-500">{Math.round(downloadProgress)}%</span>
                        </div>
                      ) : (
                        <button
                          onClick={() => handleInstall(v.version, 'fabric')}
                          className="btn-primary text-sm py-2 px-4 flex items-center gap-1"
                        >
                          <Download className="w-4 h-4" />
                          安装
                        </button>
                      )}
                    </div>
                  )
                })}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
