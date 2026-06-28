<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import BaseChart from '@/components/BaseChart.vue';
import GridLayout from '@/components/draggable/GridLayout.vue';
import GridItem from '@/components/draggable/GridItem.vue';
import { BarChart2, LayoutDashboard, AlertCircle } from 'lucide-vue-next';
import { useVariableStore } from '@/store/variableStore';

const route = useRoute();
const loading = ref(true);
const error = ref('');
const dashboard = ref(null);
const components = ref([]);
const variableStore = useVariableStore();

// Only pass items that are valid grid objects to the layout engine.
// Using :layout (not v-model:layout) since the view is read-only — v-model on a
// computed causes an infinite reactive loop when GridLayout emits layout updates.
const validComponents = computed(() =>
    components.value.filter(item =>
        item &&
        typeof item === 'object' &&
        !Array.isArray(item) &&
        typeof item.x === 'number' &&
        typeof item.y === 'number' &&
        typeof item.w === 'number' &&
        typeof item.h === 'number' &&
        item.i !== undefined
    )
);

const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:9876';

const isChartType = (type) =>
    ['LineChart', 'BarChart', 'AreaChart', 'PieChart', 'DoughnutChart',
     'PolarAreaChart', 'RadarChart', 'ScatterChart', 'BubbleChart', 'MixedChart'].includes(type);

const chartTypeFor = (type) => ({
    LineChart: 'line', BarChart: 'bar', AreaChart: 'area',
    PieChart: 'pie', DoughnutChart: 'doughnut', PolarAreaChart: 'polarArea',
    RadarChart: 'radar', ScatterChart: 'scatter', BubbleChart: 'bubble'
})[type] || 'bar';

// ── SQL widget helpers (mirrors Dashboard.vue — read-only, uses stored queryResults) ──

function getSqlWidgetViz(item) {
    try { return JSON.parse(item.visualization || '{"type":"table"}'); }
    catch { return { type: 'table' }; }
}

function getSqlWidgetChartData(item) {
    const viz = getSqlWidgetViz(item);
    const rows = item.queryResults || [];
    const columns = item.queryColumns || [];
    if (!rows.length || !columns.length) return null;

    const labelCol = viz.labelColumn || columns[0]?.field || '';
    const valueCols = (viz.valueColumns || []).length > 0
        ? viz.valueColumns
        : columns.filter(c => c.field !== labelCol).slice(0, 4).map(c => c.field);
    if (!valueCols.length) return null;

    const palette = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272'];
    const vizType = viz.type;

    if (vizType === 'pie') {
        const col = valueCols[0] || labelCol;
        return {
            labels: rows.map(r => String(r[labelCol] ?? '')),
            datasets: [{ label: col, data: rows.map(r => Number(r[col]) || 0), backgroundColor: palette, borderColor: palette, borderWidth: 1 }]
        };
    }

    const labels = rows.map(r => String(r[labelCol] ?? ''));
    const datasets = valueCols.map((col, i) => ({
        label: columns.find(c => c.field === col)?.header || col,
        data: rows.map(r => Number(r[col]) || 0),
        backgroundColor: palette[i % palette.length] + 'bb',
        borderColor: palette[i % palette.length],
        borderWidth: 1,
        fill: vizType === 'area'
    }));
    return { labels, datasets };
}

function getSqlWidgetPivotData(item) {
    const viz = getSqlWidgetViz(item);
    const rows = item.queryResults || [];
    const { pivotRowField, pivotColField, pivotValueField, pivotAggregation = 'sum' } = viz;
    if (!rows.length || !pivotRowField || !pivotColField || !pivotValueField) return null;

    const colValues = [...new Set(rows.map(r => String(r[pivotColField] ?? '')))].sort();
    const rowValues = [...new Set(rows.map(r => String(r[pivotRowField] ?? '')))].sort();

    const agg = (matchingRows) => {
        if (!matchingRows.length) return null;
        const nums = matchingRows.map(r => Number(r[pivotValueField]) || 0);
        switch (pivotAggregation) {
            case 'avg': return +(nums.reduce((a, b) => a + b, 0) / nums.length).toFixed(2);
            case 'count': return matchingRows.length;
            case 'min': return Math.min(...nums);
            case 'max': return Math.max(...nums);
            default: return nums.reduce((a, b) => a + b, 0);
        }
    };

    return {
        columns: colValues,
        rows: rowValues.map(rowVal => {
            const values = {};
            for (const colVal of colValues) {
                const matching = rows.filter(r =>
                    String(r[pivotRowField] ?? '') === rowVal &&
                    String(r[pivotColField] ?? '') === colVal
                );
                values[colVal] = agg(matching);
            }
            return { label: rowVal, values };
        })
    };
}

