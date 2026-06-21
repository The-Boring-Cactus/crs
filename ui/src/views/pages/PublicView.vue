<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import BaseChart from '@/components/BaseChart.vue';
import GridLayout from '@/components/draggable/GridLayout.vue';
import GridItem from '@/components/draggable/GridItem.vue';
import { BarChart2, LayoutDashboard, AlertCircle } from 'lucide-vue-next';

const route = useRoute();
const loading = ref(true);
const error = ref('');
const dashboard = ref(null);
const components = ref([]);

const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:9876';

const isChartType = (type) =>
    ['LineChart', 'BarChart', 'AreaChart', 'PieChart', 'DoughnutChart',
     'PolarAreaChart', 'RadarChart', 'ScatterChart', 'BubbleChart', 'MixedChart'].includes(type);

const chartTypeFor = (type) => ({
    LineChart: 'line', BarChart: 'bar', AreaChart: 'area',
    PieChart: 'pie', DoughnutChart: 'doughnut', PolarAreaChart: 'polarArea',
    RadarChart: 'radar', ScatterChart: 'scatter', BubbleChart: 'bubble'
})[type] || 'bar';

onMounted(async () => {
    const token = route.params.shareToken;
    try {
        const resp = await fetch(`${apiUrl}/api/public/dashboard/${token}`);
        if (!resp.ok) {
            error.value = resp.status === 404 ? 'This dashboard is not available or the link has expired.' : 'Failed to load dashboard.';
            return;
        }
        const data = await resp.json();
        dashboard.value = data;
        const config = typeof data.config === 'string' ? JSON.parse(data.config) : data.config;
        components.value = config?.components || [];
    } catch (e) {
        error.value = 'Failed to load dashboard.';
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="min-h-screen bg-background">
        <!-- Header -->
        <header class="border-b bg-card px-6 py-3 flex items-center gap-3">
            <BarChart2 class="w-6 h-6 text-primary" />
            <span class="font-bold text-lg">CRS Reporter</span>
            <span v-if="dashboard" class="text-muted-foreground mx-2">·</span>
            <span v-if="dashboard" class="font-medium">{{ dashboard.name }}</span>
            <span class="ml-auto text-xs text-muted-foreground bg-muted px-2 py-1 rounded">Public View</span>
        </header>

        <!-- Loading -->
        <div v-if="loading" class="flex items-center justify-center h-64">
            <div class="animate-spin w-8 h-8 border-2 border-primary border-t-transparent rounded-full"></div>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="flex flex-col items-center justify-center h-64 gap-4 text-muted-foreground">
            <AlertCircle class="w-12 h-12 opacity-40" />
            <p class="text-lg">{{ error }}</p>
        </div>

        <!-- Empty -->
        <div v-else-if="!components.length" class="flex flex-col items-center justify-center h-64 gap-4 text-muted-foreground">
            <LayoutDashboard class="w-12 h-12 opacity-40" />
            <p>This dashboard has no widgets.</p>
        </div>

        <!-- Dashboard (read-only grid) -->
        <div v-else class="p-4">
            <grid-layout
                v-model:layout="components"
                :col-num="15"
                :row-height="40"
                :is-draggable="false"
                :is-resizable="false"
                :auto-size="true"
                use-css-transforms
            >
                <grid-item
                    v-for="item in components"
                    :key="item.i"
                    :x="item.x" :y="item.y" :w="item.w" :h="item.h" :i="item.i"
                    :static="true"
                    class="grid-item-container"
                >
                    <!-- Text -->
                    <div v-if="item.type === 'Text'" class="w-full h-full flex items-center justify-center p-2">
                        <span class="text-lg">{{ item.value }}</span>
                    </div>

                    <!-- Charts -->
                    <div v-else-if="isChartType(item.type)" class="chart-container flex flex-col h-full border rounded-md p-2 bg-card">
                        <div class="font-medium text-sm mb-1">{{ item.title }}</div>
                        <div class="flex-1 min-h-0">
                            <BaseChart
                                :type="chartTypeFor(item.type)"
                                :data="item.chartData"
                                :title="item.title"
                                :show-header="false"
                                :show-footer="false"
                                height="100%"
                            />
                        </div>
                    </div>

                    <!-- DataTable -->
                    <div v-else-if="item.type === 'DataTable'" class="h-full border rounded-md overflow-auto bg-card">
                        <div class="p-2 font-medium text-sm border-b">{{ item.title }}</div>
                        <table class="w-full text-xs">
                            <thead>
                                <tr>
                                    <th v-for="col in item.columns" :key="col.field" class="text-left px-3 py-2 bg-muted font-medium">{{ col.header }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, ri) in item.tableData" :key="ri" class="border-t">
                                    <td v-for="col in item.columns" :key="col.field" class="px-3 py-2">{{ row[col.field] }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- Fallback -->
                    <div v-else class="w-full h-full flex items-center justify-center text-muted-foreground text-sm border rounded-md">
                        {{ item.type }}
                    </div>
                </grid-item>
            </grid-layout>
        </div>
    </div>
</template>

<style scoped>
.grid-item-container {
    background: transparent;
}
</style>
