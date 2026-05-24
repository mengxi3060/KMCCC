import { useState, useEffect } from 'react'
import { apiClient, Resource } from '../services/api'
import { Box, Search, Filter, Download, Heart, Tag, Plus, Gamepad2, Layers, Palette, Sparkles } from 'lucide-react'

export default function Resources() {
  const [resources, setResources] = useState<Resource[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [typeFilter, setTypeFilter] = useState<string>('')
  const [showUploadModal, setShowUploadModal] = useState(false)

  useEffect(() => {
    loadResources()
  }, [])

  const loadResources = async () => {
    try {
      const response = await apiClient.get('/resources')
      setResources(response.data.resources || [])
    } catch (err) {
      console.error('加载资源失败:', err)
    } finally {
      setLoading(false)
    }
  }

  const getResourceIcon = (type: string) => {
    switch (type) {
      case 'Mod':
        return <Gamepad2 className="w-6 h-6" />
      case 'Modpack':
        return <Layers className="w-6 h-6" />
      case 'Shader':
        return <Sparkles className="w-6 h-6" />
      case 'TexturePack':
        return <Palette className="w-6 h-6" />
      default:
        return <Box className="w-6 h-6" />
    }
  }

  const getResourceColor = (type: string) => {
    switch (type) {
      case 'Mod':
        return 'bg-primary-100 text-primary-600'
      case 'Modpack':
        return 'bg-accent-100 text-accent-600'
      case 'Shader':
        return 'bg-purple-100 text-purple-600'
      case 'TexturePack':
        return 'bg-orange-100 text-orange-600'
      default:
        return 'bg-gray-100 text-gray-600'
    }
  }

  const filteredResources = resources.filter((r) => {
    const matchesSearch = r.name.toLowerCase().includes(search.toLowerCase()) ||
                         r.description.toLowerCase().includes(search.toLowerCase())
    const matchesType = !typeFilter || r.type === typeFilter
    return matchesSearch && matchesType && r.status === 'Approved'
  })

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">资源社区</h1>
          <p className="text-gray-500">发现和分享精彩的 Minecraft 资源</p>
        </div>
        <button
          onClick={() => setShowUploadModal(true)}
          className="btn-primary flex items-center gap-2"
        >
          <Plus className="w-5 h-5" />
          上传资源
        </button>
      </div>

      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1 relative">
          <Search className="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="搜索资源..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="input-field pl-12"
          />
        </div>
        <div className="flex gap-2">
          <select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
            className="input-field w-auto"
          >
            <option value="">全部类型</option>
            <option value="Mod">模组</option>
            <option value="Modpack">整合包</option>
            <option value="Shader">光影</option>
            <option value="TexturePack">材质包</option>
          </select>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-16">
          <div className="w-10 h-10 border-3 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
          <p className="text-gray-500">加载中...</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredResources.map((resource) => (
            <div key={resource.id} className="card overflow-hidden hover:shadow-md transition-shadow">
              <div className="h-40 bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center">
                {resource.screenshots.length > 0 ? (
                  <img src={resource.screenshots[0]} alt="" className="w-full h-full object-cover" />
                ) : (
                  <div className={`w-16 h-16 rounded-2xl ${getResourceColor(resource.type)} flex items-center justify-center`}>
                    {getResourceIcon(resource.type)}
                  </div>
                )}
              </div>
              <div className="p-5">
                <div className="flex items-center gap-2 mb-2">
                  <span className={`px-2.5 py-1 rounded-lg text-xs font-medium ${getResourceColor(resource.type)}`}>
                    {resource.type}
                  </span>
                </div>
                <h3 className="text-lg font-semibold text-gray-800 mb-2">{resource.name}</h3>
                <p className="text-gray-500 text-sm mb-4 line-clamp-2">{resource.description}</p>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span className="flex items-center gap-1">
                      <Download className="w-4 h-4" />
                      {resource.downloadCount}
                    </span>
                    <span className="flex items-center gap-1">
                      <Heart className="w-4 h-4" />
                      {resource.likeCount}
                    </span>
                  </div>
                  <button className="btn-primary text-sm py-2 px-4">
                    安装
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {showUploadModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="card p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-bold text-gray-800">上传资源</h2>
              <button
                onClick={() => setShowUploadModal(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                ✕
              </button>
            </div>
            <p className="text-gray-500 mb-6">资源上传功能即将推出，敬请期待！</p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowUploadModal(false)}
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
