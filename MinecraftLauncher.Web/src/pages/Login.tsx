import { useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { Gamepad2, User, Mail, Lock, LogIn, UserPlus, Monitor, AlertCircle } from 'lucide-react'

export default function Login() {
  const [mode, setMode] = useState<'login' | 'register' | 'offline'>('login')
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [playerName, setPlayerName] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const { login, loginMicrosoft, loginOffline, register } = useAuth()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      if (mode === 'login') {
        await login(email, password)
      } else if (mode === 'register') {
        await register(username, email, password)
        setMode('login')
      } else if (mode === 'offline') {
        await loginOffline(playerName)
      }
    } catch (err: any) {
      setError(err.response?.data?.message || '操作失败')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-gradient-to-br from-primary-500 to-accent-500 rounded-3xl flex items-center justify-center mx-auto mb-4 shadow-lg">
            <Gamepad2 className="w-10 h-10 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">Minecraft 启动器</h1>
          <p className="text-gray-500">开始你的冒险之旅</p>
        </div>

        <div className="card p-8">
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3 text-red-700">
              <AlertCircle className="w-5 h-5 flex-shrink-0" />
              <span className="text-sm">{error}</span>
            </div>
          )}

          <div className="flex gap-2 mb-6">
            <button
              onClick={() => { setMode('login'); setError(''); }}
              className={`flex-1 py-2.5 px-4 rounded-xl text-sm font-medium transition-all ${
                mode === 'login'
                  ? 'bg-primary-600 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              账号登录
            </button>
            <button
              onClick={() => { setMode('offline'); setError(''); }}
              className={`flex-1 py-2.5 px-4 rounded-xl text-sm font-medium transition-all ${
                mode === 'offline'
                  ? 'bg-accent-600 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              离线模式
            </button>
          </div>

          {mode === 'login' || mode === 'register' ? (
            <form onSubmit={handleSubmit} className="space-y-4">
              {mode === 'register' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    <User className="w-4 h-4 inline mr-2" />
                    用户名
                  </label>
                  <input
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    className="input-field"
                    placeholder="输入用户名"
                    required
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Mail className="w-4 h-4 inline mr-2" />
                  邮箱
                </label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="input-field"
                  placeholder="输入邮箱"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Lock className="w-4 h-4 inline mr-2" />
                  密码
                </label>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="input-field"
                  placeholder="输入密码"
                  required
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full btn-primary flex items-center justify-center gap-2"
              >
                {loading ? (
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                ) : (
                  <>
                    <LogIn className="w-5 h-5" />
                    {mode === 'login' ? '登录' : '注册'}
                  </>
                )}
              </button>

              <button
                type="button"
                onClick={loginMicrosoft}
                className="w-full btn-secondary flex items-center justify-center gap-2"
              >
                <svg className="w-5 h-5" viewBox="0 0 21 21" fill="currentColor">
                  <rect x="1" y="1" width="9" height="9" fill="#f25022" />
                  <rect x="11" y="1" width="9" height="9" fill="#7fba00" />
                  <rect x="1" y="11" width="9" height="9" fill="#00a4ef" />
                  <rect x="11" y="11" width="9" height="9" fill="#ffb900" />
                </svg>
                微软账号登录
              </button>

              <p className="text-center text-sm text-gray-500">
                {mode === 'login' ? '还没有账号？' : '已有账号？'}{' '}
                <button
                  type="button"
                  onClick={() => { setMode(mode === 'login' ? 'register' : 'login'); setError(''); }}
                  className="text-primary-600 hover:text-primary-700 font-medium"
                >
                  {mode === 'login' ? '立即注册' : '立即登录'}
                </button>
              </p>
            </form>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Monitor className="w-4 h-4 inline mr-2" />
                  玩家名称
                </label>
                <input
                  type="text"
                  value={playerName}
                  onChange={(e) => setPlayerName(e.target.value)}
                  className="input-field"
                  placeholder="输入玩家名称"
                  required
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full btn-accent flex items-center justify-center gap-2"
              >
                {loading ? (
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                ) : (
                  <>
                    <Monitor className="w-5 h-5" />
                    进入离线模式
                  </>
                )}
              </button>

              <p className="text-center text-sm text-gray-500">
                离线模式下无法进入正版服务器
              </p>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
