<script setup>
import { ref, computed, getCurrentInstance, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';
import { toast } from 'vue-sonner';
import { Toaster } from '@/components/ui/sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { BarChart, Home, Database, Table, FileSpreadsheet, Code, FileCode2, LogOut, Search, Settings, Bell, AlignRight, FileText, ChevronRight, ChevronDown, Pencil, Plus, Trash2, Sun, Moon, Palette, Check, Eye, EyeOff } from 'lucide-vue-next';
import { useLayout } from '@/layout/composables/layout';

const router = useRouter();
const route = useRoute();
const userStore = userStoreMe();
const projectStore = useProjectStore();
const { proxy } = getCurrentInstance();
const { layoutConfig, toggleDarkMode, setThemeColor, themeColors } = useLayout();

const isSetupRoute = computed(() => route.path === '/setup');
const isPublicRoute = computed(() => route.path.startsWith('/share/') || route.path.startsWith('/public/'));

const loading = ref(false);
const loginData = ref({
    username: '',
    password: ''
});
const searchQuery = ref('');
const isSidebarHovered = ref(false);
const showRegister = ref(false);
const registerData = ref({ username: '', email: '', password: '' });
const showProjectDialog = ref(false);
const isEditingProject = ref(false);
const editingProject = ref({ id: undefined, name: '', description: '' });

// Settings dialog
const showSettings = ref(false);
const settingsTab = ref('profile');
const settingsDisplayName = ref('');
const settingsOldPassword = ref('');
const settingsNewPassword = ref('');
const settingsConfirmPassword = ref('');
const savingProfile = ref(false);
const savingPassword = ref(false);
const showOldPw = ref(false);
const showNewPw = ref(false);
const showConfirmPw = ref(false);

const accentColors = [
    { name: 'indigo', label: 'Indigo', cls: 'bg-indigo-500' },
    { name: 'emerald', label: 'Emerald', cls: 'bg-emerald-500' },
    { name: 'blue', label: 'Blue', cls: 'bg-blue-500' },
    { name: 'rose', label: 'Rose', cls: 'bg-rose-500' },
    { name: 'amber', label: 'Amber', cls: 'bg-amber-500' },
];

const openSettings = () => {
    settingsDisplayName.value = '';
    settingsOldPassword.value = '';
    settingsNewPassword.value = '';
    settingsConfirmPassword.value = '';
    settingsTab.value = 'profile';
    showSettings.value = true;
};

const saveProfile = async () => {
    if (!settingsDisplayName.value.trim()) return;
    savingProfile.value = true;
    try {
        await userStore.executeCommand('UpdateUserProfile', { displayName: settingsDisplayName.value.trim() }, proxy.$socket);
        toast.success('Display name updated');
        settingsDisplayName.value = '';
    } catch (e) {
        toast.error('Failed to update profile', { description: e.message });
    } finally {
        savingProfile.value = false;
    }
};

const changePassword = async () => {
    if (!settingsOldPassword.value || !settingsNewPassword.value) {
        toast.error('Fill in all password fields');
        return;
    }
    if (settingsNewPassword.value !== settingsConfirmPassword.value) {
        toast.error('New passwords do not match');
        return;
    }
    if (settingsNewPassword.value.length < 6) {
        toast.error('New password must be at least 6 characters');
        return;
    }
    savingPassword.value = true;
    try {
        await userStore.executeCommand('ChangePassword', { oldPassword: settingsOldPassword.value, newPassword: settingsNewPassword.value }, proxy.$socket);
        toast.success('Password changed successfully');
        settingsOldPassword.value = '';
        settingsNewPassword.value = '';
        settingsConfirmPassword.value = '';
    } catch (e) {
        toast.error('Failed to change password', { description: e.message });
    } finally {
        savingPassword.value = false;
    }
};

// Check if user is authenticated
const isLoggedIn = computed(() => userStore.auth);
const userName = computed(() => userStore.name || 'User');
const userInitial = computed(() => userName.value.charAt(0).toUpperCase());

const login = async () => {
    loading.value = true;
    try {
        await userStore.authenticate(loginData.value.username, loginData.value.password, proxy.$socket);
        await projectStore.loadProjects(proxy.$socket);
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
    projectStore.setCurrentProject(null);
    router.push('/');
};

onMounted(async () => {
    if (!userStore.auth) {
        try {
            await userStore.restoreSession(proxy.$socket);
            await projectStore.loadProjects(proxy.$socket);
            router.push('/');
        } catch (_) {
            // No stored session — show login form
        }
    } else {
        await projectStore.loadProjects(proxy.$socket);
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

const isActive = (path) => {
    return route.path === path;
};

const projectSubItems = [
    { icon: Database, key: 'databases', label: 'Databases', path: '/pages/databases' },
    { icon: Table, key: 'datasets', label: 'Datasets', path: '/pages/dataset' },
    { icon: Code, key: 'sqleditor', label: 'SQL Queries', path: '/pages/sqleditor' },
    { icon: FileCode2, key: 'cseditor', label: 'Scripts', path: '/pages/cseditor' },
    { icon: FileSpreadsheet, key: 'myexcel', label: 'Excel', path: '/pages/myexcel' },
    { icon: BarChart, key: 'dashboard', label: 'Dashboards', path: '/pages/dashboard' },
];

const selectProjectItem = (project, subItem) => {
    projectStore.setCurrentProject(project.id);
    router.push(subItem.path);
};

const openNewProject = () => {
    isEditingProject.value = false;
    editingProject.value = { id: undefined, name: '', description: '' };
    showProjectDialog.value = true;
};

const openEditProject = (project) => {
    isEditingProject.value = true;
    editingProject.value = { id: project.id, name: project.name, description: project.description };
    showProjectDialog.value = true;
};

const saveProject = async () => {
    if (!editingProject.value.name.trim()) {
        toast.error('Name required', { description: 'Please enter a project name.' });
        return;
    }
    try {
        await projectStore.saveProject(editingProject.value, proxy.$socket);
        toast.success(isEditingProject.value ? 'Project updated' : 'Project created');
        showProjectDialog.value = false;
    } catch (error) {
        toast.error('Failed to save project', { description: error.message });
    }
};

const deleteProject = async (id) => {
    try {
        await projectStore.deleteProject(id, proxy.$socket);
        toast.success('Project deleted');
        showProjectDialog.value = false;
    } catch (error) {
        toast.error('Failed to delete project', { description: error.message });
    }
};
</script>

<template>
    <Toaster position="top-right" />

    <!-- Setup Wizard (full screen, no chrome) -->
    <div v-if="isSetupRoute" class="setup-fullscreen">
        <RouterView />
    </div>

    <!-- Public shared views — no auth required -->
    <div v-else-if="isPublicRoute">
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
    <div v-else class="app-container flex h-screen bg-background text-foreground transition-colors duration-300">
        <!-- Sidebar Wrap to give floating effect space -->
        <div class="sidebar-wrapper p-4 pr-0 hidden md:flex flex-col h-full shrink-0">
            <div
                class="bg-zinc-50 dark:bg-zinc-950 text-zinc-800 dark:text-zinc-50 rounded-[1.25rem] h-full flex flex-col shadow-xl border border-zinc-200 dark:border-zinc-800 overflow-hidden relative transition-all duration-300 ease-in-out"
                :class="isSidebarHovered ? 'w-[260px]' : 'w-[68px]'"
                @mouseenter="isSidebarHovered = true"
                @mouseleave="isSidebarHovered = false"
            >
                <!-- Header -->
                <div class="py-4 flex items-center border-b border-zinc-200 dark:border-zinc-800 transition-all duration-300 h-[53px]" :class="isSidebarHovered ? 'justify-between px-5' : 'justify-center px-0'">
                    <span class="font-medium text-[13px] tracking-wide text-zinc-800 dark:text-zinc-100 whitespace-nowrap transition-all duration-300" :class="isSidebarHovered ? 'opacity-100 w-auto' : 'opacity-0 w-0 overflow-hidden'"> Menu </span>
                    <AlignRight class="w-4 h-4 text-zinc-500 dark:text-zinc-400 shrink-0" />
                </div>

                <!-- Nav Items -->
                <div class="flex-col p-2 gap-1 flex overflow-y-auto mt-1 custom-scrollbar">
                    <!-- Home -->
                    <div
                        class="group flex items-center p-3 rounded-xl border cursor-pointer transition-all duration-200"
                        :class="[
                            isActive('/') 
                                ? 'bg-primary text-primary-foreground border-primary/10 shadow-lg shadow-primary/10 font-medium' 
                                : 'border-transparent text-zinc-500 hover:text-zinc-800 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-900/50', 
                            isSidebarHovered ? 'justify-start' : 'justify-center'
                        ]"
                        @click="navigateTo('/')"
                        title="Home"
                    >
                        <Home class="w-5 h-5 shrink-0 transition-colors" :class="isActive('/') ? 'text-primary-foreground' : 'text-zinc-400 group-hover:text-zinc-600 dark:group-hover:text-zinc-200'" />
                        <span class="overflow-hidden whitespace-nowrap transition-all duration-300 text-[13px]" :class="[isSidebarHovered ? 'opacity-100 ml-3 w-auto' : 'opacity-0 w-0 ml-0', isActive('/') ? 'text-primary-foreground' : 'text-zinc-600 dark:text-zinc-400 group-hover:text-zinc-900 dark:group-hover:text-zinc-200']">
                            Home
                        </span>
                    </div>

                    <!-- Projects section header -->
                    <div class="overflow-hidden whitespace-nowrap transition-all duration-300" :class="isSidebarHovered ? 'opacity-100' : 'opacity-0 h-0'">
                        <div class="flex items-center justify-between px-3 py-1 mt-2">
                            <span class="text-[10px] font-semibold uppercase tracking-wider text-zinc-400 dark:text-zinc-500">Projects</span>
                            <button
                                class="w-5 h-5 flex items-center justify-center rounded hover:bg-zinc-200 dark:hover:bg-zinc-800 text-zinc-400 dark:text-zinc-500 hover:text-zinc-700 dark:hover:text-zinc-300 transition-colors"
                                @click.stop="openNewProject()"
                                title="New project"
                            >
                                <Plus class="w-3 h-3" />
                            </button>
                        </div>
                    </div>

                    <!-- Collapsed state: show project icons -->
                    <div v-if="!isSidebarHovered" class="flex flex-col gap-1">
                        <div
                            v-for="project in projectStore.projects"
                            :key="project.id"
                            class="w-9 h-9 mx-auto flex items-center justify-center rounded-lg cursor-pointer transition-colors"
                            :class="projectStore.currentProjectId === project.id 
                                ? 'bg-primary/20 text-primary border border-primary/30' 
                                : 'text-zinc-500 hover:text-zinc-800 dark:hover:text-zinc-200 hover:bg-zinc-200 dark:hover:bg-zinc-800'"
                            :title="project.name"
                            @click="projectStore.toggleExpanded(project.id)"
                        >
                            <span class="text-[11px] font-bold">{{ project.name.charAt(0).toUpperCase() }}</span>
                        </div>
                    </div>

                    <!-- Expanded state: full project tree -->
                    <div v-if="isSidebarHovered" class="flex flex-col gap-0.5">
                        <div v-for="project in projectStore.projects" :key="project.id">
                            <!-- Project row -->
                            <div
                                class="group flex items-center gap-2 px-2 py-2 rounded-xl cursor-pointer transition-colors"
                                :class="projectStore.currentProjectId === project.id 
                                    ? 'bg-primary/10 text-primary font-medium' 
                                    : 'text-zinc-600 dark:text-zinc-400 hover:text-zinc-800 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-900/50'"
                                @click="projectStore.toggleExpanded(project.id)"
                            >
                                <ChevronDown v-if="projectStore.isExpanded(project.id)" class="w-3.5 h-3.5 shrink-0 text-zinc-400 dark:text-zinc-500" />
                                <ChevronRight v-else class="w-3.5 h-3.5 shrink-0 text-zinc-400 dark:text-zinc-500" />
                                <span class="text-[13px] flex-1 truncate">{{ project.name }}</span>
                                <button
                                    class="opacity-0 group-hover:opacity-100 w-5 h-5 flex items-center justify-center rounded hover:bg-zinc-200 dark:hover:bg-zinc-800 text-zinc-400 dark:text-zinc-500 hover:text-zinc-700 dark:hover:text-zinc-300 transition-all"
                                    @click.stop="openEditProject(project)"
                                    title="Edit project"
                                >
                                    <Pencil class="w-3 h-3" />
                                </button>
                            </div>

                            <!-- Sub-items -->
                            <div v-if="projectStore.isExpanded(project.id)" class="ml-4 flex flex-col gap-0.5 mb-1 pl-2 border-l border-zinc-200 dark:border-zinc-800">
                                <div
                                    v-for="subItem in projectSubItems"
                                    :key="subItem.key"
                                    class="group flex items-center gap-2.5 px-2 py-1.5 rounded-lg cursor-pointer transition-colors"
                                    :class="isActive(subItem.path) && projectStore.currentProjectId === project.id
                                        ? 'bg-primary/15 text-primary font-semibold'
                                        : 'text-zinc-500 dark:text-zinc-400 hover:text-zinc-800 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-900/50'"
                                    @click="selectProjectItem(project, subItem)"
                                >
                                    <component :is="subItem.icon" class="w-3.5 h-3.5 shrink-0" />
                                    <span class="text-[12px]">{{ subItem.label }}</span>
                                </div>
                            </div>
                        </div>

                        <div v-if="projectStore.projects.length === 0" class="px-3 py-2 text-[11px] text-zinc-500 italic">
                            No projects yet
                        </div>
                    </div>

                </div>

                <!-- Bottom items -->
                <div class="mt-auto p-2 border-t border-zinc-200 dark:border-zinc-800 flex flex-col gap-2">
                    <div
                        class="group flex items-center justify-center p-3 rounded-lg border border-zinc-200 dark:border-zinc-800 hover:bg-zinc-100 dark:hover:bg-zinc-900 transition-all duration-200 cursor-pointer overflow-hidden whitespace-nowrap"
                        @click="logout"
                        :title="!isSidebarHovered ? 'Cerrar Sesión' : ''"
                    >
                        <LogOut class="w-5 h-5 shrink-0 text-zinc-400 dark:text-zinc-500 group-hover:text-zinc-800 dark:group-hover:text-zinc-300" />
                        <span class="text-[13px] font-medium text-zinc-600 dark:text-zinc-300 group-hover:text-zinc-900 dark:group-hover:text-zinc-50 transition-all duration-300" :class="isSidebarHovered ? 'opacity-100 w-auto ml-2' : 'opacity-0 w-0 ml-0'"> Cerrar Sesión </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Project Dialog -->
        <div v-if="showProjectDialog" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="showProjectDialog = false">
            <div class="bg-zinc-950 border border-zinc-800 rounded-2xl p-6 w-full max-w-sm shadow-xl text-zinc-100">
                <h3 class="text-lg font-semibold text-zinc-100 mb-4">{{ isEditingProject ? 'Edit Project' : 'New Project' }}</h3>
                <div class="flex flex-col gap-3">
                    <div>
                        <Label class="text-zinc-400 text-xs mb-1 block">Name</Label>
                        <Input v-model="editingProject.name" placeholder="Project name" class="bg-zinc-900 border-zinc-800 text-zinc-100" @keyup.enter="saveProject" />
                    </div>
                    <div>
                        <Label class="text-zinc-400 text-xs mb-1 block">Description</Label>
                        <Input v-model="editingProject.description" placeholder="Optional description" class="bg-zinc-900 border-zinc-800 text-zinc-100" />
                    </div>
                    <div class="flex gap-2 mt-2">
                        <Button class="flex-1" @click="saveProject">Save</Button>
                        <Button variant="outline" class="flex-1" @click="showProjectDialog = false">Cancel</Button>
                    </div>
                    <Button v-if="isEditingProject" variant="destructive" class="w-full" @click="deleteProject(editingProject.id)">
                        <Trash2 class="w-4 h-4 mr-2" />
                        Delete Project
                    </Button>
                </div>
            </div>
        </div>

        <!-- Main Content -->
        <div class="main-content flex-1 flex flex-col min-w-0">
            <!-- Search Bar -->
            <div class="search-bar h-14 border-b bg-background flex items-center px-4 gap-4 sticky top-0 z-10 transition-colors duration-300">
                <div class="relative flex-1 max-w-md">
                    <Search class="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input type="text" placeholder="Buscar..." v-model="searchQuery" class="pl-9 bg-muted/50 w-full" />
                </div>
                
                <!-- Theme Colors Selector (Row of colored circles) -->
                <div class="flex items-center gap-1.5 px-3 py-1 bg-muted/40 rounded-full border border-zinc-200 dark:border-zinc-800 transition-colors">
                    <button
                        v-for="color in themeColors"
                        :key="color"
                        @click="setThemeColor(color)"
                        class="w-4 h-4 rounded-full transition-all duration-200 hover:scale-125 focus:outline-none"
                        :class="[
                            color === 'indigo' ? 'bg-indigo-500' : '',
                            color === 'emerald' ? 'bg-emerald-500' : '',
                            color === 'blue' ? 'bg-blue-500' : '',
                            color === 'rose' ? 'bg-rose-500' : '',
                            color === 'amber' ? 'bg-amber-500' : '',
                            layoutConfig.themeColor === color ? 'scale-110 ring-2 ring-primary ring-offset-2 dark:ring-offset-zinc-950' : 'opacity-60 hover:opacity-100'
                        ]"
                        :title="'Theme: ' + color"
                    ></button>
                </div>

                <!-- Dark Mode Toggle -->
                <button 
                    @click="toggleDarkMode" 
                    class="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted/60 transition-colors"
                    title="Toggle dark mode"
                >
                    <Sun v-if="layoutConfig.darkMode" class="w-4.5 h-4.5" />
                    <Moon v-else class="w-4.5 h-4.5" />
                </button>

                <button @click="openSettings" class="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted/60 transition-colors" title="Settings">
                    <Settings class="w-4.5 h-4.5" />
                </button>
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

    <!-- Settings Dialog -->
    <Dialog v-model:open="showSettings">
        <DialogContent class="max-w-lg">
            <DialogHeader>
                <DialogTitle class="flex items-center gap-2">
                    <Settings class="w-4 h-4" /> Settings
                </DialogTitle>
            </DialogHeader>

            <!-- Tab nav -->
            <div class="flex gap-1 bg-muted p-1 rounded-md mb-2">
                <button
                    v-for="tab in [{ id: 'profile', label: 'Profile' }, { id: 'password', label: 'Password' }, { id: 'appearance', label: 'Appearance' }]"
                    :key="tab.id"
                    @click="settingsTab = tab.id"
                    class="flex-1 px-3 py-1.5 text-sm font-medium rounded-sm transition-colors"
                    :class="settingsTab === tab.id ? 'bg-background text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
                >
                    {{ tab.label }}
                </button>
            </div>

            <!-- Profile tab -->
            <div v-if="settingsTab === 'profile'" class="space-y-4">
                <div class="space-y-2">
                    <Label>Display Name</Label>
                    <Input v-model="settingsDisplayName" placeholder="Enter new display name" @keyup.enter="saveProfile" />
                    <p class="text-xs text-muted-foreground">This name appears next to your avatar in the header.</p>
                </div>
                <Button @click="saveProfile" :disabled="!settingsDisplayName.trim() || savingProfile" class="w-full">
                    {{ savingProfile ? 'Saving...' : 'Update Display Name' }}
                </Button>
            </div>

            <!-- Password tab -->
            <div v-if="settingsTab === 'password'" class="space-y-4">
                <div class="space-y-2">
                    <Label>Current Password</Label>
                    <div class="relative">
                        <Input v-model="settingsOldPassword" :type="showOldPw ? 'text' : 'password'" placeholder="Enter current password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showOldPw = !showOldPw">
                            <EyeOff v-if="showOldPw" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                </div>
                <div class="space-y-2">
                    <Label>New Password</Label>
                    <div class="relative">
                        <Input v-model="settingsNewPassword" :type="showNewPw ? 'text' : 'password'" placeholder="Enter new password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showNewPw = !showNewPw">
                            <EyeOff v-if="showNewPw" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                </div>
                <div class="space-y-2">
                    <Label>Confirm New Password</Label>
                    <div class="relative">
                        <Input v-model="settingsConfirmPassword" :type="showConfirmPw ? 'text' : 'password'" placeholder="Confirm new password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showConfirmPw = !showConfirmPw">
                            <EyeOff v-if="showConfirmPw" class="w-4 h-4" /><Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                    <p v-if="settingsConfirmPassword && settingsNewPassword !== settingsConfirmPassword" class="text-xs text-destructive">Passwords do not match</p>
                </div>
                <Button @click="changePassword" :disabled="savingPassword || !settingsOldPassword || !settingsNewPassword || settingsNewPassword !== settingsConfirmPassword" class="w-full">
                    {{ savingPassword ? 'Changing...' : 'Change Password' }}
                </Button>
            </div>

            <!-- Appearance tab -->
            <div v-if="settingsTab === 'appearance'" class="space-y-5">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-sm font-medium">Theme</p>
                        <p class="text-xs text-muted-foreground">Switch between light and dark mode</p>
                    </div>
                    <button
                        @click="toggleDarkMode"
                        class="flex items-center gap-2 px-3 py-1.5 rounded-md border bg-background hover:bg-muted transition-colors text-sm font-medium"
                    >
                        <Moon v-if="!layoutConfig.darkMode" class="w-4 h-4" />
                        <Sun v-else class="w-4 h-4" />
                        {{ layoutConfig.darkMode ? 'Switch to Light' : 'Switch to Dark' }}
                    </button>
                </div>

                <div class="space-y-3">
                    <div>
                        <p class="text-sm font-medium">Accent Color</p>
                        <p class="text-xs text-muted-foreground">Primary color used across buttons and highlights</p>
                    </div>
                    <div class="flex gap-4 flex-wrap">
                        <button
                            v-for="color in accentColors"
                            :key="color.name"
                            @click="setThemeColor(color.name)"
                            class="flex flex-col items-center gap-1.5"
                            :title="color.label"
                        >
                            <div
                                class="w-9 h-9 rounded-full flex items-center justify-center ring-2 ring-offset-2 ring-offset-background transition-all"
                                :class="[color.cls, layoutConfig.themeColor === color.name ? 'ring-foreground scale-110' : 'ring-transparent']"
                            >
                                <Check v-if="layoutConfig.themeColor === color.name" class="w-4 h-4 text-white" />
                            </div>
                            <span class="text-xs text-muted-foreground">{{ color.label }}</span>
                        </button>
                    </div>
                </div>
            </div>
        </DialogContent>
    </Dialog>
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
