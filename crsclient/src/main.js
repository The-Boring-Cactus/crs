import { createApp } from 'vue'
import App from './App.vue'

import SplitPanes from './plugins/splitpanes'
import NaiveUI from './plugins/naive-ui'
import i18n from './plugins/i18n'
import pinia from './plugins/pinia'

import 'virtual:windi.css'
import 'virtual:windi-devtools'
import './assets/index.css'
import CarbonComponentsVue from '@carbon/vue'
const app = createApp(App)

app.use(SplitPanes)
app.use(NaiveUI)
app.use(i18n)
app.use(pinia)
app.use(CarbonComponentsVue)
app.mount('#app')
