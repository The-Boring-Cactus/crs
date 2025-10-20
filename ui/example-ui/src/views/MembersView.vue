<script setup>
import { ref } from 'vue'

const members = ref([
  {
    id: 1,
    name: 'Scarlett',
    role: 'Admin',
    email: 'scarlett@smarthome.com',
    avatar: 'S',
    color: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    permissions: ['Full Access', 'Manage Users', 'Security', 'Analytics'],
    lastSeen: 'Active now'
  },
  {
    id: 2,
    name: 'Nariya',
    role: 'Member',
    email: 'nariya@smarthome.com',
    avatar: 'N',
    color: 'linear-gradient(135deg, #FF6B6B 0%, #FF5252 100%)',
    permissions: ['Full Access', 'Control Devices', 'View Analytics'],
    lastSeen: '2 hours ago'
  },
  {
    id: 3,
    name: 'Riya',
    role: 'Member',
    email: 'riya@smarthome.com',
    avatar: 'R',
    color: 'linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%)',
    permissions: ['Full Access', 'Control Devices'],
    lastSeen: 'Active now'
  },
  {
    id: 4,
    name: 'Dad',
    role: 'Member',
    email: 'dad@smarthome.com',
    avatar: 'D',
    color: 'linear-gradient(135deg, #FBB03B 0%, #F79E1B 100%)',
    permissions: ['Full Access', 'Security', 'Control Devices'],
    lastSeen: '5 hours ago'
  },
  {
    id: 5,
    name: 'Mom',
    role: 'Member',
    email: 'mom@smarthome.com',
    avatar: 'M',
    color: 'linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%)',
    permissions: ['Full Access', 'Control Devices', 'View Analytics'],
    lastSeen: '1 hour ago'
  }
])
</script>

<template>
  <div class="home-section">
    <div class="section-header">
      <h3>
        <i class="pi pi-users" style="margin-right: 0.5rem;"></i>
        Family Members
      </h3>
      <Button label="Invite Member" icon="pi pi-user-plus" class="p-button-rounded" />
    </div>

    <!-- Members Grid -->
    <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 2rem;">
      <div
        v-for="member in members"
        :key="member.id"
        style="background: white; border-radius: 20px; padding: 2rem; box-shadow: 0 2px 10px rgba(0,0,0,0.05);"
      >
        <div style="display: flex; align-items: flex-start; gap: 1.5rem;">
          <!-- Avatar -->
          <div
            class="member-avatar"
            :style="{ background: member.color, width: '80px', height: '80px', fontSize: '2rem' }"
          >
            {{ member.avatar }}
          </div>

          <!-- Info -->
          <div style="flex: 1;">
            <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 0.5rem;">
              <div>
                <h3 style="margin-bottom: 0.3rem;">{{ member.name }}</h3>
                <div style="color: #8B5CF6; font-weight: 600; font-size: 0.9rem;">{{ member.role }}</div>
              </div>
              <Button
                v-if="member.role !== 'Admin'"
                icon="pi pi-ellipsis-v"
                class="p-button-rounded p-button-text"
                style="color: #666;"
              />
            </div>

            <div style="color: #666; font-size: 0.9rem; margin-bottom: 1rem;">
              <i class="pi pi-envelope" style="margin-right: 0.5rem;"></i>
              {{ member.email }}
            </div>

            <div style="margin-bottom: 1rem;">
              <div style="font-weight: 600; font-size: 0.9rem; margin-bottom: 0.5rem; color: #333;">Permissions</div>
              <div style="display: flex; flex-wrap: wrap; gap: 0.5rem;">
                <span
                  v-for="permission in member.permissions"
                  :key="permission"
                  style="background: #F8F8F8; padding: 0.3rem 0.8rem; border-radius: 15px; font-size: 0.85rem; color: #666;"
                >
                  {{ permission }}
                </span>
              </div>
            </div>

            <div style="display: flex; align-items: center; gap: 0.5rem; color: #999; font-size: 0.85rem;">
              <i class="pi pi-clock"></i>
              Last seen: <strong style="color: #666;">{{ member.lastSeen }}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Activity Log -->
    <div style="margin-top: 2rem; background: #F8F8F8; border-radius: 20px; padding: 2rem;">
      <h4 style="margin-bottom: 1.5rem;">
        <i class="pi pi-history" style="margin-right: 0.5rem;"></i>
        Recent Activity
      </h4>
      <div style="display: flex; flex-direction: column; gap: 1rem;">
        <div style="display: flex; align-items: center; gap: 1rem; padding: 1rem; background: white; border-radius: 15px;">
          <div class="member-avatar" style="width: 40px; height: 40px; font-size: 1rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">S</div>
          <div style="flex: 1;">
            <div style="font-weight: 600; color: #333;">Scarlett turned on Living Room lights</div>
            <div style="font-size: 0.85rem; color: #999;">2 minutes ago</div>
          </div>
        </div>
        <div style="display: flex; align-items: center; gap: 1rem; padding: 1rem; background: white; border-radius: 15px;">
          <div class="member-avatar" style="width: 40px; height: 40px; font-size: 1rem; background: linear-gradient(135deg, #4FC3F7 0%, #29B6F6 100%);">R</div>
          <div style="flex: 1;">
            <div style="font-weight: 600; color: #333;">Riya adjusted temperature to 24°C</div>
            <div style="font-size: 0.85rem; color: #999;">15 minutes ago</div>
          </div>
        </div>
        <div style="display: flex; align-items: center; gap: 1rem; padding: 1rem; background: white; border-radius: 15px;">
          <div class="member-avatar" style="width: 40px; height: 40px; font-size: 1rem; background: linear-gradient(135deg, #66BB6A 0%, #4CAF50 100%);">M</div>
          <div style="flex: 1;">
            <div style="font-weight: 600; color: #333;">Mom activated security system</div>
            <div style="font-size: 0.85rem; color: #999;">1 hour ago</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
