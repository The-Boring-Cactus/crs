<script setup>
import { computed, ref } from 'vue';
import VChart from 'vue-echarts';
// Import core from echarts to avoid full bundle if we want, but since this generic chart handles everything, import all is fine or necessary
import * as echarts from 'echarts/core';
import { LineChart, BarChart, PieChart, ScatterChart, RadarChart, HeatmapChart, FunnelChart, GaugeChart } from 'echarts/charts';
import { TitleComponent, TooltipComponent, GridComponent, DatasetComponent, TransformComponent, LegendComponent, RadarComponent, VisualMapComponent } from 'echarts/components';
import { LabelLayout, UniversalTransition } from 'echarts/features';
import { CanvasRenderer } from 'echarts/renderers';

echarts.use([LineChart, BarChart, PieChart, ScatterChart, RadarChart, HeatmapChart, FunnelChart, GaugeChart, TitleComponent, TooltipComponent, GridComponent, DatasetComponent, TransformComponent, LegendComponent, RadarComponent, VisualMapComponent, LabelLayout, UniversalTransition, CanvasRenderer]);

import { Card, CardHeader, CardContent, CardTitle, CardDescription, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Plus, Minus, RefreshCw, Pause, Play } from 'lucide-vue-next';
import { useLayout } from '@/layout/composables/layout';

const { layoutConfig } = useLayout();

const props = defineProps({
    type: {
        type: String,
        required: true,
        validator: (value) => ['line', 'bar', 'bar-h', 'pie', 'doughnut', 'polarArea', 'radar', 'scatter', 'bubble', 'area', 'mixed', 'waterfall', 'heatmap', 'funnel', 'gauge'].includes(value)
    },
    data: {
        type: Object,
        required: true
    },
    options: {
        type: Object,
        default: () => ({})
    },
    width: {
        type: [String, Number],
        default: '100%'
    },
    height: {
        type: [String, Number],
        default: '400px'
    },
    title: {
        type: String,
        default: ''
    },
    description: {
        type: String,
        default: ''
    },
    showHeader: {
        type: Boolean,
        default: true
    },
    showFooter: {
        type: Boolean,
        default: true
    },
    showControls: {
        type: Boolean,
        default: false
    },
    showLegend: {
        type: Boolean,
        default: true
    },
    responsive: {
        type: Boolean,
        default: true
    },
    maintainAspectRatio: {
        type: Boolean,
        default: false
    },
    animationDuration: {
        type: Number,
        default: 1000
    }
});

const emit = defineEmits(['data-updated', 'chart-clicked', 'chart-created']);

const animationEnabled = ref(true);
const chartRef = ref(null);

const cssHeight = computed(() => {
    if (typeof props.height === 'number') return `${props.height}px`;
    return props.height || '100%';
});

const palettes = {
    indigo: ['#6366f1', '#818cf8', '#a5b4fc', '#c7d2fe', '#4f46e5', '#3730a3', '#4338ca'],
    emerald: ['#10b981', '#34d399', '#6ee7b7', '#a7f3d0', '#059669', '#065f46', '#047857'],
    blue: ['#3b82f6', '#60a5fa', '#93c5fd', '#bfdbfe', '#2563eb', '#1e3a8a', '#1d4ed8'],
    rose: ['#f43f5e', '#fb7185', '#fda4af', '#fecdd3', '#e11d48', '#9f1239', '#be123c'],
    amber: ['#f59e0b', '#fbbf24', '#fcd34d', '#fde68a', '#d97706', '#92400e', '#b45309']
};

