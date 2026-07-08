<script setup>
import { reactive, shallowRef, ref, computed, onMounted, watch } from 'vue';
import { redo, undo } from '@codemirror/commands';
import { search } from '@codemirror/search';
import { Codemirror } from 'vue-codemirror';
import { sql } from '@codemirror/lang-sql';
import { oneDark } from '@codemirror/theme-one-dark';
import { basicLight } from '@fsegurai/codemirror-theme-basic-light';
import { useLayout } from '@/layout/composables/layout';
import { toast } from 'vue-sonner';
import { useDatabaseStore } from '@/store/databaseStore';
import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';
import { getCurrentInstance } from 'vue';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import BaseChart from '@/components/BaseChart.vue';
import {
    Database, Loader2, Play, Save, File, Pencil, FolderOpen, Plus, Undo, RefreshCw,
    Copy, Search, Download, ChevronLeft, ChevronRight, Info, Trash2,
    Table2, LayoutList, TrendingUp, BarChart2, BarChart3, AreaChart, PieChart,
    ScatterChart, BarChart4, Settings2, Braces
} from 'lucide-vue-next';
import { useVariableStore } from '@/store/variableStore';

// Stores and services
const databaseStore = useDatabaseStore();
const userStore = userStoreMe();
const projectStore = useProjectStore();
var { proxy } = getCurrentInstance();
const variableStore = useVariableStore();

// Variable manager state
const showVariableManager = ref(false);
const editingVariable = ref(null);
const newVar = reactive({ id: '', name: '', label: '', type: 'input', defaultValue: '', dropdownSource: 'static', dropdownValues: '', dropdownQuery: '', dropdownConnectionId: '' });

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

// Computed: variables detected in current SQL
const detectedVars = computed(() => variableStore.detectInSql(code.value));
const hasVariables = computed(() => detectedVars.value.length > 0);

// Resolved options for dropdown-type variables (supports both static and SQL sources)
const resolvedOptions = ref({});

async function resolveDetectedOptions(vars) {
    for (const name of vars) {
        const def = variableStore.definitions.find(d => d.name === name);
        if (def?.type === 'dropdown') {
            resolvedOptions.value[name] = await variableStore.resolveDropdownOptions(def, proxy.$socket);
        }
    }
}

// Re-resolve whenever detected variables or definitions change
watch(
    [detectedVars, () => variableStore.definitions.length],
    ([vars]) => resolveDetectedOptions(vars)
);

// Database and script state
const selectedDatabase = ref(null);
const isExecuting = ref(false);
const hasExecuted = ref(false);
const queryResults = ref([]);
const queryColumns = ref([]);

// ── Visualization state ─────────────────────────────────────────────────────
// vizType: 'table' | 'pivot' | 'line' | 'bar' | 'bar-h' | 'area' | 'pie' | 'scatter' | 'waterfall'
const vizType = ref('table');
const vizConfig = reactive({
    labelColumn: '',
    valueColumns: [],
    aggregation: 'none',
    pivotRowField: '',
    pivotColField: '',
    pivotValueField: '',
    pivotAggregation: 'sum'
});

const TABULAR_TYPES = [
    { type: 'table',  label: 'Table',   icon: Table2 },
    { type: 'pivot',  label: 'Pivot',   icon: LayoutList },
];

const CHART_TYPES = [
    { type: 'line',      label: 'Line',       icon: TrendingUp },
    { type: 'bar',       label: 'Bar',        icon: BarChart2 },
    { type: 'bar-h',     label: 'H. Bar',     icon: BarChart3 },
    { type: 'area',      label: 'Area',       icon: AreaChart },
    { type: 'pie',       label: 'Pie',        icon: PieChart },
    { type: 'scatter',   label: 'Scatter',    icon: ScatterChart },
    { type: 'waterfall', label: 'Waterfall',  icon: BarChart4 },
];

const isChartVizType = computed(() =>
    ['line', 'bar', 'bar-h', 'area', 'pie', 'scatter', 'waterfall'].includes(vizType.value)
);

const chartVizType = computed(() => vizType.value); // already matches BaseChart types

const toggleValueColumn = (field) => {
    const idx = vizConfig.valueColumns.indexOf(field);
    if (idx >= 0) vizConfig.valueColumns.splice(idx, 1);
    else vizConfig.valueColumns.push(field);
};

const autoDetectVizColumns = () => {
    if (!queryColumns.value.length || !queryResults.value.length) return;
    const firstRow = queryResults.value[0];
    let labelCol = '';
    const valueCols = [];
    for (const col of queryColumns.value) {
        const val = firstRow[col.field];
        const isNum = typeof val === 'number' || (val !== null && val !== '' && !isNaN(Number(val)));
        if (!labelCol && !isNum) labelCol = col.field;
        else if (isNum) valueCols.push(col.field);
        else if (!labelCol) labelCol = col.field;
    }
    if (!labelCol && queryColumns.value.length > 0) labelCol = queryColumns.value[0].field;
    vizConfig.labelColumn = labelCol;
    vizConfig.valueColumns = valueCols.slice(0, 4);
};

const resetVizConfig = () => {
    vizType.value = 'table';
    vizConfig.labelColumn = '';
    vizConfig.valueColumns = [];
    vizConfig.aggregation = 'none';
    vizConfig.pivotRowField = '';
    vizConfig.pivotColField = '';
    vizConfig.pivotValueField = '';
    vizConfig.pivotAggregation = 'sum';
};

