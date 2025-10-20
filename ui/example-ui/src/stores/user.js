import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useUserStore = defineStore('user', () => {
  // State
  const user = ref(null)
  const isLoggedIn = ref(false)
  const selectedRoom = ref('Living Room')
  const homeStats = ref({
    humidity: 35,
    temperature: 15
  })

  const controls = ref({
    refrigerator: false,
    temperature: true,
    airConditioner: false,
    lights: false
  })

  const devices = ref({
    refrigerator: true,
    router: true,
    music: true,
    lamps: true
  })

  const temperatureOn = ref(true)
  const currentTemp = ref(25)
  const targetTemp = ref(25)

  // Getters
  const userName = computed(() => user.value?.username || 'Scarlett')
  const userInitial = computed(() => userName.value.charAt(0).toUpperCase())

  // Actions
  function login(username, password) {
    if (username && password) {
      user.value = {
        username,
        email: `${username}@smarthome.com`,
        role: 'Admin',
        avatar: username.charAt(0).toUpperCase()
      }
      isLoggedIn.value = true
      return true
    }
    return false
  }

  function logout() {
    user.value = null
    isLoggedIn.value = false
    // Reset states to defaults
    selectedRoom.value = 'Living Room'
    controls.value = {
      refrigerator: false,
      temperature: true,
      airConditioner: false,
      lights: false
    }
    devices.value = {
      refrigerator: true,
      router: true,
      music: true,
      lamps: true
    }
    temperatureOn.value = true
    currentTemp.value = 25
    targetTemp.value = 25
  }

  function toggleControl(control) {
    if (controls.value[control] !== undefined) {
      controls.value[control] = !controls.value[control]
      return controls.value[control]
    }
    return false
  }

  function toggleDevice(device) {
    if (devices.value[device] !== undefined) {
      devices.value[device] = !devices.value[device]
      return devices.value[device]
    }
    return false
  }

  function increaseTemp() {
    if (targetTemp.value < 30) {
      targetTemp.value++
    }
  }

  function decreaseTemp() {
    if (targetTemp.value > 10) {
      targetTemp.value--
    }
  }

  function updateCurrentTemp() {
    if (currentTemp.value < targetTemp.value) {
      currentTemp.value++
    } else if (currentTemp.value > targetTemp.value) {
      currentTemp.value--
    }
  }

  function setRoom(room) {
    selectedRoom.value = room
  }

  return {
    // State
    user,
    isLoggedIn,
    selectedRoom,
    homeStats,
    controls,
    devices,
    temperatureOn,
    currentTemp,
    targetTemp,
    // Getters
    userName,
    userInitial,
    // Actions
    login,
    logout,
    toggleControl,
    toggleDevice,
    increaseTemp,
    decreaseTemp,
    updateCurrentTemp,
    setRoom
  }
})
