import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'
import HomeView from '@/views/HomeView.vue'
import RoomsView from '@/views/RoomsView.vue'
import EnergyView from '@/views/EnergyView.vue'
import SecurityView from '@/views/SecurityView.vue'
import LocationView from '@/views/LocationView.vue'
import MembersView from '@/views/MembersView.vue'
import AnalyticsView from '@/views/AnalyticsView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
    meta: { requiresAuth: true, icon: 'pi-home' }
  },
  {
    path: '/rooms',
    name: 'rooms',
    component: RoomsView,
    meta: { requiresAuth: true, icon: 'pi-th-large' }
  },
  {
    path: '/energy',
    name: 'energy',
    component: EnergyView,
    meta: { requiresAuth: true, icon: 'pi-sun' }
  },
  {
    path: '/security',
    name: 'security',
    component: SecurityView,
    meta: { requiresAuth: true, icon: 'pi-shield' }
  },
  {
    path: '/location',
    name: 'location',
    component: LocationView,
    meta: { requiresAuth: true, icon: 'pi-map-marker' }
  },
  {
    path: '/members',
    name: 'members',
    component: MembersView,
    meta: { requiresAuth: true, icon: 'pi-users' }
  },
  {
    path: '/analytics',
    name: 'analytics',
    component: AnalyticsView,
    meta: { requiresAuth: true, icon: 'pi-chart-bar' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const userStore = useUserStore()

  if (to.meta.requiresAuth && !userStore.isLoggedIn) {
    // Redirect to login (which is the main app)
    next('/')
  } else {
    next()
  }
})

export default router
