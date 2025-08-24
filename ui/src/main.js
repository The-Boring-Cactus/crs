import { createApp } from 'vue';
import App from './App.vue';
import router from './router';

import Aura from '@primeuix/themes/aura';
import PrimeVue from 'primevue/config';
import ConfirmationService from 'primevue/confirmationservice';
import ToastService from 'primevue/toastservice';


import VueExcelEditor from '@/components/VueExcelEditor.vue'

import { useSocketStoreWithOut } from "@/store/pinia/useSocketStore";
import  VueNativeSock  from "@/websocket/VueNativeSock";

import {userStoreMe} from "@/store/userStore";


import '@/assets/styles.scss';

const app = createApp(App);
const piniaSocketStore = useSocketStoreWithOut(app);


app.use(router);
app.use(PrimeVue, {
    theme: {
        preset: Aura,
        options: {
            darkModeSelector: '.app-dark'
        }
    },
    zIndex: {
            modal: 1100, // For dialogs, drawers, etc.
            overlay: 1000, // For select, popover, etc.
            menu: 4200, // For overlay menus like the Menubar dropdowns
            tooltip: 1100 // For tooltips
        }
});

app.use(
    VueNativeSock,
  'ws://localhost:8080/srv/',
  {
    store: piniaSocketStore,
     format: "json",
     connectManually: false,
      reconnection: true,
      reconnectionAttempts: 5,
      reconnectionDelay: 3000
  }
);
const userStore = userStoreMe();
app.use(ToastService);
app.use(ConfirmationService);
// app.use(VueExcelEditor)
app.component('VueExcelEditor', VueExcelEditor)

app.mount('#app');
