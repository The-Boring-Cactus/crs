<script setup>
import { ref, computed, onMounted, onUnmounted, reactive, markRaw } from 'vue';
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
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { FileCode, Pencil, Loader2, Play, Save, FolderOpen, Plus, Undo, RefreshCw, Copy, Search, Code, Info, Square, Trash2, Download, Check, BarChart2, TableIcon, X, FlaskConical, BookOpen, Wand2, Hash, Braces, Sigma, Type, Database, ChevronDown, Calendar, TestTube, DollarSign, TrendingUp, Shuffle, Combine } from 'lucide-vue-next';

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

    code.value = `// Your code here\n\n`;

    debugText.value = '';
    hasExecuted.value = false;

    if (editorRef.value?.setCode) {
        editorRef.value.setCode(code.value);
    }
};

const saveScript = async () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled Script',
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
}

// Con incremento explícito
for(var k = 0; k < 3; k = k + 1) {
    Print(Concat('k = ', ToString(k)));
}

// Con operador ++
for(var k = 0; k < 3; k++) {
    Print(Concat('k = ', ToString(k)));
}

// Con operador --
for(var k = 10; k > 0; k--) {
    Print(ToString(k));
}

// Con incrementos personalizados
for(var k = 0; k < 10; k = k + 2) {
    Print(ToString(k)); // 0, 2, 4, 6, 8
}

// Con expresiones más complejas
for(var k = 1; k < 100; k = k * 2) {
    Print(ToString(k)); // 1, 2, 4, 8, 16, 32, 64
}`;

const loadSampleScript = () => {
    currentScript.id = null;
    currentScript.name = 'Sample';
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
            { sig: 'GetColumn(rows, key)', desc: 'Extract one column across every row into a flat array' },
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

// Runnable example scripts for the Snippets Toolbar, organized by the same
// categories as functionCategories above. Each snippet is a complete,
// self-contained script that only calls functions actually reachable from
// this pseudocode language (built-in functions registered under a plain
// name — DLL-loaded functions such as "DoeLibrary.OneWayAnova" use a
// dotted Type.Method name the parser doesn't support calling, so those are
// intentionally not used here).
const snippetCategories = [
    {
        name: 'Basic', icon: markRaw(Hash),
        snippets: [
            {
                name: 'Print & Concat',
                description: 'Build and print formatted strings in a loop',
                code: `// Basic: printing and string concatenation
var name = 'World';
var count = 3;
for (var i = 0; i < count; i = i + 1) {
    Print(Concat('Hello, ', name, '! (', ToString(i + 1), '/', ToString(count), ')'));
}`
            }
        ]
    },
    {
        name: 'Arrays', icon: markRaw(Braces),
        snippets: [
            {
                name: 'Array Basics',
                description: 'Create, push, sort, reverse, and slice an array',
                code: `// Arrays: create, push, sort, reverse, slice
var numbers = Array(5, 3, 8, 1, 9, 2);
Print(Concat('Original: ', ArrayJoin(numbers, ', ')));

ArrayPush(numbers, 7);
Print(Concat('After push(7): ', ArrayJoin(numbers, ', ')));

var sorted = ArraySort(numbers);
Print(Concat('Sorted: ', ArrayJoin(sorted, ', ')));

var reversed = ArrayReverse(sorted);
Print(Concat('Reversed: ', ArrayJoin(reversed, ', ')));

var smallestThree = ArraySlice(sorted, 0, 3);
Print(Concat('3 smallest: ', ArrayJoin(smallestThree, ', ')));`
            }
        ]
    },
    {
        name: 'Math', icon: markRaw(Sigma),
        snippets: [
            {
                name: 'Math Operations',
                description: 'Powers, roots, rounding, and trig functions',
                code: `// Math: arithmetic, powers, roots, rounding, trig
var numBase = 2;
var exponent = 10;
Print(Concat(ToString(numBase), '^', ToString(exponent), ' = ', ToString(Power(numBase, exponent))));
Print(Concat('Sqrt(144) = ', ToString(Sqrt(144))));
Print(Concat('Round(3.7) = ', ToString(Round(3.7)), '  Floor(3.7) = ', ToString(Floor(3.7)), '  Ceil(3.2) = ', ToString(Ceil(3.2))));
Print(Concat('Sin(PI/2) = ', ToString(Sin(Divide(PI(), 2)))));
Print(Concat('Max(4, 9) = ', ToString(Max(4, 9)), '  Min(4, 9) = ', ToString(Min(4, 9))));`
            }
        ]
    },
    {
        name: 'Strings', icon: markRaw(Type),
        snippets: [
            {
                name: 'String Utilities',
                description: 'Trim, case conversion, search, split, and padding',
                code: `// Strings: case conversion, search, split, padding
var text = '  Hello, FunctEngine World!  ';
var trimmed = Trim(text);
Print(Concat('Trimmed: [', trimmed, ']'));
Print(Concat('Upper: ', ToUpper(trimmed)));
Print(Concat('Lower: ', ToLower(trimmed)));
Print(Concat('Contains "Engine"? ', ToString(Contains(trimmed, 'Engine'))));
Print(Concat('Index of "FunctEngine": ', ToString(IndexOf(trimmed, 'FunctEngine'))));

var words = Split(trimmed, ' ');
Print(Concat('Word count: ', ToString(ArrayLength(words))));

var padded = PadLeft(ToString(42), 6, '0');
Print(Concat('Padded: ', padded));`
            }
        ]
    },
    {
        name: 'Statistics', icon: markRaw(FlaskConical),
        snippets: [
            {
                name: 'Descriptive Statistics',
                description: 'Mean, median, mode, std dev, and a five-number summary chart',
                code: `// Statistics: descriptive summary of a dataset
var data = Array(12, 15, 14, 10, 18, 21, 14, 16, 13, 19, 22, 17, 15, 20, 11);

Print(Concat('Count: ', ToString(Count(data))));
Print(Concat('Mean: ', ToString(Mean(data))));
Print(Concat('Median: ', ToString(Median(data))));
Print(Concat('Mode: ', ToString(Mode(data))));
Print(Concat('Std Dev: ', ToString(StandardDeviation(data))));

Chart('bar', Array('Min', 'Q1', 'Median', 'Q3', 'Max'),
    Array(Percentile(data, 0), Percentile(data, 25), Median(data), Percentile(data, 75), Percentile(data, 100)),
    'Five-Number Summary');`
            },
            {
                name: 'Correlation',
                description: 'Pearson correlation between two variables, plotted as a scatter chart',
                code: `// Statistics: Pearson correlation between two variables
var hoursStudied = Array(1, 2, 3, 4, 5, 6, 7, 8);
var examScore    = Array(52, 58, 63, 65, 70, 74, 80, 85);

var r = Correlation(hoursStudied, examScore);
Print(Concat('Correlation (r): ', ToString(r)));

Chart('scatter', hoursStudied, examScore, 'Exam Score vs. Hours Studied');`
            },
            {
                name: 'One-Way ANOVA',
                description: 'Compare means across groups: F-statistic, group table, and fitted-vs-actual chart',
                code: `// Statistics: One-Way ANOVA — compare crop yield (kg) across 3 fertilizer
// treatments. Built from Array()/Mean()/Variance() primitives since ANOVA
// isn't exposed as a single built-in function.
var groupA = Array(20.1, 21.4, 19.8, 22.0, 20.6); // Fertilizer A
var groupB = Array(23.5, 24.1, 22.9, 23.8, 24.6); // Fertilizer B
var groupC = Array(19.2, 18.7, 20.1, 19.5, 18.9); // Fertilizer C
var groupNames = Array('Fertilizer A', 'Fertilizer B', 'Fertilizer C');

// Matrix of groups — one row per treatment
var groups = Array(groupA, groupB, groupC);
var k = ArrayLength(groups);

