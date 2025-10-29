import { onMounted, onUnmounted } from 'vue'

export const FILTERS_CHANGED_EVENT = 'filters.changed'

type Handler = () => void | Promise<void>

export function useFiltersReload(handler: Handler, options: { immediate?: boolean } = {}) {
  const { immediate = true } = options

  const run = () => {
    Promise.resolve(handler()).catch((err) => console.error('Failed to reload data', err))
  }

  onMounted(() => {
    if (immediate) run()
    window.addEventListener(FILTERS_CHANGED_EVENT, run)
  })

  onUnmounted(() => {
    window.removeEventListener(FILTERS_CHANGED_EVENT, run)
  })
}

export function emitFiltersChanged() {
  window.dispatchEvent(new Event(FILTERS_CHANGED_EVENT))
}
