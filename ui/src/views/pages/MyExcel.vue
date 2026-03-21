<script setup>
import { markRaw } from 'vue';
import { Upload, Grid, Search, Filter, Pencil, RefreshCcw, Scissors, Plus, BarChart, PlusCircle, Eraser, Check, Copy, SortDesc, Download, File, Table, Trash2, FileSpreadsheet, CheckSquare, Save, SortAsc, Code } from 'lucide-vue-next';
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue';
import { toast } from 'vue-sonner';
import * as XLSX from 'xlsx';

import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ContextMenu, ContextMenuContent, ContextMenuItem, ContextMenuSeparator, ContextMenuTrigger } from '@/components/ui/context-menu';

// State
const sheetTitle = ref('MyExcel Spreadsheet');
const sheetData = ref([]);
const columnHeaders = ref([]);
const selectedCells = ref(new Set());
const editingCell = ref(null);
const editingValue = ref('');
const clipboard = ref(null);
const showImportDialog = ref(false);
const showExportDialog = ref(false);
const pasteData = ref('');
const cellInput = ref(null);
const editingSheetTitle = ref(false);
const gridBody = ref(null);
const showFindReplace = ref(false);
const showSortDialog = ref(false);
const showFilterDialog = ref(false);
const findText = ref('');
const replaceText = ref('');
const sortColumn = ref(null);
const sortOrder = ref('asc');
const lastSelectedCell = ref(null);

// Initialize with default data
const initializeSheet = () => {
    const rows = 20;
    const cols = 10;

    // Generate column headers (A, B, C, ..., Z, AA, AB, ...)
    const headers = [];
    for (let i = 0; i < cols; i++) {
        headers.push(getColumnLabel(i));
    }
    columnHeaders.value = headers;

    // Initialize empty cells
    const data = [];
    for (let i = 0; i < rows; i++) {
        const row = {};
        headers.forEach((col) => {
            row[col] = '';
        });
        data.push(row);
    }
    sheetData.value = data;
};

// Generate Excel-style column labels (A, B, C, ..., Z, AA, AB, ...)
const getColumnLabel = (index) => {
    let label = '';
    let num = index + 1;
    while (num > 0) {
        let remainder = (num - 1) % 26;
        label = String.fromCharCode(65 + remainder) + label;
        num = Math.floor((num - 1) / 26);
    }
    return label;
};

// Current cell reference (e.g., A1, B5)
const currentCellReference = computed(() => {
    if (!lastSelectedCell.value) return '';
    const { row, col } = lastSelectedCell.value;
    return `${columnHeaders.value[col]}${row + 1}`;
});

// Cell selection functions
const isCellSelected = (row, col) => {
    return selectedCells.value.has(`${row},${col}`);
};

const selectCell = (row, col, event) => {
    if (!event.ctrlKey && !event.shiftKey) {
        selectedCells.value.clear();
    }
    selectedCells.value.add(`${row},${col}`);
    lastSelectedCell.value = { row, col };

    // Focus the cell for keyboard input
    nextTick(() => {
        const cellElements = document.querySelectorAll('.excel-cell');
        const cellIndex = row * columnHeaders.value.length + col;
        if (cellElements[cellIndex]) {
            cellElements[cellIndex].focus();
        }
    });
};

const selectRow = (rowIndex) => {
    selectedCells.value.clear();
    columnHeaders.value.forEach((_, colIndex) => {
        selectedCells.value.add(`${rowIndex},${colIndex}`);
    });
};

const selectAll = () => {
    selectedCells.value.clear();
    sheetData.value.forEach((_, rowIndex) => {
        columnHeaders.value.forEach((_, colIndex) => {
            selectedCells.value.add(`${rowIndex},${colIndex}`);
        });
    });
    toast('Selected All', {
        description: 'All cells selected'
    });
};

const clearSelection = () => {
    selectedCells.value.clear();
    lastSelectedCell.value = null;
};

// Cell keydown handler - start editing on any printable key
const onCellKeyDown = (event, row, col) => {
    // Don't interfere if already editing
    if (editingCell.value) return;

    // Handle navigation keys
    if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Tab', 'Enter'].includes(event.key)) {
        event.preventDefault();
        handleCellNavigation(event.key, row, col, event.shiftKey);
        return;
    }

    // Start editing on F2
    if (event.key === 'F2') {
        event.preventDefault();
        startEditing(row, col);
        return;
    }

    // Delete key clears the cell
    if (event.key === 'Delete') {
        event.preventDefault();
        sheetData.value[row][columnHeaders.value[col]] = '';
        return;
    }

    // Start editing on any printable character
    if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) {
        event.preventDefault();
        startEditingWithCharacter(row, col, event.key);
    }
};