// Combine all observations to compute the grand mean
var allValues = Array();
for (var g = 0; g < k; g = g + 1) {
    var group = ArrayGet(groups, g);
    var n = ArrayLength(group);
    for (var j = 0; j < n; j = j + 1) {
        ArrayPush(allValues, ArrayGet(group, j));
    }
}
var grandMean = Mean(allValues);
var totalN = ArrayLength(allValues);

// Between-groups (SSB) and within-groups (SSW) sums of squares
var ssBetween = 0;
var ssWithin = 0;
var groupMeans = Array();
var tableRows = Array();

for (var g = 0; g < k; g = g + 1) {
    var group = ArrayGet(groups, g);
    var n = ArrayLength(group);
    var groupMean = Mean(group);
    var groupVariance = Variance(group); // population variance = SS / n

    var diff = groupMean - grandMean;
    ssBetween = ssBetween + n * diff * diff;
    ssWithin = ssWithin + groupVariance * n;

    ArrayPush(groupMeans, groupMean);
    ArrayPush(tableRows, Array(ArrayGet(groupNames, g), n, Round(groupMean * 100) / 100, Round(groupVariance * 100) / 100));
}

var dfBetween = k - 1;
var dfWithin = totalN - k;
var msBetween = ssBetween / dfBetween;
var msWithin = ssWithin / dfWithin;
var fStatistic = msBetween / msWithin;

Print(Concat('Grand mean: ', ToString(grandMean)));
Print(Concat('SS between: ', ToString(ssBetween), '  SS within: ', ToString(ssWithin)));
Print(Concat('df between: ', ToString(dfBetween), '  df within: ', ToString(dfWithin)));
Print(Concat('MS between: ', ToString(msBetween), '  MS within: ', ToString(msWithin)));
Print(Concat('F statistic: ', ToString(fStatistic)));

// Descriptive table: one row per treatment (Group, N, Mean, Variance)
Table(tableRows, 'ANOVA Group Statistics (Group, N, Mean, Variance)');

// Compare group means visually
Chart('bar', groupNames, groupMeans, 'Mean Yield by Fertilizer Treatment');

// "Adjusted predictors": the ANOVA model's prediction for each observation
// is simply its own group's mean — chart actual vs. that fitted value.
var obsLabels = Array();
var actualValues = Array();
var fittedValues = Array();
for (var g = 0; g < k; g = g + 1) {
    var group = ArrayGet(groups, g);
    var n = ArrayLength(group);
    var groupMean = ArrayGet(groupMeans, g);
    for (var j = 0; j < n; j = j + 1) {
        ArrayPush(obsLabels, Concat(ArrayGet(groupNames, g), ' #', ToString(j + 1)));
        ArrayPush(actualValues, ArrayGet(group, j));
        ArrayPush(fittedValues, groupMean);
    }
}
Chart('line', obsLabels, Array(actualValues, fittedValues), 'Actual vs. Fitted (Group Mean) Values');

StatReport(Concat('One-Way ANOVA: F(', ToString(dfBetween), ', ', ToString(dfWithin), ') = ', ToString(fStatistic)));`
            }
        ]
    },
    {
        name: 'Output', icon: markRaw(BarChart2),
        snippets: [
            {
                name: 'Table Output',
                description: 'Render row/column data as a Table widget',
                code: `// Output: render structured data with Table()
var rows = Array(
    Array('Alice', 29, 'Engineering'),
    Array('Bob', 34, 'Sales'),
    Array('Carol', 41, 'Marketing')
);
Table(rows, 'Employee Directory');`
            },
            {
                name: 'Chart Output',
                description: 'Single-series and multi-series charts',
                code: `// Output: single-series and multi-series charts
var months = Array('Jan', 'Feb', 'Mar', 'Apr', 'May');
var revenue = Array(12000, 15000, 14000, 18000, 21000);
Chart('line', months, revenue, 'Monthly Revenue');

var costs = Array(8000, 9000, 8500, 10000, 11000);
Chart('bar', months, Array(revenue, costs), 'Revenue vs. Costs');`
            },
            {
                name: 'StatReport Output',
                description: 'A titled report panel — pair with Table()/Chart() for the data',
                code: `// Output: StatReport renders a titled panel. Structured sections require
// a result dictionary (e.g. from a native statistics function); from plain
// script the simplest reliable form is a title-only summary, paired with
// Table()/Chart() for the actual data.
StatReport('Quarterly Review');
Table(Array(Array('Q1', 41000), Array('Q2', 47000), Array('Q3', 52000)), 'Revenue by Quarter');`
            }
        ]
    },
    {
        name: 'Database', icon: markRaw(Database),
        snippets: [
            {
                name: 'Query a Connection',
                description: 'Run SQL against a connection configured in this project',
                code: `// Database: query a connection already configured in this project's
// Database Connections panel — replace with its name or id.
var rows = ExecuteQuery('my-connection-name', 'SELECT name, amount FROM sales ORDER BY amount DESC LIMIT 10');
Table(rows, 'Top 10 Sales');`
            },
            {
                name: 'Saved Script & Dataset',
                description: 'Reuse a saved SQL script and a saved Dataset by name',
                code: `// Database: reuse work already saved elsewhere in the project
var salesRows = ExecuteScript('My Saved SQL Script'); // runs a saved SQL script by name
var importedRows = ReadDataset('My Dataset');         // loads a saved Dataset by name
Table(salesRows, 'From Saved SQL Script');
Table(importedRows, 'From Saved Dataset');`
            },
            {
                name: 'Extract a Column as an Array (GetColumn)',
                description: 'Pull one column out of a row-list (e.g. from ReadDataset) into a flat array',
                code: `// Database: GetColumn(rows, columnName) turns one column of a row-list --
// the same shape ExecuteQuery/ExecuteScript/ReadDataset/ReadSpreadsheet all
// return -- into a flat array, ready for Sum()/Mean()/Chart()/etc.
//
// var rows = ReadDataset('My Dataset');
// var columnA = GetColumn(rows, 'A');

// Self-contained demo: build rows in that same shape (MakeRow is from the
// DataTable Functions category) so this snippet runs standalone.
var columns = Array('A', 'B');
var rows = Array();
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array(10, 100)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array(20, 200)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array(30, 300)));

var columnA = GetColumn(rows, 'A');
Print(Concat('Column A: [', ArrayJoin(columnA, ', '), ']'));
Print(Concat('Sum: ', ToString(Sum(columnA)), ', Mean: ', ToString(Mean(columnA))));

Chart('bar', Array('Row 1', 'Row 2', 'Row 3'), columnA, 'Column A');`
            }
        ]
    },
    {
        name: 'Memory', icon: markRaw(Save),
        snippets: [
            {
                name: 'Counters & Variables',
                description: 'Named counters and persisted script variables',
                code: `// Memory: named counters and persisted script variables
var visits = Counter('page_views');
Print(Concat('Page views so far: ', ToString(visits)));

SaveToVar('lastRun', ToString(Random()));
Print(Concat('Last run token: ', LoadFromVar('lastRun')));

MaxVar('highScore', 87);
MaxVar('highScore', 95);
MaxVar('highScore', 62);
Print(Concat('High score: ', LoadFromVar('highScore')));`
            }
        ]
    },
    {
        // Every function is registered under a dotted "Type.Method" name (e.g.
        // DateTimeLibrary.Datediff), which the parser now supports calling.
        name: 'Date & Time', icon: markRaw(Calendar),
        snippets: [
            {
                name: 'Current Date & Time',
                description: 'The current moment in several forms, and its components',
                code: `// Date & Time: current moment in several forms and its components
Print(Concat('Date(): ', DateTimeLibrary.Date()));
Print(Concat('Date(yyyy-MM): ', DateTimeLibrary.Date('yyyy-MM')));
Print(Concat('Now(): ', DateTimeLibrary.Now()));
Print(Concat('UtcNow(): ', DateTimeLibrary.UtcNow()));

