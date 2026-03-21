import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import VueExcelEditor from '@/components/VueExcelEditor.vue';
import { useSocketStoreWithOut } from '@/store/pinia/useSocketStore';
import VueNativeSock from '@/websocket/VueNativeSock';
import { userStoreMe } from '@/store/userStore';
import '@/assets/styles.scss';

const app = createApp(App);
const piniaSocketStore = useSocketStoreWithOut(app);

app.use(router);

app.use(VueNativeSock, 'ws://localhost:9876/srv/', {
    store: piniaSocketStore,
    format: 'json',
    connectManually: false,
    reconnection: true,
    reconnectionAttempts: 5,
    reconnectionDelay: 3000
});
const userStore = userStoreMe();
app.component('VueExcelEditor', VueExcelEditor);

app.mount('#app');
