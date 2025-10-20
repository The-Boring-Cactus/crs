<script setup>
  import { nextTick, ref, computed } from 'vue'
import { useLayout } from '@/layout/composables/layout';
import AppConfigurator from './AppConfigurator.vue';
import {userStoreMe} from "@/store/userStore";
import { useRouter } from 'vue-router'

const router = useRouter()
const userStore = userStoreMe();

const { toggleMenu } = useLayout();

const selectedCategory = ref(null)
const exit = async () => {
        userStore.setCurr(false,'','');
        await nextTick();
        await  router.push({ path: '/auth/login', replace: true })
        console.log("Save");
      }



</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/" class="layout-topbar-logo">
                
                <span>μ-Reporter</span>
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
                        <i class="pi pi-palette"></i>
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <button
                class="layout-topbar-menu-button layout-topbar-action"
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            >
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                   
                    <button  
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                 @click="exit">  
                        <i class="pi pi-times-circle"></i>
                        
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
