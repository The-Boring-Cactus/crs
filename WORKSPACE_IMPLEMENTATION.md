# Workspace CRUD Implementation

## Overview

This implementation adds create, read, update, and delete (CRUD) functionality to Vue components using WebSocket connections and database storage via the ReportsCache system. The data is persisted in either PostgreSQL or MSSQL databases based on configuration.

## Architecture

### Server-Side Components

#### 1. Data Models (`Server/Core/Models/WorkspaceData.cs`)

Defines the data structure for storing workspace information:

- **WorkspaceData**: Base model for all workspace types
  - `Id`: Unique identifier
  - `UserId`: Owner of the workspace
  - `WorkspaceType`: Type of workspace (CsEditor, SqlEditor, Dashboard, Excel, Database)
  - `Name`: User-defined name
  - `Description`: Optional description
  - `Content`: Type-specific content (stored as JSON)
  - `CreatedAt`, `UpdatedAt`: Timestamps
  - `Metadata`: Additional key-value data

- **Content Models**:
  - `CsEditorContent`: C# code and language
  - `SqlEditorContent`: SQL query and database ID
  - `DashboardContent`: Dashboard title and components
  - `ExcelContent`: Spreadsheet sheets and data
  - `DatabaseContent`: Connection information

#### 2. Workspace Service (`Server/Core/WorkspaceService.cs`)

Implements `IWorkspaceService` interface with methods:

- `GetWorkspaceAsync(id, userId)`: Retrieve a specific workspace
- `ListWorkspacesAsync(userId, workspaceType?)`: List all workspaces for a user, optionally filtered by type
- `SaveWorkspaceAsync(workspace)`: Create or update a workspace
- `DeleteWorkspaceAsync(id, userId)`: Delete a workspace

The service uses the ReportsCache for persistent storage with appropriate TTLs:
- Individual workspaces: 365 days
- Workspace lists: 5 minutes (for faster updates)

#### 3. WebSocket Message Handlers (`Server/Core/WebSocketManager.cs`)

Added handlers for `DataMessage` type with the following data types:

- `save_workspace`: Save/update a workspace
- `load_workspace`: Load a specific workspace by ID
- `list_workspaces`: List all workspaces (optionally filtered by type)
- `delete_workspace`: Delete a workspace by ID

#### 4. Program Setup (`Server/Program.cs`)

The `WorkspaceService` is instantiated and passed to `WebSocketManager`:

```csharp
var workspaceService = new WorkspaceService(cache);
var webSocketManager = new WebSocketManager(authService, reportsService, workspaceService);
```

### Client-Side Components

#### 1. Workspace Composable (`ui/src/composables/useWorkspace.js`)

Reusable Vue composable that provides:

**State:**
- `workspaces`: Array of available workspaces
- `currentWorkspace`: Currently loaded workspace
- `isLoading`: Loading state indicator
- `isSaving`: Saving state indicator

**Methods:**
- `saveWorkspace(workspaceData)`: Save workspace to server
- `loadWorkspace(workspaceId)`: Load workspace from server
- `listWorkspaces()`: Fetch list of workspaces
- `deleteWorkspace(workspaceId)`: Delete workspace
- `createNew(initialData)`: Create new blank workspace

**Usage Example:**

```javascript
const {
    workspaces,
    currentWorkspace,
    isLoading,
    isSaving,
    saveWorkspace,
    loadWorkspace,
    listWorkspaces,
    deleteWorkspace,
    createNew
} = useWorkspace('CsEditor');
```

#### 2. Updated Components

##### CsEditor.vue

**Changes Made:**
- Replaced localStorage-based persistence with WebSocket/database persistence
- Integrated `useWorkspace` composable
- Updated `saveScript()` to use `saveWorkspace()`
- Updated `loadScript()` to use `listWorkspaces()`
- Updated `confirmLoadScript()` to use `loadWorkspace()`
- Updated `deleteScript()` to use `deleteWorkspace()`
- Modified `onMounted()` to load workspaces from server

