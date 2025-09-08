<template>
  <div class="chart-dashboard" :class="`theme-${currentTheme}`">
    <h1>Dashboard de Gráficos con Chart.js</h1>
    
    <div class="controls-wrapper">
      <div class="chart-selector">
        <label for="chartType">Seleccionar tipo de gráfico:</label>
        <select id="chartType" v-model="selectedChartType" @change="updateChart">
          <option value="line">Línea</option>
          <option value="bar">Barras</option>
          <option value="pie">Pastel</option>
          <option value="doughnut">Dona</option>
          <option value="polarArea">Área Polar</option>
          <option value="radar">Radar</option>
          <option value="scatter">Dispersión</option>
          <option value="bubble">Burbuja</option>
          <option value="area">Área</option>
          <option value="mixed">Mixto</option>
        </select>
      </div>

      <div class="theme-selector">
        <label for="themeToggle">Tema del Dashboard:</label>
        <button id="themeToggle" @click="toggleTheme">
          Cambiar a Tema {{ currentTheme === 'light' ? 'Oscuro' : 'Claro' }}
        </button>
      </div>
    </div>

    <div class="chart-container">
      <canvas ref="chartCanvas" width="400" height="200"></canvas>
    </div>

    <div class="chart-controls">
      <button @click="addData">Agregar Datos</button>
      <button @click="removeData">Quitar Datos</button>
      <button @click="randomizeData">Aleatorizar Datos</button>
      <button @click="toggleAnimation">
        {{ animationEnabled ? 'Desactivar' : 'Activar' }} Animación
      </button>
    </div>

    <div class="chart-info">
      <h3>Información del Gráfico: {{ getChartTypeName(selectedChartType) }}</h3>
      <p>{{ getChartDescription(selectedChartType) }}</p>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, nextTick } from 'vue'

const showDataLabels = ref(true);
const showLegend = ref(false);

