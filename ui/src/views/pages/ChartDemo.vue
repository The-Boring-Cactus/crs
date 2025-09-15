<template>
  <div class="chart-demo">
    <div class="card mb-4">
      <div class="card-header flex justify-content-between align-items-center p-4">
        <h2 class="m-0">Chart Component Demo</h2>
        <div class="flex gap-2">
          <Button
            icon="pi pi-plus"
            label="Add Chart"
            @click="showAddChartDialog = true"
          />
          <Button
            icon="pi pi-refresh"
            label="Reset All"
            severity="secondary"
            @click="resetCharts"
          />
        </div>
      </div>
    </div>

    <!-- Chart Grid -->
    <div class="grid">
      <div
        v-for="(chart, index) in charts"
        :key="chart.id"
        :class="chart.gridClass"
      >
        <PrimeChart
          :type="chart.type"
          :data="chart.data"
          :title="chart.title"
          :description="chart.description"
          :show-controls="chart.showControls"
          :show-data-labels="chart.showDataLabels"
          :show-legend="chart.showLegend"
          :height="chart.height"
          @chart-created="onChartCreated"
          @data-updated="onDataUpdated"
          @chart-clicked="onChartClicked"
        >
          <template #header v-if="chart.customHeader">
            <div class="flex justify-content-between align-items-center p-4">
              <div>
                <h3 class="m-0">{{ chart.title }}</h3>
                <p class="text-muted-color text-sm m-0 mt-1">{{ chart.description }}</p>
              </div>
              <div class="flex gap-2">
                <Button
                  icon="pi pi-cog"
                  size="small"
                  severity="secondary"
                  @click="editChart(index)"
                />
                <Button
                  icon="pi pi-trash"
                  size="small"
                  severity="danger"
                  @click="removeChart(index)"
                />
              </div>
            </div>
          </template>
        </PrimeChart>
      </div>
    </div>

    <!-- Add Chart Dialog -->
    <Dialog
      v-model:visible="showAddChartDialog"
      :style="{ width: '600px' }"
      header="Add New Chart"
      :modal="true"
      class="p-fluid"
    >
      <div class="grid">
        <div class="col-12 md:col-6">
          <div class="field">
            <label for="chartType" class="font-semibold">Chart Type</label>
            <Dropdown
              id="chartType"
              v-model="newChart.type"
              :options="chartTypes"
              optionLabel="label"
              optionValue="value"
              placeholder="Select chart type"
            />
          </div>
        </div>

        <div class="col-12 md:col-6">
          <div class="field">
            <label for="chartSize" class="font-semibold">Chart Size</label>
            <Dropdown
              id="chartSize"
              v-model="newChart.gridClass"
              :options="sizeOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Select size"
            />
          </div>
        </div>

        <div class="col-12">
          <div class="field">
            <label for="chartTitle" class="font-semibold">Title</label>
            <InputText
              id="chartTitle"
              v-model="newChart.title"
              placeholder="Enter chart title"
            />
          </div>
        </div>

        <div class="col-12">
          <div class="field">
            <label for="chartDescription" class="font-semibold">Description</label>
            <Textarea
              id="chartDescription"
              v-model="newChart.description"
              rows="2"
              placeholder="Enter chart description"
            />
          </div>
        </div>

        <div class="col-12">
          <div class="field">
            <label class="font-semibold">Chart Options</label>
            <div class="flex flex-wrap gap-3 mt-2">
              <div class="flex align-items-center">
                <Checkbox
                  id="showControls"
                  v-model="newChart.showControls"
                  binary
                />
                <label for="showControls" class="ml-2">Show Controls</label>
              </div>
              <div class="flex align-items-center">
                <Checkbox
                  id="showDataLabels"
                  v-model="newChart.showDataLabels"
                  binary
                />
                <label for="showDataLabels" class="ml-2">Show Data Labels</label>
              </div>
              <div class="flex align-items-center">
                <Checkbox
                  id="showLegend"
                  v-model="newChart.showLegend"
                  binary
                />
                <label for="showLegend" class="ml-2">Show Legend</label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <Button
          label="Cancel"
          icon="pi pi-times"
          @click="showAddChartDialog = false"
          severity="secondary"
        />
        <Button
          label="Add Chart"
          icon="pi pi-check"
          @click="addChart"
        />
      </template>
    </Dialog>

    <!-- Event Log -->
    <Card class="mt-4" v-if="eventLog.length > 0">
      <template #title>
        <div class="flex justify-content-between align-items-center">
          <span>Event Log</span>
          <Button
            icon="pi pi-trash"
            size="small"
            severity="secondary"
            @click="eventLog = []"
          />
        </div>
      </template>
      <template #content>
        <div class="max-h-20rem overflow-auto">
          <div
            v-for="(event, index) in eventLog"
            :key="index"
            class="flex justify-content-between align-items-center p-2 border-bottom-1 surface-border"
          >
            <span class="text-sm">{{ event.message }}</span>
            <small class="text-muted-color">{{ event.timestamp }}</small>
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import PrimeChart from '@/components/PrimeChart.vue'

