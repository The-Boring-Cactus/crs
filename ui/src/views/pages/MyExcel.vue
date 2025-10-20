<template>
  <Toast />

  <!-- Main Toolbar -->
  <Toolbar class="excel-toolbar">
    <template #start>
      <div class="flex align-items-center gap-2">
        <label class="excel-label">Spreadsheet:</label>
        <Inplace v-tooltip="'Click to Change'" class="excel-name">
          <template #display>
            <div class="flex align-items-center gap-2 cursor-pointer">
              <i class="pi pi-file-excel text-muted-color"></i>
              <span class="excel-title">{{ sheetTitle || 'Untitled Spreadsheet' }}</span>
              <i class="pi pi-pencil text-xs text-muted-color"></i>
            </div>
          </template>
          <template #content="{ closeCallback }">
            <div class="flex align-items-center gap-2">
              <InputText v-model="sheetTitle" placeholder="Enter spreadsheet name" autofocus />
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
          {{ sheetData.length }} rows × {{ columnHeaders.length }} columns
        </Tag>
        <Tag v-if="selectedCells.size > 0" severity="success" class="stats-tag">
          <i class="pi pi-check-square mr-1"></i>
          {{ selectedCells.size }} cells selected
        </Tag>
      </div>
    </template>

    <template #end>
      <div class="flex align-items-center gap-2">
        <Button
          icon="pi pi-save"
          @click="quickSave"
          v-tooltip.bottom="'Save Spreadsheet'"
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
  <Toolbar class="excel-secondary-toolbar">
    <template #start>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-file" text @click="newSheet" v-tooltip.top="'New Spreadsheet'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-plus-circle" text @click="addRow" v-tooltip.top="'Add Row'" />
        <Button icon="pi pi-table" text @click="addColumn" v-tooltip.top="'Add Column'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-copy" text @click="copySelection" v-tooltip.top="'Copy (Ctrl+C)'" :disabled="selectedCells.size === 0" />
        <Button icon="pi pi-clone" text @click="pasteSelection" v-tooltip.top="'Paste (Ctrl+V)'" :disabled="!clipboard" />
        <Button icon="pi pi-scissors" text @click="cutSelection" v-tooltip.top="'Cut (Ctrl+X)'" :disabled="selectedCells.size === 0" />
      </div>
    </template>

    <template #center>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-check-square" text @click="selectAll" v-tooltip.top="'Select All (Ctrl+A)'" />
        <Button icon="pi pi-eraser" text @click="clearSelection" v-tooltip.top="'Clear Selection'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-trash" text @click="deleteSelectedCells" v-tooltip.top="'Delete Selected'" :disabled="selectedCells.size === 0" />
        <Button icon="pi pi-times" text @click="confirmClearAll" v-tooltip.top="'Clear All Data'" severity="danger" />
      </div>
    </template>

    <template #end>
      <div class="flex align-items-center gap-1">
        <Button icon="pi pi-search" text @click="showFindReplace = true" v-tooltip.top="'Find & Replace'" />
        <Button icon="pi pi-sort-amount-down" text @click="showSortDialog = true" v-tooltip.top="'Sort Data'" />
        <Button icon="pi pi-filter" text @click="showFilterDialog = true" v-tooltip.top="'Filter Data'" />
        <Divider layout="vertical" />
        <Button icon="pi pi-chart-bar" text @click="showStatistics" v-tooltip.top="'Show Statistics'" />
      </div>
    </template>
  </Toolbar>

  <!-- Excel Grid Container -->
  <div class="excel-container" @keydown="handleKeyboardShortcuts">
    <!-- Cell Reference Bar -->
    <div class="excel-reference-bar">
      <div class="reference-bar-cell-ref">
        <i class="pi pi-th-large mr-2"></i>
        <span class="cell-ref-text">{{ currentCellReference || 'Select a cell' }}</span>
      </div>
    </div>

    <div class="excel-grid-wrapper">
      <!-- Column Headers (A, B, C, ...) -->
      <div class="excel-grid-header">
        <div class="excel-corner-cell"></div>
        <div
          v-for="(col, index) in columnHeaders"
          :key="`header-${index}`"
          class="excel-column-header"
          @contextmenu.prevent="onColumnHeaderContextMenu($event, index)"
        >
          {{ col }}
        </div>
      </div>

      <!-- Grid Rows -->
      <div class="excel-grid-body" ref="gridBody">
        <div
          v-for="(row, rowIndex) in sheetData"
          :key="`row-${rowIndex}`"
          class="excel-row"
        >
          <!-- Row Number -->
          <div
            class="excel-row-header"
            @click="selectRow(rowIndex)"
            @contextmenu.prevent="onRowHeaderContextMenu($event, rowIndex)"
          >
            {{ rowIndex + 1 }}
          </div>

          <!-- Cells -->
          <div
            v-for="(col, colIndex) in columnHeaders"
            :key="`cell-${rowIndex}-${colIndex}`"
            class="excel-cell"
            :class="{
              'excel-cell-selected': isCellSelected(rowIndex, colIndex),
              'excel-cell-editing': editingCell && editingCell.row === rowIndex && editingCell.col === colIndex
            }"
            @click="selectCell(rowIndex, colIndex, $event)"
            @dblclick="startEditing(rowIndex, colIndex)"
            @keydown="onCellKeyDown($event, rowIndex, colIndex)"
            @contextmenu.prevent="onCellContextMenu($event, rowIndex, colIndex)"
            tabindex="0"
          >
            <input
              v-if="editingCell && editingCell.row === rowIndex && editingCell.col === colIndex"
              ref="cellInput"
              v-model="editingValue"
              @blur="finishEditing"
              @keydown.enter="finishEditingAndMoveDown"
              @keydown.tab.prevent="finishEditingAndMoveRight"
              @keydown.esc="cancelEditing"
              class="excel-cell-input"
              autofocus
            />
            <span v-else class="excel-cell-value">
              {{ sheetData[rowIndex][columnHeaders[colIndex]] || '' }}
            </span>
          </div>
        </div>
      </div>
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
            label="Excel (.xlsx)"
            @click="exportData('excel')"
            class="format-button"
          />
          <Button
            icon="pi pi-code"
            label="JSON"
            @click="exportData('json')"
            class="format-button"
          />
          <Button
            icon="pi pi-table"
            label="HTML Table"
            @click="exportData('html')"
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

  <!-- Find & Replace Dialog -->
  <Dialog
    v-model:visible="showFindReplace"
    modal
    header="Find & Replace"
    :style="{ width: '450px' }"
  >
    <div class="find-replace-content">
      <div class="flex flex-column gap-3">
        <div>
          <label for="findText" class="block mb-2">Find:</label>
          <InputText id="findText" v-model="findText" class="w-full" />
        </div>
        <div>
          <label for="replaceText" class="block mb-2">Replace with:</label>
          <InputText id="replaceText" v-model="replaceText" class="w-full" />
        </div>
        <div class="flex gap-2">
          <Button label="Find Next" @click="findNext" icon="pi pi-search" />
          <Button label="Replace" @click="replaceOne" icon="pi pi-pencil" />
          <Button label="Replace All" @click="replaceAll" icon="pi pi-sync" severity="warning" />
        </div>
      </div>
    </div>
  </Dialog>

  <!-- Sort Dialog -->
  <Dialog
    v-model:visible="showSortDialog"
    modal
    header="Sort Data"
    :style="{ width: '400px' }"
  >
    <div class="sort-content">
      <div class="flex flex-column gap-3">
        <div>
          <label for="sortColumn" class="block mb-2">Sort by Column:</label>
          <Dropdown
            id="sortColumn"
            v-model="sortColumn"
            :options="columnHeaders"
            placeholder="Select column"
            class="w-full"
          />
        </div>
        <div>
          <label class="block mb-2">Order:</label>
          <div class="flex gap-2">
            <Button
              label="Ascending"
              @click="sortData('asc')"
              icon="pi pi-sort-amount-up"
              :severity="sortOrder === 'asc' ? 'success' : 'secondary'"
            />
            <Button
              label="Descending"
              @click="sortData('desc')"
              icon="pi pi-sort-amount-down"
              :severity="sortOrder === 'desc' ? 'success' : 'secondary'"
            />
          </div>
        </div>
      </div>
    </div>
  </Dialog>

  <!-- Filter Dialog -->
  <Dialog
    v-model:visible="showFilterDialog"
    modal
    header="Filter Data"
    :style="{ width: '400px' }"
  >
    <div class="filter-content">
      <p class="mb-3">Filter functionality coming soon...</p>
      <small>This feature will allow you to filter rows based on column values.</small>
    </div>
  </Dialog>
