import { defineStore } from "pinia";
import { userStoreMe } from "@/store/userStore";
import { useProjectStore } from "@/store/projectStore";

export interface DatabaseConnection {
  id: string;
  name: string;
  type: 'postgresql' | 'oracle' | 'mssql' | 'mysql';
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  connectionString?: string;
  status: 'connected' | 'disconnected' | 'error' | 'testing';
  lastTested?: Date;
  createdAt: Date;
  updatedAt: Date;
}

const DEFAULT_PORTS = {
  postgresql: 5432,
  oracle: 1521,
  mssql: 1433,
  mysql: 3306
};

export const useDatabaseStore = defineStore({
  id: "database",
  state: () => ({
    connections: [] as DatabaseConnection[],
    loading: false
  }),

  getters: {
    allConnections: (state) => state.connections,
    connectionById: (state) => (id: string) => state.connections.find(conn => conn.id === id),
    connectionsByType: (state) => (type: string) => state.connections.filter(conn => conn.type === type),
    connectedCount: (state) => state.connections.filter(conn => conn.status === 'connected').length,
    isLoading: (state) => state.loading
  },

  actions: {
    async loadConnections(socket: any) {
      try {
        const userStore = userStoreMe();
        const projectStore = useProjectStore();
        const params = projectStore.currentProjectId ? { projectId: projectStore.currentProjectId } : {};
        const result = await userStore.executeCommand('LoadDatabaseConnections', params, socket);
        if (result && result.Data) {
          this.connections = result.Data.map((conn: any) => ({
            id: conn.id || conn.Id,
            name: conn.name || conn.Name || '',
            type: ((conn.type || conn.Type || '') as DatabaseConnection['type']),
            host: conn.host || conn.Host || '',
            port: conn.port || conn.Port || 0,
            // PostgreSQL lowercases column names: DatabaseName → databasename
            database: conn.database || conn.Database || conn.databasename || conn.DatabaseName || '',
            username: conn.username || conn.Username || '',
            password: conn.password || conn.Password || '',
            connectionString: conn.connectionString || conn.ConnectionString || conn.connectionstring || '',
            isGlobal: conn.isGlobal || conn.IsGlobal || conn.isglobal || false,
            sharedWith: conn.sharedWith || conn.SharedWith || conn.sharedwith || '',
            status: 'disconnected' as DatabaseConnection['status'],
            createdAt: new Date(conn.createdAt || conn.CreatedAt || conn.createdat || Date.now()),
            updatedAt: new Date(conn.updatedAt || conn.UpdatedAt || conn.updatedat || conn.createdAt || conn.CreatedAt || conn.createdat || Date.now()),
            lastTested: undefined
          }));
        }
      } catch (error) {
        console.error('Error loading database connections:', error);
        this.connections = [];
      }
    },

    async addConnection(connection: Omit<DatabaseConnection, 'id' | 'createdAt' | 'updatedAt' | 'status'>, socket: any) {
      const newConnection: DatabaseConnection = {
        ...connection,
        id: this.generateId(),
        status: 'disconnected',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      try {
        const userStore = userStoreMe();
        await userStore.executeCommand('SaveDatabaseConnection', { connection: newConnection }, socket);
        this.connections.push(newConnection);
        return newConnection;
      } catch (error) {
        console.error('Error adding database connection:', error);
        throw error;
      }
    },

    async updateConnection(id: string, updates: Partial<DatabaseConnection>, socket: any) {
      const index = this.connections.findIndex(conn => conn.id === id);
      if (index !== -1) {
        const updatedConnection = {
          ...this.connections[index],
          ...updates,
          updatedAt: new Date()
        };
        try {
          const userStore = userStoreMe();
          await userStore.executeCommand('SaveDatabaseConnection', { connection: updatedConnection }, socket);
          this.connections[index] = updatedConnection;
          return updatedConnection;
        } catch (error) {
          console.error('Error updating database connection:', error);
          throw error;
        }
      }
      return null;
    },

    async deleteConnection(id: string, socket: any) {
      const index = this.connections.findIndex(conn => conn.id === id);
      if (index !== -1) {
        try {
          const userStore = userStoreMe();
          await userStore.executeCommand('DeleteDatabaseConnection', { id }, socket);
          this.connections.splice(index, 1);
          return true;
        } catch (error) {
          console.error('Error deleting database connection:', error);
          throw error;
        }
      }
      return false;
    },

    async duplicateConnection(id: string, socket: any) {
      const original = this.connectionById(id);
      if (original) {
        const duplicate = {
          ...original,
          name: `${original.name} (Copy)`,
          status: 'disconnected' as const,
          lastTested: undefined
        };
        delete (duplicate as any).id;
        delete (duplicate as any).createdAt;
        delete (duplicate as any).updatedAt;
        return this.addConnection(duplicate, socket);
      }
      return null;
    },

    async testConnection(id: string, socket: any): Promise<boolean> {
      const connection = this.connectionById(id);
      if (!connection) return false;

      // Update state locally first to show testing UI
      const index = this.connections.findIndex(conn => conn.id === id);
      if (index !== -1) {
          this.connections[index].status = 'testing';
      }
      
      this.loading = true;

      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('TestDatabaseConnection', { connection }, socket);
        const success = result?.Data?.success === true;
        
        await this.updateConnection(id, {
          status: success ? 'connected' : 'error',
          lastTested: new Date()
        }, socket);

        return success;
      } catch (error) {
        await this.updateConnection(id, {
          status: 'error',
          lastTested: new Date()
        }, socket);
        return false;
      } finally {
        this.loading = false;
      }
    },

    getDefaultPort(type: DatabaseConnection['type']): number {
      return DEFAULT_PORTS[type];
    },

    generateId(): string {
      // Must be a valid GUID — server stores connection Id as uniqueidentifier/uuid
      if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID();
      }
      // Fallback RFC4122 v4 generator
      return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = (Math.random() * 16) | 0;
        const v = c === 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      });
    },

    exportConnections(): string {
      return JSON.stringify(this.connections, null, 2);
    },

    async importConnections(data: string, socket: any): Promise<boolean> {
      try {
        const parsed = JSON.parse(data);
        if (Array.isArray(parsed)) {
          const newConnections = parsed.map((conn: any) => ({
            ...conn,
            id: this.generateId(), // Generate new IDs to avoid conflicts
            createdAt: new Date(conn.createdAt || new Date()),
            updatedAt: new Date(conn.updatedAt || new Date()),
            lastTested: conn.lastTested ? new Date(conn.lastTested) : undefined,
            status: 'disconnected' // Reset status on import
          }));
          
          for (const conn of newConnections) {
             const userStore = userStoreMe();
             await userStore.executeCommand('SaveDatabaseConnection', { connection: conn }, socket);
          }
          
          this.connections = newConnections;
          return true;
        }
      } catch (error) {
        console.error('Error importing connections:', error);
      }
      return false;
    }
  }
});