<template>
  <main class="max-w-6xl mx-auto p-3 md:p-6 space-y-4">
    <header class="flex items-center justify-between">
      <h1 class="text-xl md:text-2xl font-bold">Payroll Analytics</h1>
      <div class="flex items-center gap-3 text-xs text-neutral-500"><span>Vue 3 • ECharts • .NET 8</span><button class="px-2 py-1 rounded bg-neutral-200" @click="logout()">Logout</button></div>
    </header>

    <section class="card filterbar">
      <div class="text-sm font-semibold mr-2">Filters</div>
      <input type="date" v-model="filters.from" />
      <input type="date" v-model="filters.to" />
      <select v-model="filters.province">
        <option value="">All Provinces</option>
        <option v-for="p in provinces" :key="p" :value="p">{{ p }}</option>
      </select>
      <input placeholder="Org Unit" v-model="filters.org" />
      <input placeholder="Job Family" v-model="filters.jobFamily" />
      <button class="ml-auto text-xs px-3 py-1 rounded bg-neutral-100" @click="refresh()">Apply</button>
    </section>

    <KpiTiles class="grid grid-cols-2 md:grid-cols-5 gap-3" />

    <!-- Headcount & Geo -->
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <HeadcountTrend class="card" />
      <GeoMorphChart class="card" />
    </section>

    <!-- Time & Compensation -->
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <TimeHeatmap class="card" />
      <CompensationBox class="card" />
    </section>

    <!-- Cost-related KPIs -->
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <TcowOverview class="card" />
      <BudgetVariance class="card" />
    </section>
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <OvertimeCostTrend class="card" />
      <AbsenteeismCostTrend class="card" />
    </section>

    <!-- Efficiency & Competitiveness -->
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <ProcessEfficiency class="card" />
      <CompetitivenessScatter class="card" />
    </section>

    <!-- Pay Equity & Impact -->
    <section class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <PayEquityBridge class="card" />
      <ImpactKPIs class="card" />
    </section>

    <!-- Turnover -->
    <section class="grid grid-cols-1 md:grid-cols-1 gap-3">
      <TurnoverCosts class="card" />
    </section>
  </main>
</template>

<script setup lang="ts">
import { useFilters } from '../stores/filters'
import KpiTiles from '../components/KpiTiles.vue'
import HeadcountTrend from '../components/HeadcountTrend.vue'
import GeoMorphChart from '../components/GeoMorphChart.vue'
import TimeHeatmap from '../components/TimeHeatmap.vue'
import CompensationBox from '../components/CompensationBox.vue'

import TcowOverview from '../components/visier/TcowOverview.vue'
import BudgetVariance from '../components/visier/BudgetVariance.vue'
import OvertimeCostTrend from '../components/visier/OvertimeCostTrend.vue'
import AbsenteeismCostTrend from '../components/visier/AbsenteeismCostTrend.vue'
import ProcessEfficiency from '../components/visier/ProcessEfficiency.vue'
import CompetitivenessScatter from '../components/visier/CompetitivenessScatter.vue'
import PayEquityBridge from '../components/visier/PayEquityBridge.vue'
import ImpactKPIs from '../components/visier/ImpactKPIs.vue'
import TurnoverCosts from '../components/visier/TurnoverCosts.vue'

const filters = useFilters()
const provinces = ['Ontario','Quebec','British Columbia','Alberta','Manitoba','Saskatchewan','Nova Scotia','New Brunswick','Newfoundland and Labrador','Prince Edward Island','Yukon','Northwest Territories','Nunavut']

function refresh() {
  window.dispatchEvent(new Event('filters.changed'))
}

import { useAuth } from '../stores/auth'
const auth = useAuth()
function logout(){ auth.logout(); window.location.href='/login' }
</script>

