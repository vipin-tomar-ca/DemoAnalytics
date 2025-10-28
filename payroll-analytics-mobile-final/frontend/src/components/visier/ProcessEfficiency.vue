<template>
  <div>
    <h2 class="font-semibold mb-2">Payroll Process Efficiency</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { GaugeChart } from 'echarts/charts'
import { TooltipComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import axios from 'axios'

use([GaugeChart, TooltipComponent, TitleComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const { data } = await axios.get('/api/process/efficiency')
  option.value = {
    series: [
      {
        type: 'gauge',
        title: { show: true, offsetCenter: [0, '80%'] },
        detail: { formatter: '{value} h', fontSize: 14 },
        min: 0, max: 48,
        data: [{ value: data.timeToRunPayrollHours, name: 'Time to Run Payroll' }]
      },
      {
        type: 'gauge',
        center: ['75%', '50%'],
        radius: '45%',
        min: 0, max: 100,
        detail: { formatter: '{value}%', fontSize: 14 },
        data: [{ value: data.accuracyRatePct, name: 'Accuracy' }]
      }
    ],
    tooltip: { show: true }
  }
}
onMounted(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