// Handle cell navigation with arrow keys
const handleCellNavigation = (key, row, col, shiftKey = false) => {
    let newRow = row;
    let newCol = col;

    switch (key) {
        case 'ArrowUp':
            newRow = Math.max(0, row - 1);
            break;
        case 'ArrowDown':
            newRow = row + 1;
            // Add new row if at the end
            if (newRow >= sheetData.value.length) {
                addRow(true); // silent mode
            }
            break;
        case 'Enter':
            newRow = row + 1;
            // Add new row if at the end
            if (newRow >= sheetData.value.length) {
                addRow(true); // silent mode
            }
            break;
        case 'ArrowLeft':
            newCol = Math.max(0, col - 1);
            break;
        case 'ArrowRight':
            newCol = col + 1;
            // Add new column if at the end
            if (newCol >= columnHeaders.value.length) {
                addColumn(true); // silent mode
            }
            break;
        case 'Tab':
            if (shiftKey) {
                // Shift+Tab goes left
                newCol = Math.max(0, col - 1);
            } else {
                // Tab goes right
                newCol = col + 1;
                // Add new column if at the end
                if (newCol >= columnHeaders.value.length) {
                    addColumn(true); // silent mode
                }
            }
            break;
    }

    if (newRow !== row || newCol !== col) {
        nextTick(() => {
            selectCell(newRow, newCol, { ctrlKey: false, shiftKey: false });
        });
    }
};

// Editing functions
const startEditing = (row, col) => {
    editingCell.value = { row, col };
    editingValue.value = sheetData.value[row][columnHeaders.value[col]] || '';
    nextTick(() => {
        if (cellInput.value && cellInput.value[0]) {
            cellInput.value[0].focus();
            cellInput.value[0].select();
        }
    });
};

// Start editing with a character typed by user
const startEditingWithCharacter = (row, col, char) => {
    editingCell.value = { row, col };
    editingValue.value = char;
    nextTick(() => {
        if (cellInput.value && cellInput.value[0]) {
            cellInput.value[0].focus();
        }
    });
};

const finishEditing = () => {
    if (editingCell.value) {
        const { row, col } = editingCell.value;
        sheetData.value[row][columnHeaders.value[col]] = editingValue.value;
        editingCell.value = null;
        editingValue.value = '';
    }
};

const finishEditingAndMoveDown = () => {
    if (editingCell.value) {
        const { row, col } = editingCell.value;
        finishEditing();

        const newRow = row + 1;
        // Add new row if at the end
        if (newRow >= sheetData.value.length) {
            addRow(true); // silent mode
        }

        nextTick(() => {
            selectCell(newRow, col, { ctrlKey: false, shiftKey: false });
        });
    }
};

const finishEditingAndMoveRight = () => {
    if (editingCell.value) {
        const { row, col } = editingCell.value;
        finishEditing();

        const newCol = col + 1;
        // Add new column if at the end
        if (newCol >= columnHeaders.value.length) {
            addColumn(true); // silent mode
        }

        nextTick(() => {
            selectCell(row, newCol, { ctrlKey: false, shiftKey: false });
        });
    }
};

const cancelEditing = () => {
    editingCell.value = null;
    editingValue.value = '';
};

// Row/Column operations
const addRow = (silent = false) => {
    const newRow = {};
    columnHeaders.value.forEach((col) => {
        newRow[col] = '';
    });
    sheetData.value.push(newRow);

    if (!silent) {
        toast('Row Added', {
            description: `Row ${sheetData.value.length} added`
        });
    }
};

const addColumn = (silent = false) => {
    const newColIndex = columnHeaders.value.length;
    const newColLabel = getColumnLabel(newColIndex);
    columnHeaders.value.push(newColLabel);

    sheetData.value.forEach((row) => {
        row[newColLabel] = '';
    });

    if (!silent) {
        toast('Column Added', {
            description: `Column ${newColLabel} added`
        });
    }
};

const deleteRow = () => {
    if (lastSelectedCell.value && sheetData.value.length > 1) {
        const { row } = lastSelectedCell.value;
        sheetData.value.splice(row, 1);
        selectedCells.value.clear();

        toast('Row Deleted', {
            description: `Row ${row + 1} deleted`
        });
    }
};