var now = DateTimeLibrary.Now();
Print(Concat('Year: ', ToString(DateTimeLibrary.Year(now))));
Print(Concat('Month: ', ToString(DateTimeLibrary.Month(now))));
Print(Concat('Day: ', ToString(DateTimeLibrary.Day(now))));
Print(Concat('Hour: ', ToString(DateTimeLibrary.Hour(now))));
Print(Concat('Minute: ', ToString(DateTimeLibrary.Minute(now))));
Print(Concat('Second: ', ToString(DateTimeLibrary.Second(now))));
Print(Concat('WeekDay (0=Sun): ', ToString(DateTimeLibrary.WeekDay(now))));
Print(Concat('Quarter: ', ToString(DateTimeLibrary.Quarter(now))));
Print(Concat('WeekNum: ', ToString(DateTimeLibrary.WeekNum(now))));

var today = DateTimeLibrary.Today();
Print(Concat('Today(): ', ToString(DateTimeLibrary.Year(today)), '-', ToString(DateTimeLibrary.Month(today)), '-', ToString(DateTimeLibrary.Day(today))));`
            },
            {
                name: 'Date Arithmetic & Parsing',
                description: 'Parse dates/times from text and compute differences',
                code: `// Date & Time: parse dates/times from text and compute differences
var start = DateTimeLibrary.Datevalue('2024-01-01');
var end = DateTimeLibrary.Datevalue('2024-09-15');
Print(Concat('Days between: ', ToString(DateTimeLibrary.Datediff(start, end))));
Print(Concat('Years between: ', ToString(DateTimeLibrary.YearFrac(start, end))));

var meetingTime = DateTimeLibrary.Time(14, 30, 0);
Print(Concat('Meeting hour: ', ToString(DateTimeLibrary.Hour(meetingTime)), ':', ToString(DateTimeLibrary.Minute(meetingTime))));

var parsedTime = DateTimeLibrary.Timevalue('09:15:45');
Print(Concat('Parsed seconds: ', ToString(DateTimeLibrary.Second(parsedTime))));`
            }
        ]
    },
    {
        name: 'Design of Experiments', icon: markRaw(TestTube),
        snippets: [
            {
                name: 'Regression',
                description: 'Simple, multiple, and GLM linear regression',
                code: `// DOE: simple and multiple linear regression
var xs = Array(1, 2, 3, 4, 5);
var ys = Array(2.1, 3.9, 6.2, 7.8, 10.1);
var simple = DoeLibrary.LinearRegression(xs, ys);
Print(Concat('slope=', ToString(ArrayGet(simple, 0)), ' intercept=', ToString(ArrayGet(simple, 1)), ' R2=', ToString(ArrayGet(simple, 2))));

// Multiple regression: y from two predictors
var predictors = Array(
    Array(1, 50), Array(2, 52), Array(3, 65), Array(4, 68), Array(5, 80), Array(6, 78), Array(7, 90), Array(8, 95)
);
var targets = Array(21, 25, 30, 34, 39, 41, 47, 52);
var coefficients = DoeLibrary.MultipleLinearRegression(predictors, targets);
Print(Concat('Intercept: ', ToString(ArrayGet(coefficients, 0)), '  Coef 1: ', ToString(ArrayGet(coefficients, 1)), '  Coef 2: ', ToString(ArrayGet(coefficients, 2))));

// Glm defaults to a Gaussian family (equivalent to MultipleLinearRegression),
// and also returns the model's overall F-test and R-squared -- fStatistic/pValue
// are NaN for non-Gaussian families, where an F-test isn't statistically valid.
var glm = DoeLibrary.Glm(predictors, targets);
var glmCoefficients = ArrayGet(glm, 0);
var glmF = ArrayGet(glm, 1);
var glmP = ArrayGet(glm, 2);
Print(Concat('GLM intercept: ', ToString(ArrayGet(glmCoefficients, 0))));
Print(Concat('GLM model F=', ToString(glmF), ' p=', ToString(glmP), ' R2=', ToString(ArrayGet(glm, 5)), ' -- significant: ', ToString(glmP < 0.05)));`
            },
            {
                name: 'Native ANOVA & MANOVA (F-Tables & Significance)',
                description: 'Call the library\'s own ANOVA/MANOVA implementations and display their full F-tables',
                code: `// DOE: run the built-in ANOVA/MANOVA implementations directly and display
// their full F-tables (SS, df, MS, F, p) to judge statistical significance.
// (See the Statistics category's "One-Way ANOVA" snippet for how this is
// computed from scratch.)
var groupA = Array(20.1, 21.4, 19.8, 22.0, 20.6);
var groupB = Array(23.5, 24.1, 22.9, 23.8, 24.6);
var groupC = Array(19.2, 18.7, 20.1, 19.5, 18.9);
var oneWay = DoeLibrary.OneWayAnova(Array(groupA, groupB, groupC));

var oneWayF = ArrayGet(oneWay, 0);
var oneWayP = ArrayGet(oneWay, 1);
var oneWayTable = Array(
    Array('Between Groups', ArrayGet(oneWay, 2), ArrayGet(oneWay, 4), ArrayGet(oneWay, 6), oneWayF, oneWayP, oneWayP < 0.05),
    Array('Within Groups', ArrayGet(oneWay, 3), ArrayGet(oneWay, 5), ArrayGet(oneWay, 7), '', '', '')
);
Table(oneWayTable, 'One-Way ANOVA F-Table (Source, SS, df, MS, F, p, Significant)');
Print(Concat('One-Way ANOVA -- F=', ToString(oneWayF), ' p=', ToString(oneWayP), ' significant=', ToString(oneWayP < 0.05)));

// Two-Way ANOVA: 2 fertilizer levels x 2 soil levels, 3 replicates each
var data = Array(
    Array(20.0), Array(21.0), Array(19.5),
    Array(24.0), Array(23.5), Array(24.5),
    Array(18.0), Array(17.5), Array(18.5),
    Array(22.0), Array(21.5), Array(22.5)
);
var factorA = Array(0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1);
var factorB = Array(0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1);
var twoWay = DoeLibrary.TwoWayAnova(data, factorA, factorB);

var fA = ArrayGet(twoWay, 0); var fB = ArrayGet(twoWay, 1); var fInt = ArrayGet(twoWay, 2);
var pA = ArrayGet(twoWay, 3); var pB = ArrayGet(twoWay, 4); var pInt = ArrayGet(twoWay, 5);
var twoWayTable = Array(
    Array('Factor A', ArrayGet(twoWay, 6), ArrayGet(twoWay, 11), ArrayGet(twoWay, 15), fA, pA, pA < 0.05),
    Array('Factor B', ArrayGet(twoWay, 7), ArrayGet(twoWay, 12), ArrayGet(twoWay, 16), fB, pB, pB < 0.05),
    Array('A x B Interaction', ArrayGet(twoWay, 8), ArrayGet(twoWay, 13), ArrayGet(twoWay, 17), fInt, pInt, pInt < 0.05),
    Array('Error', ArrayGet(twoWay, 9), ArrayGet(twoWay, 14), ArrayGet(twoWay, 18), '', '', ''),
    Array('Total', ArrayGet(twoWay, 10), '', '', '', '', '')
);
Table(twoWayTable, 'Two-Way ANOVA F-Table (Source, SS, df, MS, F, p, Significant)');
Print(Concat('Two-Way ANOVA -- F(A)=', ToString(fA), ' p(A)=', ToString(pA), '  F(B)=', ToString(fB), ' p(B)=', ToString(pB), '  F(AxB)=', ToString(fInt), ' p(AxB)=', ToString(pInt)));

