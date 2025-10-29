<template>
  <div>
    <h2 class="font-semibold mb-2">Absenteeism — Rate & Cost</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsAbsenteeism } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'

use([LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const data = await fetchCostsAbsenteeism()
  option.value = {
    grid: { left: 40, right: 10, top: 20, bottom: 30 },
    tooltip: { trigger: 'axis' },
    legend: { top: 0 },
    xAxis: { type: 'category', data: data.labels },
    yAxis: [{ type: 'value' }],
    series: [
      { type: 'bar', name: 'Cost', data: data.costs },
      { type: 'line', name: 'Absenteeism %', data: data.ratePct, smooth: true }
    ]
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
