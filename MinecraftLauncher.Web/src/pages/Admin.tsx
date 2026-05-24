import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { apiClient, Resource } from '../services/api'
import { Shield, CheckCircle, XCircle, AlertCircle, Clock, Users, Box } from 'lucide-react'
import { Navigate } from 'react-router-dom'

export default function Admin() {
  const { user } = useAuth()
  const [pendingResources, setPendingResources] = useState<Resource[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<'review' | 'users' | 'resources'>('review')

  if (user?.role !== 'Admin' && user?.role !== 'Moderator') {
    return <Navigate to="/" replace />
  }

  useEffect(() => {
    loadPendingResources()
  }, [])

  const loadPendingResources = async () => {
    try {
      const response = await apiClient.get('/admin/review/queue')
      setPendingResources(response.data || [])
    } catch (err) {
      console.error('加载待审核资源失败:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleReview = async (resourceId: string, action: 'approve' | 'reject') => {
    try {
      await apiClient.post(`/admin/review/${resourceId}/${action}`, {
        comment: action === 'approve' ? '审核通过' : '资源不符合要求'
      })
      loadPendingResources()
    } catch (err) {
      console.error('审核失败:', err)
      alert('审核失败，请重试')
    }
  }

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div className="flex items-center gap-3 mb-2">
        <div className="w-10 h-10 bg-red-100 rounded-xl flex items-center justify-center">
          <Shield className="w-6 h-6 text-red-600" />
        </div>
        <div>
          <h1 className="text-3xl font-bold text-gray-800">管理后台</h1>
          <p className="text-gray-500">审核资源和管理社区</p>
        </div>
      </div>

      <div className="flex gap-2 border-b border-gray-200">
        <button
          onClick={() => setActiveTab('review')}
          className={`px-6 py-3 font-medium border-b-2 transition-colors ${
            activeTab === 'review'
              ? 'text-primary-600 border-primary-600'
              : 'text-gray-500 border-transparent hover:text-gray-700'
          }`}
        >
          <span className="flex items-center gap-2">
            <Clock className="w-4 h-4" />
            待审核
            {pendingResources.length > 0 && (
              <span className="px-2 py-0.5 bg-red-100 text-red-600 rounded-full text-xs">
                {pendingResources.length}
              </span>
            )}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('resources')}
          className={`px-6 py-3 font-medium border-b-2 transition-colors ${
            activeTab === 'resources'
              ? 'text-primary-600 border-primary-600'
              : 'text-gray-500 border-transparent hover:text-gray-700'
          }`}
        >
          <span className="flex items-center gap-2">
            <Box className="w-4 h-4" />
            资源管理
          </span>
        </button>
        <button
          onClick={() => setActiveTab('users')}
          className={`px-6 py-3 font-medium border-b-2 transition-colors ${
            activeTab === 'users'
              ? 'text-primary-600 border-primary-600'
              : 'text-gray-500 border-transparent hover:text-gray-700'
          }`}
        >
          <span className="flex items-center gap-2">
            <Users className="w-4 h-4" />
            用户管理
          </span>
        </button>
      </div>

      {activeTab === 'review' && (
        <div className="card overflow-hidden">
          {loading ? (
            <div className="p-8 text-center">
              <div className="w-8 h-8 border-3 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
              <p className="text-gray-500">加载中...</p>
            </div>
          ) : pendingResources.length === 0 ? (
            <div className="p-8 text-center">
              <CheckCircle className="w-12 h-12 text-accent-500 mx-auto mb-4" />
              <p className="text-gray-500">太棒了！没有待审核的资源</p>
            </div>
          ) : (
            <div className="divide-y divide-gray-100">
              {pendingResources.map((resource) => (
                <div key={resource.id} className="p-6">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-3">
                        <span className="px-3 py-1 bg-yellow-100 text-yellow-700 rounded-lg text-sm font-medium">
                          待审核
                        </span>
                        <span className="px-3 py-1 bg-gray-100 text-gray-700 rounded-lg text-sm">
                          {resource.type}
                        </span>
                      </div>
                      <h3 className="text-lg font-semibold text-gray-800 mb-2">{resource.name}</h3>
                      <p className="text-gray-500 mb-4">{resource.description}</p>
                      <div className="flex items-center gap-6 text-sm text-gray-500">
                        <span>
                          兼容版本: {resource.compatibilities.map(c => c.gameVersion).join(', ')}
                        </span>
                        <span>
                          加载器: {resource.compatibilities.map(c => c.loaderType).join(', ')}
                        </span>
                      </div>
                    </div>
                    <div className="flex flex-col gap-2 ml-6">
                      <button
                        onClick={() => handleReview(resource.id, 'approve')}
                        className="btn-accent flex items-center gap-2 whitespace-nowrap"
                      >
                        <CheckCircle className="w-4 h-4" />
                        通过
                      </button>
                      <button
                        onClick={() => handleReview(resource.id, 'reject')}
                        className="btn-secondary text-red-600 border-red-200 hover:bg-red-50 flex items-center gap-2 whitespace-nowrap"
                      >
                        <XCircle className="w-4 h-4" />
                        拒绝
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'resources' && (
        <div className="card p-8 text-center">
          <Box className="w-12 h-12 text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500">资源管理功能即将推出</p>
        </div>
      )}

      {activeTab === 'users' && (
        <div className="card p-8 text-center">
          <Users className="w-12 h-12 text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500">用户管理功能即将推出</p>
        </div>
      )}
    </div>
  )
}
