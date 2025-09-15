<template>
  <Card class="chart-card">
    <template #header v-if="showHeader">
      <div class="flex justify-content-between align-items-center p-4">
        <div>
          <h3 class="m-0">{{ title || getChartTypeName(type) }}</h3>
          <p v-if="description" class="text-muted-color text-sm m-0 mt-1">{{ description }}</p>
        </div>
        <div class="flex gap-2" v-if="showControls">
          <Button
            icon="pi pi-plus"
            size="small"
            severity="secondary"
            @click="addRandomData"
            v-tooltip.left="'Add Data'"
          />
          <Button
            icon="pi pi-minus"
            size="small"
            severity="secondary"
            @click="removeData"
            v-tooltip.left="'Remove Data'"
          />
          <Button
            icon="pi pi-refresh"
            size="small"
            severity="secondary"
            @click="randomizeData"
            v-tooltip.left="'Randomize Data'"
          />
          <Button
            :icon="animationEnabled ? 'pi pi-pause' : 'pi pi-play'"
            size="small"
            severity="secondary"
            @click="toggleAnimation"
            v-tooltip.left="animationEnabled ? 'Disable Animation' : 'Enable Animation'"
          />
        </div>
      </div>
    </template>

    <template #content>
      <div class="chart-wrapper" :style="{ height: height }">
        <canvas
          ref="chartCanvas"
          :width="width"
          :height="height"
        ></canvas>
      </div>
    </template>

    <template #footer v-if="showFooter">
      <div class="flex justify-content-between align-items-center text-sm text-muted-color">
        <span>{{ getChartDescription(type) }}</span>
        <Chip
          v-if="chartData?.datasets?.[0]?.data"
          :label="`${chartData.datasets[0].data.length} data points`"
          size="small"
        />
      </div>
    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted, nextTick, watch, computed } from 'vue'
import { useLayout } from '@/layout/composables/layout'

