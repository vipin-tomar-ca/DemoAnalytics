<template>
  <div>
    <h2 class="font-semibold mb-2">Actual vs Budgeted Workforce Costs — Variance</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { BarChart, LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsBudgetVariance } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis } from '../../charts/theme'
import { fallbackBudgetVariance } from '../../data/fallbacks'

use([BarChart, LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchCostsBudgetVariance().catch((err) => {
    console.error('Failed to fetch budget variance data', err)
    return null
  })
  const fallback = fallbackBudgetVariance()
  const data = response && Array.isArray(response.labels) && Array.isArray(response.budget) && Array.isArray(response.actual) && Array.isArray(response.variancePct)
    ? response
    : fallback
  if (data === fallback) console.warn('Budget variance response invalid, using fallback data')
  const currencyFormatter = (val: number) => `$${Math.round(val / 1000)}k`
  option.value = {
    color: chartPalette,
    grid: { left: 60, right: 60, top: 36, bottom: 50 },
    tooltip: {
      ...tooltipStyle,
      formatter: (items: any[]) => {
        if (!items?.length) return ''
        const header = `<strong>${items[0].axisValueLabel}</strong>`
        const lines = items
          .map(item => {
            const val = Number(item.value)
            const formatted = item.seriesName.includes('%')
              ? `${val.toFixed(1)}%`
              : currencyFormatter(val)
            return `${item.marker}${item.seriesName}: ${formatted}`
          })
          .join('<br/>')
        return `${header}<br/>${lines}`
      }
    },
    legend: {
      top: 0,
      itemWidth: 12,
      itemHeight: 12,
      textStyle: { color: '#475569', fontSize: 12 }
    },
    xAxis: categoryAxis({ data: data.labels }),
    yAxis: [
      valueAxis({
        axisLabel: { formatter: currencyFormatter }
      }),
      valueAxis({
        axisLabel: { formatter: (val: number) => `${val.toFixed(1)}%` }
      })
    ],
    series: [
      {
        type: 'bar',
        name: 'Budget',
        barWidth: 18,
        itemStyle: { borderRadius: 8, color: chartPalette[5] },
        data: data.budget
      },
      {
        type: 'bar',
        name: 'Actual',
        barWidth: 18,
        itemStyle: {
          borderRadius: 8,
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: chartPalette[1] },
            { offset: 1, color: chartPalette[0] }
          ])
        },
        data: data.actual
      },
      {
        type: 'line',
        name: 'Variance %',
        smooth: true,
        yAxisIndex: 1,
        symbol: 'circle',
        symbolSize: 8,
        lineStyle: { width: 3 },
        data: data.variancePct,
        markLine: {
          symbol: 'none',
          lineStyle: { color: '#94a3b8', type: 'dashed' },
          label: { formatter: '0% breakeven', color: '#475569', fontSize: 11 },
          data: [{ yAxis: 0 }]
        }
      }
    ],
    animationDuration: 600
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
