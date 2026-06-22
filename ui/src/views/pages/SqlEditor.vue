<script setup>
import { reactive, shallowRef, ref, computed, onMounted } from 'vue';
import { redo, undo } from '@codemirror/commands';
import { search } from '@codemirror/search';
import { Codemirror } from 'vue-codemirror';
import { sql } from '@codemirror/lang-sql';
import { toast } from 'vue-sonner';
import { useDatabaseStore } from '@/store/databaseStore';
import { userStoreMe } from '@/store/userStore';
import { getCurrentInstance } from 'vue';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import BaseChart from '@/components/BaseChart.vue';
import { Database, Loader2, Play, Save, File, Pencil, FolderOpen, Plus, Undo, RefreshCw, Copy, Search, Download, ChevronLeft, ChevronRight, Info, Trash2, Grid, BarChart } from 'lucide-vue-next';

// Stores and services
const databaseStore = useDatabaseStore();
const userStore = userStoreMe();
var { proxy } = getCurrentInstance();

// Editor state
const code = ref('SELECT 1;');
const cmView = shallowRef();
const editorRef = ref();
const config = reactive({
    disabled: false,
    indentWithTab: true,
    tabSize: 2,
    autofocus: true,
    height: '300px'
});

// Database and script state
const selectedDatabase = ref(null);
const isExecuting = ref(false);
const hasExecuted = ref(false);
const queryResults = ref([]);
const queryColumns = ref([]);

// View mode and Chart config
const currentViewMode = ref('table');
const chartType = ref('bar');
const chartData = computed(() => {
    if (queryResults.value.length === 0 || queryColumns.value.length === 0) return null;

    let labelCol = null;
    const dataCols = [];

    const firstRow = queryResults.value[0];
    for (const col of queryColumns.value) {
        const val = firstRow[col.field];
        if (typeof val === 'string' && !labelCol) {
            labelCol = col.field;
        } else if (typeof val === 'number') {
            dataCols.push(col.field);
        } else if (!labelCol) {
            labelCol = col.field;
        }
    }

    if (!labelCol && dataCols.length > 0) labelCol = dataCols.shift();
    if (dataCols.length === 0 && queryColumns.value.length > 0) {
        dataCols.push(queryColumns.value[0].field);
    }

    const labels = queryResults.value.map(row => String(row[labelCol] || ''));
    
    const colors = [
        { border: 'rgba(75, 192, 192, 1)', bg: 'rgba(75, 192, 192, 0.6)' },
        { border: 'rgba(255, 99, 132, 1)', bg: 'rgba(255, 99, 132, 0.6)' },
        { border: 'rgba(54, 162, 235, 1)', bg: 'rgba(54, 162, 235, 0.6)' },
        { border: 'rgba(255, 205, 86, 1)', bg: 'rgba(255, 205, 86, 0.6)' },
    ];

    const datasets = dataCols.map((colField, i) => {
        const color = colors[i % colors.length];
        return {
            label: queryColumns.value.find(c => c.field === colField)?.header || colField,
            data: queryResults.value.map(row => Number(row[colField]) || 0),
            borderColor: color.border,
            backgroundColor: chartType.value === 'line' ? 'transparent' : color.bg,
            borderWidth: 1,
            fill: chartType.value === 'line' ? false : true
        };
    });

    return { labels, datasets };
});

// Script management
const currentScript = reactive({
    id: null,
    name: '',
    code: '',
    database: null
});

const savedScripts = ref([]);
const showLoadDialog = ref(false);
const selectedScriptToLoad = ref(null);
const editingScriptName = ref(false);

// Pagination logic for results table
const currentPage = ref(1);
const rowsPerPage = ref(20);

// Reset pagination when results change
const paginatedResults = computed(() => {
    const start = (currentPage.value - 1) * rowsPerPage.value;
    const end = start + rowsPerPage.value;
    return queryResults.value.slice(start, end);
});

const totalPages = computed(() => {
    return Math.ceil(queryResults.value.length / rowsPerPage.value);
});