const applyVizFromScript = (script) => {
    if (!script.visualization) { vizType.value = 'table'; return; }
    try {
        const viz = typeof script.visualization === 'string'
            ? JSON.parse(script.visualization)
            : script.visualization;
        vizType.value = viz.type || 'table';
        vizConfig.labelColumn = viz.labelColumn || '';
        vizConfig.valueColumns = viz.valueColumns || [];
        vizConfig.aggregation = viz.aggregation || 'none';
        vizConfig.pivotRowField = viz.pivotRowField || '';
        vizConfig.pivotColField = viz.pivotColField || '';
        vizConfig.pivotValueField = viz.pivotValueField || '';
        vizConfig.pivotAggregation = viz.pivotAggregation || 'sum';
    } catch { vizType.value = 'table'; }
};

// Aggregated rows for chart data (groups + aggregates when aggregation != 'none')
const aggregatedRows = computed(() => {
    if (!queryResults.value.length) return [];
    if (vizConfig.aggregation === 'none' || !vizConfig.labelColumn) return queryResults.value;

    const groups = new Map();
    for (const row of queryResults.value) {
        const key = String(row[vizConfig.labelColumn] ?? '');
        if (!groups.has(key)) groups.set(key, []);
        groups.get(key).push(row);
    }

    const valueCols = vizConfig.valueColumns.length > 0
        ? vizConfig.valueColumns
        : queryColumns.value.filter(c => c.field !== vizConfig.labelColumn).map(c => c.field);

    return Array.from(groups.entries()).map(([key, rows]) => {
        const result = { [vizConfig.labelColumn]: key };
        for (const col of valueCols) {
            const nums = rows.map(r => Number(r[col]) || 0);
            switch (vizConfig.aggregation) {
                case 'sum':   result[col] = nums.reduce((a, b) => a + b, 0); break;
                case 'avg':   result[col] = nums.reduce((a, b) => a + b, 0) / nums.length; break;
                case 'count': result[col] = rows.length; break;
                case 'min':   result[col] = Math.min(...nums); break;
                case 'max':   result[col] = Math.max(...nums); break;
                default:      result[col] = nums[0];
            }
        }
        return result;
    });
});

const chartData = computed(() => {
    const rows = aggregatedRows.value;
    if (!rows.length || !queryColumns.value.length) return null;

    const labelCol = vizConfig.labelColumn || queryColumns.value[0]?.field || '';
    const valueCols = vizConfig.valueColumns.length > 0
        ? vizConfig.valueColumns
        : queryColumns.value.filter(c => c.field !== labelCol).slice(0, 4).map(c => c.field);

    if (!labelCol && valueCols.length === 0) return null;

    const palette = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272'];

    if (vizType.value === 'pie') {
        const col = valueCols[0] || labelCol;
        return {
            labels: rows.map(r => String(r[labelCol] ?? '')),
            datasets: [{
                label: queryColumns.value.find(c => c.field === col)?.header || col,
                data: rows.map(r => Number(r[col]) || 0),
                backgroundColor: palette,
                borderColor: palette,
                borderWidth: 1
            }]
        };
    }

    if (vizType.value === 'scatter') {
        const xCol = valueCols[0] || labelCol;
        const yCol = valueCols[1] || valueCols[0];
        if (!xCol || !yCol) return null;
        return {
            labels: [],
            datasets: [{
                label: `${xCol} vs ${yCol}`,
                data: rows.map(r => ({ x: Number(r[xCol]) || 0, y: Number(r[yCol]) || 0 })),
                backgroundColor: palette[0] + 'aa',
                borderColor: palette[0]
            }]
        };
    }

    const labels = rows.map(r => String(r[labelCol] ?? ''));
    const datasets = valueCols.map((col, i) => ({
        label: queryColumns.value.find(c => c.field === col)?.header || col,
        data: rows.map(r => Number(r[col]) || 0),
        backgroundColor: palette[i % palette.length] + 'bb',
        borderColor: palette[i % palette.length],
        borderWidth: 1,
        fill: vizType.value === 'area'
    }));

    return { labels, datasets };
});

