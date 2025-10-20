<template>
  <div class="logo-container">
    <svg
      viewBox="0 0 320 100"
      :class="['logo-svg', themeClass]"
      xmlns="http://www.w3.org/2000/svg"
    >
      <!-- Background circle for logo icon -->
      <circle
        cx="50"
        cy="50"
        r="35"
        :fill="backgroundColor"
        :stroke="strokeColor"
        stroke-width="2"
      />

      <!-- Data flow lines (representing flow of information) -->
      <path
        d="M 20 35 Q 30 25 40 35 Q 50 45 60 35 Q 70 25 80 35"
        :stroke="accentColor"
        stroke-width="2"
        fill="none"
        opacity="0.6"
      />

      <!-- Chart bars (representing reports/analytics) -->
      <rect x="30" y="45" width="4" height="15" :fill="primaryColor" rx="2" />
      <rect x="37" y="40" width="4" height="20" :fill="primaryColor" rx="2" />
      <rect x="44" y="35" width="4" height="25" :fill="primaryColor" rx="2" />
      <rect x="51" y="42" width="4" height="18" :fill="primaryColor" rx="2" />
      <rect x="58" y="38" width="4" height="22" :fill="primaryColor" rx="2" />
      <rect x="65" y="43" width="4" height="17" :fill="primaryColor" rx="2" />

      

      <!-- Data points/dots -->
      <circle cx="35" cy="32" r="2" :fill="accentColor" opacity="0.8" />
      <circle cx="50" cy="28" r="2" :fill="accentColor" opacity="0.8" />
      <circle cx="65" cy="32" r="2" :fill="accentColor" opacity="0.8" />

      <!-- ReportFlow text -->
      <text
        x="115"
        y="52"
        :fill="textColor"
        font-family="Inter, system-ui, sans-serif"
        font-size="26"
        font-weight="700"
        letter-spacing="1px"
      >
        μ-Reporter
      </text>

     
    </svg>
  </div>
</template>

<script>
export default {
  name: 'LogoSvg',
  data() {
    return {
      backgroundColor: '#f7fafc',
      strokeColor: '#e2e8f0',
      primaryColor: '#2b6cb0',
      accentColor: '#319795',
      textColor: '#1a202c',
      subtextColor: '#718096'
    }
  }
}
</script>

<style scoped>
.logo-container {
  display: inline-block;
}

.logo-svg {
  height: auto;
  max-width: 100%;
}
</style>