// Theme-aware editor styling
const editorStyle = computed(() => {
    return {
        width: '100%',
        height: config.height,
        border: `1px solid var(--p-border-color)`,
        borderRadius: 'var(--p-border-radius)'
    };
});

// Available databases from store — show all connections so users can run queries
// without needing to test first. Error-status connections are shown dimmed.
const availableDatabases = computed(() => {
    return databaseStore.allConnections;
});

// Editor extensions with search support
const extensions = [sql(), search()];

// Editor event handlers
const handleReady = ({ view }) => {
    cmView.value = view;
};

const handleUndo = () => {
    if (!cmView.value) return;
    undo({
        state: cmView.value.state,
        dispatch: cmView.value.dispatch
    });
};

const handleRedo = () => {
    if (!cmView.value) return;
    redo({
        state: cmView.value.state,
        dispatch: cmView.value.dispatch
    });
};

const handleStateUpdate = () => {
    currentScript.code = code.value;
};

// Text operations
const copyText = async () => {
    if (!cmView.value) return;

    const selection = cmView.value.state.selection.main;
    const selectedText = selection.empty ? cmView.value.state.doc.toString() : cmView.value.state.doc.sliceString(selection.from, selection.to);

    try {
        await navigator.clipboard.writeText(selectedText);
        toast('Copied', {
            description: 'Text copied to clipboard'
        });
    } catch (error) {
        toast.error('Copy Failed', {
            description: 'Failed to copy text to clipboard'
        });
    }
};

const pasteText = async () => {
    if (!cmView.value) return;

    try {
        const text = await navigator.clipboard.readText();
        const selection = cmView.value.state.selection.main;

        cmView.value.dispatch({
            changes: {
                from: selection.from,
                to: selection.to,
                insert: text
            }
        });

        toast('Pasted', {
            description: 'Text pasted from clipboard'
        });
    } catch (error) {
        toast.error('Paste Failed', {
            description: 'Failed to paste text from clipboard'
        });
    }
};

const toggleSearch = () => {
    if (!cmView.value) return;

    // Focus the editor and trigger search
    cmView.value.focus();

    // Dispatch search command
    const searchCommand = () => {
        // This will open the search panel
        cmView.value.dispatch({
            effects: []
        });

        // Trigger Ctrl+F equivalent
        const event = new KeyboardEvent('keydown', {
            key: 'f',
            ctrlKey: true
        });
        cmView.value.dom.dispatchEvent(event);
    };

    searchCommand();
};

// Database operations
const getSelectedDatabaseName = () => {
    const db = databaseStore.connectionById(selectedDatabase.value);
    return db ? db.name : '';
};

const getSelectedDatabaseType = () => {
    const db = databaseStore.connectionById(selectedDatabase.value);
    return db ? db.type : '';
};

const getDatabaseIcon = (type) => {
    return Database;
};

// Query execution
const executeQuery = async () => {
    if (!selectedDatabase.value || !code.value.trim()) return;

    isExecuting.value = true;
    hasExecuted.value = false;
    queryResults.value = [];
    queryColumns.value = [];

    try {
        const result = await userStore.executeCommand('ExecuteSql', {
            database: selectedDatabase.value,
            code: code.value
        }, proxy.$socket);

        if (result && result.Data) {
            queryResults.value = result.Data.rows || [];
            queryColumns.value = result.Data.columns || [];
        }
        
        currentPage.value = 1; // Reset pagination
        hasExecuted.value = true;

        toast('Query Executed', {
            description: `Query executed successfully. ${queryResults.value.length} rows returned.`
        });
    } catch (error) {
        toast.error('Query Failed', {
            description: error.message || 'An error occurred while executing the query'
        });
    } finally {
        isExecuting.value = false;
    }
};

