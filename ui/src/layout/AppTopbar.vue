<script setup>
import { nextTick, ref, computed, getCurrentInstance } from 'vue';
import { useLayout } from '@/layout/composables/layout';
import { userStoreMe } from '@/store/userStore';
import { useRouter } from 'vue-router';
import { Menu, Settings, XCircle, Sun, Moon, Check, Eye, EyeOff } from 'lucide-vue-next';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { toast } from 'vue-sonner';

const router = useRouter();
const userStore = userStoreMe();
const { toggleMenu, layoutConfig, toggleDarkMode, setThemeColor, themeColors } = useLayout();
const { proxy } = getCurrentInstance();

// Settings dialog state
const showSettings = ref(false);
const settingsTab = ref('profile');

// Profile
const displayName = ref('');
const savingProfile = ref(false);

// Password
const oldPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const savingPassword = ref(false);
const showOld = ref(false);
const showNew = ref(false);
const showConfirm = ref(false);

const accentColors = [
    { name: 'indigo', label: 'Indigo', class: 'bg-indigo-500' },
    { name: 'emerald', label: 'Emerald', class: 'bg-emerald-500' },
    { name: 'blue', label: 'Blue', class: 'bg-blue-500' },
    { name: 'rose', label: 'Rose', class: 'bg-rose-500' },
    { name: 'amber', label: 'Amber', class: 'bg-amber-500' },
];

function openSettings() {
    displayName.value = '';
    oldPassword.value = '';
    newPassword.value = '';
    confirmPassword.value = '';
    settingsTab.value = 'profile';
    showSettings.value = true;
}

async function saveProfile() {
    if (!displayName.value.trim()) return;
    savingProfile.value = true;
    try {
        await userStore.executeCommand('UpdateUserProfile', { displayName: displayName.value.trim() }, proxy.$socket);
        toast.success('Display name updated');
        displayName.value = '';
    } catch (e) {
        toast.error('Failed to update profile', { description: e.message });
    } finally {
        savingProfile.value = false;
    }
}

async function changePassword() {
    if (!oldPassword.value || !newPassword.value) {
        toast.error('Please fill in all password fields');
        return;
    }
    if (newPassword.value !== confirmPassword.value) {
        toast.error('New passwords do not match');
        return;
    }
    if (newPassword.value.length < 6) {
        toast.error('New password must be at least 6 characters');
        return;
    }
    savingPassword.value = true;
    try {
        await userStore.executeCommand('ChangePassword', { oldPassword: oldPassword.value, newPassword: newPassword.value }, proxy.$socket);
        toast.success('Password changed successfully');
        oldPassword.value = '';
        newPassword.value = '';
        confirmPassword.value = '';
    } catch (e) {
        toast.error('Failed to change password', { description: e.message });
    } finally {
        savingPassword.value = false;
    }
}