const deleteColumn = () => {
    if (lastSelectedCell.value && columnHeaders.value.length > 1) {
        const { col } = lastSelectedCell.value;
        const colLabel = columnHeaders.value[col];

        columnHeaders.value.splice(col, 1);
        sheetData.value.forEach((row) => {
            delete row[colLabel];
        });

        selectedCells.value.clear();

        toast('Column Deleted', {
            description: `Column ${colLabel} deleted`
        });
    }
};

const deleteSelectedCells = () => {
    selectedCells.value.forEach((cellKey) => {
        const [row, col] = cellKey.split(',').map(Number);
        sheetData.value[row][columnHeaders.value[col]] = '';
    });

    toast('Contents Cleared', {
        description: `${selectedCells.value.size} cell(s) cleared`
    });
};

// Copy/Paste operations
const copySelection = () => {
    if (selectedCells.value.size === 0) {
        toast.error('No Selection', {
            description: 'Please select cells to copy'
        });
        return;
    }

    const data = [];
    selectedCells.value.forEach((cellKey) => {
        const [row, col] = cellKey.split(',').map(Number);
        data.push({
            row,
            col,
            value: sheetData.value[row][columnHeaders.value[col]]
        });
    });

    clipboard.value = data;

    toast('Copied', {
        description: `${selectedCells.value.size} cell(s) copied`
    });
};

const pasteSelection = () => {
    if (!clipboard.value || clipboard.value.length === 0) {
        toast.error('No Data', {
            description: 'No data in clipboard'
        });
        return;
    }

    if (lastSelectedCell.value) {
        const { row: startRow, col: startCol } = lastSelectedCell.value;

        clipboard.value.forEach((item) => {
            const targetRow = startRow + (item.row - clipboard.value[0].row);
            const targetCol = startCol + (item.col - clipboard.value[0].col);

            if (targetRow < sheetData.value.length && targetCol < columnHeaders.value.length) {
                sheetData.value[targetRow][columnHeaders.value[targetCol]] = item.value;
            }
        });

        toast('Pasted', {
            description: `${clipboard.value.length} cell(s) pasted`
        });
    }
};

const cutSelection = () => {
    copySelection();
    deleteSelectedCells();
};

// File operations
const newSheet = () => {
    if (confirm('Create a new spreadsheet? Unsaved changes will be lost.')) {
        sheetTitle.value = 'Untitled Spreadsheet';
        initializeSheet();
        selectedCells.value.clear();
        clipboard.value = null;

        toast('New Spreadsheet', {
            description: 'Created a new empty spreadsheet'
        });
    }
};

const quickSave = () => {
    const dataStr = JSON.stringify(
        {
            name: sheetTitle.value,
            columns: columnHeaders.value,
            data: sheetData.value,
            timestamp: new Date().toISOString()
        },
        null,
        2
    );

    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${sheetTitle.value || 'spreadsheet'}-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);

    toast('Saved', {
        description: 'Spreadsheet saved as JSON'
    });
};

const handleFileImport = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    try {
        const fileExtension = file.name.split('.').pop().toLowerCase();

        if (['xlsx', 'xls'].includes(fileExtension)) {
            const arrayBuffer = await file.arrayBuffer();
            const workbook = XLSX.read(arrayBuffer, { type: 'array' });
            const sheetName = workbook.SheetNames[0];
            const worksheet = workbook.Sheets[sheetName];
            const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

            if (jsonData.length > 0) {
                // First row as headers
                const headers = jsonData[0].map((_, i) => getColumnLabel(i));
                columnHeaders.value = headers;

                // Remaining rows as data
                sheetData.value = jsonData.slice(1).map((row) => {
                    const rowData = {};
                    headers.forEach((header, i) => {
                        rowData[header] = row[i] !== undefined ? String(row[i]) : '';
                    });
                    return rowData;
                });

                sheetTitle.value = file.name.split('.')[0];
                showImportDialog.value = false;

                toast('Import Successful', {
                    description: `Imported ${sheetData.value.length} rows`
                });
            }
        } else if (fileExtension === 'csv') {
            const text = await file.text();
            parseCSVData(text);
            sheetTitle.value = file.name.split('.')[0];
            showImportDialog.value = false;
        } else if (fileExtension === 'json') {
            const text = await file.text();
            const parsed = JSON.parse(text);

            if (parsed.columns && parsed.data) {
                columnHeaders.value = parsed.columns;
                sheetData.value = parsed.data;
                if (parsed.name) sheetTitle.value = parsed.name;
            }

            showImportDialog.value = false;

            toast('Import Successful', {
                description: 'Data imported from JSON'
            });
        }
    } catch (error) {
        toast.error('Import Failed', {
            description: error.message
        });
    }

    event.target.value = '';
};

