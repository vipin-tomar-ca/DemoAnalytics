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
import { use } from 'echarts/core'
import { HeatmapChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, VisualMapComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

use([HeatmapChart, GridComponent, TooltipComponent, VisualMapComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const { days, hours, values } = await fetchTimeHeatmap()
  option.value = {
    tooltip: { position: 'top' },
    grid: { left: 40, right: 10, top: 20, bottom: 20 },
    xAxis: { type: 'category', data: hours, splitArea: { show: true } },
    yAxis: { type: 'category', data: days, splitArea: { show: true } },
    visualMap: { min: 0, max: 100, calculable: true, orient: 'horizontal', left: 'center', bottom: 0 },
    series: [{ type: 'heatmap', data: values, emphasis: { itemStyle: { shadowBlur: 5 } } }]
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