</template>

<script setup>
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import * as XLSX from 'xlsx'

const toast = useToast()

// State
const sheetTitle = ref('MyExcel Spreadsheet')
const sheetData = ref([])
const columnHeaders = ref([])
const selectedCells = ref(new Set())
const editingCell = ref(null)
const editingValue = ref('')
const clipboard = ref(null)
const showImportDialog = ref(false)
const showExportDialog = ref(false)
const pasteData = ref('')
const contextMenuRef = ref()
const cellInput = ref(null)
const gridBody = ref(null)
const showFindReplace = ref(false)
const showSortDialog = ref(false)
const showFilterDialog = ref(false)
const findText = ref('')
const replaceText = ref('')
const sortColumn = ref(null)
const sortOrder = ref('asc')
const lastSelectedCell = ref(null)

// Initialize with default data
const initializeSheet = () => {
  const rows = 20
  const cols = 10

  // Generate column headers (A, B, C, ..., Z, AA, AB, ...)
  const headers = []
  for (let i = 0; i < cols; i++) {
    headers.push(getColumnLabel(i))
  }
  columnHeaders.value = headers

  // Initialize empty cells
  const data = []
  for (let i = 0; i < rows; i++) {
    const row = {}
    headers.forEach(col => {
      row[col] = ''
    })
    data.push(row)
  }
  sheetData.value = data
}

