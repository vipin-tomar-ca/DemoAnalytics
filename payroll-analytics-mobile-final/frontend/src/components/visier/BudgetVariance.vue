<template>
  <div>
    <h2 class="font-semibold mb-2">Actual vs Budgeted Workforce Costs — Variance</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BarChart, LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import axios from 'axios'

use([BarChart, LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const { data } = await axios.get('/api/costs/budget-variance')
  option.value = {
    grid: { left: 40, right: 10, top: 30, bottom: 30 },
    tooltip: { trigger: 'axis' },
    legend: { top: 0 },
    xAxis: { type: 'category', data: data.labels },
    yAxis: { type: 'value' },
    series: [
      { type: 'bar', name: 'Budget', data: data.budget },
      { type: 'bar', name: 'Actual', data: data.actual },
      { type: 'line', name: 'Variance %', data: data.variancePct, yAxisIndex: 0, smooth: true }
    ]
  }
}
onMounted(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
