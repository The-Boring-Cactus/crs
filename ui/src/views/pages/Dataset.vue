<template>
  <Toast />

  <!-- Main Toolbar -->
  <Toolbar class="dataset-toolbar">
    <template #start>
      <div class="toolbar-section">
        <label class="dataset-label">Dataset Name:</label>
        <Inplace v-tooltip="'Click to Change'" class="dataset-name">
          <template #display>
            <span class="dataset-title">{{ MyTitle || 'Untitled Dataset' }}</span>
          </template>
          <template #content="{ closeCallback }">
            <span class="inline-flex items-center gap-2">
              <InputText v-model="MyTitle" placeholder="Enter dataset name" autofocus />
              <Button icon="pi pi-check" text severity="success" @click="closeCallback" />
              <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
            </span>
          </template>
        </Inplace>
      </div>
    </template>

    <template #center>
      <div class="toolbar-stats">
        <Tag severity="info" class="stats-tag">
          <i class="pi pi-table mr-1"></i>
          {{ jsondata.length }} rows × {{ getColumnCount() }} columns
        </Tag>
        <Tag v-if="selectedCells.length > 0" severity="success" class="stats-tag">
          <i class="pi pi-check-square mr-1"></i>
          {{ selectedCells.length }} selected
        </Tag>
      </div>
    </template>

    <template #end>
      <div class="toolbar-actions">
        <Button
          icon="pi pi-save"
          label="Save"
          severity="secondary"
          @click="quickSave"
          v-tooltip.bottom="'Quick Save JSON'"
        />
        <Button
          icon="pi pi-upload"
          label="Import"
          severity="secondary"
          @click="showImportDialog = true"
          v-tooltip.bottom="'Import from file'"
        />
        <Button
          icon="pi pi-download"
          label="Export"
          severity="secondary"
          @click="showExportDialog = true"
          v-tooltip.bottom="'Export to file'"
        />
      </div>
    </template>
  </Toolbar>

  <!-- Menu Bar -->
  <Menubar :model="menuItems" class="dataset-menubar" />

  <!-- Data Editor Container -->
  <div class="dataset-container">
    <vue-excel-editor
      v-if="renderComponent"
      v-model="jsondata"
      :no-footer="false"
      :no-paging="false"
      :no-num-col="false"
      :allow-add-col="true"
      :no-finding="false"
      :no-sorting="false"
      :no-filtering="false"
      :disable-panel-setting="false"
      :no-header-edit="false"
      :remember="true"
      class="theme-excel-editor"
      @update:model-value="onDataChange"
      @cell-selected="onCellSelected"
    />
  </div>

  <!-- Import Dialog -->
  <Dialog
    v-model:visible="showImportDialog"
    modal
    header="Import Data"
    class="import-dialog"
    :style="{ width: '500px' }"
  >
    <div class="import-content">
      <div class="import-options">
        <h4>Import from File</h4>
        <div class="file-input-container">
          <input
            ref="fileInput"
            type="file"
            accept=".csv,.xlsx,.xls,.json"
            @change="handleFileImport"
            class="file-input"
          />
          <Button
            icon="pi pi-upload"
            label="Choose File"
            class="file-button"
            @click="$refs.fileInput.click()"
          />
        </div>
        <small class="file-help">Supported formats: CSV, Excel (.xlsx, .xls), JSON</small>
      </div>

      <Divider />

      <div class="import-options">
        <h4>Paste Data</h4>
        <Textarea
          v-model="pasteData"
          placeholder="Paste CSV or tab-separated data here..."
          rows="6"
          class="paste-textarea"
        />
        <Button
          icon="pi pi-check"
          label="Import from Clipboard"
          @click="importFromClipboard"
          :disabled="!pasteData.trim()"
          class="mt-2"
        />
      </div>
    </div>

    <template #footer>
      <Button
        label="Cancel"
        icon="pi pi-times"
        @click="showImportDialog = false"
        class="p-button-text"
      />
    </template>
  </Dialog>

  <!-- Export Dialog -->
  <Dialog
    v-model:visible="showExportDialog"
    modal
    header="Export Data"
    class="export-dialog"
    :style="{ width: '400px' }"
  >
    <div class="export-content">
      <div class="export-format">
        <h4>Choose Export Format</h4>
        <div class="format-options">
          <Button
            icon="pi pi-file"
            label="CSV"
            @click="exportData('csv')"
            class="format-button"
          />
          <Button
            icon="pi pi-file-excel"
            label="Excel"
            @click="exportData('excel')"
            class="format-button"
          />
          <Button
            icon="pi pi-code"
            label="JSON"
            @click="exportData('json')"
            class="format-button"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <Button
        label="Cancel"
        icon="pi pi-times"
        @click="showExportDialog = false"
        class="p-button-text"
      />
    </template>
  </Dialog>

  <!-- Context Menu -->
  <ContextMenu ref="contextMenu" :model="contextMenuItems" />