// MANOVA: 2 groups, 3 observations each, 2 dependent variables per observation.
// Significance comes from Rao's F-approximation of Wilks' Lambda (exact when
// there's 1 dependent variable or 2 groups, as in this example).
var group1 = Array(Array(10.0, 5.0), Array(12.0, 6.0), Array(11.0, 5.5));
var group2 = Array(Array(15.0, 8.0), Array(16.0, 8.5), Array(14.5, 7.8));
var manova = DoeLibrary.Manova(Array(group1, group2));

var manovaF = ArrayGet(manova, 2);
var manovaP = ArrayGet(manova, 3);
var manovaTable = Array(
    Array('Wilks Lambda', ArrayGet(manova, 0)),
    Array('Pillai Trace', ArrayGet(manova, 1)),
    Array('F (Rao approx.)', manovaF),
    Array('df1', ArrayGet(manova, 4)),
    Array('df2', ArrayGet(manova, 5)),
    Array('p-value', manovaP),
    Array('Significant', manovaP < 0.05)
);
Table(manovaTable, 'MANOVA Summary');
Print(Concat('MANOVA -- Wilks Lambda=', ToString(ArrayGet(manova, 0)), ' F=', ToString(manovaF), ' p=', ToString(manovaP), ' significant=', ToString(manovaP < 0.05)));`
            },
            {
                name: 'Hypothesis Tests',
                description: 't-tests, chi-square, Mann-Whitney U, and confidence intervals',
                code: `// DOE: common hypothesis tests
var groupA = Array(23, 25, 22, 27, 24);
var groupB = Array(30, 32, 29, 31, 33);
var tTest = DoeLibrary.IndependentTTest(groupA, groupB);
Print(Concat('Independent t-test -- t=', ToString(ArrayGet(tTest, 0)), ' p=', ToString(ArrayGet(tTest, 2))));

var before = Array(150, 155, 148, 160, 152);
var after = Array(145, 150, 146, 155, 148);
var pairedTest = DoeLibrary.PairedTTest(before, after);
Print(Concat('Paired t-test -- t=', ToString(ArrayGet(pairedTest, 0)), ' p=', ToString(ArrayGet(pairedTest, 2))));

var observed = Array(18, 22, 20, 40);
var expected = Array(25, 25, 25, 25);
var chiSquare = DoeLibrary.ChiSquareTest(observed, expected);
Print(Concat('Chi-square -- stat=', ToString(ArrayGet(chiSquare, 0)), ' p=', ToString(ArrayGet(chiSquare, 2))));

var mannWhitney = DoeLibrary.MannWhitneyU(groupA, groupB);
Print(Concat('Mann-Whitney U -- U=', ToString(ArrayGet(mannWhitney, 0)), ' p=', ToString(ArrayGet(mannWhitney, 1))));

var sample = Array(12, 15, 14, 10, 18, 21, 14, 16, 13, 19);
var ci = DoeLibrary.ConfidenceInterval(sample);
Print(Concat('95% CI -- [', ToString(ArrayGet(ci, 0)), ', ', ToString(ArrayGet(ci, 1)), ']  mean=', ToString(ArrayGet(ci, 2))));`
            },
            {
                name: 'Post-Hoc & Effect Size',
                description: 'Tukey HSD, Bonferroni correction, Cohen\'s d, eta-squared',
                code: `// DOE: post-hoc pairwise comparisons and effect sizes
var groupA = Array(20.1, 21.4, 19.8, 22.0, 20.6);
var groupB = Array(23.5, 24.1, 22.9, 23.8, 24.6);
var groupC = Array(19.2, 18.7, 20.1, 19.5, 18.9);
var names = Array('A', 'B', 'C');
var pairs = DoeLibrary.TukeyHSD(Array(groupA, groupB, groupC), names);
for (var i = 0; i < ArrayLength(pairs); i = i + 1) {
    var pair = ArrayGet(pairs, i);
    Print(Concat(GetRowValue(pair, 'groupA'), ' vs ', GetRowValue(pair, 'groupB'), ': meanDiff=', ToString(GetRowValue(pair, 'meanDiff')), ' significant=', ToString(GetRowValue(pair, 'significant'))));
}

var pValues = Array(0.01, 0.04, 0.03, 0.20);
var corrected = DoeLibrary.BonferroniCorrection(pValues);
Print(Concat('Bonferroni-corrected p-values: ', ArrayJoin(corrected, ', ')));

var cohensD = DoeLibrary.CohenD(groupA, groupB);
Print(Concat('Cohen D (A vs B): ', ToString(cohensD)));

var etaSq = DoeLibrary.EtaSquared(52.5, 58.66);
Print(Concat('Eta-squared: ', ToString(etaSq)));`
            },
            {
                name: 'Descriptive Stats & Gage R&R',
                description: 'The library\'s own Mean/Variance/StdDev/Correlation, plus Gage R&R',
                code: `// DOE: the library's own Mean/Variance/StandardDeviation/Correlation (equivalent
// to the plain built-ins already shown in the Statistics category) plus GageRR
var data = Array(12, 15, 14, 10, 18, 21, 14, 16, 13, 19);
Print(Concat('DoeLibrary.Mean: ', ToString(DoeLibrary.Mean(data))));
Print(Concat('DoeLibrary.Variance: ', ToString(DoeLibrary.Variance(data))));
Print(Concat('DoeLibrary.StandardDeviation: ', ToString(DoeLibrary.StandardDeviation(data))));

var x = Array(1, 2, 3, 4, 5);
var y = Array(2, 4, 5, 4, 5);
Print(Concat('DoeLibrary.Correlation: ', ToString(DoeLibrary.Correlation(x, y))));

// Gage R&R: 3 parts, 2 operators, 2 replicates each
var measurements = Array(
    Array(Array(10.1, 10.3), Array(10.0, 10.2)),
    Array(Array(12.2, 12.0), Array(12.1, 12.3)),
    Array(Array(9.8, 9.9), Array(9.7, 10.0))
);
var gageRR = DoeLibrary.GageRR(measurements);
Print(Concat('Repeatability: ', ToString(ArrayGet(gageRR, 0)), ' Reproducibility: ', ToString(ArrayGet(gageRR, 1)), ' Gage R&R: ', ToString(ArrayGet(gageRR, 2)), ' Part Variation: ', ToString(ArrayGet(gageRR, 3))));`
            },
            {
                name: 'Gage R&R -- Full Study with Charts',
                description: 'All 6 standard Gage R&R charts: Components of Variation, X-Bar/R by Operator, Measurements by Part/Operator, and Interaction',
                code: `// DOE: Gage R&R -- full measurement systems analysis with all 6 standard charts
// (Components of Variation, X-Bar by Operator, R Chart by Operator,
// Measurements by Part, Measurements by Operator, Operator x Part Interaction)
var measurements = Array(
    Array(Array(10.02, 9.98, 10.05), Array(10.15, 10.20, 10.10), Array(9.92, 9.88, 9.95)),
    Array(Array(12.05, 11.95, 12.00), Array(12.20, 12.15, 12.25), Array(11.90, 11.85, 11.95)),
    Array(Array(9.52, 9.48, 9.55), Array(9.65, 9.70, 9.60), Array(9.42, 9.38, 9.45)),
    Array(Array(11.02, 10.98, 11.05), Array(11.18, 11.12, 11.20), Array(10.90, 10.95, 10.88)),
    Array(Array(10.52, 10.48, 10.55), Array(10.65, 10.60, 10.70), Array(10.40, 10.45, 10.38))
);
var partLabels = Array('Part 1', 'Part 2', 'Part 3', 'Part 4', 'Part 5');
var operatorLabels = Array('Operator A', 'Operator B', 'Operator C');
var numParts = ArrayLength(measurements);
var numOperators = ArrayLength(operatorLabels);