// Generate Excel-style column labels (A, B, C, ..., Z, AA, AB, ...)
const getColumnLabel = (index) => {
  let label = ''
  let num = index + 1
  while (num > 0) {
    let remainder = (num - 1) % 26
    label = String.fromCharCode(65 + remainder) + label
    num = Math.floor((num - 1) / 26)
  }
  return label
}

// Current cell reference (e.g., A1, B5)
const currentCellReference = computed(() => {
  if (!lastSelectedCell.value) return ''
  const { row, col } = lastSelectedCell.value
  return `${columnHeaders.value[col]}${row + 1}`
})

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
    label: 'Add Row',
    icon: 'pi pi-plus',
    command: () => addRow()
  },
  {
    label: 'Add Column',
    icon: 'pi pi-plus',
    command: () => addColumn()
  },
  { separator: true },
  {
    label: 'Delete Row',
    icon: 'pi pi-trash',
    command: () => deleteRow()
  },
  {
    label: 'Delete Column',
    icon: 'pi pi-trash',
    command: () => deleteColumn()
  },
  { separator: true },
  {
    label: 'Clear Contents',
    icon: 'pi pi-eraser',
    command: () => deleteSelectedCells()
  }
])

// Cell selection functions
const isCellSelected = (row, col) => {
  return selectedCells.value.has(`${row},${col}`)
}

const selectCell = (row, col, event) => {
  if (!event.ctrlKey && !event.shiftKey) {
    selectedCells.value.clear()
  }
  selectedCells.value.add(`${row},${col}`)
  lastSelectedCell.value = { row, col }

  // Focus the cell for keyboard input
  nextTick(() => {
    const cellElements = document.querySelectorAll('.excel-cell')
    const cellIndex = row * columnHeaders.value.length + col
    if (cellElements[cellIndex]) {
      cellElements[cellIndex].focus()
    }
  })
}

const selectRow = (rowIndex) => {
  selectedCells.value.clear()
  columnHeaders.value.forEach((_, colIndex) => {
    selectedCells.value.add(`${rowIndex},${colIndex}`)
  })
}

const selectAll = () => {
  selectedCells.value.clear()
  sheetData.value.forEach((_, rowIndex) => {
    columnHeaders.value.forEach((_, colIndex) => {
      selectedCells.value.add(`${rowIndex},${colIndex}`)
    })
  })
  toast.add({
    severity: 'info',
    summary: 'Selected All',
    detail: 'All cells selected',
    life: 1000
  })
}

const clearSelection = () => {
  selectedCells.value.clear()
  lastSelectedCell.value = null
  formulaBarValue.value = ''
}

// Cell keydown handler - start editing on any printable key
const onCellKeyDown = (event, row, col) => {
  // Don't interfere if already editing
  if (editingCell.value) return

  // Handle navigation keys
  if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Tab', 'Enter'].includes(event.key)) {
    event.preventDefault()
    handleCellNavigation(event.key, row, col, event.shiftKey)
    return
  }

  // Start editing on F2
  if (event.key === 'F2') {
    event.preventDefault()
    startEditing(row, col)
    return
  }

  // Delete key clears the cell
  if (event.key === 'Delete') {
    event.preventDefault()
    sheetData.value[row][columnHeaders.value[col]] = ''
    return
  }

  // Start editing on any printable character
  if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) {
    event.preventDefault()
    startEditingWithCharacter(row, col, event.key)
  }
}

