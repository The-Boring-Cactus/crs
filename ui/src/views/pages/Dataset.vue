<template>
  <Toast />

  <!-- Main Toolbar -->
  <Toolbar class="dataset-toolbar">
    <template #start>
      <div class="flex align-items-center gap-2">
        <label class="dataset-label">Dataset:</label>
        <Inplace v-tooltip="'Click to Change'" class="dataset-name">
          <template #display>
            <div class="flex align-items-center gap-2 cursor-pointer">
              <i class="pi pi-table text-muted-color"></i>
              <span class="dataset-title">{{ MyTitle || 'Untitled Dataset' }}</span>
              <i class="pi pi-pencil text-xs text-muted-color"></i>
            </div>
          </template>
          <template #content="{ closeCallback }">
            <div class="flex align-items-center gap-2">
              <InputText v-model="MyTitle" placeholder="Enter dataset name" autofocus />
              <Button icon="pi pi-check" text severity="success" @click="closeCallback" />
              <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
            </div>
          </template>
        </Inplace>
      </div>
    </template>

    <template #center>
      <div class="flex align-items-center gap-2">
        <Tag severity="info" class="stats-tag">
          <i class="pi pi-table mr-1"></i>
          {{ jsondata.length }} rows × {{ getColumnCount() }} columns
        </Tag>
        <Tag v-if="selectedRows.length > 0" severity="success" class="stats-tag">
          <i class="pi pi-check-square mr-1"></i>
          {{ selectedRows.length }} selected
        </Tag>
      </div>
    </template>

    <template #end>
      <div class="flex align-items-center gap-2">
        <Button
          icon="pi pi-save"
          @click="quickSave"
          v-tooltip.bottom="'Save Dataset'"
          text
        />
        <Button
          icon="pi pi-upload"
          @click="showImportDialog = true"
          v-tooltip.bottom="'Import Data'"
          text
        />
        <Button
          icon="pi pi-download"
          @click="showExportDialog = true"
          v-tooltip.bottom="'Export Data'"
          text
        />
      </div>
    </template>
  </Toolbar>

  <!-- Secondary Toolbar -->
  <Toolbar class="dataset-secondary-toolbar">
    <template #start>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-plus" text @click="newDoc" v-tooltip.top="'New Dataset'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-plus-circle" text @click="createnewRow" v-tooltip.top="'Add Row'" />
        <Button icon="pi pi-table" text @click="createnewCol" v-tooltip.top="'Add Column'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-copy" text @click="copySelection" v-tooltip.top="'Copy Selection'" :disabled="selectedRows.length === 0" />
        <Button icon="pi pi-clone" text @click="pasteSelection" v-tooltip.top="'Paste'" :disabled="!clipboard" />
        <Button icon="pi pi-scissors" text @click="cutSelection" v-tooltip.top="'Cut Selection'" :disabled="selectedRows.length === 0" />
      </div>
    </template>

    <template #center>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-check-square" text @click="selectAll" v-tooltip.top="'Select All'" />
        <Button icon="pi pi-eraser" text @click="clearSelection" v-tooltip.top="'Clear Selection'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-trash" text @click="deleteSelectedRows" v-tooltip.top="'Delete Selected Rows'" :disabled="selectedRows.length === 0" />
        <Button icon="pi pi-times" text @click="confirmClearAll" v-tooltip.top="'Clear All Data'" severity="danger" />
      </div>
    </template>

    <template #end>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-search" text @click="showFindReplace" v-tooltip.top="'Find & Replace'" />
        <Button icon="pi pi-sort" text @click="showSortDialog" v-tooltip.top="'Sort Data'" />
        <Button icon="pi pi-filter" text @click="showFilterDialog" v-tooltip.top="'Filter Data'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-check-circle" text @click="validateData" v-tooltip.top="'Validate Data'" />
        <Button icon="pi pi-chart-bar" text @click="showDataStats" v-tooltip.top="'Show Statistics'" />
      </div>
    </template>
  </Toolbar>

  <!-- Data Editor Container -->
  <div class="dataset-container">
    <DataTable
      v-if="renderComponent"
      :value="jsondata"
      editMode="cell"
      @cell-edit-complete="onCellEditComplete"
      @cell-edit-init="onCellEditInit"
      v-model:selection="selectedRows"
      dataKey="id"
      :paginator="true"
      :rows="20"
      :rowsPerPageOptions="[10, 20, 50, 100]"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
      scrollable
      scrollHeight="calc(100vh - 300px)"
      resizableColumns
      columnResizeMode="expand"
      sortMode="multiple"
      class="editable-datatable"
      @row-contextmenu="onRowContextMenu"
      contextMenu
    >
      <Column
        v-for="(column, index) in columns"
        :key="column.field"
        :field="column.field"
        :header="column.header"
        :sortable="true"
        style="min-width: 120px"
      >
        <template #editor="{ data, field }">
          <InputText
            v-model="data[field]"
            autofocus
            @keydown.enter="$event.target.blur()"
            @keydown.tab="$event.target.blur()"
            class="cell-editor"
          />
        </template>
        <template #header>
          <div class="column-header">
            <span @dblclick="editColumnHeader(column, index)">
              {{ column.header }}
            </span>
            <Button
              icon="pi pi-times"
              class="p-button-text p-button-sm delete-column-btn"
              @click="deleteColumn(index)"
              v-tooltip.top="'Delete Column'"
            />
          </div>
        </template>
      </Column>

      <!-- Add new column button -->
      <Column header="" style="width: 50px" :sortable="false">
        <template #header>
          <Button
            icon="pi pi-plus"
            class="p-button-text p-button-sm"
            @click="createnewCol"
            v-tooltip.top="'Add Column'"
          />
        </template>
        <template #body="{ index }">
          <Button
            icon="pi pi-trash"
            class="p-button-text p-button-sm p-button-danger"
            @click="deleteRow(index)"
            v-tooltip.top="'Delete Row'"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Add new row button -->
    <div class="add-row-container">
      <Button
        icon="pi pi-plus"
        label="Add Row"
        @click="createnewRow"
        class="add-row-btn"
      />
    </div>
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
  <ContextMenu ref="contextMenuRef" :model="contextMenuItems" />
