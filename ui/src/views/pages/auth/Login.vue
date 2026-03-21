<script setup>
import { ref } from 'vue';
import { getCurrentInstance } from 'vue';
import { userStoreMe } from '@/store/userStore';
import { useRouter } from 'vue-router';
import { toast } from 'vue-sonner';
import { WebSocketMessageClient } from '@/websocket/WebSocketMessageClient';
import { ServerResponse } from '@/websocket/ServerResponse';
import LogoSvg from '@/components/LogoSvg.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Loader2 } from 'lucide-vue-next';

const router = useRouter();
const userStore = userStoreMe();

const loging = ref('');
const password = ref('');
const loading = ref(false);

var { proxy } = getCurrentInstance();

proxy.$socket.onmessage = (data) => {
    const responseHandler = new ServerResponse();
    var jResponse = JSON.parse(data.data);
    if (jResponse.Type == 3) {
        var response = responseHandler.analizeMessage(jResponse);
        console.log(response);
        loading.value = false;
        if (response.Type == 'Response' || response.Status == 'Success') {
            toast.success('Welcome', { description: 'OK' });
            userStore.setCurr(true, 'User', 'admin', response.Data.Functions);
            router.push({ path: '/', replace: true });
        }
    }
};

const client = new WebSocketMessageClient(proxy.$socket);

const handleReady = () => {
    client.sendAuthentication(loging.value, password.value);
    loading.value = true;
};
</script>

<template>
    <div class="bg-muted/30 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden relative">
        <div v-if="loading" class="card flex justify-center">
            <Loader2 class="w-12 h-12 animate-spin text-primary" />
        </div>
        <div class="flex flex-col items-center justify-center" v-if="!loading">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, hsl(var(--primary)) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-card py-20 px-8 sm:px-20" style="border-radius: 53px">
                    <div class="text-center mb-8">
                        <LogoSvg theme="light" style="width: 300px; height: 75px" />

                        <div class="text-foreground text-3xl font-medium mb-4">Welcome!</div>
                        <span class="text-muted-foreground font-medium">Sign in to continue</span>
                    </div>

                    <div>
                        <label for="loging" class="block text-foreground text-xl font-medium mb-2">Login</label>
                        <Input id="loging" type="text" placeholder="Username" class="w-full md:w-[30rem] mb-8" v-model="loging" />

                        <label for="password1" class="block text-foreground font-medium text-xl mb-2">Password</label>
                        <Input id="password1" type="password" v-model="password" placeholder="Password" class="mb-4" />

                        <Button class="w-full mt-4" @click="handleReady">Sign In</Button>
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
