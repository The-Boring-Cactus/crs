<script setup>
import { onMounted, ref, reactive, computed } from 'vue';
import { useDatabaseStore } from '@/store/databaseStore';
import { toast } from 'vue-sonner';
import { Toaster } from '@/components/ui/sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Database, Plus, Trash2, Download, Upload, RefreshCw, Search, ArrowUpDown, Check, Pencil, Copy, X, Save, AlertTriangle, Loader2 } from 'lucide-vue-next';

const databaseStore = useDatabaseStore();

const fileInput = ref();
const connectionDialog = ref(false);
const deleteConnectionDialog = ref(false);
const deleteSelectedDialog = ref(false);
const selectedConnections = ref([]);
const filters = ref({
    global: { value: null }
});

// Pagination and sorting state
const currentPage = ref(1);
const rowsPerPage = ref(10);
const sortField = ref('name');
const sortOrder = ref(1);

const filteredConnections = computed(() => {
    let result = databaseStore.allConnections;
    const globalFilter = filters.value.global.value?.toLowerCase();

    if (globalFilter) {
        result = result.filter((conn) => conn.name?.toLowerCase().includes(globalFilter) || conn.type?.toLowerCase().includes(globalFilter) || conn.host?.toLowerCase().includes(globalFilter) || conn.database?.toLowerCase().includes(globalFilter));
    }

    if (sortField.value) {
        result.sort((a, b) => {
            let value1 = a[sortField.value];
            let value2 = b[sortField.value];

            if (value1 == null) return sortOrder.value;
            if (value2 == null) return -sortOrder.value;

            if (typeof value1 === 'string') {
                return value1.localeCompare(value2) * sortOrder.value;
            }
            return value1 < value2 ? -sortOrder.value : value1 > value2 ? sortOrder.value : 0;
        });
    }

    return result;
});

const totalPages = computed(() => Math.ceil(filteredConnections.value.length / rowsPerPage.value));
const paginationStart = computed(() => (currentPage.value - 1) * rowsPerPage.value + 1);
const paginationEnd = computed(() => Math.min(currentPage.value * rowsPerPage.value, filteredConnections.value.length));

const paginatedConnections = computed(() => {
    const start = (currentPage.value - 1) * rowsPerPage.value;
    const end = start + rowsPerPage.value;
    return filteredConnections.value.slice(start, end);
});

const selectAllChecked = computed(() => {
    return selectedConnections.value.length > 0 && selectedConnections.value.length === paginatedConnections.value.length;
});

function toggleAllSelection(checked) {
    if (checked) {
        selectedConnections.value = [...paginatedConnections.value];
    } else {
        selectedConnections.value = [];
    }
}

function toggleSelection(item) {
    const index = selectedConnections.value.findIndex((c) => c.id === item.id);
    if (index >= 0) {
        selectedConnections.value.splice(index, 1);
    } else {
        selectedConnections.value.push(item);
    }
}

function isSelected(item) {
    return selectedConnections.value.some((c) => c.id === item.id);
}

function sortBy(field) {
    if (sortField.value === field) {
        sortOrder.value = -sortOrder.value;
    } else {
        sortField.value = field;
        sortOrder.value = 1;
    }
}
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
    { label: 'PostgreSQL', value: 'postgresql', icon: 'Database' },
    { label: 'Oracle', value: 'oracle', icon: 'Database' },
    { label: 'Microsoft SQL Server', value: 'mssql', icon: 'Database' },
    { label: 'MySQL', value: 'mysql', icon: 'Database' }
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
            toast('Success', {
                description: 'Connection updated successfully'
            });
        } else {
            databaseStore.addConnection(connectionData);
            toast('Success', {
                description: 'Connection created successfully'
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
                toast('Success', {
                    description: 'Connection test successful and saved!'
                });
                hideDialog();
            } else {
                toast.error('Connection Failed', {
                    description: 'Could not connect to database. Connection saved anyway.'
                });
            }
        } finally {
            testing.value = false;
        }
    }
}

function isValidConnection() {
    return connection.name?.trim() && connection.type && connection.host?.trim() && connection.port && connection.database?.trim() && connection.username?.trim() && connection.password?.trim();
}

async function testConnection(conn) {
    const success = await databaseStore.testConnection(conn.id);

    if (success) {
        toast('Success', {
            description: `Connected to ${conn.name} successfully`
        });
    } else {
        toast.error('Connection Failed', {
            description: `Could not connect to ${conn.name}`
        });
    }
}

