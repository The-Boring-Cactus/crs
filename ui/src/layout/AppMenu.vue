<script setup>
import { ref } from 'vue';

import AppMenuItem from './AppMenuItem.vue';

const model = ref([
    {
        label: 'Home',
        items: [{ label: 'Home', icon: 'pi pi-fw pi-home', to: '/' }]
    },
    {
        label: 'Collections',
        icon: 'pi pi-fw pi-briefcase',
        
        items: [
            {
                label: 'Datasets',
                icon: 'pi pi-fw pi-table',
                to: '/pages/dataset'
            },
            {
                label: 'SQL Querys',
                icon: 'pi pi-fw pi-database',
                to: '/pages/sqleditor'
            },
            {
                label: 'Dashboards',
                icon: 'pi pi-fw pi-chart-line',
                to: '/pages/dashboard'
            },
            {
                label: 'Scripts',
                icon: 'pi pi-fw pi-file-edit',
                to: '/pages/cseditor'
            },
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