const showAddChartDialog = ref(false)
const eventLog = ref([])

const charts = ref([
  {
    id: 1,
    type: 'line',
    title: 'Sales Over Time',
    description: 'Monthly sales data for 2024',
    gridClass: 'col-12 md:col-6',
    height: '300px',
    showControls: true,
    showDataLabels: true,
    showLegend: true,
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        label: 'Sales 2024',
        data: [12, 19, 3, 5, 2, 3],
        borderColor: 'rgba(75, 192, 192, 1)',
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        tension: 0.4
      }]
    }
  },
  {
    id: 2,
    type: 'bar',
    title: 'Product Categories',
    description: 'Revenue by product category',
    gridClass: 'col-12 md:col-6',
    height: '300px',
    showControls: false,
    showDataLabels: true,
    showLegend: false,
    data: {
      labels: ['Electronics', 'Clothing', 'Books', 'Home', 'Sports'],
      datasets: [{
        label: 'Revenue',
        data: [25, 15, 10, 20, 8],
        backgroundColor: [
          'rgba(255, 99, 132, 0.8)',
          'rgba(54, 162, 235, 0.8)',
          'rgba(255, 205, 86, 0.8)',
          'rgba(75, 192, 192, 0.8)',
          'rgba(153, 102, 255, 0.8)'
        ]
      }]
    }
  },
  {
    id: 3,
    type: 'pie',
    title: 'Market Share',
    description: 'Company market share distribution',
    gridClass: 'col-12 md:col-6',
    height: '350px',
    showControls: false,
    showDataLabels: false,
    showLegend: true,
    data: {
      labels: ['Company A', 'Company B', 'Company C', 'Company D', 'Others'],
      datasets: [{
        data: [35, 25, 20, 15, 5],
        backgroundColor: [
          'rgba(255, 99, 132, 0.8)',
          'rgba(54, 162, 235, 0.8)',
          'rgba(255, 205, 86, 0.8)',
          'rgba(75, 192, 192, 0.8)',
          'rgba(153, 102, 255, 0.8)'
        ]
      }]
    }
  },
  {
    id: 4,
    type: 'doughnut',
    title: 'Budget Allocation',
    description: 'Department budget distribution',
    gridClass: 'col-12 md:col-6',
    height: '350px',
    showControls: false,
    showDataLabels: true,
    showLegend: true,
    data: {
      labels: ['Development', 'Marketing', 'Sales', 'Support', 'Admin'],
      datasets: [{
        data: [40, 25, 20, 10, 5],
        backgroundColor: [
          'rgba(255, 99, 132, 0.8)',
          'rgba(54, 162, 235, 0.8)',
          'rgba(255, 205, 86, 0.8)',
          'rgba(75, 192, 192, 0.8)',
          'rgba(153, 102, 255, 0.8)'
        ]
      }]
    }
  }
])

const newChart = reactive({
  type: 'line',
  title: '',
  description: '',
  gridClass: 'col-12 md:col-6',
  height: '300px',
  showControls: false,
  showDataLabels: true,
  showLegend: true
})

const chartTypes = ref([
  { label: 'Line Chart', value: 'line' },
  { label: 'Bar Chart', value: 'bar' },
  { label: 'Pie Chart', value: 'pie' },
  { label: 'Doughnut Chart', value: 'doughnut' },
  { label: 'Area Chart', value: 'area' },
  { label: 'Polar Area Chart', value: 'polarArea' },
  { label: 'Radar Chart', value: 'radar' },
  { label: 'Scatter Plot', value: 'scatter' },
  { label: 'Bubble Chart', value: 'bubble' },
  { label: 'Mixed Chart', value: 'mixed' }
])

