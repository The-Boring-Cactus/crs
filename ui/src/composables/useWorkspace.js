import { ref, computed, getCurrentInstance } from 'vue';
import { useToast } from 'primevue/usetoast';
import { WebSocketMessageClient } from '@/websocket/WebSocketMessageClient';

export function useWorkspace(workspaceType) {
    const { proxy } = getCurrentInstance();
    const toast = useToast();
    const client = new WebSocketMessageClient(proxy.$socket);

    const workspaces = ref([]);
    const currentWorkspace = ref(null);
    const isLoading = ref(false);
    const isSaving = ref(false);

    // WebSocket message handler
    const setupMessageHandler = (onResponse) => {
        const originalHandler = proxy.$socket.onmessage;

        proxy.$socket.onmessage = (event) => {
            try {
                const payload = JSON.parse(event.data);

                // Handle workspace-related responses
                if (payload.Type === 3) { // Response type
                    const response = payload;

                    if (response.Status === 'Success' && response.Data) {
                        onResponse(response);
                    } else if (response.Status === 'Error') {
                        toast.add({
                            severity: 'error',
                            summary: 'Error',
                            detail: response.ErrorMessage || 'An error occurred',
                            life: 5000
                        });
                        isLoading.value = false;
                        isSaving.value = false;
                    }
                }

                // Call original handler if it exists
                if (originalHandler) {
                    originalHandler(event);
                }
            } catch (error) {
                console.error('Error handling WebSocket message:', error);
            }
        };
    };

    /**
     * Save a workspace
     * @param {Object} workspaceData - The workspace data to save
     * @returns {Promise<Object>} The saved workspace
     */
    const saveWorkspace = (workspaceData) => {
        return new Promise((resolve, reject) => {
            isSaving.value = true;

            const workspace = {
                id: workspaceData.id || null,
                workspaceType: workspaceType,
                name: workspaceData.name,
                description: workspaceData.description || '',
                content: workspaceData.content,
                metadata: workspaceData.metadata || {}
            };

            setupMessageHandler((response) => {
                if (response.Data && (response.Data.id || response.Data.Id)) {
                    currentWorkspace.value = response.Data;
                    isSaving.value = false;

                    toast.add({
                        severity: 'success',
                        summary: 'Saved',
                        detail: `${workspaceType} saved successfully`,
                        life: 3000
                    });

                    resolve(response.Data);
                }
            });

            client.sendData('save_workspace', workspace, {});
        });
    };

    /**
     * Load a workspace by ID
     * @param {string} workspaceId - The workspace ID to load
     * @returns {Promise<Object>} The loaded workspace
     */
    const loadWorkspace = (workspaceId) => {
        return new Promise((resolve, reject) => {
            isLoading.value = true;

            setupMessageHandler((response) => {
                if (response.Data) {
                    currentWorkspace.value = response.Data;
                    isLoading.value = false;

                    toast.add({
                        severity: 'success',
                        summary: 'Loaded',
                        detail: `${workspaceType} loaded successfully`,
                        life: 3000
                    });

                    resolve(response.Data);
                }
            });

            client.sendData('load_workspace', {}, { workspaceId });
        });
    };

    /**
     * List all workspaces of the current type
     * @returns {Promise<Array>} List of workspaces
     */
    const listWorkspaces = () => {
        return new Promise((resolve, reject) => {
            isLoading.value = true;

            setupMessageHandler((response) => {
                if (response.Data && Array.isArray(response.Data)) {
                    workspaces.value = response.Data;
                    isLoading.value = false;
                    resolve(response.Data);
                } else if (response.Data) {
                    // Handle case where Data might be an object containing an array
                    const data = response.Data;
                    if (data.workspaces && Array.isArray(data.workspaces)) {
                        workspaces.value = data.workspaces;
                    } else {
                        workspaces.value = [];
                    }
                    isLoading.value = false;
                    resolve(workspaces.value);
                }
            });

            client.sendData('list_workspaces', {}, { workspaceType });
        });
    };

    /**
     * Delete a workspace by ID
     * @param {string} workspaceId - The workspace ID to delete
     * @returns {Promise<boolean>} Success status
     */
    const deleteWorkspace = (workspaceId) => {
        return new Promise((resolve, reject) => {
            setupMessageHandler((response) => {
                if (response.Data && response.Data.success) {
                    workspaces.value = workspaces.value.filter(w => w.id !== workspaceId);

                    toast.add({
                        severity: 'success',
                        summary: 'Deleted',
                        detail: `${workspaceType} deleted successfully`,
                        life: 3000
                    });

                    resolve(true);
                } else {
                    reject(new Error('Delete failed'));
                }
            });

            client.sendData('delete_workspace', {}, { workspaceId });
        });
    };

    /**
     * Create a new workspace
     * @param {Object} initialData - Initial workspace data
     */
    const createNew = (initialData = {}) => {
        currentWorkspace.value = {
            id: null,
            workspaceType: workspaceType,
            name: '',
            description: '',
            content: initialData.content || {},
            metadata: initialData.metadata || {}
        };

        toast.add({
            severity: 'info',
            summary: 'New Workspace',
            detail: `New ${workspaceType} created`,
            life: 2000
        });
    };

    return {
        workspaces,
        currentWorkspace,
        isLoading,
        isSaving,
        saveWorkspace,
        loadWorkspace,
        listWorkspaces,
        deleteWorkspace,
        createNew
    };
}
