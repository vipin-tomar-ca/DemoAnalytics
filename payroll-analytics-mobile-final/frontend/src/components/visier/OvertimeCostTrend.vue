<template>
  <div>
    <h2 class="font-semibold mb-2">Overtime Costs (Monthly)</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, DataZoomComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsOvertime } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis } from '../../charts/theme'
import { fallbackOvertimeCosts } from '../../data/fallbacks'

use([LineChart, BarChart, GridComponent, TooltipComponent, DataZoomComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchCostsOvertime().catch((err) => {
    console.error('Failed to fetch overtime cost data', err)
    return null
  })
  const fallback = fallbackOvertimeCosts()
  const data = response && Array.isArray(response.labels) && Array.isArray(response.costs) && Array.isArray(response.hours)
    ? response
    : fallback
  if (data === fallback) console.warn('Overtime cost response invalid, using fallback data')
  const highCostThreshold = Math.max(...data.costs) * 0.85
  option.value = {
    color: chartPalette,
    grid: { left: 60, right: 60, top: 36, bottom: 52 },
    tooltip: {
      ...tooltipStyle,
      formatter: (items: any[]) => {
        if (!items?.length) return ''
        const header = `<strong>${items[0].axisValueLabel}</strong>`
        const lines = items
          .map(item => {
            const val = Number(item.value)
            const formatted = item.seriesName.includes('Hours')
              ? `${val.toLocaleString()} hrs`
              : `$${val.toLocaleString()}`
            return `${item.marker}${item.seriesName}: ${formatted}`
          })
          .join('<br/>')
        return `${header}<br/>${lines}`
      }
    },
    xAxis: categoryAxis({ data: data.labels }),
    yAxis: [
      valueAxis({
        axisLabel: { formatter: (val: number) => `$${Math.round(val / 1000)}k` }
      }),
      valueAxis({
        axisLabel: { formatter: (val: number) => `${val.toLocaleString()} hrs` }
      })
    ],
    dataZoom: [
      { type: 'inside' },
      {
        type: 'slider',
        height: 14,
        bottom: 6,
        borderRadius: 8,
        brushSelect: false,
        fillerColor: 'rgba(37,99,235,0.12)',
        handleSize: 0
      }
    ],
    series: [
      {
        type: 'bar',
        name: 'Overtime Cost',
        yAxisIndex: 0,
        barWidth: 16,
        itemStyle: {
          borderRadius: [8, 8, 0, 0],
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: '#fb923c' },
            { offset: 1, color: '#f59e0b' }
          ])
        },
        markArea: {
          itemStyle: { color: 'rgba(245, 158, 11, 0.08)' },
          data: [
            [
              { yAxis: highCostThreshold, name: 'Elevated Cost' },
              { yAxis: Math.max(...data.costs) }
            ]
          ]
        },
        data: data.costs
      },
      {
        type: 'line',
        name: 'Overtime Hours',
        smooth: true,
        showSymbol: false,
        yAxisIndex: 1,
        lineStyle: { width: 3 },
        areaStyle: {
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(14, 165, 233, 0.28)' },
            { offset: 1, color: 'rgba(14, 165, 233, 0.05)' }
          ])
        },
        data: data.hours
      }
    ],
    animationDuration: 600
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
