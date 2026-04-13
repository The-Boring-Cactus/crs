<script setup>
import { computed, ref } from 'vue';
import VChart from 'vue-echarts';
// Import core from echarts to avoid full bundle if we want, but since this generic chart handles everything, import all is fine or necessary
import * as echarts from 'echarts/core';
import { LineChart, BarChart, PieChart, ScatterChart, RadarChart } from 'echarts/charts';
import { TitleComponent, TooltipComponent, GridComponent, DatasetComponent, TransformComponent, LegendComponent, RadarComponent } from 'echarts/components';
import { LabelLayout, UniversalTransition } from 'echarts/features';
import { CanvasRenderer } from 'echarts/renderers';

echarts.use([LineChart, BarChart, PieChart, ScatterChart, RadarChart, TitleComponent, TooltipComponent, GridComponent, DatasetComponent, TransformComponent, LegendComponent, RadarComponent, LabelLayout, UniversalTransition, CanvasRenderer]);

import { Card, CardHeader, CardContent, CardTitle, CardDescription, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Plus, Minus, RefreshCw, Pause, Play } from 'lucide-vue-next';

const props = defineProps({
    type: {
        type: String,
        required: true,
        validator: (value) => ['line', 'bar', 'pie', 'doughnut', 'polarArea', 'radar', 'scatter', 'bubble', 'area', 'mixed'].includes(value)
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

const defaultPalette = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc'];

const chartOptions = computed(() => {
    const type = props.type;
    const datasets = props.data?.datasets || [];
    const labels = (props.data?.labels || []).map(String);

    const isCategorical = ['line', 'bar', 'area', 'mixed'].includes(type) && labels.length > 0;

    let options = {
        title: props.title ? { text: props.title, left: 'center' } : undefined,
        tooltip: {
            trigger: isCategorical ? 'axis' : 'item'
        },
        legend: {
            show: props.showLegend && datasets.length > 0,
            bottom: 0
        },
        animation: animationEnabled.value,
        animationDuration: props.animationDuration,
        ...props.options // merge specific echart overrides if any
    };

    if (isCategorical) {
        options.xAxis = {
            type: 'category',
            data: labels
        };
        options.yAxis = {
            type: 'value'
        };
        options.grid = {
            left: '3%',
            right: '4%',
            bottom: props.showLegend ? '15%' : '3%',
            containLabel: true
        };
    } else if (['scatter', 'bubble'].includes(type)) {
        options.xAxis = { type: 'value' };
        options.yAxis = { type: 'value' };
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

    datasets.forEach((ds, idx) => {
        let dsType = ds.type || type;
        const color = ds.backgroundColor || defaultPalette[idx % defaultPalette.length];
        const lineColor = ds.borderColor || color;
        const pLabel = ds.label || `Series ${idx + 1}`;

        let s = {
            name: pLabel,
            type: dsType === 'area' ? 'line' : dsType,
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
        pie: 'Pie Chart',
        doughnut: 'Doughnut Chart',
        polarArea: 'Polar Area Chart',
        radar: 'Radar Chart',
        scatter: 'Scatter Plot',
        bubble: 'Bubble Chart',
        area: 'Area Chart',
        mixed: 'Mixed Chart'
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
        mixed: 'Combines different chart types'
    };
    return descriptions[type] || 'Chart visualization';
}

function onChartReady() {
    emit('chart-created');
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
                <v-chart ref="chartRef" class="w-full h-full min-h-[200px]" :option="chartOptions" :autoresize="true" @ready="onChartReady" />
            </div>
        </CardContent>
        <CardFooter v-if="showFooter" class="pt-4 border-t flex justify-between text-sm text-muted-foreground">
            <span>{{ getChartDescription(type) }}</span>
            <Badge v-if="data?.datasets?.[0]?.data" variant="secondary">{{ data.datasets[0].data.length }} data points</Badge>
        </CardFooter>
    </Card>
</template>