const props = defineProps({
  type: {
    type: String,
    required: true,
    validator: (value) => [
      'line', 'bar', 'pie', 'doughnut', 'polarArea',
      'radar', 'scatter', 'bubble', 'area', 'mixed'
    ].includes(value)
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
  showDataLabels: {
    type: Boolean,
    default: true
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
})

const emit = defineEmits(['chart-created', 'data-updated', 'chart-clicked'])

const { isDarkTheme } = useLayout()
const chartCanvas = ref(null)
const animationEnabled = ref(true)
let chartInstance = null

// Computed chart data to ensure reactivity
const chartData = computed(() => ({
  ...props.data,
  datasets: props.data.datasets?.map(dataset => ({
    ...dataset,
    backgroundColor: dataset.backgroundColor || getDefaultColors().background,
    borderColor: dataset.borderColor || getDefaultColors().border,
    borderWidth: dataset.borderWidth || 2
  })) || []
}))

// Theme-aware colors
const themeColors = computed(() => ({
  light: {
    gridColor: 'rgba(0, 0, 0, 0.1)',
    textColor: '#333',
    tooltipBg: '#fff',
    tooltipColor: '#333'
  },
  dark: {
    gridColor: 'rgba(255, 255, 255, 0.2)',
    textColor: '#f1f1f1',
    tooltipBg: '#222',
    tooltipColor: '#f1f1f1'
  }
}))

const currentTheme = computed(() => isDarkTheme.value ? 'dark' : 'light')
const currentThemeColors = computed(() => themeColors.value[currentTheme.value])

function getDefaultColors() {
  const colors = [
    'rgba(255, 99, 132, 0.8)', 'rgba(54, 162, 235, 0.8)',
    'rgba(255, 205, 86, 0.8)', 'rgba(75, 192, 192, 0.8)',
    'rgba(153, 102, 255, 0.8)', 'rgba(255, 159, 64, 0.8)',
    'rgba(199, 199, 199, 0.8)', 'rgba(83, 102, 255, 0.8)'
  ]

  return {
    background: colors,
    border: colors.map(color => color.replace('0.8', '1'))
  }
}

function getChartConfig() {
  const config = {
    type: props.type,
    data: JSON.parse(JSON.stringify(chartData.value)),
    options: {
      responsive: props.responsive,
      maintainAspectRatio: props.maintainAspectRatio,
      animation: {
        duration: animationEnabled.value ? props.animationDuration : 0
      },
      plugins: {
        legend: {
          display: props.showLegend,
          position: 'top',
          labels: {
            color: currentThemeColors.value.textColor
          }
        },
        title: {
          display: !!props.title,
          text: props.title,
          color: currentThemeColors.value.textColor
        },
        tooltip: {
          backgroundColor: currentThemeColors.value.tooltipBg,
          titleColor: currentThemeColors.value.tooltipColor,
          bodyColor: currentThemeColors.value.tooltipColor,
        }
      },
      onClick: (event, elements) => {
        if (elements.length > 0) {
          const element = elements[0]
          const datasetIndex = element.datasetIndex
          const dataIndex = element.index
          emit('chart-clicked', {
            event,
            element,
            datasetIndex,
            dataIndex,
            data: chartData.value.datasets[datasetIndex].data[dataIndex],
            label: chartData.value.labels?.[dataIndex]
          })
        }
      },
      ...props.options
    }
  }

  // Apply type-specific configurations
  switch (props.type) {
    case 'line':
    case 'area':
      config.type = 'line'
      config.data.datasets = config.data.datasets.map(dataset => ({
        ...dataset,
        fill: props.type === 'area',
        tension: dataset.tension ?? 0.4
      }))
      config.options.scales = {
        y: {
          beginAtZero: true,
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        },
        x: {
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        }
      }
      break

    case 'bar':
      config.options.scales = {
        y: {
          beginAtZero: true,
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        },
        x: {
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        }
      }
      break

    case 'pie':
    case 'doughnut':
      if (props.showLegend) {
        config.options.plugins.legend = {
          display: true,
          position: 'top',
          labels: {
            generateLabels: function(chart) {
              const data = chart.data
              if (data.labels.length && data.datasets.length) {
                const dataset = data.datasets[0]
                const total = dataset.data.reduce((sum, value) => sum + value, 0)

                return data.labels.map((label, i) => {
                  const value = dataset.data[i]
                  const percentage = ((value / total) * 100).toFixed(1)

                  return {
                    text: `${label}: ${percentage}%`,
                    fillStyle: dataset.backgroundColor[i],
                    strokeStyle: dataset.borderColor[i],
                    lineWidth: dataset.borderWidth,
                    hidden: isNaN(value) || chart.getDatasetMeta(0).data[i].hidden,
                    index: i
                  }
                })
              }
              return []
            },
            color: currentThemeColors.value.textColor
          }
        }
      }
      break

    case 'polarArea':
      config.options.scales = {
        r: {
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor, backdropColor: 'transparent' },
          pointLabels: { color: currentThemeColors.value.textColor }
        }
      }
      break

    case 'radar':
      config.options.scales = {
        r: {
          angleLines: { color: currentThemeColors.value.gridColor },
          grid: { color: currentThemeColors.value.gridColor },
          suggestedMin: 0,
          suggestedMax: 100,
          ticks: { color: currentThemeColors.value.textColor, backdropColor: 'transparent' },
          pointLabels: { color: currentThemeColors.value.textColor }
        }
      }
      break

    case 'scatter':
    case 'bubble':
      config.options.scales = {
        y: {
          beginAtZero: true,
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        },
        x: {
          type: 'linear',
          position: 'bottom',
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        }
      }
      break

    case 'mixed':
      config.type = 'bar'
      config.options.scales = {
        y: {
          beginAtZero: true,
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        },
        x: {
          grid: { color: currentThemeColors.value.gridColor },
          ticks: { color: currentThemeColors.value.textColor }
        }
      }
      break
  }

  // Add data labels plugin if enabled
  if (props.showDataLabels && !['pie', 'doughnut'].includes(props.type)) {
    config.plugins = config.plugins || []
    config.plugins.push({
      id: 'dataLabels',
      afterDatasetsDraw: function(chart) {
        const ctx = chart.ctx
        chart.data.datasets.forEach((dataset, i) => {
          const meta = chart.getDatasetMeta(i)
          meta.data.forEach((element, index) => {
            const data = dataset.data[index]
            ctx.fillStyle = currentThemeColors.value.textColor
            ctx.font = 'bold 12px Arial'
            ctx.textAlign = 'center'
            ctx.textBaseline = 'bottom'
            ctx.fillText(data, element.x, element.y - 5)
          })
        })
      }
    })
  }

  return config
}

async function createChart() {
  await nextTick()
  if (chartInstance) {
    chartInstance.destroy()
  }

  const ctx = chartCanvas.value?.getContext('2d')
  if (!ctx) return

  const config = getChartConfig()

  try {
    const { Chart, registerables } = await import('chart.js')
    Chart.register(...registerables)

    chartInstance = new Chart(ctx, config)
    emit('chart-created', chartInstance)
  } catch (error) {
    console.error('Error creating chart:', error)
  }
}

function updateChart() {
  if (chartInstance) {
    chartInstance.data = JSON.parse(JSON.stringify(chartData.value))
    chartInstance.update('active')
    emit('data-updated', chartInstance)
  }
}

function addRandomData() {
  if (!chartInstance) return

  const randomValue = Math.floor(Math.random() * 50) + 1

  if (['scatter', 'bubble'].includes(props.type)) {
    chartInstance.data.datasets[0].data.push({
      x: Math.random() * (props.type === 'bubble' ? 60 : 20) - 10,
      y: Math.random() * (props.type === 'bubble' ? 40 : 20) - 10,
      r: props.type === 'bubble' ? Math.random() * 20 + 5 : undefined
    })
  } else {
    const newLabel = `Point ${chartInstance.data.labels.length + 1}`
    chartInstance.data.labels.push(newLabel)
    chartInstance.data.datasets.forEach(dataset => {
      dataset.data.push(randomValue)
    })
  }

  chartInstance.update()
  emit('data-updated', chartInstance)
}

function removeData() {
  if (!chartInstance) return

  if (['scatter', 'bubble'].includes(props.type)) {
    if (chartInstance.data.datasets[0].data.length > 0) {
      chartInstance.data.datasets[0].data.pop()
    }
  } else {
    if (chartInstance.data.labels.length > 0) {
      chartInstance.data.labels.pop()
      chartInstance.data.datasets.forEach(dataset => {
        dataset.data.pop()
      })
    }
  }

  chartInstance.update()
  emit('data-updated', chartInstance)
}

function randomizeData() {
  if (!chartInstance) return

  chartInstance.data.datasets.forEach(dataset => {
    if (['scatter', 'bubble'].includes(props.type)) {
      dataset.data = dataset.data.map(() => ({
        x: Math.random() * (props.type === 'bubble' ? 60 : 20) - 10,
        y: Math.random() * (props.type === 'bubble' ? 40 : 20) - 10,
        r: props.type === 'bubble' ? Math.random() * 20 + 5 : undefined
      }))
    } else {
      dataset.data = dataset.data.map(() => Math.floor(Math.random() * 50) + 1)
    }
  })

  chartInstance.update()
  emit('data-updated', chartInstance)
}

function toggleAnimation() {
  animationEnabled.value = !animationEnabled.value
  createChart() // Recreate chart with new animation setting
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
  }
  return names[type] || type
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
  }
  return descriptions[type] || 'Chart visualization'
}

// Watchers
watch(() => props.data, () => {
  updateChart()
}, { deep: true })

watch(() => props.type, () => {
  createChart()
})

watch(currentTheme, () => {
  createChart()
})

// Lifecycle
onMounted(() => {
  createChart()
})

// Expose methods
defineExpose({
  chartInstance: () => chartInstance,
  updateChart,
  addRandomData,
  removeData,
  randomizeData,
  toggleAnimation
})
</script>

<style scoped>
.chart-card {
  height: 100%;
}

.chart-wrapper {
  position: relative;
  width: 100%;
}

.chart-wrapper canvas {
  max-width: 100%;
  height: auto;
}

@media (max-width: 768px) {
  .chart-wrapper {
    height: 300px !important;
  }
}
</style>