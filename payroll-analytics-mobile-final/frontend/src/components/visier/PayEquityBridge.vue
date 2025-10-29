<template>
  <div>
    <h2 class="font-semibold mb-2">Pay Equity — Wage Gap Bridge</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchEquityPayGap } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'

use([BarChart, GridComponent, TooltipComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const data = await fetchEquityPayGap()
  // Waterfall-style using positive/negative bars with a running total
  option.value = {
    grid: { left: 50, right: 10, top: 20, bottom: 40 },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: data.steps.map(s=>s.label) },
    yAxis: { type: 'value', axisLabel: { formatter: '{value}%' } },
    series: [{
      type: 'bar',
      data: data.steps.map(s=>s.delta),
      label: { show: true, position: 'top', formatter: p => `${p.value>0?'+':''}${p.value}%` }
    }]
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