// Handle cell navigation with arrow keys
const handleCellNavigation = (key, row, col, shiftKey = false) => {
  let newRow = row
  let newCol = col

  switch (key) {
    case 'ArrowUp':
      newRow = Math.max(0, row - 1)
      break
    case 'ArrowDown':
      newRow = row + 1
      // Add new row if at the end
      if (newRow >= sheetData.value.length) {
        addRow(true) // silent mode
      }
      break
    case 'Enter':
      newRow = row + 1
      // Add new row if at the end
      if (newRow >= sheetData.value.length) {
        addRow(true) // silent mode
      }
      break
    case 'ArrowLeft':
      newCol = Math.max(0, col - 1)
      break
    case 'ArrowRight':
      newCol = col + 1
      // Add new column if at the end
      if (newCol >= columnHeaders.value.length) {
        addColumn(true) // silent mode
      }
      break
    case 'Tab':
      if (shiftKey) {
        // Shift+Tab goes left
        newCol = Math.max(0, col - 1)
      } else {
        // Tab goes right
        newCol = col + 1
        // Add new column if at the end
        if (newCol >= columnHeaders.value.length) {
          addColumn(true) // silent mode
        }
      }
      break
  }

  if (newRow !== row || newCol !== col) {
    nextTick(() => {
      selectCell(newRow, newCol, { ctrlKey: false, shiftKey: false })
    })
  }
}

// Editing functions
const startEditing = (row, col) => {
  editingCell.value = { row, col }
  editingValue.value = sheetData.value[row][columnHeaders.value[col]] || ''
  nextTick(() => {
    if (cellInput.value && cellInput.value[0]) {
      cellInput.value[0].focus()
      cellInput.value[0].select()
    }
  })
}

// Start editing with a character typed by user
const startEditingWithCharacter = (row, col, char) => {
  editingCell.value = { row, col }
  editingValue.value = char
  nextTick(() => {
    if (cellInput.value && cellInput.value[0]) {
      cellInput.value[0].focus()
    }
  })
}

const finishEditing = () => {
  if (editingCell.value) {
    const { row, col } = editingCell.value
    sheetData.value[row][columnHeaders.value[col]] = editingValue.value
    editingCell.value = null
    editingValue.value = ''
  }
}

const finishEditingAndMoveDown = () => {
  if (editingCell.value) {
    const { row, col } = editingCell.value
    finishEditing()

    const newRow = row + 1
    // Add new row if at the end
    if (newRow >= sheetData.value.length) {
      addRow(true) // silent mode
    }

    nextTick(() => {
      selectCell(newRow, col, { ctrlKey: false, shiftKey: false })
    })
  }
}

const finishEditingAndMoveRight = () => {
  if (editingCell.value) {
    const { row, col } = editingCell.value
    finishEditing()

    const newCol = col + 1
    // Add new column if at the end
    if (newCol >= columnHeaders.value.length) {
      addColumn(true) // silent mode
    }

    nextTick(() => {
      selectCell(row, newCol, { ctrlKey: false, shiftKey: false })
    })
  }
}

const cancelEditing = () => {
  editingCell.value = null
  editingValue.value = ''
}


// Row/Column operations
const addRow = (silent = false) => {
  const newRow = {}
  columnHeaders.value.forEach(col => {
    newRow[col] = ''
  })
  sheetData.value.push(newRow)

  if (!silent) {
    toast.add({
      severity: 'success',
      summary: 'Row Added',
      detail: `Row ${sheetData.value.length} added`,
      life: 2000
    })
  }
}

const addColumn = (silent = false) => {
  const newColIndex = columnHeaders.value.length
  const newColLabel = getColumnLabel(newColIndex)
  columnHeaders.value.push(newColLabel)

  sheetData.value.forEach(row => {
    row[newColLabel] = ''
  })

  if (!silent) {
    toast.add({
      severity: 'success',
      summary: 'Column Added',
      detail: `Column ${newColLabel} added`,
      life: 2000
    })
  }
}

const insertRowAbove = () => {
  addRow()
}

const insertRowBelow = () => {
  addRow()
}

const insertColumnLeft = () => {
  addColumn()
}

const insertColumnRight = () => {
  addColumn()
}