**Features:**
- Scripts are saved to database with metadata
- Scripts can be loaded from any device/session
- Script list is synchronized across sessions
- Delete operations update server immediately

## Database Schema

The data is stored in the cache table (PostgreSQL or MSSQL) with the following structure:

### PostgreSQL
```sql
reports_cache (
    key VARCHAR(500) PRIMARY KEY,      -- Format: "workspace:{userId}:{workspaceId}"
    data TEXT NOT NULL,                -- JSON serialized WorkspaceData
    timestamp BIGINT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
)
```

### MSSQL
```sql
ReportsCache (
    [Key] NVARCHAR(500) PRIMARY KEY,   -- Format: "workspace:{userId}:{workspaceId}"
    [Data] NVARCHAR(MAX) NOT NULL,     -- JSON serialized WorkspaceData
    [Timestamp] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
```

### Cache Keys

- Individual workspace: `workspace:{userId}:{workspaceId}`
- All workspaces list: `workspaces:{userId}:all`
- Type-specific list: `workspaces:{userId}:{workspaceType}`

## WebSocket Message Protocol

### Save Workspace

**Request:**
```javascript
client.sendData('save_workspace', {
    id: 'workspace-123',              // null for new workspace
    workspaceType: 'CsEditor',
    name: 'My Script',
    description: 'Description here',
    content: {
        code: 'Console.WriteLine("Hello");',
        language: 'csharp'
    },
    metadata: {
        lastExecuted: '2025-01-01T00:00:00Z'
    }
}, {});
```

**Response:**
```json
{
    "Type": 3,
    "Status": "Success",
    "Data": {
        "id": "workspace-123",
        "userId": "user-456",
        "workspaceType": "CsEditor",
        "name": "My Script",
        "description": "Description here",
        "content": { ... },
        "createdAt": "2025-01-01T00:00:00Z",
        "updatedAt": "2025-01-01T00:00:00Z",
        "metadata": { ... }
    }
}
```

### Load Workspace

**Request:**
```javascript
client.sendData('load_workspace', {}, {
    workspaceId: 'workspace-123'
});
```

**Response:**
```json
{
    "Type": 3,
    "Status": "Success",
    "Data": {
        "id": "workspace-123",
        "name": "My Script",
        "content": { ... },
        ...
    }
}
```

### List Workspaces

**Request:**
```javascript
client.sendData('list_workspaces', {}, {
    workspaceType: 'CsEditor'  // Optional filter
});
```

**Response:**
```json
{
    "Type": 3,
    "Status": "Success",
    "Data": [
        {
            "id": "workspace-123",
            "name": "My Script",
            "workspaceType": "CsEditor",
            ...
        },
        ...
    ]
}
```

### Delete Workspace

**Request:**
```javascript
client.sendData('delete_workspace', {}, {
    workspaceId: 'workspace-123'
});
```

**Response:**
```json
{
    "Type": 3,
    "Status": "Success",
    "Data": {
        "success": true,
        "workspaceId": "workspace-123"
    }
}
```

## How to Implement for Other Components

To add workspace functionality to Dashboard.vue, SqlEditor.vue, Databases.vue, or MyExcel.vue:

### 1. Import the composable

```javascript
import { useWorkspace } from '@/composables/useWorkspace';

const {
    workspaces,
    currentWorkspace,
    saveWorkspace,
    loadWorkspace,
    listWorkspaces,
    deleteWorkspace,
    createNew
} = useWorkspace('Dashboard'); // Use appropriate workspace type
```

### 2. Create save function

```javascript
const saveDashboard = async () => {
    try {
        const workspace = await saveWorkspace({
            id: currentDashboard.id,
            name: dashboardName.value,
            description: 'My dashboard',
            content: {
                title: dashboardTitle.value,
                components: layout.componentes // Your component-specific data
            },
            metadata: {
                componentCount: layout.componentes.length
            }
        });

        currentDashboard.id = workspace.id || workspace.Id;
    } catch (error) {
        // Handle error
    }
};
```

