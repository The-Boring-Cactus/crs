<script setup>
import { useUserStore } from '@/stores/user'
import { ref } from 'vue'

const userStore = useUserStore()

const rooms = ref([
  {
    name: 'Living Room',
    icon: 'pi-home',
    devices: 8,
    temperature: 22,
    humidity: 45,
    color: '#8B5CF6'
  },
  {
    name: 'Bedroom',
    icon: 'pi-moon',
    devices: 5,
    temperature: 20,
    humidity: 50,
    color: '#4FC3F7'
  },
  {
    name: 'Kitchen',
    icon: 'pi-sparkles',
    devices: 6,
    temperature: 24,
    humidity: 55,
    color: '#FBB03B'
  },
  {
    name: 'Bathroom',
    icon: 'pi-box',
    devices: 3,
    temperature: 23,
    humidity: 60,
    color: '#FF6B6B'
  },
  {
    name: 'Office',
    icon: 'pi-desktop',
    devices: 7,
    temperature: 21,
    humidity: 42,
    color: '#66BB6A'
  },
  {
    name: 'Garage',
    icon: 'pi-car',
    devices: 4,
    temperature: 18,
    humidity: 48,
    color: '#9C27B0'
  }
])

const selectRoom = (room) => {
  userStore.setRoom(room.name)
}
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-th-large" style="margin-right: 0.5rem;"></i>
        Rooms
      </h3>
      <Button label="Add Room" icon="pi pi-plus" class="p-button-rounded" />
    </div>

    <div class="controls-grid" style="grid-template-columns: repeat(3, 1fr); gap: 2rem;">
      <div
        v-for="room in rooms"
        :key="room.name"
        class="control-card"
        :class="{ active: userStore.selectedRoom === room.name }"
        @click="selectRoom(room)"
        style="padding: 2rem; cursor: pointer;"
      >
        <div class="control-icon" style="font-size: 3rem; margin-bottom: 1rem;">
          <i :class="`pi ${room.icon}`" :style="{ color: userStore.selectedRoom === room.name ? 'white' : room.color }"></i>
        </div>
        <h3 style="margin-bottom: 1rem;">{{ room.name }}</h3>
        <div style="display: flex; flex-direction: column; gap: 0.5rem; width: 100%;">
          <div style="display: flex; justify-content: space-between;">
            <span><i class="pi pi-box"></i> Devices:</span>
            <strong>{{ room.devices }}</strong>
          </div>
          <div style="display: flex; justify-content: space-between;">
            <span><i class="pi pi-sun"></i> Temp:</span>
            <strong>{{ room.temperature }}°C</strong>
          </div>
          <div style="display: flex; justify-content: space-between;">
            <span><i class="pi pi-cloud"></i> Humidity:</span>
            <strong>{{ room.humidity }}%</strong>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.control-card h3 {
  font-size: 1.2rem;
  font-weight: 600;
}
</style>