export default {
  name: 'ChartDashboard',
  setup() {
    const chartCanvas = ref(null)
    const selectedChartType = ref('line')
    const animationEnabled = ref(true)
    let chartInstance = null

    // --- Inicio: Funcionalidad de Theming ---
    const currentTheme = ref('light'); // 'light' o 'dark'
    const themes = {
      light: {
        gridColor: 'rgba(0, 0, 0, 0.1)',
        textColor: '#333',
        tooltipBg: '#fff',
        tooltipColor: '#333'
      },
      dark: {
        gridColor: 'rgba(255, 255, 255, 0.2)',
        textColor: '#f1f1f1',
        tooltipBg: '#222',
        tooltipColor: '#f1f1f1'
      }
    };

    const toggleTheme = () => {
      currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light';
      updateChart(); // Redibuja el gráfico con el nuevo tema
    };
    // --- Fin: Funcionalidad de Theming ---

    const baseData = {
      labels: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio'],
      datasets: [{
        label: 'Ventas 2024',
        data: [12, 19, 3, 5, 2, 3],
        backgroundColor: [
          'rgba(255, 99, 132, 0.2)', 'rgba(54, 162, 235, 0.2)',
          'rgba(255, 205, 86, 0.2)', 'rgba(75, 192, 192, 0.2)',
          'rgba(153, 102, 255, 0.2)', 'rgba(255, 159, 64, 0.2)'
        ],
        borderColor: [
          'rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)',
          'rgba(255, 205, 86, 1)', 'rgba(75, 192, 192, 1)',
          'rgba(153, 102, 255, 1)', 'rgba(255, 159, 64, 1)'
        ],
        borderWidth: 2,
        fill: false
      }]
    };

    const getChartConfig = (type) => {
      const themeColors = themes[currentTheme.value]; // Obtiene los colores del tema actual
      const config = {
        type: type,
        data: JSON.parse(JSON.stringify(baseData)), // Copia profunda para evitar mutaciones
        options: {
          responsive: true,
          maintainAspectRatio: false,
          animation: {
            duration: animationEnabled.value ? 1000 : 0
          },
          plugins: {
            legend: {
              position: 'top',
              labels: { color: themeColors.textColor }
            },
            title: {
              display: true,
              text: `Gráfico de ${getChartTypeName(type)}`,
              color: themeColors.textColor
            },
            tooltip: {
                backgroundColor: themeColors.tooltipBg,
                titleColor: themeColors.tooltipColor,
                bodyColor: themeColors.tooltipColor,
            }
          }
        }
      };

      // Configuraciones específicas por tipo
      switch (type) {
        case 'line':
        case 'area':
            config.type = 'line';
            config.data.datasets[0].fill = type === 'area';
            config.data.datasets[0].backgroundColor = type === 'area' ? 'rgba(75, 192, 192, 0.4)' : 'transparent';
            config.data.datasets[0].tension = 0.4;
            config.options.scales = {
                y: { beginAtZero: true, grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } },
                x: { grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } }
            };
            if (showDataLabels.value) {
              config.plugins = [{
                id: 'dataLabels',
                afterDatasetsDraw: function(chart) {
                  const ctx = chart.ctx
                  const dataset = chart.data.datasets[0]
                  const meta = chart.getDatasetMeta(0)
                  
                  meta.data.forEach((point, index) => {
                    const data = dataset.data[index]
                    ctx.fillStyle = currentTheme.value === 'light' ? '#666' : '#eee'
                    ctx.font = 'bold 12px Arial'
                    ctx.textAlign = 'center'
                    ctx.textBaseline = 'bottom'
                    ctx.fillText(data, point.x, point.y - 5)
                  })
                }
              }]
            }
            break;

        case 'bar':
            config.options.scales = {
                y: { beginAtZero: true, grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } },
                x: { grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } }
            };
             if (showDataLabels.value) {
              config.options.plugins.tooltip = { enabled: true }
              // Agregar plugin personalizado para mostrar valores en barras
              config.plugins = [{
                id: 'dataLabels',
                afterDatasetsDraw: function(chart) {
                  const ctx = chart.ctx
                  chart.data.datasets.forEach((dataset, i) => {
                    const meta = chart.getDatasetMeta(i)
                    meta.data.forEach((bar, index) => {
                      const data = dataset.data[index]
                      ctx.fillStyle =  currentTheme.value === 'light' ? '#666' : '#eee'
                      ctx.font = 'bold 12px Arial'
                      ctx.textAlign = 'center'
                      ctx.textBaseline = 'bottom'
                      ctx.fillText(data, bar.x, bar.y - 5)
                    })
                  })
                }
              }]
            }
            break;

        case 'pie':
        case 'doughnut':
         
          
          // Configurar la leyenda para mostrar porcentajes
          config.options.plugins.legend = {
            display: showLegend.value,
            position: 'top',
            labels: {
              generateLabels: function(chart) {
                const data = chart.data
                if (data.labels.length && data.datasets.length) {
                  const dataset = data.datasets[0]
                  const total = dataset.data.reduce((sum, value) => sum + value, 0)
                  
                  return data.labels.map((label, i) => {
                    const value = dataset.data[i]
                    const percentage = ((value / total) * 100).toFixed(1)
                    
                    return {
                      text: `${label}: ${percentage}%`,
                      fillStyle: dataset.backgroundColor[i],
                      strokeStyle: dataset.borderColor[i],
                      lineWidth: dataset.borderWidth,
                      hidden: isNaN(value) || chart.getDatasetMeta(0).data[i].hidden,
                      index: i
                    }
                  })
                }
                return []
              }
            }
          }
          
          // Plugin para mostrar el total en el centro del gráfico de dona
          config.plugins = [{
            id: 'centerText',
            beforeDraw: function(chart) {
              if (type === 'doughnut') {
                const ctx = chart.ctx
                const centerX = (chart.chartArea.left + chart.chartArea.right) / 2
                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2
                
                // Calcular el total
                const total = chart.data.datasets[0].data.reduce((sum, value) => sum + value, 0)
                
                ctx.save()
                ctx.fillStyle = currentTheme.value === 'light' ? '#333' : '#fff'
                ctx.font = 'bold 24px Arial'
                ctx.textAlign = 'center'
                ctx.textBaseline = 'middle'
                ctx.fillText('Total', centerX, centerY - 15)
                
                ctx.fillStyle =  currentTheme.value === 'light' ? '#666' : '#eee'
                ctx.font = 'bold 32px Arial'
                ctx.fillText(total.toString(), centerX, centerY + 15)
                ctx.restore()
              }
            },
            afterDatasetsDraw: function(chart) {
              if (showDataLabels.value) {
                const ctx = chart.ctx
                const total = chart.data.datasets[0].data.reduce((sum, value) => sum + value, 0)
                
                chart.data.datasets.forEach((dataset, i) => {
                  const meta = chart.getDatasetMeta(i)
                  meta.data.forEach((segment, index) => {
                    const data = dataset.data[index]
                    const percentage = ((data / total) * 100).toFixed(1)
                    const midAngle = segment.startAngle + (segment.endAngle - segment.startAngle) / 2
                    const x = segment.x + Math.cos(midAngle) * (segment.outerRadius - 30)
                    const y = segment.y + Math.sin(midAngle) * (segment.outerRadius - 30)
                    
                    ctx.fillStyle =  currentTheme.value === 'light' ? '#666' : '#eee'
                    ctx.font = 'bold 11px Arial'
                    ctx.textAlign = 'center'
                    ctx.textBaseline = 'middle'
                    ctx.fillText(`${data}`, x, y - 8)
                    ctx.fillText(`(${percentage}%)`, x, y + 8)
                  })
                })
              }
            }
          }]
          break; // No scales needed

        case 'polarArea':
            config.options.scales = {
                r: {
                    grid: { color: themeColors.gridColor },
                    ticks: { color: themeColors.textColor, backdropColor: 'transparent' },
                    pointLabels: { color: themeColors.textColor }
                }
            };
            break;

        case 'radar':
            config.data.labels = ['Velocidad', 'Confiabilidad', 'Confort', 'Seguridad', 'Eficiencia', 'Precio'];
            config.data.datasets[0] = {
                ...config.data.datasets[0],
                data: [65, 59, 90, 81, 56, 55],
                fill: true,
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderColor: 'rgba(54, 162, 235, 1)'
            };
            config.options.scales = {
                r: {
                    angleLines: { color: themeColors.gridColor },
                    grid: { color: themeColors.gridColor },
                    suggestedMin: 0,
                    suggestedMax: 100,
                    ticks: { color: themeColors.textColor, backdropColor: 'transparent' },
                    pointLabels: { color: themeColors.textColor }
                }
            };
            break;

        case 'scatter':
        case 'bubble':
            config.data.datasets[0] = {
                label: `Datos de ${type === 'scatter' ? 'Dispersión' : 'Burbuja'}`,
                data: type === 'scatter'
                    ? [{x: -10, y: 0}, {x: 0, y: 10}, {x: 10, y: 5}, {x: 0.5, y: 5.5}]
                    : [{x: 20, y: 30, r: 15}, {x: 40, y: 10, r: 10}, {x: 15, y: 22, r: 25}],
                backgroundColor: 'rgba(255, 99, 132, 0.6)'
            };
            config.options.scales = {
                y: { beginAtZero: true, grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } },
                x: { type: 'linear', position: 'bottom', grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } }
            };
            break;

        case 'mixed':
            config.type = 'bar';
            config.data.datasets = [
                {
                    label: 'Barras', type: 'bar', data: [12, 19, 3, 5, 2, 3],
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                },
                {
                    label: 'Línea', type: 'line', data: [5, 10, 15, 8, 12, 7],
                    borderColor: 'rgba(54, 162, 235, 1)', backgroundColor: 'transparent',
                    fill: false
                }
            ];
            config.options.scales = {
                y: { beginAtZero: true, grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } },
                x: { grid: { color: themeColors.gridColor }, ticks: { color: themeColors.textColor } }
            };
            if (showDataLabels.value) {
              config.plugins = [{
                id: 'dataLabels',
                afterDatasetsDraw: function(chart) {
                  const ctx = chart.ctx
                  chart.data.datasets.forEach((dataset, i) => {
                    const meta = chart.getDatasetMeta(i)
                    meta.data.forEach((element, index) => {
                      const data = dataset.data[index]
                      ctx.fillStyle =  currentTheme.value === 'light' ? '#666' : '#eee'
                      ctx.font = 'bold 10px Arial'
                      ctx.textAlign = 'center'
                      
                      if (dataset.type === 'bar') {
                        ctx.textBaseline = 'bottom'
                        ctx.fillText(data, element.x, element.y - 5)
                      } else {
                        ctx.textBaseline = 'bottom'
                        ctx.fillText(data, element.x, element.y - 5)
                      }
                    })
                  })
                }
              }]
            }
            break;
      }
      return config;
    }

    const createChart = async (type) => {
      await nextTick();
      if (chartInstance) chartInstance.destroy();
      
      const ctx = chartCanvas.value.getContext('2d');
      const config = getChartConfig(type);
      
      const { Chart, registerables } = await import('chart.js');
      Chart.register(...registerables);
      
      chartInstance = new Chart(ctx, config);
    }

    const updateChart = () => {
      createChart(selectedChartType.value);
    }

    // Funciones de control de datos (sin cambios)
    const addData = () => {
      if (!chartInstance) return;
      const randomValue = Math.floor(Math.random() * 50) + 1;
      if (['scatter', 'bubble'].includes(selectedChartType.value)) {
        chartInstance.data.datasets[0].data.push({
          x: Math.random() * (selectedChartType.value === 'bubble' ? 60 : 20) - 10,
          y: Math.random() * (selectedChartType.value === 'bubble' ? 40 : 20) - 10,
          r: selectedChartType.value === 'bubble' ? Math.random() * 20 + 5 : undefined
        });
      } else {
        chartInstance.data.labels.push(`Dato ${chartInstance.data.labels.length + 1}`);
        chartInstance.data.datasets.forEach(d => d.data.push(randomValue));
      }
      chartInstance.update();
    }

    const removeData = () => {
      if (!chartInstance) return;
      if (['scatter', 'bubble'].includes(selectedChartType.value)) {
        chartInstance.data.datasets[0].data.pop();
      } else {
        chartInstance.data.labels.pop();
        chartInstance.data.datasets.forEach(d => d.data.pop());
      }
      chartInstance.update();
    }

    const randomizeData = () => {
      if (!chartInstance) return;
      chartInstance.data.datasets.forEach(dataset => {
        if (['scatter', 'bubble'].includes(selectedChartType.value)) {
          dataset.data = dataset.data.map(() => ({
            x: Math.random() * (selectedChartType.value === 'bubble' ? 60 : 20) - 10,
            y: Math.random() * (selectedChartType.value === 'bubble' ? 40 : 20) - 10,
            r: selectedChartType.value === 'bubble' ? Math.random() * 20 + 5 : undefined
          }));
        } else {
          dataset.data = dataset.data.map(() => Math.floor(Math.random() * 50) + 1);
        }
      });
      chartInstance.update();
    }

    const toggleAnimation = () => {
      animationEnabled.value = !animationEnabled.value;
      updateChart();
    }
    
    // Funciones de utilidad (sin cambios)
    const getChartTypeName = (type) => ({ line: 'Línea', bar: 'Barras', pie: 'Pastel', doughnut: 'Dona', polarArea: 'Área Polar', radar: 'Radar', scatter: 'Dispersión', bubble: 'Burbuja', area: 'Área', mixed: 'Mixto' }[type] || type);
    const getChartDescription = (type) => ({ line: 'Ideal para mostrar tendencias a lo largo del tiempo.', bar: 'Perfecto para comparar categorías de datos.', pie: 'Excelente para mostrar proporciones de un total.', doughnut: 'Similar al gráfico de pastel pero con un centro hueco.', polarArea: 'Combina características de gráficos de pastel y radar.', radar: 'Útil para mostrar múltiples métricas en un formato circular.', scatter: 'Ideal para mostrar correlaciones entre dos variables.', bubble: 'Permite mostrar tres dimensiones de datos usando x, y y tamaño.', area: 'Similar al gráfico de línea pero con el área sombreada.', mixed: 'Combina diferentes tipos de gráficos en uno solo.' }[type] || 'Descripción no disponible.');

    onMounted(() => {
      createChart(selectedChartType.value);
    });

    return {
      chartCanvas, selectedChartType, animationEnabled, currentTheme,
      updateChart, addData, removeData, randomizeData, toggleAnimation, toggleTheme,
      getChartTypeName, getChartDescription
    };
  }
}
</script>

