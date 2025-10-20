<script setup>
import { ref, onMounted } from 'vue';
import { getCurrentInstance } from 'vue'
import {userStoreMe} from "@/store/userStore";
import { useRouter } from 'vue-router'
import { useToast } from "primevue/usetoast";
import {WebSocketMessageClient} from "@/websocket/WebSocketMessageClient";
import {ServerResponse} from "@/websocket/ServerResponse";
import LogoSvg from '@/components/LogoSvg.vue';


const toast = useToast();

const router = useRouter()
const userStore = userStoreMe();


const loging = ref('');
const password = ref('');
const loading = ref(false);


var { proxy } = getCurrentInstance();

proxy.$socket.onmessage =  (data) => {
    const responseHandler = new ServerResponse();
    var jResponse = JSON.parse(data.data);
    if(jResponse.Type == 3){
        var response = responseHandler.analizeMessage(jResponse);
        console.log(response);
        loading.value = false;
        if(response.Type=="Response" || response.Status=="Success"){
            toast.add({ severity: 'Success', summary: 'OK', detail: 'Welcome', life: 3000 });
            userStore.setCurr(true,'User','admin', response.Data.Functions);
            router.push({ path: '/', replace: true })
        }
    }
}

const client = new WebSocketMessageClient(proxy.$socket);

const handleReady = () => {
        client.sendAuthentication(loging.value, password.value);
        loading.value = true;
}



</script>

<template>
     <Toast />
    <div class="bg-surface-50 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden relative" >
        <div v-if="loading" class="card flex justify-center">

    <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="8" fill="transparent"
    animationDuration=".5s" aria-label="Custom ProgressSpinner" />

    </div>
        <div class="flex flex-col items-center justify-center" v-if="!loading">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 py-20 px-8 sm:px-20" style="border-radius: 53px">
                    <div class="text-center mb-8">
                        <LogoSvg theme="light" style="width: 300px; height: 75px;" />

                        <div class="text-surface-900 text-3xl font-medium mb-4">Welcome!</div>
                        <span class="text-muted-color font-medium">Sign in to continue</span>
                    </div>

                    <div>
                        <label for="loging" class="block text-surface-900 text-xl font-medium mb-2">Login</label>
                        <InputText id="loging" type="text" placeholder="Username" class="w-full md:w-[30rem] mb-8" v-model="loging" />

                        <label for="password1" class="block text-surface-900 font-medium text-xl mb-2">Password</label>
                        <Password id="password1" v-model="password" placeholder="Password" :toggleMask="true" class="mb-4" fluid :feedback="false"></Password>

                        <Button label="Sign In" class="w-full" as="router-link" to="/" @click="handleReady"></Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.pi-eye {
    transform: scale(1.6);
    margin-right: 1rem;
}

.pi-eye-slash {
    transform: scale(1.6);
    margin-right: 1rem;
}
</style>