const sizeOptions = ref([
  { label: 'Small (1/4 width)', value: 'col-12 md:col-3' },
  { label: 'Medium (1/2 width)', value: 'col-12 md:col-6' },
  { label: 'Large (3/4 width)', value: 'col-12 md:col-9' },
  { label: 'Full Width', value: 'col-12' }
])

function generateSampleData(type) {
  const baseLabels = ['A', 'B', 'C', 'D', 'E']
  const baseData = [12, 19, 3, 5, 2]

  switch (type) {
    case 'radar':
      return {
        labels: ['Speed', 'Reliability', 'Comfort', 'Safety', 'Efficiency'],
        datasets: [{
          label: 'Performance',
          data: [65, 59, 90, 81, 56],
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          borderColor: 'rgba(54, 162, 235, 1)',
          fill: true
        }]
      }
    case 'scatter':
      return {
        datasets: [{
          label: 'Scatter Data',
          data: [
            { x: -10, y: 0 },
            { x: 0, y: 10 },
            { x: 10, y: 5 },
            { x: 0.5, y: 5.5 }
          ],
          backgroundColor: 'rgba(255, 99, 132, 0.6)'
        }]
      }
    case 'bubble':
      return {
        datasets: [{
          label: 'Bubble Data',
          data: [
            { x: 20, y: 30, r: 15 },
            { x: 40, y: 10, r: 10 },
            { x: 15, y: 22, r: 25 }
          ],
          backgroundColor: 'rgba(255, 99, 132, 0.6)'
        }]
      }
    case 'mixed':
      return {
        labels: baseLabels,
        datasets: [
          {
            type: 'bar',
            label: 'Bars',
            data: baseData,
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
          },
          {
            type: 'line',
            label: 'Line',
            data: [5, 10, 15, 8, 12],
            borderColor: 'rgba(54, 162, 235, 1)',
            backgroundColor: 'transparent',
            fill: false
          }
        ]
      }
    default:
      return {
        labels: baseLabels,
        datasets: [{
          label: 'Sample Data',
          data: baseData,
          backgroundColor: [
            'rgba(255, 99, 132, 0.8)',
            'rgba(54, 162, 235, 0.8)',
            'rgba(255, 205, 86, 0.8)',
            'rgba(75, 192, 192, 0.8)',
            'rgba(153, 102, 255, 0.8)'
          ]
        }]
      }
  }
}

function addChart() {
  const chart = {
    id: Date.now(),
    ...newChart,
    data: generateSampleData(newChart.type)
  }

  charts.value.push(chart)
  showAddChartDialog.value = false

  // Reset form
  Object.assign(newChart, {
    type: 'line',
    title: '',
    description: '',
    gridClass: 'col-12 md:col-6',
    height: '300px',
    showControls: false,
    showDataLabels: true,
    showLegend: true
  })

  addToEventLog(`Added new ${chart.type} chart: ${chart.title || 'Untitled'}`)
}

function removeChart(index) {
  const chart = charts.value[index]
  charts.value.splice(index, 1)
  addToEventLog(`Removed chart: ${chart.title || chart.type}`)
}

function resetCharts() {
  charts.value = []
  addToEventLog('Reset all charts')
}

function onChartCreated(chartInstance) {
  addToEventLog('Chart created successfully')
}

function onDataUpdated(chartInstance) {
  addToEventLog('Chart data updated')
}

function onChartClicked(event) {
  addToEventLog(`Chart clicked - Dataset: ${event.datasetIndex}, Index: ${event.dataIndex}`)
}

function addToEventLog(message) {
  eventLog.value.unshift({
    message,
    timestamp: new Date().toLocaleTimeString()
  })

  // Keep only last 10 events
  if (eventLog.value.length > 10) {
    eventLog.value = eventLog.value.slice(0, 10)
  }
}
</script>

<style scoped>
.chart-demo {
  padding: 1rem;
}

.max-h-20rem {
  max-height: 20rem;
}
</style>