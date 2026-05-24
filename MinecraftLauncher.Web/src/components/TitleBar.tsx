import { useState, useEffect } from 'react'
import { Minus, Square, X } from 'lucide-react'

export default function TitleBar() {
  const [isMaximized, setIsMaximized] = useState(false)
  const [isElectronEnv, setIsElectronEnv] = useState(false)

  useEffect(() => {
    const electronAPI = (window as any).electronAPI
    if (electronAPI?.isElectron) {
      setIsElectronEnv(true)
      electronAPI.windowIsMaximized().then((maximized: boolean) => {
        setIsMaximized(maximized)
      })
    }
  }, [])

  if (!isElectronEnv) return null

  const electronAPI = (window as any).electronAPI

  const handleMinimize = () => electronAPI.windowMinimize()
  const handleMaximize = () => {
    electronAPI.windowMaximize()
    setIsMaximized(!isMaximized)
  }
  const handleClose = () => electronAPI.windowClose()

  return (
    <div className="flex items-center justify-between h-10 bg-white border-b border-gray-100 px-4 select-none" style={{ WebkitAppRegion: 'drag' } as any}>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium text-gray-600">Minecraft Launcher</span>
      </div>
      <div className="flex items-center gap-1" style={{ WebkitAppRegion: 'no-drag' } as any}>
        <button
          onClick={handleMinimize}
          className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors"
        >
          <Minus className="w-4 h-4 text-gray-500" />
        </button>
        <button
          onClick={handleMaximize}
          className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors"
        >
          <Square className="w-3.5 h-3.5 text-gray-500" />
        </button>
        <button
          onClick={handleClose}
          className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-100 hover:text-red-600 transition-colors"
        >
          <X className="w-4 h-4 text-gray-500" />
        </button>
      </div>
    </div>
  )
}