### 3. Create load function

```javascript
const loadDashboard = async (workspaceId) => {
    try {
        const workspace = await loadWorkspace(workspaceId);

        // Apply loaded data to component state
        dashboardTitle.value = workspace.content.title;
        layout.componentes = workspace.content.components;
    } catch (error) {
        // Handle error
    }
};
```

### 4. List workspaces on mount

```javascript
onMounted(async () => {
    try {
        await listWorkspaces();
    } catch (error) {
        console.error('Failed to load workspaces:', error);
    }
});
```

### 5. Add UI elements

Add buttons/dialogs for:
- Save current workspace
- Load workspace (with selection dialog)
- Create new workspace
- Delete workspace

## Component-Specific Content Structures

### Dashboard
```javascript
content: {
    title: 'Dashboard Name',
    components: [
        {
            i: 'component-1',
            x: 0, y: 0, w: 4, h: 3,
            type: 'LineChart',
            title: 'Sales Over Time',
            chartData: { ... },
            ...
        }
    ]
}
```

### SqlEditor
```javascript
content: {
    query: 'SELECT * FROM users',
    databaseId: 'db-connection-1'
}
```

### MyExcel
```javascript
content: {
    sheets: [
        {
            name: 'Sheet1',
            data: [
                ['A1', 'B1', 'C1'],
                ['A2', 'B2', 'C2']
            ]
        }
    ]
}
```

### Databases
```javascript
content: {
    connectionString: 'Server=localhost;Database=mydb;...',
    databaseType: 'MSSQL',
    host: 'localhost',
    port: 1433,
    database: 'mydb',
    username: 'user',
    password: 'encrypted-password'
}
```

## Security Considerations

1. **User Isolation**: Workspaces are isolated by `userId` - users can only access their own workspaces
2. **Password Encryption**: Database passwords in DatabaseContent should be encrypted before storage (not implemented in current version)
3. **Validation**: Server validates workspace ownership before delete/update operations
4. **SQL Injection**: All database operations use parameterized queries

## Testing

### Server-Side Testing

1. Build the server project:
```bash
cd Server
dotnet build
```

2. Run the server:
```bash
dotnet run
```

### Client-Side Testing

1. Open the CsEditor component
2. Create a new script or modify existing code
3. Click "Save Script" - verify toast notification appears
4. Click "Open Script" - verify saved scripts appear in the list
5. Select and load a script - verify code loads correctly
6. Delete a script - verify it's removed from the list
7. Refresh the page - verify scripts persist across sessions

## Troubleshooting

### Workspaces not saving

1. Check WebSocket connection is established
2. Verify cache configuration in `appsettings.json`
3. Check database connection string
4. Look for errors in browser console and server logs

### Workspaces not loading

1. Verify `listWorkspaces()` is called on component mount
2. Check WebSocket message handler is properly set up
3. Ensure user ID is being set in ConnectionInfo

### Delete not working

1. Check workspace ID is being passed correctly
2. Verify user owns the workspace being deleted
3. Check cache invalidation is working

## Future Enhancements

1. **Sharing**: Allow users to share workspaces with other users
2. **Versioning**: Track workspace versions and allow rollback
3. **Export/Import**: Export workspaces to JSON files
4. **Templates**: Create workspace templates for common use cases
5. **Search**: Add full-text search across workspace names and content
6. **Tags**: Allow tagging workspaces for better organization
7. **Folders**: Organize workspaces into folders/categories
8. **Offline Support**: Cache workspaces locally for offline access
9. **Collaboration**: Real-time collaborative editing
10. **Audit Log**: Track all changes to workspaces

## Summary

This implementation provides a robust, scalable solution for persisting user workspace data across sessions using:

- WebSocket-based real-time communication
- Database persistence (PostgreSQL or MSSQL)
- Reusable Vue composable for easy integration
- Type-safe server-side models and services
- User isolation and security

The pattern demonstrated with CsEditor.vue can be easily replicated for all other components (Dashboard, SqlEditor, Databases, MyExcel).
