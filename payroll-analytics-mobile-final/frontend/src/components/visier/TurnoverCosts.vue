<template>
  <div>
    <h2 class="font-semibold mb-2">Turnover — Rates & Replacement Costs</h2>
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
import { fetchTurnoverCosts } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis, percentFormatter } from '../../charts/theme'
import { fallbackTurnoverCosts } from '../../data/fallbacks'

use([BarChart, LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchTurnoverCosts().catch((err) => {
    console.error('Failed to fetch turnover cost data', err)
    return null
  })
  const fallback = fallbackTurnoverCosts()
  const data = response && Array.isArray(response.labels) && Array.isArray(response.replacementCost) && Array.isArray(response.voluntaryPct) && Array.isArray(response.involuntaryPct)
    ? response
    : fallback
  if (data === fallback) console.warn('Turnover cost response invalid, using fallback data')
  option.value = {
    color: chartPalette,
    grid: { left: 60, right: 60, top: 32, bottom: 48 },
    tooltip: {
      ...tooltipStyle,
      formatter: (items: any[]) => {
        if (!items?.length) return ''
        const header = `<strong>${items[0].axisValueLabel}</strong>`
        const lines = items
          .map(item => {
            const val = Number(item.value)
            const formatted = item.seriesName.includes('%')
              ? percentFormatter(val)
              : `$${val.toLocaleString()}`
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
      valueAxis({ axisLabel: { formatter: (val: number) => `$${Math.round(val / 1000)}k` } }),
      valueAxis({
        axisLabel: { formatter: (val: number) => `${val.toFixed(1)}%` },
        min: 0
      })
    ],
    series: [
      {
        type: 'bar',
        name: 'Replacement Cost',
        barWidth: 18,
        itemStyle: {
          borderRadius: [8, 8, 0, 0],
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: chartPalette[2] },
            { offset: 1, color: chartPalette[0] }
          ])
        },
        data: data.replacementCost
      },
      {
        type: 'line',
        name: 'Voluntary %',
        smooth: true,
        yAxisIndex: 1,
        showSymbol: false,
        lineStyle: { width: 3 },
        areaStyle: {
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(34,197,94,0.25)' },
            { offset: 1, color: 'rgba(34,197,94,0.05)' }
          ])
        },
        data: data.voluntaryPct
      },
      {
        type: 'line',
        name: 'Involuntary %',
        smooth: true,
        yAxisIndex: 1,
        showSymbol: false,
        lineStyle: { width: 3, type: 'dashed' },
        areaStyle: {
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(239,68,68,0.22)' },
            { offset: 1, color: 'rgba(239,68,68,0.04)' }
          ])
        },
        data: data.involuntaryPct
      }
    ],
    animationDuration: 600
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
