import { defineStore } from "pinia";
import { userStoreMe } from "@/store/userStore";
import { useProjectStore } from "@/store/projectStore";

export const useDatasetStore = defineStore({
  id: "dataset",
  state: () => ({
    datasets: [] as any[],
    loading: false
  }),
  actions: {
    async loadDatasets(socket: any) {
      try {
        const userStore = userStoreMe();
        const projectStore = useProjectStore();
        const params = projectStore.currentProjectId ? { projectId: projectStore.currentProjectId } : {};
        const result = await userStore.executeCommand('LoadDatasets', params, socket);
        if (result && result.Data) {
          this.datasets = result.Data;
        }
      } catch (error) {
        console.error('Error loading datasets:', error);
        this.datasets = [];
      }
    },
    async saveDataset(dataset: any, socket: any) {
      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('SaveDataset', { dataset }, socket);
        return result;
      } catch (error) {
        console.error('Error saving dataset:', error);
        throw error;
      }
    },
    async deleteDataset(id: string, socket: any) {
      try {
        const userStore = userStoreMe();
        await userStore.executeCommand('DeleteDataset', { id }, socket);
        return true;
      } catch (error) {
        console.error('Error deleting dataset:', error);
        throw error;
      }
    }
  }
});
