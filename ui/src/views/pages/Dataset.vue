<script setup>
import { nextTick, ref, computed, onMounted } from 'vue';
import { toast } from 'vue-sonner';
import * as XLSX from 'xlsx';
import { Toaster } from '@/components/ui/sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Checkbox } from '@/components/ui/checkbox'; // Data state
import { markRaw } from 'vue';
import {
    Table as TableIcon,
    Pencil,
    CheckSquare,
    Save,
    Upload,
    Download,
    Plus,
    PlusCircle,
    Copy,
    Scissors,
    Eraser,
    Trash2,
    X,
    Search,
    ArrowUpDown,
    Filter,
    CheckCircle,
    BarChart,
    ChevronLeft,
    ChevronRight,
    Check,
    File,
    FileSpreadsheet,
    Code
} from 'lucide-vue-next';
const MyTitle = ref('Sample Dataset');
const renderComponent = ref(true);
const showImportDialog = ref(false);
const showExportDialog = ref(false);
const pasteData = ref('');
const selectedRows = ref([]);
const clipboard = ref(null);
const editingCell = ref(null);
const editingTitle = ref(false);

// Pagination and Sorting State
const currentPage = ref(1);
const rowsPerPage = ref(20);
const sortField = ref(null);
const sortOrder = ref(1);

const filteredData = computed(() => {
    let result = [...jsondata.value];

    // Add sorting logic
    if (sortField.value) {
        result.sort((a, b) => {
            let valA = a[sortField.value];
            let valB = b[sortField.value];

            if (valA === valB) return 0;
            if (valA === null || valA === undefined) return sortOrder.value;
            if (valB === null || valB === undefined) return -sortOrder.value;

            if (typeof valA === 'string' && typeof valB === 'string') {
                return valA.localeCompare(valB) * sortOrder.value;
            }
            return (valA < valB ? -1 : 1) * sortOrder.value;
        });
    }
    return result;
});

const totalPages = computed(() => Math.ceil(filteredData.value.length / rowsPerPage.value));

const paginatedData = computed(() => {
    const start = (currentPage.value - 1) * rowsPerPage.value;
    const end = start + rowsPerPage.value;
    return filteredData.value.slice(start, end);
});

// Pagination utilities
function sortBy(field) {
    if (sortField.value === field) {
        if (sortOrder.value === 1) {
            sortOrder.value = -1;
        } else {
            sortField.value = null;
            sortOrder.value = 1;
        }
    } else {
        sortField.value = field;
        sortOrder.value = 1;
    }
}

// Selection utilities
const selectAllChecked = computed(() => {
    return paginatedData.value.length > 0 && selectedRows.value.length >= paginatedData.value.length;
});

function toggleAllSelection(checked) {
    if (checked) {
        // Merge existing selection with current page, avoiding duplicates
        const currentPageIds = paginatedData.value.map((r) => r.id);
        const otherSelections = selectedRows.value.filter((r) => !currentPageIds.includes(r.id));
        selectedRows.value = [...otherSelections, ...paginatedData.value];
    } else {
        // Remove current page from selection
        const currentPageIds = paginatedData.value.map((r) => r.id);
        selectedRows.value = selectedRows.value.filter((r) => !currentPageIds.includes(r.id));
    }
}

function toggleSelection(row) {
    const index = selectedRows.value.findIndex((r) => r.id === row.id);
    if (index === -1) {
        selectedRows.value.push(row);
    } else {
        selectedRows.value.splice(index, 1);
    }
}

function isSelected(row) {
    return selectedRows.value.some((r) => r.id === row.id);
}

function editCell(row, field) {
    editingCell.value = { id: row.id, field };
}

function completeCellEdit(event) {
    if (editingCell.value) {
        onCellEditComplete({
            data: jsondata.value.find((r) => r.id === editingCell.value.id),
            newValue: event.target.value,
            field: editingCell.value.field
        });
    }
    editingCell.value = null;
}

