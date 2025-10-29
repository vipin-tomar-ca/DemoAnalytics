<template>
  <div>
    <h2 class="font-semibold mb-2">Overtime Costs (Monthly)</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, DataZoomComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsOvertime } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'

use([LineChart, BarChart, GridComponent, TooltipComponent, DataZoomComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const data = await fetchCostsOvertime()
  option.value = {
    grid: { left: 40, right: 10, top: 20, bottom: 40 },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: data.labels },
    yAxis: { type: 'value' },
    dataZoom: [{ type: 'inside' }],
    series: [
      { type: 'bar', name: 'Overtime Cost', data: data.costs },
      { type: 'line', name: 'Overtime Hours', data: data.hours, smooth: true }
    ]
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
