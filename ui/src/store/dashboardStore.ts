import { defineStore } from "pinia";
import { userStoreMe } from "@/store/userStore";

export const useDashboardStore = defineStore({
  id: "dashboard",
  state: () => ({
    dashboards: [] as any[],
    loading: false
  }),
  actions: {
    async loadDashboards(socket: any) {
      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('LoadDashboards', {}, socket);
        if (result && result.Data) {
          this.dashboards = result.Data;
        }
      } catch (error) {
        console.error('Error loading dashboards:', error);
        this.dashboards = [];
      }
    },
    async saveDashboard(dashboard: any, socket: any) {
      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('SaveDashboard', { dashboard }, socket);
        return result;
      } catch (error) {
        console.error('Error saving dashboard:', error);
        throw error;
      }
    },
    async deleteDashboard(id: string, socket: any) {
      try {
        const userStore = userStoreMe();
        await userStore.executeCommand('DeleteDashboard', { id }, socket);
        return true;
      } catch (error) {
        console.error('Error deleting dashboard:', error);
        throw error;
      }
    }
  }
});