// Pivot table computed
const pivotData = computed(() => {
    if (!queryResults.value.length) return null;
    const { pivotRowField, pivotColField, pivotValueField, pivotAggregation } = vizConfig;
    if (!pivotRowField || !pivotColField || !pivotValueField) return null;

    const colValues = [...new Set(queryResults.value.map(r => String(r[pivotColField] ?? '')))].sort();
    const rowValues = [...new Set(queryResults.value.map(r => String(r[pivotRowField] ?? '')))].sort();

    const agg = (matchingRows) => {
        if (!matchingRows.length) return null;
        const nums = matchingRows.map(r => Number(r[pivotValueField]) || 0);
        switch (pivotAggregation) {
            case 'sum':   return nums.reduce((a, b) => a + b, 0);
            case 'avg':   return +(nums.reduce((a, b) => a + b, 0) / nums.length).toFixed(2);
            case 'count': return matchingRows.length;
            case 'min':   return Math.min(...nums);
            case 'max':   return Math.max(...nums);
            default:      return nums[0];
        }
    };

    const rows = rowValues.map(rowVal => {
        const values = {};
        for (const colVal of colValues) {
            const matching = queryResults.value.filter(r =>
                String(r[pivotRowField] ?? '') === rowVal &&
                String(r[pivotColField] ?? '') === colVal
            );
            values[colVal] = agg(matching);
        }
        return { label: rowVal, values };
    });

    // Row totals
    rows.forEach(row => {
        const nums = colValues.map(c => row.values[c]).filter(v => v !== null && !isNaN(v));
        row.values['__total__'] = nums.length ? nums.reduce((a, b) => a + b, 0) : null;
    });

    return { columns: colValues, rows };
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

const paginatedResults = computed(() => {
    const start = (currentPage.value - 1) * rowsPerPage.value;
    const end = start + rowsPerPage.value;
    return queryResults.value.slice(start, end);
});

const totalPages = computed(() => Math.ceil(queryResults.value.length / rowsPerPage.value));

const editorStyle = computed(() => ({
    width: '100%',
    height: config.height,
    border: `1px solid var(--p-border-color)`,
    borderRadius: 'var(--p-border-radius)'
}));

const availableDatabases = computed(() => databaseStore.allConnections);

const { layoutConfig } = useLayout();
const extensions = computed(() => {
    return [sql(), search(), layoutConfig.darkMode ? oneDark : basicLight];
});

const handleReady = ({ view }) => { cmView.value = view; };

const handleUndo = () => {
    if (!cmView.value) return;
    undo({ state: cmView.value.state, dispatch: cmView.value.dispatch });
};

const handleRedo = () => {
    if (!cmView.value) return;
    redo({ state: cmView.value.state, dispatch: cmView.value.dispatch });
};

const handleStateUpdate = () => { currentScript.code = code.value; };

const copyText = async () => {
    if (!cmView.value) return;
    const selection = cmView.value.state.selection.main;
    const selectedText = selection.empty
        ? cmView.value.state.doc.toString()
        : cmView.value.state.doc.sliceString(selection.from, selection.to);
    try {
        await navigator.clipboard.writeText(selectedText);
        toast('Copied', { description: 'Text copied to clipboard' });
    } catch {
        toast.error('Copy Failed', { description: 'Failed to copy text to clipboard' });
    }
};

const pasteText = async () => {
    if (!cmView.value) return;
    try {
        const text = await navigator.clipboard.readText();
        const selection = cmView.value.state.selection.main;
        cmView.value.dispatch({ changes: { from: selection.from, to: selection.to, insert: text } });
        toast('Pasted', { description: 'Text pasted from clipboard' });
    } catch {
        toast.error('Paste Failed', { description: 'Failed to paste text from clipboard' });
    }
};

const toggleSearch = () => {
    if (!cmView.value) return;
    cmView.value.focus();
    const event = new KeyboardEvent('keydown', { key: 'f', ctrlKey: true });
    cmView.value.dom.dispatchEvent(event);
};

const getSelectedDatabaseName = () => databaseStore.connectionById(selectedDatabase.value)?.name || '';
const getSelectedDatabaseType = () => databaseStore.connectionById(selectedDatabase.value)?.type || '';
const getDatabaseIcon = () => Database;

const executeQuery = async () => {
    if (!selectedDatabase.value || !code.value.trim()) return;
    isExecuting.value = true;
    hasExecuted.value = false;
    queryResults.value = [];
    queryColumns.value = [];

    try {
        const db = databaseStore.connections?.find(c => c.id === selectedDatabase.value);
        const substitutedCode = variableStore.substituteInSql(code.value, db?.type || '');
        const result = await userStore.executeCommand('ExecuteSql', {
            database: selectedDatabase.value,
            code: substitutedCode
        }, proxy.$socket);

        if (result && result.Data) {
            queryResults.value = result.Data.rows || [];
            queryColumns.value = result.Data.columns || [];
        }
        currentPage.value = 1;
        hasExecuted.value = true;
        autoDetectVizColumns();

        toast('Query Executed', {
            description: `Query executed successfully. ${queryResults.value.length} rows returned.`
        });
    } catch (error) {
        toast.error('Query Failed', { description: error.message || 'An error occurred while executing the query' });
    } finally {
        isExecuting.value = false;
    }
};

const newScript = () => {
    currentScript.id = null;
    currentScript.name = '';
    currentScript.code = '';
    currentScript.database = null;
    code.value = '';
    queryResults.value = [];
    queryColumns.value = [];
    hasExecuted.value = false;
    resetVizConfig();
};

const buildVisualizationPayload = () => JSON.stringify({
    type: vizType.value,
    labelColumn: vizConfig.labelColumn,
    valueColumns: [...vizConfig.valueColumns],
    aggregation: vizConfig.aggregation,
    pivotRowField: vizConfig.pivotRowField,
    pivotColField: vizConfig.pivotColField,
    pivotValueField: vizConfig.pivotValueField,
    pivotAggregation: vizConfig.pivotAggregation
});

const saveScript = async () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled Script',
        code: code.value,
        database: selectedDatabase.value || '',
        projectId: projectStore.currentProjectId || undefined,
        visualization: buildVisualizationPayload(),
        createdAt: currentScript.id ? undefined : new Date(),
        updatedAt: new Date()
    };

    try {
        await userStore.executeCommand('SaveScript', { language: 'sql', script }, proxy.$socket);

        const existingIndex = savedScripts.value.findIndex(s => s.id === script.id);
        if (existingIndex >= 0) {
            savedScripts.value[existingIndex] = { ...savedScripts.value[existingIndex], ...script };
        } else {
            savedScripts.value.push({ ...script, databaseName: getSelectedDatabaseName() });
            currentScript.id = script.id;
        }
        toast('Script Saved', { description: `Script "${script.name}" saved successfully` });
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
    applyVizFromScript(script);
    showLoadDialog.value = false;
    selectedScriptToLoad.value = null;
    toast('Script Loaded', { description: `Script "${script.name}" loaded successfully` });
};

const deleteScript = async (scriptId) => {
    try {
        await userStore.executeCommand('DeleteScript', { language: 'sql', id: scriptId }, proxy.$socket);
        const index = savedScripts.value.findIndex(s => s.id === scriptId);
        if (index >= 0) {
            const scriptName = savedScripts.value[index].name;
            savedScripts.value.splice(index, 1);
            toast('Script Deleted', { description: `Script "${scriptName}" deleted successfully` });
        }
    } catch (error) {
        toast.error('Delete Failed', { description: error.message });
    }
};

const loadScriptsFromStorage = async () => {
    try {
        const params = { language: 'sql' };
        if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
        const result = await userStore.executeCommand('LoadScripts', params, proxy.$socket);
        if (result && result.Data) {
            savedScripts.value = result.Data.map(s => ({
                id: s.id || s.Id,
                name: s.name || s.Name || '',
                code: s.code || s.Code || '',
                database: s.database || s.Database || s.databaseconnectionid || s.DatabaseConnectionId || '',
                databaseName: s.databaseName || s.DatabaseName || '',
                language: s.language || s.Language || 'sql',
                visualization: s.visualization || s.Visualization || null,
                createdAt: new Date(s.createdAt || s.CreatedAt || s.createdat || Date.now()),
                updatedAt: new Date(s.updatedAt || s.UpdatedAt || s.updatedat || s.createdAt || s.CreatedAt || s.createdat || Date.now()),
            }));
        }
    } catch (error) {
        console.error('Failed to load scripts:', error);
        savedScripts.value = [];
    }
};

const generateId = () => {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') return crypto.randomUUID();
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
        const r = (Math.random() * 16) | 0;
        return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
    });
};

