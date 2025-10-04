<template>
    <div>
        <div class="card">
            <Toolbar class="mb-6">
                <template #start>
                    <Button
                        label="New Connection"
                        icon="pi pi-plus"
                        @click="openNew"
                        class="mr-2"
                    />
                    <Button
                        label="Delete Selected"
                        icon="pi pi-trash"
                        severity="danger"
                        @click="confirmDeleteSelected"
                        :disabled="!selectedConnections || !selectedConnections.length"
                        class="mr-2"
                    />
                    <Button
                        label="Export"
                        icon="pi pi-download"
                        severity="secondary"
                        @click="exportConnections"
                        class="mr-2"
                    />
                    <Button
                        label="Import"
                        icon="pi pi-upload"
                        severity="secondary"
                        @click="$refs.fileInput.click()"
                    />
                    <input
                        ref="fileInput"
                        type="file"
                        accept=".json"
                        @change="importConnections"
                        style="display: none"
                    />
                </template>

                <template #end>
                    <div class="flex align-items-center gap-2">
                        <span class="text-sm text-muted-color">
                            {{ databaseStore.connectedCount }} of {{ databaseStore.allConnections.length }} connected
                        </span>
                        <Button
                            icon="pi pi-refresh"
                            @click="refreshConnections"
                            :loading="databaseStore.isLoading"
                            severity="secondary"
                        />
                    </div>
                </template>
            </Toolbar>

            <DataTable
                ref="dt"
                v-model:selection="selectedConnections"
                :value="databaseStore.allConnections"
                dataKey="id"
                :paginator="true"
                :rows="10"
                :filters="filters"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 25]"
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} connections"
                :globalFilterFields="['name', 'type', 'host', 'database']"
                responsiveLayout="scroll"
            >
                <template #header>
                    <div class="flex justify-content-between align-items-center">
                        <h5 class="m-0">Database Connections</h5>
                    </div>
                    <div class="flex justify-content-between align-items-center">
                        <IconField iconPosition="left">
                            <InputIcon>
                                <i class="pi pi-search" />
                            </InputIcon>
                            <InputText
                                v-model="filters['global'].value"
                                placeholder="Search connections..."
                            />
                        </IconField>
                    </div>
                </template>

                <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>

                <Column field="name" header="Name" :sortable="true" style="min-width: 12rem">
                    <template #body="{ data }">
                        <div class="flex align-items-center gap-2">
                            <i :class="getDatabaseIcon(data.type)" class="text-lg"></i>
                            <span class="font-medium">{{ data.name }}</span>
                        </div>
                    </template>
                </Column>

                <Column field="type" header="Type" :sortable="true" style="min-width: 8rem">
                    <template #body="{ data }">
                        <Chip :label="data.type.toUpperCase()" class="text-sm" />
                    </template>
                </Column>

                <Column field="host" header="Host" :sortable="true" style="min-width: 10rem">
                    <template #body="{ data }">
                        <span>{{ data.host }}:{{ data.port }}</span>
                    </template>
                </Column>

                <Column field="database" header="Database" :sortable="true" style="min-width: 10rem"></Column>

                <Column field="status" header="Status" :sortable="true" style="min-width: 8rem">
                    <template #body="{ data }">
                        <Badge
                            :value="getStatusLabel(data.status)"
                            :severity="getStatusSeverity(data.status)"
                        />
                    </template>
                </Column>

                <Column field="lastTested" header="Last Tested" :sortable="true" style="min-width: 10rem">
                    <template #body="{ data }">
                        <span v-if="data.lastTested" class="text-sm text-muted-color">
                            {{ formatDate(data.lastTested) }}
                        </span>
                        <span v-else class="text-sm text-muted-color">Never</span>
                    </template>
                </Column>

                <Column header="Actions" style="min-width: 12rem">
                    <template #body="{ data }">
                        <div class="flex gap-2">
                            <Button
                                icon="pi pi-check"
                                severity="success"
                                size="small"
                                @click="testConnection(data)"
                                :loading="data.status === 'testing'"
                                v-tooltip.top="'Test Connection'"
                            />
                            <Button
                                icon="pi pi-pencil"
                                severity="secondary"
                                size="small"
                                @click="editConnection(data)"
                                v-tooltip.top="'Edit'"
                            />
                            <Button
                                icon="pi pi-copy"
                                severity="secondary"
                                size="small"
                                @click="duplicateConnection(data)"
                                v-tooltip.top="'Duplicate'"
                            />
                            <Button
                                icon="pi pi-trash"
                                severity="danger"
                                size="small"
                                @click="confirmDeleteConnection(data)"
                                v-tooltip.top="'Delete'"
                            />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="text-center p-6">
                        <i class="pi pi-database text-6xl text-muted-color mb-3"></i>
                        <div class="text-xl text-muted-color mb-2">No database connections found</div>
                        <div class="text-muted-color mb-4">Get started by creating your first database connection</div>
                        <Button
                            label="Add Connection"
                            icon="pi pi-plus"
                            @click="openNew"
                        />
                    </div>
                </template>
            </DataTable>
        </div>

        <!-- Connection Configuration Dialog -->
        <Dialog
            v-model:visible="connectionDialog"
            :style="{ width: '600px' }"
            :header="editMode ? 'Edit Connection' : 'New Connection'"
            :modal="true"
            class="p-fluid"
        >
            <form @submit.prevent="saveConnection">
                <div class="grid">
                    <div class="col-12">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="name" class="font-semibold">Connection Name</label>
                            <InputText
                                id="name"
                                v-model="connection.name"
                                :class="{ 'p-invalid': submitted && !connection.name }"
                                placeholder="Enter connection name"
                            />
                            <small v-if="submitted && !connection.name" class="p-error">Name is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="type" class="font-semibold">Database Type</label>
                            <Select
                                id="type"
                                v-model="connection.type"
                                :options="databaseTypes"
                                optionLabel="label"
                                optionValue="value"
                                @change="onTypeChange"
                                placeholder="Select database type"
                                :class="{ 'p-invalid': submitted && !connection.type }"
                            />
                            <small v-if="submitted && !connection.type" class="p-error">Database type is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="host" class="font-semibold">Host</label>
                            <InputText
                                id="host"
                                v-model="connection.host"
                                :class="{ 'p-invalid': submitted && !connection.host }"
                                placeholder="localhost"
                            />
                            <small v-if="submitted && !connection.host" class="p-error">Host is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="port" class="font-semibold">Port</label>
                            <InputNumber
                                id="port"
                                v-model="connection.port"
                                :class="{ 'p-invalid': submitted && !connection.port }"
                                :min="1"
                                :max="65535"
                            />
                            <small v-if="submitted && !connection.port" class="p-error">Port is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="database" class="font-semibold">Database Name</label>
                            <InputText
                                id="database"
                                v-model="connection.database"
                                :class="{ 'p-invalid': submitted && !connection.database }"
                                placeholder="Database name"
                            />
                            <small v-if="submitted && !connection.database" class="p-error">Database name is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="username" class="font-semibold">Username</label>
                            <InputText
                                id="username"
                                v-model="connection.username"
                                :class="{ 'p-invalid': submitted && !connection.username }"
                                placeholder="Database username"
                            />
                            <small v-if="submitted && !connection.username" class="p-error">Username is required.</small>
                        </div>
                    </div>

                    <div class="col-12 md:col-6">
                        <div class="flex items-center gap-4 mb-4">
                            <label for="password" class="font-semibold">Password</label>
                            <Password
                                id="password"
                                v-model="connection.password"
                                :class="{ 'p-invalid': submitted && !connection.password }"
                                placeholder="Database password"
                                :feedback="false"
                                toggleMask
                            />
                            <small v-if="submitted && !connection.password" class="p-error">Password is required.</small>
                        </div>
                    </div>

                    <div class="col-12">
                        <Divider />
                        <div class="flex items-center gap-4 mb-4">
                            <label for="connectionString" class="font-semibold">
                                Custom Connection String
                                <small class="text-muted-color">(Optional)</small>
                            </label>
                            <Textarea
                                id="connectionString"
                                v-model="connection.connectionString"
                                rows="3"
                                placeholder="Override with custom connection string"
                            />
                            <small class="text-muted-color">
                                Leave empty to use the connection details above
                            </small>
                        </div>
                    </div>
                </div>
            </form>

            <template #footer>
                <Button
                    label="Cancel"
                    icon="pi pi-times"
                    @click="hideDialog"
                    severity="secondary"
                />
                <Button
                    label="Test & Save"
                    icon="pi pi-check"
                    @click="testAndSave"
                    :loading="testing"
                />
                <Button
                    label="Save"
                    icon="pi pi-save"
                    @click="saveConnection"
                />
            </template>
        </Dialog>

        <!-- Delete Confirmation Dialog -->
        <Dialog
            v-model:visible="deleteConnectionDialog"
            :style="{ width: '450px' }"
            header="Confirm Deletion"
            :modal="true"
        >
            <div class="flex align-items-center justify-content-center">
                <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
                <span v-if="connection">
                    Are you sure you want to delete <b>{{ connection.name }}</b>?
                </span>
            </div>
            <template #footer>
                <Button
                    label="No"
                    icon="pi pi-times"
                    @click="deleteConnectionDialog = false"
                    severity="secondary"
                />
                <Button
                    label="Yes"
                    icon="pi pi-check"
                    @click="deleteConnection"
                    severity="danger"
                />
            </template>
        </Dialog>

        <!-- Delete Selected Confirmation Dialog -->
        <Dialog
            v-model:visible="deleteSelectedDialog"
            :style="{ width: '450px' }"
            header="Confirm Deletion"
            :modal="true"
        >
            <div class="flex align-items-center justify-content-center">
                <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
                <span v-if="selectedConnections">
                    Are you sure you want to delete {{ selectedConnections.length }} selected connection(s)?
                </span>
            </div>
            <template #footer>
                <Button
                    label="No"
                    icon="pi pi-times"
                    @click="deleteSelectedDialog = false"
                    severity="secondary"
                />
                <Button
                    label="Yes"
                    icon="pi pi-check"
                    @click="deleteSelectedConnections"
                    severity="danger"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup>
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';
import { onMounted, ref, reactive } from 'vue';
import { useDatabaseStore } from '@/store/databaseStore';