// GageRR now returns: repeatability, reproducibility, gageRR, partVariation,
// percentContribution[4], xBarCenter, xBarUcl, xBarLcl, rCenter, rUcl, rLcl
var gageRR = DoeLibrary.GageRR(measurements);
var repeatability = ArrayGet(gageRR, 0);
var reproducibility = ArrayGet(gageRR, 1);
var totalGageRR = ArrayGet(gageRR, 2);
var partVariation = ArrayGet(gageRR, 3);
var percentContribution = ArrayGet(gageRR, 4);
var xBarCenter = ArrayGet(gageRR, 5);
var xBarUcl = ArrayGet(gageRR, 6);
var xBarLcl = ArrayGet(gageRR, 7);
var rCenter = ArrayGet(gageRR, 8);
var rUcl = ArrayGet(gageRR, 9);
var rLcl = ArrayGet(gageRR, 10);

Print(Concat('Repeatability (EV): ', ToString(repeatability), '  Reproducibility (AV): ', ToString(reproducibility)));
Print(Concat('Total Gage R&R: ', ToString(totalGageRR), '  Part Variation: ', ToString(partVariation)));

// 1. Components of Variation (Bar Chart) -- % contribution of each source
Chart('bar', Array('Repeatability', 'Reproducibility', 'Gage R&R', 'Part-to-Part'), percentContribution, 'Components of Variation (% Contribution)');

// Per-operator subgroup means and ranges by part (used by charts 2, 3 and 6)
var opMeansByOperator = Array();
var opRangesByOperator = Array();
for (var o = 0; o < numOperators; o = o + 1) {
    var meansForOp = Array();
    var rangesForOp = Array();
    for (var p = 0; p < numParts; p = p + 1) {
        var replicates = ArrayGet(ArrayGet(measurements, p), o);
        ArrayPush(meansForOp, Mean(replicates));
        ArrayPush(rangesForOp, Range(replicates));
    }
    ArrayPush(opMeansByOperator, meansForOp);
    ArrayPush(opRangesByOperator, rangesForOp);
}

// 2. X-Bar Chart by Operator -- subgroup means per part, one series per operator
Chart('line', partLabels, opMeansByOperator, 'X-Bar Chart by Operator');
Print(Concat('X-Bar control limits -- center: ', ToString(xBarCenter), ', UCL: ', ToString(xBarUcl), ', LCL: ', ToString(xBarLcl)));

// 3. R Chart (Range Chart) by Operator -- subgroup ranges per part, one series per operator
Chart('line', partLabels, opRangesByOperator, 'R Chart by Operator');
Print(Concat('R chart control limits -- center: ', ToString(rCenter), ', UCL: ', ToString(rUcl), ', LCL: ', ToString(rLcl)));

// 4. Measurements by Part -- every individual reading, grouped by part
var partPointLabels = Array();
var partPointValues = Array();
for (var p = 0; p < numParts; p = p + 1) {
    for (var o = 0; o < numOperators; o = o + 1) {
        var replicates = ArrayGet(ArrayGet(measurements, p), o);
        for (var r = 0; r < ArrayLength(replicates); r = r + 1) {
            ArrayPush(partPointLabels, Concat(ArrayGet(partLabels, p), ' / ', ArrayGet(operatorLabels, o)));
            ArrayPush(partPointValues, ArrayGet(replicates, r));
        }
    }
}
Chart('scatter', partPointLabels, partPointValues, 'Measurements by Part');

// 5. Measurements by Operator -- every individual reading, grouped by operator
var opPointLabels = Array();
var opPointValues = Array();
for (var o = 0; o < numOperators; o = o + 1) {
    for (var p = 0; p < numParts; p = p + 1) {
        var replicates = ArrayGet(ArrayGet(measurements, p), o);
        for (var r = 0; r < ArrayLength(replicates); r = r + 1) {
            ArrayPush(opPointLabels, Concat(ArrayGet(operatorLabels, o), ' / ', ArrayGet(partLabels, p)));
            ArrayPush(opPointValues, ArrayGet(replicates, r));
        }
    }
}
Chart('scatter', opPointLabels, opPointValues, 'Measurements by Operator');

// 6. Operator x Part Interaction -- same subgroup means as the X-bar chart, without
// control limits: parallel lines mean no interaction; crossing lines mean operators
// rank parts differently
Chart('line', partLabels, opMeansByOperator, 'Operator x Part Interaction');`
            }
        ]
    },
    {
        name: 'Financial', icon: markRaw(DollarSign),
        snippets: [
            {
                name: 'Time Value of Money',
                description: 'Fv, Pv, Pmt, Nper, Rate for a simple loan',
                code: `// Financial: time value of money for a $10,000 loan, 60 months at 0.5%/month
var monthlyRate = 0.005;
var nper = 60;
var loanAmount = 10000;

var payment = FinancialLibrary.Pmt(monthlyRate, nper, loanAmount);
Print(Concat('Monthly payment: ', ToString(payment)));

var futureValue = FinancialLibrary.Fv(monthlyRate, nper, payment, loanAmount);
Print(Concat('Future value (should be ~0 if paid off): ', ToString(futureValue)));

var presentValue = FinancialLibrary.Pv(monthlyRate, nper, payment);
Print(Concat('Present value implied by payment: ', ToString(presentValue)));

var periodsNeeded = FinancialLibrary.Nper(monthlyRate, payment, loanAmount);
Print(Concat('Periods to pay off: ', ToString(periodsNeeded)));

var impliedRate = FinancialLibrary.Rate(nper, payment, loanAmount, 0, 0, 0.005);
Print(Concat('Implied monthly rate: ', ToString(impliedRate)));`
            },
            {
                name: 'Loan Amortization',
                description: 'Interest/principal split per period, and cumulative totals',
                code: `// Financial: interest/principal breakdown per period, and cumulative totals
var rate = 0.005;
var nper = 60;
var pv = 10000;

var interestPeriod1 = FinancialLibrary.Ipmt(rate, 1, nper, pv);
var principalPeriod1 = FinancialLibrary.Ppmt(rate, 1, nper, pv);
Print(Concat('Period 1 -- interest: ', ToString(interestPeriod1), ', principal: ', ToString(principalPeriod1)));

var totalInterestYear1 = FinancialLibrary.Cumipmt(rate, nper, pv, 1, 12, 0);
var totalPrincipalYear1 = FinancialLibrary.Cumprinc(rate, nper, pv, 1, 12, 0);
Print(Concat('Year 1 -- total interest: ', ToString(totalInterestYear1), ', total principal: ', ToString(totalPrincipalYear1)));

var straightLineInterest = FinancialLibrary.Ispmt(rate, 1, nper, pv);
Print(Concat('Straight-line interest estimate, period 1: ', ToString(straightLineInterest)));`
            },
            {
                name: 'Depreciation Methods',
                description: 'Compare straight-line, SYD, and declining-balance depreciation',
                code: `// Financial: compare depreciation methods for a $10,000 asset, $1,000 salvage, 5-year life
var cost = 10000;
var salvage = 1000;
var life = 5;

Print(Concat('Straight-line (Sln): ', ToString(FinancialLibrary.Sln(cost, salvage, life))));
Print(Concat('Sum-of-years-digits, year 1 (Syd): ', ToString(FinancialLibrary.Syd(cost, salvage, life, 1))));
Print(Concat('Fixed-declining balance, year 1 (Db): ', ToString(FinancialLibrary.Db(cost, salvage, life, 1))));
Print(Concat('Double-declining balance, year 1 (Ddb): ', ToString(FinancialLibrary.Ddb(cost, salvage, life, 1))));`
            },
            {
                name: 'Interest Rate Conversion & Growth',
                description: 'Effect, Nominal, Rri, and Pduration',
                code: `// Financial: convert between nominal/effective rates, and solve for growth
var effectiveRate = FinancialLibrary.Effect(0.06, 12);
Print(Concat('Effective annual rate from 6% nominal (monthly compounding): ', ToString(effectiveRate)));

var nominalRate = FinancialLibrary.Nominal(effectiveRate, 12);
Print(Concat('Nominal rate recovered: ', ToString(nominalRate)));

