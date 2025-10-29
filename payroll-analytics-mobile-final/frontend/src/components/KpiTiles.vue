<template>
  <div :class="$attrs.class">
    <div v-for="k in kpis" :key="k.label" class="card kpi">
      <div class="label">{{ k.label }}</div>
      <div class="value">{{ k.value }}</div>
      <div v-if="k.delta !== undefined" class="text-xs" :class="k.delta>0?'text-green-600':'text-red-600'">
        {{ k.delta>0? '+' : '' }}{{ k.delta }} {{ k.suffix || '' }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { fetchKpisAll, fetchKpis, type Kpis } from '../api'
import { useFiltersReload } from '../composables/useFiltersReload'
import { useFilters } from '../stores/filters' // Corrected import path

const model = ref<Kpis | null>(null)
const kpis = ref<any[]>([])
const filters = useFilters()

async function load() {
  if (filters.from && filters.to) {
    model.value = await fetchKpis(filters.from.toISOString().split('T')[0], filters.to.toISOString().split('T')[0])
  } else {
    model.value = await fetchKpisAll()
  }
  
  if (!model.value) return
  kpis.value = [
    { label: 'Headcount', value: model.value.headcount.toLocaleString() },
    { label: 'New Hires', value: model.value.newHires.toLocaleString() },
    { label: 'Terminations', value: model.value.terminations.toLocaleString() },
    { label: 'Overall Cost', value: `$${Math.round(model.value.overallCost).toLocaleString()}` },
    { label: 'Overtime Cost', value: `$${Math.round(model.value.overtimeCost).toLocaleString()}` },
    { label: 'Overtime %', value: model.value.overtimePct.toFixed(1), suffix: '%' },
    { label: 'Avg Salary', value: `$${Math.round(model.value.averageSalary).toLocaleString()}` },
    { label: 'Turnover Rate', value: model.value.turnoverRate.toFixed(1), suffix: '%' },
    { label: 'Turnover Cost', value: `$${Math.round(model.value.turnoverCost).toLocaleString()}` },
  ]
}

useFiltersReload(load)
load()
</script>
