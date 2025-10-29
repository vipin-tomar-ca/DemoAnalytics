<template>
  <div>
    <h2 class="font-semibold mb-2">Headcount Trend (12 mo)</h2>
    <v-chart autoresize :option="option" class="h-72"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { fetchHeadcountTrend } from '../api'
import { useFiltersReload } from '../composables/useFiltersReload'
import { use, graphic } from 'echarts/core'
import VChart from 'vue-echarts'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent, DataZoomComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis } from '../charts/theme'
import { fallbackHeadcountTrend } from '../data/fallbacks'

use([LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent, DataZoomComponent, CanvasRenderer])

const option = ref({})

async function load() {
  const data = await fetchHeadcountTrend().catch((err) => {
    console.error('Failed to fetch headcount trend', err)
    return fallbackHeadcountTrend()
  })
  if (!data || !Array.isArray(data.labels) || !Array.isArray(data.headcount) || !Array.isArray(data.hires) || !Array.isArray(data.exits)) {
    console.warn('Headcount trend response missing expected arrays, using fallback data')
    Object.assign(data, fallbackHeadcountTrend())
  }
  option.value = {
    color: chartPalette,
    grid: { left: 12, right: 12, top: 36, bottom: 48, containLabel: true },
    tooltip: {
      ...tooltipStyle,
      formatter: (items: any[]) => {
        if (!items?.length) return ''
        const header = `<strong>${items[0].axisValueLabel}</strong>`
        const lines = items
          .map(item => {
            const raw = Number(item.value)
            const formatted = Number.isFinite(raw) ? Math.abs(raw).toLocaleString() : item.value
            return `${item.marker}${item.seriesName}: ${formatted}`
          })
          .join('<br/>')
        return `${header}<br/>${lines}`
      }
    },
    legend: {
      top: 8,
      icon: 'circle',
      itemWidth: 8,
      textStyle: { color: '#475569', fontSize: 11 }
    },
    dataZoom: [
      { type: 'inside' },
      {
        type: 'slider',
        height: 12,
        bottom: 6,
        borderRadius: 8,
        brushSelect: false,
        handleSize: 0,
        showDetail: false,
        fillerColor: 'rgba(37, 99, 235, 0.18)'
      }
    ],
    xAxis: categoryAxis({ data: data.labels }),
    yAxis: valueAxis({
      min: (extent: { min: number }) => (extent.min < 0 ? extent.min * 1.1 : 0),
      max: (extent: { max: number }) => extent.max * 1.1
    }),
    series: [
      {
        type: 'line',
        name: 'Headcount',
        smooth: true,
        showSymbol: false,
        lineStyle: { width: 3 },
        areaStyle: {
          color: new graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(37, 99, 235, 0.35)' },
            { offset: 1, color: 'rgba(37, 99, 235, 0.02)' }
          ])
        },
        data: data.headcount,
        emphasis: { focus: 'series', lineStyle: { width: 4 } }
      },
      {
        type: 'bar',
        name: 'Hires',
        stack: 'flow',
        barWidth: 12,
        itemStyle: { borderRadius: [8, 8, 0, 0] },
        emphasis: { focus: 'series' },
        data: data.hires
      },
      {
        type: 'bar',
        name: 'Exits',
        stack: 'flow',
        barWidth: 12,
        itemStyle: { borderRadius: [8, 8, 0, 0] },
        emphasis: { focus: 'series' },
        data: data.exits.map(v => -v)
      }
    ],
    animationDuration: 600
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