const toast = useToast();
const confirm = useConfirm();
const databaseStore = useDatabaseStore();

const dt = ref();
const fileInput = ref();
const connectionDialog = ref(false);
const deleteConnectionDialog = ref(false);
const deleteSelectedDialog = ref(false);
const selectedConnections = ref();
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS }
});
const submitted = ref(false);
const editMode = ref(false);
const testing = ref(false);

const connection = reactive({
    name: '',
    type: '',
    host: '',
    port: null,
    database: '',
    username: '',
    password: '',
    connectionString: ''
});

const databaseTypes = ref([
    { label: 'PostgreSQL', value: 'postgresql', icon: 'pi pi-database' },
    { label: 'Oracle', value: 'oracle', icon: 'pi pi-database' },
    { label: 'Microsoft SQL Server', value: 'mssql', icon: 'pi pi-database' },
    { label: 'MySQL', value: 'mysql', icon: 'pi pi-database' }
]);

onMounted(() => {
    databaseStore.loadConnections();
});

function openNew() {
    resetConnection();
    editMode.value = false;
    submitted.value = false;
    connectionDialog.value = true;
}

function editConnection(conn) {
    Object.assign(connection, { ...conn });
    editMode.value = true;
    submitted.value = false;
    connectionDialog.value = true;
}

