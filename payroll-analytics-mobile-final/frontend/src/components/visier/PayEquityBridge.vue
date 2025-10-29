<template>
  <div>
    <h2 class="font-semibold mb-2">Pay Equity — Wage Gap Bridge</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchEquityPayGap } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis } from '../../charts/theme'
import { fallbackPayGap } from '../../data/fallbacks'

use([BarChart, GridComponent, TooltipComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchEquityPayGap().catch((err) => {
    console.error('Failed to fetch pay equity bridge data', err)
    return null
  })
  const fallback = fallbackPayGap()
  const data = response && Array.isArray(response.steps) ? response : fallback
  if (data === fallback) console.warn('Pay equity bridge response invalid, using fallback data')
  let running = 0
  const helper: number[] = []
  const deltas: number[] = []
  const signed: number[] = []
  data.steps.forEach(step => {
    helper.push(step.delta >= 0 ? running : running + step.delta)
    deltas.push(Math.abs(step.delta))
    signed.push(step.delta)
    running += step.delta
  })
  option.value = {
    color: chartPalette,
    grid: { left: 60, right: 24, top: 36, bottom: 48 },
    tooltip: {
      ...tooltipStyle,
      trigger: 'axis',
      formatter: (items: any[]) => {
        if (!items?.length) return ''
        const header = `<strong>${items[0].axisValueLabel}</strong>`
        const lines = items
          .filter(item => item.seriesName === 'Bridge')
          .map(item => {
            const original = signed[item.dataIndex]
            return `${item.marker}${item.seriesName}: ${original >= 0 ? '+' : ''}${original.toFixed(1)}%`
          })
          .join('<br/>')
        return `${header}<br/>${lines}`
      }
    },
    xAxis: categoryAxis({ data: data.steps.map(s => s.label) }),
    yAxis: valueAxis({
      axisLabel: { formatter: (val: number) => `${val.toFixed(1)}%` }
    }),
    series: [
      {
        name: 'Baseline',
        type: 'bar',
        stack: 'total',
        itemStyle: { color: 'transparent' },
        emphasis: { disabled: true },
        data: helper
      },
      {
        name: 'Bridge',
        type: 'bar',
        stack: 'total',
        barWidth: 24,
        label: {
          show: true,
          position: 'top',
          formatter: (p: any) => {
            const original = signed[p.dataIndex]
            return `${original >= 0 ? '+' : ''}${original.toFixed(1)}%`
          },
          color: '#0f172a',
          fontSize: 12
        },
        itemStyle: {
          color: (params: any) => (signed[params.dataIndex] >= 0 ? chartPalette[3] : chartPalette[4]),
          borderRadius: [6, 6, 0, 0],
          shadowBlur: 10,
          shadowColor: 'rgba(15,23,42,0.15)'
        },
        data: deltas,
        emphasis: {
          itemStyle: {
            color: (params: any) => (signed[params.dataIndex] >= 0 ? '#16a34a' : '#dc2626')
          }
        },
        markLine: {
          symbol: 'none',
          data: [{ xAxis: data.steps.length - 1 }],
          label: { show: false }
        }
      }
    ],
    animationDuration: 600,
    animationEasing: 'cubicOut'
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
