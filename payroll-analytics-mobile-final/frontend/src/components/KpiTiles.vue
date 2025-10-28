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
import { onMounted, ref } from 'vue'
import { fetchKpis, type Kpis } from '../api'

const model = ref<Kpis | null>(null)
const kpis = ref<any[]>([])

async function load() {
  model.value = await fetchKpis()
  if (!model.value) return
  kpis.value = [
    { label: 'Headcount', value: model.value.headcount.toLocaleString() },
    { label: 'Hires (MTD)', value: model.value.hiresMtd },
    { label: 'Exits (MTD)', value: model.value.exitsMtd },
    { label: 'Overtime %', value: model.value.overtimePct.toFixed(1), suffix: '%' },
    { label: 'Avg Salary', value: `$${Math.round(model.value.avgSalary).toLocaleString()}` }
  ]
}

onMounted(() => {
  load()
  window.addEventListener('filters.changed', load)
})
</script>