const exit = async () => {
    userStore.setCurr(false, '', '');
    await nextTick();
    await router.push({ path: '/auth/login', replace: true });
};
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <Menu class="w-5 h-5 text-zinc-500" />
            </button>
            <router-link to="/" class="layout-topbar-logo">
                <span>CRS Reporter</span>
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <!-- Settings button -->
            <button type="button" class="layout-topbar-action layout-topbar-action-highlight" @click="openSettings" title="Settings">
                <Settings class="w-5 h-5 text-zinc-500" />
            </button>

            <!-- Exit button -->
            <button type="button" class="layout-topbar-action layout-topbar-action-highlight" @click="exit" title="Sign out">
                <XCircle class="w-5 h-5 text-zinc-500" />
            </button>
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
            <div class="flex gap-1 bg-muted p-1 rounded-md mb-4">
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
                    <Label for="displayName">Display Name</Label>
                    <Input
                        id="displayName"
                        v-model="displayName"
                        placeholder="Enter new display name"
                        @keyup.enter="saveProfile"
                    />
                    <p class="text-xs text-muted-foreground">This name appears in the application header.</p>
                </div>
                <Button @click="saveProfile" :disabled="!displayName.trim() || savingProfile" class="w-full">
                    {{ savingProfile ? 'Saving...' : 'Update Display Name' }}
                </Button>
            </div>

            <!-- Password tab -->
            <div v-if="settingsTab === 'password'" class="space-y-4">
                <div class="space-y-2">
                    <Label for="oldPw">Current Password</Label>
                    <div class="relative">
                        <Input id="oldPw" v-model="oldPassword" :type="showOld ? 'text' : 'password'" placeholder="Enter current password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showOld = !showOld">
                            <EyeOff v-if="showOld" class="w-4 h-4" />
                            <Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                </div>
                <div class="space-y-2">
                    <Label for="newPw">New Password</Label>
                    <div class="relative">
                        <Input id="newPw" v-model="newPassword" :type="showNew ? 'text' : 'password'" placeholder="Enter new password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showNew = !showNew">
                            <EyeOff v-if="showNew" class="w-4 h-4" />
                            <Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                </div>
                <div class="space-y-2">
                    <Label for="confirmPw">Confirm New Password</Label>
                    <div class="relative">
                        <Input id="confirmPw" v-model="confirmPassword" :type="showConfirm ? 'text' : 'password'" placeholder="Confirm new password" class="pr-10" />
                        <button type="button" class="absolute right-3 top-2.5 text-muted-foreground" @click="showConfirm = !showConfirm">
                            <EyeOff v-if="showConfirm" class="w-4 h-4" />
                            <Eye v-else class="w-4 h-4" />
                        </button>
                    </div>
                    <p v-if="confirmPassword && newPassword !== confirmPassword" class="text-xs text-destructive">Passwords do not match</p>
                </div>
                <Button @click="changePassword" :disabled="savingPassword || !oldPassword || !newPassword || newPassword !== confirmPassword" class="w-full">
                    {{ savingPassword ? 'Changing...' : 'Change Password' }}
                </Button>
            </div>

            <!-- Appearance tab -->
            <div v-if="settingsTab === 'appearance'" class="space-y-5">
                <!-- Dark / Light toggle -->
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
                        {{ layoutConfig.darkMode ? 'Light Mode' : 'Dark Mode' }}
                    </button>
                </div>

                <!-- Accent color -->
                <div class="space-y-3">
                    <div>
                        <p class="text-sm font-medium">Accent Color</p>
                        <p class="text-xs text-muted-foreground">Choose the primary accent color for the interface</p>
                    </div>
                    <div class="flex gap-3 flex-wrap">
                        <button
                            v-for="color in accentColors"
                            :key="color.name"
                            @click="setThemeColor(color.name)"
                            class="flex flex-col items-center gap-1.5 group"
                            :title="color.label"
                        >
                            <div
                                class="w-9 h-9 rounded-full flex items-center justify-center ring-2 ring-offset-2 ring-offset-background transition-all"
                                :class="[color.class, layoutConfig.themeColor === color.name ? 'ring-foreground scale-110' : 'ring-transparent']"
                            >
                                <Check v-if="layoutConfig.themeColor === color.name" class="w-4 h-4 text-white" />
                            </div>
                            <span class="text-xs text-muted-foreground group-hover:text-foreground transition-colors">{{ color.label }}</span>
                        </button>
                    </div>
                </div>

                <!-- Menu mode -->
                <div class="space-y-2">
                    <div>
                        <p class="text-sm font-medium">Sidebar Mode</p>
                        <p class="text-xs text-muted-foreground">How the sidebar behaves by default</p>
                    </div>
                    <div class="flex bg-muted p-1 rounded-md w-fit">
                        <button
                            v-for="mode in ['static', 'overlay']"
                            :key="mode"
                            @click="layoutConfig.menuMode = mode"
                            class="px-4 py-1 text-sm font-medium rounded-sm transition-colors capitalize"
                            :class="layoutConfig.menuMode === mode ? 'bg-background text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
                        >
                            {{ mode }}
                        </button>
                    </div>
                </div>
            </div>
        </DialogContent>
    </Dialog>
</template>
