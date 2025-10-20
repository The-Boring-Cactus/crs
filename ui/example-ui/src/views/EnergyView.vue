<script setup>
import { ref } from 'vue'
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()

const energyData = ref({
  today: 45.6,
  week: 312.4,
  month: 1245.8,
  cost: 187.45
})

const weatherData = ref({
  current: 25,
  feels: 23,
  humidity: 65,
  wind: 12,
  condition: 'Partly Cloudy'
})
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-sun" style="margin-right: 0.5rem;"></i>
        Energy & Climate
      </h3>
    </div>

    <!-- Energy Consumption -->
    <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem;">
      <div class="control-card" style="background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white;">
        <div style="font-size: 0.9rem; margin-bottom: 0.5rem;">Today</div>
        <div style="font-size: 2rem; font-weight: bold;">{{ energyData.today }}</div>
        <div style="font-size: 0.8rem; opacity: 0.9;">kWh</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%); color: white;">
        <div style="font-size: 0.9rem; margin-bottom: 0.5rem;">This Week</div>
        <div style="font-size: 2rem; font-weight: bold;">{{ energyData.week }}</div>
        <div style="font-size: 0.8rem; opacity: 0.9;">kWh</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%); color: white;">
        <div style="font-size: 0.9rem; margin-bottom: 0.5rem;">This Month</div>
        <div style="font-size: 2rem; font-weight: bold;">{{ energyData.month }}</div>
        <div style="font-size: 0.8rem; opacity: 0.9;">kWh</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%); color: white;">
        <div style="font-size: 0.9rem; margin-bottom: 0.5rem;">Cost</div>
        <div style="font-size: 2rem; font-weight: bold;">${{ energyData.cost }}</div>
        <div style="font-size: 0.8rem; opacity: 0.9;">This month</div>
      </div>
    </div>

    <!-- Climate Control -->
    <div style="background: #F8F8F8; border-radius: 20px; padding: 2rem; margin-bottom: 2rem;">
      <h4 style="margin-bottom: 1.5rem;">
        <i class="pi pi-cloud" style="margin-right: 0.5rem;"></i>
        Climate Control
      </h4>
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
    </div>

    <!-- Weather Info -->
    <div style="background: linear-gradient(135deg, #FFE5B4 0%, #FFDAB9 100%); border-radius: 20px; padding: 2rem;">
      <h4 style="margin-bottom: 1.5rem; color: #D2691E;">
        <i class="pi pi-sun" style="margin-right: 0.5rem;"></i>
        Outdoor Weather
      </h4>
      <div style="display: grid; grid-template-columns: repeat(5, 1fr); gap: 1.5rem;">
        <div style="text-align: center;">
          <i class="pi pi-sun" style="font-size: 2.5rem; color: #FDB813;"></i>
          <div style="font-size: 2rem; font-weight: bold; margin-top: 0.5rem;">{{ weatherData.current }}°C</div>
          <div style="font-size: 0.9rem; color: #666;">Temperature</div>
        </div>
        <div style="text-align: center;">
          <i class="pi pi-heart" style="font-size: 2.5rem; color: #FF6B6B;"></i>
          <div style="font-size: 2rem; font-weight: bold; margin-top: 0.5rem;">{{ weatherData.feels }}°C</div>
          <div style="font-size: 0.9rem; color: #666;">Feels Like</div>
        </div>
        <div style="text-align: center;">
          <i class="pi pi-cloud" style="font-size: 2.5rem; color: #4FC3F7;"></i>
          <div style="font-size: 2rem; font-weight: bold; margin-top: 0.5rem;">{{ weatherData.humidity }}%</div>
          <div style="font-size: 0.9rem; color: #666;">Humidity</div>
        </div>
        <div style="text-align: center;">
          <i class="pi pi-replay" style="font-size: 2.5rem; color: #66BB6A;"></i>
          <div style="font-size: 2rem; font-weight: bold; margin-top: 0.5rem;">{{ weatherData.wind }} km/h</div>
          <div style="font-size: 0.9rem; color: #666;">Wind Speed</div>
        </div>
        <div style="text-align: center;">
          <i class="pi pi-sparkles" style="font-size: 2.5rem; color: #FBB03B;"></i>
          <div style="font-size: 1.2rem; font-weight: bold; margin-top: 0.5rem;">{{ weatherData.condition }}</div>
          <div style="font-size: 0.9rem; color: #666;">Condition</div>
        </div>
      </div>
    </div>
  </div>
</template>
