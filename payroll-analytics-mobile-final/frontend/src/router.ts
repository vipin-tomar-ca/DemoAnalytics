import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from './views/Dashboard.vue'
import Login from './views/Login.vue'
import { useAuth } from './stores/auth'

const routes = [
  { path: '/login', component: Login, name: 'login' },
  { path: '/', component: Dashboard, name: 'dashboard', meta: { requiresAuth: true } }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to, from, next) => {
  const auth = useAuth()
  if (to.meta.requiresAuth && !auth.token) next({ name: 'login' })
  else next()
})

export default router
