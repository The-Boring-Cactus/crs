<template>
    <div class="cs-editor-container">
        <Toast />

        <!-- Main Toolbar -->
        <Toolbar class="mb-3">
            <template #start>
                <div class="flex align-items-center gap-2">
                    <Inplace>
                        <template #display>
                            <div class="flex align-items-center gap-2 cursor-pointer">
                                <i class="pi pi-file-code text-muted-color"></i>
                                <span class="font-medium">{{ currentScript.name || 'Untitled C# Script' }}</span>
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

            <template #end>
                <div class="flex align-items-center gap-2">
                    <Button
                        icon="pi pi-play"
                        label="Execute"
                        @click="handleExecute"
                        :disabled="!code.trim() || isExecuting"
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

        <!-- Edit Menu Toolbar -->
        <Toolbar class="mb-3">
            <template #start>
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
                    <Button icon="pi pi-code" text @click="formatCode" v-tooltip.top="'Format Code'" />
                </div>
            </template>

            <template #center>
                <div class="flex align-items-center gap-2">
                    <i class="pi pi-info-circle text-muted-color"></i>
                    <span class="text-sm text-muted-color">C# Script Editor</span>
                </div>
            </template>

            <template #end>
                <div class="flex align-items-center gap-2">
                    <Button
                        icon="pi pi-stop"
                        label="Stop"
                        @click="stopExecution"
                        :disabled="!isExecuting"
                        severity="danger"
                        size="small"
                    />
                    <Button
                        icon="pi pi-trash"
                        @click="clearDebug"
                        v-tooltip.top="'Clear Debug'"
                        severity="secondary"
                        size="small"
                    />
                </div>
            </template>
        </Toolbar>

        <!-- Code Editor -->
        <div class="editor-container card mb-3">
            <CodeMirrorEditor
                ref="editorRef"
                :initial-code="code"
                :code-functions="codeFunctions"
                :initial-language="'csharp'"
                @update:code="handleCodeChange"
                @language-changed="handleLanguageChange"
                :style="editorStyle"
                :theme="editorTheme"
            />
        </div>

        <!-- Debug Output -->
        <div class="debug-container card">
            <div class="flex justify-content-between align-items-center mb-3">
                <div class="flex align-items-center gap-2">
                    <h6 class="m-0">Debug Output</h6>
                    <Badge v-if="isExecuting" value="Running" severity="warning" />
                    <Badge v-else-if="hasExecuted" value="Completed" severity="success" />
                </div>
                <div class="flex align-items-center gap-2">
                    <Button
                        icon="pi pi-download"
                        label="Export Log"
                        size="small"
                        severity="secondary"
                        @click="exportDebugLog"
                        :disabled="!debugText.trim()"
                    />
                </div>
            </div>

            <div class="debug-output" :class="{ 'executing': isExecuting }">
                <Textarea
                    v-model="debugText"
                    :rows="debugRows"
                    :autoResize="true"
                    :readonly="isExecuting"
                    placeholder="Debug output will appear here when you execute the script..."
                    class="debug-textarea"
                />
            </div>
        </div>

        <!-- Load Script Dialog -->
        <Dialog
            v-model:visible="showLoadDialog"
            :style="{ width: '600px' }"
            header="Load C# Script"
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
import { shallowRef, ref, computed, onMounted, reactive } from 'vue';
import { getCurrentInstance } from 'vue';
import { useToast } from "primevue/usetoast";
import CodeMirrorEditor from "@/components/CodeMirrorEditor.vue";
import { useLayout } from '@/layout/composables/layout.js';
import { basicDark } from '@fsegurai/codemirror-theme-basic-dark';
import { basicLight } from '@fsegurai/codemirror-theme-basic-light';

import {userStoreMe} from "@/store/userStore";


import {WebSocketMessageClient} from "@/websocket/WebSocketMessageClient";
import {ServerResponse} from "@/websocket/ServerResponse";



// Services and stores
const toast = useToast();
const { proxy } = getCurrentInstance();
const client = new WebSocketMessageClient(proxy.$socket);
const userStore = userStoreMe();
const { isDarkTheme } = useLayout();

// Editor state
const code = ref(`// Welcome to C# Script Editor
using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        // Your code here

    }
}`);

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
    content: '',
    language: 'csharp'
});

const savedScripts = ref([]);
const showLoadDialog = ref(false);
const selectedScriptToLoad = ref(null);

// Theme-aware editor styling and extensions
const editorStyle = computed(() => {
    return {
        width: '100%',
        minHeight: '350px',
        border: `1px solid var(--p-border-color)`,
        borderRadius: 'var(--p-border-radius)'
    };
});

const editorTheme = computed(() => {
    return isDarkTheme.value ? basicDark : basicLight;
});


// WebSocket message handling
proxy.$socket.onmessage = (data) => {
    const payload = JSON.parse(data.data);
    console.log('WebSocket message:', payload);

    if (payload.TypeMsg === "FinishCode") {
        isExecuting.value = false;
        hasExecuted.value = true;

        if (payload.status === "Fail") {
            toast.add({
                severity: 'error',
                summary: 'Execution Failed',
                detail: 'An error occurred during script execution',
                life: 5000
            });
        } else {
            toast.add({
                severity: 'success',
                summary: 'Execution Complete',
                detail: 'Script executed successfully',
                life: 3000
            });
        }
    }

    if (payload.TypeMsg === "Debug") {
        const timestamp = new Date().toLocaleTimeString();
        debugText.value += `[${timestamp}] ${payload.data}\n`;

        // Auto-scroll to bottom
        setTimeout(() => {
            const textarea = document.querySelector('.debug-textarea textarea');
            if (textarea) {
                textarea.scrollTop = textarea.scrollHeight;
            }
        }, 100);
    }
};





