<script setup>
import { useUserStore } from '@/stores/user'
import { useToast } from 'primevue/usetoast'
import { onMounted, onUnmounted } from 'vue'

const userStore = useUserStore()
const toast = useToast()

let tempInterval = null

const handleToggleControl = (control) => {
  const isActive = userStore.toggleControl(control)
  toast.add({
    severity: isActive ? 'success' : 'info',
    summary: control.charAt(0).toUpperCase() + control.slice(1),
    detail: isActive ? 'Encendido' : 'Apagado',
    life: 2000
  })
}

onMounted(() => {
  tempInterval = setInterval(() => {
    userStore.updateCurrentTemp()
  }, 2000)
})

onUnmounted(() => {
  if (tempInterval) {
    clearInterval(tempInterval)
  }
})
</script>

<template>
  <!-- Welcome Card -->
  <div class="welcome-card">
    <div class="welcome-text">
      <h2>Hello, {{ userStore.userName }}!</h2>
      <p>Welcome Home! The air quality is good & fresh you<br>can go out today.</p>
    </div>
    <div style="position: absolute; right: 100px; bottom: 10px;">
      <svg width="200" height="150" viewBox="0 0 200 150">
        <g transform="translate(50, 20)">
          <!-- Persona -->
          <circle cx="50" cy="30" r="8" fill="#4A5568"/>
          <path d="M50,38 L50,60" stroke="#4A5568" stroke-width="3" stroke-linecap="round"/>
          <path d="M50,45 L35,55" stroke="#4A5568" stroke-width="2" stroke-linecap="round"/>
          <path d="M50,45 L65,55" stroke="#4A5568" stroke-width="2" stroke-linecap="round"/>
          <path d="M50,60 L45,80" stroke="#4A5568" stroke-width="3" stroke-linecap="round"/>
          <path d="M50,60 L55,80" stroke="#4A5568" stroke-width="3" stroke-linecap="round"/>

          <!-- Perro -->
          <ellipse cx="20" cy="70" rx="12" ry="8" fill="#8B4513"/>
          <circle cx="12" cy="68" r="4" fill="#8B4513"/>
          <path d="M8,75 L8,80" stroke="#8B4513" stroke-width="2" stroke-linecap="round"/>
          <path d="M15,75 L15,80" stroke="#8B4513" stroke-width="2" stroke-linecap="round"/>
          <path d="M25,75 L25,80" stroke="#8B4513" stroke-width="2" stroke-linecap="round"/>
          <path d="M30,75 L30,80" stroke="#8B4513" stroke-width="2" stroke-linecap="round"/>

          <!-- Correa -->
          <path d="M35,55 L25,70" stroke="#666" stroke-width="1" stroke-dasharray="2,2"/>

          <!-- Plantas decorativas -->
          <circle cx="80" cy="75" r="15" fill="#90CDF4" opacity="0.3"/>
          <circle cx="95" cy="70" r="12" fill="#FBB03B" opacity="0.3"/>
        </g>
      </svg>
    </div>
    <div class="weather-info" style="margin-right: 250px;">
      <i class="pi pi-sun" style="font-size: 2rem; color: #FDB813;"></i>
      <div>
        <div style="font-size: 1.5rem; font-weight: bold;">+25°C</div>
        <div style="font-size: 0.9rem; color: #666;">Outdoor temperature</div>
      </div>
    </div>
    <div style="position: absolute; bottom: 20px; left: 30px; color: #666;">
      <i class="pi pi-cloud" style="margin-right: 0.5rem;"></i>
      Fuzzy cloudy weather
    </div>
  </div>

  <!-- Home Section -->
  <div class="home-section">
    <div class="section-header">
      <h3>{{ userStore.userName }}'s Home</h3>
      <div style="display: flex; align-items: center; gap: 1rem;">
        <span style="color: #666;">🏠 {{ userStore.homeStats.humidity }}%</span>
        <span style="color: #666;">🌡️ {{ userStore.homeStats.temperature }}°C</span>
        <select class="room-selector" v-model="userStore.selectedRoom">
          <option>Living Room</option>
          <option>Bedroom</option>
          <option>Kitchen</option>
          <option>Bathroom</option>
        </select>
      </div>
    </div>

    <!-- Controls Grid -->
    <div class="controls-grid">
      <div class="control-card" :class="{ active: userStore.controls.refrigerator }" @click="handleToggleControl('refrigerator')">
        <div class="control-icon">
          <i class="pi pi-inbox"></i>
        </div>
        <div>Refrigerator</div>
        <InputSwitch v-model="userStore.controls.refrigerator" />
      </div>

      <div class="control-card" :class="{ active: userStore.controls.temperature }" @click="handleToggleControl('temperature')">
        <div class="control-icon">
          <i class="pi pi-sun" :style="{ color: userStore.controls.temperature ? 'white' : '' }"></i>
        </div>
        <div>Temperature</div>
        <InputSwitch v-model="userStore.controls.temperature" />
      </div>

      <div class="control-card" :class="{ active: userStore.controls.airConditioner }" @click="handleToggleControl('airConditioner')">
        <div class="control-icon">
          <i class="pi pi-cloud"></i>
        </div>
        <div>Air Conditioner</div>
        <InputSwitch v-model="userStore.controls.airConditioner" />
      </div>

      <div class="control-card" :class="{ active: userStore.controls.lights }" @click="handleToggleControl('lights')">
        <div class="control-icon">
          <i class="pi pi-sun"></i>
        </div>
        <div>Lights</div>
        <InputSwitch v-model="userStore.controls.lights" />
      </div>
    </div>

    <!-- Temperature Control -->
    <div class="temperature-control">
      <div>
        <h4 style="color: #8B5CF6; margin-bottom: 1rem;">
          <i class="pi pi-sun" style="margin-right: 0.5rem;"></i>
          {{ userStore.selectedRoom }} Temperature
        </h4>
        <p style="color: #666; font-size: 2rem; margin-bottom: 1rem;">{{ userStore.homeStats.temperature }}°C</p>
        <InputSwitch v-model="userStore.temperatureOn" />
      </div>

      <div class="temp-display">
        <div class="temp-value">{{ userStore.currentTemp }}°C</div>
        <div class="temp-label">Celsius</div>
      </div>

      <div style="text-align: center;">
        <Button
          icon="pi pi-plus"
          class="p-button-rounded p-button-lg"
          style="background: #8B5CF6; border: none;"
          @click="userStore.increaseTemp"
        />
        <div style="margin-top: 1rem; font-size: 1.5rem; color: #FF6B6B;">{{ userStore.targetTemp }}°C</div>
        <Button
          icon="pi pi-minus"
          class="p-button-rounded p-button-lg"
          style="background: #FF6B6B; border: none; margin-top: 1rem;"
          @click="userStore.decreaseTemp"
        />
      </div>
    </div>

    <!-- My Devices -->
    <div style="display: flex; justify-content: space-between; align-items: center; margin: 2rem 0 1rem 0;">
      <h4>My Devices</h4>
      <div style="display: flex; align-items: center; gap: 0.5rem;">
        <span style="color: #666;">ON</span>
        <i class="pi pi-chevron-down" style="color: #666;"></i>
      </div>
    </div>

    <div class="devices-grid">
      <div class="device-card refrigerator">
        <div class="device-info">
          <i class="pi pi-inbox" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
          <h4>Refrigerator</h4>
        </div>
        <InputSwitch v-model="userStore.devices.refrigerator" />
      </div>

      <div class="device-card router">
        <div class="device-info">
          <i class="pi pi-wifi" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
          <h4>Router</h4>
        </div>
        <InputSwitch v-model="userStore.devices.router" />
      </div>

      <div class="device-card music">
        <div class="device-info">
          <i class="pi pi-volume-up" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
          <h4>Music System</h4>
        </div>
        <InputSwitch v-model="userStore.devices.music" />
      </div>

      <div class="device-card lamps">
        <div class="device-info">
          <i class="pi pi-sun" style="font-size: 1.5rem; margin-bottom: 0.5rem;"></i>
          <h4>Lamps</h4>
        </div>
        <InputSwitch v-model="userStore.devices.lamps" />
      </div>
    </div>

    <!-- Members -->
    <div class="members-section">
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <h4>Members</h4>
        <i class="pi pi-chevron-right" style="color: #666; cursor: pointer;"></i>
      </div>
      <div class="members-list">
        <div class="member-avatar">S</div>
        <div class="member-avatar" style="background: linear-gradient(135deg, #FF6B6B 0%, #FF5252 100%);">N</div>
        <div class="member-avatar" style="background: linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%);">R</div>
        <div class="member-avatar" style="background: linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%);">D</div>
        <div class="member-avatar" style="background: linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%);">M</div>
      </div>
      <div style="display: flex; gap: 1rem; margin-top: 0.5rem; font-size: 0.85rem; color: #666;">
        <span>Scarlett</span>
        <span>Nariya</span>
        <span>Riya</span>
        <span>Dad</span>
        <span>Mom</span>
      </div>
      <div style="display: flex; gap: 1.3rem; margin-top: 0.3rem; font-size: 0.75rem; color: #999;">
        <span>Admin</span>
        <span>Full Access</span>
        <span>Full Access</span>
        <span>Full Access</span>
        <span>Full Access</span>
      </div>
    </div>

    <!-- Power Consumption -->
    <div class="power-chart">
      <div class="chart-header">
        <div style="display: flex; align-items: center; gap: 0.5rem;">
          <div style="width: 10px; height: 10px; background: #FF6B6B; border-radius: 50%;"></div>
          <span>Electricity Consumed</span>
        </div>
        <div style="display: flex; align-items: center; gap: 1rem;">
          <span class="chart-percentage">73% Spending</span>
          <select style="background: transparent; border: none; color: #666;">
            <option>Month</option>
            <option>Week</option>
            <option>Year</option>
          </select>
        </div>
      </div>
      <div style="height: 150px; background: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjE1MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cGF0aCBkPSJNMCwxMDAgQzUwLDgwIDEwMCw2MCAxNTAsNzAgQzIwMCw4MCAyNTAsNDAgMzAwLDUwIEMzNTAsNjAgNDAwLDMwIDQwMCwzMCBMNDAwLDE1MCBMMCwxNTAgWiIgZmlsbD0iI0ZGQjNBQSIgb3BhY2l0eT0iMC4zIi8+CiAgPHBhdGggZD0iTTAsMTAwIEM1MCw4MCAxMDAsNjAgMTUwLDcwIEMyMDAsODAgMjUwLDQwIDMwMCw1MCBDMzUwLDYwIDQwMCwzMCA0MDAsMzAiIHN0cm9rZT0iI0ZGNkI2QiIgc3Ryb2tlLXdpZHRoPSIyIiBmaWxsPSJub25lIi8+Cjwvc3ZnPg==') no-repeat center; background-size: cover; border-radius: 10px; margin-top: 1rem;"></div>
      <div style="display: flex; justify-content: space-between; margin-top: 1rem; font-size: 0.8rem; color: #999;">
        <span>Jan</span>
        <span>Feb</span>
        <span>Mar</span>
        <span>Apr</span>
        <span>May</span>
        <span>Jun</span>
        <span>Jul</span>
        <span>Aug</span>
      </div>
    </div>
  </div>
</template>
