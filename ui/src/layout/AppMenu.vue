<script setup>
import { ref } from 'vue';

import AppMenuItem from './AppMenuItem.vue';

const model = ref([
    {
        label: 'Home',
        items: [{ label: 'Home', icon: 'pi pi-fw pi-home', to: '/' }]
    },
    {
        label: 'Admin',
        items: [
            {
                label: 'Database Connection',
                icon: 'pi pi-fw pi-server',
                to: '/pages/databases',
                tooltip: 'Create/Edit Connections'
            }]
    },
    {
        label: 'Collections',
        icon: 'pi pi-fw pi-briefcase',
        
        items: [
            {
                label: 'Datasets',
                icon: 'pi pi-fw pi-table',
                to: '/pages/dataset',
                tooltip: 'Create/Edit Datasets'
            },
            {
                label: 'New Datasets',
                icon: 'pi pi-fw pi-table',
                to: '/pages/myexcel',
                tooltip: 'Create/Edit Datasets'
            },
            {
                label: 'SQL Querys',
                icon: 'pi pi-fw pi-database',
                to: '/pages/sqleditor',
                tooltip: 'Create/Edit SQL Querys'
            },
            {
                label: 'Scripts',
                icon: 'pi pi-fw pi-code',
                to: '/pages/cseditor',
                tooltip: 'Internal Code Editor'
            },
            {
                label: 'Dashboards',
                icon: 'pi pi-fw pi-chart-line',
                to: '/pages/dashboard',
                tooltip: 'Create/Edit Dashboards'
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
