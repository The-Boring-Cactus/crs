<script setup>
import { ref, markRaw } from 'vue';
import AppMenuItem from './AppMenuItem.vue';
import { Home, Server, Briefcase, Table, Database, Code, LineChart } from 'lucide-vue-next';

const model = ref([
    {
        label: 'Home',
        items: [{ label: 'Home', icon: markRaw(Home), to: '/' }]
    },
    {
        label: 'Admin',
        items: [
            {
                label: 'Database Connection',
                icon: markRaw(Server),
                to: '/pages/databases',
                tooltip: 'Create/Edit Connections'
            }
        ]
    },
    {
        label: 'Collections',
        icon: markRaw(Briefcase),

        items: [
            {
                label: 'Datasets',
                icon: markRaw(Table),
                to: '/pages/dataset',
                tooltip: 'Create/Edit Datasets'
            },
            {
                label: 'New Datasets',
                icon: markRaw(Table),
                to: '/pages/myexcel',
                tooltip: 'Create/Edit Datasets'
            },
            {
                label: 'SQL Querys',
                icon: markRaw(Database),
                to: '/pages/sqleditor',
                tooltip: 'Create/Edit SQL Querys'
            },
            {
                label: 'Scripts',
                icon: markRaw(Code),
                to: '/pages/cseditor',
                tooltip: 'Internal Code Editor'
            },
            {
                label: 'Dashboards',
                icon: markRaw(LineChart),
                to: '/pages/dashboard',
                tooltip: 'Create/Edit Dashboards'
            }
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