function hideDialog() {
    connectionDialog.value = false;
    submitted.value = false;
    resetConnection();
}

function resetConnection() {
    Object.assign(connection, {
        name: '',
        type: '',
        host: '',
        port: null,
        database: '',
        username: '',
        password: '',
        connectionString: ''
    });
}

function onTypeChange() {
    if (connection.type) {
        connection.port = databaseStore.getDefaultPort(connection.type);
    }
}

function saveConnection() {
    submitted.value = true;

    if (isValidConnection()) {
        const connectionData = { ...connection };

        if (editMode.value) {
            databaseStore.updateConnection(connection.id, connectionData);
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Connection updated successfully',
                life: 3000
            });
        } else {
            databaseStore.addConnection(connectionData);
            toast.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Connection created successfully',
                life: 3000
            });
        }

        hideDialog();
    }
}

async function testAndSave() {
    submitted.value = true;

    if (isValidConnection()) {
        testing.value = true;

        try {
            let connectionToTest;

            if (editMode.value) {
                connectionToTest = databaseStore.updateConnection(connection.id, { ...connection });
            } else {
                connectionToTest = databaseStore.addConnection({ ...connection });
            }

            const success = await databaseStore.testConnection(connectionToTest.id);

            if (success) {
                toast.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Connection test successful and saved!',
                    life: 3000
                });
                hideDialog();
            } else {
                toast.add({
                    severity: 'error',
                    summary: 'Connection Failed',
                    detail: 'Could not connect to database. Connection saved anyway.',
                    life: 5000
                });
            }
        } finally {
            testing.value = false;
        }
    }
}

