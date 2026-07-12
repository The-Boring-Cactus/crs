<script setup>
import { ref, watch, onMounted, onUnmounted, nextTick, computed } from 'vue';
import { Card, CardHeader, CardContent, CardTitle, CardDescription, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Plus, Minus, RefreshCw, Pause, Play, Maximize2 } from 'lucide-vue-next';
import { useLayout } from '@/layout/composables/layout';
import elementResizeDetectorMaker from 'element-resize-detector';
import { applyBokehDarkTheme } from '@/helpers/bokehUtils';
import { loadBokeh } from '@/composables/useBokeh';

const { layoutConfig } = useLayout();

const props = defineProps({
    bokehJson: {
        type: Object,
        default: null
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
        default: false
    },
    showControls: {
        type: Boolean,
        default: false
    },
    responsive: {
        type: Boolean,
        default: true
    }
});

const emit = defineEmits(['chart-created', 'data-updated']);

const containerRef = ref(null);
const plotRootRef = ref(null);
const bokehReady = ref(false);
const bokehLoading = ref(false);
const bokehError = ref(null);
const animationEnabled = ref(true);
const plotItem = ref(null);

// Bokeh's embed_item() targets a DOM element by id — must be unique per
// instance since a page can host multiple BokehChart widgets at once
// (e.g. several Dashboard chart widgets rendered simultaneously). A plain
// incrementing counter doesn't work here: <script setup> top-level code
// runs once per component *instance*, not once per module, so it would
// reset to the same value for every instance.
const plotRootId = `bokeh-plot-root-${crypto.randomUUID()}`;

const cssHeight = computed(() => {
    if (typeof props.height === 'number') return `${props.height}px`;
    return props.height || '400px';
});

// Render the Bokeh plot from JSON
async function renderPlot() {
    if (!props.bokehJson) {
        destroyPlot();
        return;
    }

    bokehLoading.value = true;
    bokehError.value = null;

    try {
        const Bokeh = await loadBokeh();

        // Destroy previous plot if exists
        destroyPlot();

        await nextTick();

        if (!plotRootRef.value) {
            throw new Error('Plot container not found in DOM');
        }

        const plotData = JSON.parse(JSON.stringify(props.bokehJson)); // deep clone

        // Apply dark/light theme awareness to the plot
        if (layoutConfig.darkMode) {
            applyBokehDarkTheme(plotData);
        }

        // Embed the item into this instance's uniquely-id'd container
        const result = await Bokeh.embed.embed_item(plotData, plotRootId);
        plotItem.value = result;

        bokehReady.value = true;
        emit('chart-created', { item: plotItem.value });
    } catch (err) {
        console.error('BokehChart render error:', err);
        bokehError.value = err.message || 'Failed to render Bokeh plot';
        bokehReady.value = false;
    } finally {
        bokehLoading.value = false;
    }
}

// Destroy the current Bokeh plot
function destroyPlot() {
    if (plotRootRef.value) {
        // Bokeh stores views on the document; clear them
        plotRootRef.value.innerHTML = '';
    }
    if (plotItem.value) {
        try {
            // Attempt to clean up Bokeh views
            if (plotItem.value.clear && typeof plotItem.value.clear === 'function') {
                plotItem.value.clear();
            }
        } catch (e) {
            // ignore cleanup errors
        }
        plotItem.value = null;
    }
    bokehReady.value = false;
}

// Resize detector
let erdInstance = null;

function setupResizeDetection() {
    if (!props.responsive || !containerRef.value) return;

    erdInstance = elementResizeDetectorMaker({
        strategy: 'scroll',
        callOnAdd: false
    });

    erdInstance.listenTo(containerRef.value, () => {
        resizePlot();
    });
}

function resizePlot() {
    if (!plotItem.value) return;
    try {
        // Notify all Bokeh views to recompute layout
        const views = plotItem.value;
        if (Array.isArray(views)) {
            views.forEach(view => {
                if (view && view.resize) view.resize();
            });
        } else if (views && views.resize) {
            views.resize();
        }
    } catch (e) {
        // non-critical
    }
}

