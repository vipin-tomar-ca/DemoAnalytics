<template>
  <div>
    <h2 class="font-semibold mb-2">Headcount Trend (12 mo)</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { fetchHeadcountTrend } from '../api'
import { use } from 'echarts/core'
import VChart from 'vue-echarts'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent, DataZoomComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

use([LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent, DataZoomComponent, CanvasRenderer])

const option = ref({})

async function load() {
  const data = await fetchHeadcountTrend()
  option.value = {
    grid: { left: 8, right: 8, top: 30, bottom: 40, containLabel: true },
    tooltip: { trigger: 'axis' },
    legend: { top: 0 },
    xAxis: { type: 'category', data: data.labels },
    yAxis: { type: 'value' },
    dataZoom: [{ type: 'inside' }],
    series: [
      { type: 'line', name: 'Headcount', areaStyle: {}, smooth: true, symbol: 'none', data: data.headcount },
      { type: 'bar', name: 'Hires', data: data.hires, stack: 'flow', barWidth: 8 },
      { type: 'bar', name: 'Exits', data: data.exits, stack: 'flow', barWidth: 8 }
    ]
  }
}

onMounted(() => {
  load()
  window.addEventListener('filters.changed', load)
})
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