function duplicateConnection(conn) {
    const duplicate = databaseStore.duplicateConnection(conn.id);
    if (duplicate) {
        toast('Success', {
            description: 'Connection duplicated successfully'
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
        toast('Success', {
            description: 'Connection deleted successfully'
        });
    }
    deleteConnectionDialog.value = false;
    resetConnection();
}

function confirmDeleteSelected() {
    deleteSelectedDialog.value = true;
}

function deleteSelectedConnections() {
    selectedConnections.value.forEach((conn) => {
        databaseStore.deleteConnection(conn.id);
    });

    toast('Success', {
        description: `${selectedConnections.value.length} connection(s) deleted successfully`
    });

    selectedConnections.value = [];
    deleteSelectedDialog.value = false;
}

function refreshConnections() {
    databaseStore.loadConnections();
    toast('Refreshed', {
        description: 'Connections list refreshed'
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

    toast('Success', {
        description: 'Connections exported successfully'
    });
}

function importConnections(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const success = databaseStore.importConnections(e.target.result);
            if (success) {
                toast('Success', {
                    description: 'Connections imported successfully'
                });
            } else {
                toast.error('Import Failed', {
                    description: 'Invalid file format'
                });
            }
        };
        reader.readAsText(file);
    }
    event.target.value = '';
}

function getDatabaseIcon(type) {
    return Database;
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
        connected: 'default',
        disconnected: 'secondary',
        error: 'destructive',
        testing: 'secondary'
    };
    return severities[status] || 'secondary';
}

function formatDate(date) {
    return new Date(date).toLocaleString();
}
</script>

<template>
    <div>
        <div class="card bg-card p-6 rounded-lg mb-4 border">
            <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
                <div class="flex flex-wrap items-center gap-2">
                    <Button @click="openNew"> <Plus class="w-4 h-4 mr-2" /> New Connection </Button>
                    <Button variant="destructive" @click="confirmDeleteSelected" :disabled="!selectedConnections || !selectedConnections.length"> <Trash2 class="w-4 h-4 mr-2" /> Delete Selected </Button>
                    <Button variant="outline" @click="exportConnections"> <Download class="w-4 h-4 mr-2" /> Export </Button>
                    <Button variant="outline" @click="$refs.fileInput.click()"> <Upload class="w-4 h-4 mr-2" /> Import </Button>
                    <input ref="fileInput" type="file" accept=".json" @change="importConnections" class="hidden" />
                </div>

                <div class="flex items-center gap-4">
                    <span class="text-sm text-muted-foreground"> {{ databaseStore.connectedCount }} of {{ databaseStore.allConnections.length }} connected </span>
                    <Button variant="outline" size="icon" @click="refreshConnections" :disabled="databaseStore.isLoading">
                        <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': databaseStore.isLoading }" />
                    </Button>
                </div>
            </div>

            <div class="flex justify-between items-center mb-4">
                <h2 class="text-xl font-semibold m-0">Database Connections</h2>
                <div class="relative w-64">
                    <Search class="w-4 h-4 absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground" />
                    <Input v-model="filters.global.value" placeholder="Search connections..." class="pl-9" />
                </div>
            </div>

            <div class="rounded-md border overflow-hidden">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead class="w-12">
                                <Checkbox :checked="selectAllChecked" @update:checked="toggleAllSelection" />
                            </TableHead>
                            <TableHead class="min-w-[12rem] cursor-pointer" @click="sortBy('name')">Name <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[8rem] cursor-pointer" @click="sortBy('type')">Type <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[10rem] cursor-pointer" @click="sortBy('host')">Host <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[10rem] cursor-pointer" @click="sortBy('database')">Database <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[8rem] cursor-pointer" @click="sortBy('status')">Status <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[10rem] cursor-pointer" @click="sortBy('lastTested')">Last Tested <ArrowUpDown class="w-3 h-3 inline-block ml-1 text-muted-foreground" /></TableHead>
                            <TableHead class="min-w-[12rem]">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        <TableRow v-if="!paginatedConnections.length">
                            <TableCell colspan="8" class="text-center py-10 text-muted-foreground">
                                <div class="flex flex-col items-center justify-center">
                                    <Database class="w-16 h-16 mb-4 opacity-50 text-muted-foreground" />
                                    <div class="text-xl mb-2 font-medium text-foreground">No database connections found</div>
                                    <div class="mb-4">Get started by creating your first database connection</div>
                                    <Button @click="openNew"> <Plus class="w-4 h-4 mr-2" /> Add Connection </Button>
                                </div>
                            </TableCell>
                        </TableRow>
                        <TableRow v-for="data in paginatedConnections" :key="data.id">
                            <TableCell>
                                <Checkbox :checked="isSelected(data)" @update:checked="toggleSelection(data)" />
                            </TableCell>
                            <TableCell>
                                <div class="flex items-center gap-2">
                                    <component :is="getDatabaseIcon(data.type)" class="w-5 h-5 text-zinc-500" />
                                    <span class="font-medium">{{ data.name }}</span>
                                </div>
                            </TableCell>
                            <TableCell>
                                <Badge variant="secondary" class="uppercase text-[10px] tracking-wider">{{ data.type }}</Badge>
                            </TableCell>
                            <TableCell>
                                <span>{{ data.host }}:{{ data.port }}</span>
                            </TableCell>
                            <TableCell>{{ data.database }}</TableCell>
                            <TableCell>
                                <Badge :variant="getStatusSeverity(data.status)">
                                    {{ getStatusLabel(data.status) }}
                                </Badge>
                            </TableCell>
                            <TableCell>
                                <span v-if="data.lastTested" class="text-sm text-muted-foreground">
                                    {{ formatDate(data.lastTested) }}
                                </span>
                                <span v-else class="text-sm text-muted-foreground">Never</span>
                            </TableCell>
                            <TableCell>
                                <div class="flex gap-1">
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        class="h-8 w-8 text-green-600 hover:text-green-700 hover:bg-green-100 dark:hover:bg-green-900/30"
                                        @click="testConnection(data)"
                                        :disabled="data.status === 'testing'"
                                        title="Test Connection"
                                    >
                                        <Loader2 v-if="data.status === 'testing'" class="w-4 h-4 animate-spin" />
                                        <Check v-else class="w-4 h-4" />
                                    </Button>
                                    <Button variant="ghost" size="icon" class="h-8 w-8" @click="editConnection(data)" title="Edit">
                                        <Pencil class="w-4 h-4" />
                                    </Button>
                                    <Button variant="ghost" size="icon" class="h-8 w-8" @click="duplicateConnection(data)" title="Duplicate">
                                        <Copy class="w-4 h-4" />
                                    </Button>
                                    <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive hover:bg-destructive/10" @click="confirmDeleteConnection(data)" title="Delete">
                                        <Trash2 class="w-4 h-4" />
                                    </Button>
                                </div>
                            </TableCell>
                        </TableRow>
                    </TableBody>
                </Table>
            </div>

            <div class="flex items-center justify-between mt-4">
                <div class="text-sm text-muted-foreground">Showing {{ paginationStart }} to {{ paginationEnd }} of {{ filteredConnections.length }} connections</div>
                <div class="flex gap-1 items-center">
                    <span class="text-sm mr-2 text-muted-foreground">Rows per page:</span>
                    <select v-model="rowsPerPage" class="border rounded px-2 py-1 text-sm bg-background mr-4">
                        <option :value="5">5</option>
                        <option :value="10">10</option>
                        <option :value="25">25</option>
                    </select>
                    <Button variant="outline" size="sm" :disabled="currentPage === 1" @click="currentPage--">Previous</Button>
                    <span class="text-sm px-2">Page {{ currentPage }} of {{ totalPages || 1 }}</span>
                    <Button variant="outline" size="sm" :disabled="currentPage >= totalPages" @click="currentPage++">Next</Button>
                </div>
            </div>
        </div>

        <Dialog :open="connectionDialog" @update:open="connectionDialog = $event">
            <DialogContent class="sm:max-w-[600px] p-0 overflow-hidden">
                <DialogHeader class="px-6 py-4 pb-0">
                    <DialogTitle>{{ editMode ? 'Edit Connection' : 'New Connection' }}</DialogTitle>
                </DialogHeader>

                <div class="px-6 py-4">
                    <form @submit.prevent="saveConnection" class="space-y-4">
                        <div class="grid grid-cols-12 gap-4">
                            <div class="col-span-12">
                                <Label for="name" class="font-semibold mb-2 block">Connection Name</Label>
                                <Input id="name" v-model="connection.name" :class="{ 'border-destructive': submitted && !connection.name }" placeholder="Enter connection name" />
                                <span v-if="submitted && !connection.name" class="text-xs text-destructive mt-1 block">Name is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="type" class="font-semibold mb-2 block">Database Type</Label>
                                <select
                                    id="type"
                                    v-model="connection.type"
                                    @change="onTypeChange"
                                    class="flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                    :class="{ 'border-destructive': submitted && !connection.type }"
                                >
                                    <option value="" disabled selected>Select database type</option>
                                    <option v-for="type in databaseTypes" :key="type.value" :value="type.value">
                                        {{ type.label }}
                                    </option>
                                </select>
                                <span v-if="submitted && !connection.type" class="text-xs text-destructive mt-1 block">Database type is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="host" class="font-semibold mb-2 block">Host</Label>
                                <Input id="host" v-model="connection.host" :class="{ 'border-destructive': submitted && !connection.host }" placeholder="localhost" />
                                <span v-if="submitted && !connection.host" class="text-xs text-destructive mt-1 block">Host is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="port" class="font-semibold mb-2 block">Port</Label>
                                <Input id="port" type="number" v-model.number="connection.port" :class="{ 'border-destructive': submitted && !connection.port }" :min="1" :max="65535" />
                                <span v-if="submitted && !connection.port" class="text-xs text-destructive mt-1 block">Port is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="database" class="font-semibold mb-2 block">Database Name</Label>
                                <Input id="database" v-model="connection.database" :class="{ 'border-destructive': submitted && !connection.database }" placeholder="Database name" />
                                <span v-if="submitted && !connection.database" class="text-xs text-destructive mt-1 block">Database name is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="username" class="font-semibold mb-2 block">Username</Label>
                                <Input id="username" v-model="connection.username" :class="{ 'border-destructive': submitted && !connection.username }" placeholder="Database username" />
                                <span v-if="submitted && !connection.username" class="text-xs text-destructive mt-1 block">Username is required.</span>
                            </div>

                            <div class="col-span-12 md:col-span-6">
                                <Label for="password" class="font-semibold mb-2 block">Password</Label>
                                <Input id="password" type="password" v-model="connection.password" :class="{ 'border-destructive': submitted && !connection.password }" placeholder="Database password" />
                                <span v-if="submitted && !connection.password" class="text-xs text-destructive mt-1 block">Password is required.</span>
                            </div>

                            <div class="col-span-12 mt-4 pt-4 border-t">
                                <Label for="connectionString" class="font-semibold mb-2 block">
                                    Custom Connection String
                                    <span class="text-muted-foreground font-normal ml-1 text-sm">(Optional)</span>
                                </Label>
                                <textarea
                                    id="connectionString"
                                    v-model="connection.connectionString"
                                    rows="3"
                                    class="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                    placeholder="Override with custom connection string"
                                ></textarea>
                                <span class="text-xs text-muted-foreground mt-1 block"> Leave empty to use the connection details above </span>
                            </div>
                        </div>
                    </form>
                </div>

                <DialogFooter class="px-6 py-4 bg-muted/50 sm:justify-between border-t">
                    <Button variant="outline" @click="hideDialog"> <X class="w-4 h-4 mr-2" /> Cancel </Button>
                    <div class="flex gap-2">
                        <Button variant="secondary" @click="testAndSave" :disabled="testing">
                            <Loader2 v-if="testing" class="w-4 h-4 mr-2 animate-spin" />
                            <Check v-else class="w-4 h-4 mr-2" />
                            {{ testing ? 'Testing...' : 'Test & Save' }}
                        </Button>
                        <Button @click="saveConnection"> <Save class="w-4 h-4 mr-2" /> Save </Button>
                    </div>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <!-- Delete Confirmation Dialog -->
        <Dialog :open="deleteConnectionDialog" @update:open="deleteConnectionDialog = $event">
            <DialogContent class="sm:max-w-[450px]">
                <DialogHeader>
                    <DialogTitle>Confirm Deletion</DialogTitle>
                </DialogHeader>
                <div class="flex items-center justify-center py-4">
                    <AlertTriangle class="w-8 h-8 mr-3 text-destructive" />
                    <span v-if="connection">
                        Are you sure you want to delete <b>{{ connection.name }}</b
                        >?
                    </span>
                </div>
                <DialogFooter>
                    <Button variant="outline" @click="deleteConnectionDialog = false"> <X class="w-4 h-4 mr-2" /> No </Button>
                    <Button variant="destructive" @click="deleteConnection"> <Check class="w-4 h-4 mr-2" /> Yes </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <!-- Delete Selected Confirmation Dialog -->
        <Dialog :open="deleteSelectedDialog" @update:open="deleteSelectedDialog = $event">
            <DialogContent class="sm:max-w-[450px]">
                <DialogHeader>
                    <DialogTitle>Confirm Deletion</DialogTitle>
                </DialogHeader>
                <div class="flex items-center justify-center py-4">
                    <AlertTriangle class="w-8 h-8 mr-3 text-destructive" />
                    <span v-if="selectedConnections"> Are you sure you want to delete {{ selectedConnections.length }} selected connection(s)? </span>
                </div>
                <DialogFooter>
                    <Button variant="outline" @click="deleteSelectedDialog = false"> <X class="w-4 h-4 mr-2" /> No </Button>
                    <Button variant="destructive" @click="deleteSelectedConnections"> <Check class="w-4 h-4 mr-2" /> Yes </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
<style>
.card {
    background: var(--surface-card);
    padding: 2rem;
    border-radius: 10px;
    margin-bottom: 1rem;
}
</style>
