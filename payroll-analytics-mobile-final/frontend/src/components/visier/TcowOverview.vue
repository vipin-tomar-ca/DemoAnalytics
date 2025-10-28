<template>
  <div>
    <h2 class="font-semibold mb-2">Total Cost of Workforce (TCOW) — Breakdown</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { PieChart } from 'echarts/charts'
import { TooltipComponent, LegendComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import axios from 'axios'

use([PieChart, TooltipComponent, LegendComponent, TitleComponent, CanvasRenderer])

const option = ref<any>({})

async function load() {
  const { data } = await axios.get('/api/costs/tcow')
  option.value = {
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
    legend: { bottom: 0 },
    series: [{
      type: 'pie',
      radius: ['30%','70%'],
      roseType: false,
      label: { show: true, formatter: '{b}\n{d}%'},
      data: data.breakdown
    }],
    title: { left: 'center', text: `TCOW: $${data.total.toLocaleString()}` }
  }
}

onMounted(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
