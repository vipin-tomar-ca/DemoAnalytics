const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

const last12Months = () => {
  const now = new Date()
  return Array.from({ length: 12 }).map((_, idx) => {
    const date = new Date(now.getFullYear(), now.getMonth() - (11 - idx), 1)
    return `${monthNames[date.getMonth()]} ${String(date.getFullYear()).slice(-2)}`
  })
}

export const fallbackHeadcountTrend = () => {
  const labels = last12Months()
  const headcount = labels.map((_, idx) => 1500 + idx * 12 + (idx % 3) * 18)
  const hires = labels.map(() => 25 + Math.floor(Math.random() * 15))
  const exits = labels.map(() => 15 + Math.floor(Math.random() * 10))
  return { labels, headcount, hires, exits }
}

export const fallbackGeoHeadcount = () => ([
  { name: 'Ontario', value: 520 },
  { name: 'Quebec', value: 340 },
  { name: 'British Columbia', value: 280 },
  { name: 'Alberta', value: 240 },
  { name: 'Manitoba', value: 120 },
  { name: 'Saskatchewan', value: 95 },
  { name: 'Nova Scotia', value: 80 },
  { name: 'New Brunswick', value: 70 },
  { name: 'Newfoundland and Labrador', value: 45 },
  { name: 'Prince Edward Island', value: 20 },
  { name: 'Yukon', value: 12 },
  { name: 'Northwest Territories', value: 9 },
  { name: 'Nunavut', value: 6 }
])

export const fallbackTimeHeatmap = () => {
  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
  const hours = Array.from({ length: 24 }).map((_, h) => `${h.toString().padStart(2, '0')}:00`)
  const values = [] as number[][]
  for (let d = 0; d < days.length; d++) {
    for (let h = 0; h < hours.length; h++) {
      const base = d < 5 ? 30 : 12
      const spike = h >= 18 && h <= 21 ? 25 : 0
      const value = Math.max(0, base + spike + Math.floor(Math.random() * 20 - 10))
      values.push([h, d, value])
    }
  }
  return { days, hours, values }
}

export const fallbackCompensation = () => {
  const categories = ['Engineering', 'Sales', 'HR', 'Finance', 'Operations', 'Customer Success']
  const boxData = categories.map((_, idx) => {
    const base = 55_000 + idx * 8_000
    const min = base
    const q1 = base + 6_000
    const median = base + 14_000
    const q3 = base + 22_000
    const max = base + 33_000
    return [min, q1, median, q3, max]
  })
  return { categories, boxData }
}

export const fallbackBudgetVariance = () => {
  const labels = last12Months().map(label => label.split(' ')[0])
  const budget = labels.map(() => 1_500_000 + Math.floor(Math.random() * 80_000 - 40_000))
  const actual = budget.map(b => b + Math.floor(Math.random() * 120_000 - 60_000))
  const variancePct = actual.map((a, idx) => Number((((a - budget[idx]) / budget[idx]) * 100).toFixed(1)))
  return { labels, budget, actual, variancePct }
}

export const fallbackOvertimeCosts = () => {
  const labels = last12Months().map(label => label.split(' ')[0])
  const costs = labels.map(() => 75_000 + Math.floor(Math.random() * 25_000))
  const hours = labels.map(() => 2_000 + Math.floor(Math.random() * 600))
  return { labels, costs, hours }
}

export const fallbackAbsenteeism = () => {
  const labels = last12Months().map(label => label.split(' ')[0])
  const costs = labels.map(() => 50_000 + Math.floor(Math.random() * 18_000))
  const ratePct = labels.map(() => Number((2.5 + Math.random() * 1.8).toFixed(2)))
  return { labels, costs, ratePct }
}

export const fallbackProcessEfficiency = () => ({
  timeToRunPayrollHours: 12.5,
  accuracyRatePct: 98.2
})

export const fallbackCompetitiveness = () => {
  const points = Array.from({ length: 120 }).map(() => {
    const tenure = Number((Math.random() * 18).toFixed(1))
    const compa = Number((0.75 + Math.random() * 0.5).toFixed(2))
    const level = Math.floor(Math.random() * 5) + 1
    return [tenure, compa, level]
  })
  return { points }
}

export const fallbackImpactFinance = () => {
  const orgs = ['Sales', 'Engineering', 'Product', 'Services', 'Finance', 'HR', 'Operations']
  return {
    orgs: orgs.map(org => ({
      org,
      prRatio: Number((28 + Math.random() * 20).toFixed(1)),
      salesPerEmp: 160_000 + Math.floor(Math.random() * 180_000),
      productivityIndex: Math.floor(Math.random() * 5) + 3
    }))
  }
}

export const fallbackTurnoverCosts = () => {
  const labels = last12Months().map(label => label.split(' ')[0])
  const replacementCost = labels.map(() => 180_000 + Math.floor(Math.random() * 50_000))
  const voluntaryPct = labels.map(() => Number((2 + Math.random() * 1.2).toFixed(2)))
  const involuntaryPct = labels.map(() => Number((1.1 + Math.random() * 0.6).toFixed(2)))
  return { labels, replacementCost, voluntaryPct, involuntaryPct }
}

export const fallbackTcowBreakdown = () => ({
  total: 21_450_000,
  breakdown: [
    { name: 'Base Pay', value: 14_200_000 },
    { name: 'Bonuses', value: 1_250_000 },
    { name: 'Benefits', value: 3_100_000 },
    { name: 'Payroll Taxes', value: 2_050_000 },
    { name: 'Training', value: 420_000 },
    { name: 'Travel', value: 310_000 }
  ]
})

export const fallbackPayGap = () => ({
  steps: [
    { label: 'Raw Gap', delta: -12.0 },
    { label: 'Job Mix', delta: 4.1 },
    { label: 'Tenure', delta: 2.7 },
    { label: 'Performance', delta: 1.2 },
    { label: 'Geo/Market', delta: 1.0 },
    { label: 'Residual Gap', delta: -3.0 }
  ]
})