const parseCSVData = (text) => {
    const lines = text.trim().split('\n');
    if (lines.length === 0) return;

    const rows = lines.map((line) => {
        // Simple CSV parser (doesn't handle quoted commas)
        return line.split(',').map((cell) => cell.trim());
    });

    const maxCols = Math.max(...rows.map((row) => row.length));
    const headers = Array.from({ length: maxCols }, (_, i) => getColumnLabel(i));
    columnHeaders.value = headers;

    sheetData.value = rows.map((row) => {
        const rowData = {};
        headers.forEach((header, i) => {
            rowData[header] = row[i] || '';
        });
        return rowData;
    });

    toast('Import Successful', {
        description: `Imported ${sheetData.value.length} rows`
    });
};

const importFromClipboard = () => {
    try {
        parseCSVData(pasteData.value);
        showImportDialog.value = false;
        pasteData.value = '';
    } catch (error) {
        toast.error('Import Failed', {
            description: 'Error parsing clipboard data'
        });
    }
};

const exportData = (format) => {
    try {
        const timestamp = new Date().toISOString().split('T')[0];
        const name = sheetTitle.value || 'spreadsheet';

        if (format === 'csv') {
            const csv = sheetData.value
                .map((row) =>
                    columnHeaders.value
                        .map((col) => {
                            const value = row[col] || '';
                            return value.includes(',') ? `"${value}"` : value;
                        })
                        .join(',')
                )
                .join('\n');

            const blob = new Blob([csv], { type: 'text/csv' });
            downloadFile(blob, `${name}-${timestamp}.csv`);
        } else if (format === 'excel') {
            const ws = XLSX.utils.json_to_sheet(
                sheetData.value.map((row) => {
                    const obj = {};
                    columnHeaders.value.forEach((col) => {
                        obj[col] = row[col] || '';
                    });
                    return obj;
                })
            );

            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
            const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
            const blob = new Blob([excelBuffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });
            downloadFile(blob, `${name}-${timestamp}.xlsx`);
        } else if (format === 'json') {
            const json = JSON.stringify(
                {
                    name: sheetTitle.value,
                    columns: columnHeaders.value,
                    data: sheetData.value,
                    exported: new Date().toISOString()
                },
                null,
                2
            );

            const blob = new Blob([json], { type: 'application/json' });
            downloadFile(blob, `${name}-${timestamp}.json`);
        } else if (format === 'html') {
            let html = '<table border="1">\n<thead>\n<tr>';
            columnHeaders.value.forEach((col) => {
                html += `<th>${col}</th>`;
            });
            html += '</tr>\n</thead>\n<tbody>\n';

            sheetData.value.forEach((row) => {
                html += '<tr>';
                columnHeaders.value.forEach((col) => {
                    html += `<td>${row[col] || ''}</td>`;
                });
                html += '</tr>\n';
            });

            html += '</tbody>\n</table>';

            const blob = new Blob([html], { type: 'text/html' });
            downloadFile(blob, `${name}-${timestamp}.html`);
        }

        showExportDialog.value = false;

        toast('Export Successful', {
            description: `Data exported as ${format.toUpperCase()}`
        });
    } catch (error) {
        toast.error('Export Failed', {
            description: error.message
        });
    }
};

const downloadFile = (blob, filename) => {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
};

const confirmClearAll = () => {
    if (confirm('Clear all data? This action cannot be undone.')) {
        newSheet();
    }
};

// Find & Replace
const findNext = () => {
    if (!findText.value) return;

    let found = false;
    for (let i = 0; i < sheetData.value.length && !found; i++) {
        for (let j = 0; j < columnHeaders.value.length && !found; j++) {
            const value = sheetData.value[i][columnHeaders.value[j]];
            if (value && value.toLowerCase().includes(findText.value.toLowerCase())) {
                selectCell(i, j, { ctrlKey: false, shiftKey: false });
                found = true;
            }
        }
    }

    if (!found) {
        toast('Not Found', {
            description: `"${findText.value}" not found`
        });
    }
};

const replaceOne = () => {
    if (lastSelectedCell.value) {
        const { row, col } = lastSelectedCell.value;
        const currentValue = sheetData.value[row][columnHeaders.value[col]];

        if (currentValue && currentValue.toLowerCase().includes(findText.value.toLowerCase())) {
            sheetData.value[row][columnHeaders.value[col]] = currentValue.replace(new RegExp(findText.value, 'gi'), replaceText.value);

            toast('Replaced', {
                description: 'Value replaced'
            });
        }
    }
};

