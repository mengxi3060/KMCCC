interface Window {
  electronAPI?: {
    openExternal: (url: string) => Promise<void>
    getAppPath: () => Promise<string>
    getPlatform: () => Promise<string>
    windowMinimize: () => Promise<void>
    windowMaximize: () => Promise<void>
    windowClose: () => Promise<void>
    windowIsMaximized: () => Promise<boolean>
    isElectron: boolean
  }
}
