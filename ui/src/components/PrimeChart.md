# PrimeChart Component

A comprehensive, reusable chart component built with Vue 3, PrimeVue, and Chart.js that integrates seamlessly with your application's theme system.

## Features

- 🎨 **PrimeVue Integration**: Uses PrimeVue Card, Button, and other components for consistent UI
- 🌓 **Theme Aware**: Automatically adapts to light/dark theme changes
- 📊 **Multiple Chart Types**: Supports line, bar, pie, doughnut, polar area, radar, scatter, bubble, area, and mixed charts
- 🎛️ **Interactive Controls**: Optional data manipulation controls (add, remove, randomize data)
- 📱 **Responsive**: Mobile-friendly and responsive design
- 🎯 **Event Handling**: Emits chart events for custom interactions
- ⚙️ **Highly Configurable**: Extensive props for customization

## Supported Chart Types

- `line` - Line charts for trends over time
- `bar` - Bar charts for category comparisons
- `pie` - Pie charts for proportional data
- `doughnut` - Doughnut charts with hollow center
- `polarArea` - Polar area charts combining pie and radar features
- `radar` - Radar charts for multi-metric data
- `scatter` - Scatter plots for correlation analysis
- `bubble` - Bubble charts for three-dimensional data
- `area` - Area charts (filled line charts)
- `mixed` - Mixed charts combining different types

## Basic Usage

```vue
<template>
  <PrimeChart
    type="line"
    :data="chartData"
    title="Sales Over Time"
    description="Monthly sales data for 2024"
  />
</template>

<script setup>
import PrimeChart from '@/components/PrimeChart.vue'

const chartData = {
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
  datasets: [{
    label: 'Sales',
    data: [12, 19, 3, 5, 2, 3],
    borderColor: 'rgba(75, 192, 192, 1)',
    backgroundColor: 'rgba(75, 192, 192, 0.2)'
  }]
}
</script>
```

## Props

### Required Props

| Prop | Type | Description |
|------|------|-------------|
| `type` | String | Chart type (line, bar, pie, etc.) |
| `data` | Object | Chart.js data object with labels and datasets |

### Optional Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `options` | Object | `{}` | Chart.js options object |
| `width` | String/Number | `'100%'` | Chart width |
| `height` | String/Number | `'400px'` | Chart height |
| `title` | String | `''` | Chart title displayed in header |
| `description` | String | `''` | Chart description displayed in header |
| `showHeader` | Boolean | `true` | Show/hide card header |
| `showFooter` | Boolean | `true` | Show/hide card footer |
| `showControls` | Boolean | `false` | Show/hide interactive controls |
| `showDataLabels` | Boolean | `true` | Show/hide data labels on chart |
| `showLegend` | Boolean | `true` | Show/hide chart legend |
| `responsive` | Boolean | `true` | Make chart responsive |
| `maintainAspectRatio` | Boolean | `false` | Maintain aspect ratio |
| `animationDuration` | Number | `1000` | Animation duration in ms |

## Events

| Event | Payload | Description |
|-------|---------|-------------|
| `chart-created` | Chart instance | Emitted when chart is created |
| `data-updated` | Chart instance | Emitted when chart data is updated |
| `chart-clicked` | Event object | Emitted when chart element is clicked |

### Chart Click Event Object

```javascript
{
  event: MouseEvent,
  element: Chart element,
  datasetIndex: Number,
  dataIndex: Number,
  data: Any,
  label: String
}
```

## Data Format

The `data` prop should follow Chart.js data structure:

### Basic Format
```javascript
{
  labels: ['Label 1', 'Label 2', 'Label 3'],
  datasets: [{
    label: 'Dataset 1',
    data: [10, 20, 30],
    backgroundColor: 'rgba(75, 192, 192, 0.2)',
    borderColor: 'rgba(75, 192, 192, 1)',
    borderWidth: 2
  }]
}
```

### Scatter/Bubble Format
```javascript
{
  datasets: [{
    label: 'Scatter Data',
    data: [
      {x: 10, y: 20},
      {x: 15, y: 25},
      {x: 20, y: 30, r: 15} // r for bubble charts
    ]
  }]
}
```

### Mixed Chart Format
```javascript
{
  labels: ['A', 'B', 'C'],
  datasets: [
    {
      type: 'bar',
      label: 'Bars',
      data: [10, 20, 30]
    },
    {
      type: 'line',
      label: 'Line',
      data: [15, 25, 35]
    }
  ]
}
```

## Examples

### Simple Line Chart
```vue
<PrimeChart
  type="line"
  :data="lineData"
  title="Monthly Revenue"
  height="300px"
/>
```

### Interactive Bar Chart with Controls
```vue
<PrimeChart
  type="bar"
  :data="barData"
  title="Product Sales"
  :show-controls="true"
  :show-data-labels="true"
  @chart-clicked="handleChartClick"
/>
```

### Pie Chart with Custom Options
```vue
<PrimeChart
  type="pie"
  :data="pieData"
  title="Market Share"
  :show-legend="true"
  :show-data-labels="false"
  :options="{ maintainAspectRatio: true }"
/>
```

### Radar Chart
```vue
<PrimeChart
  type="radar"
  :data="radarData"
  title="Performance Metrics"
  description="Comparing different performance indicators"
/>
```

## Theme Integration

The component automatically adapts to your application's theme:

- **Light Theme**: Uses light backgrounds and dark text
- **Dark Theme**: Uses dark backgrounds and light text
- **Grid Colors**: Automatically adjusts grid line colors
- **Tooltips**: Theme-aware tooltip styling

## Methods (via ref)

Access chart instance and methods using template refs:

```vue
<template>
  <PrimeChart
    ref="chartRef"
    type="line"
    :data="chartData"
  />
  <Button @click="addData">Add Random Data</Button>
</template>

<script setup>
import { ref } from 'vue'

const chartRef = ref()

function addData() {
  chartRef.value.addRandomData()
}
</script>
```

### Available Methods

- `chartInstance()` - Returns Chart.js instance
- `updateChart()` - Updates chart with current data
- `addRandomData()` - Adds random data point
- `removeData()` - Removes last data point
- `randomizeData()` - Randomizes all data
- `toggleAnimation()` - Toggles chart animations

## Best Practices

1. **Data Reactivity**: Use reactive data to enable automatic chart updates
2. **Performance**: For large datasets, consider disabling animations
3. **Accessibility**: Always provide meaningful titles and descriptions
4. **Mobile**: Test on mobile devices and adjust height accordingly
5. **Colors**: Use consistent color schemes across charts

## Dependencies

- Vue 3
- PrimeVue
- Chart.js
- Application theme system (via `useLayout` composable)

## File Structure

```
src/
├── components/
│   ├── PrimeChart.vue           # Main component
│   └── examples/
│       └── SimpleChartExample.vue  # Usage examples
└── views/pages/
    └── ChartDemo.vue            # Demo page
```