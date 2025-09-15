import { defineStore } from "pinia";

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
    loadConnections() {
      try {
        const saved = localStorage.getItem('database-connections');
        if (saved) {
          this.connections = JSON.parse(saved).map((conn: any) => ({
            ...conn,
            createdAt: new Date(conn.createdAt),
            updatedAt: new Date(conn.updatedAt),
            lastTested: conn.lastTested ? new Date(conn.lastTested) : undefined
          }));
        }
      } catch (error) {
        console.error('Error loading database connections:', error);
        this.connections = [];
      }
    },

    saveConnections() {
      try {
        localStorage.setItem('database-connections', JSON.stringify(this.connections));
      } catch (error) {
        console.error('Error saving database connections:', error);
      }
    },

    addConnection(connection: Omit<DatabaseConnection, 'id' | 'createdAt' | 'updatedAt' | 'status'>) {
      const newConnection: DatabaseConnection = {
        ...connection,
        id: this.generateId(),
        status: 'disconnected',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      this.connections.push(newConnection);
      this.saveConnections();
      return newConnection;
    },

    updateConnection(id: string, updates: Partial<DatabaseConnection>) {
      const index = this.connections.findIndex(conn => conn.id === id);
      if (index !== -1) {
        this.connections[index] = {
          ...this.connections[index],
          ...updates,
          updatedAt: new Date()
        };
        this.saveConnections();
        return this.connections[index];
      }
      return null;
    },

    deleteConnection(id: string) {
      const index = this.connections.findIndex(conn => conn.id === id);
      if (index !== -1) {
        this.connections.splice(index, 1);
        this.saveConnections();
        return true;
      }
      return false;
    },

    duplicateConnection(id: string) {
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
        return this.addConnection(duplicate);
      }
      return null;
    },

    async testConnection(id: string): Promise<boolean> {
      const connection = this.connectionById(id);
      if (!connection) return false;

      this.updateConnection(id, { status: 'testing' });
      this.loading = true;

      try {
        // Simulate connection test - replace with actual connection logic
        await new Promise(resolve => setTimeout(resolve, 2000));

        // For demo purposes, randomly succeed or fail
        const success = Math.random() > 0.3;

        this.updateConnection(id, {
          status: success ? 'connected' : 'error',
          lastTested: new Date()
        });

        return success;
      } catch (error) {
        this.updateConnection(id, {
          status: 'error',
          lastTested: new Date()
        });
        return false;
      } finally {
        this.loading = false;
      }
    },

    getDefaultPort(type: DatabaseConnection['type']): number {
      return DEFAULT_PORTS[type];
    },

    generateId(): string {
      return Date.now().toString(36) + Math.random().toString(36).substr(2);
    },

    exportConnections(): string {
      return JSON.stringify(this.connections, null, 2);
    },

    importConnections(data: string): boolean {
      try {
        const parsed = JSON.parse(data);
        if (Array.isArray(parsed)) {
          this.connections = parsed.map((conn: any) => ({
            ...conn,
            id: this.generateId(), // Generate new IDs to avoid conflicts
            createdAt: new Date(conn.createdAt || new Date()),
            updatedAt: new Date(conn.updatedAt || new Date()),
            lastTested: conn.lastTested ? new Date(conn.lastTested) : undefined,
            status: 'disconnected' // Reset status on import
          }));
          this.saveConnections();
          return true;
        }
      } catch (error) {
        console.error('Error importing connections:', error);
      }
      return false;
    }
  }
});