const formatDate = (date) => new Date(date).toLocaleString();

const formatCellValue = (value) => {
    if (value === null || value === undefined) return 'NULL';
    if (typeof value === 'string' && value.length > 100) return value.substring(0, 100) + '...';
    return value;
};

const exportResults = () => {
    if (queryResults.value.length === 0) return;
    const csv = [
        queryColumns.value.map(col => col.header).join(','),
        ...queryResults.value.map(row =>
            queryColumns.value.map(col => {
                const value = row[col.field];
                if (value === null || value === undefined) return '';
                if (typeof value === 'string' && value.includes(',')) return `"${value.replace(/"/g, '""')}"`;
                return value;
            }).join(',')
        )
    ].join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `query-results-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    toast('Export Complete', { description: 'Query results exported to CSV file' });
};

const saveVariableDef = async () => {
    if (!newVar.name.trim()) return;
    await variableStore.saveDefinition({ ...newVar }, proxy.$socket);
    editingVariable.value = null;
    Object.assign(newVar, { id: '', name: '', label: '', type: 'input', defaultValue: '', dropdownSource: 'static', dropdownValues: '', dropdownQuery: '', dropdownConnectionId: '' });
};

const editVar = (v) => {
    editingVariable.value = v;
    Object.assign(newVar, { ...v });
};

const deleteVar = async (id) => {
    await variableStore.deleteDefinition(id, proxy.$socket);
};

onMounted(() => {
    databaseStore.loadConnections(proxy.$socket);
    loadScriptsFromStorage();
    variableStore.loadDefinitions(proxy.$socket);
});

watch(() => projectStore.currentProjectId, () => {
    databaseStore.loadConnections(proxy.$socket);
    loadScriptsFromStorage();
    variableStore.loadDefinitions(proxy.$socket);
});

</script>

<template>
    <div class="sql-editor-container">
        <!-- Top Toolbar -->
        <div class="flex items-center justify-between mb-3 bg-card p-3 rounded-md border">
            <div class="flex items-center gap-3">
                <label class="font-semibold text-sm">Database:</label>
                <Select v-model="selectedDatabase" :disabled="isExecuting">
                    <SelectTrigger class="w-[200px]">
                        <SelectValue placeholder="Select Database">
                            <div v-if="selectedDatabase" class="flex items-center gap-2">
                                <Database class="w-4 h-4" />
                                <span>{{ getSelectedDatabaseName() }}</span>
                            </div>
                        </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem v-for="db in availableDatabases" :key="db.id" :value="db.id">
                            <div class="flex items-center gap-2">
                                <Database class="w-4 h-4" />
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
                <Button variant="outline" @click="showVariableManager = true" class="gap-2" title="Manage Variables">
                    <Braces class="w-4 h-4" />
                    Variables
                    <Badge v-if="variableStore.definitions.length" variant="secondary" class="ml-1 h-4 px-1 text-xs">{{ variableStore.definitions.length }}</Badge>
                </Button>
            </div>
        </div>

        <!-- Script Management Toolbar -->
        <div class="flex items-center justify-between mb-3 bg-card p-2 rounded-md border">
            <div class="flex items-center gap-2 px-2">
                <div class="flex items-center gap-2 cursor-pointer group">
                    <File class="w-4 h-4 text-muted-foreground" />
                    <Input v-if="editingScriptName" v-model="currentScript.name" class="h-8 w-[200px]" autofocus @blur="editingScriptName = false" @keyup.enter="editingScriptName = false" />
                    <span v-else class="font-medium" @click="editingScriptName = true">{{ currentScript.name || 'Untitled Script' }}</span>
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

        <!-- Variable Values Panel (shown when SQL has {{varName}}) -->
        <div v-if="hasVariables" class="mb-3 bg-card border rounded-md p-3">
            <div class="flex items-center gap-2 mb-2">
                <Braces class="w-4 h-4 text-muted-foreground" />
                <span class="text-sm font-semibold">Variable Values</span>
                <span class="text-xs text-muted-foreground">(substituted before execution)</span>
            </div>
            <div class="flex flex-wrap gap-3">
                <div v-for="varName in detectedVars" :key="varName" class="flex items-center gap-2">
                    <label class="text-sm font-medium whitespace-nowrap">{{ varName }}:</label>
                    <template v-if="variableStore.definitions.find(d => d.name === varName)?.type === 'dropdown'">
                        <Select :model-value="variableStore.values[varName] || variableStore.definitions.find(d => d.name === varName)?.defaultValue || ''" @update:model-value="v => variableStore.setValue(varName, v)">
                            <SelectTrigger class="h-8 w-[180px] text-xs">
                                <SelectValue :placeholder="varName" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem v-for="opt in (resolvedOptions[varName] || [])" :key="opt" :value="opt">{{ opt }}</SelectItem>
                            </SelectContent>
                        </Select>
                    </template>
                    <template v-else-if="variableStore.definitions.find(d => d.name === varName)?.type === 'date'">
                        <input type="date" class="flex h-8 w-[160px] rounded-md border border-input bg-transparent px-3 py-1 text-xs shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                            :value="variableStore.values[varName] || variableStore.definitions.find(d => d.name === varName)?.defaultValue || ''"
                            @change="variableStore.setValue(varName, $event.target.value)" />
                    </template>
                    <template v-else-if="variableStore.definitions.find(d => d.name === varName)?.type === 'datetime'">
                        <input type="datetime-local" class="flex h-8 w-[200px] rounded-md border border-input bg-transparent px-3 py-1 text-xs shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                            :value="variableStore.values[varName] || variableStore.definitions.find(d => d.name === varName)?.defaultValue || ''"
                            @change="variableStore.setValue(varName, $event.target.value)" />
                    </template>
                    <template v-else>
                        <input type="text" class="flex h-8 w-[160px] rounded-md border border-input bg-transparent px-3 py-1 text-xs shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                            :value="variableStore.values[varName] || variableStore.definitions.find(d => d.name === varName)?.defaultValue || ''"
                            @input="variableStore.setValue(varName, $event.target.value)"
                            :placeholder="varName" />
                    </template>
                </div>
            </div>
        </div>

        <!-- Results Panel -->
        <div class="results-container border rounded-md bg-card p-4 flex flex-col">

            <!-- Results header: title + count + export -->
            <div class="flex justify-between items-center mb-3">
                <h6 class="text-sm font-semibold">Query Results</h6>
                <div class="flex items-center gap-2">
                    <Badge v-if="queryResults.length > 0" variant="secondary">{{ queryResults.length }} rows</Badge>
                    <Button variant="outline" size="sm" @click="exportResults" :disabled="queryResults.length === 0" class="gap-2">
                        <Download class="w-4 h-4" />Export
                    </Button>
                </div>
            </div>

            <!-- Visualization picker (only when results exist) -->
            <div v-if="queryResults.length > 0" class="flex flex-wrap items-center gap-1 mb-2">
                <span class="text-xs text-muted-foreground mr-1 font-medium">Tabular:</span>
                <button
                    v-for="vt in TABULAR_TYPES" :key="vt.type"
                    @click="vizType = vt.type"
                    :class="[
                        'inline-flex items-center gap-1 px-2.5 py-1 rounded text-xs border cursor-pointer transition-colors',
                        vizType === vt.type
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-background border-border text-foreground hover:bg-muted'
                    ]"
                    :title="vt.label">
                    <component :is="vt.icon" class="w-3.5 h-3.5" />
                    {{ vt.label }}
                </button>

                <div class="w-px h-5 bg-border mx-2"></div>

                <span class="text-xs text-muted-foreground mr-1 font-medium">Charts:</span>
                <button
                    v-for="vt in CHART_TYPES" :key="vt.type"
                    @click="vizType = vt.type"
                    :class="[
                        'inline-flex items-center gap-1 px-2.5 py-1 rounded text-xs border cursor-pointer transition-colors',
                        vizType === vt.type
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-background border-border text-foreground hover:bg-muted'
                    ]"
                    :title="vt.label">
                    <component :is="vt.icon" class="w-3.5 h-3.5" />
                    {{ vt.label }}
                </button>
            </div>

            <!-- Chart config panel -->
            <div v-if="queryResults.length > 0 && isChartVizType"
                 class="flex flex-wrap items-center gap-x-4 gap-y-2 px-3 py-2 bg-muted/40 rounded-md border mb-3 text-sm">
                <div class="flex items-center gap-2">
                    <Settings2 class="w-3.5 h-3.5 text-muted-foreground" />
                    <span class="text-xs font-medium text-muted-foreground">Label:</span>
                    <Select v-model="vizConfig.labelColumn">
                        <SelectTrigger class="h-7 w-[130px] text-xs"><SelectValue placeholder="column" /></SelectTrigger>
                        <SelectContent>
                            <SelectItem v-for="col in queryColumns" :key="col.field" :value="col.field">{{ col.header }}</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div class="flex items-center gap-2">
                    <span class="text-xs font-medium text-muted-foreground">Aggregation:</span>
                    <Select v-model="vizConfig.aggregation">
                        <SelectTrigger class="h-7 w-[110px] text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                            <SelectItem value="none">None</SelectItem>
                            <SelectItem value="sum">Sum</SelectItem>
                            <SelectItem value="avg">Average</SelectItem>
                            <SelectItem value="count">Count</SelectItem>
                            <SelectItem value="min">Min</SelectItem>
                            <SelectItem value="max">Max</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div class="flex items-center gap-2 flex-wrap">
                    <span class="text-xs font-medium text-muted-foreground">Values:</span>
                    <button
                        v-for="col in queryColumns.filter(c => c.field !== vizConfig.labelColumn)" :key="col.field"
                        @click="toggleValueColumn(col.field)"
                        :class="[
                            'px-2 py-0.5 rounded-sm text-xs border cursor-pointer transition-colors',
                            vizConfig.valueColumns.includes(col.field)
                                ? 'bg-primary text-primary-foreground border-primary'
                                : 'bg-background border-border text-foreground hover:bg-muted'
                        ]">
                        {{ col.header }}
                    </button>
                </div>
            </div>

            <!-- Pivot config panel -->
            <div v-else-if="queryResults.length > 0 && vizType === 'pivot'"
                 class="flex flex-wrap items-center gap-x-4 gap-y-2 px-3 py-2 bg-muted/40 rounded-md border mb-3 text-sm">
                <div class="flex items-center gap-2">
                    <Settings2 class="w-3.5 h-3.5 text-muted-foreground" />
                    <span class="text-xs font-medium text-muted-foreground">Rows:</span>
                    <Select v-model="vizConfig.pivotRowField">
                        <SelectTrigger class="h-7 w-[130px] text-xs"><SelectValue placeholder="field" /></SelectTrigger>
                        <SelectContent>
                            <SelectItem v-for="col in queryColumns" :key="col.field" :value="col.field">{{ col.header }}</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <div class="flex items-center gap-2">
                    <span class="text-xs font-medium text-muted-foreground">Columns:</span>
                    <Select v-model="vizConfig.pivotColField">
                        <SelectTrigger class="h-7 w-[130px] text-xs"><SelectValue placeholder="field" /></SelectTrigger>
                        <SelectContent>
                            <SelectItem v-for="col in queryColumns" :key="col.field" :value="col.field">{{ col.header }}</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <div class="flex items-center gap-2">
                    <span class="text-xs font-medium text-muted-foreground">Value:</span>
                    <Select v-model="vizConfig.pivotValueField">
                        <SelectTrigger class="h-7 w-[130px] text-xs"><SelectValue placeholder="field" /></SelectTrigger>
                        <SelectContent>
                            <SelectItem v-for="col in queryColumns" :key="col.field" :value="col.field">{{ col.header }}</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <div class="flex items-center gap-2">
                    <span class="text-xs font-medium text-muted-foreground">Agg:</span>
                    <Select v-model="vizConfig.pivotAggregation">
                        <SelectTrigger class="h-7 w-[100px] text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                            <SelectItem value="sum">Sum</SelectItem>
                            <SelectItem value="avg">Average</SelectItem>
                            <SelectItem value="count">Count</SelectItem>
                            <SelectItem value="min">Min</SelectItem>
                            <SelectItem value="max">Max</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
            </div>

            <!-- TABLE view -->
            <div v-if="queryResults.length > 0 && vizType === 'table'" class="flex-1 overflow-auto border rounded-md excel-grid-wrapper bg-background">
                <div class="excel-grid-header">
                    <div class="excel-corner-cell"></div>
                    <div v-for="col in queryColumns" :key="col.field" class="excel-column-header">{{ col.header }}</div>
                </div>
                <div class="excel-grid-body">
                    <div v-for="(row, i) in paginatedResults" :key="i" class="excel-row">
                        <div class="excel-row-header">{{ (currentPage - 1) * rowsPerPage + i + 1 }}</div>
                        <div v-for="col in queryColumns" :key="col.field" class="excel-cell">
                            <span class="excel-cell-value px-2 truncate w-full pointer-events-none" :title="String(formatCellValue(row[col.field]))">
                                {{ formatCellValue(row[col.field]) }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- PIVOT TABLE view -->
            <div v-else-if="queryResults.length > 0 && vizType === 'pivot'" class="flex-1 overflow-auto border rounded-md bg-background">
                <div v-if="pivotData" class="overflow-auto">
                    <table class="text-xs w-full border-collapse">
                        <thead class="sticky top-0 bg-secondary">
                            <tr>
                                <th class="border border-border px-3 py-2 text-left font-semibold min-w-[120px]">{{ vizConfig.pivotRowField }}</th>
                                <th v-for="col in pivotData.columns" :key="col" class="border border-border px-3 py-2 text-center font-semibold min-w-[100px]">{{ col }}</th>
                                <th class="border border-border px-3 py-2 text-center font-semibold min-w-[100px] bg-muted">Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in pivotData.rows" :key="row.label" class="hover:bg-muted/40 transition-colors">
                                <td class="border border-border px-3 py-1.5 font-medium">{{ row.label }}</td>
                                <td v-for="col in pivotData.columns" :key="col" class="border border-border px-3 py-1.5 text-right">
                                    {{ row.values[col] !== null ? row.values[col] : '—' }}
                                </td>
                                <td class="border border-border px-3 py-1.5 text-right font-medium bg-muted/40">
                                    {{ row.values['__total__'] !== null ? row.values['__total__'] : '—' }}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div v-else class="text-center text-muted-foreground p-8">
                    Select Row, Column and Value fields above to generate the pivot table.
                </div>
            </div>

            <!-- CHART view -->
            <div v-else-if="queryResults.length > 0 && isChartVizType" class="flex-1 border rounded-md bg-card flex items-center justify-center" style="min-height: 380px;">
                <div class="w-full h-full" style="height: 380px;">
                    <BaseChart
                        v-if="chartData"
                        :type="chartVizType"
                        :data="chartData"
                        :show-header="false"
                        :show-footer="false"
                        :show-controls="false"
                        :show-legend="true"
                        height="380px"
                    />
                    <div v-else class="flex flex-col items-center justify-center h-full text-muted-foreground gap-2">
                        <Settings2 class="w-8 h-8 opacity-40" />
                        <p class="text-sm">Configure Label and Value columns above to render the chart.</p>
                    </div>
                </div>
            </div>

            <!-- Pagination (table only) -->
            <div v-if="queryResults.length > 0 && vizType === 'table'" class="flex items-center justify-between py-3 px-2 border-t mt-auto">
                <p class="text-sm text-muted-foreground">
                    Showing {{ (currentPage - 1) * rowsPerPage + 1 }} to {{ Math.min(currentPage * rowsPerPage, queryResults.length) }} of {{ queryResults.length }} entries
                </p>
                <div class="flex items-center space-x-2">
                    <p class="text-sm font-medium mr-2">Rows per page</p>
                    <select v-model="rowsPerPage" class="h-8 w-[70px] rounded-md border border-input bg-transparent px-2 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring mr-4">
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

            <!-- Empty states -->
            <div v-else-if="hasExecuted && queryResults.length === 0" class="text-center p-12 my-auto">
                <Info class="w-12 h-12 text-muted-foreground mx-auto mb-3 opacity-50" />
                <div class="text-xl font-medium mb-2">No Results</div>
                <div class="text-muted-foreground">The query executed successfully but returned no data</div>
            </div>
            <div v-else-if="!hasExecuted" class="text-center p-12 my-auto">
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
                    <DialogDescription>Select a saved SQL script to load into the editor.</DialogDescription>
                </DialogHeader>
                <div class="py-4 max-h-[400px] overflow-auto border rounded-md">
                    <Table v-if="savedScripts.length > 0">
                        <TableHeader class="bg-muted">
                            <TableRow>
                                <TableHead class="w-[40px]"></TableHead>
                                <TableHead>Name</TableHead>
                                <TableHead>Database</TableHead>
                                <TableHead>Visualization</TableHead>
                                <TableHead>Last Modified</TableHead>
                                <TableHead class="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow v-for="script in savedScripts" :key="script.id"
                                      :class="{ 'bg-primary/10': selectedScriptToLoad?.id === script.id }"
                                      @click="selectedScriptToLoad = script" class="cursor-pointer">
                                <TableCell>
                                    <div class="h-4 w-4 rounded-full border border-primary flex items-center justify-center">
                                        <div v-if="selectedScriptToLoad?.id === script.id" class="h-2 w-2 rounded-full bg-primary"></div>
                                    </div>
                                </TableCell>
                                <TableCell class="font-medium">{{ script.name }}</TableCell>
                                <TableCell>{{ databaseStore.connectionById(script.database)?.name || script.databaseName || '—' }}</TableCell>
                                <TableCell>
                                    <Badge v-if="script.visualization" variant="outline" class="text-xs capitalize">
                                        {{ (() => { try { return JSON.parse(script.visualization).type || 'table'; } catch { return 'table'; } })() }}
                                    </Badge>
                                    <span v-else class="text-muted-foreground text-xs">table</span>
                                </TableCell>
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

        <!-- Variable Manager Dialog -->
        <Dialog v-model:open="showVariableManager">
            <DialogContent class="max-w-3xl">
                <DialogHeader>
                    <DialogTitle>Variable Manager</DialogTitle>
                    <DialogDescription>Define variables that can be referenced in SQL with &#123;&#123;variableName&#125;&#125; syntax.</DialogDescription>
                </DialogHeader>

                <!-- Existing variables -->
                <div v-if="variableStore.definitions.length > 0" class="mb-4">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Label</TableHead>
                                <TableHead>Type</TableHead>
                                <TableHead>Default</TableHead>
                                <TableHead class="w-20"></TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow v-for="v in variableStore.definitions" :key="v.id">
                                <TableCell class="font-mono text-sm">{{ '{' + '{' + v.name + '}' + '}' }}</TableCell>
                                <TableCell>{{ v.label || v.name }}</TableCell>
                                <TableCell>{{ v.type }}</TableCell>
                                <TableCell>{{ v.defaultValue }}</TableCell>
                                <TableCell class="flex gap-1">
                                    <Button variant="ghost" size="icon" @click="editVar(v)"><Pencil class="w-4 h-4" /></Button>
                                    <Button variant="ghost" size="icon" @click="deleteVar(v.id)"><Trash2 class="w-4 h-4 text-destructive" /></Button>
                                </TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </div>
                <p v-else class="text-sm text-muted-foreground mb-4">No variables defined yet.</p>

                <!-- Add / Edit form -->
                <div class="border rounded-md p-4 space-y-3">
                    <h4 class="text-sm font-semibold">{{ editingVariable ? 'Edit Variable' : 'New Variable' }}</h4>
                    <div class="grid grid-cols-2 gap-3">
                        <div>
                            <label class="text-xs text-muted-foreground mb-1 block">Variable Name (used in SQL as {{ '{' + '{' + 'name' + '}' + '}' }})</label>
                            <Input v-model="newVar.name" placeholder="variableName" class="h-8" />
                        </div>
                        <div>
                            <label class="text-xs text-muted-foreground mb-1 block">Label (display name)</label>
                            <Input v-model="newVar.label" placeholder="Variable Label" class="h-8" />
                        </div>
                        <div>
                            <label class="text-xs text-muted-foreground mb-1 block">Type</label>
                            <Select v-model="newVar.type">
                                <SelectTrigger class="h-8">
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="input">InputBox</SelectItem>
                                    <SelectItem value="date">Date</SelectItem>
                                    <SelectItem value="datetime">Date & Time</SelectItem>
                                    <SelectItem value="dropdown">Dropdown</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div>
                            <label class="text-xs text-muted-foreground mb-1 block">Default Value</label>
                            <Input v-model="newVar.defaultValue" placeholder="default" class="h-8" />
                        </div>
                    </div>

                    <template v-if="newVar.type === 'dropdown'">
                        <div>
                            <label class="text-xs text-muted-foreground mb-1 block">Dropdown Source</label>
                            <Select v-model="newVar.dropdownSource">
                                <SelectTrigger class="h-8 w-40">
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="static">Static (comma-separated)</SelectItem>
                                    <SelectItem value="sql">SQL Query</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div v-if="newVar.dropdownSource === 'static'">
                            <label class="text-xs text-muted-foreground mb-1 block">Values (comma-separated)</label>
                            <Input v-model="newVar.dropdownValues" placeholder="Option A, Option B, Option C" class="h-8" />
                        </div>
                        <template v-if="newVar.dropdownSource === 'sql'">
                            <div>
                                <label class="text-xs text-muted-foreground mb-1 block">Connection</label>
                                <Select v-model="newVar.dropdownConnectionId">
                                    <SelectTrigger class="h-8">
                                        <SelectValue placeholder="Select connection" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem v-for="db in availableDatabases" :key="db.id" :value="db.id">{{ db.name }}</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            <div>
                                <label class="text-xs text-muted-foreground mb-1 block">SQL Query (single column)</label>
                                <Input v-model="newVar.dropdownQuery" placeholder="SELECT name FROM table ORDER BY name" class="h-8" />
                            </div>
                        </template>
                    </template>
                </div>

                <DialogFooter>
                    <Button variant="outline" @click="editingVariable = null; Object.assign(newVar, { id: '', name: '', label: '', type: 'input', defaultValue: '', dropdownSource: 'static', dropdownValues: '', dropdownQuery: '', dropdownConnectionId: '' })">Clear</Button>
                    <Button @click="saveVariableDef" :disabled="!newVar.name.trim()">{{ editingVariable ? 'Update' : 'Save Variable' }}</Button>
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

    :deep(.cm-editor) { height: 100%; }

    :deep(.cm-content) {
        font-family: 'JetBrains Mono', 'Monaco', 'Consolas', monospace;
        font-size: 14px;
        line-height: 1.5;
        padding: 1rem;
    }
}

@media (max-width: 768px) {
    .sql-editor-container { gap: 0.5rem; }
    .editor-container { min-height: 250px; }
    .results-container { min-height: 300px; }
}

:global(html:not(.dark)) .editor-container {
    :deep(.cm-content) {
        .cm-keyword { color: var(--p-primary-color); font-weight: 600; }
        .cm-string { color: var(--p-green-500); }
        .cm-number { color: var(--p-orange-500); }
        .cm-comment { color: var(--p-text-muted-color); font-style: italic; }
        .cm-operator { color: var(--p-text-color); font-weight: 500; }
    }
}

/* Excel-like Grid */
.excel-grid-wrapper { position: relative; min-height: 200px; }

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

.excel-row { display: flex; }
.excel-row:hover .excel-cell { background: var(--muted, #f8fafc); }

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
