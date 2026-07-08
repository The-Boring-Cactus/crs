import { defineStore } from "pinia";
import { userStoreMe } from "@/store/userStore";
import { useProjectStore } from "@/store/projectStore";

export const useExcelStore = defineStore({
  id: "excel",
  state: () => ({
    excels: [] as any[],
    loading: false
  }),
  actions: {
    async loadExcels(socket: any) {
      try {
        const userStore = userStoreMe();
        const projectStore = useProjectStore();
        const params = projectStore.currentProjectId ? { projectId: projectStore.currentProjectId } : {};
        const result = await userStore.executeCommand('LoadExcels', params, socket);
        if (result && result.Data) {
          // LoadEntities returns raw DB rows: {Id, Name, Config, ...} where Config
          // is the serialized JSON string of the actual payload ({id, name, columns,
          // data, timestamp}). Unwrap it so consumers get a flat, usable shape.
          this.excels = result.Data.map((row: any) => {
            const configStr = row.config ?? row.Config;
            let cfg: any = {};
            if (configStr) {
              try { cfg = typeof configStr === 'string' ? JSON.parse(configStr) : configStr; } catch { cfg = {}; }
            }
            return {
              id: row.id || row.Id || cfg.id,
              name: row.name || row.Name || cfg.name || 'Untitled Spreadsheet',
              columns: cfg.columns || [],
              data: cfg.data || [],
              timestamp: cfg.timestamp
            };
          });
        }
      } catch (error) {
        console.error('Error loading spreadsheets:', error);
        this.excels = [];
      }
    },
    async saveExcel(excel: any, socket: any) {
      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('SaveExcel', { excel }, socket);
        return result;
      } catch (error) {
        console.error('Error saving spreadsheet:', error);
        throw error;
      }
    },
    async deleteExcel(id: string, socket: any) {
      try {
        const userStore = userStoreMe();
        await userStore.executeCommand('DeleteExcel', { id }, socket);
        return true;
      } catch (error) {
        console.error('Error deleting spreadsheet:', error);
        throw error;
      }
    }
  }
});