// Mock data generator for demo purposes
const generateMockData = (query) => {
    const lowerQuery = query.toLowerCase();

    if (lowerQuery.includes('select')) {
        // Generate columns based on SELECT statement
        const columns = [
            { field: 'id', header: 'ID' },
            { field: 'name', header: 'Name' },
            { field: 'sales', header: 'Sales ($)' },
            { field: 'expenses', header: 'Expenses ($)' },
            { field: 'created_at', header: 'Date' }
        ];

        // Generate mock rows
        const rows = Array.from({ length: Math.floor(Math.random() * 20) + 10 }, (_, i) => ({
            id: i + 1,
            name: `Entity ${String.fromCharCode(65 + (i % 26))}${i + 1}`,
            sales: Math.floor(Math.random() * 8000) + 2000,
            expenses: Math.floor(Math.random() * 5000) + 1000,
            created_at: new Date(Date.now() - Math.random() * 10000000000).toISOString().split('T')[0]
        }));

        return { columns, rows };
    }

    return { columns: [], rows: [] };
};

// Script management
const newScript = () => {
    currentScript.id = null;
    currentScript.name = '';
    currentScript.code = '';
    currentScript.database = null;
    code.value = '';
    queryResults.value = [];
    queryColumns.value = [];
    hasExecuted.value = false;
};

const saveScript = async () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled Script',
        code: code.value,
        database: selectedDatabase.value || '',
        createdAt: currentScript.id ? undefined : new Date(),
        updatedAt: new Date()
    };

    try {
        await userStore.executeCommand('SaveScript', { language: 'sql', script }, proxy.$socket);

        const existingIndex = savedScripts.value.findIndex((s) => s.id === script.id);
        if (existingIndex >= 0) {
            savedScripts.value[existingIndex] = { ...savedScripts.value[existingIndex], ...script };
        } else {
            savedScripts.value.push({ ...script, databaseName: getSelectedDatabaseName() });
            currentScript.id = script.id;
        }

        toast('Script Saved', {
            description: `Script "${script.name}" saved successfully`
        });
    } catch (error) {
        toast.error('Save Failed', { description: error.message });
    }
};

const loadScript = () => {
    loadScriptsFromStorage();
    showLoadDialog.value = true;
};

const confirmLoadScript = () => {
    if (!selectedScriptToLoad.value) return;

    const script = selectedScriptToLoad.value;
    currentScript.id = script.id;
    currentScript.name = script.name;
    currentScript.code = script.code;
    currentScript.database = script.database;

    code.value = script.code || '';
    selectedDatabase.value = script.database || null;

    queryResults.value = [];
    queryColumns.value = [];
    hasExecuted.value = false;

    showLoadDialog.value = false;
    selectedScriptToLoad.value = null;

    toast('Script Loaded', {
        description: `Script "${script.name}" loaded successfully`
    });
};

const deleteScript = async (scriptId) => {
    try {
        await userStore.executeCommand('DeleteScript', { language: 'sql', id: scriptId }, proxy.$socket);
        const index = savedScripts.value.findIndex((s) => s.id === scriptId);
        if (index >= 0) {
            const scriptName = savedScripts.value[index].name;
            savedScripts.value.splice(index, 1);

            toast('Script Deleted', {
                description: `Script "${scriptName}" deleted successfully`
            });
        }
    } catch (error) {
        toast.error('Delete Failed', { description: error.message });
    }
};

const loadScriptsFromStorage = async () => {
    try {
        const result = await userStore.executeCommand('LoadScripts', { language: 'sql' }, proxy.$socket);
        if (result && result.Data) {
            savedScripts.value = result.Data.map((s) => ({
                id: s.id || s.Id,
                name: s.name || s.Name || '',
                // PostgreSQL lowercases column names: Code → code, DatabaseConnectionId → databaseconnectionid
                code: s.code || s.Code || '',
                database: s.database || s.Database || s.databaseconnectionid || s.DatabaseConnectionId || '',
                databaseName: s.databaseName || s.DatabaseName || '',
                language: s.language || s.Language || 'sql',
                createdAt: new Date(s.createdAt || s.CreatedAt || s.createdat || Date.now()),
                updatedAt: new Date(s.updatedAt || s.UpdatedAt || s.updatedat || s.createdAt || s.CreatedAt || s.createdat || Date.now()),
            }));
        }
    } catch (error) {
        console.error('Failed to load scripts:', error);
        savedScripts.value = [];
    }
};

