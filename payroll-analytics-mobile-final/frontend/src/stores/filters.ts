import { defineStore } from 'pinia'

export type Filters = {
  from?: string
  to?: string
  province?: string
  org?: string
  jobFamily?: string
}

export const useFilters = defineStore('filters', {
  state: (): Filters => ({
    from: '',
    to: '',
    province: '',
    org: '',
    jobFamily: ''
  })
})