// Exposed methods for parent
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

function chartDescription(type) {
    return 'Interactive BokehJS-powered visualization';
}

defineExpose({
    addRandomData,
    removeData,
    randomizeData,
    toggleAnimation,
    refreshPlot: renderPlot
});

// Watch for bokehJson changes and re-render
watch(() => props.bokehJson, () => {
    renderPlot();
}, { deep: true });

// Watch for dark mode changes
watch(() => layoutConfig.darkMode, () => {
    if (props.bokehJson) renderPlot();
});

onMounted(() => {
    setupResizeDetection();
    if (props.bokehJson) {
        renderPlot();
    }
});

onUnmounted(() => {
    destroyPlot();
    if (erdInstance && containerRef.value) {
        erdInstance.removeListener(containerRef.value, () => {});
    }
});
</script>

<template>
    <Card class="flex flex-col h-full shadow-sm hover:shadow-md transition-shadow bg-card text-card-foreground">
        <CardHeader v-if="showHeader" class="pb-2">
            <div class="flex justify-between items-center w-full">
                <div>
                    <CardTitle>{{ title || 'Bokeh Chart' }}</CardTitle>
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
            <div ref="containerRef" class="w-full relative overflow-hidden" :style="{ height: cssHeight }">
                <!-- Loading state -->
                <div v-if="bokehLoading" class="absolute inset-0 flex items-center justify-center bg-background/50 z-10">
                    <div class="flex flex-col items-center gap-2">
                        <RefreshCw class="w-6 h-6 animate-spin text-primary" />
                        <span class="text-sm text-muted-foreground">Loading BokehJS...</span>
                    </div>
                </div>

                <!-- Error state -->
                <div v-else-if="bokehError" class="absolute inset-0 flex items-center justify-center z-10">
                    <div class="flex flex-col items-center gap-2 text-center p-4">
                        <span class="text-sm text-destructive font-medium">Render Error</span>
                        <span class="text-xs text-muted-foreground max-w-xs">{{ bokehError }}</span>
                        <Button variant="outline" size="sm" @click="renderPlot" class="mt-2">
                            <RefreshCw class="w-3 h-3 mr-1" /> Retry
                        </Button>
                    </div>
                </div>

                <!-- Empty state -->
                <div v-else-if="!bokehJson" class="absolute inset-0 flex items-center justify-center">
                    <div class="flex flex-col items-center gap-2 text-center p-4">
                        <Maximize2 class="w-8 h-8 text-muted-foreground/30" />
                        <span class="text-sm text-muted-foreground">Provide <code class="bg-muted px-1 rounded text-xs">bokehJson</code> prop to render a Bokeh plot</span>
                    </div>
                </div>

                <!-- Plot root container -->
                <div
                    ref="plotRootRef"
                    :id="plotRootId"
                    class="w-full h-full bokeh-plot-container"
                    :class="{ 'bokeh-dark': layoutConfig.darkMode }"
                ></div>
            </div>
        </CardContent>
        <CardFooter v-if="showFooter" class="pt-4 border-t flex justify-between text-sm text-muted-foreground">
            <span>{{ chartDescription('bokeh') }}</span>
            <Badge v-if="bokehReady" variant="secondary">BokehJS 3.9</Badge>
        </CardFooter>
    </Card>
</template>

<style scoped>
.bokeh-plot-container {
    min-height: 200px;
}

/* Ensure Bokeh toolbars are visible */
.bokeh-plot-container :deep(.bk-toolbar) {
    background: transparent !important;
    border: none !important;
}

.bokeh-plot-container :deep(.bk-root) {
    width: 100% !important;
    height: 100% !important;
}

/* Dark mode adjustments */
.bokeh-dark :deep(.bk) {
    color-scheme: dark;
}
</style>
