<template>
  <svg
    :width="width"
    :height="height"
    viewBox="0 0 410 100"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
  >
    <!-- Main container -->
    <rect
      width="410"
      height="100"
      rx="12"
      :fill="isDark ? '#1f2937' : '#f8fafc'"
      :stroke="isDark ? '#374151' : '#e2e8f0'"
      stroke-width="1"
    />

    <!-- Logo symbol -->
    <circle
      cx="50"
      cy="50"
      r="20"
      :fill="isDark ? '#10b981' : '#059669'"
    />

    <!-- Inner circle -->
    <circle
      cx="50"
      cy="50"
      r="12"
      :fill="isDark ? '#065f46' : '#047857'"
    />

    <!-- Text -->
    <text
      x="85"
      y="35"
      font-family="Arial, sans-serif"
      font-size="24"
      font-weight="bold"
      :fill="isDark ? '#f9fafb' : '#111827'"
    >
      CRS
    </text>

    <!-- Subtitle -->
    <text
      x="85"
      y="55"
      font-family="Arial, sans-serif"
      font-size="12"
      :fill="isDark ? '#9ca3af' : '#6b7280'"
    >
      System
    </text>

    <!-- Decorative elements -->
    <rect
      x="85"
      y="65"
      width="60"
      height="2"
      :fill="isDark ? '#10b981' : '#059669'"
    />
  </svg>
</template>

<script setup>
import { computed } from 'vue'
import { useLayout } from '@/layout/composables/layout.js'

const props = defineProps({
  width: {
    type: [String, Number],
    default: 200
  },
  height: {
    type: [String, Number],
    default: 50
  }
})

const { isDarkTheme } = useLayout()
const isDark = computed(() => isDarkTheme.value)
</script>