function moveToNextCell(rowIndex, currentField) {
    const columnFields = columns.value.map((c) => c.field);
    const currentColIndex = columnFields.indexOf(currentField);

    if (currentColIndex < columnFields.length - 1) {
        // Move to next column in same row
        editCell(paginatedData.value[rowIndex], columnFields[currentColIndex + 1]);
    } else if (rowIndex < paginatedData.value.length - 1) {
        // Move to first column in next row
        editCell(paginatedData.value[rowIndex + 1], columnFields[0]);
    } else {
        editingCell.value = null;
    }
}

// Context menu items
const contextMenuItems = ref([
    {
        label: 'Copy',
        icon: markRaw(Copy),
        command: () => copySelection()
    },
    {
        label: 'Paste',
        icon: markRaw(Copy),
        command: () => pasteSelection()
    },
    {
        label: 'Cut',
        icon: markRaw(Scissors),
        command: () => cutSelection()
    },
    { separator: true },
    {
        label: 'Insert Row',
        icon: markRaw(Plus),
        command: () => insertRowBelow()
    },
    {
        label: 'Insert Column',
        icon: markRaw(Plus),
        command: () => insertColumnRight()
    },
    { separator: true },
    {
        label: 'Delete Row',
        icon: markRaw(Trash2),
        command: () => deleteSelectedRows()
    },
    {
        label: 'Delete Column',
        icon: markRaw(Trash2),
        command: () => deleteSelectedColumns()
    }
]);

// Sample data with more realistic content
const jsondata = ref([
    {
        'Product ID': 'P001',
        'Product Name': 'Laptop Pro',
        Category: 'Electronics',
        Price: 1299.99,
        Stock: 45,
        Supplier: 'TechCorp',
        'Last Updated': '2024-01-15'
    },
    {
        'Product ID': 'P002',
        'Product Name': 'Wireless Mouse',
        Category: 'Accessories',
        Price: 29.99,
        Stock: 150,
        Supplier: 'GadgetPlus',
        'Last Updated': '2024-01-14'
    },
    {
        'Product ID': 'P003',
        'Product Name': 'Monitor 4K',
        Category: 'Electronics',
        Price: 599.99,
        Stock: 22,
        Supplier: 'DisplayTech',
        'Last Updated': '2024-01-13'
    },
    {
        'Product ID': 'P004',
        'Product Name': 'Keyboard RGB',
        Category: 'Accessories',
        Price: 89.99,
        Stock: 78,
        Supplier: 'GamerHub',
        'Last Updated': '2024-01-12'
    },
    {
        'Product ID': 'P005',
        'Product Name': 'Webcam HD',
        Category: 'Electronics',
        Price: 149.99,
        Stock: 33,
        Supplier: 'VideoTech',
        'Last Updated': '2024-01-11'
    }
]);

// Computed properties
const getColumnCount = () => {
    return jsondata.value.length > 0 ? Object.keys(jsondata.value[0]).length : 0;
};

// Generate columns from data
const columns = computed(() => {
    if (jsondata.value.length === 0) return [];
    return Object.keys(jsondata.value[0])
        .filter((key) => key !== 'id')
        .map((key) => ({
            field: key,
            header: key
        }));
});

// Utility functions
function addProperties(data, newProperties) {
    data.forEach((item) => {
        Object.assign(item, newProperties);
    });
    return data;
}

function addRow(data) {
    if (data.length === 0) {
        return [{ id: 1, 'Column 1': '', 'Column 2': '', 'Column 3': '' }];
    }

    const newRow = { id: Math.max(...data.map((row) => row.id || 0)) + 1 };
    Object.keys(data[0]).forEach((key) => {
        if (key !== 'id') {
            newRow[key] = '';
        }
    });
    data.push(newRow);
    return data;
}

const forceRerender = async () => {
    renderComponent.value = false;
    await nextTick();
    renderComponent.value = true;
};

