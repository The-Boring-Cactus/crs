<script setup>
import { ref } from 'vue'

const location = ref({
  address: '123 Smart Street, Tech City, TC 12345',
  coordinates: { lat: 37.7749, lng: -122.4194 },
  timezone: 'PST (UTC-8)',
  sunrise: '06:45 AM',
  sunset: '07:30 PM'
})

const nearbyDevices = ref([
  { id: 1, name: 'Scarlett\'s Phone', distance: 'At Home', status: 'Connected', icon: 'pi-mobile' },
  { id: 2, name: 'Dad\'s Car', distance: '2.5 km away', status: 'Away', icon: 'pi-car' },
  { id: 3, name: 'Mom\'s Phone', distance: 'At Home', status: 'Connected', icon: 'pi-mobile' },
  { id: 4, name: 'Riya\'s Tablet', distance: 'At Home', status: 'Connected', icon: 'pi-tablet' }
])

const zones = ref([
  { id: 1, name: 'Home Zone', radius: 100, active: true, color: '#66BB6A' },
  { id: 2, name: 'Work Zone', radius: 50, active: false, color: '#8B5CF6' },
  { id: 3, name: 'School Zone', radius: 75, active: false, color: '#FBB03B' }
])
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-map-marker" style="margin-right: 0.5rem;"></i>
        Location & Geofencing
      </h3>
    </div>

    <!-- Location Info -->
    <div style="background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); border-radius: 20px; padding: 2rem; color: white; margin-bottom: 2rem;">
      <h4 style="margin-bottom: 1.5rem;">
        <i class="pi pi-home" style="margin-right: 0.5rem;"></i>
        Home Location
      </h4>
      <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 2rem;">
        <div>
          <div style="font-size: 0.9rem; opacity: 0.9; margin-bottom: 0.5rem;">Address</div>
          <div style="font-size: 1.1rem; font-weight: bold;">{{ location.address }}</div>
        </div>
        <div>
          <div style="font-size: 0.9rem; opacity: 0.9; margin-bottom: 0.5rem;">Coordinates</div>
          <div style="font-size: 1.1rem; font-weight: bold;">{{ location.coordinates.lat }}, {{ location.coordinates.lng }}</div>
        </div>
        <div>
          <div style="font-size: 0.9rem; opacity: 0.9; margin-bottom: 0.5rem;">Timezone</div>
          <div style="font-size: 1.1rem; font-weight: bold;">{{ location.timezone }}</div>
        </div>
        <div>
          <div style="font-size: 0.9rem; opacity: 0.9; margin-bottom: 0.5rem;">Sunrise / Sunset</div>
          <div style="font-size: 1.1rem; font-weight: bold;">{{ location.sunrise }} / {{ location.sunset }}</div>
        </div>
      </div>
    </div>

    <!-- Map Placeholder -->
    <div style="background: #F8F8F8; border-radius: 20px; padding: 3rem; margin-bottom: 2rem; text-align: center; min-height: 300px; display: flex; align-items: center; justify-content: center; flex-direction: column;">
      <i class="pi pi-map" style="font-size: 5rem; color: #8B5CF6; margin-bottom: 1rem;"></i>
      <h4 style="color: #666;">Interactive Map</h4>
      <p style="color: #999; margin-top: 0.5rem;">Map integration would be displayed here</p>
    </div>

    <!-- Nearby Devices -->
    <div style="margin-bottom: 2rem;">
      <h4 style="margin-bottom: 1rem;">
        <i class="pi pi-mobile" style="margin-right: 0.5rem;"></i>
        Family Devices
      </h4>
      <div class="devices-grid">
        <div
          v-for="device in nearbyDevices"
          :key="device.id"
          class="device-card"
          :class="device.status === 'Connected' ? 'refrigerator' : 'router'"
        >
          <div class="device-info">
            <i :class="`pi ${device.icon}`" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
            <h4>{{ device.name }}</h4>
            <div style="font-size: 0.85rem; opacity: 0.9; margin-top: 0.3rem;">
              <i class="pi pi-map-marker" style="font-size: 0.8rem;"></i> {{ device.distance }}
            </div>
          </div>
          <div style="text-align: right; font-weight: bold;">
            {{ device.status }}
          </div>
        </div>
      </div>
    </div>

    <!-- Geofencing Zones -->
    <div>
      <h4 style="margin-bottom: 1rem;">
        <i class="pi pi-circle" style="margin-right: 0.5rem;"></i>
        Geofencing Zones
      </h4>
      <div class="controls-grid" style="grid-template-columns: repeat(3, 1fr);">
        <div
          v-for="zone in zones"
          :key="zone.id"
          class="control-card"
          :class="{ active: zone.active }"
          @click="zone.active = !zone.active"
        >
          <div class="control-icon">
            <i class="pi pi-circle" :style="{ color: zone.active ? 'white' : zone.color }"></i>
          </div>
          <h4>{{ zone.name }}</h4>
          <div style="font-size: 0.9rem; opacity: 0.9;">Radius: {{ zone.radius }}m</div>
          <InputSwitch v-model="zone.active" style="margin-top: 1rem;" />
        </div>
      </div>
    </div>
  </div>
</template>
