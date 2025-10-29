<template>
  <div>
    <h2 class="font-semibold mb-2">Impact — Payroll vs Revenue Productivity</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchImpactFinance } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'

use([ScatterChart, GridComponent, TooltipComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const data = await fetchImpactFinance()
  option.value = {
    grid: { left: 45, right: 10, top: 20, bottom: 40 },
    tooltip: { formatter: p => `${p.data.org}<br/>Payroll/Revenue: ${p.data.prRatio}%<br/>Sales/Emp: ${p.data.salesPerEmp}` },
    xAxis: { type: 'value', name: 'Payroll to Revenue %' },
    yAxis: { type: 'value', name: 'Sales per Employee' },
    series: [{
      type: 'scatter',
      data: data.orgs.map(o => ({
        value: [o.prRatio, o.salesPerEmp],
        org: o.org,
        prRatio: o.prRatio,
        salesPerEmp: o.salesPerEmp,
        symbolSize: 10 + (o.productivityIndex || 0)
      }))
    }]
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
