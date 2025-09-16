<template>
    <div class="sql-editor-container">
        <!-- Top Toolbar with Database Selection -->
        <Toolbar class="mb-3">
            <template #start>
                <div class="flex align-items-center gap-2">
                    <label for="database-select" class="font-semibold text-sm">Database:</label>
                    <Dropdown
                        id="database-select"
                        v-model="selectedDatabase"
                        :options="availableDatabases"
                        optionLabel="name"
                        optionValue="id"
                        placeholder="Select Database"
                        class="w-12rem"
                        :disabled="isExecuting"
                    >
                        <template #value="slotProps">
                            <div v-if="slotProps.value" class="flex align-items-center gap-2">
                                <i :class="getDatabaseIcon(getSelectedDatabaseType())" class="text-sm"></i>
                                <span>{{ getSelectedDatabaseName() }}</span>
                            </div>
                            <span v-else>{{ slotProps.placeholder }}</span>
                        </template>
                        <template #option="slotProps">
                            <div class="flex align-items-center gap-2">
                                <i :class="getDatabaseIcon(slotProps.option.type)" class="text-sm"></i>
                                <div>
                                    <div class="font-medium">{{ slotProps.option.name }}</div>
                                    <div class="text-xs text-muted-color">{{ slotProps.option.host }}:{{ slotProps.option.port }}</div>
                                </div>
                            </div>
                        </template>
                    </Dropdown>
                </div>
            </template>

            <template #end>
                <div class="flex align-items-center gap-2">
                    <Button
                        icon="pi pi-play"
                        label="Execute"
                        @click="executeQuery"
                        :disabled="!selectedDatabase || !code.trim() || isExecuting"
                        :loading="isExecuting"
                        severity="success"
                    />
                    <Button
                        icon="pi pi-save"
                        label="Save Script"
                        @click="saveScript"
                        :disabled="!code.trim()"
                        severity="secondary"
                    />
                </div>
            </template>
        </Toolbar>

        <!-- Script Management Toolbar -->
        <Toolbar class="mb-3">
            <template #start>
                <div class="flex align-items-center gap-2">
                    <Inplace>
                        <template #display>
                            <div class="flex align-items-center gap-2 cursor-pointer">
                                <i class="pi pi-file text-muted-color"></i>
                                <span class="font-medium">{{ currentScript.name || 'Untitled Script' }}</span>
                                <i class="pi pi-pencil text-xs text-muted-color"></i>
                            </div>
                        </template>
                        <template #content="{ closeCallback }">
                            <div class="flex align-items-center gap-2">
                                <InputText v-model="currentScript.name" placeholder="Script name" autofocus />
                                <Button icon="pi pi-check" text severity="success" @click="closeCallback" />
                                <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
                            </div>
                        </template>
                    </Inplace>
                </div>
            </template>

            <template #center>
                <div class="flex align-items-center gap-1">
                    <Button icon="pi pi-folder-open" text @click="loadScript" v-tooltip.top="'Open Script'" />
                    <Button icon="pi pi-plus" text @click="newScript" v-tooltip.top="'New Script'" />
                    <Divider layout="vertical" />
                    <Button icon="pi pi-undo" text @click="handleUndo" v-tooltip.top="'Undo'" />
                    <Button icon="pi pi-redo" text @click="handleRedo" v-tooltip.top="'Redo'" />
                    <Divider layout="vertical" />
                    <Button icon="pi pi-copy" text @click="copyText" v-tooltip.top="'Copy'" />
                    <Button icon="pi pi-clone" text @click="pasteText" v-tooltip.top="'Paste'" />
                    <Divider layout="vertical" />
                    <Button icon="pi pi-search" text @click="toggleSearch" v-tooltip.top="'Find'" />
                </div>
            </template>
        </Toolbar>

        <!-- SQL Editor -->
        <div class="editor-container card mb-3">
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
        <div class="results-container card">
            <div class="flex justify-content-between align-items-center mb-3">
                <h6 class="m-0">Query Results</h6>
                <div class="flex align-items-center gap-2">
                    <Badge v-if="queryResults.length > 0" :value="`${queryResults.length} rows`" severity="info" />
                    <Button
                        icon="pi pi-download"
                        label="Export"
                        size="small"
                        severity="secondary"
                        @click="exportResults"
                        :disabled="queryResults.length === 0"
                    />
                </div>
            </div>

            <DataTable
                v-if="queryResults.length > 0"
                :value="queryResults"
                :paginator="true"
                :rows="20"
                :rowsPerPageOptions="[10, 20, 50, 100]"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
                scrollable
                scrollHeight="400px"
                resizableColumns
                columnResizeMode="expand"
                class="results-table"
            >
                <Column
                    v-for="col in queryColumns"
                    :key="col.field"
                    :field="col.field"
                    :header="col.header"
                    :sortable="true"
                    style="min-width: 120px"
                >
                    <template #body="{ data }">
                        <span class="result-cell" :title="data[col.field]">
                            {{ formatCellValue(data[col.field]) }}
                        </span>
                    </template>
                </Column>
            </DataTable>

            <div v-else-if="hasExecuted" class="text-center p-6">
                <i class="pi pi-info-circle text-4xl text-muted-color mb-3"></i>
                <div class="text-xl text-muted-color mb-2">No Results</div>
                <div class="text-muted-color">The query executed successfully but returned no data</div>
            </div>

            <div v-else class="text-center p-6">
                <i class="pi pi-database text-4xl text-muted-color mb-3"></i>
                <div class="text-xl text-muted-color mb-2">Ready to Execute</div>
                <div class="text-muted-color">Write your SQL query above and click Execute to see results</div>
            </div>
        </div>

        <!-- Load Script Dialog -->
        <Dialog
            v-model:visible="showLoadDialog"
            :style="{ width: '600px' }"
            header="Load Script"
            :modal="true"
        >
            <DataTable
                v-model:selection="selectedScriptToLoad"
                :value="savedScripts"
                selectionMode="single"
                dataKey="id"
                :paginator="true"
                :rows="10"
                class="mb-3"
            >
                <Column field="name" header="Name" :sortable="true"></Column>
                <Column field="database" header="Database" :sortable="true"></Column>
                <Column field="updatedAt" header="Last Modified" :sortable="true">
                    <template #body="{ data }">
                        {{ formatDate(data.updatedAt) }}
                    </template>
                </Column>
                <Column header="Actions" style="width: 120px">
                    <template #body="{ data }">
                        <Button
                            icon="pi pi-trash"
                            severity="danger"
                            size="small"
                            @click="deleteScript(data.id)"
                            v-tooltip.top="'Delete'"
                        />
                    </template>
                </Column>
            </DataTable>

            <template #footer>
                <Button label="Cancel" icon="pi pi-times" @click="showLoadDialog = false" severity="secondary" />
                <Button
                    label="Load"
                    icon="pi pi-check"
                    @click="confirmLoadScript"
                    :disabled="!selectedScriptToLoad"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup>