const deleteRow = () => {
  if (lastSelectedCell.value && sheetData.value.length > 1) {
    const { row } = lastSelectedCell.value
    sheetData.value.splice(row, 1)
    selectedCells.value.clear()

    toast.add({
      severity: 'warn',
      summary: 'Row Deleted',
      detail: `Row ${row + 1} deleted`,
      life: 2000
    })
  }
}

const deleteColumn = () => {
  if (lastSelectedCell.value && columnHeaders.value.length > 1) {
    const { col } = lastSelectedCell.value
    const colLabel = columnHeaders.value[col]

    columnHeaders.value.splice(col, 1)
    sheetData.value.forEach(row => {
      delete row[colLabel]
    })

    selectedCells.value.clear()

    toast.add({
      severity: 'warn',
      summary: 'Column Deleted',
      detail: `Column ${colLabel} deleted`,
      life: 2000
    })
  }
}

const deleteSelectedCells = () => {
  selectedCells.value.forEach(cellKey => {
    const [row, col] = cellKey.split(',').map(Number)
    sheetData.value[row][columnHeaders.value[col]] = ''
  })

  toast.add({
    severity: 'info',
    summary: 'Contents Cleared',
    detail: `${selectedCells.value.size} cell(s) cleared`,
    life: 2000
  })
}

// Copy/Paste operations
const copySelection = () => {
  if (selectedCells.value.size === 0) {
    toast.add({
      severity: 'warn',
      summary: 'No Selection',
      detail: 'Please select cells to copy',
      life: 2000
    })
    return
  }

  const data = []
  selectedCells.value.forEach(cellKey => {
    const [row, col] = cellKey.split(',').map(Number)
    data.push({
      row,
      col,
      value: sheetData.value[row][columnHeaders.value[col]]
    })
  })

  clipboard.value = data

  toast.add({
    severity: 'info',
    summary: 'Copied',
    detail: `${selectedCells.value.size} cell(s) copied`,
    life: 2000
  })
}

const pasteSelection = () => {
  if (!clipboard.value || clipboard.value.length === 0) {
    toast.add({
      severity: 'warn',
      summary: 'No Data',
      detail: 'No data in clipboard',
      life: 2000
    })
    return
  }

  if (lastSelectedCell.value) {
    const { row: startRow, col: startCol } = lastSelectedCell.value

    clipboard.value.forEach(item => {
      const targetRow = startRow + (item.row - clipboard.value[0].row)
      const targetCol = startCol + (item.col - clipboard.value[0].col)

      if (targetRow < sheetData.value.length && targetCol < columnHeaders.value.length) {
        sheetData.value[targetRow][columnHeaders.value[targetCol]] = item.value
      }
    })

    toast.add({
      severity: 'success',
      summary: 'Pasted',
      detail: `${clipboard.value.length} cell(s) pasted`,
      life: 2000
    })
  }
}

const cutSelection = () => {
  copySelection()
  deleteSelectedCells()
}

// File operations
const newSheet = () => {
  if (confirm('Create a new spreadsheet? Unsaved changes will be lost.')) {
    sheetTitle.value = 'Untitled Spreadsheet'
    initializeSheet()
    selectedCells.value.clear()
    clipboard.value = null

    toast.add({
      severity: 'info',
      summary: 'New Spreadsheet',
      detail: 'Created a new empty spreadsheet',
      life: 2000
    })
  }
}

