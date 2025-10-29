<template>
  <div>
    <h2 class="font-semibold mb-2">Time Management — Overtime Heatmap</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { fetchTimeHeatmap } from '../api'
import { useFiltersReload } from '../composables/useFiltersReload'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { HeatmapChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, VisualMapComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { chartPalette, tooltipStyle, axisLabelColor } from '../charts/theme'
import { fallbackTimeHeatmap } from '../data/fallbacks'

use([HeatmapChart, GridComponent, TooltipComponent, VisualMapComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchTimeHeatmap().catch((err) => {
    console.error('Failed to fetch time heatmap data', err)
    return null
  })
  const fallback = fallbackTimeHeatmap()
  const data = response && Array.isArray(response.days) && Array.isArray(response.hours) && Array.isArray(response.values)
    ? response
    : fallback
  if (data === fallback) console.warn('Time heatmap response invalid, using fallback data')
  const { days, hours, values } = data
  option.value = {
    grid: { left: 60, right: 16, top: 24, bottom: 60 },
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (params: any) => {
        const [hourIndex, dayIndex, value] = params.data
        return `<strong>${days[dayIndex]}</strong><br/>${hours[hourIndex]} · ${value} overtime hrs`
      }
    },
    xAxis: {
      type: 'category',
      data: hours,
      axisLine: { show: false },
      axisLabel: { color: axisLabelColor, interval: 3 },
      splitArea: {
        show: true,
        areaStyle: { color: ['rgba(248, 250, 252, 0.8)', '#ffffff'] }
      }
    },
    yAxis: {
      type: 'category',
      data: days,
      axisLine: { show: false },
      axisLabel: { color: axisLabelColor },
      splitArea: { show: true, areaStyle: { color: ['#ffffff'] } }
    },
    visualMap: {
      min: 0,
      max: 100,
      calculable: true,
      orient: 'horizontal',
      left: 'center',
      bottom: 10,
      itemWidth: 14,
      itemHeight: 80,
      textStyle: { color: axisLabelColor },
      inRange: { color: ['#eff6ff', '#bfdbfe', chartPalette[0]] }
    },
    series: [
      {
        type: 'heatmap',
        progressive: 400,
        data: values,
        label: { show: false },
        emphasis: {
          itemStyle: {
            borderColor: chartPalette[1],
            borderWidth: 2,
            shadowBlur: 12,
            shadowColor: 'rgba(79, 70, 229, 0.35)'
          }
        }
      }
    ],
    animationDuration: 500,
    backgroundColor: new graphic.LinearGradient(0, 0, 1, 1, [
      { offset: 0, color: 'rgba(248,250,252,0.6)' },
      { offset: 1, color: 'rgba(233, 238, 255, 0.6)' }
    ])
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