var growthRate = FinancialLibrary.Rri(10, 5000, 8000);
Print(Concat('Equivalent rate for $5,000 -> $8,000 over 10 periods: ', ToString(growthRate)));

var periodsToDouble = FinancialLibrary.Pduration(0.07, 5000, 10000);
Print(Concat('Periods to double $5,000 at 7%: ', ToString(periodsToDouble)));`
            },
            {
                name: 'Cash Flow Analysis (Xnpv/Xirr)',
                description: 'Net present value and internal rate of return for irregular cash flows',
                code: `// Financial: net present value and internal rate of return for irregular cash flows
var values = Array(-10000, 3000, 4200, 6800);
var dates = Array(
    DateTimeLibrary.Datevalue('2024-01-01'),
    DateTimeLibrary.Datevalue('2024-06-01'),
    DateTimeLibrary.Datevalue('2025-01-01'),
    DateTimeLibrary.Datevalue('2025-06-01')
);

var npv = FinancialLibrary.Xnpv(0.1, values, dates);
Print(Concat('XNPV at 10%: ', ToString(npv)));

var irr = FinancialLibrary.Xirr(values, dates);
Print(Concat('XIRR: ', ToString(irr)));`
            },
            {
                name: 'Bond & T-Bill Pricing',
                description: 'Disc, Intrate, Received, Pricedisc, Tbillprice, TbillYield, Tbilleq',
                code: `// Financial: bond and T-bill pricing between a settlement and maturity date
var settlement = DateTimeLibrary.Datevalue('2024-01-01');
var maturity = DateTimeLibrary.Datevalue('2024-07-01');

var discountRate = FinancialLibrary.Disc(settlement, maturity, 98, 100);
Print(Concat('Discount rate (Disc): ', ToString(discountRate)));

var investRate = FinancialLibrary.Intrate(settlement, maturity, 98, 100);
Print(Concat('Interest rate (Intrate): ', ToString(investRate)));

var receivedAmount = FinancialLibrary.Received(settlement, maturity, 98, 0.04);
Print(Concat('Amount received at maturity (Received): ', ToString(receivedAmount)));

var priceDiscounted = FinancialLibrary.Pricedisc(settlement, maturity, 0.04, 100);
Print(Concat('Price per $100 face value (Pricedisc): ', ToString(priceDiscounted)));

var tbillPrice = FinancialLibrary.Tbillprice(settlement, maturity, 0.04);
Print(Concat('T-bill price (Tbillprice): ', ToString(tbillPrice)));

var tbillYield = FinancialLibrary.TbillYield(settlement, maturity, tbillPrice);
Print(Concat('T-bill yield (TbillYield): ', ToString(tbillYield)));

var tbillEquivalent = FinancialLibrary.Tbilleq(settlement, maturity, 0.04);
Print(Concat('Bond-equivalent yield (Tbilleq): ', ToString(tbillEquivalent)));`
            },
            {
                name: 'Fractional Price Conversion',
                description: 'Convert between fractional and decimal price quotes (e.g. bond ticks)',
                code: `// Financial: convert between fractional and decimal price quotes (e.g. bond ticks)
var decimalPrice = FinancialLibrary.Dollarde(1.10, 32);
Print(Concat('Dollarde(1.10, 32): ', ToString(decimalPrice)));

var fractionalPrice = FinancialLibrary.Dollarfr(decimalPrice, 32);
Print(Concat('Dollarfr back to fraction: ', ToString(fractionalPrice)));`
            }
        ]
    },
    {
        name: 'Time Series', icon: markRaw(TrendingUp),
        snippets: [
            {
                name: 'Autocorrelation (ACF & PACF)',
                description: 'Autocorrelation and partial autocorrelation functions, with lag-bar charts',
                code: `// Time Series: autocorrelation (ACF) and partial autocorrelation (PACF)
var series = Array(10.2, 10.8, 10.5, 11.1, 11.6, 11.4, 12.0, 12.5, 12.3, 12.9, 13.4, 13.2, 13.8, 14.3, 14.1, 14.7, 15.2, 15.0, 15.6, 16.1);
var maxLag = 5;

var acfValues = TimeSeriesLibrary.Acf(series, maxLag);
var pacfValues = TimeSeriesLibrary.Pacf(series, maxLag);

var lags = Array();
var acfRows = Array();
var pacfRows = Array();
for (var k = 0; k <= maxLag; k = k + 1) {
    ArrayPush(lags, Concat('Lag ', ToString(k)));
    ArrayPush(acfRows, Array(k, Round(ArrayGet(acfValues, k) * 1000) / 1000));
    ArrayPush(pacfRows, Array(k, Round(ArrayGet(pacfValues, k) * 1000) / 1000));
}

Table(acfRows, 'ACF (Lag, Value)');
Table(pacfRows, 'PACF (Lag, Value)');
Chart('bar', lags, acfValues, 'Autocorrelation Function (ACF)');
Chart('bar', lags, pacfValues, 'Partial Autocorrelation Function (PACF)');`
            },
            {
                name: 'Smoothing & Transformations',
                description: 'Moving average, exponential smoothing, and Box-Cox / log transforms',
                code: `// Time Series: moving average, exponential smoothing, and Box-Cox / log transforms
var rawSeries = Array(100, 105, 98, 110, 115, 108, 120, 125, 118, 130, 135, 128, 140, 145, 138);

var ma = TimeSeriesLibrary.MovingAverage(rawSeries, 3);
var smoothed = TimeSeriesLibrary.ExponentialSmoothing(rawSeries, 0.3);
var boxCox = TimeSeriesLibrary.BoxCoxTransform(rawSeries, 0.5);
var logged = TimeSeriesLibrary.LogTransform(rawSeries);

var labels = Array();
for (var i = 0; i < ArrayLength(rawSeries); i = i + 1) {
    ArrayPush(labels, Concat('t', ToString(i + 1)));
}
Chart('line', labels, rawSeries, 'Original Series');
Chart('line', labels, smoothed, 'Exponential Smoothing (alpha=0.3)');

var maLabels = Array();
for (var i = 0; i < ArrayLength(ma); i = i + 1) {
    ArrayPush(maLabels, Concat('t', ToString(i + 3)));
}
Chart('line', maLabels, ma, 'Moving Average (window=3)');

Print(Concat('Box-Cox(lambda=0.5) first value: ', ToString(ArrayGet(boxCox, 0))));
Print(Concat('Log transform first value: ', ToString(ArrayGet(logged, 0))));`
            },
            {
                name: 'Residual Diagnostics (Ljung-Box & Durbin-Watson)',
                description: 'Test for autocorrelation in a series and in regression residuals',
                code: `// Time Series: Ljung-Box test for autocorrelation, and Durbin-Watson for
// first-order autocorrelation in regression residuals
var series = Array(10.2, 10.8, 10.5, 11.1, 11.6, 11.4, 12.0, 12.5, 12.3, 12.9, 13.4, 13.2, 13.8, 14.3, 14.1, 14.7, 15.2, 15.0, 15.6, 16.1);

var ljungBox = TimeSeriesLibrary.LjungBoxTest(series, 5);
Print(Concat('Ljung-Box Q = ', ToString(ArrayGet(ljungBox, 0)), ', df = ', ToString(ArrayGet(ljungBox, 1)), ', p-value = ', ToString(ArrayGet(ljungBox, 2))));

