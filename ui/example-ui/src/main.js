import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import InputSwitch from 'primevue/inputswitch'
import Toast from 'primevue/toast'

import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'
import './style.css'

import App from './App.vue'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      prefix: 'p',
      darkModeSelector: false,
      cssLayer: false
    }
  }
})
app.use(ToastService)

app.component('InputText', InputText)
app.component('Password', Password)
app.component('Button', Button)
app.component('InputSwitch', InputSwitch)
app.component('Toast', Toast)

app.mount('#app')