// Editor operations
const handleCodeChange = (newCode) => {
    code.value = newCode;
    currentScript.content = newCode;
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
    if (!editorRef.value) return;

    try {
        const text = await navigator.clipboard.readText();

        // Insert at current cursor position or replace selection
        if (editorRef.value.setCode) {
            const currentCode = code.value;
            const newCode = currentCode + text; // Simple append for now
            editorRef.value.setCode(newCode);
        }

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
            .map(line => line.trim())
            .filter(line => line.length > 0)
            .join('\n');

        if (editorRef.value?.setCode) {
            editorRef.value.setCode(formatted);
        }

        toast.add({
            severity: 'success',
            summary: 'Code Formatted',
            detail: 'Code has been formatted',
            life: 2000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Format Failed',
            detail: 'Failed to format code',
            life: 3000
        });
    }
};

// Execution operations
const handleExecute = () => {
    if (!code.value.trim()) return;

    console.log('Executing code:', code.value);
    isExecuting.value = true;
    hasExecuted.value = false;
    debugText.value = '';

    const timestamp = new Date().toLocaleTimeString();
    debugText.value = `[${timestamp}] Starting script execution...\n`;

    proxy.$socket.sendObj({
        type: "CodeScript",
        data: code.value,
        name: currentScript.name || 'Untitled Script'
    });

    toast.add({
        severity: 'info',
        summary: 'Execution Started',
        detail: 'Script execution has been initiated',
        life: 3000
    });
};

const stopExecution = () => {
    proxy.$socket.sendObj({
        type: "StopExecution"
    });

    isExecuting.value = false;

    const timestamp = new Date().toLocaleTimeString();
    debugText.value += `[${timestamp}] Execution stopped by user.\n`;

    toast.add({
        severity: 'warn',
        summary: 'Execution Stopped',
        detail: 'Script execution has been stopped',
        life: 3000
    });
};

const clearDebug = () => {
    debugText.value = '';
    toast.add({
        severity: 'info',
        summary: 'Debug Cleared',
        detail: 'Debug output has been cleared',
        life: 2000
    });
};

// Script management
const newScript = () => {
    currentScript.id = null;
    currentScript.name = '';
    currentScript.content = '';
    currentScript.language = 'csharp';

    code.value = `// New C# Script\nusing System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine("Hello, World!");\n\n        // Your code here\n\n    }\n}`;

    debugText.value = '';
    hasExecuted.value = false;

    if (editorRef.value?.setCode) {
        editorRef.value.setCode(code.value);
    }
};

const saveScript = () => {
    const script = {
        id: currentScript.id || generateId(),
        name: currentScript.name || 'Untitled C# Script',
        content: code.value,
        language: 'csharp',
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
    currentScript.language = script.language || 'csharp';

    code.value = script.content;
    debugText.value = '';
    hasExecuted.value = false;

    if (editorRef.value?.setCode) {
        editorRef.value.setCode(script.content);
    }

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
        localStorage.setItem('cs-scripts', JSON.stringify(savedScripts.value));
    } catch (error) {
        console.error('Failed to save scripts:', error);
    }
};

const loadScriptsFromStorage = () => {
    try {
        const saved = localStorage.getItem('cs-scripts');
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

const exportDebugLog = () => {
    if (!debugText.value.trim()) return;

    const blob = new Blob([debugText.value], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `debug-log-${new Date().toISOString().split('T')[0]}.txt`;
    link.click();
    URL.revokeObjectURL(url);

    toast.add({
        severity: 'success',
        summary: 'Export Complete',
        detail: 'Debug log exported successfully',
        life: 3000
    });
};

// Utility functions
const generateId = () => {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
};

const formatDate = (date) => {
    return new Date(date).toLocaleString();
};

// Legacy handlers for backward compatibility
const handleSave = () => {
    saveScript();
};

// Initialize component
onMounted(() => {
    loadScriptsFromStorage();
});


</script>
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
  border: 1px solid var(--p-border-color);
  border-radius: var(--p-border-radius);
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
        background: linear-gradient(
          90deg,
          transparent,
          rgba(var(--p-primary-color-rgb), 0.1),
          transparent
        );
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
  0%, 100% {
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
}



// Status badges styling
:deep(.p-badge) {
  font-size: 0.75rem;
  padding: 0.25rem 0.5rem;
}

// Button group styling
.cs-editor-container {
  :deep(.p-toolbar) {
    background: var(--p-surface-50);
    border: 1px solid var(--p-border-color);
    border-radius: var(--p-border-radius);
    padding: 0.75rem 1rem;

    .p-button {
      &.p-button-text {
        color: var(--p-text-muted-color);

        &:hover {
          background-color: var(--p-surface-100);
          color: var(--p-text-color);
        }

        &:disabled {
          opacity: 0.4;
        }
      }
    }

    .p-divider {
      &.p-divider-vertical {
        height: 1.5rem;
        margin: 0 0.5rem;
        border-color: var(--p-border-color);
      }
    }
  }
}

</style>