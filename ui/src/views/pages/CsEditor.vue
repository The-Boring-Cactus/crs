<script setup>
import { ref, computed, onMounted, onUnmounted, reactive } from 'vue';
import { getCurrentInstance } from 'vue';
import { toast } from 'vue-sonner';
import CodeMirrorEditor from '@/components/CodeMirrorEditor.vue';
import BaseChart from '@/components/BaseChart.vue';
import { basicLight } from '@fsegurai/codemirror-theme-basic-light';
import { oneDark } from '@codemirror/theme-one-dark';
import { useLayout } from '@/layout/composables/layout';

import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { FileCode, Pencil, Loader2, Play, Save, FolderOpen, Plus, Undo, RefreshCw, Copy, Search, Code, Info, Square, Trash2, Download, Check, BarChart2, TableIcon, X, FlaskConical, BookOpen, Wand2 } from 'lucide-vue-next';

import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';
import { useVariableStore } from '@/store/variableStore';

import { WebSocketMessageClient } from '@/websocket/WebSocketMessageClient';

// Services and stores
const { proxy } = getCurrentInstance();
const userStore = userStoreMe();
const projectStore = useProjectStore();
const variableStore = useVariableStore();

// Script output: tables and charts emitted by Table() / Chart() calls
const scriptOutputs = ref([]);

const handleSocketOutput = (e) => {
    const response = e.detail;
    console.log(response);
    if (!response) return;
    const dataType = response.DataType || response.dataType;
    const payload = response.Payload || response.payload;
    if (!dataType || !payload) return;
    scriptOutputs.value.push({ id: Date.now() + Math.random(), type: dataType, payload });
};

const handleExecutionComplete = () => {
    isExecuting.value = false;
    hasExecuted.value = true;
    const timestamp = new Date().toLocaleTimeString();
    debugText.value += `[${timestamp}] Script execution finished.\n`;
    toast.success('Execution Complete', { description: 'Script executed successfully' });
};

const clearOutputs = () => {
    scriptOutputs.value = [];
};

// Chart output compatible format for BaseChart
const chartTypeMap = {
    bar: 'bar', line: 'line', pie: 'pie', doughnut: 'doughnut',
    area: 'area', radar: 'radar', scatter: 'scatter', bubble: 'bubble'
};

// Editor state
const code = ref(`// Welcome to Script Editor`);

const editorRef = ref();
const debugText = ref('');
const debugRows = ref(8);
const codeFunctions = ref(userStore.functions);

// Execution state
const isExecuting = ref(false);
const hasExecuted = ref(false);

// Script management
const currentScript = reactive({
    id: null,
    name: '',
    code: '',
    language: 'csharp'
});

const savedScripts = ref([]);
const showLoadDialog = ref(false);
const selectedScriptToLoad = ref(null);
const editingScriptName = ref(false);

// Theme-aware editor styling and extensions
const editorStyle = computed(() => {
    return {
        width: '100%',
        minHeight: '350px',
        border: `none`,
        borderRadius: 'var(--radius)'
    };
});

const { layoutConfig } = useLayout();

const editorTheme = computed(() => {
    return layoutConfig.darkMode ? oneDark : basicLight;
});

const handleSocketDebug = (e) => {
    console.log(e.detail);
    const timestamp = new Date().toLocaleTimeString();
    debugText.value += `[${timestamp}] ${e.detail}\n`;

    setTimeout(() => {
        const textarea = document.querySelector('.debug-textarea textarea');
        if (textarea) {
            textarea.scrollTop = textarea.scrollHeight;
        }
    }, 100);
};

// Editor operations
const handleCodeChange = (newCode) => {
    code.value = newCode;
    currentScript.code = newCode;
};

const handleLanguageChange = (language) => {
    console.log('Language changed to:', language);
    currentScript.language = language;
};

