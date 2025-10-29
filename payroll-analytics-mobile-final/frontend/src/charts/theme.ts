export const chartPalette = ['#2563eb', '#7c3aed', '#0ea5e9', '#22c55e', '#f97316', '#facc15']
export const secondaryPalette = ['#94a3b8', '#cbd5f5', '#f1f5f9']

export const tooltipStyle = {
  trigger: 'axis' as const,
  appendToBody: true,
  backgroundColor: 'rgba(15, 23, 42, 0.92)',
  borderColor: 'rgba(15, 23, 42, 0.92)',
  borderRadius: 10,
  padding: 14,
  textStyle: { color: '#f8fafc', fontSize: 12 },
  axisPointer: {
    type: 'shadow' as const
  },
  extraCssText: 'box-shadow: 0 20px 45px rgba(15,23,42,0.45);'
}

export const axisLabelColor = '#64748b'
export const axisLineColor = '#d4d4d8'
export const splitLineColor = '#e4e4e7'

export function categoryAxis(overrides: Record<string, unknown> = {}) {
  return {
    type: 'category',
    boundaryGap: true,
    axisLine: { lineStyle: { color: axisLineColor } },
    axisLabel: { color: axisLabelColor, fontSize: 11, margin: 12 },
    axisTick: { show: false },
    axisPointer: {
      lineStyle: { color: chartPalette[0], width: 2 },
      label: {
        backgroundColor: '#f8fafc',
        borderRadius: 6,
        padding: [3, 8],
        shadowBlur: 6,
        shadowColor: 'rgba(15,23,42,0.25)',
        color: '#0f172a'
      }
    },
    ...overrides
  }
}

export function valueAxis(overrides: Record<string, unknown> = {}) {
  return {
    type: 'value',
    axisLine: { show: false },
    axisLabel: { color: axisLabelColor, fontSize: 11, margin: 12 },
    splitLine: { show: true, lineStyle: { color: splitLineColor } },
    axisTick: { show: false },
    ...overrides
  }
}

export function percentFormatter(value: number) {
  return `${Number.isFinite(value) ? value.toFixed(1) : 0}%`
}
