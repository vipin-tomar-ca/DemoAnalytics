import axios from 'axios'
import { useFilters } from './stores/filters'
import { useAuth } from './stores/auth'

const api = axios.create({ baseURL: '/api', timeout: 15000 })

api.interceptors.request.use((config) => {
  config.headers = config.headers || {}
  const auth = useAuth()
  if (auth.token) config.headers['Authorization'] = `Bearer ${auth.token}`
  config.headers['X-Tenant-Id'] = 'demo-tenant'
  const f = useFilters()
  const params: any = { ...config.params }
  if (f.from) params.from = f.from
  if (f.to) params.to = f.to
  if (f.province) params.province = f.province
  if (f.org) params.org = f.org
  if (f.jobFamily) params.jobFamily = f.jobFamily
  config.params = params
  return config
})

export type Kpis = {
  headcount: number
  hiresMtd: number
  exitsMtd: number
  overtimePct: number
  avgSalary: number
}

export const login = async (username: string, password: string) => (await api.post('/auth/login', { username, password })).data

export const fetchKpis = async () => (await api.get<Kpis>('/kpis')).data
export const fetchHeadcountTrend = async () => (await api.get('/headcount/trend')).data
export const fetchGeoHeadcount = async () => (await api.get('/geo/headcount')).data
export const fetchTimeHeatmap = async () => (await api.get('/time/heatmap')).data
export const fetchCompensation = async () => (await api.get('/compensation/summary')).data

export default api
