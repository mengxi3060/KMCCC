import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { apiClient, GameVersion, JavaInfo, LaunchOptions } from '../services/api'
import { Play, Gamepad2, Cpu, HardDrive, Server, Monitor } from 'lucide-react'

export default function Home() {
  const { user } = useAuth()
  const [versions, setVersions] = useState<GameVersion[]>([])
  const [javaVersions, setJavaVersions] = useState<JavaInfo[]>([])
  const [selectedVersion, setSelectedVersion] = useState<string>('')
  const [javaPath, setJavaPath] = useState<string>('')
  const [maxMemory, setMaxMemory] = useState<number>(2048)
  const [loading, setLoading] = useState(false)
  const [launching, setLaunching] = useState(false)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    try {
      const [versionsRes, javaRes] = await Promise.all([
        apiClient.get('/launcher/versions'),
        apiClient.get('/launcher/java')
      ])
      setVersions(versionsRes.data)
      setJavaVersions(javaRes.data)
      if (versionsRes.data.length > 0) {
        setSelectedVersion(versionsRes.data[0].id)
      }
      if (javaRes.data.length > 0) {
        setJavaPath(javaRes.data[0].path)
      }
    } catch (err) {
      console.error('加载数据失败:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleLaunch = async () => {
    if (!selectedVersion) return
    setLaunching(true)
    try {
      const options: LaunchOptions = {
        versionId: selectedVersion,
        maxMemory,
        javaPath: javaPath || undefined
      }
      await apiClient.post('/launcher/launch', options)
    } catch (err: any) {
      alert('启动失败: ' + (err.response?.data?.message || err.message))
    } finally {
      setLaunching(false)
    }
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">欢迎回来，{user?.username}！</h1>
          <p className="text-gray-500">准备好开始你的冒险了吗？</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="card p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-12 h-12 bg-gradient-to-br from-primary-500 to-primary-600 rounded-2xl flex items-center justify-center">
                <Gamepad2 className="w-6 h-6 text-white" />
              </div>
              <div>
                <h2 className="text-xl font-bold text-gray-800">启动游戏</h2>
                <p className="text-gray-500 text-sm">选择版本并开始游戏</p>
              </div>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">游戏版本</label>
                <select
                  value={selectedVersion}
                  onChange={(e) => setSelectedVersion(e.target.value)}
                  className="input-field"
                >
                  {loading ? (
                    <option>加载中...</option>
                  ) : versions.length > 0 ? (
                    versions.map((v) => (
                      <option key={v.id} value={v.id}>{v.name}</option>
                    ))
                  ) : (
                    <option>没有找到版本</option>
                  )}
                </select>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    <Cpu className="w-4 h-4 inline mr-2" />
                    内存分配 (MB)
                  </label>
                  <input
                    type="number"
                    value={maxMemory}
                    onChange={(e) => setMaxMemory(Number(e.target.value))}
                    className="input-field"
                    min="512"
                    max="16384"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    <HardDrive className="w-4 h-4 inline mr-2" />
                    Java 路径
                  </label>
                  <select
                    value={javaPath}
                    onChange={(e) => setJavaPath(e.target.value)}
                    className="input-field"
                  >
                    {javaVersions.map((j) => (
                      <option key={j.path} value={j.path}>{j.version}</option>
                    ))}
                  </select>
                </div>
              </div>

              <button
                onClick={handleLaunch}
                disabled={!selectedVersion || launching}
                className="w-full btn-accent py-4 text-lg flex items-center justify-center gap-2"
              >
                {launching ? (
                  <>
                    <div className="w-6 h-6 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    正在启动...
                  </>
                ) : (
                  <>
                    <Play className="w-6 h-6" />
                    开始游戏
                  </>
                )}
              </button>
            </div>
          </div>

          <div className="card p-6">
            <div className="flex items-center gap-3 mb-4">
              <Server className="w-6 h-6 text-gray-600" />
              <h3 className="text-lg font-semibold text-gray-800">快速启动服务器</h3>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <button className="btn-secondary text-left">
                <p className="font-medium text-gray-800">我的服务器</p>
                <p className="text-sm text-gray-500">mc.myserver.com:25565</p>
              </button>
              <button className="btn-secondary text-left">
                <p className="font-medium text-gray-800">好友服务器</p>
                <p className="text-sm text-gray-500">192.168.1.100:25565</p>
              </button>
            </div>
          </div>
        </div>

        <div className="space-y-6">
          <div className="card p-6">
            <div className="flex items-center gap-3 mb-4">
              <Monitor className="w-6 h-6 text-gray-600" />
              <h3 className="text-lg font-semibold text-gray-800">统计信息</h3>
            </div>
            <div className="space-y-4">
              <div className="p-4 bg-gray-50 rounded-xl">
                <p className="text-sm text-gray-500">已安装版本</p>
                <p className="text-2xl font-bold text-gray-800">{versions.length}</p>
              </div>
              <div className="p-4 bg-primary-50 rounded-xl">
                <p className="text-sm text-primary-600">游戏时间</p>
                <p className="text-2xl font-bold text-primary-700">125 小时</p>
              </div>
              <div className="p-4 bg-accent-50 rounded-xl">
                <p className="text-sm text-accent-600">已下载资源</p>
                <p className="text-2xl font-bold text-accent-700">8 个</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
