import axios from 'axios'
import { useFilters } from './stores/filters'
import { useAuth } from './stores/auth'

const api = axios.create({ baseURL: 'http://localhost:5000/api', timeout: 15000 })

api.interceptors.request.use((config) => {
  config.headers = config.headers || {}
  const auth = useAuth()
  if (auth.token) config.headers['Authorization'] = `Bearer ${auth.token}`
  config.headers['X-Tenant-Id'] = 'demo-tenant'
  const f = useFilters()
  const params: Record<string, unknown> = { ...config.params }
  if (f.from) params.from = f.from.toISOString().split('T')[0] // Format date to YYYY-MM-DD
  if (f.to) params.to = f.to.toISOString().split('T')[0] // Format date to YYYY-MM-DD
  if (f.province) params.province = f.province
  if (f.org) params.org = f.org
  if (f.jobFamily) params.jobFamily = f.jobFamily
  config.params = params
  return config
})

export type Kpis = {
  overallCost: number
  overtimeCost: number
  overtimePct: number
  averageSalary: number
  headcount: number
  newHires: number
  terminations: number
  turnoverCost: number
  turnoverRate: number
}

export type LoginResponse = { token: string; username: string; role: string }
export type HeadcountTrend = { labels: string[]; headcount: number[]; hires: number[]; exits: number[] }
export type GeoHeadcountRow = { name: string; value: number }
export type TimeHeatmap = { days: string[]; hours: string[]; values: Array<[number, number, number]> }
export type CompensationSummary = { categories: string[]; boxData: number[][] }
export type TcowBreakdown = { total: number; breakdown: Array<{ name: string; value: number }> }
export type BudgetVariance = { labels: string[]; budget: number[]; actual: number[]; variancePct: number[] }
export type OvertimeCosts = { labels: string[]; costs: number[]; hours: number[] }
export type AbsenteeismCosts = { labels: string[]; costs: number[]; ratePct: number[] }
export type ProcessEfficiency = { timeToRunPayrollHours: number; accuracyRatePct: number }
export type Competitiveness = { points: Array<[number, number, number]> }
export type PayGap = { steps: Array<{ label: string; delta: number }> }
export type ImpactFinance = { orgs: Array<{ org: string; prRatio: number; salesPerEmp: number; productivityIndex: number }> }
export type TurnoverCosts = { labels: string[]; replacementCost: number[]; voluntaryPct: number[]; involuntaryPct: number[] }

export const login = async (username: string, password: string) =>
  (await api.post<LoginResponse>('/auth/login', { username, password })).data

export const fetchKpisAll = async () => (await api.get<Kpis>('/kpis/all')).data // New function for all KPIs
export const fetchKpis = async (from: string, to: string) => (await api.get<Kpis>('/kpis', { params: { from, to } })).data // Modified fetchKpis
export const fetchHeadcountTrend = async () => (await api.get<HeadcountTrend>('/headcount/trend')).data
export const fetchGeoHeadcount = async () => (await api.get<GeoHeadcountRow[]>('/geo/headcount')).data
export const fetchTimeHeatmap = async () => (await api.get<TimeHeatmap>('/time/heatmap')).data
export const fetchCompensation = async () => (await api.get<CompensationSummary>('/compensation/summary')).data
export const fetchCostsTcow = async () => (await api.get<TcowBreakdown>('/costs/tcow')).data
export const fetchCostsBudgetVariance = async () => (await api.get<BudgetVariance>('/costs/budget-variance')).data
export const fetchCostsOvertime = async () => (await api.get<OvertimeCosts>('/costs/overtime')).data
export const fetchCostsAbsenteeism = async () => (await api.get<AbsenteeismCosts>('/costs/absenteeism')).data
export const fetchProcessEfficiency = async () => (await api.get<ProcessEfficiency>('/process/efficiency')).data
export const fetchCompCompetitiveness = async () => (await api.get<Competitiveness>('/comp/competitiveness')).data
export const fetchEquityPayGap = async () => (await api.get<PayGap>('/equity/paygap')).data
export const fetchImpactFinance = async () => (await api.get<ImpactFinance>('/impact/finance')).data
export const fetchTurnoverCosts = async () => (await api.get<TurnoverCosts>('/turnover/costs')).data

export default api
