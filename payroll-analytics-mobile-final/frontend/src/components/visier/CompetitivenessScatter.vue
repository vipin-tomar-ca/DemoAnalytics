<template>
  <div>
    <h2 class="font-semibold mb-2">Competitiveness — Compa-Ratio vs Tenure</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, VisualMapComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCompCompetitiveness } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, valueAxis } from '../../charts/theme'
import { fallbackCompetitiveness } from '../../data/fallbacks'

use([ScatterChart, GridComponent, TooltipComponent, VisualMapComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchCompCompetitiveness().catch((err) => {
    console.error('Failed to fetch competitiveness data', err)
    return null
  })
  const fallback = fallbackCompetitiveness()
  const data = response && Array.isArray(response.points) ? response : fallback
  if (data === fallback) console.warn('Competitiveness response invalid, using fallback data')
  const targetZone = [
    [{ xAxis: 2, yAxis: 0.95 }, { xAxis: 8, yAxis: 1.1 }]
  ]
  option.value = {
    color: chartPalette,
    grid: { left: 70, right: 60, top: 36, bottom: 50 },
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (p: any) =>
        `Tenure: <strong>${p.value[0]} yrs</strong><br/>Compa-ratio: <strong>${p.value[1]}</strong><br/>Level: ${p.value[2]}`
    },
    xAxis: valueAxis({
      name: 'Tenure (yrs)',
      nameLocation: 'middle',
      nameGap: 32,
      min: 0,
      max: 20
    }),
    yAxis: valueAxis({
      name: 'Compa-Ratio',
      nameLocation: 'middle',
      nameGap: 45,
      min: 0.6,
      max: 1.4
    }),
    visualMap: {
      min: 1,
      max: 5,
      dimension: 2,
      right: 0,
      top: 24,
      text: ['Level ↑', 'Level ↓'],
      itemWidth: 14,
      itemHeight: 120,
      textStyle: { color: '#475569' },
      inRange: { color: ['#bae6fd', '#2563eb', '#7c3aed'] }
    },
    series: [{
      type: 'scatter',
      symbolSize: (v: number[]) => 8 + v[2] * 2,
      data: data.points,
      itemStyle: {
        shadowBlur: 14,
        shadowColor: 'rgba(37,99,235,0.25)',
        opacity: 0.9
      },
      emphasis: {
        scale: true,
        focus: 'series'
      },
      markLine: {
        symbol: 'none',
        lineStyle: { color: '#f97316', type: 'dashed' },
        label: { formatter: 'Market parity', color: '#f97316', fontSize: 11 },
        data: [{ yAxis: 1 }]
      },
      markArea: {
        itemStyle: { color: 'rgba(34,197,94,0.08)' },
        data: targetZone
      }
    }],
    animationDuration: 600,
    backgroundColor: new graphic.LinearGradient(0, 0, 1, 1, [
      { offset: 0, color: 'rgba(248,250,252,0.95)' },
      { offset: 1, color: 'rgba(226,232,240,0.7)' }
    ])
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
