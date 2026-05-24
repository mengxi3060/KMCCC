import { useState, useEffect } from 'react'
import { apiClient, GameVersion } from '../services/api'
import { Gamepad2, Download, FolderOpen, CheckCircle, AlertCircle, Plus } from 'lucide-react'

export default function Versions() {
  const [versions, setVersions] = useState<GameVersion[]>([])
  const [loading, setLoading] = useState(true)
  const [showInstallModal, setShowInstallModal] = useState(false)

  useEffect(() => {
    loadVersions()
  }, [])

  const loadVersions = async () => {
    try {
      const response = await apiClient.get('/launcher/versions')
      setVersions(response.data)
    } catch (err) {
      console.error('加载版本失败:', err)
    } finally {
      setLoading(false)
    }
  }

  const formatSize = (bytes: number) => {
    if (bytes === 0) return '0 B'
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">版本管理</h1>
          <p className="text-gray-500">管理你的 Minecraft 游戏版本</p>
        </div>
        <button
          onClick={() => setShowInstallModal(true)}
          className="btn-primary flex items-center gap-2"
        >
          <Plus className="w-5 h-5" />
          安装新版本
        </button>
      </div>

      <div className="card overflow-hidden">
        <div className="divide-y divide-gray-100">
          {loading ? (
            <div className="p-8 text-center">
              <div className="w-8 h-8 border-3 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
              <p className="text-gray-500">加载中...</p>
            </div>
          ) : versions.length === 0 ? (
            <div className="p-8 text-center">
              <Gamepad2 className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <p className="text-gray-500 mb-4">还没有安装任何版本</p>
              <button
                onClick={() => setShowInstallModal(true)}
                className="btn-primary"
              >
                安装第一个版本
              </button>
            </div>
          ) : (
            versions.map((version) => (
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

      {showInstallModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="card p-6 w-full max-w-lg">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-gray-800">安装新版本</h2>
              <button
                onClick={() => setShowInstallModal(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                ✕
              </button>
            </div>
            <p className="text-gray-500 mb-6">版本安装功能即将推出，敬请期待！</p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowInstallModal(false)}
                className="btn-secondary"
              >
                关闭
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