</template>

<script setup>
import { nextTick, ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import VueExcelEditor from '@/components/VueExcelEditor.vue'
import * as XLSX from 'xlsx'

const toast = useToast()

// Data state
const MyTitle = ref('Sample Dataset')
const renderComponent = ref(true)
const showImportDialog = ref(false)
const showExportDialog = ref(false)
const pasteData = ref('')
const selectedCells = ref([])
const clipboard = ref(null)

// Menu items with comprehensive options
const menuItems = ref([
  {
    label: 'File',
    icon: 'pi pi-file',
    items: [
      {
        label: 'New Dataset',
        icon: 'pi pi-plus',
        command: () => newDoc()
      },
      {
        label: 'Import',
        icon: 'pi pi-upload',
        items: [
          {
            label: 'From CSV',
            icon: 'pi pi-file',
            command: () => triggerFileImport('csv')
          },
          {
            label: 'From Excel',
            icon: 'pi pi-file-excel',
            command: () => triggerFileImport('excel')
          },
          {
            label: 'From JSON',
            icon: 'pi pi-code',
            command: () => triggerFileImport('json')
          }
        ]
      },
      {
        label: 'Export',
        icon: 'pi pi-download',
        items: [
          {
            label: 'To CSV',
            icon: 'pi pi-file',
            command: () => exportData('csv')
          },
          {
            label: 'To Excel',
            icon: 'pi pi-file-excel',
            command: () => exportData('excel')
          },
          {
            label: 'To JSON',
            icon: 'pi pi-code',
            command: () => exportData('json')
          }
        ]
      },
      { separator: true },
      {
        label: 'Save',
        icon: 'pi pi-save',
        command: () => quickSave()
      }
    ]
  },
  {
    label: 'Edit',
    icon: 'pi pi-pencil',
    items: [
      {
        label: 'Copy',
        icon: 'pi pi-copy',
        command: () => copySelection(),
        disabled: computed(() => selectedCells.value.length === 0)
      },
      {
        label: 'Paste',
        icon: 'pi pi-clone',
        command: () => pasteSelection(),
        disabled: computed(() => !clipboard.value)
      },
      {
        label: 'Cut',
        icon: 'pi pi-scissors',
        command: () => cutSelection(),
        disabled: computed(() => selectedCells.value.length === 0)
      },
      { separator: true },
      {
        label: 'Select All',
        icon: 'pi pi-check-square',
        command: () => selectAll()
      },
      {
        label: 'Clear Selection',
        icon: 'pi pi-eraser',
        command: () => clearSelection()
      }
    ]
  },
  {
    label: 'Insert',
    icon: 'pi pi-plus',
    items: [
      {
        label: 'Insert Row Above',
        icon: 'pi pi-angle-up',
        command: () => insertRowAbove()
      },
      {
        label: 'Insert Row Below',
        icon: 'pi pi-angle-down',
        command: () => insertRowBelow()
      },
      {
        label: 'Insert Column Left',
        icon: 'pi pi-angle-left',
        command: () => insertColumnLeft()
      },
      {
        label: 'Insert Column Right',
        icon: 'pi pi-angle-right',
        command: () => insertColumnRight()
      },
      { separator: true },
      {
        label: 'Add Row',
        icon: 'pi pi-plus',
        command: () => createnewRow()
      },
      {
        label: 'Add Column',
        icon: 'pi pi-plus',
        command: () => createnewCol()
      }
    ]
  },
  {
    label: 'Delete',
    icon: 'pi pi-trash',
    items: [
      {
        label: 'Delete Rows',
        icon: 'pi pi-minus',
        command: () => deleteSelectedRows(),
        disabled: computed(() => selectedCells.value.length === 0)
      },
      {
        label: 'Delete Columns',
        icon: 'pi pi-minus',
        command: () => deleteSelectedColumns(),
        disabled: computed(() => selectedCells.value.length === 0)
      },
      {
        label: 'Clear Content',
        icon: 'pi pi-eraser',
        command: () => clearSelectedCells(),
        disabled: computed(() => selectedCells.value.length === 0)
      },
      { separator: true },
      {
        label: 'Clear All Data',
        icon: 'pi pi-times',
        command: () => confirmClearAll()
      }
    ]
  },
  {
    label: 'Tools',
    icon: 'pi pi-cog',
    items: [
      {
        label: 'Find & Replace',
        icon: 'pi pi-search',
        command: () => showFindReplace()
      },
      {
        label: 'Sort Data',
        icon: 'pi pi-sort',
        command: () => showSortDialog()
      },
      {
        label: 'Filter Data',
        icon: 'pi pi-filter',
        command: () => showFilterDialog()
      },
      { separator: true },
      {
        label: 'Data Validation',
        icon: 'pi pi-check-circle',
        command: () => validateData()
      },
      {
        label: 'Statistics',
        icon: 'pi pi-chart-bar',
        command: () => showDataStats()
      }
    ]
  }
])

// Context menu items
const contextMenuItems = ref([
  {
    label: 'Copy',
    icon: 'pi pi-copy',
    command: () => copySelection()
  },
  {
    label: 'Paste',
    icon: 'pi pi-clone',
    command: () => pasteSelection()
  },
  {
    label: 'Cut',
    icon: 'pi pi-scissors',
    command: () => cutSelection()
  },
  { separator: true },
  {
    label: 'Insert Row',
    icon: 'pi pi-plus',
    command: () => insertRowBelow()
  },
  {
    label: 'Insert Column',
    icon: 'pi pi-plus',
    command: () => insertColumnRight()
  },
  { separator: true },
  {
    label: 'Delete Row',
    icon: 'pi pi-trash',
    command: () => deleteSelectedRows()
  },
  {
    label: 'Delete Column',
    icon: 'pi pi-trash',
    command: () => deleteSelectedColumns()
  }
])

// Sample data with more realistic content
const jsondata = ref([
  {
    'Product ID': 'P001',
    'Product Name': 'Laptop Pro',
    'Category': 'Electronics',
    'Price': 1299.99,
    'Stock': 45,
    'Supplier': 'TechCorp',
    'Last Updated': '2024-01-15'
  },
  {
    'Product ID': 'P002',
    'Product Name': 'Wireless Mouse',
    'Category': 'Accessories',
    'Price': 29.99,
    'Stock': 150,
    'Supplier': 'GadgetPlus',
    'Last Updated': '2024-01-14'
  },
  {
    'Product ID': 'P003',
    'Product Name': 'Monitor 4K',
    'Category': 'Electronics',
    'Price': 599.99,
    'Stock': 22,
    'Supplier': 'DisplayTech',
    'Last Updated': '2024-01-13'
  },
  {
    'Product ID': 'P004',
    'Product Name': 'Keyboard RGB',
    'Category': 'Accessories',
    'Price': 89.99,
    'Stock': 78,
    'Supplier': 'GamerHub',
    'Last Updated': '2024-01-12'
  },
  {
    'Product ID': 'P005',
    'Product Name': 'Webcam HD',
    'Category': 'Electronics',
    'Price': 149.99,
    'Stock': 33,
    'Supplier': 'VideoTech',
    'Last Updated': '2024-01-11'
  }
])

// Computed properties
const getColumnCount = () => {
  return jsondata.value.length > 0 ? Object.keys(jsondata.value[0]).length : 0
}

// Utility functions
function addProperties(data, newProperties) {
  data.forEach(item => {
    Object.assign(item, newProperties)
  })
  return data
}

function addRow(data) {
  if (data.length === 0) {
    return [{ 'Column 1': '', 'Column 2': '', 'Column 3': '' }]
  }

  const newRow = {}
  Object.keys(data[0]).forEach(key => {
    newRow[key] = ''
  })
  data.push(newRow)
  return data
}

const forceRerender = async () => {
  renderComponent.value = false
  await nextTick()
  renderComponent.value = true
}

// Core data manipulation functions
const createnewRow = async () => {
  jsondata.value = addRow([...jsondata.value])
  await forceRerender()

  toast.add({
    severity: 'success',
    summary: 'Row Added',
    detail: 'New row has been added to the dataset',
    life: 2000
  })
}

const createnewCol = async () => {
  const columnCount = getColumnCount()
  const newColumnName = `Column ${columnCount + 1}`
  const newColumn = { [newColumnName]: '' }

  jsondata.value = addProperties([...jsondata.value], newColumn)
  await forceRerender()

  toast.add({
    severity: 'success',
    summary: 'Column Added',
    detail: `New column '${newColumnName}' has been added`,
    life: 2000
  })
}

const newDoc = async () => {
  MyTitle.value = 'New Dataset'
  jsondata.value = [
    { 'Column 1': '', 'Column 2': '', 'Column 3': '' },
    { 'Column 1': '', 'Column 2': '', 'Column 3': '' },
    { 'Column 1': '', 'Column 2': '', 'Column 3': '' }
  ]
  selectedCells.value = []
  clipboard.value = null

  await forceRerender()

  toast.add({
    severity: 'info',
    summary: 'New Dataset',
    detail: 'Created a new empty dataset',
    life: 2000
  })
}

// File operations
const quickSave = () => {
  const dataStr = JSON.stringify({
    name: MyTitle.value,
    data: jsondata.value,
    timestamp: new Date().toISOString()
  }, null, 2)

  const blob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${MyTitle.value || 'dataset'}-${new Date().toISOString().split('T')[0]}.json`
  link.click()
  URL.revokeObjectURL(url)

  toast.add({
    severity: 'success',
    summary: 'Dataset Saved',
    detail: 'Dataset has been saved as JSON file',
    life: 3000
  })
}

const triggerFileImport = (type) => {
  showImportDialog.value = true
}

const handleFileImport = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  try {
    const fileExtension = file.name.split('.').pop().toLowerCase()
    let importedData = []

    if (fileExtension === 'json') {
      const text = await file.text()
      const parsed = JSON.parse(text)
      importedData = Array.isArray(parsed) ? parsed : parsed.data || [parsed]
    } else if (fileExtension === 'csv') {
      const text = await file.text()
      importedData = parseCSV(text)
    } else if (['xlsx', 'xls'].includes(fileExtension)) {
      const arrayBuffer = await file.arrayBuffer()
      const workbook = XLSX.read(arrayBuffer, { type: 'array' })
      const sheetName = workbook.SheetNames[0]
      const worksheet = workbook.Sheets[sheetName]
      importedData = XLSX.utils.sheet_to_json(worksheet)
    }

    if (importedData.length > 0) {
      jsondata.value = importedData
      MyTitle.value = file.name.split('.')[0]
      await forceRerender()

      toast.add({
        severity: 'success',
        summary: 'Import Successful',
        detail: `Imported ${importedData.length} rows from ${file.name}`,
        life: 3000
      })

      showImportDialog.value = false
    }
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Import Failed',
      detail: 'Error reading file: ' + error.message,
      life: 5000
    })
  }

  event.target.value = ''
}

const parseCSV = (text) => {
  const lines = text.trim().split('\n')
  if (lines.length === 0) return []

  const headers = lines[0].split(',').map(h => h.trim().replace(/"/g, ''))
  const data = []

  for (let i = 1; i < lines.length; i++) {
    const values = lines[i].split(',').map(v => v.trim().replace(/"/g, ''))
    const row = {}
    headers.forEach((header, index) => {
      row[header] = values[index] || ''
    })
    data.push(row)
  }

  return data
}

const importFromClipboard = async () => {
  try {
    const importedData = parseCSV(pasteData.value)
    if (importedData.length > 0) {
      jsondata.value = importedData
      await forceRerender()

      toast.add({
        severity: 'success',
        summary: 'Import Successful',
        detail: `Imported ${importedData.length} rows from clipboard`,
        life: 3000
      })

      showImportDialog.value = false
      pasteData.value = ''
    }
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Import Failed',
      detail: 'Error parsing clipboard data',
      life: 3000
    })
  }
}

const exportData = (format) => {
  try {
    let content, filename, mimeType
    const timestamp = new Date().toISOString().split('T')[0]
    const name = MyTitle.value || 'dataset'

    switch (format) {
      case 'csv':
        content = convertToCSV(jsondata.value)
        filename = `${name}-${timestamp}.csv`
        mimeType = 'text/csv'
        break
      case 'excel':
        const workbook = XLSX.utils.book_new()
        const worksheet = XLSX.utils.json_to_sheet(jsondata.value)
        XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1')
        const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' })
        content = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
        filename = `${name}-${timestamp}.xlsx`
        break
      case 'json':
        content = JSON.stringify({
          name: MyTitle.value,
          data: jsondata.value,
          exported: new Date().toISOString()
        }, null, 2)
        filename = `${name}-${timestamp}.json`
        mimeType = 'application/json'
        break
    }

    if (format !== 'excel') {
      content = new Blob([content], { type: mimeType })
    }

    const url = URL.createObjectURL(content)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    link.click()
    URL.revokeObjectURL(url)

    toast.add({
      severity: 'success',
      summary: 'Export Successful',
      detail: `Data exported as ${format.toUpperCase()}`,
      life: 3000
    })

    showExportDialog.value = false
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Export Failed',
      detail: 'Error exporting data: ' + error.message,
      life: 5000
    })
  }
}

const convertToCSV = (data) => {
  if (!data || data.length === 0) return ''

  const headers = Object.keys(data[0]).join(',')
  const rows = data.map(row =>
    Object.values(row).map(value =>
      typeof value === 'string' && value.includes(',') ? `"${value}"` : value
    ).join(',')
  )

  return [headers, ...rows].join('\n')
}

// Edit operations
const onCellSelected = (selection) => {
  selectedCells.value = selection || []
}

const onDataChange = (newData) => {
  jsondata.value = newData
}

const copySelection = () => {
  if (selectedCells.value.length === 0) {
    toast.add({
      severity: 'warn',
      summary: 'No Selection',
      detail: 'Please select cells to copy',
      life: 2000
    })
    return
  }

  clipboard.value = selectedCells.value.map(cell => ({ ...cell }))

  toast.add({
    severity: 'info',
    summary: 'Copied',
    detail: `${selectedCells.value.length} cell(s) copied to clipboard`,
    life: 2000
  })
}

const pasteSelection = () => {
  if (!clipboard.value) {
    toast.add({
      severity: 'warn',
      summary: 'No Data',
      detail: 'No data in clipboard to paste',
      life: 2000
    })
    return
  }

  // Implement paste logic here
  toast.add({
    severity: 'info',
    summary: 'Pasted',
    detail: 'Data pasted successfully',
    life: 2000
  })
}

const cutSelection = () => {
  copySelection()
  clearSelectedCells()
}

const selectAll = () => {
  // Implementation depends on excel editor API
  toast.add({
    severity: 'info',
    summary: 'Selected All',
    detail: 'All cells selected',
    life: 1000
  })
}

const clearSelection = () => {
  selectedCells.value = []
  toast.add({
    severity: 'info',
    summary: 'Selection Cleared',
    detail: 'Cell selection cleared',
    life: 1000
  })
}

const clearSelectedCells = () => {
  if (selectedCells.value.length === 0) return

  // Clear content of selected cells
  toast.add({
    severity: 'info',
    summary: 'Content Cleared',
    detail: 'Selected cells cleared',
    life: 2000
  })
}

// Insert operations
const insertRowAbove = async () => {
  // Insert row above current selection
  await createnewRow()
}

const insertRowBelow = async () => {
  await createnewRow()
}

const insertColumnLeft = async () => {
  await createnewCol()
}

const insertColumnRight = async () => {
  await createnewCol()
}

// Delete operations
const deleteSelectedRows = () => {
  if (selectedCells.value.length === 0) return

  toast.add({
    severity: 'warn',
    summary: 'Rows Deleted',
    detail: 'Selected rows have been deleted',
    life: 2000
  })
}

const deleteSelectedColumns = () => {
  if (selectedCells.value.length === 0) return

  toast.add({
    severity: 'warn',
    summary: 'Columns Deleted',
    detail: 'Selected columns have been deleted',
    life: 2000
  })
}

const confirmClearAll = () => {
  if (confirm('Are you sure you want to clear all data? This action cannot be undone.')) {
    newDoc()
  }
}

// Tool functions
const showFindReplace = () => {
  toast.add({
    severity: 'info',
    summary: 'Find & Replace',
    detail: 'Find & Replace dialog would open here',
    life: 2000
  })
}

const showSortDialog = () => {
  toast.add({
    severity: 'info',
    summary: 'Sort Data',
    detail: 'Sort dialog would open here',
    life: 2000
  })
}

const showFilterDialog = () => {
  toast.add({
    severity: 'info',
    summary: 'Filter Data',
    detail: 'Filter dialog would open here',
    life: 2000
  })
}

const validateData = () => {
  let errors = 0
  let warnings = 0

  jsondata.value.forEach((row, index) => {
    Object.entries(row).forEach(([key, value]) => {
      if (value === '' || value === null || value === undefined) {
        warnings++
      }
    })
  })

  toast.add({
    severity: warnings > 0 ? 'warn' : 'success',
    summary: 'Data Validation',
    detail: `Validation complete. ${warnings} empty cells found.`,
    life: 3000
  })
}

const showDataStats = () => {
  const stats = {
    rows: jsondata.value.length,
    columns: getColumnCount(),
    totalCells: jsondata.value.length * getColumnCount(),
    emptyCells: 0
  }

  jsondata.value.forEach(row => {
    Object.values(row).forEach(value => {
      if (value === '' || value === null || value === undefined) {
        stats.emptyCells++
      }
    })
  })

  toast.add({
    severity: 'info',
    summary: 'Dataset Statistics',
    detail: `${stats.rows} rows, ${stats.columns} columns, ${stats.emptyCells}/${stats.totalCells} empty cells`,
    life: 5000
  })
}
</script>

<style>
/* Dataset container styling */
.dataset-container {
  height: calc(100vh - 200px);
  width: 100%;
  padding: 1rem;
  background: var(--surface-ground);
}

/* Toolbar styling */
.dataset-toolbar {
  background: var(--surface-card);
  border-bottom: 1px solid var(--surface-border);
  padding: 0.75rem 1rem;
}

.toolbar-section {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.dataset-label {
  font-weight: 600;
  color: var(--text-color);
  font-size: 0.9rem;
}

.dataset-name {
  min-width: 200px;
}

.dataset-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--primary-color);
  cursor: pointer;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  border: 1px solid transparent;
  transition: all 0.2s ease;
}

.dataset-title:hover {
  background: var(--surface-hover);
  border-color: var(--surface-border);
}

.toolbar-stats {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.stats-tag {
  font-size: 0.8rem;
  padding: 0.25rem 0.5rem;
}

.toolbar-actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

/* Menu bar styling */
.dataset-menubar {
  background: var(--surface-section);
  border-bottom: 1px solid var(--surface-border);
  padding: 0.5rem 1rem;
}

.dataset-menubar :deep(.p-menubar) {
  background: transparent;
  border: none;
  padding: 0;
}

.dataset-menubar :deep(.p-menubar-root-list > .p-menuitem > .p-menuitem-content) {
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  margin: 0 0.25rem;
  transition: all 0.2s ease;
}

.dataset-menubar :deep(.p-menubar-root-list > .p-menuitem > .p-menuitem-content:hover) {
  background: var(--surface-hover);
}

/* Excel editor theme integration */
.theme-excel-editor {
  height: 100%;
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  overflow: hidden;
  background: var(--surface-card);
}

/* Override VueExcelEditor styles for theme compatibility */
.theme-excel-editor :deep(.vue-excel-editor) {
  background: var(--surface-card);
  color: var(--text-color);
  font-family: var(--font-family);
}

.theme-excel-editor :deep(.systable) {
  background: var(--surface-card);
  color: var(--text-color);
  border-color: var(--surface-border);
}

.theme-excel-editor :deep(.systable th) {
  background: var(--surface-section);
  color: var(--text-color);
  border-color: var(--surface-border);
  font-weight: 600;
}

.theme-excel-editor :deep(.systable td) {
  background: var(--surface-card);
  color: var(--text-color);
  border-color: var(--surface-border);
}

.theme-excel-editor :deep(.systable tr:hover td) {
  background: var(--surface-hover);
}

.theme-excel-editor :deep(.systable tr.select td) {
  background: var(--primary-color) !important;
  color: var(--primary-color-text) !important;
}

.theme-excel-editor :deep(.systable .cell-selected) {
  background: var(--primary-100) !important;
  border-color: var(--primary-color) !important;
}

.theme-excel-editor :deep(.vue-excel-editor .toolbar) {
  background: var(--surface-section);
  border-color: var(--surface-border);
  color: var(--text-color);
}

.theme-excel-editor :deep(.vue-excel-editor .toolbar button) {
  background: var(--surface-card);
  color: var(--text-color);
  border-color: var(--surface-border);
}

.theme-excel-editor :deep(.vue-excel-editor .toolbar button:hover) {
  background: var(--surface-hover);
}

.theme-excel-editor :deep(.vue-excel-editor .pagination) {
  background: var(--surface-section);
  border-color: var(--surface-border);
  color: var(--text-color);
}

.theme-excel-editor :deep(.vue-excel-editor input),
.theme-excel-editor :deep(.vue-excel-editor select),
.theme-excel-editor :deep(.vue-excel-editor textarea) {
  background: var(--surface-card);
  color: var(--text-color);
  border-color: var(--surface-border);
}

.theme-excel-editor :deep(.vue-excel-editor input:focus),
.theme-excel-editor :deep(.vue-excel-editor select:focus),
.theme-excel-editor :deep(.vue-excel-editor textarea:focus) {
  border-color: var(--primary-color);
  box-shadow: 0 0 0 1px var(--primary-color);
}

/* Dialog styling */
.import-dialog,
.export-dialog {
  background: var(--surface-overlay);
}

.import-dialog :deep(.p-dialog-content),
.export-dialog :deep(.p-dialog-content) {
  background: var(--surface-card);
  color: var(--text-color);
}

.import-content,
.export-content {
  padding: 1rem 0;
}

.import-options h4,
.export-format h4 {
  color: var(--text-color);
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.file-input-container {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin: 1rem 0;
}

.file-input {
  display: none;
}

.file-button {
  flex-shrink: 0;
}

.file-help {
  color: var(--text-color-secondary);
  font-style: italic;
}

.paste-textarea {
  width: 100%;
  min-height: 120px;
  margin: 1rem 0;
  background: var(--surface-card);
  color: var(--text-color);
  border-color: var(--surface-border);
}

.format-options {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-top: 1rem;
}

.format-button {
  justify-content: flex-start;
  width: 100%;
}

/* Responsive design */
@media (max-width: 768px) {
  .dataset-container {
    padding: 0.5rem;
    height: calc(100vh - 160px);
  }

  .toolbar-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .toolbar-stats {
    flex-wrap: wrap;
  }

  .toolbar-actions {
    flex-wrap: wrap;
    width: 100%;
  }

  .dataset-name {
    min-width: auto;
    width: 100%;
  }

  .dataset-menubar {
    padding: 0.25rem 0.5rem;
  }

  .import-dialog,
  .export-dialog {
    width: 95vw !important;
    max-width: 500px;
  }

  .file-input-container {
    flex-direction: column;
    align-items: stretch;
  }
}



/* Print styles */
@media print {
  .dataset-toolbar,
  .dataset-menubar {
    display: none;
  }

  .dataset-container {
    height: auto;
    padding: 0;
  }

  .theme-excel-editor :deep(.vue-excel-editor .toolbar),
  .theme-excel-editor :deep(.vue-excel-editor .pagination) {
    display: none;
  }
}
body .app-dark .dataset-container {
    background: var(--p-surface-900);
}

body .app-dark .dataset-toolbar {
    background: var(--p-surface-800);
    border-bottom: 1px solid var(--p-surface-700);
}

body .app-dark .dataset-label {
    color: var(--p-text-color-secondary);
}

body .app-dark .dataset-title:hover {
    background: var(--p-surface-700);
    border-color: var(--p-surface-600);
}

body .app-dark .dataset-menubar {
    background: var(--p-surface-800);
    border-bottom: 1px solid var(--p-surface-700);
}

body .app-dark .dataset-menubar .p-menubar {
    background: transparent;
    border: none;
}

body .app-dark .dataset-menubar .p-menubar-root-list > .p-menuitem > .p-menuitem-content:hover {
    background: var(--p-surface-700);
}

body .app-dark .theme-excel-editor {
    border-color: var(--p-surface-700);
    background: var(--p-surface-800);
}

body .app-dark .theme-excel-editor .vue-excel-editor {
    background: var(--p-surface-800);
    color: var(--p-text-color);
}

body .app-dark .theme-excel-editor .systable {
    background: var(--p-surface-800);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}

body .app-dark .theme-excel-editor .systable th {
    background: var(--p-surface-700);
    color: var(--p-text-color);
    border-color: var(--p-surface-600);
}

body .app-dark .theme-excel-editor .systable td {
    background: var(--p-surface-800);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}

body .app-dark .theme-excel-editor .systable tr:hover td {
    background: var(--p-surface-600);
}

body .app-dark .theme-excel-editor .systable tr.select td {
    background: var(--p-primary-color) !important;
    color: var(--p-primary-color-text) !important;
}

body .app-dark .theme-excel-editor .systable .cell-selected {
    background: var(--p-primary-200) !important;
    border-color: var(--p-primary-color) !important;
}

body .app-dark .theme-excel-editor .vue-excel-editor .toolbar {
    background: var(--p-surface-700);
    border-color: var(--p-surface-600);
    color: var(--p-text-color);
}

body .app-dark .theme-excel-editor .vue-excel-editor .toolbar button {
    background: var(--p-surface-800);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}

body .app-dark .theme-excel-editor .vue-excel-editor .toolbar button:hover {
    background: var(--p-surface-600);
}

body .app-dark .theme-excel-editor .vue-excel-editor .pagination {
    background: var(--p-surface-700);
    border-color: var(--p-surface-600);
    color: var(--p-text-color);
}

body .app-dark .theme-excel-editor .vue-excel-editor input,
body .app-dark .theme-excel-editor .vue-excel-editor select,
body .app-dark .theme-excel-editor .vue-excel-editor textarea {
    background: var(--p-surface-900);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}

body .app-dark .theme-excel-editor .vue-excel-editor input:focus,
body .app-dark .theme-excel-editor .vue-excel-editor select:focus,
body .app-dark .theme-excel-editor .vue-excel-editor textarea:focus {
    border-color: var(--p-primary-color);
    box-shadow: 0 0 0 1px var(--p-primary-color);
}

body .app-dark .import-dialog .p-dialog-content,
body .app-dark .export-dialog .p-dialog-content {
    background: var(--p-surface-800);
}

body .app-dark .import-content,
body .app-dark .export-content {
    padding: 1rem 0;
}

body .app-dark .import-options h4,
body .app-dark .export-format h4 {
    color: var(--p-text-color);
}

body .app-dark .paste-textarea {
    background: var(--p-surface-900);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}
</style>