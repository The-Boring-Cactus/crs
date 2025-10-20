<script setup>
import { ref } from 'vue'

const securityStatus = ref({
  armed: true,
  mode: 'Home',
  alerts: 0
})

const cameras = ref([
  { id: 1, name: 'Front Door', status: 'Online', location: 'Entrance', recording: true },
  { id: 2, name: 'Backyard', status: 'Online', location: 'Garden', recording: true },
  { id: 3, name: 'Garage', status: 'Offline', location: 'Garage', recording: false },
  { id: 4, name: 'Living Room', status: 'Online', location: 'Interior', recording: true }
])

const sensors = ref([
  { id: 1, name: 'Motion Sensor - Hallway', status: 'Active', battery: 85 },
  { id: 2, name: 'Door Sensor - Main Door', status: 'Closed', battery: 92 },
  { id: 3, name: 'Window Sensor - Bedroom', status: 'Closed', battery: 78 },
  { id: 4, name: 'Motion Sensor - Garage', status: 'Active', battery: 65 }
])
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-shield" style="margin-right: 0.5rem;"></i>
        Security System
      </h3>
      <Button
        :label="securityStatus.armed ? 'Disarm' : 'Arm'"
        :icon="securityStatus.armed ? 'pi pi-lock' : 'pi pi-lock-open'"
        :class="securityStatus.armed ? 'p-button-danger' : 'p-button-success'"
        @click="securityStatus.armed = !securityStatus.armed"
      />
    </div>

    <!-- Security Status -->
    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 1.5rem; margin-bottom: 2rem;">
      <div class="control-card" :style="{ background: securityStatus.armed ? 'linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%)' : 'linear-gradient(135deg, #FF6B6B 0%, #FF5252 100%)', color: 'white' }">
        <i :class="securityStatus.armed ? 'pi pi-lock' : 'pi pi-lock-open'" style="font-size: 3rem; margin-bottom: 1rem;"></i>
        <h3>{{ securityStatus.armed ? 'Armed' : 'Disarmed' }}</h3>
        <div style="font-size: 0.9rem; opacity: 0.9;">System Status</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white;">
        <i class="pi pi-home" style="font-size: 3rem; margin-bottom: 1rem;"></i>
        <h3>{{ securityStatus.mode }}</h3>
        <div style="font-size: 0.9rem; opacity: 0.9;">Security Mode</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%); color: white;">
        <i class="pi pi-bell" style="font-size: 3rem; margin-bottom: 1rem;"></i>
        <h3>{{ securityStatus.alerts }}</h3>
        <div style="font-size: 0.9rem; opacity: 0.9;">Active Alerts</div>
      </div>
    </div>

    <!-- Cameras -->
    <div style="margin-bottom: 2rem;">
      <h4 style="margin-bottom: 1rem;">
        <i class="pi pi-video" style="margin-right: 0.5rem;"></i>
        Security Cameras
      </h4>
      <div class="devices-grid">
        <div
          v-for="camera in cameras"
          :key="camera.id"
          class="device-card"
          :class="camera.status === 'Online' ? 'refrigerator' : 'router'"
        >
          <div class="device-info">
            <i class="pi pi-video" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
            <h4>{{ camera.name }}</h4>
            <div style="font-size: 0.85rem; opacity: 0.9; margin-top: 0.3rem;">
              <i class="pi pi-map-marker" style="font-size: 0.8rem;"></i> {{ camera.location }}
            </div>
          </div>
          <div style="text-align: right;">
            <div style="margin-bottom: 0.5rem; font-weight: bold;">{{ camera.status }}</div>
            <InputSwitch v-model="camera.recording" />
          </div>
        </div>
      </div>
    </div>

    <!-- Sensors -->
    <div>
      <h4 style="margin-bottom: 1rem;">
        <i class="pi pi-bolt" style="margin-right: 0.5rem;"></i>
        Sensors
      </h4>
      <div class="devices-grid">
        <div
          v-for="sensor in sensors"
          :key="sensor.id"
          class="device-card music"
        >
          <div class="device-info">
            <i class="pi pi-bolt" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
            <h4 style="font-size: 1rem;">{{ sensor.name }}</h4>
            <div style="font-size: 0.85rem; opacity: 0.9; margin-top: 0.3rem;">
              {{ sensor.status }}
            </div>
          </div>
          <div style="text-align: right;">
            <i class="pi pi-battery-2" style="font-size: 1.5rem;"></i>
            <div style="font-weight: bold; margin-top: 0.3rem;">{{ sensor.battery }}%</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