// Text operations
const copyText = async () => {
    if (!editorRef.value) return;

    try {
        const selectedText = window.getSelection()?.toString() || code.value;
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
    if (!editorRef.value) return;

    try {
        const text = await navigator.clipboard.readText();

        // Insert at current cursor position or replace selection
        if (editorRef.value.setCode) {
            const currentCode = code.value;
            const newCode = currentCode + text; // Simple append for now
            editorRef.value.setCode(newCode);
        }

        toast('Pasted', {
            description: 'Text pasted from clipboard'
        });
    } catch (error) {
        toast.error('Paste Failed', {
            description: 'Failed to paste text from clipboard'
        });
    }
};

const handleUndo = () => {
    console.log('Undo requested');
};

const handleRedo = () => {
    console.log('Redo requested');
};

const toggleSearch = () => {
    console.log('Search toggled');
};

const formatCode = () => {
    try {
        let formatted = code.value
            .replace(/;/g, ';\n')
            .replace(/{/g, '{\n')
            .replace(/}/g, '\n}')
            .split('\n')
            .map((line) => line.trim())
            .filter((line) => line.length > 0)
            .join('\n');

        if (editorRef.value?.setCode) {
            editorRef.value.setCode(formatted);
        }

        toast('Code Formatted', {
            description: 'Code has been formatted'
        });
    } catch (error) {
        toast.error('Format Failed', {
            description: 'Failed to format code'
        });
    }
};

// Execution operations
const handleExecute = async () => {
    if (!code.value.trim()) return;

    console.log('Executing code:', code.value);
    isExecuting.value = true;
    hasExecuted.value = false;
    debugText.value = '';
    scriptOutputs.value = [];

    const timestamp = new Date().toLocaleTimeString();
    debugText.value = `[${timestamp}] Starting script execution...\n`;

    toast('Execution Started', {
        description: 'Script execution has been initiated'
    });

    try {
        // ExecuteCs returns immediately; execution runs in background on server.
        // Completion is signalled by socket-execution-complete event.
        await userStore.executeCommand('ExecuteCs', {
            code: code.value,
            name: currentScript.name || 'Untitled Script',
            variables: variableStore.getValuesDict()
        }, proxy.$socket);
    } catch (error) {
        isExecuting.value = false;
        toast.error('Execution Failed', {
            description: error.message || 'An error occurred during script execution'
        });
    }
};

const stopExecution = () => {
    proxy.$socket.sendObj({
        type: 'StopExecution'
    });

    isExecuting.value = false;

    const timestamp = new Date().toLocaleTimeString();
    debugText.value += `[${timestamp}] Execution stopped by user.\n`;

    toast('Execution Stopped', {
        description: 'Script execution has been stopped'
    });
};

const clearDebug = () => {
    debugText.value = '';
    toast('Debug Cleared', {
        description: 'Debug output has been cleared'
    });
};

// Script management
const newScript = () => {
    currentScript.id = null;
    currentScript.name = '';
    currentScript.code = '';
    currentScript.language = 'csharp';

    code.value = `// New C# Script\nusing System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine("Hello, World!");\n\n        // Your code here\n\n    }\n}`;

    debugText.value = '';
    hasExecuted.value = false;

    if (editorRef.value?.setCode) {
        editorRef.value.setCode(code.value);
    }
};

const saveScript = async () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled C# Script',
        code: code.value,
        language: 'csharp',
        projectId: projectStore.currentProjectId || undefined,
        createdAt: currentScript.id ? undefined : new Date(),
        updatedAt: new Date()
    };

    try {
        await userStore.executeCommand('SaveScript', { language: 'csharp', script }, proxy.$socket);

        const existingIndex = savedScripts.value.findIndex((s) => s.id === script.id);
        if (existingIndex >= 0) {
            savedScripts.value[existingIndex] = { ...savedScripts.value[existingIndex], ...script };
        } else {
            savedScripts.value.push(script);
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
    currentScript.language = script.language || 'csharp';

    code.value = script.code || '';
    debugText.value = '';
    hasExecuted.value = false;

    if (editorRef.value?.setCode) {
        editorRef.value.setCode(script.code || '');
    }

    showLoadDialog.value = false;
    selectedScriptToLoad.value = null;

    toast('Script Loaded', {
        description: `Script "${script.name}" loaded successfully`
    });
};

const deleteScript = async (scriptId) => {
    try {
        await userStore.executeCommand('DeleteScript', { language: 'csharp', id: scriptId }, proxy.$socket);
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

// Storage management
const saveScriptsToStorage = () => {
    try {
        localStorage.setItem('cs-scripts', JSON.stringify(savedScripts.value));
    } catch (error) {
        console.error('Failed to save scripts:', error);
    }
};

const loadScriptsFromStorage = async () => {
    try {
        const params = { language: 'csharp' };
        if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
        const result = await userStore.executeCommand('LoadScripts', params, proxy.$socket);
        if (result && result.Data) {
            savedScripts.value = result.Data.map((script) => ({
                ...script,
                createdAt: script.createdAt ? new Date(script.createdAt) : new Date(),
                updatedAt: script.updatedAt ? new Date(script.updatedAt) : new Date()
            }));
        }
    } catch (error) {
        console.error('Failed to load scripts:', error);
        savedScripts.value = [];
    }
};

const exportTableCsv = (output) => {
    const { columns, rows } = output.payload;
    if (!columns?.length) return;
    const header = columns.join(',');
    const body = rows.map(row => columns.map(c => {
        const val = row[c] ?? '';
        return String(val).includes(',') ? `"${val}"` : val;
    }).join(',')).join('\n');
    const blob = new Blob([`${header}\n${body}`], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${output.payload.title || 'table'}-${Date.now()}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    toast('CSV exported');
};

const exportDebugLog = () => {
    if (!debugText.value.trim()) return;

    const blob = new Blob([debugText.value], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `debug-log-${new Date().toISOString().split('T')[0]}.txt`;
    link.click();
    URL.revokeObjectURL(url);

    toast('Export Complete', {
        description: 'Debug log exported successfully'
    });
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

const showHelpPanel = ref(false);

const SAMPLE_SCRIPT = `var numbers = Array(10, 25, 30, 45, 50);
var target = 30;
var index = 0;
var found = false;

while(index < ArrayLength(numbers) && !found) {
    var current = ArrayGet(numbers, index);
    if(current == target) {
        found = true;
        Print('¡Encontrado!');
    } else {
        index = index + 1;
    }
}`;

const loadSampleScript = () => {
    currentScript.id = null;
    currentScript.name = 'Linear Search Sample';
    code.value = SAMPLE_SCRIPT;
    if (editorRef.value?.setCode) editorRef.value.setCode(SAMPLE_SCRIPT);
    debugText.value = '';
    scriptOutputs.value = [];
    hasExecuted.value = false;
};

const functionCategories = [
    {
        name: 'Basic', badge: 'bg-blue-100 text-blue-800',
        fns: [
            { sig: 'Print(value)', desc: 'Prints value to debug output' },
            { sig: 'Concat(v1, v2, ...)', desc: 'Concatenates multiple values into a string' },
            { sig: 'ToString(value)', desc: 'Converts value to string' },
            { sig: 'Add(a, b)', desc: 'Returns a + b' },
            { sig: 'Multiply(a, b)', desc: 'Returns a × b' },
            { sig: 'CountWords(text)', desc: 'Counts words in a text' },
            { sig: 'GetTextStats()', desc: 'Prints accumulated text statistics' },
        ]
    },
    {
        name: 'Arrays', badge: 'bg-green-100 text-green-800',
        fns: [
            { sig: 'Array(v1, v2, ...)', desc: 'Creates a new array' },
            { sig: 'ArrayLength(arr)', desc: 'Returns number of elements' },
            { sig: 'ArrayGet(arr, index)', desc: 'Gets element at index (0-based)' },
            { sig: 'ArraySet(arr, index, value)', desc: 'Sets element at index' },
            { sig: 'ArrayPush(arr, value)', desc: 'Adds value to end; returns new length' },
            { sig: 'ArrayPop(arr)', desc: 'Removes and returns last element' },
            { sig: 'ArraySlice(arr, start, count)', desc: 'Returns sub-array' },
            { sig: 'ArrayJoin(arr, separator)', desc: 'Joins elements into a string' },
            { sig: 'ArraySort(arr)', desc: 'Returns numerically sorted copy' },
            { sig: 'ArrayReverse(arr)', desc: 'Returns reversed copy' },
        ]
    },
    {
        name: 'Math', badge: 'bg-purple-100 text-purple-800',
        fns: [
            { sig: 'Add(a, b)', desc: 'a + b' },
            { sig: 'Subtract(a, b)', desc: 'a − b' },
            { sig: 'Multiply(a, b)', desc: 'a × b' },
            { sig: 'Divide(a, b)', desc: 'a ÷ b' },
            { sig: 'Power(base, exp)', desc: 'base raised to exp' },
            { sig: 'Sqrt(x)', desc: 'Square root' },
            { sig: 'Abs(x)', desc: 'Absolute value' },
            { sig: 'Floor(x)', desc: 'Round down to integer' },
            { sig: 'Ceil(x)', desc: 'Round up to integer' },
            { sig: 'Round(x)', desc: 'Round to nearest integer' },
            { sig: 'Sin(x)', desc: 'Sine (radians)' },
            { sig: 'Cos(x)', desc: 'Cosine (radians)' },
            { sig: 'Tan(x)', desc: 'Tangent (radians)' },
            { sig: 'Log(x)', desc: 'Natural logarithm' },
            { sig: 'Log10(x)', desc: 'Base-10 logarithm' },
            { sig: 'Exp(x)', desc: 'e raised to x' },
            { sig: 'Min(a, b)', desc: 'Smaller of two values' },
            { sig: 'Max(a, b)', desc: 'Larger of two values' },
            { sig: 'Random()', desc: 'Random number 0–1' },
            { sig: 'PI()', desc: 'Returns π ≈ 3.14159' },
            { sig: 'E()', desc: 'Returns e ≈ 2.71828' },
        ]
    },
    {
        name: 'Strings', badge: 'bg-orange-100 text-orange-800',
        fns: [
            { sig: 'StringLength(str)', desc: 'Length of string' },
            { sig: 'Substring(str, start[, length])', desc: 'Extract part of a string' },
            { sig: 'IndexOf(str, search)', desc: 'First index of search (−1 if not found)' },
            { sig: 'ToUpper(str)', desc: 'Convert to uppercase' },
            { sig: 'ToLower(str)', desc: 'Convert to lowercase' },
            { sig: 'Trim(str)', desc: 'Remove leading/trailing whitespace' },
            { sig: 'Replace(str, old, new)', desc: 'Replace all occurrences' },
            { sig: 'Split(str[, separator])', desc: 'Split string into array' },
            { sig: 'StartsWith(str, prefix)', desc: 'true if str starts with prefix' },
            { sig: 'EndsWith(str, suffix)', desc: 'true if str ends with suffix' },
            { sig: 'Contains(str, search)', desc: 'true if str contains search' },
            { sig: 'PadLeft(str, width[, char])', desc: 'Pad string on the left' },
            { sig: 'PadRight(str, width[, char])', desc: 'Pad string on the right' },
        ]
    },
    {
        name: 'Statistics', badge: 'bg-teal-100 text-teal-800',
        fns: [
            { sig: 'Sum(arr)', desc: 'Sum of all elements' },
            { sig: 'Count(arr)', desc: 'Number of elements' },
            { sig: 'Mean(arr)', desc: 'Arithmetic mean' },
            { sig: 'Median(arr)', desc: 'Middle value of sorted data' },
            { sig: 'Mode(arr)', desc: 'Most frequent value' },
            { sig: 'Range(arr)', desc: 'max − min' },
            { sig: 'Variance(arr)', desc: 'Statistical variance' },
            { sig: 'StandardDeviation(arr)', desc: 'Standard deviation' },
            { sig: 'Percentile(arr, p)', desc: 'p-th percentile (0–100)' },
            { sig: 'Quartile(arr, q)', desc: 'Quartile 1, 2, or 3' },
            { sig: 'Correlation(arr1, arr2)', desc: 'Pearson correlation coefficient' },
            { sig: 'ZScore(arr, value)', desc: 'Z-score of value in dataset' },
            { sig: 'PrintHistogram(arr, bins)', desc: 'Print text histogram to debug output' },
            { sig: 'CreateHistogram(arr, bins)', desc: 'Return histogram bin data' },
        ]
    },
    {
        name: 'Output', badge: 'bg-pink-100 text-pink-800',
        fns: [
            { sig: 'Table(data[, title])', desc: 'Render query results as a table widget' },
            { sig: 'Chart(type, labels, values[, title])', desc: 'Render a chart — type: "bar" | "line" | "pie" | "doughnut" | "area" | "radar" | "scatter"' },
            { sig: 'StatReport(title, data)', desc: 'Render a formatted statistical report' },
        ]
    },
    {
        name: 'Database', badge: 'bg-slate-100 text-slate-800',
        fns: [
            { sig: 'ConnectPostgres(host, db, user, pass[, port])', desc: 'Connect to PostgreSQL' },
            { sig: 'ConnectSqlServer(server, db, user, pass)', desc: 'Connect to SQL Server' },
            { sig: 'DisconnectDB()', desc: 'Close the active connection' },
            { sig: 'ExecuteQuery(connectionId, sql)', desc: 'Run SELECT — returns list of row dictionaries' },
            { sig: 'ExecuteNonQuery(connectionId, sql)', desc: 'Run INSERT / UPDATE / DELETE — returns rows affected' },
            { sig: 'ExecuteScalar(connectionId, sql)', desc: 'Run query and return a single value' },
            { sig: 'ExecuteScript(scriptName)', desc: 'Run a saved SQL Query from the project by name — returns same as ExecuteQuery' },
            { sig: 'ReadDataset(name)', desc: 'Load a saved Dataset from the project by name — returns list of row dictionaries' },
            { sig: 'ReadSpreadsheet(name)', desc: 'Load a saved Spreadsheet from the project by name — returns list of row dictionaries' },
            { sig: 'GetRowValue(row, key)', desc: 'Get a column value from a row dictionary' },
            { sig: 'GetRowKeys(row)', desc: 'Get all column names from a row' },
            { sig: 'BeginTransaction()', desc: 'Begin a database transaction' },
            { sig: 'CommitTransaction()', desc: 'Commit the current transaction' },
            { sig: 'RollbackTransaction()', desc: 'Rollback the current transaction' },
        ]
    },
    {
        name: 'Memory', badge: 'bg-yellow-100 text-yellow-800',
        fns: [
            { sig: 'Counter(name)', desc: 'Increment named counter; returns new value' },
            { sig: 'SaveToVar(name, value)', desc: 'Save value to a named variable' },
            { sig: 'LoadFromVar(name)', desc: 'Load value from a named variable' },
            { sig: 'MaxVar(name, value)', desc: 'Keep the maximum value in a named variable' },
            { sig: 'IncrementVar(name)', desc: 'Increment a named variable by 1' },
        ]
    },
];

// Initialize component
onMounted(() => {
    window.addEventListener('socket-debug', handleSocketDebug);
    window.addEventListener('socket-output', handleSocketOutput);
    window.addEventListener('socket-execution-complete', handleExecutionComplete);
    loadScriptsFromStorage();
    variableStore.loadDefinitions(proxy.$socket);
});

onUnmounted(() => {
    window.removeEventListener('socket-debug', handleSocketDebug);
    window.removeEventListener('socket-output', handleSocketOutput);
    window.removeEventListener('socket-execution-complete', handleExecutionComplete);
});
</script>

<template>
    <div class="cs-editor-container">
        <!-- Main Toolbar -->
        <div class="flex items-center justify-between mb-3 bg-card p-2 border rounded-md">
            <div class="flex items-center gap-2">
                <div class="flex items-center gap-2 relative group cursor-pointer">
                    <FileCode class="w-4 h-4 text-muted-foreground" />
                    <Input v-if="editingScriptName" v-model="currentScript.name" placeholder="Script name" class="h-8 w-[200px]" autofocus @blur="editingScriptName = false" @keyup.enter="editingScriptName = false" />
                    <span v-else class="font-medium px-2 py-1 rounded hover:bg-muted" @click="editingScriptName = true">
                        {{ currentScript.name || 'Untitled C# Script' }}
                    </span>
                    <Pencil v-if="!editingScriptName" class="w-3 h-3 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity" @click="editingScriptName = true" />
                </div>
            </div>

            <div class="flex items-center gap-2">
                <Button @click="handleExecute" :disabled="!code.trim() || isExecuting" class="gap-2 bg-green-600 hover:bg-green-700">
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

        <!-- Edit Menu Toolbar -->
        <div class="flex items-center justify-between mb-3 bg-muted/50 p-2 border rounded-md">
            <div class="flex items-center gap-1">
                <Button variant="ghost" size="icon" @click="loadScript" title="Open Script"><FolderOpen class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="newScript" title="New Script"><Plus class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="loadSampleScript" title="Load Sample Script"><Wand2 class="w-4 h-4" /></Button>
                <div class="w-px h-5 bg-border mx-1"></div>
                <Button variant="ghost" size="icon" @click="handleUndo" title="Undo"><Undo class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="handleRedo" title="Redo"><RefreshCw class="w-4 h-4" /></Button>
                <div class="w-px h-5 bg-border mx-1"></div>
                <Button variant="ghost" size="icon" @click="copyText" title="Copy"><Copy class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="pasteText" title="Paste"><Copy class="w-4 h-4" /></Button>
                <div class="w-px h-5 bg-border mx-1"></div>
                <Button variant="ghost" size="icon" @click="toggleSearch" title="Find"><Search class="w-4 h-4" /></Button>
                <Button variant="ghost" size="icon" @click="formatCode" title="Format Code"><Code class="w-4 h-4" /></Button>
            </div>

            <div class="flex items-center gap-2">
                <Button variant="outline" size="sm" @click="showHelpPanel = true" class="gap-1.5">
                    <BookOpen class="w-4 h-4" />
                    Functions
                </Button>
                <div class="w-px h-5 bg-border mx-1"></div>
                <Button variant="destructive" size="sm" @click="stopExecution" :disabled="!isExecuting" class="gap-2">
                    <Square class="w-4 h-4" />
                    Stop
                </Button>
                <Button variant="secondary" size="icon" class="h-8 w-8" @click="clearDebug" title="Clear Debug">
                    <Trash2 class="w-4 h-4" />
                </Button>
            </div>
        </div>

        <!-- Code Editor -->
        <div class="editor-container border rounded-md mb-3">
            <CodeMirrorEditor ref="editorRef" :initial-code="code" :code-functions="codeFunctions" :initial-language="'csharp'" @update:code="handleCodeChange" @language-changed="handleLanguageChange" :style="editorStyle" :theme="editorTheme" />
        </div>

        <!-- Debug Output -->
        <div class="debug-container border rounded-md p-4 bg-card">
            <div class="flex justify-between items-center mb-3">
                <div class="flex items-center gap-2">
                    <h6 class="m-0 font-semibold">Debug Output</h6>
                    <Badge v-if="isExecuting" variant="secondary" class="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Running</Badge>
                    <Badge v-else-if="hasExecuted" variant="secondary" class="bg-green-100 text-green-800 hover:bg-green-100">Completed</Badge>
                </div>
                <div class="flex items-center gap-2">
                    <Button variant="secondary" size="sm" @click="exportDebugLog" :disabled="!debugText.trim()" class="gap-2">
                        <Download class="w-4 h-4" />
                        Export Log
                    </Button>
                </div>
            </div>

            <div class="debug-output" :class="{ executing: isExecuting }">
                <Textarea v-model="debugText" :rows="debugRows" :readonly="isExecuting" placeholder="Debug output will appear here when you execute the script..." class="debug-textarea font-mono text-sm resize-y" />
            </div>
        </div>

        <!-- Script Output: Tables and Charts -->
        <div v-if="scriptOutputs.length > 0" class="output-container border rounded-md p-4 bg-card">
            <div class="flex justify-between items-center mb-3">
                <div class="flex items-center gap-2">
                    <h6 class="m-0 font-semibold">Script Output</h6>
                    <Badge variant="secondary">{{ scriptOutputs.length }} result{{ scriptOutputs.length > 1 ? 's' : '' }}</Badge>
                </div>
                <Button variant="ghost" size="icon" @click="clearOutputs" title="Clear outputs">
                    <X class="w-4 h-4" />
                </Button>
            </div>

            <div v-for="output in scriptOutputs" :key="output.id" class="mb-6 last:mb-0">
                <!-- Table output -->
                <template v-if="output.type === 'Table'">
                    <div class="flex items-center gap-2 mb-2">
                        <TableIcon class="w-4 h-4 text-muted-foreground" />
                        <span class="font-medium text-sm">{{ output.payload.title || 'Table' }}</span>
                        <Badge variant="outline" class="text-xs">{{ output.payload.rows?.length || 0 }} rows</Badge>
                        <Button variant="ghost" size="icon" class="h-6 w-6 ml-auto" title="Export CSV" @click="exportTableCsv(output)">
                            <Download class="w-3 h-3" />
                        </Button>
                    </div>
                    <div class="border rounded overflow-auto max-h-80">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead v-for="col in output.payload.columns" :key="col">{{ col }}</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                <TableRow v-for="(row, ri) in output.payload.rows" :key="ri">
                                    <TableCell v-for="col in output.payload.columns" :key="col">{{ row[col] }}</TableCell>
                                </TableRow>
                                <TableRow v-if="!output.payload.rows?.length">
                                    <TableCell :colspan="output.payload.columns?.length || 1" class="text-center text-muted-foreground h-16">No data</TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>
                    </div>
                </template>

                <!-- Chart output -->
                <template v-else-if="output.type === 'Chart'">
                    <div class="flex items-center gap-2 mb-2">
                        <BarChart2 class="w-4 h-4 text-muted-foreground" />
                        <span class="font-medium text-sm">{{ output.payload.title || 'Chart' }}</span>
                        <Badge variant="outline" class="text-xs capitalize">{{ output.payload.chartType }}</Badge>
                    </div>
                    <div class="h-64">
                        <BaseChart
                            :type="chartTypeMap[output.payload.chartType] || 'bar'"
                            :data="{ labels: output.payload.labels, datasets: output.payload.datasets }"
                            :title="output.payload.title"
                            :show-header="false"
                            :show-footer="false"
                            height="100%"
                        />
                    </div>
                </template>

                <!-- StatReport output -->
                <template v-else-if="output.type === 'StatReport'">
                    <div class="flex items-center gap-2 mb-3">
                        <FlaskConical class="w-4 h-4 text-muted-foreground" />
                        <span class="font-semibold text-sm">{{ output.payload.title || 'Statistical Report' }}</span>
                    </div>
                    <div class="space-y-4 border rounded p-4 bg-muted/20">
                        <div v-for="(section, si) in output.payload.sections" :key="si" class="space-y-2">
                            <h4 v-if="section.heading" class="font-medium text-sm border-b pb-1">{{ section.heading }}</h4>
                            <!-- Table section -->
                            <div v-if="section.type === 'table'" class="overflow-auto">
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead v-for="col in section.columns" :key="col" class="text-xs">{{ col }}</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        <TableRow v-for="(row, ri) in section.rows" :key="ri">
                                            <TableCell v-for="col in section.columns" :key="col" class="text-xs font-mono">{{ row[col] }}</TableCell>
                                        </TableRow>
                                    </TableBody>
                                </Table>
                            </div>
                            <!-- Text section -->
                            <p v-else-if="section.type === 'text'" class="text-sm text-muted-foreground font-mono bg-muted/50 p-2 rounded">{{ section.content }}</p>
                        </div>
                    </div>
                </template>
            </div>
        </div>

        <!-- Function Reference Dialog -->
        <Dialog :open="showHelpPanel" @update:open="showHelpPanel = $event">
            <DialogContent class="max-w-4xl max-h-[85vh] flex flex-col">
                <DialogHeader>
                    <DialogTitle class="flex items-center gap-2">
                        <BookOpen class="w-5 h-5" />
                        FunctEngine — Function Reference
                    </DialogTitle>
                </DialogHeader>
                <div class="overflow-y-auto flex-1 pr-1">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 py-2">
                        <div v-for="cat in functionCategories" :key="cat.name" class="border rounded-lg overflow-hidden">
                            <div class="px-3 py-2 bg-muted/50 border-b flex items-center gap-2">
                                <span class="text-xs font-semibold uppercase tracking-wider">{{ cat.name }}</span>
                                <span :class="['text-[10px] font-medium px-1.5 py-0.5 rounded-full', cat.badge]">{{ cat.fns.length }} functions</span>
                            </div>
                            <div class="divide-y">
                                <div v-for="fn in cat.fns" :key="fn.sig" class="px-3 py-2 hover:bg-muted/30 transition-colors">
                                    <code class="text-xs font-mono text-primary font-medium">{{ fn.sig }}</code>
                                    <p class="text-[11px] text-muted-foreground mt-0.5 leading-snug">{{ fn.desc }}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <DialogFooter class="pt-2">
                    <Button variant="outline" @click="showHelpPanel = false">Close</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <!-- Load Script Dialog -->
        <Dialog :open="showLoadDialog" @update:open="showLoadDialog = $event">
            <DialogContent class="sm:max-w-[600px]">
                <DialogHeader>
                    <DialogTitle>Load C# Script</DialogTitle>
                </DialogHeader>

                <div class="py-4">
                    <div class="border rounded-md overflow-hidden">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Name</TableHead>
                                    <TableHead>Last Modified</TableHead>
                                    <TableHead class="w-[100px]">Actions</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                <TableRow v-for="script in savedScripts" :key="script.id" :class="{ 'bg-muted': selectedScriptToLoad?.id === script.id }" @click="selectedScriptToLoad = script" class="cursor-pointer">
                                    <TableCell class="font-medium">{{ script.name }}</TableCell>
                                    <TableCell>{{ formatDate(script.updatedAt) }}</TableCell>
                                    <TableCell @click.stop>
                                        <Button variant="ghost" size="icon" class="text-destructive hover:bg-destructive hover:text-destructive-foreground h-8 w-8" @click="deleteScript(script.id)" title="Delete">
                                            <Trash2 class="w-4 h-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                                <TableRow v-if="savedScripts.length === 0">
                                    <TableCell colspan="3" class="h-24 text-center text-muted-foreground"> No saved scripts found. </TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>
                    </div>
                </div>

                <DialogFooter>
                    <Button variant="outline" @click="showLoadDialog = false">Cancel</Button>
                    <Button @click="confirmLoadScript" :disabled="!selectedScriptToLoad" class="gap-2">
                        <Check class="w-4 h-4" />
                        Load
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
<style lang="scss">
.cs-editor-container {
    height: 100%;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.editor-container {
    flex: 0 0 auto;
    min-height: 350px;
    overflow: hidden;

    :deep(.code-editor-container) {
        border: none;
        border-radius: 0;
    }

    :deep(.code-editor) {
        min-height: 350px;
    }

    :deep(.cm-editor) {
        height: 100%;
        font-family: 'JetBrains Mono', 'Monaco', 'Consolas', monospace;
    }

    :deep(.cm-content) {
        font-size: 14px;
        line-height: 1.5;
        padding: 1rem;
    }
}

.debug-container {
    flex: 1;
    min-height: 300px;
    display: flex;
    flex-direction: column;

    .debug-output {
        flex: 1;
        position: relative;

        &.executing {
            &::after {
                content: '';
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: linear-gradient(90deg, transparent, rgba(var(--p-primary-color-rgb), 0.1), transparent);
                animation: pulse 2s ease-in-out infinite;
                pointer-events: none;
            }
        }
    }

    .debug-textarea {
        height: 100%;

        :deep(textarea) {
            font-family: 'JetBrains Mono', 'Monaco', 'Consolas', monospace;
            font-size: 13px;
            line-height: 1.4;
            background-color: var(--p-surface-50);
            border: 1px solid var(--p-border-color);
            color: var(--p-text-color);
            resize: vertical;
            min-height: 200px;

            &:focus {
                outline: 2px solid var(--p-primary-color);
                outline-offset: -2px;
            }

            &[readonly] {
                background-color: var(--p-surface-100);
                cursor: not-allowed;
            }
        }
    }
}

// Responsive design
@media (max-width: 768px) {
    .cs-editor-container {
        gap: 0.5rem;
    }

    .editor-container {
        min-height: 300px;

        :deep(.code-editor) {
            min-height: 300px;
        }
    }

    .debug-container {
        min-height: 250px;
    }

    :deep(.p-toolbar) {
        flex-wrap: wrap;
        gap: 0.5rem;

        .p-toolbar-group-start,
        .p-toolbar-group-center,
        .p-toolbar-group-end {
            flex-wrap: wrap;
            gap: 0.25rem;
        }
    }
}

// Animation for executing state
@keyframes pulse {
    0%,
    100% {
        opacity: 0;
    }
    50% {
        opacity: 1;
    }
}
</style>
