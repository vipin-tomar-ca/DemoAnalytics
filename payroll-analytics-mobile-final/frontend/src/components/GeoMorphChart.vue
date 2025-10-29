<template>
  <div>
    <div class="flex items-center justify-between mb-2">
      <h2 class="font-semibold">Headcount by Province — Map ⇄ Bar (morph)</h2>
      <button class="text-xs px-2 py-1 rounded bg-neutral-100" @click="toggle()">Toggle View</button>
    </div>
    <v-chart autoresize :option="option" class="h-80"/>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { fetchGeoHeadcount } from '../api'
import { useFiltersReload } from '../composables/useFiltersReload'
import VChart from 'vue-echarts'
import { use, graphic, registerMap } from 'echarts/core'
import { MapChart, BarChart } from 'echarts/charts'
import { GeoComponent, TooltipComponent, VisualMapComponent, GridComponent, DatasetComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { chartPalette, tooltipStyle, axisLabelColor } from '../charts/theme'
import { fallbackGeoHeadcount } from '../data/fallbacks'

use([MapChart, BarChart, GeoComponent, TooltipComponent, VisualMapComponent, GridComponent, DatasetComponent, CanvasRenderer])

const current = ref<'map'|'bar'>('map')
const option = ref<any>({})

// Demo simplified GeoJSON for Canada (rect approximations)
const canadaGeoJson = {
  "type": "FeatureCollection",
  "features": [
    {"type":"Feature","id":"ON","properties":{"name":"Ontario"},"geometry":{"type":"Polygon","coordinates":[[[-95,49],[-95,56],[-74,56],[-74,42],[-95,42],[-95,49]]]}},
    {"type":"Feature","id":"QC","properties":{"name":"Quebec"},"geometry":{"type":"Polygon","coordinates":[[[-79,62],[-57,62],[-57,45],[-79,45],[-79,62]]]}},
    {"type":"Feature","id":"BC","properties":{"name":"British Columbia"},"geometry":{"type":"Polygon","coordinates":[[[-139,60],[-114,60],[-114,49],[-139,49],[-139,60]]]}},
    {"type":"Feature","id":"AB","properties":{"name":"Alberta"},"geometry":{"type":"Polygon","coordinates":[[[-120,60],[-110,60],[-110,49],[-120,49],[-120,60]]]}},
    {"type":"Feature","id":"MB","properties":{"name":"Manitoba"},"geometry":{"type":"Polygon","coordinates":[[[-102,60],[-95,60],[-95,49],[-102,49],[-102,60]]]}},
    {"type":"Feature","id":"SK","properties":{"name":"Saskatchewan"},"geometry":{"type":"Polygon","coordinates":[[[-110,60],[-102,60],[-102,49],[-110,49],[-110,60]]]}},
    {"type":"Feature","id":"NS","properties":{"name":"Nova Scotia"},"geometry":{"type":"Polygon","coordinates":[[[-66,47],[-60,47],[-60,43],[-66,43],[-66,47]]]}},
    {"type":"Feature","id":"NB","properties":{"name":"New Brunswick"},"geometry":{"type":"Polygon","coordinates":[[[-69,48],[-64,48],[-64,45],[-69,45],[-69,48]]]}},
    {"type":"Feature","id":"NL","properties":{"name":"Newfoundland and Labrador"},"geometry":{"type":"Polygon","coordinates":[[[-61,60],[-52,60],[-52,46],[-61,46],[-61,60]]]}},
    {"type":"Feature","id":"PE","properties":{"name":"Prince Edward Island"},"geometry":{"type":"Polygon","coordinates":[[[-64.5,47.5],[-62.9,47.5],[-62.9,46],[-64.5,46],[-64.5,47.5]]]}},
    {"type":"Feature","id":"YT","properties":{"name":"Yukon"},"geometry":{"type":"Polygon","coordinates":[[[-141,69],[-123,69],[-123,60],[-141,60],[-141,69]]]}},
    {"type":"Feature","id":"NT","properties":{"name":"Northwest Territories"},"geometry":{"type":"Polygon","coordinates":[[[-136,70],[-102,70],[-102,60],[-136,60],[-136,70]]]}},
    {"type":"Feature","id":"NU","properties":{"name":"Nunavut"},"geometry":{"type":"Polygon","coordinates":[[[-110,84],[-61,84],[-61,60],[-110,60],[-110,84]]]}}
  ]
}

async function load() {
  registerMap('CANADA_SIMPLIFIED', canadaGeoJson as any)

  const response = await fetchGeoHeadcount().catch((err) => {
    console.error('Failed to fetch geo headcount data', err)
    return null
  })
  const fallback = fallbackGeoHeadcount()
  const rows = Array.isArray(response) && response.length ? response : fallback
  if (rows === fallback) console.warn('Geo headcount response invalid, using fallback data')
  const dataset = [{ id: 'geoHeadcount', source: rows }]
  const maxValue = rows.length > 0 ? Math.max(...rows.map((r: any) => r.value)) : 1

  const makeMap = () => ({
    dataset,
    tooltip: {
      ...tooltipStyle,
      trigger: 'item',
      axisPointer: undefined,
      formatter: (params: any) => `<strong>${params.name}</strong><br/>Headcount: ${params.value?.toLocaleString?.() ?? params.value}`
    },
    visualMap: {
      left: 0,
      min: 0,
      max: maxValue,
      calculable: true,
      textStyle: { color: axisLabelColor },
      inRange: { color: ['#e0f2fe', '#2563eb'] },
      itemWidth: 18,
      itemHeight: 180
    },
    series: [{
      name: 'Headcount',
      type: 'map',
      map: 'CANADA_SIMPLIFIED',
      nameProperty: 'name',
      universalTransition: true,
      roam: true,
      zoom: 0.95,
      itemStyle: {
        borderColor: '#ffffff',
        borderWidth: 1.2,
        shadowBlur: 10,
        shadowColor: 'rgba(15,23,42,0.1)'
      },
      emphasis: {
        itemStyle: { shadowBlur: 20, shadowColor: 'rgba(37,99,235,0.35)' },
        label: { show: true, color: '#0f172a', fontWeight: 'bold' }
      },
      encode: { itemName: 'name', value: 'value' }
    }]
  })

  const makeBar = () => {
    const barRows = [...rows].sort((a, b) => a.value - b.value)
    const categories = barRows.map(r => r.name)
    const values = barRows.map(r => r.value)
    return {
      grid: { left: 120, right: 32, top: 32, bottom: 20 },
      tooltip: {
        ...tooltipStyle,
        trigger: 'item',
        axisPointer: undefined,
        formatter: (params: any) => {
          if (!params) return ''
          const row = barRows[params.dataIndex]
          return `<strong>${row.name}</strong><br/>Headcount: ${row.value.toLocaleString()}`
        }
      },
      xAxis: {
        type: 'value',
        axisLabel: { color: axisLabelColor, formatter: (val: number) => val.toLocaleString() },
        splitLine: { lineStyle: { color: '#e2e8f0' } }
      },
      yAxis: {
        type: 'category',
        inverse: true,
        data: categories,
        axisLabel: { color: '#334155', fontSize: 12 },
        axisLine: { lineStyle: { color: '#e2e8f0' } }
      },
      series: [{
        type: 'bar',
        data: values,
        universalTransition: true,
        itemStyle: {
          borderRadius: [0, 10, 10, 0],
          color: new graphic.LinearGradient(0, 0, 1, 0, [
            { offset: 0, color: chartPalette[0] },
            { offset: 1, color: chartPalette[1] }
          ])
        },
        label: {
          show: true,
          position: 'right',
          color: '#0f172a',
          formatter: (params: any) => {
            const value = values[params.dataIndex] ?? 0
            return Number(value).toLocaleString()
          }
        }
      }]
    }
  }

  option.value = current.value === 'map' ? makeMap() : makeBar()

  ;(toggle as any).impl = () => {
    current.value = current.value === 'map' ? 'bar' : 'map'
    option.value = current.value === 'map' ? makeMap() : makeBar()
  }
}

useFiltersReload(load)

function toggle() { (toggle as any).impl?.() }
</script>

<script lang="ts">export default { components: { 'v-chart': VChart } }</script>