onMounted(async () => {
    const token = route.params.shareToken;
    try {
        const resp = await fetch(`${apiUrl}/api/public/dashboard/${token}`);
        if (!resp.ok) {
            error.value = resp.status === 404
                ? 'This dashboard is not available or the link has expired.'
                : 'Failed to load dashboard.';
            return;
        }
        const data = await resp.json();
        dashboard.value = data;
        const config = typeof data.config === 'string' ? JSON.parse(data.config) : data.config;
        components.value = config?.components || [];
        variableStore.loadFromStorage();
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
        <div v-else-if="!validComponents.length" class="flex flex-col items-center justify-center h-64 gap-4 text-muted-foreground">
            <LayoutDashboard class="w-12 h-12 opacity-40" />
            <p>This dashboard has no widgets.</p>
        </div>

        <!-- Dashboard (read-only grid) -->
        <!-- :layout not v-model:layout — GridLayout emits layout updates that would recurse on a computed -->
        <div v-else class="p-4">
            <grid-layout
                :layout="validComponents"
                :col-num="15"
                :row-height="40"
                :is-draggable="false"
                :is-resizable="false"
                :auto-size="true"
                use-css-transforms
            >
                <grid-item
                    v-for="item in validComponents"
                    :key="item.i"
                    :x="item.x" :y="item.y" :w="item.w" :h="item.h" :i="item.i"
                    :static="true"
                    class="grid-item-container"
                >
                    <!-- Text -->
                    <div v-if="item.type === 'Text'" class="w-full h-full flex items-center justify-center p-2">
                        <span class="text-lg">{{ item.value }}</span>
                    </div>

                    <!-- Static Charts (chartData stored at save time) -->
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

                    <!-- DataTable (tableData stored at save time) -->
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

                    <!-- SqlWidget: displays stored queryResults with the configured visualization -->
                    <div v-else-if="item.type === 'SqlWidget'" class="flex flex-col h-full border rounded-md p-2 bg-card">
                        <div class="font-medium text-sm mb-2 truncate">{{ item.title || item.sqlScriptName || 'SQL Query' }}</div>
                        <div class="flex-1 overflow-auto min-h-0">
                            <!-- Table view -->
                            <div v-if="getSqlWidgetViz(item).type === 'table'" class="h-full overflow-auto border rounded-md bg-background">
                                <table v-if="item.queryResults?.length" class="text-xs w-full border-collapse">
                                    <thead class="sticky top-0 bg-secondary z-10">
                                        <tr>
                                            <th v-for="col in item.queryColumns" :key="col.field" class="border border-border px-2 py-1 text-left font-semibold whitespace-nowrap">{{ col.header }}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="(row, ri) in item.queryResults" :key="ri" class="hover:bg-muted/40">
                                            <td v-for="col in item.queryColumns" :key="col.field" class="border border-border px-2 py-1">{{ row[col.field] ?? 'NULL' }}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div v-else class="flex items-center justify-center h-full text-muted-foreground text-xs p-4">No data available</div>
                            </div>

                            <!-- Pivot view -->
                            <div v-else-if="getSqlWidgetViz(item).type === 'pivot'" class="h-full overflow-auto border rounded-md bg-background">
                                <table v-if="getSqlWidgetPivotData(item)" class="text-xs w-full border-collapse">
                                    <thead class="sticky top-0 bg-secondary z-10">
                                        <tr>
                                            <th class="border border-border px-2 py-1 text-left font-semibold">Row</th>
                                            <th v-for="col in getSqlWidgetPivotData(item).columns" :key="col" class="border border-border px-2 py-1 text-center font-semibold">{{ col }}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="row in getSqlWidgetPivotData(item).rows" :key="row.label" class="hover:bg-muted/40">
                                            <td class="border border-border px-2 py-1 font-medium">{{ row.label }}</td>
                                            <td v-for="col in getSqlWidgetPivotData(item).columns" :key="col" class="border border-border px-2 py-1 text-right">{{ row.values[col] !== null ? row.values[col] : '—' }}</td>
                                        </tr>
                                    </tbody>
                                </table>
                                <div v-else class="flex items-center justify-center h-full text-muted-foreground text-xs p-4">No pivot data</div>
                            </div>

                            <!-- Chart view -->
                            <div v-else class="h-full flex items-center justify-center">
                                <BaseChart
                                    v-if="getSqlWidgetChartData(item)"
                                    :type="getSqlWidgetViz(item).type || 'bar'"
                                    :data="getSqlWidgetChartData(item)"
                                    :show-header="false"
                                    :show-footer="false"
                                    :show-controls="false"
                                    :show-legend="true"
                                    height="100%"
                                    class="w-full"
                                />
                                <div v-else class="text-muted-foreground text-xs text-center p-4">No chart data</div>
                            </div>
                        </div>
                    </div>

                    <!-- Variable / KPI widget -->
                    <div v-else-if="item.type === 'Variable'" class="flex flex-col h-full border rounded-md p-3 bg-card">
                        <div class="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                            {{ item.label || 'Value' }}
                        </div>
                        <div class="flex-1 flex flex-col items-center justify-center gap-1">
                            <div class="tabular-nums font-bold leading-none" :class="item.h >= 4 ? 'text-4xl' : 'text-2xl'">
                                {{ item.value !== undefined && item.value !== null && item.value !== '' ? item.value : '—' }}
                                <span v-if="item.unit" class="text-base font-normal text-muted-foreground ml-1">{{ item.unit }}</span>
                            </div>
                            <p v-if="item.description" class="text-xs text-muted-foreground text-center">{{ item.description }}</p>
                        </div>
                    </div>

                    <!-- FunctEngine Output widget (displays stored output) -->
                    <div v-else-if="item.type === 'FunctOutput'" class="flex flex-col h-full border rounded-md p-2 bg-card">
                        <div class="font-medium text-sm mb-2">{{ item.title || 'Script Output' }}</div>
                        <div class="flex-1 overflow-auto min-h-0">
                            <!-- Chart output -->
                            <div v-if="item.outputType === 'chart' && item.chartData" class="h-full">
                                <BaseChart
                                    :type="item.chartType || 'bar'"
                                    :data="item.chartData"
                                    :show-header="false"
                                    :show-footer="false"
                                    :show-controls="false"
                                    :show-legend="true"
                                    height="100%"
                                    class="w-full"
                                />
                            </div>
                            <!-- Table output -->
                            <div v-else-if="item.outputType === 'table'" class="h-full overflow-auto border rounded-md bg-background">
                                <table v-if="item.tableData?.length" class="text-xs w-full border-collapse">
                                    <thead class="sticky top-0 bg-secondary z-10">
                                        <tr>
                                            <th v-for="col in item.tableColumns" :key="col.field" class="border border-border px-2 py-1 text-left font-semibold">{{ col.header }}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="(row, ri) in item.tableData" :key="ri" class="hover:bg-muted/40">
                                            <td v-for="col in item.tableColumns" :key="col.field" class="border border-border px-2 py-1">{{ row[col.field] }}</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <!-- StatReport preview -->
                            <div v-else-if="item.outputType === 'statreport' && item.statReportData" class="text-xs space-y-2 p-1 overflow-auto h-full">
                                <h4 class="font-semibold text-sm border-b pb-1">{{ item.statReportData.title }}</h4>
                                <div v-for="(section, si) in (item.statReportData.sections || [])" :key="si" class="space-y-1">
                                    <p v-if="section.heading" class="font-medium text-muted-foreground">{{ section.heading }}</p>
                                    <div v-if="section.type === 'table'" class="overflow-x-auto">
                                        <table class="border-collapse w-full">
                                            <thead><tr><th v-for="h in section.headers" :key="h" class="px-1 py-0.5 bg-muted border text-left">{{ h }}</th></tr></thead>
                                            <tbody><tr v-for="(row, ri) in section.rows" :key="ri" class="border-t"><td v-for="(cell, ci) in row" :key="ci" class="px-1 py-0.5 border">{{ typeof cell === 'number' ? cell.toFixed(3) : cell }}</td></tr></tbody>
                                        </table>
                                    </div>
                                    <p v-else class="text-muted-foreground whitespace-pre-wrap">{{ section.content || section.text }}</p>
                                </div>
                            </div>
                            <!-- No output stored -->
                            <div v-else class="flex items-center justify-center h-full text-muted-foreground text-xs">No output data</div>
                        </div>
                    </div>

                    <!-- InputText (editable when bound to variable) -->
                    <div v-else-if="item.type === 'InputText'" class="flex flex-col h-full border rounded-md p-2 bg-card">
                        <div class="font-medium text-sm mb-2">{{ item.title || 'Input' }}</div>
                        <div v-if="item.boundVariable" class="flex flex-col gap-1 flex-1 justify-center">
                            <label class="text-xs text-muted-foreground">{{ item.boundVariable }}</label>
                            <input
                                class="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                                :value="variableStore.getValue(item.boundVariable)"
                                :placeholder="item.placeholder"
                                @input="e => variableStore.setValue(item.boundVariable, e.target.value)"
                            />
                        </div>
                        <div v-else class="flex-1 flex items-center justify-center text-sm text-muted-foreground">{{ item.value }}</div>
                    </div>

                    <!-- Select (editable when bound to variable) -->
                    <div v-else-if="item.type === 'Select'" class="flex flex-col h-full border rounded-md p-2 bg-card">
                        <div class="font-medium text-sm mb-2">{{ item.title || 'Select' }}</div>
                        <div v-if="item.boundVariable" class="flex flex-col gap-1 flex-1 justify-center">
                            <label class="text-xs text-muted-foreground">{{ item.boundVariable }}</label>
                            <select
                                class="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                                :value="variableStore.getValue(item.boundVariable)"
                                @change="e => variableStore.setValue(item.boundVariable, e.target.value)"
                            >
                                <option value="" disabled v-if="item.placeholder">{{ item.placeholder }}</option>
                                <option v-for="opt in item.options" :key="opt[item.optionValue || 'value'] || opt" :value="opt[item.optionValue || 'value'] || opt">{{ opt[item.optionLabel || 'label'] || opt }}</option>
                            </select>
                        </div>
                        <div v-else class="flex-1 flex items-center justify-center text-sm text-muted-foreground">{{ item.selectedValue || item.placeholder }}</div>
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