const chartOptions = computed(() => {
    const type = props.type;
    const datasets = props.data?.datasets || [];
    const labels = (props.data?.labels || []).map(String);

    const isCategorical = ['line', 'bar', 'bar-h', 'area', 'mixed', 'waterfall'].includes(type) && labels.length > 0;

    const isDark = layoutConfig.darkMode;
    const themeColor = layoutConfig.themeColor || 'indigo';
    const activePalette = palettes[themeColor] || palettes.indigo;

    const textColor = isDark ? '#a1a1aa' : '#3f3f46';
    const axisColor = isDark ? '#27272a' : '#e4e4e7';
    const splitColor = isDark ? '#27272a' : '#f4f4f5';

    let options = {
        color: activePalette,
        textStyle: {
            color: textColor
        },
        title: props.title ? { text: props.title, left: 'center', textStyle: { color: isDark ? '#f4f4f5' : '#18181b' } } : undefined,
        tooltip: {
            trigger: isCategorical ? 'axis' : 'item',
            backgroundColor: isDark ? '#18181b' : '#ffffff',
            borderColor: isDark ? '#27272a' : '#e4e4e7',
            textStyle: { color: textColor }
        },
        legend: {
            show: props.showLegend && datasets.length > 0,
            bottom: 0,
            textStyle: { color: textColor }
        },
        animation: animationEnabled.value,
        animationDuration: props.animationDuration,
        ...props.options // merge specific echart overrides if any
    };

    const commonAxisOptions = {
        axisLine: { lineStyle: { color: axisColor } },
        axisLabel: { color: textColor },
        splitLine: { lineStyle: { color: splitColor } }
    };

    if (type === 'bar-h') {
        // Horizontal bar: swap axes
        options.xAxis = { type: 'value', ...commonAxisOptions };
        options.yAxis = { type: 'category', data: labels, ...commonAxisOptions };
        options.grid = { left: '3%', right: '4%', bottom: props.showLegend ? '15%' : '3%', containLabel: true };
    } else if (isCategorical) {
        options.xAxis = {
            type: 'category',
            data: labels,
            ...commonAxisOptions
        };
        options.yAxis = {
            type: 'value',
            ...commonAxisOptions
        };
        options.grid = {
            left: '3%',
            right: '4%',
            bottom: props.showLegend ? '15%' : '3%',
            containLabel: true
        };
    } else if (['scatter', 'bubble'].includes(type)) {
        options.xAxis = { type: 'value', ...commonAxisOptions };
        options.yAxis = { type: 'value', ...commonAxisOptions };
        options.grid = { left: '3%', right: '4%', bottom: '15%', containLabel: true };
    }

    let series = [];

    if (type === 'radar') {
        const maxVals = [];
        datasets.forEach((ds) => {
            ds.data.forEach((val, i) => {
                if (!maxVals[i] || val > maxVals[i]) maxVals[i] = val;
            });
        });

        options.radar = {
            indicator: labels.length ? labels.map((l, i) => ({ name: l, max: maxVals[i] ? maxVals[i] * 1.2 : undefined })) : [{ name: 'Indicator' }]
        };
    }

    // Waterfall: compute ghost (transparent base) + value series from first dataset
    if (type === 'waterfall' && datasets.length > 0) {
        const vals = datasets[0].data.map(v => Number(v) || 0);
        const ghost = [];
        let cumulative = 0;
        vals.forEach(v => {
            ghost.push(v >= 0 ? cumulative : cumulative + v);
            cumulative += v;
        });
        options.series = [
            {
                name: 'ghost',
                type: 'bar',
                stack: 'waterfall',
                itemStyle: { color: 'transparent', borderColor: 'transparent' },
                emphasis: { itemStyle: { color: 'transparent' } },
                data: ghost
            },
            {
                name: datasets[0].label || 'Value',
                type: 'bar',
                stack: 'waterfall',
                data: vals.map((v, i) => ({
                    value: Math.abs(v),
                    itemStyle: { color: v >= 0 ? '#91cc75' : '#ee6666' }
                })),
                label: { show: true, position: 'top', formatter: (p) => vals[p.dataIndex] >= 0 ? `+${vals[p.dataIndex]}` : `${vals[p.dataIndex]}` }
            }
        ];
        return options;
    }

    // Funnel: label/value pairs from the first dataset, like a pie chart
    if (type === 'funnel' && datasets.length > 0) {
        const ds = datasets[0];
        const color = ds.backgroundColor || activePalette;
        options.tooltip.trigger = 'item';
        options.legend.show = props.showLegend && labels.length > 0;
        options.series = [{
            name: ds.label || 'Value',
            type: 'funnel',
            left: '10%',
            width: '80%',
            top: props.title ? '15%' : '5%',
            bottom: props.showLegend ? '15%' : '5%',
            sort: 'descending',
            gap: 2,
            label: { show: true, position: 'inside', color: '#fff' },
            itemStyle: { borderColor: isDark ? '#18181b' : '#ffffff', borderWidth: 1 },
            data: ds.data.map((val, i) => ({
                value: Number(val) || 0,
                name: labels[i] || `Category ${i + 1}`,
                itemStyle: { color: Array.isArray(color) ? color[i % color.length] : activePalette[i % activePalette.length] }
            }))
        }];
        return options;
    }

    // Gauge: a single KPI reading -- the sum of the first dataset's values, so
    // it works whether the query already aggregates to one row or returns many.
    if (type === 'gauge') {
        const ds = datasets[0];
        const value = (ds?.data || []).reduce((sum, v) => sum + (Number(v) || 0), 0);
        const max = props.options?.gaugeMax ?? Math.max(10, Math.ceil((value * 1.2) / 10) * 10);
        options.tooltip = undefined;
        options.legend.show = false;
        options.series = [{
            type: 'gauge',
            min: 0,
            max,
            radius: '90%',
            progress: { show: true, width: 14 },
            axisLine: { lineStyle: { width: 14 } },
            axisLabel: { color: textColor, fontSize: 10 },
            pointer: { show: true },
            detail: { valueAnimation: true, color: textColor, fontSize: 24, offsetCenter: [0, '70%'] },
            data: [{ value, name: ds?.label || props.title || '' }]
        }];
        return options;
    }

    // Heatmap: expects props.data = { labels (x categories), yLabels (y categories),
    // datasets: [{ data: [[xLabel, yLabel, value], ...] }] } -- see pivotToHeatmapData().
    if (type === 'heatmap') {
        const yLabels = (props.data?.yLabels || []).map(String);
        const cells = datasets[0]?.data || [];
        const values = cells.map(c => Number(c[2]) || 0);
        const min = values.length ? Math.min(...values) : 0;
        const max = values.length ? Math.max(...values) : 1;

        options.xAxis = { type: 'category', data: labels, splitArea: { show: true }, ...commonAxisOptions };
        options.yAxis = { type: 'category', data: yLabels, splitArea: { show: true }, ...commonAxisOptions };
        options.grid = { left: '3%', right: '4%', bottom: '15%', top: props.title ? '15%' : '5%', containLabel: true };
        options.tooltip.trigger = 'item';
        options.legend.show = false;
        options.visualMap = {
            min, max,
            calculable: true,
            orient: 'horizontal',
            left: 'center',
            bottom: 0,
            textStyle: { color: textColor },
            inRange: { color: isDark ? ['#1e3a8a', '#60a5fa', '#fde047'] : ['#e0f2fe', '#0369a1'] }
        };
        options.series = [{
            type: 'heatmap',
            data: cells.map(c => [labels.indexOf(String(c[0])), yLabels.indexOf(String(c[1])), c[2]]),
            label: { show: true, color: textColor },
            emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,0.4)' } }
        }];
        return options;
    }

    datasets.forEach((ds, idx) => {
        let dsType = ds.type || type;
        const color = ds.backgroundColor || activePalette[idx % activePalette.length];
        const lineColor = ds.borderColor || color;
        const pLabel = ds.label || `Series ${idx + 1}`;

        let s = {
            name: pLabel,
            type: dsType === 'area' ? 'line' : (dsType === 'bar-h' ? 'bar' : dsType),
            data: ds.data,
            itemStyle: {}
        };

        if (['line', 'area'].includes(dsType) || (dsType === 'mixed' && !ds.type)) {
            s.type = 'line';
            s.lineStyle = { color: lineColor, width: 2 };
            s.itemStyle = { color: lineColor };
            if (dsType === 'area' || ds.fill) {
                s.areaStyle = {
                    color: Array.isArray(color) ? color[0] : color,
                    opacity: 0.3
                };
            }
            s.symbolSize = 6;
        } else if (dsType === 'bar') {
            s.type = 'bar';
            if (Array.isArray(color)) {
                s.itemStyle = {
                    color: (params) => color[params.dataIndex % color.length]
                };
            } else {
                s.itemStyle = { color: color };
            }
            s.barWidth = '60%';
        } else if (['scatter', 'bubble'].includes(dsType)) {
            s.type = 'scatter';
            if (ds.data[0] && typeof ds.data[0] === 'object') {
                s.data = ds.data.map((d, i) => [d.x !== undefined ? d.x : i, d.y !== undefined ? d.y : d, d.r || 5]);
                s.symbolSize = (data) => data[2] * 2;
            } else {
                s.data = ds.data.map((y, i) => [i, y]);
                s.symbolSize = 10;
            }
            s.itemStyle = { color: Array.isArray(color) ? color[0] : color, opacity: 0.6 };
            if (lineColor) s.itemStyle.borderColor = lineColor;
        } else if (['pie', 'doughnut'].includes(dsType)) {
            s.type = 'pie';
            s.radius = dsType === 'doughnut' ? ['40%', '70%'] : '50%';
            s.data = ds.data.map((val, i) => ({
                value: val,
                name: labels[i] || `Category ${i + 1}`,
                itemStyle: {
                    color: Array.isArray(color) ? color[i % color.length] : color
                }
            }));
            options.tooltip.trigger = 'item';
        } else if (dsType === 'polarArea') {
            s.type = 'pie';
            s.radius = [20, '70%'];
            s.roseType = 'area';
            s.data = ds.data.map((val, i) => ({
                value: val,
                name: labels[i] || `Category ${i + 1}`,
                itemStyle: {
                    color: Array.isArray(color) ? color[i % color.length] : color
                }
            }));
            options.tooltip.trigger = 'item';
        } else if (dsType === 'radar') {
            s.type = 'radar';
            s.data = [
                {
                    value: ds.data,
                    name: pLabel,
                    itemStyle: { color: Array.isArray(color) ? color[0] : color },
                    areaStyle: { color: Array.isArray(color) ? color[0] : color, opacity: 0.3 }
                }
            ];
        }

        series.push(s);
    });

    options.series = series;

    return options;
});

