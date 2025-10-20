<script setup>
import { ref } from 'vue'

const stats = ref({
  totalDevices: 24,
  activeDevices: 18,
  energyConsumption: 1245.8,
  costSavings: 23.5
})

const chartPeriod = ref('Month')
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-chart-bar" style="margin-right: 0.5rem;"></i>
        Analytics & Reports
      </h3>
      <div style="display: flex; gap: 0.5rem;">
        <Button label="Export" icon="pi pi-download" class="p-button-outlined" />
        <select v-model="chartPeriod" style="background: #F5F5F5; border: none; border-radius: 20px; padding: 0.5rem 1rem; cursor: pointer;">
          <option>Week</option>
          <option>Month</option>
          <option>Year</option>
        </select>
      </div>
    </div>

    <!-- Overview Stats -->
    <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem;">
      <div class="control-card" style="background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); color: white;">
        <i class="pi pi-box" style="font-size: 2.5rem; margin-bottom: 1rem;"></i>
        <div style="font-size: 2rem; font-weight: bold;">{{ stats.totalDevices }}</div>
        <div style="font-size: 0.9rem; opacity: 0.9;">Total Devices</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%); color: white;">
        <i class="pi pi-check-circle" style="font-size: 2.5rem; margin-bottom: 1rem;"></i>
        <div style="font-size: 2rem; font-weight: bold;">{{ stats.activeDevices }}</div>
        <div style="font-size: 0.9rem; opacity: 0.9;">Active Now</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%); color: white;">
        <i class="pi pi-bolt" style="font-size: 2.5rem; margin-bottom: 1rem;"></i>
        <div style="font-size: 2rem; font-weight: bold;">{{ stats.energyConsumption }}</div>
        <div style="font-size: 0.9rem; opacity: 0.9;">kWh This Month</div>
      </div>
      <div class="control-card" style="background: linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%); color: white;">
        <i class="pi pi-dollar" style="font-size: 2.5rem; margin-bottom: 1rem;"></i>
        <div style="font-size: 2rem; font-weight: bold;">${{ stats.costSavings }}</div>
        <div style="font-size: 0.9rem; opacity: 0.9;">Savings This Month</div>
      </div>
    </div>

    <!-- Power Consumption Chart -->
    <div class="power-chart" style="margin-bottom: 2rem;">
      <div class="chart-header">
        <div style="display: flex; align-items: center; gap: 1rem;">
          <div style="display: flex; align-items: center; gap: 0.5rem;">
            <div style="width: 10px; height: 10px; background: #FF6B6B; border-radius: 50%;"></div>
            <span>Electricity Consumed</span>
          </div>
          <div style="display: flex; align-items: center; gap: 0.5rem;">
            <div style="width: 10px; height: 10px; background: #4CAF50; border-radius: 50%;"></div>
            <span>Solar Generated</span>
          </div>
        </div>
        <div style="display: flex; align-items: center; gap: 1rem;">
          <span class="chart-percentage">73% Spending</span>
        </div>
      </div>
      <div style="height: 250px; background: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjE1MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cGF0aCBkPSJNMCwxMDAgQzUwLDgwIDEwMCw2MCAxNTAsNzAgQzIwMCw4MCAyNTAsNDAgMzAwLDUwIEMzNTAsNjAgNDAwLDMwIDQwMCwzMCBMNDAwLDE1MCBMMCwxNTAgWiIgZmlsbD0iI0ZGQjNBQSIgb3BhY2l0eT0iMC4zIi8+CiAgPHBhdGggZD0iTTAsMTAwIEM1MCw4MCAxMDAsNjAgMTUwLDcwIEMyMDAsODAgMjUwLDQwIDMwMCw1MCBDMzUwLDYwIDQwMCwzMCA0MDAsMzAiIHN0cm9rZT0iI0ZGNkI2QiIgc3Ryb2tlLXdpZHRoPSIyIiBmaWxsPSJub25lIi8+Cjwvc3ZnPg==') no-repeat center; background-size: cover; border-radius: 10px; margin-top: 1rem; display: flex; align-items: center; justify-content: center; color: #666;"></div>
      <div style="display: flex; justify-content: space-between; margin-top: 1rem; font-size: 0.8rem; color: #999;">
        <span>Jan</span>
        <span>Feb</span>
        <span>Mar</span>
        <span>Apr</span>
        <span>May</span>
        <span>Jun</span>
        <span>Jul</span>
        <span>Aug</span>
        <span>Sep</span>
        <span>Oct</span>
        <span>Nov</span>
        <span>Dec</span>
      </div>
    </div>

    <!-- Device Usage -->
    <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 2rem;">
      <!-- Most Used Devices -->
      <div style="background: white; border-radius: 20px; padding: 2rem; box-shadow: 0 2px 10px rgba(0,0,0,0.05);">
        <h4 style="margin-bottom: 1.5rem;">
          <i class="pi pi-star" style="margin-right: 0.5rem; color: #FBB03B;"></i>
          Most Used Devices
        </h4>
        <div style="display: flex; flex-direction: column; gap: 1rem;">
          <div style="display: flex; align-items: center; gap: 1rem;">
            <div style="width: 40px; height: 40px; background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%); border-radius: 10px; display: flex; align-items: center; justify-content: center;">
              <i class="pi pi-sun" style="color: white;"></i>
            </div>
            <div style="flex: 1;">
              <div style="font-weight: 600;">Living Room Lights</div>
              <div style="font-size: 0.85rem; color: #999;">8.5 hrs/day</div>
            </div>
            <div style="font-weight: bold; color: #8B5CF6;">85%</div>
          </div>
          <div style="display: flex; align-items: center; gap: 1rem;">
            <div style="width: 40px; height: 40px; background: linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%); border-radius: 10px; display: flex; align-items: center; justify-content: center;">
              <i class="pi pi-cloud" style="color: white;"></i>
            </div>
            <div style="flex: 1;">
              <div style="font-weight: 600;">Air Conditioner</div>
              <div style="font-size: 0.85rem; color: #999;">6.2 hrs/day</div>
            </div>
            <div style="font-weight: bold; color: #4FC3F7;">72%</div>
          </div>
          <div style="display: flex; align-items: center; gap: 1rem;">
            <div style="width: 40px; height: 40px; background: linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%); border-radius: 10px; display: flex; align-items: center; justify-content: center;">
              <i class="pi pi-wifi" style="color: white;"></i>
            </div>
            <div style="flex: 1;">
              <div style="font-weight: 600;">WiFi Router</div>
              <div style="font-size: 0.85rem; color: #999;">24 hrs/day</div>
            </div>
            <div style="font-weight: bold; color: #FBB03B;">100%</div>
          </div>
          <div style="display: flex; align-items: center; gap: 1rem;">
            <div style="width: 40px; height: 40px; background: linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%); border-radius: 10px; display: flex; align-items: center; justify-content: center;">
              <i class="pi pi-inbox" style="color: white;"></i>
            </div>
            <div style="flex: 1;">
              <div style="font-weight: 600;">Refrigerator</div>
              <div style="font-size: 0.85rem; color: #999;">24 hrs/day</div>
            </div>
            <div style="font-weight: bold; color: #66BB6A;">100%</div>
          </div>
        </div>
      </div>

      <!-- Energy by Room -->
      <div style="background: white; border-radius: 20px; padding: 2rem; box-shadow: 0 2px 10px rgba(0,0,0,0.05);">
        <h4 style="margin-bottom: 1.5rem;">
          <i class="pi pi-bolt" style="margin-right: 0.5rem; color: #FF6B6B;"></i>
          Energy by Room
        </h4>
        <div style="display: flex; flex-direction: column; gap: 1rem;">
          <div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
              <span>Living Room</span>
              <strong>385 kWh</strong>
            </div>
            <div style="height: 8px; background: #F0F0F0; border-radius: 10px; overflow: hidden;">
              <div style="height: 100%; width: 85%; background: linear-gradient(90deg, #8B5CF6 0%, #7C3AED 100%);"></div>
            </div>
          </div>
          <div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
              <span>Kitchen</span>
              <strong>320 kWh</strong>
            </div>
            <div style="height: 8px; background: #F0F0F0; border-radius: 10px; overflow: hidden;">
              <div style="height: 100%; width: 70%; background: linear-gradient(90deg, #FBB03B 0%, #F79E1B 100%);"></div>
            </div>
          </div>
          <div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
              <span>Bedroom</span>
              <strong>245 kWh</strong>
            </div>
            <div style="height: 8px; background: #F0F0F0; border-radius: 10px; overflow: hidden;">
              <div style="height: 100%; width: 55%; background: linear-gradient(90deg, #4FC3F7 0%, #29B6F6 100%);"></div>
            </div>
          </div>
          <div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
              <span>Bathroom</span>
              <strong>165 kWh</strong>
            </div>
            <div style="height: 8px; background: #F0F0F0; border-radius: 10px; overflow: hidden;">
              <div style="height: 100%; width: 35%; background: linear-gradient(90deg, #FF6B6B 0%, #FF5252 100%);"></div>
            </div>
          </div>
          <div>
            <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
              <span>Office</span>
              <strong>130 kWh</strong>
            </div>
            <div style="height: 8px; background: #F0F0F0; border-radius: 10px; overflow: hidden;">
              <div style="height: 100%; width: 28%; background: linear-gradient(90deg, #66BB6A 0%, #4CAF50 100%);"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
