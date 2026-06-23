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
          this.excels = result.Data;
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
