import { useState, useEffect } from 'react'
import { Settings as SettingsIcon, Folder, Cpu, HardDrive, User, Bell, Palette } from 'lucide-react'

export default function Settings() {
  const [gamePath, setGamePath] = useState('')
  const [javaPath, setJavaPath] = useState('')
  const [maxMemory, setMaxMemory] = useState('2048')
  const [autoUpdate, setAutoUpdate] = useState(true)
  const [notifications, setNotifications] = useState(true)

  useEffect(() => {
    const saved = localStorage.getItem('launcherSettings')
    if (saved) {
      const settings = JSON.parse(saved)
      setGamePath(settings.gamePath || '')
      setJavaPath(settings.javaPath || '')
      setMaxMemory(settings.maxMemory || '2048')
      setAutoUpdate(settings.autoUpdate !== false)
      setNotifications(settings.notifications !== false)
    }
  }, [])

  const saveSettings = () => {
    localStorage.setItem('launcherSettings', JSON.stringify({
      gamePath,
      javaPath,
      maxMemory,
      autoUpdate,
      notifications
    }))
    alert('设置已保存！')
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
              <input
                type="text"
                value={gamePath}
                onChange={(e) => setGamePath(e.target.value)}
                placeholder="C:\Users\...\.minecraft"
                className="input-field"
              />
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-accent-100 rounded-xl flex items-center justify-center">
              <Cpu className="w-5 h-5 text-accent-600" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-800">Java 设置</h2>
              <p className="text-gray-500 text-sm">配置 Java 运行环境</p>
            </div>
          </div>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Java 路径</label>
              <input
                type="text"
                value={javaPath}
                onChange={(e) => setJavaPath(e.target.value)}
                placeholder="自动检测"
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
            </div>
          </div>
        </div>

        <div className="card p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-purple-100 rounded-xl flex items-center justify-center">
              <Bell className="w-5 h-5 text-purple-600" />
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
          <button className="btn-secondary">重置默认</button>
          <button onClick={saveSettings} className="btn-primary">保存设置</button>
        </div>
      </div>
    </div>
  )
}