function isValidConnection() {
    return connection.name?.trim() &&
           connection.type &&
           connection.host?.trim() &&
           connection.port &&
           connection.database?.trim() &&
           connection.username?.trim() &&
           connection.password?.trim();
}

async function testConnection(conn) {
    const success = await databaseStore.testConnection(conn.id);

    if (success) {
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: `Connected to ${conn.name} successfully`,
            life: 3000
        });
    } else {
        toast.add({
            severity: 'error',
            summary: 'Connection Failed',
            detail: `Could not connect to ${conn.name}`,
            life: 5000
        });
    }
}

function duplicateConnection(conn) {
    const duplicate = databaseStore.duplicateConnection(conn.id);
    if (duplicate) {
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Connection duplicated successfully',
            life: 3000
        });
    }
}

function confirmDeleteConnection(conn) {
    Object.assign(connection, conn);
    deleteConnectionDialog.value = true;
}

function deleteConnection() {
    const success = databaseStore.deleteConnection(connection.id);
    if (success) {
        toast.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Connection deleted successfully',
            life: 3000
        });
    }
    deleteConnectionDialog.value = false;
    resetConnection();
}

function confirmDeleteSelected() {
    deleteSelectedDialog.value = true;
}

function deleteSelectedConnections() {
    selectedConnections.value.forEach(conn => {
        databaseStore.deleteConnection(conn.id);
    });

    toast.add({
        severity: 'success',
        summary: 'Success',
        detail: `${selectedConnections.value.length} connection(s) deleted successfully`,
        life: 3000
    });

    selectedConnections.value = null;
    deleteSelectedDialog.value = false;
}

function refreshConnections() {
    databaseStore.loadConnections();
    toast.add({
        severity: 'info',
        summary: 'Refreshed',
        detail: 'Connections list refreshed',
        life: 2000
    });
}

function exportConnections() {
    const data = databaseStore.exportConnections();
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `database-connections-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);

    toast.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Connections exported successfully',
        life: 3000
    });
}

function importConnections(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const success = databaseStore.importConnections(e.target.result);
            if (success) {
                toast.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Connections imported successfully',
                    life: 3000
                });
            } else {
                toast.add({
                    severity: 'error',
                    summary: 'Import Failed',
                    detail: 'Invalid file format',
                    life: 5000
                });
            }
        };
        reader.readAsText(file);
    }
    event.target.value = '';
}

function getDatabaseIcon(type) {
    const icons = {
        postgresql: 'pi pi-database text-blue-500',
        oracle: 'pi pi-database text-red-500',
        mssql: 'pi pi-database text-orange-500',
        mysql: 'pi pi-database text-green-500'
    };
    return icons[type] || 'pi pi-database';
}

function getStatusLabel(status) {
    const labels = {
        connected: 'Connected',
        disconnected: 'Disconnected',
        error: 'Error',
        testing: 'Testing...'
    };
    return labels[status] || status;
}

function getStatusSeverity(status) {
    const severities = {
        connected: 'success',
        disconnected: 'secondary',
        error: 'danger',
        testing: 'warning'
    };
    return severities[status] || 'secondary';
}

function formatDate(date) {
    return new Date(date).toLocaleString();
}
</script>
<style>
.card {
    background: var(--surface-card);
    padding: 2rem;
    border-radius: 10px;
    margin-bottom: 1rem;
}
body .app-dark .card {
    background: var(--p-surface-800);
    border-color: var(--p-surface-border);
}

body .app-dark .p-datatable .p-datatable-thead > tr > th {
    background-color: var(--p-surface-700);
    color: var(--p-text-color);
    border-color: var(--p-surface-border);
}

body .app-dark .p-datatable .p-datatable-tbody > tr {
    background-color: var(--p-surface-800);
    color: var(--p-text-color);
}

body .app-dark .p-datatable .p-datatable-tbody > tr > td {
    border-color: var(--p-surface-border);
}

body .app-dark .p-datatable .p-datatable-tbody > tr:hover > td {
    background-color: var(--p-surface-700);
}

body .app-dark .p-toolbar {
    background: var(--p-surface-700);
    border-color: var(--p-surface-border);
}

body .app-dark .p-dialog .p-dialog-header,
body .app-dark .p-dialog .p-dialog-content,
body .app-dark .p-dialog .p-dialog-footer {
    background: var(--p-surface-800);
    color: var(--p-text-color);
    border-color: var(--p-surface-700);
}
</style>
