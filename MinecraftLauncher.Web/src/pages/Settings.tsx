import { useState, useEffect } from 'react'
import { Settings as SettingsIcon, Folder, Cpu, HardDrive, Bell, Search, RefreshCw, CheckCircle } from 'lucide-react'
import { apiClient, JavaInfo } from '../services/api'

export default function Settings() {
  const [gamePath, setGamePath] = useState('')
  const [javaPath, setJavaPath] = useState('')
  const [maxMemory, setMaxMemory] = useState('2048')
  const [minMemory, setMinMemory] = useState('512')
  const [autoUpdate, setAutoUpdate] = useState(true)
  const [notifications, setNotifications] = useState(true)
  const [detectedJavas, setDetectedJavas] = useState<JavaInfo[]>([])
  const [detecting, setDetecting] = useState(false)
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    const savedSettings = localStorage.getItem('launcherSettings')
    if (savedSettings) {
      const s = JSON.parse(savedSettings)
      setGamePath(s.gamePath || '')
      setJavaPath(s.javaPath || '')
      setMaxMemory(s.maxMemory || '2048')
      setMinMemory(s.minMemory || '512')
      setAutoUpdate(s.autoUpdate !== false)
      setNotifications(s.notifications !== false)
    }
    detectJava()
  }, [])

  const detectJava = async () => {
    setDetecting(true)
    try {
      const response = await apiClient.get('/launcher/java')
      const javas: JavaInfo[] = response.data
      setDetectedJavas(javas)
      if (javas.length > 0 && !javaPath) {
        setJavaPath(javas[0].path)
      }
    } catch {
      const commonPaths = [
        'C:\\Program Files\\Java\\jdk-17\\bin\\javaw.exe',
        'C:\\Program Files\\Java\\jdk-21\\bin\\javaw.exe',
        'C:\\Program Files (x86)\\Java\\jre1.8.0_XXX\\bin\\javaw.exe',
        '/usr/bin/java',
        '/usr/lib/jvm/java-17-openjdk/bin/java'
      ]
      setDetectedJavas(commonPaths.map(p => ({ path: p, version: p.includes('17') ? '17' : p.includes('21') ? '21' : '8' })))
    } finally {
      setDetecting(false)
    }
  }

  const saveSettings = () => {
    localStorage.setItem('launcherSettings', JSON.stringify({
      gamePath,
      javaPath,
      maxMemory,
      minMemory,
      autoUpdate,
      notifications
    }))
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-800 mb-2">设置</h1>
        <p className="text-gray-500">配置你的启动器选项</p>
      </div>

      <div className="space-y-6">
        <div className="card p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-primary-100 rounded-xl flex items-center justify-center">
              <Folder className="w-5 h-5 text-primary-600" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-800">游戏目录</h2>
              <p className="text-gray-500 text-sm">Minecraft 游戏文件存储位置</p>
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">游戏路径</label>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={gamePath}
                  onChange={(e) => setGamePath(e.target.value)}
                  placeholder="自动检测 .minecraft 目录"
                  className="input-field flex-1"
                />
                <button className="btn-secondary flex items-center gap-2 whitespace-nowrap">
                  <Search className="w-4 h-4" />
                  浏览
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-accent-100 rounded-xl flex items-center justify-center">
                <Cpu className="w-5 h-5 text-accent-600" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-gray-800">Java 设置</h2>
                <p className="text-gray-500 text-sm">配置 Java 运行环境</p>
              </div>
            </div>
            <button
              onClick={detectJava}
              disabled={detecting}
              className="btn-secondary flex items-center gap-2"
            >
              <RefreshCw className={`w-4 h-4 ${detecting ? 'animate-spin' : ''}`} />
              {detecting ? '检测中...' : '自动检测'}
            </button>
          </div>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Java 路径</label>
              <div className="flex gap-2">
                <select
                  value={javaPath}
                  onChange={(e) => setJavaPath(e.target.value)}
                  className="input-field flex-1"
                >
                  <option value="">自动选择</option>
                  {detectedJavas.map((j, i) => (
                    <option key={i} value={j.path}>Java {j.version} - {j.path}</option>
                  ))}
                </select>
                <button className="btn-secondary flex items-center gap-2 whitespace-nowrap">
                  <Search className="w-4 h-4" />
                  浏览
                </button>
              </div>
              {detectedJavas.length > 0 && (
                <p className="text-xs text-accent-600 mt-2 flex items-center gap-1">
                  <CheckCircle className="w-3 h-3" />
                  已检测到 {detectedJavas.length} 个 Java 环境
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-purple-100 rounded-xl flex items-center justify-center">
              <HardDrive className="w-5 h-5 text-purple-600" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-800">内存分配</h2>
              <p className="text-gray-500 text-sm">配置游戏运行内存</p>
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                最小内存 (MB)
              </label>
              <input
                type="number"
                value={minMemory}
                onChange={(e) => setMinMemory(e.target.value)}
                min="256"
                max="8192"
                className="input-field"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                最大内存 (MB)
              </label>
              <input
                type="number"
                value={maxMemory}
                onChange={(e) => setMaxMemory(e.target.value)}
                min="512"
                max="16384"
                className="input-field"
              />
              <div className="mt-3 flex gap-2">
                {[1024, 2048, 4096, 8192].map(v => (
                  <button
                    key={v}
                    onClick={() => setMaxMemory(String(v))}
                    className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-all ${
                      maxMemory === String(v)
                        ? 'bg-primary-600 text-white'
                        : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                    }`}
                  >
                    {v >= 1024 ? `${v / 1024}G` : `${v}M`}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-orange-100 rounded-xl flex items-center justify-center">
              <Bell className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-800">偏好设置</h2>
              <p className="text-gray-500 text-sm">个性化你的启动器</p>
            </div>
          </div>
          <div className="space-y-4">
            <label className="flex items-center justify-between p-4 bg-gray-50 rounded-xl cursor-pointer">
              <div>
                <p className="font-medium text-gray-800">自动检查更新</p>
                <p className="text-sm text-gray-500">启动时自动检查启动器更新</p>
              </div>
              <div
                onClick={() => setAutoUpdate(!autoUpdate)}
                className={`w-12 h-6 rounded-full transition-colors ${
                  autoUpdate ? 'bg-primary-600' : 'bg-gray-300'
                }`}
              >
                <div
                  className={`w-5 h-5 bg-white rounded-full shadow-md transform transition-transform ${
                    autoUpdate ? 'translate-x-6' : 'translate-x-0.5'
                  } translate-y-0.5`}
                />
              </div>
            </label>
            <label className="flex items-center justify-between p-4 bg-gray-50 rounded-xl cursor-pointer">
              <div>
                <p className="font-medium text-gray-800">通知提醒</p>
                <p className="text-sm text-gray-500">接收资源更新和社区动态</p>
              </div>
              <div
                onClick={() => setNotifications(!notifications)}
                className={`w-12 h-6 rounded-full transition-colors ${
                  notifications ? 'bg-primary-600' : 'bg-gray-300'
                }`}
              >
                <div
                  className={`w-5 h-5 bg-white rounded-full shadow-md transform transition-transform ${
                    notifications ? 'translate-x-6' : 'translate-x-0.5'
                  } translate-y-0.5`}
                />
              </div>
            </label>
          </div>
        </div>

        <div className="flex justify-end gap-3">
          <button
            onClick={() => {
              setGamePath('')
              setJavaPath('')
              setMaxMemory('2048')
              setMinMemory('512')
              setAutoUpdate(true)
              setNotifications(true)
            }}
            className="btn-secondary"
          >
            重置默认
          </button>
          <button onClick={saveSettings} className="btn-primary flex items-center gap-2">
            {saved ? (
              <>
                <CheckCircle className="w-5 h-5" />
                已保存
              </>
            ) : (
              '保存设置'
            )}
          </button>
        </div>
      </div>
    </div>
  )
}