<style scoped>
/* --- Estilos Base --- */
.chart-dashboard {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  transition: background-color 0.4s ease, color 0.4s ease;
}

h1 {
  text-align: center;
  margin-bottom: 30px;
}

.controls-wrapper {
  display: flex;
  justify-content: center;
  gap: 40px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.chart-selector, .theme-selector {
  text-align: center;
}

.chart-selector label, .theme-selector label {
  display: block;
  margin-bottom: 10px;
  font-weight: bold;
}

.chart-selector select, .theme-selector button {
  padding: 10px 15px;
  border: 2px solid #ddd;
  border-radius: 5px;
  font-size: 16px;
  cursor: pointer;
  min-width: 200px;
  transition: border-color 0.3s ease, background-color 0.3s ease;
}

.theme-selector button {
  background-color: #6c757d;
  color: white;
  font-weight: bold;
}

.chart-container {
  position: relative;
  height: 450px;
  margin: 20px 0;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 10px;
  transition: background-color 0.4s ease, border-color 0.4s ease;
}

.chart-controls {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin: 20px 0;
  flex-wrap: wrap;
}

.chart-controls button {
  padding: 10px 20px; border: none; border-radius: 5px; cursor: pointer;
  font-size: 14px; font-weight: bold; transition: all 0.3s ease;
}

.chart-controls button:nth-child(1) { background-color: #4CAF50; color: white; }
.chart-controls button:nth-child(2) { background-color: #f44336; color: white; }
.chart-controls button:nth-child(3) { background-color: #2196F3; color: white; }
.chart-controls button:nth-child(4) { background-color: #FF9800; color: white; }
.chart-controls button:hover { transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }

.chart-info {
  border: 1px solid;
  border-radius: 8px;
  padding: 20px;
  margin-top: 20px;
  transition: background-color 0.4s ease, border-color 0.4s ease;
}

.chart-info h3 { margin-bottom: 10px; }
.chart-info p { line-height: 1.6; }

/* --- Tema Claro (Default) --- */
.theme-light { background-color: #fff; color: #333; }
.theme-light h1 { color: #333; }
.theme-light .chart-selector label, .theme-light .theme-selector label { color: #555; }
.theme-light .chart-selector select { background-color: white; border-color: #ddd; color: #333; }
.theme-light .chart-selector select:focus { border-color: #4CAF50; }
.theme-light .chart-container { background-color: #fafafa; border-color: #ddd; }
.theme-light .chart-info { background-color: #f0f8ff; border-color: #b0d4f1; }
.theme-light .chart-info h3 { color: #2c5aa0; }
.theme-light .chart-info p { color: #555; }

/* --- Tema Oscuro --- */
.theme-dark { background-color: #1e1e1e; color: #f1f1f1; }
.theme-dark h1 { color: #f1f1f1; }
.theme-dark .chart-selector label, .theme-dark .theme-selector label { color: #a0cff1; }
.theme-dark .chart-selector select { background-color: #333; border-color: #555; color: #f1f1f1; }
.theme-dark .chart-selector select:focus { border-color: #4CAF50; }
.theme-dark .theme-selector button { background-color: #f8f9fa; color: #333; }
.theme-dark .chart-container { background-color: #2a2a2a; border-color: #555; }
.theme-dark .chart-info { background-color: #2c3e50; border-color: #34495e; }
.theme-dark .chart-info h3 { color: #5dade2; }
.theme-dark .chart-info p { color: #bdc3c7; }

@media (max-width: 768px) {
  .chart-dashboard { padding: 10px; }
  .chart-container { height: 300px; padding: 10px; }
  .controls-wrapper { flex-direction: column; gap: 20px; }
  .chart-controls { flex-direction: column; align-items: center; }
  .chart-controls button { width: 200px; margin: 5px 0; }
}
</style>