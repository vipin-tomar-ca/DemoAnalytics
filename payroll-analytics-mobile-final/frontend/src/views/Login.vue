<template>
  <main class="min-h-screen flex items-center justify-center p-4">
    <form class="card w-full max-w-sm space-y-3" @submit.prevent="doLogin">
      <h1 class="text-xl font-bold">Sign in</h1>
      <input v-model="username" placeholder="Username" class="w-full border rounded-lg px-3 py-2" />
      <input v-model="password" placeholder="Password" type="password" class="w-full border rounded-lg px-3 py-2" />
      <button class="w-full px-3 py-2 rounded-lg bg-black text-white">Login</button>
      <p class="text-xs text-neutral-500">Demo: admin / admin123</p>
      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
    </form>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../api'
import { useAuth } from '../stores/auth'

const router = useRouter()
const auth = useAuth()
const username = ref('admin')
const password = ref('admin123')
const error = ref('')

async function doLogin() {
  error.value = ''
  try {
    const res = await login(username.value, password.value)
    auth.setAuth(res.token, res.username)
    router.replace('/')
  } catch {
    error.value = 'Invalid credentials'
  }
}
</script>