function addRandomData() {
    emit('data-updated', { action: 'add' });
}
function removeData() {
    emit('data-updated', { action: 'remove' });
}
function randomizeData() {
    emit('data-updated', { action: 'randomize' });
}
function toggleAnimation() {
    animationEnabled.value = !animationEnabled.value;
}

function getChartTypeName(type) {
    const names = {
        line: 'Line Chart',
        bar: 'Bar Chart',
        'bar-h': 'Horizontal Bar Chart',
        pie: 'Pie Chart',
        doughnut: 'Doughnut Chart',
        polarArea: 'Polar Area Chart',
        radar: 'Radar Chart',
        scatter: 'Scatter Plot',
        bubble: 'Bubble Chart',
        area: 'Area Chart',
        mixed: 'Mixed Chart',
        waterfall: 'Waterfall Chart',
        heatmap: 'Heatmap',
        funnel: 'Funnel Chart',
        gauge: 'Gauge Chart'
    };
    return names[type] || type;
}

function getChartDescription(type) {
    const descriptions = {
        line: 'Perfect for showing trends over time',
        bar: 'Great for comparing categories',
        pie: 'Ideal for showing proportions of a whole',
        doughnut: 'Similar to pie chart with a hollow center',
        polarArea: 'Combines pie and radar chart features',
        radar: 'Useful for showing multiple metrics',
        scatter: 'Shows correlation between two variables',
        bubble: 'Displays three dimensions of data',
        area: 'Line chart with filled area underneath',
        mixed: 'Combines different chart types',
        heatmap: 'Shows value intensity across two categorical dimensions',
        funnel: 'Visualizes stages of a process narrowing down',
        gauge: 'Shows a single KPI value against a range'
    };
    return descriptions[type] || 'Chart visualization';
}

