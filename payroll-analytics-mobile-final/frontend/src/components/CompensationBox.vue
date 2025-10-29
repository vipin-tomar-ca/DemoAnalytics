<template>
  <div>
    <h2 class="font-semibold mb-2">Compensation — Distribution by Job Family</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { fetchCompensation } from '../api'
import { useFiltersReload } from '../composables/useFiltersReload'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BoxplotChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, DatasetComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

use([BoxplotChart, GridComponent, TooltipComponent, DatasetComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const { categories, boxData } = await fetchCompensation()
  option.value = {
    grid: { left: 50, right: 10, top: 20, bottom: 40 },
    tooltip: { trigger: 'item' },
    xAxis: { type: 'category', data: categories, axisLabel: { rotate: 30 } },
    yAxis: { type: 'value' },
    series: [{ type: 'boxplot', data: boxData }]
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