import { reactive, shallowRef, ref, computed, onMounted } from 'vue';
import { redo, undo } from '@codemirror/commands';
import { search } from '@codemirror/search';
import { Codemirror } from 'vue-codemirror';
import { sql } from '@codemirror/lang-sql';
import { useToast } from 'primevue/usetoast';
import { useDatabaseStore } from '@/store/databaseStore';

// Stores and services
const toast = useToast();
const databaseStore = useDatabaseStore();

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

// Script management
const currentScript = reactive({
    id: null,
    name: '',
    content: '',
    database: null
});

const savedScripts = ref([]);
const showLoadDialog = ref(false);
const selectedScriptToLoad = ref(null);

// Theme-aware editor styling
const editorStyle = computed(() => {
    return {
        width: '100%',
        height: config.height,
        border: `1px solid var(--p-border-color)`,
        borderRadius: 'var(--p-border-radius)'
    };
});

// Available databases from store
const availableDatabases = computed(() => {
    return databaseStore.allConnections.filter(conn => conn.status === 'connected');
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

const handleStateUpdate = (viewUpdate) => {
    // Auto-save current script content
    currentScript.content = code.value;
};

// Text operations
const copyText = async () => {
    if (!cmView.value) return;

    const selection = cmView.value.state.selection.main;
    const selectedText = selection.empty
        ? cmView.value.state.doc.toString()
        : cmView.value.state.doc.sliceString(selection.from, selection.to);

    try {
        await navigator.clipboard.writeText(selectedText);
        toast.add({
            severity: 'success',
            summary: 'Copied',
            detail: 'Text copied to clipboard',
            life: 2000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Copy Failed',
            detail: 'Failed to copy text to clipboard',
            life: 3000
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

        toast.add({
            severity: 'success',
            summary: 'Pasted',
            detail: 'Text pasted from clipboard',
            life: 2000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Paste Failed',
            detail: 'Failed to paste text from clipboard',
            life: 3000
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
    const icons = {
        postgresql: 'pi pi-database text-blue-500',
        oracle: 'pi pi-database text-red-500',
        mssql: 'pi pi-database text-orange-500',
        mysql: 'pi pi-database text-green-500'
    };
    return icons[type] || 'pi pi-database';
};

// Query execution
const executeQuery = async () => {
    if (!selectedDatabase.value || !code.value.trim()) return;

    isExecuting.value = true;
    hasExecuted.value = false;
    queryResults.value = [];
    queryColumns.value = [];

    try {
        // Simulate query execution - replace with actual database query logic
        await new Promise(resolve => setTimeout(resolve, 1500));

        // Mock data generation for demo
        const mockData = generateMockData(code.value);
        queryResults.value = mockData.rows;
        queryColumns.value = mockData.columns;

        hasExecuted.value = true;

        toast.add({
            severity: 'success',
            summary: 'Query Executed',
            detail: `Query executed successfully. ${queryResults.value.length} rows returned.`,
            life: 3000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Query Failed',
            detail: error.message || 'An error occurred while executing the query',
            life: 5000
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
            { field: 'email', header: 'Email' },
            { field: 'created_at', header: 'Created At' }
        ];

        // Generate mock rows
        const rows = Array.from({ length: Math.floor(Math.random() * 50) + 1 }, (_, i) => ({
            id: i + 1,
            name: `User ${i + 1}`,
            email: `user${i + 1}@example.com`,
            created_at: new Date(Date.now() - Math.random() * 1000000000).toISOString()
        }));

        return { columns, rows };
    }

    return { columns: [], rows: [] };
};

// Script management
const newScript = () => {
    currentScript.id = null;
    currentScript.name = '';
    currentScript.content = '';
    currentScript.database = null;
    code.value = '';
    queryResults.value = [];
    queryColumns.value = [];
    hasExecuted.value = false;
};

const saveScript = () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled Script',
        content: code.value,
        database: getSelectedDatabaseName(),
        databaseId: selectedDatabase.value,
        createdAt: currentScript.id ? undefined : new Date(),
        updatedAt: new Date()
    };

    const existingIndex = savedScripts.value.findIndex(s => s.id === script.id);

    if (existingIndex >= 0) {
        savedScripts.value[existingIndex] = { ...savedScripts.value[existingIndex], ...script };
    } else {
        savedScripts.value.push(script);
        currentScript.id = script.id;
    }

    saveScriptsToStorage();

    toast.add({
        severity: 'success',
        summary: 'Script Saved',
        detail: `Script "${script.name}" saved successfully`,
        life: 3000
    });
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
    currentScript.content = script.content;
    currentScript.database = script.database;

    code.value = script.content;
    selectedDatabase.value = script.databaseId;

    queryResults.value = [];
    queryColumns.value = [];
    hasExecuted.value = false;

    showLoadDialog.value = false;
    selectedScriptToLoad.value = null;

    toast.add({
        severity: 'success',
        summary: 'Script Loaded',
        detail: `Script "${script.name}" loaded successfully`,
        life: 3000
    });
};

const deleteScript = (scriptId) => {
    const index = savedScripts.value.findIndex(s => s.id === scriptId);
    if (index >= 0) {
        const scriptName = savedScripts.value[index].name;
        savedScripts.value.splice(index, 1);
        saveScriptsToStorage();

        toast.add({
            severity: 'success',
            summary: 'Script Deleted',
            detail: `Script "${scriptName}" deleted successfully`,
            life: 3000
        });
    }
};

// Storage management
const saveScriptsToStorage = () => {
    try {
        localStorage.setItem('sql-scripts', JSON.stringify(savedScripts.value));
    } catch (error) {
        console.error('Failed to save scripts:', error);
    }
};

const loadScriptsFromStorage = () => {
    try {
        const saved = localStorage.getItem('sql-scripts');
        if (saved) {
            savedScripts.value = JSON.parse(saved).map(script => ({
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

// Utility functions
const generateId = () => {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
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
        queryColumns.value.map(col => col.header).join(','),
        ...queryResults.value.map(row =>
            queryColumns.value.map(col => {
                const value = row[col.field];
                if (value === null || value === undefined) return '';
                if (typeof value === 'string' && value.includes(',')) {
                    return `"${value.replace(/"/g, '""')}"`;
                }
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

    toast.add({
        severity: 'success',
        summary: 'Export Complete',
        detail: 'Query results exported to CSV file',
        life: 3000
    });
};

// Initialize component
onMounted(() => {
    databaseStore.loadConnections();
    loadScriptsFromStorage();
});
</script>
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

.results-container {
  flex: 1;
  min-height: 400px;
  display: flex;
  flex-direction: column;

  .results-table {
    flex: 1;

    :deep(.p-datatable-table) {
      font-size: 13px;
    }

    :deep(.p-datatable-tbody > tr > td) {
      padding: 0.5rem;
      border-bottom: 1px solid var(--p-border-color);
    }

    :deep(.p-datatable-thead > tr > th) {
      padding: 0.75rem 0.5rem;
      background-color: var(--p-surface-100);
      font-weight: 600;
      border-bottom: 2px solid var(--p-border-color);
    }
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

  .result-cell {
    max-width: 120px;
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

// Dark theme adjustments
body .app-dark .sql-editor-container {
    .editor-container {
        background: var(--p-surface-800);
        border-color: var(--p-surface-border);
        
        .cm-search {
            background-color: var(--p-surface-700);
            border-color: var(--p-surface-border);
            color: var(--p-text-color);
        }
    }
    .results-container {
        background: var(--p-surface-800);
        color: var(--p-text-color);
        .results-table {
            .p-datatable-thead > tr > th {
                background-color: var(--p-surface-700);
                color: var(--p-text-color);
                border-color: var(--p-surface-border);
            }
            .p-datatable-tbody > tr > td {
                background-color: var(--p-surface-800);
                color: var(--p-text-color);
                border-color: var(--p-surface-border);
            }
            .p-datatable-tbody > tr:hover > td {
                background-color: var(--p-surface-700);
            }
        }
    }
    .p-toolbar {
        background: var(--p-surface-700);
        border-color: var(--p-surface-border);
        color: var(--p-text-color);
    }
    .p-dropdown {
        background: var(--p-surface-800);
        color: var(--p-text-color);
        border-color: var(--p-surface-border);
    }
    .p-badge {
        background: var(--p-primary-color);
        color: var(--p-primary-color-text);
    }
}
</style>