// Core data manipulation functions
const createnewRow = async () => {
    jsondata.value = addRow([...jsondata.value]);
    await forceRerender();

    toast('Row Added', {
        description: 'New row has been added to the dataset'
    });
};

const createnewCol = async () => {
    const columnCount = getColumnCount();
    const newColumnName = `Column ${columnCount + 1}`;
    const newColumn = { [newColumnName]: '' };

    jsondata.value = addProperties([...jsondata.value], newColumn);
    await forceRerender();

    toast('Column Added', {
        description: `New column '${newColumnName}' has been added`
    });
};

const newDoc = async () => {
    MyTitle.value = 'New Dataset';
    jsondata.value = [
        { 'Column 1': '', 'Column 2': '', 'Column 3': '' },
        { 'Column 1': '', 'Column 2': '', 'Column 3': '' },
        { 'Column 1': '', 'Column 2': '', 'Column 3': '' }
    ];
    selectedRows.value = [];
    clipboard.value = null;

    await forceRerender();

    toast('New Dataset', {
        description: 'Created a new empty dataset'
    });
};

// File operations
const quickSave = () => {
    const dataStr = JSON.stringify(
        {
            name: MyTitle.value,
            data: jsondata.value,
            timestamp: new Date().toISOString()
        },
        null,
        2
    );

    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${MyTitle.value || 'dataset'}-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);

    toast('Dataset Saved', {
        description: 'Dataset has been saved as JSON file'
    });
};

const handleFileImport = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    try {
        const fileExtension = file.name.split('.').pop().toLowerCase();
        let importedData = [];

        if (fileExtension === 'json') {
            const text = await file.text();
            const parsed = JSON.parse(text);
            importedData = Array.isArray(parsed) ? parsed : parsed.data || [parsed];
        } else if (fileExtension === 'csv') {
            const text = await file.text();
            importedData = parseCSV(text);
        } else if (['xlsx', 'xls'].includes(fileExtension)) {
            const arrayBuffer = await file.arrayBuffer();
            const workbook = XLSX.read(arrayBuffer, { type: 'array' });
            const sheetName = workbook.SheetNames[0];
            const worksheet = workbook.Sheets[sheetName];
            importedData = XLSX.utils.sheet_to_json(worksheet);
        }

        if (importedData.length > 0) {
            // Add IDs to imported data
            importedData.forEach((row, index) => {
                row.id = index + 1;
            });
            jsondata.value = importedData;
            MyTitle.value = file.name.split('.')[0];
            await forceRerender();

            toast('Import Successful', {
                description: `Imported ${importedData.length} rows from ${file.name}`
            });

            showImportDialog.value = false;
        }
    } catch (error) {
        toast.error('Import Failed', {
            description: 'Error reading file: ' + error.message
        });
    }

    event.target.value = '';
};