// Fit a simple linear trend (DoeLibrary, another built-in library) and check
// its residuals for leftover autocorrelation
var x = Array(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
var y = Array(2.1, 4.3, 5.8, 8.2, 9.9, 12.3, 13.8, 16.1, 18.2, 20.1);
var fit = DoeLibrary.LinearRegression(x, y);
var slope = ArrayGet(fit, 0);
var intercept = ArrayGet(fit, 1);

var residuals = Array();
for (var i = 0; i < ArrayLength(x); i = i + 1) {
    var predicted = slope * ArrayGet(x, i) + intercept;
    ArrayPush(residuals, ArrayGet(y, i) - predicted);
}

var dw = TimeSeriesLibrary.DurbinWatsonTest(residuals);
Value(dw, 'Durbin-Watson Statistic', '(~2 = no autocorrelation)');`
            },
            {
                name: 'Granger Causality',
                description: 'Test whether one series helps predict another, beyond its own past',
                code: `// Time Series: Granger causality -- does 'cause' help predict 'effect'
// beyond effect's own past values?
var cause = Array(1.2, 0.8, 1.5, 0.9, 1.7, 1.1, 1.9, 1.0, 2.1, 1.3, 2.0, 1.4, 2.2, 1.6, 2.4, 1.8, 2.5, 1.9, 2.6, 2.0);
var effect = Array(0.5, 0.3, 2.5, 1.8, 3.1, 2.0, 3.5, 2.3, 3.9, 2.5, 4.2, 2.9, 4.5, 3.2, 4.8, 3.6, 5.1, 3.9, 5.4, 4.1);

var granger = TimeSeriesLibrary.GrangerCausalityTest(cause, effect, 2);
Print(Concat('F statistic = ', ToString(ArrayGet(granger, 0))));
Print(Concat('df1 = ', ToString(ArrayGet(granger, 1)), ', df2 = ', ToString(ArrayGet(granger, 2))));
Print(Concat('p-value = ', ToString(ArrayGet(granger, 3))));
Print('A small p-value means cause Granger-causes effect.');`
            },
            {
                name: 'Cointegration (Engle-Granger & Johansen)',
                description: 'Test whether two (or more) non-stationary series share a long-run equilibrium',
                code: `// Time Series: cointegration -- Engle-Granger (two series) and Johansen (N series)
var seriesA = Array(100, 101, 99, 103, 105, 104, 108, 110, 107, 112, 115, 113, 118, 120, 117, 122, 125, 123, 128, 130);
var seriesB = Array();
for (var i = 0; i < ArrayLength(seriesA); i = i + 1) {
    ArrayPush(seriesB, ArrayGet(seriesA, i) * 2.05 + 3);
}

var eg = TimeSeriesLibrary.EngleGrangerTest(seriesB, seriesA);
Print(Concat('Engle-Granger -- alpha=', ToString(ArrayGet(eg, 0)), ' beta=', ToString(ArrayGet(eg, 1))));
Print(Concat('ADF statistic=', ToString(ArrayGet(eg, 2)), ' (5% critical value=', ToString(ArrayGet(eg, 3)), ')'));
Print(Concat('Cointegrated: ', ToString(ArrayGet(eg, 4))));

// Johansen takes a matrix: one row per observation, one column per variable
var matrix = Array();
for (var i = 0; i < ArrayLength(seriesA); i = i + 1) {
    ArrayPush(matrix, Array(ArrayGet(seriesA, i), ArrayGet(seriesB, i)));
}
var johansen = TimeSeriesLibrary.JohansenTest(matrix);
var eigenvalues = ArrayGet(johansen, 0);
var traceStats = ArrayGet(johansen, 1);
Print(Concat('Johansen eigenvalues: ', ToString(ArrayGet(eigenvalues, 0)), ', ', ToString(ArrayGet(eigenvalues, 1))));
Print(Concat('Johansen trace statistics: ', ToString(ArrayGet(traceStats, 0)), ', ', ToString(ArrayGet(traceStats, 1))));`
            }
        ]
    },
    {
        name: 'Non-Parametric Tests', icon: markRaw(Shuffle),
        snippets: [
            {
                name: 'Two-Sample Location Tests (Mann-Whitney & KS)',
                description: 'Compare two independent samples without assuming normality',
                code: `// Non-Parametric: Mann-Whitney U (rank-sum) and two-sample Kolmogorov-Smirnov
var groupA = Array(23, 25, 21, 28, 30, 26, 24, 27);
var groupB = Array(31, 35, 29, 38, 40, 33, 36, 34);

var mw = NonParametricLibrary.MannWhitneyUTest(groupA, groupB);
Print(Concat('Mann-Whitney U = ', ToString(ArrayGet(mw, 0)), ', z = ', ToString(ArrayGet(mw, 1)), ', p-value = ', ToString(ArrayGet(mw, 2))));

var ks2 = NonParametricLibrary.TwoSampleKsTest(groupA, groupB);
Print(Concat('KS D statistic = ', ToString(ArrayGet(ks2, 0)), ', p-value = ', ToString(ArrayGet(ks2, 1))));

Chart('bar', Array('Group A', 'Group B'), Array(Mean(groupA), Mean(groupB)), 'Group Means');`
            },
            {
                name: 'Paired Tests (Wilcoxon Signed-Rank & McNemar)',
                description: 'Compare paired/matched observations -- continuous (Wilcoxon) or binary (McNemar)',
                code: `// Non-Parametric: Wilcoxon signed-rank (paired continuous) and McNemar (paired binary)
var before = Array(68, 72, 65, 70, 75, 69, 71, 74, 66, 73);
var after = Array(65, 70, 66, 68, 71, 67, 69, 70, 64, 70);

var wilcoxon = NonParametricLibrary.WilcoxonSignedRankTest(before, after);
Print(Concat('Wilcoxon W = ', ToString(ArrayGet(wilcoxon, 0)), ', z = ', ToString(ArrayGet(wilcoxon, 1)), ', p-value = ', ToString(ArrayGet(wilcoxon, 2))));

// McNemar: 2x2 table of paired Yes/No outcomes (e.g. before/after a treatment)
var table = Array(Array(45, 15), Array(5, 35));
var mcnemar = NonParametricLibrary.McNemarTest(table);
Print(Concat('McNemar chi-squared = ', ToString(ArrayGet(mcnemar, 0)), ', p-value = ', ToString(ArrayGet(mcnemar, 1))));`
            },
            {
                name: 'Multi-Sample Rank Tests (Kruskal-Wallis & Friedman)',
                description: 'Non-parametric alternatives to one-way and repeated-measures ANOVA',
                code: `// Non-Parametric: Kruskal-Wallis (independent groups) and Friedman (repeated measures)
var diet1 = Array(4.2, 3.8, 4.5, 4.0, 3.9);
var diet2 = Array(5.1, 4.8, 5.3, 5.0, 4.9);
var diet3 = Array(3.5, 3.2, 3.7, 3.4, 3.6);
var groups = Array(diet1, diet2, diet3);

var kruskal = NonParametricLibrary.KruskalWallisTest(groups);
Print(Concat('Kruskal-Wallis H = ', ToString(ArrayGet(kruskal, 0)), ', df = ', ToString(ArrayGet(kruskal, 1)), ', p-value = ', ToString(ArrayGet(kruskal, 2))));

// Friedman: each row is a subject, each column a treatment
var subjects = Array(
    Array(8, 6, 7),
    Array(9, 7, 8),
    Array(7, 5, 6),
    Array(8, 6, 6),
    Array(9, 8, 7)
);
var friedman = NonParametricLibrary.FriedmanTest(subjects);
Print(Concat('Friedman chi-squared = ', ToString(ArrayGet(friedman, 0)), ', df = ', ToString(ArrayGet(friedman, 1)), ', p-value = ', ToString(ArrayGet(friedman, 2))));`
            },
            {
                name: 'Rank Correlation (Spearman & Kendall)',
                description: 'Monotonic association between two variables, robust to outliers and non-linearity',
                code: `// Non-Parametric: Spearman's rho and Kendall's tau
var studyHours = Array(2, 4, 5, 7, 8, 10, 12, 14);
var examScore = Array(55, 60, 68, 70, 78, 82, 88, 95);

var spearman = NonParametricLibrary.SpearmanRankCorrelation(studyHours, examScore);
Print(Concat('Spearman rho = ', ToString(ArrayGet(spearman, 0)), ', p-value = ', ToString(ArrayGet(spearman, 1))));

var kendall = NonParametricLibrary.KendallTau(studyHours, examScore);
Print(Concat('Kendall tau = ', ToString(ArrayGet(kendall, 0)), ', z = ', ToString(ArrayGet(kendall, 1)), ', p-value = ', ToString(ArrayGet(kendall, 2))));

Chart('scatter', studyHours, examScore, 'Exam Score vs. Study Hours');`
            },
            {
                name: 'Goodness-of-Fit (Chi-Square Independence & One-Sample KS)',
                description: 'Test a contingency table for independence, or a sample against a Normal distribution',
                code: `// Non-Parametric: chi-square test of independence, and one-sample KS against a Normal distribution
var contingency = Array(
    Array(30, 10),
    Array(20, 40)
);
var chiSq = NonParametricLibrary.ChiSquareIndependenceTest(contingency);
Print(Concat('Chi-square = ', ToString(ArrayGet(chiSq, 0)), ', df = ', ToString(ArrayGet(chiSq, 1)), ', p-value = ', ToString(ArrayGet(chiSq, 2))));

var sample = Array(48.2, 51.5, 49.8, 50.1, 52.3, 47.9, 50.6, 49.2, 51.0, 50.4, 48.8, 51.8, 49.5, 50.9, 49.0);
var ksTest = NonParametricLibrary.OneSampleKsTest(sample, 50, 1.2);
Print(Concat('One-sample KS D = ', ToString(ArrayGet(ksTest, 0)), ', p-value = ', ToString(ArrayGet(ksTest, 1))));`
            }
        ]
    },
    {
        name: 'DataTable Functions', icon: markRaw(Combine),
        snippets: [
            {
                name: 'Build & Inspect a DataTable',
                description: 'MakeRow, FromRows, and ToRows -- round-trip pseudocode rows through a DataTable',
                code: `// DataTable: pseudocode has no dictionary/DataTable literal, so MakeRow builds
// dictionary-shaped rows (the same shape ExecuteQuery/ReadDataset return), and
// FromRows turns those into a real DataTable for DataTableLibrary to operate on.
var columns = Array('Region', 'Amount');
var rows = Array();
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('East', 100)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('West', 250)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('East', 300)));
var salesTable = DataTableLibrary.FromRows(rows);

