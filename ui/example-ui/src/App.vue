<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useToast } from 'primevue/usetoast'

const router = useRouter()
const userStore = useUserStore()
const toast = useToast()

const loading = ref(false)
const loginData = ref({
  username: '',
  password: ''
})
const searchQuery = ref('')

const login = () => {
  loading.value = true
  setTimeout(() => {
    const success = userStore.login(loginData.value.username, loginData.value.password)
    if (success) {
      toast.add({
        severity: 'success',
        summary: 'Bienvenido',
        detail: `Hola ${loginData.value.username}!`,
        life: 3000
      })
      router.push('/')
    } else {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Por favor ingresa usuario y contraseña',
        life: 3000
      })
    }
    loading.value = false
  }, 1000)
}

const logout = () => {
  userStore.logout()
  toast.add({
    severity: 'info',
    summary: 'Sesión cerrada',
    detail: 'Has cerrado sesión exitosamente',
    life: 3000
  })
}

const navigateTo = (routeName) => {
  router.push({ name: routeName })
}
</script>

<template>
  <Toast />

  <!-- Login Screen -->
  <div v-if="!userStore.isLoggedIn" class="login-container">
    <div class="login-card">
      <div class="text-center mb-4">
        <i class="pi pi-home" style="font-size: 3rem; color: #8B5CF6;"></i>
        <h2 class="mt-3">Smart Home</h2>
        <p class="text-500">Bienvenido de vuelta</p>
      </div>

      <div class="field mt-4">
        <span class="p-float-label">
          <InputText id="username" v-model="loginData.username" class="w-full" />
          <label for="username">Usuario</label>
        </span>
      </div>

      <div class="field mt-4">
        <span class="p-float-label">
          <Password id="password" v-model="loginData.password" class="w-full" :feedback="false" @keyup.enter="login" fluid toggleMask  />
          <label for="password">Contraseña</label>
        </span>
      </div>

      <Button
        label="Iniciar Sesión"
        class="w-full mt-4"
        @click="login"
        :loading="loading"
      />

      <div class="text-center mt-3">
        <a href="#" style="color: #8B5CF6; text-decoration: none; font-size: 0.9rem;">
          ¿Olvidaste tu contraseña?
        </a>
      </div>
    </div>
  </div>

  <!-- Main App -->
  <div v-else class="app-container">
    <!-- Sidebar -->
    <div class="sidebar">
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'home' }"
        @click="navigateTo('home')"
      >
        <i class="pi pi-home"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'rooms' }"
        @click="navigateTo('rooms')"
      >
        <i class="pi pi-th-large"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'energy' }"
        @click="navigateTo('energy')"
      >
        <i class="pi pi-sun"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'security' }"
        @click="navigateTo('security')"
      >
        <i class="pi pi-shield"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'location' }"
        @click="navigateTo('location')"
      >
        <i class="pi pi-map-marker"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'members' }"
        @click="navigateTo('members')"
      >
        <i class="pi pi-users"></i>
      </div>
      <div
        class="sidebar-item"
        :class="{ active: $route.name === 'analytics' }"
        @click="navigateTo('analytics')"
      >
        <i class="pi pi-chart-bar"></i>
      </div>
      <div class="sidebar-item" style="margin-top: auto;" @click="logout">
        <i class="pi pi-sign-out"></i>
      </div>
    </div>

    <!-- Main Content -->
    <div class="main-content">
      <!-- Search Bar -->
      <div class="search-bar">
        <i class="pi pi-search" style="color: #999;"></i>
        <input type="text" placeholder="Search" v-model="searchQuery">
        <i class="pi pi-cog" style="color: #666; margin-left: auto; cursor: pointer;"></i>
        <i class="pi pi-bell" style="color: #666; margin-left: 1rem; cursor: pointer;"></i>
        <div style="display: flex; align-items: center; margin-left: 1rem;">
          <div class="member-avatar" style="width: 35px; height: 35px; font-size: 0.9rem;">
            {{ userStore.userInitial }}
          </div>
          <span style="margin-left: 0.5rem; font-weight: 500;">{{ userStore.userName }}</span>
        </div>
      </div>

      <!-- Router View -->
      <RouterView />
    </div>
  </div>
</template>
