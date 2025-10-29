<template>
  <div>
    <h2 class="font-semibold mb-2">Absenteeism — Rate & Cost</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsAbsenteeism } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis, percentFormatter } from '../../charts/theme'
import { fallbackAbsenteeism } from '../../data/fallbacks'

use([LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchCostsAbsenteeism().catch((err) => {
    console.error('Failed to fetch absenteeism data', err)
    return null
  })
  const fallback = fallbackAbsenteeism()
  const data = response && Array.isArray(response.labels) && Array.isArray(response.costs) && Array.isArray(response.ratePct)
    ? response
    : fallback
  if (data === fallback) console.warn('Absenteeism response invalid, using fallback data')
  const avgRate = data.ratePct.reduce((acc, cur) => acc + cur, 0) / data.ratePct.length
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
      top: 4,
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
        name: 'Cost',
        barWidth: 16,
        itemStyle: {
          borderRadius: [8, 8, 0, 0],
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: chartPalette[3] },
            { offset: 1, color: chartPalette[4] }
          ])
        },
        data: data.costs
      },
      {
        type: 'line',
        name: 'Absenteeism %',
        smooth: true,
        yAxisIndex: 1,
        symbol: 'circle',
        symbolSize: 8,
        areaStyle: {
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(124,58,237,0.25)' },
            { offset: 1, color: 'rgba(124,58,237,0.04)' }
          ])
        },
        lineStyle: { width: 3 },
        data: data.ratePct,
        markLine: {
          symbol: 'none',
          lineStyle: { color: '#6366f1', type: 'dotted' },
          label: {
            formatter: `Avg ${avgRate.toFixed(1)}%`,
            color: '#4338ca',
            fontSize: 11,
            padding: [4, 6]
          },
          data: [{ yAxis: Number.isFinite(avgRate) ? Number(avgRate.toFixed(2)) : 0 }]
        }
      }
    ],
    animationDuration: 600
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