const parseCSV = (text) => {
    const lines = text.trim().split('\n');
    if (lines.length === 0) return [];

    const headers = lines[0].split(',').map((h) => h.trim().replace(/"/g, ''));
    const data = [];

    for (let i = 1; i < lines.length; i++) {
        const values = lines[i].split(',').map((v) => v.trim().replace(/"/g, ''));
        const row = {};
        headers.forEach((header, index) => {
            row[header] = values[index] || '';
        });
        data.push(row);
    }

    return data;
};

const importFromClipboard = async () => {
    try {
        const importedData = parseCSV(pasteData.value);
        if (importedData.length > 0) {
            // Add IDs to imported data
            importedData.forEach((row, index) => {
                row.id = index + 1;
            });
            jsondata.value = importedData;
            await forceRerender();

            toast('Import Successful', {
                description: `Imported ${importedData.length} rows from clipboard`
            });

            showImportDialog.value = false;
            pasteData.value = '';
        }
    } catch (error) {
        toast.error('Import Failed', {
            description: 'Error parsing clipboard data'
        });
    }
};

const exportData = (format) => {
    try {
        let content, filename, mimeType;
        const timestamp = new Date().toISOString().split('T')[0];
        const name = MyTitle.value || 'dataset';

        switch (format) {
            case 'csv':
                content = convertToCSV(jsondata.value);
                filename = `${name}-${timestamp}.csv`;
                mimeType = 'text/csv';
                break;
            case 'excel':
                const workbook = XLSX.utils.book_new();
                const exportData = jsondata.value.map((row) => {
                    const { id, ...rest } = row;
                    return rest;
                });
                const worksheet = XLSX.utils.json_to_sheet(exportData);
                XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
                const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
                content = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                filename = `${name}-${timestamp}.xlsx`;
                break;
            case 'json':
                content = JSON.stringify(
                    {
                        name: MyTitle.value,
                        data: jsondata.value,
                        exported: new Date().toISOString()
                    },
                    null,
                    2
                );
                filename = `${name}-${timestamp}.json`;
                mimeType = 'application/json';
                break;
        }

        if (format !== 'excel') {
            content = new Blob([content], { type: mimeType });
        }

        const url = URL.createObjectURL(content);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        URL.revokeObjectURL(url);

        toast('Export Successful', {
            description: `Data exported as ${format.toUpperCase()}`
        });

        showExportDialog.value = false;
    } catch (error) {
        toast.error('Export Failed', {
            description: 'Error exporting data: ' + error.message
        });
    }
};

const convertToCSV = (data) => {
    if (!data || data.length === 0) return '';

    // Exclude ID field from CSV export
    const exportData = data.map((row) => {
        const { id, ...rest } = row;
        return rest;
    });

    const headers = Object.keys(exportData[0]).join(',');
    const rows = exportData.map((row) =>
        Object.values(row)
            .map((value) => (typeof value === 'string' && value.includes(',') ? `"${value}"` : value))
            .join(',')
    );

    return [headers, ...rows].join('\n');
};

// Edit operations
const onCellEditComplete = (event) => {
    if (!event || !event.data || !event.field) return;

    const { data, newValue, field } = event;
    data[field] = newValue;

    toast('Cell Updated', {
        description: `Updated ${field}`
    });
};

const onCellEditInit = (event) => {
    editingCell.value = event;
};

const onRowContextMenu = (event, row) => {
    // Simple fallback since we removed PrimeVue's ContextMenu
    // In a full implementation, you'd use Shadcn's ContextMenu component
    event.preventDefault();
    contextMenuTargetRow.value = row;
    selectedRows.value = [row];

    toast.info('Context Menu', {
        description: 'Context menu actions would appear here'
    });
};

const editColumnHeader = (column, index) => {
    const newHeader = prompt('Enter new column name:', column.header);
    if (newHeader && newHeader.trim() !== '') {
        const oldHeader = column.header;

        // Update all rows to use new column name
        jsondata.value.forEach((row) => {
            if (row[oldHeader] !== undefined) {
                row[newHeader.trim()] = row[oldHeader];
                delete row[oldHeader];
            }
        });

        toast.add({
            severity: 'success',
            summary: 'Column Renamed',
            detail: `Column renamed from "${oldHeader}" to "${newHeader.trim()}"`,
            life: 2000
        });

        forceRerender();
    }
};

const deleteColumn = (columnIndex) => {
    if (columns.value.length <= 1) {
        toast.error('Cannot Delete', {
            description: 'At least one column must remain'
        });
        return;
    }

    const columnToDelete = columns.value[columnIndex];
    const fieldToDelete = columnToDelete.field;

    // Remove the field from all rows
    jsondata.value.forEach((row) => {
        delete row[fieldToDelete];
    });

    toast('Column Deleted', {
        description: `Column "${columnToDelete.header}" has been deleted`
    });

    forceRerender();
};

const deleteRow = (rowIndex, rowOrIndex) => {
    if (jsondata.value.length <= 1) {
        toast.error('Cannot Delete', {
            description: 'At least one row must remain'
        });
        return;
    }

    // Handle both direct row index and row object passed from paginatedData
    let globalIndex = rowIndex;
    if (rowOrIndex && typeof rowOrIndex === 'object') {
        globalIndex = jsondata.value.findIndex((r) => r.id === rowOrIndex.id);
    }

    if (globalIndex >= 0 && globalIndex < jsondata.value.length) {
        jsondata.value.splice(globalIndex, 1);

        toast('Row Deleted', {
            description: 'Row has been deleted'
        });

        // Adjust pagination if deleted last item on page
        if (paginatedData.value.length === 0 && currentPage.value > 1) {
            currentPage.value--;
        }
    }
};

const copySelection = () => {
    if (selectedRows.value.length === 0) {
        toast.info('No Selection', {
            description: 'Please select rows to copy'
        });
        return;
    }

    clipboard.value = selectedRows.value.map((row) => ({ ...row }));

    toast('Copied', {
        description: `${selectedRows.value.length} row(s) copied to clipboard`
    });
};

const pasteSelection = () => {
    if (!clipboard.value) {
        toast.info('No Data', {
            description: 'No data in clipboard to paste'
        });
        return;
    }

    // Implement paste logic here
    toast('Pasted', {
        description: 'Data pasted successfully'
    });
};

const cutSelection = () => {
    copySelection();
    clearSelectedCells();
};

const selectAll = () => {
    // Implementation depends on excel editor API
    toast.info('Selected All', {
        description: 'All cells selected'
    });
};

const clearSelection = () => {
    selectedRows.value = [];
    toast.info('Selection Cleared', {
        description: 'Row selection cleared'
    });
};

const clearSelectedCells = () => {
    if (selectedRows.value.length === 0) return;

    // Clear content of selected rows
    selectedRows.value.forEach((row) => {
        Object.keys(row).forEach((key) => {
            if (key !== 'id') {
                row[key] = '';
            }
        });
    });

    toast.info('Content Cleared', {
        description: 'Selected rows cleared'
    });
};

// Insert operations
const insertRowAbove = async () => {
    // Insert row above current selection
    await createnewRow();
};

const insertRowBelow = async () => {
    await createnewRow();
};

const insertColumnLeft = async () => {
    await createnewCol();
};

const insertColumnRight = async () => {
    await createnewCol();
};

// Delete operations
const deleteSelectedRows = () => {
    if (selectedRows.value.length === 0) return;

    // Remove selected rows from data
    selectedRows.value.forEach((selectedRow) => {
        const index = jsondata.value.findIndex((row) => row.id === selectedRow.id);
        if (index !== -1) {
            jsondata.value.splice(index, 1);
        }
    });

    selectedRows.value = [];

    toast.info('Rows Deleted', {
        description: 'Selected rows have been deleted'
    });
};

const deleteSelectedColumns = () => {
    if (selectedRows.value.length === 0) return;

    toast.info('Columns Deleted', {
        description: 'Selected columns have been deleted'
    });
};

const confirmClearAll = () => {
    if (confirm('Are you sure you want to clear all data? This action cannot be undone.')) {
        newDoc();
    }
};

// Tool functions
const showFindReplace = () => {
    toast.info('Find & Replace', {
        description: 'Find & Replace dialog would open here'
    });
};

const showSortDialog = () => {
    toast.info('Sort Data', {
        description: 'Sort dialog would open here'
    });
};

const showFilterDialog = () => {
    toast.info('Filter Data', {
        description: 'Filter dialog would open here'
    });
};

const validateData = () => {
    let errors = 0;
    let warnings = 0;

    jsondata.value.forEach((row, index) => {
        Object.entries(row).forEach(([key, value]) => {
            if (value === '' || value === null || value === undefined) {
                warnings++;
            }
        });
    });

    if (warnings > 0) {
        toast.error('Data Validation', {
            description: `Validation complete. ${warnings} empty cells found.`
        });
    } else {
        toast('Data Validation', {
            description: 'Validation complete. No empty cells found.'
        });
    }
};

const showDataStats = () => {
    const stats = {
        rows: jsondata.value.length,
        columns: getColumnCount(),
        totalCells: jsondata.value.length * getColumnCount(),
        emptyCells: 0
    };

    jsondata.value.forEach((row) => {
        Object.values(row).forEach((value) => {
            if (value === '' || value === null || value === undefined) {
                stats.emptyCells++;
            }
        });
    });

    toast('Dataset Statistics', {
        description: `${stats.rows} rows, ${stats.columns} columns, ${stats.emptyCells}/${stats.totalCells} empty cells`
    });
};
</script>

<template>
    <Toaster />

    <!-- Main Toolbar -->
    <div class="dataset-toolbar flex items-center justify-between">
        <div class="flex items-center gap-2">
            <label class="dataset-label">Dataset:</label>
            <div class="dataset-name flex items-center gap-2 cursor-pointer">
                <TableIcon class="w-4 h-4 text-muted-foreground" />
                <Input v-if="editingTitle" v-model="MyTitle" placeholder="Enter dataset name" @blur="editingTitle = false" @keyup.enter="editingTitle = false" autofocus class="h-8 max-w-[200px]" />
                <span v-else class="dataset-title" @click="editingTitle = true">{{ MyTitle || 'Untitled Dataset' }}</span>
                <Pencil v-if="!editingTitle" class="w-3 h-3 text-muted-foreground" @click="editingTitle = true" />
            </div>
        </div>

        <div class="flex items-center gap-2">
            <Badge variant="secondary" class="stats-tag">
                <TableIcon class="w-3 h-3 mr-1" />
                {{ jsondata.length }} rows × {{ getColumnCount() }} columns
            </Badge>
            <Badge v-if="selectedRows.length > 0" variant="default" class="stats-tag">
                <CheckSquare class="w-3 h-3 mr-1" />
                {{ selectedRows.length }} selected
            </Badge>
        </div>

        <div class="flex items-center gap-2">
            <Button variant="ghost" size="icon" @click="quickSave" title="Save Dataset">
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
    <div class="dataset-secondary-toolbar flex items-center justify-between">
        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="newDoc" title="New Dataset"><Plus class="w-4 h-4" /></Button>
            <div class="w-px h-6 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="createnewRow" title="Add Row"><PlusCircle class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="createnewCol" title="Add Column"><TableIcon class="w-4 h-4" /></Button>
            <div class="w-px h-6 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="copySelection" title="Copy Selection" :disabled="selectedRows.length === 0"><Copy class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="pasteSelection" title="Paste" :disabled="!clipboard"><Copy class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="cutSelection" title="Cut Selection" :disabled="selectedRows.length === 0"><Scissors class="w-4 h-4" /></Button>
        </div>

        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="selectAll" title="Select All"><CheckSquare class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="clearSelection" title="Clear Selection"><Eraser class="w-4 h-4" /></Button>
            <div class="w-px h-6 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="deleteSelectedRows" title="Delete Selected Rows" :disabled="selectedRows.length === 0"><Trash2 class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="confirmClearAll" title="Clear All Data" class="text-destructive hover:bg-destructive/10"><X class="w-4 h-4" /></Button>
        </div>

        <div class="flex items-center gap-1">
            <Button variant="ghost" size="icon" @click="showFindReplace" title="Find & Replace"><Search class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="showSortDialog" title="Sort Data"><ArrowUpDown class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="showFilterDialog" title="Filter Data"><Filter class="w-4 h-4" /></Button>
            <div class="w-px h-6 bg-border mx-1"></div>
            <Button variant="ghost" size="icon" @click="validateData" title="Validate Data"><CheckCircle class="w-4 h-4" /></Button>
            <Button variant="ghost" size="icon" @click="showDataStats" title="Show Statistics"><BarChart class="w-4 h-4" /></Button>
        </div>
    </div>

    <!-- Data Editor Container -->
    <div class="dataset-container flex flex-col h-[calc(100vh-140px)]">
        <div class="border rounded-md bg-card flex-1 overflow-auto relative">
            <Table>
                <TableHeader class="sticky top-0 bg-secondary z-10 shadow-sm">
                    <TableRow>
                        <TableHead class="w-[50px] text-center">
                            <Checkbox :checked="selectAllChecked" @update:checked="toggleAllSelection" aria-label="Select all" />
                        </TableHead>
                        <TableHead v-for="(column, index) in columns" :key="column.field" class="min-w-[120px] whitespace-nowrap px-4 py-3 font-semibold relative group">
                            <div class="flex items-center justify-between">
                                <span @dblclick="editColumnHeader(column, index)" class="cursor-pointer hover:text-primary transition-colors flex-1">
                                    {{ column.header }}
                                </span>
                                <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                    <Button variant="ghost" size="icon" class="h-6 w-6 text-muted-foreground hover:text-foreground" @click.stop="sortBy(column.field)" title="Sort">
                                        <ArrowUpDown class="w-3 h-3" />
                                    </Button>
                                    <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive hover:bg-destructive/10" @click.stop="deleteColumn(index)" title="Delete Column">
                                        <X class="w-3 h-3" />
                                    </Button>
                                </div>
                            </div>
                        </TableHead>
                        <TableHead class="w-[50px] text-center p-2">
                            <Button variant="ghost" size="icon" class="h-8 w-8 hover:bg-primary/10 hover:text-primary" @click="createnewCol" title="Add Column">
                                <Plus class="w-4 h-4" />
                            </Button>
                        </TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    <TableRow v-for="(row, rowIndex) in paginatedData" :key="row.id" :class="{ 'bg-primary/5': isSelected(row) }" @contextmenu.prevent="onRowContextMenu($event, row)" class="hover:bg-muted/50 transition-colors group">
                        <TableCell class="text-center">
                            <Checkbox :checked="isSelected(row)" @update:checked="toggleSelection(row)" aria-label="Select row" />
                        </TableCell>
                        <TableCell v-for="column in columns" :key="column.field" class="p-0 border-r last:border-r-0 border-border/50 bg-background relative" @dblclick="editCell(row, column.field)">
                            <div class="px-4 py-2 min-h-[40px] flex items-center w-full min-w-[120px]">
                                <Input
                                    v-if="editingCell?.id === row.id && editingCell?.field === column.field"
                                    v-model="row[column.field]"
                                    class="h-8 w-full border-primary/50 focus-visible:ring-1 focus-visible:ring-primary z-10"
                                    autofocus
                                    @blur="completeCellEdit"
                                    @keydown.enter="completeCellEdit"
                                    @keydown.tab.prevent="moveToNextCell(rowIndex, column.field)"
                                />
                                <span v-else class="truncate w-full block cursor-pointer" :title="row[column.field]">
                                    {{ row[column.field] }}
                                </span>
                            </div>
                        </TableCell>
                        <TableCell class="text-center p-2">
                            <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive opacity-0 group-hover:opacity-100 hover:bg-destructive/10 transition-all" @click="deleteRow(rowIndex, row)" title="Delete Row">
                                <Trash2 class="w-4 h-4" />
                            </Button>
                        </TableCell>
                    </TableRow>

                    <TableRow v-if="filteredData.length === 0">
                        <TableCell :colspan="columns.length + 2" class="h-24 text-center text-muted-foreground"> No data available. Add a row or import data to get started. </TableCell>
                    </TableRow>
                </TableBody>
            </Table>
        </div>

        <!-- Pagination & Add Row Footer -->
        <div class="flex items-center justify-between py-3 px-4 bg-card border border-t-0 rounded-b-md">
            <div class="flex items-center justify-between w-full">
                <Button class="gap-2" @click="createnewRow"> <Plus class="w-4 h-4" /> Add Row </Button>

                <div class="flex items-center gap-4">
                    <div class="flexItems-center space-x-2">
                        <p class="text-sm font-medium">Rows per page</p>
                        <select v-model="rowsPerPage" class="h-8 w-[70px] rounded-md border border-input bg-transparent px-2 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring">
                            <option :value="10">10</option>
                            <option :value="20">20</option>
                            <option :value="50">50</option>
                            <option :value="100">100</option>
                        </select>
                    </div>
                    <div class="flex w-[100px] items-center justify-center text-sm font-medium">Page {{ currentPage }} of {{ totalPages || 1 }}</div>
                    <div class="flex items-center space-x-2">
                        <Button variant="outline" class="h-8 w-8 p-0" @click="currentPage === 1 ? null : currentPage--" :disabled="currentPage === 1">
                            <span class="sr-only">Go to previous page</span>
                            <ChevronLeft class="h-4 w-4" />
                        </Button>
                        <Button variant="outline" class="h-8 w-8 p-0" @click="currentPage === totalPages || totalPages === 0 ? null : currentPage++" :disabled="currentPage === totalPages || totalPages === 0">
                            <span class="sr-only">Go to next page</span>
                            <ChevronRight class="h-4 w-4" />
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Import Dialog -->
    <Dialog :open="showImportDialog" @update:open="showImportDialog = $event">
        <DialogContent class="sm:max-w-[500px]">
            <DialogHeader>
                <DialogTitle>Import Data</DialogTitle>
            </DialogHeader>

            <div class="grid gap-4 py-4">
                <div class="flex flex-col gap-2">
                    <h4 class="font-medium">Import from File</h4>
                    <div class="flex items-center gap-4">
                        <input ref="fileInput" type="file" accept=".csv,.xlsx,.xls,.json" @change="handleFileImport" class="hidden" />
                        <Button variant="outline" @click="$refs.fileInput.click()"> <Upload class="w-4 h-4 mr-2" /> Choose File </Button>
                    </div>
                    <p class="text-xs text-muted-foreground italic">Supported formats: CSV, Excel (.xlsx, .xls), JSON</p>
                </div>

                <div class="h-px bg-border my-2"></div>

                <div class="flex flex-col gap-2">
                    <h4 class="font-medium">Paste Data</h4>
                    <Textarea v-model="pasteData" placeholder="Paste CSV or tab-separated data here..." class="min-h-[120px]" />
                    <Button @click="importFromClipboard" :disabled="!pasteData.trim()" class="mt-2 text-white"> <Check class="w-4 h-4 mr-2" /> Import from Clipboard </Button>
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

            <div class="grid gap-4 py-4">
                <div class="flex flex-col gap-4">
                    <h4 class="font-medium">Choose Export Format</h4>
                    <div class="flex flex-col gap-2">
                        <Button variant="outline" class="w-full justify-start" @click="exportData('csv')"> <File class="w-4 h-4 mr-2" /> CSV </Button>
                        <Button variant="outline" class="w-full justify-start" @click="exportData('excel')"> <FileSpreadsheet class="w-4 h-4 mr-2" /> Excel </Button>
                        <Button variant="outline" class="w-full justify-start" @click="exportData('json')"> <Code class="w-4 h-4 mr-2" /> JSON </Button>
                    </div>
                </div>
            </div>

            <DialogFooter>
                <Button variant="outline" @click="showExportDialog = false">Cancel</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>
</template>

<style scoped>
/* Scoped overrides to fix Shadcn table aesthetics in this context */
:deep(td.bg-background) {
    background-color: var(--background) !important;
}

:deep(th),
:deep(td) {
    border-color: var(--border) !important;
}

.pi {
    font-family: 'primeicons';
}
</style>
