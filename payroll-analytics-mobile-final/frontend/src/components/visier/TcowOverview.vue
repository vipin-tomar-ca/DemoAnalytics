<template>
  <div>
    <h2 class="font-semibold mb-2">Total Cost of Workforce (TCOW) — Breakdown</h2>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { PieChart } from 'echarts/charts'
import { TooltipComponent, LegendComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { fetchCostsTcow } from '../../api'
import { useFiltersReload } from '../../composables/useFiltersReload'
import { chartPalette, tooltipStyle } from '../../charts/theme'
import { fallbackTcowBreakdown } from '../../data/fallbacks'

use([PieChart, TooltipComponent, LegendComponent, TitleComponent, CanvasRenderer])

const option = ref<any>({})

async function load() {
  const response = await fetchCostsTcow().catch((err) => {
    console.error('Failed to fetch TCOW breakdown', err)
    return null
  })
  const fallback = fallbackTcowBreakdown()
  const data = response && Array.isArray(response.breakdown) && typeof response.total === 'number'
    ? response
    : fallback
  if (data === fallback) console.warn('TCOW breakdown response invalid, using fallback data')
  option.value = {
    color: chartPalette,
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (params: any) =>
        `<strong>${params.name}</strong><br/>Value: $${params.value.toLocaleString()}<br/>Share: ${params.percent}%`
    },
    legend: {
      orient: 'vertical',
      right: 16,
      top: 'center',
      itemWidth: 10,
      itemHeight: 10,
      textStyle: { color: '#475569', fontSize: 12 }
    },
    series: [{
      type: 'pie',
      radius: ['30%', '68%'],
      center: ['45%', '50%'],
      roseType: false,
      minAngle: 10,
      label: {
        color: '#0f172a',
        formatter: '{b|{b}}\n{c|${c}}',
        rich: {
          b: { fontWeight: 600, fontSize: 12, lineHeight: 16 },
          c: { color: '#334155', fontSize: 12 }
        }
      },
      labelLine: { smooth: true, length: 16 },
      itemStyle: {
        borderColor: '#fff',
        borderWidth: 2,
        shadowBlur: 12,
        shadowColor: 'rgba(15,23,42,0.2)'
      },
      emphasis: {
        scale: true,
        scaleSize: 6,
        itemStyle: { shadowBlur: 22, shadowColor: 'rgba(30,64,175,0.45)' }
      },
      data: data.breakdown
    }],
    title: {
      left: 'center',
      top: 12,
      text: `TCOW: $${data.total.toLocaleString()}`,
      textStyle: { fontSize: 16, fontWeight: 'bold', color: '#0f172a' },
      subtext: 'Click slices to focus',
      subtextStyle: { color: '#64748b', fontSize: 11 }
    },
    animationDuration: 700,
    animationEasing: 'cubicOut'
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