const replaceAll = () => {
    let count = 0;

    sheetData.value.forEach((row, rowIndex) => {
        columnHeaders.value.forEach((col) => {
            const value = row[col];
            if (value && value.toLowerCase().includes(findText.value.toLowerCase())) {
                sheetData.value[rowIndex][col] = value.replace(new RegExp(findText.value, 'gi'), replaceText.value);
                count++;
            }
        });
    });

    toast('Replace All', {
        description: `${count} occurrence(s) replaced`
    });
};

// Sort
const sortData = (order) => {
    if (!sortColumn.value) {
        toast.error('No Column', {
            description: 'Please select a column to sort'
        });
        return;
    }

    sortOrder.value = order;

    sheetData.value.sort((a, b) => {
        const aVal = a[sortColumn.value] || '';
        const bVal = b[sortColumn.value] || '';

        if (order === 'asc') {
            return aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
        } else {
            return aVal < bVal ? 1 : aVal > bVal ? -1 : 0;
        }
    });

    toast('Data Sorted', {
        description: `Sorted by ${sortColumn.value} (${order})`
    });

    showSortDialog.value = false;
};

// Statistics
const showStatistics = () => {
    let totalCells = 0;
    let filledCells = 0;
    let numericCells = 0;

    sheetData.value.forEach((row) => {
        columnHeaders.value.forEach((col) => {
            totalCells++;
            const value = row[col];
            if (value !== '' && value !== null && value !== undefined) {
                filledCells++;
                if (!isNaN(value) && value !== '') {
                    numericCells++;
                }
            }
        });
    });

    toast('Spreadsheet Statistics', {
        description: `${sheetData.value.length} rows, ${columnHeaders.value.length} cols, ${filledCells}/${totalCells} filled, ${numericCells} numeric`
    });
};

// Context menu handlers (these are mostly native trigger events for context menu component instead)
const onCellContextMenu = (event, row, col) => {
    selectCell(row, col, { ctrlKey: false, shiftKey: false });
};

const onRowHeaderContextMenu = (event, row) => {
    selectRow(row);
};

const onColumnHeaderContextMenu = (event, col) => {
    selectedCells.value.clear();
    sheetData.value.forEach((_, rowIndex) => {
        selectedCells.value.add(`${rowIndex},${col}`);
    });
};

// Keyboard shortcuts
const handleKeyboardShortcuts = (event) => {
    if (event.ctrlKey || event.metaKey) {
        if (event.key === 'c') {
            event.preventDefault();
            copySelection();
        } else if (event.key === 'v') {
            event.preventDefault();
            pasteSelection();
        } else if (event.key === 'x') {
            event.preventDefault();
            cutSelection();
        } else if (event.key === 'a') {
            event.preventDefault();
            selectAll();
        } else if (event.key === 'f') {
            event.preventDefault();
            showFindReplace.value = true;
        } else if (event.key === 's') {
            event.preventDefault();
            quickSave();
        }
    } else if (event.key === 'Delete') {
        event.preventDefault();
        deleteSelectedCells();
    }
};

// Lifecycle
onMounted(() => {
    initializeSheet();
    document.addEventListener('keydown', handleKeyboardShortcuts);
});

onUnmounted(() => {
    document.removeEventListener('keydown', handleKeyboardShortcuts);
});
</script>

