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
import { use, graphic } from 'echarts/core'
import { BoxplotChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, DatasetComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { chartPalette, tooltipStyle, categoryAxis, valueAxis } from '../charts/theme'
import { fallbackCompensation } from '../data/fallbacks'

use([BoxplotChart, GridComponent, TooltipComponent, DatasetComponent, CanvasRenderer])
const option = ref<any>({})

async function load() {
  const response = await fetchCompensation().catch((err) => {
    console.error('Failed to fetch compensation summary', err)
    return null
  })
  const fallback = fallbackCompensation()
  const data = response && Array.isArray(response.categories) && Array.isArray(response.boxData)
    ? response
    : fallback
  if (data === fallback) console.warn('Compensation summary response invalid, using fallback data')
  const { categories, boxData } = data
  const medians = boxData.map(b => b[2]).filter(Number.isFinite)
  const overallMedian = medians.length ? medians.sort((a, b) => a - b)[Math.floor(medians.length / 2)] : 0
  const hasMedian = overallMedian > 0
  option.value = {
    color: chartPalette,
    grid: { left: 60, right: 24, top: 28, bottom: 60 },
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (param: any) => {
        if (!param?.data) return ''
        const [min, q1, median, q3, max] = param.data
        return `
          <div>
            <strong>${categories[param.dataIndex]}</strong><br/>
            Min: $${min.toLocaleString()}<br/>
            Q1: $${q1.toLocaleString()}<br/>
            Median: $${median.toLocaleString()}<br/>
            Q3: $${q3.toLocaleString()}<br/>
            Max: $${max.toLocaleString()}
          </div>
        `
      }
    },
    xAxis: categoryAxis({
      data: categories,
      axisLabel: { rotate: 20, interval: 0, color: '#475569' }
    }),
    yAxis: valueAxis({
      axisLabel: {
        formatter: (val: number) => `$${Math.round(val / 1000)}k`
      }
    }),
    series: [
      {
        type: 'boxplot',
        data: boxData,
        itemStyle: {
          color: new graphic.LinearGradient(0, 0, 1, 1, [
            { offset: 0, color: 'rgba(59,130,246,0.35)' },
            { offset: 1, color: 'rgba(79,70,229,0.15)' }
          ]),
          borderColor: chartPalette[0],
          borderWidth: 1.5
        },
        emphasis: {
          itemStyle: {
            shadowBlur: 12,
            shadowColor: 'rgba(59,130,246,0.25)'
          }
        },
        markLine: hasMedian
          ? {
              symbol: 'none',
              lineStyle: { color: '#f97316', width: 1.5, type: 'dashed' },
              label: {
                formatter: `Overall median $${overallMedian.toLocaleString()}`,
                color: '#f97316',
                fontSize: 11,
                padding: [4, 6]
              },
              data: [{ yAxis: overallMedian }]
            }
          : undefined
      }
    ],
    animationDuration: 600
  }
}

useFiltersReload(load)
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
