<template>
  <div>
    <h2 class="font-semibold mb-2">Impact — Payroll vs Revenue Productivity</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use, graphic } from 'echarts/core'
import { ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchImpactFinance } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle, valueAxis } from '../../charts/theme'
import { fallbackImpactFinance } from '../../data/fallbacks'

use([ScatterChart, GridComponent, TooltipComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchImpactFinance().catch((err) => {
    console.error('Failed to fetch impact finance data', err)
    return null
  })
  const fallback = fallbackImpactFinance()
  const data = response && Array.isArray(response.orgs) ? response : fallback
  if (data === fallback) console.warn('Impact finance response invalid, using fallback data')
  const values = data.orgs.map(o => ({
    value: [o.prRatio, o.salesPerEmp],
    org: o.org,
    prRatio: o.prRatio,
    salesPerEmp: o.salesPerEmp,
    productivityIndex: o.productivityIndex
  }))
  const medianPayroll = values.map(v => v.prRatio).sort((a, b) => a - b)[Math.floor(values.length / 2)]
  const medianSales = values.map(v => v.salesPerEmp).sort((a, b) => a - b)[Math.floor(values.length / 2)]
  option.value = {
    color: chartPalette,
    grid: { left: 70, right: 60, top: 36, bottom: 60 },
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (p: any) =>
        `<strong>${p.data.org}</strong><br/>Payroll / Revenue: ${p.data.prRatio}%<br/>Sales per Employee: $${p.data.salesPerEmp.toLocaleString()}<br/>Productivity Index: ${p.data.productivityIndex}`
    },
    xAxis: valueAxis({
      name: 'Payroll to Revenue %',
      nameLocation: 'middle',
      nameGap: 36,
      min: 20,
      max: 60
    }),
    yAxis: valueAxis({
      name: 'Sales per Employee',
      nameLocation: 'middle',
      nameGap: 60,
      axisLabel: { formatter: (val: number) => `$${Math.round(val / 1000)}k` },
      min: 100000,
      max: 400000
    }),
    series: [{
      type: 'scatter',
      symbol: 'circle',
      symbolSize: (item: any) => 10 + (item.productivityIndex || 0) * 2,
      data: values,
      itemStyle: {
        shadowBlur: 16,
        shadowColor: 'rgba(14,165,233,0.25)',
        opacity: 0.95
      },
      emphasis: {
        scale: true,
        focus: 'series'
      },
      markLine: {
        symbol: 'none',
        lineStyle: { color: '#94a3b8', type: 'dashed' },
        label: {
          formatter: (params: any) =>
            params.name === 'x'
              ? `Median Payroll ${medianPayroll.toFixed(1)}%`
              : `Median Sales $${medianSales.toLocaleString()}`,
          color: '#475569',
          fontSize: 11,
          padding: [4, 6]
        },
        data: [
          { name: 'x', xAxis: medianPayroll },
          { name: 'y', yAxis: medianSales }
        ]
      },
      markArea: {
        itemStyle: { color: 'rgba(34,197,94,0.05)' },
        data: [
          [
            { xAxis: 25, yAxis: medianSales },
            { xAxis: medianPayroll, yAxis: 380000 }
          ]
        ]
      }
    }],
    animationDuration: 600,
    backgroundColor: new graphic.LinearGradient(0, 0, 1, 1, [
      { offset: 0, color: 'rgba(248,250,252,0.94)' },
      { offset: 1, color: 'rgba(224,231,255,0.7)' }
    ])
  }
}
useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
