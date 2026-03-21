<script setup>
import { computed, ref, watch } from 'vue';
import { Chart as ChartJS, Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, PointElement, LineElement, ArcElement, RadialLinearScale, Filler } from 'chart.js';
import { Line, Bar, Pie, Doughnut, Radar, PolarArea, Scatter, Bubble } from 'vue-chartjs';
import { Card, CardHeader, CardContent, CardTitle, CardDescription, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Plus, Minus, RefreshCw, Pause, Play } from 'lucide-vue-next';

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, PointElement, LineElement, ArcElement, RadialLinearScale, Filler);

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

const emit = defineEmits(['chart-created', 'data-updated', 'chart-clicked']);

const animationEnabled = ref(true);
const chartRef = ref(null);

const chartComponent = computed(() => {
    switch (props.type) {
        case 'line':
        case 'area':
            return Line;
        case 'bar':
        case 'mixed':
            return Bar;
        case 'pie':
            return Pie;
        case 'doughnut':
            return Doughnut;
        case 'radar':
            return Radar;
        case 'polarArea':
            return PolarArea;
        case 'scatter':
            return Scatter;
        case 'bubble':
            return Bubble;
        default:
            return Bar;
    }
});

const chartData = computed(() => {
    const data = JSON.parse(JSON.stringify(props.data));
    if (props.type === 'area') {
        data.datasets.forEach(ds => ds.fill = true);
    }
    return data;
});

const chartOptions = computed(() => {
    return {
        responsive: props.responsive,
        maintainAspectRatio: props.maintainAspectRatio,
        animation: {
            duration: animationEnabled.value ? props.animationDuration : 0
        },
        plugins: {
            legend: {
                display: props.showLegend,
                position: 'top',
                labels: { color: 'var(--foreground)' }
            },
            title: {
                display: !!props.title,
                text: props.title,
                color: 'var(--foreground)'
            },
            tooltip: {
                backgroundColor: 'hsl(var(--card))',
                titleColor: 'hsl(var(--card-foreground))',
                bodyColor: 'hsl(var(--card-foreground))',
                borderColor: 'hsl(var(--border))',
                borderWidth: 1
            }
        },
        scales: props.type !== 'pie' && props.type !== 'doughnut' && props.type !== 'polarArea' && props.type !== 'radar' ? {
            x: {
                grid: { color: 'hsl(var(--border))' },
                ticks: { color: 'hsl(var(--muted-foreground))' }
            },
            y: {
                beginAtZero: true,
                grid: { color: 'hsl(var(--border))' },
                ticks: { color: 'hsl(var(--muted-foreground))' }
            }
        } : undefined,
        onClick: (event, elements) => {
            if (elements.length > 0) {
                const element = elements[0];
                emit('chart-clicked', {
                    event,
                    element,
                    datasetIndex: element.datasetIndex,
                    dataIndex: element.index,
                    data: chartData.value.datasets[element.datasetIndex].data[element.index]
                });
            }
        },
        ...props.options
    };
});

// Emulate old control requests
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
        line: 'Line Chart', bar: 'Bar Chart', pie: 'Pie Chart', doughnut: 'Doughnut Chart',
        polarArea: 'Polar Area Chart', radar: 'Radar Chart', scatter: 'Scatter Plot', bubble: 'Bubble Chart',
        area: 'Area Chart', mixed: 'Mixed Chart'
    };
    return names[type] || type;
}

function getChartDescription(type) {
    const descriptions = {
        line: 'Perfect for showing trends over time', bar: 'Great for comparing categories',
        pie: 'Ideal for showing proportions of a whole', doughnut: 'Similar to pie chart with a hollow center',
        polarArea: 'Combines pie and radar chart features', radar: 'Useful for showing multiple metrics',
        scatter: 'Shows correlation between two variables', bubble: 'Displays three dimensions of data',
        area: 'Line chart with filled area underneath', mixed: 'Combines different chart types'
    };
    return descriptions[type] || 'Chart visualization';
}

defineExpose({
    chartInstance: () => chartRef.value?.chart,
    addRandomData,
    removeData,
    randomizeData,
    toggleAnimation
});
</script>

<template>
    <Card class="flex flex-col h-full shadow-sm hover:shadow-md transition-shadow">
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
            <div class="w-full relative" :style="{ height: height }">
                <component 
                    :is="chartComponent" 
                    :data="chartData" 
                    :options="chartOptions" 
                    ref="chartRef" 
                />
            </div>
        </CardContent>
        <CardFooter v-if="showFooter" class="pt-4 border-t flex justify-between text-sm text-muted-foreground">
            <span>{{ getChartDescription(type) }}</span>
            <Badge v-if="chartData?.datasets?.[0]?.data" variant="secondary">{{ chartData.datasets[0].data.length }} data points</Badge>
        </CardFooter>
    </Card>
</template>