<template>
    <!-- Main Toolbar -->
    <div class="excel-toolbar flex items-center justify-between bg-card border-b p-3">
        <div class="flex items-center gap-2">
            <label class="excel-label font-semibold text-sm">Spreadsheet:</label>
            <div class="flex items-center gap-2 cursor-pointer group relative">
                <FileSpreadsheet class="w-4 h-4 text-muted-foreground" />
                <Input v-if="editingSheetTitle" v-model="sheetTitle" class="h-8 w-[200px]" autofocus @blur="editingSheetTitle = false" @keyup.enter="editingSheetTitle = false" />
                <span v-else class="excel-title font-medium px-2 py-1 rounded hover:bg-muted" @click="editingSheetTitle = true">
                    {{ sheetTitle || 'Untitled Spreadsheet' }}
                </span>
                <Pencil v-if="!editingSheetTitle" class="w-3 h-3 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" @click="editingSheetTitle = true" />
            </div>
        </div>

        <div class="flex items-center gap-2">
            <Badge variant="secondary" class="stats-tag gap-1">
                <Table class="w-4 h-4" />
                {{ sheetData.length }} rows × {{ columnHeaders.length }} cols
            </Badge>
            <Badge v-if="selectedCells.size > 0" variant="default" class="stats-tag gap-1 bg-green-600 hover:bg-green-700">
                <CheckSquare class="w-4 h-4" />
                {{ selectedCells.size }} cells
            </Badge>
        </div>

        <div class="flex items-center gap-2">
            <Button variant="ghost" size="icon" @click="quickSave" title="Save Spreadsheet">
                <Save class="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon" @click="showImportDialog = true" title="Import Data">
                <Upload class="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon" @click="showExportDialog = true" title="Export Data">
                <Download class="w-4 h-4" />
            </Button>
        </div>
    </div>

    <!-- Secondary Toolbar -->
    <div class="excel-secondary-toolbar flex items-center justify-between bg-muted/50 border-b p-2">
        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="newSheet" title="New Spreadsheet"><File class="w-4 h-4" /></Button>
            <div class="w-px h-5 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="addRow" title="Add Row"><PlusCircle class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="addColumn" title="Add Column"><Table class="w-4 h-4" /></Button>
            <div class="w-px h-5 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="copySelection" title="Copy (Ctrl+C)" :disabled="selectedCells.size === 0"><Copy class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="pasteSelection" title="Paste (Ctrl+V)" :disabled="!clipboard"><Copy class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="cutSelection" title="Cut (Ctrl+X)" :disabled="selectedCells.size === 0"><Scissors class="w-4 h-4" /></Button>
        </div>

        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="selectAll" title="Select All (Ctrl+A)"><CheckSquare class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="clearSelection" title="Clear Selection"><Eraser class="w-4 h-4" /></Button>
            <div class="w-px h-5 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="deleteSelectedCells" title="Delete Selected" :disabled="selectedCells.size === 0"><Trash2 class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" class="text-destructive hover:text-destructive hover:bg-destructive/10" @click="confirmClearAll" title="Clear All Data"><X class="w-4 h-4" /></Button>
        </div>

        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="showFindReplace = true" title="Find & Replace"><Search class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="showSortDialog = true" title="Sort Data"><SortDesc class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="showFilterDialog = true" title="Filter Data"><Filter class="w-4 h-4" /></Button>
            <div class="w-px h-5 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="showStatistics" title="Show Statistics"><BarChart class="w-4 h-4" /></Button>
        </div>
    </div>

    <!-- Excel Grid Container -->
    <!-- Excel Grid Container -->
    <ContextMenu>
        <ContextMenuTrigger asChild>
            <div class="excel-container" @keydown="handleKeyboardShortcuts">
                <!-- Cell Reference Bar -->
                <div class="excel-reference-bar">
                    <div class="reference-bar-cell-ref">
                        <Grid class="w-4 h-4 mr-2" />
                        <span class="cell-ref-text">{{ currentCellReference || 'Select a cell' }}</span>
                    </div>
                </div>

                <div class="excel-grid-wrapper bg-background border rounded-md">
                    <!-- Column Headers (A, B, C, ...) -->
                    <div class="excel-grid-header">
                        <div class="excel-corner-cell"></div>
                        <div v-for="(col, index) in columnHeaders" :key="`header-${index}`" class="excel-column-header" @contextmenu.prevent="onColumnHeaderContextMenu($event, index)">
                            {{ col }}
                        </div>
                    </div>

                    <!-- Grid Rows -->
                    <div class="excel-grid-body" ref="gridBody">
                        <div v-for="(row, rowIndex) in sheetData" :key="`row-${rowIndex}`" class="excel-row">
                            <!-- Row Number -->
                            <div class="excel-row-header" @click="selectRow(rowIndex)" @contextmenu.prevent="onRowHeaderContextMenu($event, rowIndex)">
                                {{ rowIndex + 1 }}
                            </div>

                            <!-- Cells -->
                            <div
                                v-for="(col, colIndex) in columnHeaders"
                                :key="`cell-${rowIndex}-${colIndex}`"
                                class="excel-cell"
                                :class="{
                                    'excel-cell-selected bg-primary/20 ring-1 ring-primary z-10': isCellSelected(rowIndex, colIndex),
                                    'excel-cell-editing ring-2 ring-primary z-20': editingCell && editingCell.row === rowIndex && editingCell.col === colIndex
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
                                    class="excel-cell-input bg-transparent w-full h-full outline-none px-1"
                                    autofocus
                                />
                                <span v-else class="excel-cell-value px-1 truncate w-full pointer-events-none">
                                    {{ sheetData[rowIndex][columnHeaders[colIndex]] || '' }}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContextMenuTrigger>
        <ContextMenuContent class="w-64">
            <ContextMenuItem @click="copySelection"> <Copy class="w-4 h-4 mr-2" /> Copy <span class="ml-auto text-xs tracking-widest opacity-60">Ctrl+C</span> </ContextMenuItem>
            <ContextMenuItem @click="pasteSelection"> <Copy class="w-4 h-4 mr-2" /> Paste <span class="ml-auto text-xs tracking-widest opacity-60">Ctrl+V</span> </ContextMenuItem>
            <ContextMenuItem @click="cutSelection"> <Scissors class="w-4 h-4 mr-2" /> Cut <span class="ml-auto text-xs tracking-widest opacity-60">Ctrl+X</span> </ContextMenuItem>
            <ContextMenuSeparator />
            <ContextMenuItem @click="addRow"> <Plus class="w-4 h-4 mr-2" /> Add Row </ContextMenuItem>
            <ContextMenuItem @click="addColumn"> <Plus class="w-4 h-4 mr-2" /> Add Column </ContextMenuItem>
            <ContextMenuSeparator />
            <ContextMenuItem @click="deleteRow" class="text-destructive focus:bg-destructive focus:text-destructive-foreground"> <Trash2 class="w-4 h-4 mr-2" /> Delete Row </ContextMenuItem>
            <ContextMenuItem @click="deleteColumn" class="text-destructive focus:bg-destructive focus:text-destructive-foreground"> <Trash2 class="w-4 h-4 mr-2" /> Delete Column </ContextMenuItem>
            <ContextMenuSeparator />
            <ContextMenuItem @click="deleteSelectedCells" class="text-destructive focus:bg-destructive focus:text-destructive-foreground">
                <Eraser class="w-4 h-4 mr-2" /> Clear Contents <span class="ml-auto text-xs tracking-widest opacity-60">Del</span>
            </ContextMenuItem>
        </ContextMenuContent>
    </ContextMenu>

    <!-- Import Dialog -->
    <Dialog :open="showImportDialog" @update:open="showImportDialog = $event">
        <DialogContent class="sm:max-w-[500px]">
            <DialogHeader>
                <DialogTitle>Import Data</DialogTitle>
            </DialogHeader>

            <div class="import-content py-4">
                <div class="import-options mb-6">
                    <h4 class="font-medium mb-3">Import from File</h4>
                    <div class="file-input-container flex items-center gap-4">
                        <input ref="fileInput" type="file" accept=".csv,.xlsx,.xls,.json" @change="handleFileImport" class="hidden" />
                        <Button variant="outline" class="gap-2" @click="$refs.fileInput.click()">
                            <Upload class="w-4 h-4" />
                            Choose File
                        </Button>
                        <span class="text-sm text-muted-foreground italic">Supported formats: CSV, Excel (.xlsx, .xls), JSON</span>
                    </div>
                </div>

                <div class="border-t my-4"></div>

                <div class="import-options mt-4">
                    <h4 class="font-medium mb-3">Paste Data</h4>
                    <Textarea v-model="pasteData" placeholder="Paste CSV or tab-separated data here..." :rows="6" class="paste-textarea w-full mb-3" />
                    <Button @click="importFromClipboard" :disabled="!pasteData.trim()" class="gap-2">
                        <Check class="w-4 h-4" />
                        Import from Clipboard
                    </Button>
                </div>
            </div>

            <DialogFooter>
                <Button variant="outline" @click="showImportDialog = false">Cancel</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- Export Dialog -->
    <Dialog :open="showExportDialog" @update:open="showExportDialog = $event">
        <DialogContent class="sm:max-w-[400px]">
            <DialogHeader>
                <DialogTitle>Export Data</DialogTitle>
            </DialogHeader>

            <div class="export-content py-4">
                <h4 class="font-medium mb-3">Choose Export Format</h4>
                <div class="grid grid-cols-2 gap-3">
                    <Button variant="outline" @click="exportData('csv')" class="justify-start gap-2"> <File class="w-4 h-4" /> CSV </Button>
                    <Button variant="outline" @click="exportData('excel')" class="justify-start gap-2"> <FileSpreadsheet class="w-4 h-4" /> Excel (.xlsx) </Button>
                    <Button variant="outline" @click="exportData('json')" class="justify-start gap-2"> <Code class="w-4 h-4" /> JSON </Button>
                    <Button variant="outline" @click="exportData('html')" class="justify-start gap-2"> <Table class="w-4 h-4" /> HTML Table </Button>
                </div>
            </div>

            <DialogFooter>
                <Button variant="outline" @click="showExportDialog = false">Cancel</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- Find & Replace Dialog -->
    <Dialog :open="showFindReplace" @update:open="showFindReplace = $event">
        <DialogContent class="sm:max-w-[450px]">
            <DialogHeader>
                <DialogTitle>Find & Replace</DialogTitle>
            </DialogHeader>

            <div class="find-replace-content py-4">
                <div class="flex flex-col gap-4">
                    <div class="grid gap-2">
                        <label for="findText" class="text-sm font-medium">Find:</label>
                        <Input id="findText" v-model="findText" class="w-full" />
                    </div>
                    <div class="grid gap-2">
                        <label for="replaceText" class="text-sm font-medium">Replace with:</label>
                        <Input id="replaceText" v-model="replaceText" class="w-full" />
                    </div>
                    <div class="flex flex-wrap gap-2 mt-2">
                        <Button @click="findNext" class="gap-2 flex-1"><Search class="w-4 h-4" /> Find Next</Button>
                        <Button variant="secondary" @click="replaceOne" class="gap-2 flex-1"><Pencil class="w-4 h-4" /> Replace</Button>
                        <Button variant="destructive" @click="replaceAll" class="gap-2 flex-1"><RefreshCcw class="w-4 h-4" /> Replace All</Button>
                    </div>
                </div>
            </div>
        </DialogContent>
    </Dialog>

    <!-- Sort Dialog -->
    <Dialog :open="showSortDialog" @update:open="showSortDialog = $event">
        <DialogContent class="sm:max-w-[400px]">
            <DialogHeader>
                <DialogTitle>Sort Data</DialogTitle>
            </DialogHeader>

            <div class="sort-content py-4">
                <div class="flex flex-col gap-4">
                    <div class="grid gap-2">
                        <label for="sortColumn" class="text-sm font-medium">Sort by Column:</label>
                        <Select v-model="sortColumn">
                            <SelectTrigger>
                                <SelectValue placeholder="Select column" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem v-for="col in columnHeaders" :key="col" :value="col">{{ col }}</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <div class="grid gap-2">
                        <label class="text-sm font-medium">Order:</label>
                        <div class="flex gap-2">
                            <Button :variant="sortOrder === 'asc' ? 'default' : 'outline'" @click="sortData('asc')" class="gap-2 flex-1">
                                <SortAsc class="w-4 h-4" />
                                Ascending
                            </Button>
                            <Button :variant="sortOrder === 'desc' ? 'default' : 'outline'" @click="sortData('desc')" class="gap-2 flex-1">
                                <SortDesc class="w-4 h-4" />
                                Descending
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </DialogContent>
    </Dialog>

    <!-- Filter Dialog -->
    <Dialog :open="showFilterDialog" @update:open="showFilterDialog = $event">
        <DialogContent class="sm:max-w-[400px]">
            <DialogHeader>
                <DialogTitle>Filter Data</DialogTitle>
            </DialogHeader>

            <div class="filter-content py-4">
                <p class="mb-3 text-sm">Filter functionality coming soon...</p>
                <p class="text-sm text-muted-foreground">This feature will allow you to filter rows based on column values.</p>
            </div>
            <DialogFooter>
                <Button @click="showFilterDialog = false">Close</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>