const quickSave = () => {
  const dataStr = JSON.stringify({
    name: sheetTitle.value,
    columns: columnHeaders.value,
    data: sheetData.value,
    timestamp: new Date().toISOString()
  }, null, 2)

  const blob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${sheetTitle.value || 'spreadsheet'}-${new Date().toISOString().split('T')[0]}.json`
  link.click()
  URL.revokeObjectURL(url)

  toast.add({
    severity: 'success',
    summary: 'Saved',
    detail: 'Spreadsheet saved as JSON',
    life: 3000
  })
}

const handleFileImport = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  try {
    const fileExtension = file.name.split('.').pop().toLowerCase()

    if (['xlsx', 'xls'].includes(fileExtension)) {
      const arrayBuffer = await file.arrayBuffer()
      const workbook = XLSX.read(arrayBuffer, { type: 'array' })
      const sheetName = workbook.SheetNames[0]
      const worksheet = workbook.Sheets[sheetName]
      const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 })

      if (jsonData.length > 0) {
        // First row as headers
        const headers = jsonData[0].map((_, i) => getColumnLabel(i))
        columnHeaders.value = headers

        // Remaining rows as data
        sheetData.value = jsonData.slice(1).map(row => {
          const rowData = {}
          headers.forEach((header, i) => {
            rowData[header] = row[i] !== undefined ? String(row[i]) : ''
          })
          return rowData
        })

        sheetTitle.value = file.name.split('.')[0]
        showImportDialog.value = false

        toast.add({
          severity: 'success',
          summary: 'Import Successful',
          detail: `Imported ${sheetData.value.length} rows`,
          life: 3000
        })
      }
    } else if (fileExtension === 'csv') {
      const text = await file.text()
      parseCSVData(text)
      sheetTitle.value = file.name.split('.')[0]
      showImportDialog.value = false
    } else if (fileExtension === 'json') {
      const text = await file.text()
      const parsed = JSON.parse(text)

      if (parsed.columns && parsed.data) {
        columnHeaders.value = parsed.columns
        sheetData.value = parsed.data
        if (parsed.name) sheetTitle.value = parsed.name
      }

      showImportDialog.value = false

      toast.add({
        severity: 'success',
        summary: 'Import Successful',
        detail: 'Data imported from JSON',
        life: 3000
      })
    }
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Import Failed',
      detail: error.message,
      life: 5000
    })
  }

  event.target.value = ''
}

const parseCSVData = (text) => {
  const lines = text.trim().split('\n')
  if (lines.length === 0) return

  const rows = lines.map(line => {
    // Simple CSV parser (doesn't handle quoted commas)
    return line.split(',').map(cell => cell.trim())
  })

  const maxCols = Math.max(...rows.map(row => row.length))
  const headers = Array.from({ length: maxCols }, (_, i) => getColumnLabel(i))
  columnHeaders.value = headers

  sheetData.value = rows.map(row => {
    const rowData = {}
    headers.forEach((header, i) => {
      rowData[header] = row[i] || ''
    })
    return rowData
  })

  toast.add({
    severity: 'success',
    summary: 'Import Successful',
    detail: `Imported ${sheetData.value.length} rows`,
    life: 3000
  })
}

const importFromClipboard = () => {
  try {
    parseCSVData(pasteData.value)
    showImportDialog.value = false
    pasteData.value = ''
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
    const timestamp = new Date().toISOString().split('T')[0]
    const name = sheetTitle.value || 'spreadsheet'

    if (format === 'csv') {
      const csv = sheetData.value.map(row =>
        columnHeaders.value.map(col => {
          const value = row[col] || ''
          return value.includes(',') ? `"${value}"` : value
        }).join(',')
      ).join('\n')

      const blob = new Blob([csv], { type: 'text/csv' })
      downloadFile(blob, `${name}-${timestamp}.csv`)
    } else if (format === 'excel') {
      const ws = XLSX.utils.json_to_sheet(
        sheetData.value.map(row => {
          const obj = {}
          columnHeaders.value.forEach(col => {
            obj[col] = row[col] || ''
          })
          return obj
        })
      )

      const wb = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(wb, ws, 'Sheet1')
      const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' })
      const blob = new Blob([excelBuffer], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      })
      downloadFile(blob, `${name}-${timestamp}.xlsx`)
    } else if (format === 'json') {
      const json = JSON.stringify({
        name: sheetTitle.value,
        columns: columnHeaders.value,
        data: sheetData.value,
        exported: new Date().toISOString()
      }, null, 2)

      const blob = new Blob([json], { type: 'application/json' })
      downloadFile(blob, `${name}-${timestamp}.json`)
    } else if (format === 'html') {
      let html = '<table border="1">\n<thead>\n<tr>'
      columnHeaders.value.forEach(col => {
        html += `<th>${col}</th>`
      })
      html += '</tr>\n</thead>\n<tbody>\n'

      sheetData.value.forEach(row => {
        html += '<tr>'
        columnHeaders.value.forEach(col => {
          html += `<td>${row[col] || ''}</td>`
        })
        html += '</tr>\n'
      })

      html += '</tbody>\n</table>'

      const blob = new Blob([html], { type: 'text/html' })
      downloadFile(blob, `${name}-${timestamp}.html`)
    }

    showExportDialog.value = false

    toast.add({
      severity: 'success',
      summary: 'Export Successful',
      detail: `Data exported as ${format.toUpperCase()}`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Export Failed',
      detail: error.message,
      life: 5000
    })
  }
}

const downloadFile = (blob, filename) => {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.click()
  URL.revokeObjectURL(url)
}

const confirmClearAll = () => {
  if (confirm('Clear all data? This action cannot be undone.')) {
    newSheet()
  }
}

// Find & Replace
const findNext = () => {
  if (!findText.value) return

  let found = false
  for (let i = 0; i < sheetData.value.length && !found; i++) {
    for (let j = 0; j < columnHeaders.value.length && !found; j++) {
      const value = sheetData.value[i][columnHeaders.value[j]]
      if (value && value.toLowerCase().includes(findText.value.toLowerCase())) {
        selectCell(i, j, { ctrlKey: false, shiftKey: false })
        found = true
      }
    }
  }

  if (!found) {
    toast.add({
      severity: 'info',
      summary: 'Not Found',
      detail: `"${findText.value}" not found`,
      life: 2000
    })
  }
}

const replaceOne = () => {
  if (lastSelectedCell.value) {
    const { row, col } = lastSelectedCell.value
    const currentValue = sheetData.value[row][columnHeaders.value[col]]

    if (currentValue && currentValue.toLowerCase().includes(findText.value.toLowerCase())) {
      sheetData.value[row][columnHeaders.value[col]] = currentValue.replace(
        new RegExp(findText.value, 'gi'),
        replaceText.value
      )

      toast.add({
        severity: 'success',
        summary: 'Replaced',
        detail: 'Value replaced',
        life: 1000
      })
    }
  }
}

const replaceAll = () => {
  let count = 0

  sheetData.value.forEach((row, rowIndex) => {
    columnHeaders.value.forEach(col => {
      const value = row[col]
      if (value && value.toLowerCase().includes(findText.value.toLowerCase())) {
        sheetData.value[rowIndex][col] = value.replace(
          new RegExp(findText.value, 'gi'),
          replaceText.value
        )
        count++
      }
    })
  })

  toast.add({
    severity: 'success',
    summary: 'Replace All',
    detail: `${count} occurrence(s) replaced`,
    life: 3000
  })
}

// Sort
const sortData = (order) => {
  if (!sortColumn.value) {
    toast.add({
      severity: 'warn',
      summary: 'No Column',
      detail: 'Please select a column to sort',
      life: 2000
    })
    return
  }

  sortOrder.value = order

  sheetData.value.sort((a, b) => {
    const aVal = a[sortColumn.value] || ''
    const bVal = b[sortColumn.value] || ''

    if (order === 'asc') {
      return aVal > bVal ? 1 : aVal < bVal ? -1 : 0
    } else {
      return aVal < bVal ? 1 : aVal > bVal ? -1 : 0
    }
  })

  toast.add({
    severity: 'success',
    summary: 'Data Sorted',
    detail: `Sorted by ${sortColumn.value} (${order})`,
    life: 2000
  })

  showSortDialog.value = false
}

// Statistics
const showStatistics = () => {
  let totalCells = 0
  let filledCells = 0
  let numericCells = 0

  sheetData.value.forEach(row => {
    columnHeaders.value.forEach(col => {
      totalCells++
      const value = row[col]
      if (value !== '' && value !== null && value !== undefined) {
        filledCells++
        if (!isNaN(value) && value !== '') {
          numericCells++
        }
      }
    })
  })

  toast.add({
    severity: 'info',
    summary: 'Spreadsheet Statistics',
    detail: `${sheetData.value.length} rows, ${columnHeaders.value.length} cols, ${filledCells}/${totalCells} filled, ${numericCells} numeric`,
    life: 6000
  })
}

// Context menu handlers
const onCellContextMenu = (event, row, col) => {
  selectCell(row, col, { ctrlKey: false, shiftKey: false })
  contextMenuRef.value.show(event)
}

const onRowHeaderContextMenu = (event, row) => {
  selectRow(row)
  contextMenuRef.value.show(event)
}

const onColumnHeaderContextMenu = (event, col) => {
  selectedCells.value.clear()
  sheetData.value.forEach((_, rowIndex) => {
    selectedCells.value.add(`${rowIndex},${col}`)
  })
  contextMenuRef.value.show(event)
}

// Keyboard shortcuts
const handleKeyboardShortcuts = (event) => {
  if (event.ctrlKey || event.metaKey) {
    if (event.key === 'c') {
      event.preventDefault()
      copySelection()
    } else if (event.key === 'v') {
      event.preventDefault()
      pasteSelection()
    } else if (event.key === 'x') {
      event.preventDefault()
      cutSelection()
    } else if (event.key === 'a') {
      event.preventDefault()
      selectAll()
    } else if (event.key === 'f') {
      event.preventDefault()
      showFindReplace.value = true
    } else if (event.key === 's') {
      event.preventDefault()
      quickSave()
    }
  } else if (event.key === 'Delete') {
    event.preventDefault()
    deleteSelectedCells()
  }
}

// Lifecycle
onMounted(() => {
  initializeSheet()
  document.addEventListener('keydown', handleKeyboardShortcuts)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeyboardShortcuts)
})
</script>

<style scoped>
/* Excel Container */
.excel-container {
  height: calc(100vh - 280px);
  width: 100%;
  padding: 1rem;
  background: var(--surface-ground);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  overflow: hidden;
}

/* Toolbar */
.excel-toolbar,
.excel-secondary-toolbar {
  background: var(--surface-card);
  border-bottom: 1px solid var(--surface-border);
  padding: 0.75rem 1rem;
}

.excel-secondary-toolbar {
  background: var(--surface-section);
  padding: 0.5rem 1rem;
}

.excel-label {
  font-weight: 600;
  color: var(--text-color);
  font-size: 0.9rem;
}

.excel-name {
  min-width: 200px;
}

.excel-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--primary-color);
  cursor: pointer;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.excel-title:hover {
  background: var(--surface-hover);
}

.stats-tag {
  font-size: 0.8rem;
  padding: 0.25rem 0.5rem;
}

/* Reference Bar */
.excel-reference-bar {
  display: flex;
  align-items: center;
  padding: 0.5rem 0.75rem;
  background: var(--surface-card);
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  min-height: 40px;
}

.reference-bar-cell-ref {
  display: flex;
  align-items: center;
  font-weight: 600;
  color: var(--text-color);
  font-size: 0.9rem;
}

.cell-ref-text {
  color: var(--primary-color);
}

/* Excel Grid */
.excel-grid-wrapper {
  flex: 1;
  overflow: auto;
  border: 2px solid var(--surface-border);
  border-radius: 8px;
  background: white;
  min-height: 0;
}

.excel-grid-header {
  display: flex;
  position: sticky;
  top: 0;
  z-index: 10;
  background: var(--surface-100);
}

.excel-corner-cell {
  width: 50px;
  min-width: 50px;
  height: 30px;
  border-right: 1px solid var(--surface-border);
  border-bottom: 1px solid var(--surface-border);
  background: var(--surface-200);
}

.excel-column-header {
  min-width: 100px;
  width: 100px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  border-right: 1px solid var(--surface-border);
  border-bottom: 1px solid var(--surface-border);
  background: var(--surface-100);
  color: var(--text-color);
  font-size: 0.85rem;
  user-select: none;
  cursor: pointer;
}

.excel-column-header:hover {
  background: var(--surface-200);
}

.excel-grid-body {
  /* No overflow here - parent handles it */
}

.excel-row {
  display: flex;
}

.excel-row-header {
  width: 50px;
  min-width: 50px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  font-weight: 600;
  border-right: 1px solid var(--surface-border);
  border-bottom: 1px solid var(--surface-border);
  background: var(--surface-100);
  color: var(--text-color-secondary);
  user-select: none;
  cursor: pointer;
}

.excel-row-header:hover {
  background: var(--surface-200);
}

.excel-cell {
  min-width: 100px;
  width: 100px;
  height: 28px;
  border-right: 1px solid var(--surface-border);
  border-bottom: 1px solid var(--surface-border);
  padding: 2px 6px;
  cursor: cell;
  position: relative;
  background: white;
  display: flex;
  align-items: center;
}

.excel-cell:hover {
  background: var(--surface-50);
}

.excel-cell-selected {
  background: var(--primary-50) !important;
  outline: 2px solid var(--primary-color);
  outline-offset: -2px;
  z-index: 1;
}

.excel-cell-editing {
  outline: 2px solid var(--primary-color) !important;
  outline-offset: -2px;
  z-index: 2;
}

.excel-cell-value {
  width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.85rem;
}

.excel-cell-input {
  width: 100%;
  border: none;
  outline: none;
  background: white;
  font-size: 0.85rem;
  padding: 0;
  font-family: inherit;
}


/* Dialogs */
.import-content,
.export-content,
.find-replace-content,
.sort-content,
.filter-content {
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

.file-help {
  color: var(--text-color-secondary);
  font-style: italic;
}

.paste-textarea {
  width: 100%;
  min-height: 120px;
  margin: 1rem 0;
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

</style>