// ToRows converts back to plain rows so Table()/GetRowValue() can use them
var backToRows = DataTableLibrary.ToRows(salesTable);
Table(backToRows, 'Sales (round-tripped through DataTable)');
Print(Concat('Rows after round-trip: ', ToString(ArrayLength(backToRows))));`
            },
            {
                name: 'Aggregate a DataTable (Sum, Avg, Count)',
                description: 'Aggregate a column with an optional DataTable.Select()-style filter condition',
                code: `// DataTable: Sum/Avg/Count accept an optional filter condition using
// DataTable.Select() syntax (e.g. "Region = 'East'")
var columns = Array('Region', 'Amount');
var rows = Array();
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('East', 100)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('West', 250)));
ArrayPush(rows, DataTableLibrary.MakeRow(columns, Array('East', 300)));
var salesTable = DataTableLibrary.FromRows(rows);

var totalAll = DataTableLibrary.Sum(salesTable, 'Amount');
var totalEast = DataTableLibrary.Sum(salesTable, 'Amount', "Region = 'East'");
var avgAll = DataTableLibrary.Avg(salesTable, 'Amount');
var countAll = DataTableLibrary.Count(salesTable, '');
var countEast = DataTableLibrary.Count(salesTable, 'Region', "Region = 'East'");

Print(Concat('Total (all): ', ToString(totalAll)));
Print(Concat('Total (East only): ', ToString(totalEast)));
Print(Concat('Average (all): ', ToString(avgAll)));
Print(Concat('Row count (all): ', ToString(countAll), ', East rows: ', ToString(countEast)));`
            },
            {
                name: 'Join Two DataTables (Inner, Left, Right)',
                description: 'FromArrayRows builds tables directly from Array() data, then join them',
                code: `// DataTable: FromArrayRows builds a table straight from column names + array
// rows -- no MakeRow/dictionary step needed when you already have plain arrays.
var empColumns = Array('EmpId', 'Name', 'DeptId');
var empRows = Array(
    Array(1, 'Alice', 10),
    Array(2, 'Bob', 20),
    Array(3, 'Carol', 99) // DeptId with no matching department
);
var employees = DataTableLibrary.FromArrayRows(empColumns, empRows);

var deptColumns = Array('DeptId', 'DeptName');
var deptRows = Array(
    Array(10, 'Engineering'),
    Array(20, 'Sales'),
    Array(30, 'Marketing') // department with no matching employee
);
var departments = DataTableLibrary.FromArrayRows(deptColumns, deptRows);

// Join conditions use "A.column = B.column" syntax (A = first table, B = second)
var inner = DataTableLibrary.Inner_Join(employees, departments, 'A.DeptId = B.DeptId');
Table(DataTableLibrary.ToRows(inner), 'Inner Join -- only matching rows');

var left = DataTableLibrary.Left_Join(employees, departments, 'A.DeptId = B.DeptId');
Table(DataTableLibrary.ToRows(left), 'Left Join -- all employees, department blank if unmatched');

var right = DataTableLibrary.Right_Join(employees, departments, 'A.DeptId = B.DeptId');
Table(DataTableLibrary.ToRows(right), 'Right Join -- all departments, employee blank if unmatched');`
            }
        ]
    }
];

// Replaces the editor's content with a snippet's example code — mirrors
// loadSampleScript's "load a canned example" behavior.
const insertSnippet = (snippet) => {
    currentScript.id = null;
    currentScript.name = snippet.name;
    code.value = snippet.code;
    if (editorRef.value?.setCode) editorRef.value.setCode(snippet.code);
    debugText.value = '';
    scriptOutputs.value = [];
    hasExecuted.value = false;
};

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

        <!-- Snippets Toolbar: runnable example scripts, organized by category -->
        <div class="flex items-center gap-1.5 mb-3 bg-muted/30 p-2 border rounded-md flex-wrap">
            <span class="text-xs font-medium text-muted-foreground px-1">Snippets:</span>
            <DropdownMenu v-for="category in snippetCategories" :key="category.name">
                <DropdownMenuTrigger as-child>
                    <Button variant="outline" size="sm" class="gap-1.5 h-8">
                        <component :is="category.icon" class="w-3.5 h-3.5" />
                        {{ category.name }}
                        <ChevronDown class="w-3 h-3 opacity-60" />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="start" class="w-80">
                    <DropdownMenuLabel>{{ category.name }} Snippets</DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <template v-for="(snippet, si) in category.snippets" :key="snippet.name">
                        <DropdownMenuItem class="flex flex-col items-start gap-0.5 py-2" @click="insertSnippet(snippet)">
                            <span class="font-medium text-sm">{{ snippet.name }}</span>
                            <span class="text-xs text-muted-foreground whitespace-normal">{{ snippet.description }}</span>
                        </DropdownMenuItem>
                        <DropdownMenuSeparator v-if="si < category.snippets.length - 1" />
                    </template>
                </DropdownMenuContent>
            </DropdownMenu>
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

                <!-- Value output -->
                <template v-else-if="output.type === 'Value'">
                    <div class="flex items-center gap-3 p-3 border rounded bg-muted/20">
                        <Hash class="w-5 h-5 text-muted-foreground shrink-0" />
                        <div>
                            <div class="text-2xl font-bold tabular-nums">
                                {{ output.payload.value }}
                                <span v-if="output.payload.unit" class="text-sm font-normal text-muted-foreground ml-1">{{ output.payload.unit }}</span>
                            </div>
                            <div v-if="output.payload.label" class="text-xs text-muted-foreground">{{ output.payload.label }}</div>
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