</template>

<style scoped>
/* Excel Container */
.excel-container {
    height: calc(100vh - 150px);
    width: 100%;
    padding: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    overflow: hidden;
}

/* Toolbar */
/* Using utility classes for styling now */

/* Reference Bar */
.excel-reference-bar {
    display: flex;
    align-items: center;
    padding: 0.5rem 0.75rem;
    background: var(--card);
    border: 1px solid var(--border);
    border-radius: 6px;
    min-height: 40px;
}

.reference-bar-cell-ref {
    display: flex;
    align-items: center;
    font-weight: 600;
    color: var(--foreground);
    font-size: 0.9rem;
}

.cell-ref-text {
    color: var(--primary);
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
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    background: var(--secondary);
}

.excel-column-header {
    min-width: 100px;
    width: 100px;
    height: 30px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    background: var(--muted);
    color: var(--foreground);
    font-size: 0.85rem;
    user-select: none;
    cursor: pointer;
}

.excel-column-header:hover {
    background: var(--secondary);
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
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    background: var(--muted);
    color: var(--muted-foreground);
    user-select: none;
    cursor: pointer;
}

.excel-row-header:hover {
    background: var(--secondary);
}

.excel-cell {
    min-width: 100px;
    width: 100px;
    height: 28px;
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    padding: 0;
    cursor: cell;
    position: relative;
    background: transparent;
    display: flex;
    align-items: center;
}

.excel-cell:hover {
    background: var(--accent);
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
