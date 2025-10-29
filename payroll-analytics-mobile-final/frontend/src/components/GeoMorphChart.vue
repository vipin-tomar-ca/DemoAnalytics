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
import { use } from 'echarts/core'
import { MapChart, BarChart } from 'echarts/charts'
import { GeoComponent, TooltipComponent, VisualMapComponent, GridComponent, DatasetComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import * as echarts from 'echarts/core' // Import echarts directly

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
  // const echarts = (await import('echarts')).default // Removed dynamic import
  echarts.registerMap('CANADA_SIMPLIFIED', canadaGeoJson as any)

  const rows = await fetchGeoHeadcount()
  const dataset = [{ id: 'geoHeadcount', source: rows }]

  const makeMap = () => ({
    dataset,
    tooltip: { trigger: 'item' },
    visualMap: { left: 0, min: 0, max: rows.length > 0 ? Math.max(...rows.map((r:any)=>r.value)) : 1, calculable: true },
    series: [{
      name: 'Headcount',
      type: 'map',
      map: 'CANADA_SIMPLIFIED',
      nameProperty: 'name',
      universalTransition: true,
      encode: { itemName: 'name', value: 'value' }
    }]
  })

  const makeBar = () => ({
    dataset,
    grid: { left: 60, right: 10, top: 20, bottom: 10 },
    xAxis: { type: 'value' },
    yAxis: { type: 'category' },
    series: [{
      type: 'bar',
      universalTransition: true,
      encode: { x: 'value', y: 'name' }
    }]
  })

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
