import { useState, useEffect } from 'react'
import { apiClient, Resource } from '../services/api'
import { Box, Search, Download, Heart, Plus, Gamepad2, Layers, Palette, Sparkles, Upload, X, FileUp, Tag, FileText, CheckCircle } from 'lucide-react'

const RESOURCE_TYPES = [
  { key: '', label: '全部', icon: Box, color: 'bg-gray-100 text-gray-600', activeColor: 'bg-gray-600 text-white' },
  { key: 'Mod', label: '模组', icon: Gamepad2, color: 'bg-primary-100 text-primary-600', activeColor: 'bg-primary-600 text-white' },
  { key: 'Modpack', label: '整合包', icon: Layers, color: 'bg-accent-100 text-accent-600', activeColor: 'bg-accent-600 text-white' },
  { key: 'Shader', label: '光影', icon: Sparkles, color: 'bg-purple-100 text-purple-600', activeColor: 'bg-purple-600 text-white' },
  { key: 'TexturePack', label: '材质包', icon: Palette, color: 'bg-orange-100 text-orange-600', activeColor: 'bg-orange-600 text-white' }
]

export default function Resources() {
  const [resources, setResources] = useState<Resource[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [typeFilter, setTypeFilter] = useState<string>('')
  const [showUploadModal, setShowUploadModal] = useState(false)

  const [uploadForm, setUploadForm] = useState({
    name: '',
    type: 'Mod' as 'Mod' | 'Modpack' | 'Shader' | 'TexturePack',
    description: '',
    tags: '',
    gameVersion: '',
    loaderType: 'None' as 'None' | 'Forge' | 'Fabric' | 'Quilt',
    file: null as File | null
  })
  const [uploading, setUploading] = useState(false)
  const [uploadSuccess, setUploadSuccess] = useState(false)

  useEffect(() => {
    loadResources()
  }, [])

  const loadResources = async () => {
    try {
      const response = await apiClient.get('/resources')
      setResources(response.data.resources || [])
    } catch {
      setResources([])
    } finally {
      setLoading(false)
    }
  }

  const getResourceIcon = (type: string) => {
    switch (type) {
      case 'Mod': return <Gamepad2 className="w-6 h-6" />
      case 'Modpack': return <Layers className="w-6 h-6" />
      case 'Shader': return <Sparkles className="w-6 h-6" />
      case 'TexturePack': return <Palette className="w-6 h-6" />
      default: return <Box className="w-6 h-6" />
    }
  }

  const getResourceColor = (type: string) => {
    switch (type) {
      case 'Mod': return 'bg-primary-100 text-primary-600'
      case 'Modpack': return 'bg-accent-100 text-accent-600'
      case 'Shader': return 'bg-purple-100 text-purple-600'
      case 'TexturePack': return 'bg-orange-100 text-orange-600'
      default: return 'bg-gray-100 text-gray-600'
    }
  }

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'Mod': return '模组'
      case 'Modpack': return '整合包'
      case 'Shader': return '光影'
      case 'TexturePack': return '材质包'
      default: return type
    }
  }

  const filteredResources = resources.filter((r) => {
    const matchesSearch = r.name.toLowerCase().includes(search.toLowerCase()) ||
                         r.description.toLowerCase().includes(search.toLowerCase())
    const matchesType = !typeFilter || r.type === typeFilter
    return matchesSearch && matchesType && r.status === 'Approved'
  })

  const handleUpload = async () => {
    if (!uploadForm.name || !uploadForm.description) return
    setUploading(true)
    try {
      const formData = new FormData()
      formData.append('name', uploadForm.name)
      formData.append('type', uploadForm.type)
      formData.append('description', uploadForm.description)
      formData.append('tags', uploadForm.tags)
      if (uploadForm.gameVersion) {
        formData.append('gameVersion', uploadForm.gameVersion)
        formData.append('loaderType', uploadForm.loaderType)
      }
      if (uploadForm.file) {
        formData.append('file', uploadForm.file)
      }

      await apiClient.post('/resources', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })

      setUploadSuccess(true)
      setTimeout(() => {
        setShowUploadModal(false)
        setUploadSuccess(false)
        setUploadForm({
          name: '',
          type: 'Mod',
          description: '',
          tags: '',
          gameVersion: '',
          loaderType: 'None',
          file: null
        })
        loadResources()
      }, 1500)
    } catch (err: any) {
      alert('上传失败: ' + (err.response?.data?.message || err.message))
    } finally {
      setUploading(false)
    }
  }

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
          <Upload className="w-5 h-5" />
          上传资源
        </button>
      </div>

      <div className="flex flex-wrap gap-3">
        {RESOURCE_TYPES.map(({ key, label, icon: Icon, color, activeColor }) => (
          <button
            key={key}
            onClick={() => setTypeFilter(key)}
            className={`flex items-center gap-2 px-5 py-3 rounded-xl text-sm font-medium transition-all shadow-sm ${
              typeFilter === key ? activeColor : `${color} hover:shadow-md`
            }`}
          >
            <Icon className="w-5 h-5" />
            {label}
            {key && (
              <span className="text-xs opacity-75">
                ({resources.filter(r => r.type === key && r.status === 'Approved').length})
              </span>
            )}
          </button>
        ))}
      </div>

      <div className="relative">
        <Search className="w-5 h-5 absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          type="text"
          placeholder="搜索资源名称或描述..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="input-field pl-12"
        />
      </div>

      {loading ? (
        <div className="text-center py-16">
          <div className="w-10 h-10 border-3 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
          <p className="text-gray-500">加载中...</p>
        </div>
      ) : filteredResources.length === 0 ? (
        <div className="card p-12 text-center">
          <Box className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500 mb-2">暂无资源</p>
          <p className="text-gray-400 text-sm">成为第一个上传资源的玩家吧！</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredResources.map((resource) => (
            <div key={resource.id} className="card overflow-hidden hover:shadow-md transition-shadow">
              <div className="h-40 bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center relative">
                {resource.screenshots.length > 0 ? (
                  <img src={resource.screenshots[0]} alt="" className="w-full h-full object-cover" />
                ) : (
                  <div className={`w-16 h-16 rounded-2xl ${getResourceColor(resource.type)} flex items-center justify-center`}>
                    {getResourceIcon(resource.type)}
                  </div>
                )}
                <span className={`absolute top-3 left-3 px-2.5 py-1 rounded-lg text-xs font-medium ${getResourceColor(resource.type)}`}>
                  {getTypeLabel(resource.type)}
                </span>
              </div>
              <div className="p-5">
                <h3 className="text-lg font-semibold text-gray-800 mb-2">{resource.name}</h3>
                <p className="text-gray-500 text-sm mb-3 line-clamp-2">{resource.description}</p>
                {resource.tags.length > 0 && (
                  <div className="flex flex-wrap gap-1.5 mb-3">
                    {resource.tags.slice(0, 3).map((tag, i) => (
                      <span key={i} className="px-2 py-0.5 bg-gray-100 text-gray-500 rounded text-xs">
                        {tag}
                      </span>
                    ))}
                  </div>
                )}
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
            {uploadSuccess ? (
              <div className="text-center py-8">
                <CheckCircle className="w-16 h-16 text-accent-500 mx-auto mb-4" />
                <h3 className="text-xl font-bold text-gray-800 mb-2">上传成功！</h3>
                <p className="text-gray-500">资源已提交审核，审核通过后将自动发布</p>
              </div>
            ) : (
              <>
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-xl font-bold text-gray-800">上传资源</h2>
                  <button
                    onClick={() => setShowUploadModal(false)}
                    className="text-gray-400 hover:text-gray-600"
                  >
                    <X className="w-5 h-5" />
                  </button>
                </div>

                <div className="space-y-5">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">资源名称 *</label>
                    <input
                      type="text"
                      value={uploadForm.name}
                      onChange={(e) => setUploadForm({ ...uploadForm, name: e.target.value })}
                      className="input-field"
                      placeholder="输入资源名称"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">资源类型 *</label>
                    <div className="grid grid-cols-4 gap-2">
                      {[
                        { key: 'Mod', label: '模组', icon: Gamepad2 },
                        { key: 'Modpack', label: '整合包', icon: Layers },
                        { key: 'Shader', label: '光影', icon: Sparkles },
                        { key: 'TexturePack', label: '材质包', icon: Palette }
                      ].map(({ key, label, icon: Icon }) => (
                        <button
                          key={key}
                          onClick={() => setUploadForm({ ...uploadForm, type: key as any })}
                          className={`flex flex-col items-center gap-2 p-3 rounded-xl border-2 transition-all ${
                            uploadForm.type === key
                              ? 'border-primary-500 bg-primary-50'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <Icon className={`w-5 h-5 ${uploadForm.type === key ? 'text-primary-600' : 'text-gray-400'}`} />
                          <span className={`text-sm font-medium ${uploadForm.type === key ? 'text-primary-600' : 'text-gray-500'}`}>
                            {label}
                          </span>
                        </button>
                      ))}
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">资源描述 *</label>
                    <textarea
                      value={uploadForm.description}
                      onChange={(e) => setUploadForm({ ...uploadForm, description: e.target.value })}
                      className="input-field min-h-[100px] resize-y"
                      placeholder="详细描述你的资源..."
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      <Tag className="w-4 h-4 inline mr-1" />
                      标签（用逗号分隔）
                    </label>
                    <input
                      type="text"
                      value={uploadForm.tags}
                      onChange={(e) => setUploadForm({ ...uploadForm, tags: e.target.value })}
                      className="input-field"
                      placeholder="例如: 工业,科技,魔法"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">兼容游戏版本</label>
                      <input
                        type="text"
                        value={uploadForm.gameVersion}
                        onChange={(e) => setUploadForm({ ...uploadForm, gameVersion: e.target.value })}
                        className="input-field"
                        placeholder="例如: 1.20.4"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">加载器</label>
                      <select
                        value={uploadForm.loaderType}
                        onChange={(e) => setUploadForm({ ...uploadForm, loaderType: e.target.value as any })}
                        className="input-field"
                      >
                        <option value="None">无</option>
                        <option value="Forge">Forge</option>
                        <option value="Fabric">Fabric</option>
                        <option value="Quilt">Quilt</option>
                      </select>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      <FileUp className="w-4 h-4 inline mr-1" />
                      上传文件
                    </label>
                    <div className="border-2 border-dashed border-gray-300 rounded-xl p-6 text-center hover:border-primary-400 transition-colors">
                      <input
                        type="file"
                        onChange={(e) => setUploadForm({ ...uploadForm, file: e.target.files?.[0] || null })}
                        className="hidden"
                        id="file-upload"
                        accept=".jar,.zip,.litematic,.mcworld"
                      />
                      <label htmlFor="file-upload" className="cursor-pointer">
                        {uploadForm.file ? (
                          <div className="flex items-center justify-center gap-2 text-primary-600">
                            <FileText className="w-5 h-5" />
                            <span className="font-medium">{uploadForm.file.name}</span>
                          </div>
                        ) : (
                          <>
                            <FileUp className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                            <p className="text-gray-500 text-sm">点击选择文件或拖拽到此处</p>
                            <p className="text-gray-400 text-xs mt-1">支持 .jar .zip .litematic .mcworld</p>
                          </>
                        )}
                      </label>
                    </div>
                  </div>

                  <div className="flex justify-end gap-3 pt-2">
                    <button
                      onClick={() => setShowUploadModal(false)}
                      className="btn-secondary"
                    >
                      取消
                    </button>
                    <button
                      onClick={handleUpload}
                      disabled={uploading || !uploadForm.name || !uploadForm.description}
                      className="btn-primary flex items-center gap-2"
                    >
                      {uploading ? (
                        <>
                          <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                          上传中...
                        </>
                      ) : (
                        <>
                          <Upload className="w-5 h-5" />
                          提交审核
                        </>
                      )}
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
