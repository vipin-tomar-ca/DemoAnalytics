<template>
  <div>
    <h2 class="font-semibold mb-2">Payroll Process Efficiency</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { GaugeChart } from 'echarts/charts'
import { TooltipComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchProcessEfficiency } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle } from '../../charts/theme'
import { fallbackProcessEfficiency } from '../../data/fallbacks'

use([GaugeChart, TooltipComponent, TitleComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchProcessEfficiency().catch((err) => {
    console.error('Failed to fetch process efficiency metrics', err)
    return null
  })
  const fallback = fallbackProcessEfficiency()
  const data = response && typeof response.timeToRunPayrollHours === 'number' && typeof response.accuracyRatePct === 'number'
    ? response
    : fallback
  if (data === fallback) console.warn('Process efficiency response invalid, using fallback data')
  option.value = {
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (params: any) => `${params.seriesName}: ${params.value}${params.seriesName === 'Accuracy' ? '%' : ' h'}`
    },
    series: [
      {
        type: 'gauge',
        name: 'Cycle Time',
        center: ['35%', '55%'],
        min: 0,
        max: 48,
        radius: '70%',
        pointer: { width: 6, itemStyle: { color: chartPalette[0] } },
        axisLine: {
          lineStyle: {
            width: 12,
            color: [
              [0.4, '#22c55e'],
              [0.75, '#f59e0b'],
              [1, '#ef4444']
            ]
          }
        },
        progress: { show: true, width: 12, itemStyle: { color: chartPalette[2] } },
        splitLine: { length: 10, distance: -2, lineStyle: { color: '#cbd5f5' } },
        axisTick: { show: false },
        axisLabel: { distance: 18, color: '#475569', fontSize: 11 },
        title: { offsetCenter: [0, '68%'], color: '#64748b', fontSize: 12 },
        detail: {
          formatter: '{value} h',
          color: '#0f172a',
          fontWeight: 600,
          fontSize: 18,
          offsetCenter: [0, '40%']
        },
        itemStyle: { color: chartPalette[0] },
        data: [{ value: Number(data.timeToRunPayrollHours.toFixed(1)), name: 'Time to Run Payroll' }]
      },
      {
        type: 'gauge',
        name: 'Accuracy',
        center: ['78%', '55%'],
        radius: '48%',
        min: 0,
        max: 100,
        startAngle: 210,
        endAngle: -30,
        pointer: { length: '55%', width: 4, itemStyle: { color: chartPalette[3] } },
        axisLine: {
          lineStyle: {
            width: 10,
            color: [
              [0.75, '#ef4444'],
              [0.9, '#f59e0b'],
              [1, '#22c55e']
            ]
          }
        },
        progress: { show: true, width: 10, itemStyle: { color: chartPalette[3] } },
        splitLine: { length: 6, lineStyle: { color: '#cbd5f5' } },
        axisTick: { show: false },
        axisLabel: { color: '#475569', fontSize: 10, distance: 12 },
        title: { offsetCenter: [0, '70%'], color: '#64748b', fontSize: 12 },
        detail: {
          formatter: '{value}%',
          fontSize: 16,
          fontWeight: 600,
          color: '#0f172a'
        },
        data: [{ value: Number(data.accuracyRatePct.toFixed(1)), name: 'Accuracy' }]
      }
    ],
    animationDuration: 600,
    animationEasing: 'cubicOut',
    backgroundColor: new graphic.LinearGradient(0, 1, 1, 0, [
      { offset: 0, color: 'rgba(248,250,252,0.9)' },
      { offset: 1, color: 'rgba(226,232,240,0.6)' }
    ])
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