function onChartReady() {
    emit('chart-created');
}

// Normalizes an ECharts click event (bar/line/area/scatter category click or
// pie/doughnut slice click) into a plain payload for cross-filtering consumers.
function handleChartClick(params) {
    emit('chart-clicked', {
        label: params?.name ?? '',
        value: params?.value,
        seriesName: params?.seriesName,
        dataIndex: params?.dataIndex
    });
}

defineExpose({
    chartInstance: () => chartRef.value,
    addRandomData,
    removeData,
    randomizeData,
    toggleAnimation
});
</script>

<template>
    <Card class="flex flex-col h-full shadow-sm hover:shadow-md transition-shadow bg-card text-card-foreground">
        <CardHeader v-if="showHeader" class="pb-2">
            <div class="flex justify-between items-center w-full">
                <div>
                    <CardTitle>{{ title || getChartTypeName(type) }}</CardTitle>
                    <CardDescription v-if="description">{{ description }}</CardDescription>
                </div>
                <div class="flex gap-2" v-if="showControls">
                    <Button size="sm" variant="secondary" @click="addRandomData" title="Add Data">
                        <Plus class="w-4 h-4" />
                    </Button>
                    <Button size="sm" variant="secondary" @click="removeData" title="Remove Data">
                        <Minus class="w-4 h-4" />
                    </Button>
                    <Button size="sm" variant="secondary" @click="randomizeData" title="Randomize Data">
                        <RefreshCw class="w-4 h-4" />
                    </Button>
                    <Button size="sm" variant="secondary" @click="toggleAnimation" :title="animationEnabled ? 'Disable Animation' : 'Enable Animation'">
                        <Pause v-if="animationEnabled" class="w-4 h-4" />
                        <Play v-else class="w-4 h-4" />
                    </Button>
                </div>
            </div>
        </CardHeader>
        <CardContent class="flex-grow flex items-center justify-center p-4">
            <div class="w-full relative" :style="{ height: cssHeight }">
                <v-chart ref="chartRef" class="w-full h-full min-h-[200px]" :option="chartOptions" :autoresize="true" @ready="onChartReady" @click="handleChartClick" />
            </div>
        </CardContent>
        <CardFooter v-if="showFooter" class="pt-4 border-t flex justify-between text-sm text-muted-foreground">
            <span>{{ getChartDescription(type) }}</span>
            <Badge v-if="data?.datasets?.[0]?.data" variant="secondary">{{ data.datasets[0].data.length }} data points</Badge>
        </CardFooter>
    </Card>
</template>
