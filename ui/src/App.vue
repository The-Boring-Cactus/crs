<script setup>
import { ref, computed, getCurrentInstance, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { userStoreMe } from '@/store/userStore';
import { toast } from 'vue-sonner';
import { Toaster } from '@/components/ui/sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { BarChart, Home, Database, Table, FileSpreadsheet, Code, FileCode2, LogOut, Search, Settings, Bell, AlignRight, FileText } from 'lucide-vue-next';

const router = useRouter();
const route = useRoute();
const userStore = userStoreMe();
const { proxy } = getCurrentInstance();

const isSetupRoute = computed(() => route.path === '/setup');

const loading = ref(false);
const loginData = ref({
    username: '',
    password: ''
});
const searchQuery = ref('');
const isSidebarHovered = ref(false);
const showRegister = ref(false);
const registerData = ref({ username: '', email: '', password: '' });

// Check if user is authenticated
const isLoggedIn = computed(() => userStore.auth);
const userName = computed(() => userStore.name || 'User');
const userInitial = computed(() => userName.value.charAt(0).toUpperCase());

const login = async () => {
    loading.value = true;
    try {
        await userStore.authenticate(loginData.value.username, loginData.value.password, proxy.$socket);
        router.push('/');
    } catch (error) {
        toast.error('Authentication Failed', { description: 'Please check your credentials.' });
    } finally {
        loading.value = false;
    }
};

const logout = () => {
    localStorage.removeItem('crs_token');
    try { proxy.$socket?.close(); } catch (_) {}
    userStore.setCurr(false, '', '', []);
    router.push('/');
};

onMounted(async () => {
    if (!userStore.auth) {
        try {
            await userStore.restoreSession(proxy.$socket);
            router.push('/');
        } catch (_) {
            // No stored session — show login form
        }
    }
});

const handleRegister = async () => {
    if (!registerData.value.username || !registerData.value.password) {
        toast.error('Error', { description: 'Username and password are required' });
        return;
    }
    loading.value = true;
    try {
        const resp = await fetch(`${import.meta.env.VITE_API_URL}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: registerData.value.username,
                email: registerData.value.email,
                password: registerData.value.password
            })
        });
        if (!resp.ok) {
            const err = await resp.json();
            throw new Error(err.message || 'Registration failed');
        }
        toast.success('Account created', { description: 'You can now log in.' });
        showRegister.value = false;
        registerData.value = { username: '', email: '', password: '' };
    } catch (error) {
        toast.error('Registration failed', { description: error.message });
    } finally {
        loading.value = false;
    }
};

const navigateTo = (path) => {
    router.push(path);
};

// Define navigation items based on your routes
const navItems = [
    { icon: Home, path: '/', name: 'home', label: 'Home', category: 'General' },
    { icon: BarChart, path: '/pages/dashboard', name: 'dashboard', label: 'Dashboard', category: 'Pages' },
    { icon: FileText, path: '/pages/reports', name: 'reports', label: 'Reports', category: 'Pages' },
    { icon: Database, path: '/pages/databases', name: 'databases', label: 'Databases', category: 'Data' },
    { icon: Table, path: '/pages/dataset', name: 'dataset', label: 'Dataset', category: 'Data' },
    { icon: FileSpreadsheet, path: '/pages/myexcel', name: 'myexcel', label: 'Excel', category: 'Tools' },
    { icon: Code, path: '/pages/sqleditor', name: 'sqleditor', label: 'SQL Editor', category: 'Editor' },
    { icon: FileCode2, path: '/pages/cseditor', name: 'cseditor', label: 'C# Editor', category: 'Editor' }
];

const isActive = (path) => {
    return route.path === path;
};
</script>

<template>
    <Toaster position="top-right" />

    <!-- Setup Wizard (full screen, no chrome) -->
    <div v-if="isSetupRoute" class="setup-fullscreen">
        <RouterView />
    </div>

    <!-- Login Screen -->
    <div v-else-if="!isLoggedIn" class="login-container">
        <div class="login-card">
            <div class="text-center mb-4">
                <BarChart class="mx-auto" style="width: 3rem; height: 3rem; color: #8b5cf6" />
                <h2 class="mt-3 text-2xl font-bold">CRS Reporter</h2>
                <p class="text-muted-foreground">Bienvenido de vuelta</p>
            </div>

            <div class="grid w-full items-center gap-1.5 mt-4">
                <Label for="username">Usuario</Label>
                <Input id="username" v-model="loginData.username" type="text" />
            </div>

            <div class="grid w-full items-center gap-1.5 mt-4">
                <Label for="password">Contraseña</Label>
                <Input id="password" v-model="loginData.password" type="password" @keyup.enter="login" />
            </div>

            <Button class="w-full mt-6" @click="login" :disabled="loading">
                <span v-if="loading" class="animate-spin mr-2">⠋</span>
                Iniciar Sesión
            </Button>

            <div class="text-center mt-3">
                <a href="#" style="color: #8b5cf6; text-decoration: none; font-size: 0.9rem"> ¿Olvidaste tu contraseña? </a>
            </div>

            <div class="text-center mt-3 text-sm text-muted-foreground">
                Don't have an account?
                <a href="/auth/register" class="text-primary hover:underline cursor-pointer" @click.prevent="showRegister = true">Register</a>
            </div>
        </div>

        <!-- Register Dialog -->
        <div v-if="showRegister" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" @click.self="showRegister = false">
            <div class="bg-card rounded-2xl p-8 w-full max-w-sm shadow-xl">
                <h3 class="text-xl font-bold mb-6 text-center">Create Account</h3>
                <div class="grid gap-4">
                    <div>
                        <Label for="reg-username">Username</Label>
                        <Input id="reg-username" v-model="registerData.username" type="text" class="mt-1" />
                    </div>
                    <div>
                        <Label for="reg-email">Email</Label>
                        <Input id="reg-email" v-model="registerData.email" type="email" class="mt-1" />
                    </div>
                    <div>
                        <Label for="reg-password">Password</Label>
                        <Input id="reg-password" v-model="registerData.password" type="password" class="mt-1" />
                    </div>
                    <Button class="w-full mt-2" @click="handleRegister" :disabled="loading">
                        <span v-if="loading" class="animate-spin mr-2">⠋</span>
                        Create Account
                    </Button>
                    <Button variant="ghost" class="w-full" @click="showRegister = false">Cancel</Button>
                </div>
            </div>
        </div>
    </div>

    <!-- Main App -->
    <div v-else class="app-container flex h-screen">
        <!-- Sidebar Wrap to give floating effect space -->
        <div class="sidebar-wrapper p-4 pr-0 hidden md:flex flex-col h-full shrink-0">
            <div
                class="bg-zinc-950 text-zinc-50 rounded-[1.25rem] h-full flex flex-col shadow-xl border border-zinc-800 overflow-hidden relative transition-all duration-300 ease-in-out"
                :class="isSidebarHovered ? 'w-[260px]' : 'w-[68px]'"
                @mouseenter="isSidebarHovered = true"
                @mouseleave="isSidebarHovered = false"
            >
                <!-- Header -->
                <div class="py-4 flex items-center border-b border-zinc-800 transition-all duration-300 h-[53px]" :class="isSidebarHovered ? 'justify-between px-5' : 'justify-center px-0'">
                    <span class="font-medium text-[13px] tracking-wide text-zinc-100 whitespace-nowrap transition-all duration-300" :class="isSidebarHovered ? 'opacity-100 w-auto' : 'opacity-0 w-0 overflow-hidden'"> Menu </span>
                    <AlignRight class="w-4 h-4 text-zinc-400 shrink-0" />
                </div>

                <!-- Nav Items -->
                <div class="flex-col p-2 gap-2 flex overflow-y-auto mt-1 custom-scrollbar">
                    <div
                        v-for="item in navItems"
                        :key="item.path"
                        class="group flex items-center p-3 rounded-xl border cursor-pointer transition-all duration-200"
                        :class="[isActive(item.path) ? 'bg-zinc-900 border-zinc-700 shadow-sm' : 'border-transparent hover:bg-zinc-900/50 hover:border-zinc-800/50', isSidebarHovered ? 'justify-start' : 'justify-center']"
                        @click="navigateTo(item.path)"
                        :title="!isSidebarHovered ? item.label : ''"
                    >
                        <component :is="item.icon" class="w-5 h-5 shrink-0 transition-colors" :class="isActive(item.path) ? 'text-zinc-100' : 'text-zinc-500 group-hover:text-zinc-300'" />

                        <div class="flex flex-col justify-center overflow-hidden whitespace-nowrap transition-all duration-300" :class="isSidebarHovered ? 'w-[160px] opacity-100 ml-3' : 'w-0 opacity-0 ml-0'">
                            <span class="text-[10px] font-semibold uppercase tracking-wider text-zinc-500 transition-colors" :class="isActive(item.path) ? 'text-zinc-400' : 'group-hover:text-zinc-400'">
                                {{ item.category }}
                            </span>
                            <span class="text-[13px] font-medium transition-colors" :class="isActive(item.path) ? 'text-zinc-50' : 'text-zinc-300 group-hover:text-zinc-100'">
                                {{ item.label }}
                            </span>
                        </div>
                    </div>
                </div>

                <!-- Bottom items -->
                <div class="mt-auto p-2 border-t border-zinc-800 flex flex-col gap-2">
                    <div
                        class="group flex items-center justify-center p-3 rounded-lg border border-zinc-800 hover:bg-zinc-900 transition-all duration-200 cursor-pointer overflow-hidden whitespace-nowrap"
                        @click="logout"
                        :title="!isSidebarHovered ? 'Cerrar Sesión' : ''"
                    >
                        <LogOut class="w-5 h-5 shrink-0 text-zinc-500 group-hover:text-zinc-300" />
                        <span class="text-[13px] font-medium text-zinc-300 group-hover:text-zinc-50 transition-all duration-300" :class="isSidebarHovered ? 'opacity-100 w-auto ml-2' : 'opacity-0 w-0 ml-0'"> Cerrar Sesión </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Main Content -->
        <div class="main-content flex-1 flex flex-col min-w-0">
            <!-- Search Bar -->
            <div class="search-bar h-14 border-b bg-background flex items-center px-4 gap-4 sticky top-0 z-10">
                <div class="relative flex-1 max-w-md">
                    <Search class="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input type="text" placeholder="Buscar..." v-model="searchQuery" class="pl-9 bg-muted/50 w-full" />
                </div>
                <Settings class="w-5 h-5 text-muted-foreground ml-auto cursor-pointer hover:text-foreground" />
                <Bell class="w-5 h-5 text-muted-foreground cursor-pointer hover:text-foreground" />
                <div class="flex items-center gap-2 ml-2 pl-4 border-l">
                    <div class="member-avatar w-8 h-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-sm font-medium">
                        {{ userInitial }}
                    </div>
                    <span class="font-medium text-sm hidden sm:block">{{ userName }}</span>
                </div>
            </div>

            <!-- Router View -->
            <div class="flex-1 overflow-auto p-4 md:p-6 lg:p-8">
                <RouterView />
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Scoped styles if needed - most styles are in app-style.css */
.custom-scrollbar::-webkit-scrollbar {
    width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
    background: #3f3f46;
    border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: #52525b;
}
.setup-fullscreen {
    width: 100vw;
    min-height: 100vh;
}
</style>
