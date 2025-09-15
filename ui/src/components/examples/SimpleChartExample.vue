<template>
  <div class="simple-chart-examples">
    <h2>Simple Chart Component Usage Examples</h2>

    <!-- Basic Line Chart -->
    <div class="mb-4">
      <h3>Basic Line Chart</h3>
      <PrimeChart
        type="line"
        :data="lineChartData"
        title="Monthly Sales"
        description="Sales performance over the last 6 months"
        :height="'250px'"
      />
    </div>

    <!-- Bar Chart with Controls -->
    <div class="mb-4">
      <h3>Interactive Bar Chart</h3>
      <PrimeChart
        type="bar"
        :data="barChartData"
        title="Product Performance"
        description="Revenue by product category with interactive controls"
        :show-controls="true"
        :height="'300px'"
        @chart-clicked="onBarChartClick"
      />
    </div>

    <!-- Pie Chart -->
    <div class="mb-4">
      <h3>Market Share Pie Chart</h3>
      <PrimeChart
        type="pie"
        :data="pieChartData"
        title="Market Share Distribution"
        description="Current market share by company"
        :show-legend="true"
        :show-data-labels="false"
        :height="'350px'"
      />
    </div>

    <!-- Event Log -->
    <Card v-if="eventMessages.length > 0" class="mt-4">
      <template #title>Chart Events</template>
      <template #content>
        <div class="max-h-15rem overflow-auto">
          <div
            v-for="(message, index) in eventMessages"
            :key="index"
            class="p-2 border-bottom-1 surface-border text-sm"
          >
            {{ message }}
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PrimeChart from '@/components/PrimeChart.vue'

const eventMessages = ref([])

// Line Chart Data
const lineChartData = ref({
  labels: ['January', 'February', 'March', 'April', 'May', 'June'],
  datasets: [{
    label: 'Sales ($)',
    data: [12000, 19000, 15000, 25000, 22000, 30000],
    borderColor: 'rgba(75, 192, 192, 1)',
    backgroundColor: 'rgba(75, 192, 192, 0.2)',
    tension: 0.4
  }]
})

// Bar Chart Data
const barChartData = ref({
  labels: ['Electronics', 'Clothing', 'Books', 'Home & Garden', 'Sports'],
  datasets: [{
    label: 'Revenue ($)',
    data: [45000, 25000, 18000, 32000, 15000],
    backgroundColor: [
      'rgba(255, 99, 132, 0.8)',
      'rgba(54, 162, 235, 0.8)',
      'rgba(255, 205, 86, 0.8)',
      'rgba(75, 192, 192, 0.8)',
      'rgba(153, 102, 255, 0.8)'
    ],
    borderColor: [
      'rgba(255, 99, 132, 1)',
      'rgba(54, 162, 235, 1)',
      'rgba(255, 205, 86, 1)',
      'rgba(75, 192, 192, 1)',
      'rgba(153, 102, 255, 1)'
    ],
    borderWidth: 2
  }]
})

// Pie Chart Data
const pieChartData = ref({
  labels: ['Company A', 'Company B', 'Company C', 'Company D', 'Others'],
  datasets: [{
    data: [35, 25, 20, 15, 5],
    backgroundColor: [
      'rgba(255, 99, 132, 0.8)',
      'rgba(54, 162, 235, 0.8)',
      'rgba(255, 205, 86, 0.8)',
      'rgba(75, 192, 192, 0.8)',
      'rgba(153, 102, 255, 0.8)'
    ],
    borderWidth: 2
  }]
})

function onBarChartClick(event) {
  const { label, data, dataIndex } = event
  const message = `Bar clicked: ${label} with value ${data} (index: ${dataIndex})`

  eventMessages.value.unshift(message)

  // Keep only last 5 messages
  if (eventMessages.value.length > 5) {
    eventMessages.value = eventMessages.value.slice(0, 5)
  }
}
</script>

<style scoped>
.simple-chart-examples {
  padding: 1rem;
}

.max-h-15rem {
  max-height: 15rem;
}

h2 {
  color: var(--text-color);
  margin-bottom: 2rem;
}

h3 {
  color: var(--text-color-secondary);
  margin-bottom: 1rem;
  margin-top: 2rem;
}
</style>