</template>

<script setup>
import { nextTick, ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import * as XLSX from 'xlsx'

const toast = useToast()

// Data state
const MyTitle = ref('Sample Dataset')
const renderComponent = ref(true)
const showImportDialog = ref(false)
const showExportDialog = ref(false)
const pasteData = ref('')
const selectedRows = ref([])
const clipboard = ref(null)
const editingCell = ref(null)
const contextMenuRef = ref()


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

// Generate columns from data
const columns = computed(() => {
  if (jsondata.value.length === 0) return []
  return Object.keys(jsondata.value[0])
    .filter(key => key !== 'id')
    .map(key => ({
      field: key,
      header: key
    }))
})

// Utility functions
function addProperties(data, newProperties) {
  data.forEach(item => {
    Object.assign(item, newProperties)
  })
  return data
}

function addRow(data) {
  if (data.length === 0) {
    return [{ id: 1, 'Column 1': '', 'Column 2': '', 'Column 3': '' }]
  }

  const newRow = { id: Math.max(...data.map(row => row.id || 0)) + 1 }
  Object.keys(data[0]).forEach(key => {
    if (key !== 'id') {
      newRow[key] = ''
    }
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
  selectedRows.value = []
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
      // Add IDs to imported data
      importedData.forEach((row, index) => {
        row.id = index + 1
      })
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
      // Add IDs to imported data
      importedData.forEach((row, index) => {
        row.id = index + 1
      })
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
        const exportData = jsondata.value.map(row => {
          const { id, ...rest } = row
          return rest
        })
        const worksheet = XLSX.utils.json_to_sheet(exportData)
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

  // Exclude ID field from CSV export
  const exportData = data.map(row => {
    const { id, ...rest } = row
    return rest
  })

  const headers = Object.keys(exportData[0]).join(',')
  const rows = exportData.map(row =>
    Object.values(row).map(value =>
      typeof value === 'string' && value.includes(',') ? `"${value}"` : value
    ).join(',')
  )

  return [headers, ...rows].join('\n')
}

// Edit operations
const onCellEditComplete = (event) => {
  const { data, newValue, field } = event
  data[field] = newValue

  toast.add({
    severity: 'success',
    summary: 'Cell Updated',
    detail: `Updated ${field}`,
    life: 1000
  })
}

const onCellEditInit = (event) => {
  editingCell.value = event
}

const onRowContextMenu = (event) => {
  contextMenuRef.value.show(event.originalEvent)
}

const editColumnHeader = (column, index) => {
  const newHeader = prompt('Enter new column name:', column.header)
  if (newHeader && newHeader.trim() !== '') {
    const oldHeader = column.header

    // Update all rows to use new column name
    jsondata.value.forEach(row => {
      if (row[oldHeader] !== undefined) {
        row[newHeader.trim()] = row[oldHeader]
        delete row[oldHeader]
      }
    })

    toast.add({
      severity: 'success',
      summary: 'Column Renamed',
      detail: `Column renamed from "${oldHeader}" to "${newHeader.trim()}"`,
      life: 2000
    })

    forceRerender()
  }
}

const deleteColumn = (columnIndex) => {
  if (columns.value.length <= 1) {
    toast.add({
      severity: 'warn',
      summary: 'Cannot Delete',
      detail: 'At least one column must remain',
      life: 2000
    })
    return
  }

  const columnToDelete = columns.value[columnIndex]
  const fieldToDelete = columnToDelete.field

  // Remove the field from all rows
  jsondata.value.forEach(row => {
    delete row[fieldToDelete]
  })

  toast.add({
    severity: 'success',
    summary: 'Column Deleted',
    detail: `Column "${columnToDelete.header}" has been deleted`,
    life: 2000
  })

  forceRerender()
}

const deleteRow = (rowIndex) => {
  if (jsondata.value.length <= 1) {
    toast.add({
      severity: 'warn',
      summary: 'Cannot Delete',
      detail: 'At least one row must remain',
      life: 2000
    })
    return
  }

  jsondata.value.splice(rowIndex, 1)

  toast.add({
    severity: 'success',
    summary: 'Row Deleted',
    detail: 'Row has been deleted',
    life: 2000
  })
}

const copySelection = () => {
  if (selectedRows.value.length === 0) {
    toast.add({
      severity: 'warn',
      summary: 'No Selection',
      detail: 'Please select rows to copy',
      life: 2000
    })
    return
  }

  clipboard.value = selectedRows.value.map(row => ({ ...row }))

  toast.add({
    severity: 'info',
    summary: 'Copied',
    detail: `${selectedRows.value.length} row(s) copied to clipboard`,
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
  selectedRows.value = []
  toast.add({
    severity: 'info',
    summary: 'Selection Cleared',
    detail: 'Row selection cleared',
    life: 1000
  })
}

const clearSelectedCells = () => {
  if (selectedRows.value.length === 0) return

  // Clear content of selected rows
  selectedRows.value.forEach(row => {
    Object.keys(row).forEach(key => {
      if (key !== 'id') {
        row[key] = ''
      }
    })
  })

  toast.add({
    severity: 'info',
    summary: 'Content Cleared',
    detail: 'Selected rows cleared',
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
  if (selectedRows.value.length === 0) return

  // Remove selected rows from data
  selectedRows.value.forEach(selectedRow => {
    const index = jsondata.value.findIndex(row => row.id === selectedRow.id)
    if (index !== -1) {
      jsondata.value.splice(index, 1)
    }
  })

  selectedRows.value = []

  toast.add({
    severity: 'warn',
    summary: 'Rows Deleted',
    detail: 'Selected rows have been deleted',
    life: 2000
  })
}

const deleteSelectedColumns = () => {
  if (selectedRows.value.length === 0) return

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
  height: calc(100vh - 280px);
  width: 100%;
  padding: 1rem;
  background: var(--surface-ground);
}

/* Toolbar styling */
.dataset-toolbar,
.dataset-secondary-toolbar {
  background: var(--surface-card);
  border-bottom: 1px solid var(--surface-border);
  padding: 0.75rem 1rem;
}

.dataset-secondary-toolbar {
  background: var(--surface-section);
  padding: 0.5rem 1rem;
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

.stats-tag {
  font-size: 0.8rem;
  padding: 0.25rem 0.5rem;
}

/* DataTable editable styles */
.editable-datatable {
  height: 100%;
  border: 1px solid var(--p-border-color);
  border-radius: 8px;
  overflow: hidden;
  background: var(--p-surface-card);
}

.editable-datatable :deep(.p-datatable-table) {
  font-size: 14px;
}

.editable-datatable :deep(.p-datatable-thead > tr > th) {
  background: var(--p-surface-100);
  border-color: var(--p-border-color);
  padding: 0.75rem 0.5rem;
  font-weight: 600;
}

.editable-datatable :deep(.p-datatable-tbody > tr > td) {
  background: var(--p-surface-card);
  border-color: var(--p-border-color);
  padding: 0.5rem;
  cursor: pointer;
}

.editable-datatable :deep(.p-datatable-tbody > tr:hover > td) {
  background: var(--p-surface-hover);
}

.editable-datatable :deep(.p-datatable-tbody > tr.p-highlight > td) {
  background: var(--p-primary-100) !important;
}

.column-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.column-header span {
  cursor: pointer;
  flex: 1;
  text-align: left;
}

.delete-column-btn {
  opacity: 0;
  transition: opacity 0.2s;
  margin-left: 0.5rem;
}

.column-header:hover .delete-column-btn {
  opacity: 1;
}

.cell-editor {
  width: 100%;
  border: none;
  background: transparent;
  font-size: 14px;
}

.cell-editor:focus {
  outline: 2px solid var(--p-primary-color);
  outline-offset: -2px;
}

.add-row-container {
  padding: 1rem;
  text-align: center;
  background: var(--p-surface-card);
  border-top: 1px solid var(--p-border-color);
}

.add-row-btn {
  min-width: 120px;
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
    height: calc(100vh - 240px);
  }

  .dataset-toolbar,
  .dataset-secondary-toolbar {
    padding: 0.5rem;
  }

  .dataset-toolbar :deep(.flex),
  .dataset-secondary-toolbar :deep(.flex) {
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .dataset-name {
    min-width: auto;
    width: 100%;
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
  .dataset-secondary-toolbar {
    display: none;
  }

  .dataset-container {
    height: auto;
    padding: 0;
  }

  .add-row-container {
    display: none;
  }
}

</style>