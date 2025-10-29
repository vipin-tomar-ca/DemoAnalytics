<template>
  <div>
    <h2 class="font-semibold mb-2">Competitiveness — Compa-Ratio vs Tenure</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, VisualMapComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCompCompetitiveness } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'

use([ScatterChart, GridComponent, TooltipComponent, VisualMapComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const data = await fetchCompCompetitiveness()
  option.value = {
    grid: { left: 45, right: 10, top: 20, bottom: 40 },
    tooltip: { trigger: 'item', formatter: p => `Tenure: ${p.value[0]} yrs<br/>Compa-Ratio: ${p.value[1]}<br/>Level: ${p.value[2]}` },
    xAxis: { type: 'value', name: 'Tenure (yrs)' },
    yAxis: { type: 'value', name: 'Compa-Ratio' },
    visualMap: { min: 1, max: 5, dimension: 2, right: 0 },
    series: [{
      type: 'scatter',
      symbolSize: v => 6 + v[2],
      data: data.points
    }]
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
