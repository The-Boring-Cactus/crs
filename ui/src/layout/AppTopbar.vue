<script setup>
import { nextTick, ref, computed } from 'vue';
import { useLayout } from '@/layout/composables/layout';
import AppConfigurator from './AppConfigurator.vue';
import { userStoreMe } from '@/store/userStore';
import { useRouter } from 'vue-router';
import { Menu, Palette, MoreVertical, XCircle } from 'lucide-vue-next';

const router = useRouter();
const userStore = userStoreMe();

const { toggleMenu } = useLayout();

const selectedCategory = ref(null);
const exit = async () => {
    userStore.setCurr(false, '', '');
    await nextTick();
    await router.push({ path: '/auth/login', replace: true });
    console.log('Save');
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
            <div class="layout-config-menu">
                <div class="relative">
                    <button
                        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                    >
                        <Palette class="w-5 h-5 text-zinc-500" />
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <button
                class="layout-topbar-menu-button layout-topbar-action"
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            >
                <MoreVertical class="w-5 h-5 text-zinc-500" />
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <button type="button" class="layout-topbar-action layout-topbar-action-highlight" @click="exit">
                        <XCircle class="w-5 h-5 text-zinc-500" />
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