// Utility functions
const generateId = () => {
    // Must be a valid GUID — server stores script Id as uniqueidentifier/uuid
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID();
    }
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = (Math.random() * 16) | 0;
        const v = c === 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
};

const formatDate = (date) => {
    return new Date(date).toLocaleString();
};

const formatCellValue = (value) => {
    if (value === null || value === undefined) return 'NULL';
    if (typeof value === 'string' && value.length > 100) {
        return value.substring(0, 100) + '...';
    }
    return value;
};

const exportResults = () => {
    if (queryResults.value.length === 0) return;

    const csv = [
        queryColumns.value.map((col) => col.header).join(','),
        ...queryResults.value.map((row) =>
            queryColumns.value
                .map((col) => {
                    const value = row[col.field];
                    if (value === null || value === undefined) return '';
                    if (typeof value === 'string' && value.includes(',')) {
                        return `"${value.replace(/"/g, '""')}"`;
                    }
                    return value;
                })
                .join(',')
        )
    ].join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `query-results-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);

    toast('Export Complete', {
        description: 'Query results exported to CSV file'
    });
};

// Initialize component
onMounted(() => {
    databaseStore.loadConnections(proxy.$socket);
    loadScriptsFromStorage();
});
</script>

<template>
    <div class="sql-editor-container">
        <!-- Top Toolbar with Database Selection -->
        <div class="flex items-center justify-between mb-3 bg-card p-3 rounded-md border">
            <div class="flex items-center gap-3">
                <label for="database-select" class="font-semibold text-sm">Database:</label>
                <Select v-model="selectedDatabase" :disabled="isExecuting">
                    <SelectTrigger class="w-[200px]">
                        <SelectValue placeholder="Select Database">
                            <div v-if="selectedDatabase" class="flex items-center gap-2">
                                <component :is="getDatabaseIcon(getSelectedDatabaseType())" class="w-4 h-4" />
                                <span>{{ getSelectedDatabaseName() }}</span>
                            </div>
                        </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem v-for="db in availableDatabases" :key="db.id" :value="db.id">
                            <div class="flex items-center gap-2">
                                <component :is="getDatabaseIcon(db.type)" class="w-4 h-4" />
                                <div>
                                    <div class="flex items-center gap-2">
                                        <span class="font-medium">{{ db.name }}</span>
                                        <span v-if="db.status === 'connected'" class="w-2 h-2 rounded-full bg-green-500 inline-block"></span>
                                        <span v-else-if="db.status === 'error'" class="w-2 h-2 rounded-full bg-red-500 inline-block"></span>
                                    </div>
                                    <div class="text-xs text-muted-foreground">{{ db.host }}:{{ db.port }}</div>
                                </div>
                            </div>
                        </SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div class="flex items-center gap-2">
                <Button @click="executeQuery" :disabled="!selectedDatabase || !code.trim() || isExecuting" class="bg-green-600 hover:bg-green-700 text-white gap-2">
                    <Loader2 v-if="isExecuting" class="w-4 h-4 animate-spin" />
                    <Play v-else class="w-4 h-4" />
                    Execute
                </Button>
                <Button variant="secondary" @click="saveScript" :disabled="!code.trim()" class="gap-2">
                    <Save class="w-4 h-4" />
                    Save Script
                </Button>
            </div>
        </div>

        <!-- Script Management Toolbar -->
        <div class="flex items-center justify-between mb-3 bg-card p-2 rounded-md border">
            <div class="flex items-center gap-2 px-2">
                <div class="flex items-center gap-2 cursor-pointer group">
                    <File class="w-4 h-4 text-muted-foreground" />
                    <Input v-if="editingScriptName" v-model="currentScript.name" class="h-8 w-[200px]" autofocus @blur="editingScriptName = false" @keyup.enter="editingScriptName = false" />
                    <span v-else class="font-medium" @click="editingScriptName = true">
                        {{ currentScript.name || 'Untitled Script' }}
                    </span>
                    <Pencil v-if="!editingScriptName" class="w-3 h-3 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" @click="editingScriptName = true" />
                </div>
            </div>

            <div class="flex items-center gap-1">
                <Button variant="ghost" size="icon" @click="loadScript" title="Open Script"><FolderOpen class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="newScript" title="New Script"><Plus class="w-4 h-4" /></Button>
                <div class="w-px h-6 bg-border mx-2"></div>
                <Button variant="ghost" size="icon" @click="handleUndo" title="Undo"><Undo class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="handleRedo" title="Redo"><RefreshCw class="w-4 h-4" /></Button>
                <div class="w-px h-6 bg-border mx-2"></div>
                <Button variant="ghost" size="icon" @click="copyText" title="Copy"><Copy class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="pasteText" title="Paste"><Copy class="w-4 h-4" /></Button>
                <div class="w-px h-6 bg-border mx-2"></div>
                <Button variant="ghost" size="icon" @click="toggleSearch" title="Find"><Search class="w-4 h-4" /></Button>
            </div>
        </div>
        <!-- SQL Editor -->
        <div class="editor-container border rounded-md mb-3">
            <codemirror
                ref="editorRef"
                v-model="code"
                :style="editorStyle"
                placeholder="-- Enter your SQL query here..."
                :extensions="extensions"
                :autofocus="config.autofocus"
                :disabled="config.disabled || isExecuting"
                :indent-with-tab="config.indentWithTab"
                :tab-size="config.tabSize"
                @update="handleStateUpdate"
                @ready="handleReady"
            />
        </div>

        <!-- Results Grid -->
        <div class="results-container border rounded-md bg-card p-4 flex flex-col">
            <div class="flex justify-between items-center mb-4">
                <h6 class="text-sm font-semibold m-0">Query Results</h6>
                <div class="flex items-center gap-3">
                    <Tabs v-if="queryResults.length > 0" v-model="currentViewMode" class="w-[200px]">
                        <TabsList class="grid w-full grid-cols-2 h-8">
                            <TabsTrigger value="table" class="text-xs py-1"><Grid class="w-3 h-3 mr-1"/> Table</TabsTrigger>
                            <TabsTrigger value="chart" class="text-xs py-1"><BarChart class="w-3 h-3 mr-1"/> Chart</TabsTrigger>
                        </TabsList>
                    </Tabs>

                    <div v-if="currentViewMode === 'chart' && queryResults.length > 0" class="flex items-center gap-2">
                         <Select v-model="chartType">
                            <SelectTrigger class="w-[130px] h-8 text-xs">
                                <SelectValue placeholder="Chart Type" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="bar">Bar Chart</SelectItem>
                                <SelectItem value="line">Line Chart</SelectItem>
                                <SelectItem value="pie">Pie Chart</SelectItem>
                                <SelectItem value="doughnut">Doughnut Chart</SelectItem>
                            </SelectContent>
                         </Select>
                    </div>

                    <div class="flex items-center gap-2">
                        <Badge v-if="queryResults.length > 0" variant="secondary">{{ queryResults.length }} rows</Badge>
                        <Button variant="outline" size="sm" @click="exportResults" :disabled="queryResults.length === 0" class="gap-2">
                            <Download class="w-4 h-4" />
                            Export
                        </Button>
                    </div>
                </div>
            </div>

            <div v-if="queryResults.length > 0 && currentViewMode === 'table'" class="flex-1 overflow-auto border rounded-md excel-grid-wrapper bg-background">
                <div class="excel-grid-header">
                    <div class="excel-corner-cell"></div>
                    <div v-for="col in queryColumns" :key="col.field" class="excel-column-header">
                        {{ col.header }}
                    </div>
                </div>
                <div class="excel-grid-body">
                    <div v-for="(row, i) in paginatedResults" :key="i" class="excel-row">
                        <div class="excel-row-header">{{ (currentPage - 1) * rowsPerPage + i + 1 }}</div>
                        <div v-for="col in queryColumns" :key="col.field" class="excel-cell">
                            <span class="excel-cell-value px-2 truncate w-full pointer-events-none" :title="formatCellValue(row[col.field])">
                                {{ formatCellValue(row[col.field]) }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
            
            <div v-else-if="queryResults.length > 0 && currentViewMode === 'chart'" class="flex-1 overflow-auto border rounded-md bg-card p-4 flex items-center justify-center">
                <div class="w-full h-[400px]">
                    <BaseChart
                        v-if="chartData"
                        :type="chartType"
                        :data="chartData"
                        title="Query Results Chart"
                        :show-controls="false"
                        :show-data-labels="false"
                        :show-legend="true"
                        height="400px"
                    />
                    <div v-else class="text-center text-muted-foreground p-8">
                        Not enough numeric data to render a chart.
                    </div>
                </div>
            </div>

            <!-- Pagination -->
            <div v-if="queryResults.length > 0 && currentViewMode === 'table'" class="flex items-center justify-between py-3 px-2 border-t mt-auto">
                <p class="text-sm text-muted-foreground">Showing {{ (currentPage - 1) * rowsPerPage + 1 }} to {{ Math.min(currentPage * rowsPerPage, queryResults.length) }} of {{ queryResults.length }} entries</p>
                <div class="flex items-center space-x-2">
                    <p class="text-sm font-medium mr-2">Rows per page</p>
                    <select v-model="rowsPerPage" class="h-8 w-[70px] rounded-md border border-input bg-transparent px-2 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring mr-4">
                        <option :value="10">10</option>
                        <option :value="20">20</option>
                        <option :value="50">50</option>
                        <option :value="100">100</option>
                    </select>

                    <Button variant="outline" class="h-8 w-8 p-0" @click="currentPage > 1 ? currentPage-- : null" :disabled="currentPage === 1">
                        <ChevronLeft class="w-4 h-4" />
                    </Button>
                    <Button variant="outline" class="h-8 w-8 p-0" @click="currentPage < totalPages ? currentPage++ : null" :disabled="currentPage >= totalPages">
                        <ChevronRight class="w-4 h-4" />
                    </Button>
                </div>
            </div>

            <div v-else-if="hasExecuted" class="text-center p-12 my-auto">
                <Info class="w-12 h-12 text-muted-foreground mx-auto mb-3 opacity-50" />
                <div class="text-xl font-medium mb-2">No Results</div>
                <div class="text-muted-foreground">The query executed successfully but returned no data</div>
            </div>

            <div v-else class="text-center p-12 my-auto">
                <Database class="w-12 h-12 text-muted-foreground mx-auto mb-3 opacity-50" />
                <div class="text-xl font-medium mb-2">Ready to Execute</div>
                <div class="text-muted-foreground">Write your SQL query above and click Execute to see results</div>
            </div>
        </div>

        <!-- Load Script Dialog -->
        <Dialog :open="showLoadDialog" @update:open="showLoadDialog = $event">
            <DialogContent class="sm:max-w-[700px]">
                <DialogHeader>
                    <DialogTitle>Load Script</DialogTitle>
                </DialogHeader>

                <div class="py-4 max-h-[400px] overflow-auto border rounded-md">
                    <Table v-if="savedScripts.length > 0">
                        <TableHeader class="bg-muted">
                            <TableRow>
                                <TableHead class="w-[40px]"></TableHead>
                                <TableHead>Name</TableHead>
                                <TableHead>Database</TableHead>
                                <TableHead>Last Modified</TableHead>
                                <TableHead class="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow v-for="script in savedScripts" :key="script.id" :class="{ 'bg-primary/10': selectedScriptToLoad?.id === script.id }" @click="selectedScriptToLoad = script" class="cursor-pointer">
                                <TableCell>
                                    <div class="h-4 w-4 rounded-full border border-primary flex items-center justify-center">
                                        <div v-if="selectedScriptToLoad?.id === script.id" class="h-2 w-2 rounded-full bg-primary"></div>
                                    </div>
                                </TableCell>
                                <TableCell class="font-medium">{{ script.name }}</TableCell>
                                <TableCell>{{ databaseStore.connectionById(script.database)?.name || script.databaseName || '—' }}</TableCell>
                                <TableCell class="text-muted-foreground text-sm">{{ formatDate(script.updatedAt) }}</TableCell>
                                <TableCell class="text-right">
                                    <Button variant="ghost" size="icon" class="text-destructive hover:bg-destructive/10 h-8 w-8" @click.stop="deleteScript(script.id)">
                                        <Trash2 class="w-4 h-4" />
                                    </Button>
                                </TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                    <div v-else class="text-center py-8 text-muted-foreground">No saved scripts found.</div>
                </div>

                <DialogFooter>
                    <Button variant="outline" @click="showLoadDialog = false">Cancel</Button>
                    <Button @click="confirmLoadScript" :disabled="!selectedScriptToLoad">Load Script</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
<style lang="scss">
.sql-editor-container {
    height: 100%;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.editor-container {
    flex: 0 0 auto;
    min-height: 300px;
    border: 1px solid var(--p-border-color);
    border-radius: var(--p-border-radius);
    overflow: hidden;

    :deep(.cm-editor) {
        height: 100%;
    }

    :deep(.cm-content) {
        font-family: 'JetBrains Mono', 'Monaco', 'Consolas', monospace;
        font-size: 14px;
        line-height: 1.5;
        padding: 1rem;
    }
}

.result-cell {
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 200px;
}

// Responsive design
@media (max-width: 768px) {
    .sql-editor-container {
        gap: 0.5rem;
    }

    .editor-container {
        min-height: 250px;
    }

    .results-container {
        min-height: 300px;
    }
}

// Syntax highlighting adjustments for SQL
.editor-container {
    :deep(.cm-content) {
        .cm-keyword {
            color: var(--p-primary-color);
            font-weight: 600;
        }

        .cm-string {
            color: var(--p-green-500);
        }

        .cm-number {
            color: var(--p-orange-500);
        }

        .cm-comment {
            color: var(--p-text-muted-color);
            font-style: italic;
        }

        .cm-operator {
            color: var(--p-text-color);
            font-weight: 500;
        }
    }
}

/* Excel-like Grid Styles */
.excel-grid-wrapper {
    position: relative;
    min-height: 200px;
}

.excel-grid-header {
    display: flex;
    position: sticky;
    top: 0;
    z-index: 10;
    background: var(--secondary, #f1f5f9);
}

.excel-corner-cell {
    width: 50px;
    min-width: 50px;
    height: 32px;
    border-right: 1px solid var(--border, #e2e8f0);
    border-bottom: 1px solid var(--border, #e2e8f0);
    background: var(--secondary, #f1f5f9);
}

.excel-column-header {
    min-width: 150px;
    flex: 1;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    border-right: 1px solid var(--border, #e2e8f0);
    border-bottom: 1px solid var(--border, #e2e8f0);
    background: var(--secondary, #f1f5f9);
    color: var(--foreground, #0f172a);
    font-size: 0.85rem;
    user-select: none;
}

.excel-row {
    display: flex;
}

.excel-row:hover .excel-cell {
    background: var(--muted, #f8fafc);
}

.excel-row-header {
    width: 50px;
    min-width: 50px;
    height: 30px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.8rem;
    font-weight: 600;
    border-right: 1px solid var(--border, #e2e8f0);
    border-bottom: 1px solid var(--border, #e2e8f0);
    background: var(--secondary, #f1f5f9);
    color: var(--muted-foreground, #64748b);
    user-select: none;
}

.excel-cell {
    min-width: 150px;
    flex: 1;
    height: 30px;
    border-right: 1px solid var(--border, #e2e8f0);
    border-bottom: 1px solid var(--border, #e2e8f0);
    padding: 0;
    display: flex;
    align-items: center;
    font-size: 0.875rem;
}

</style>
