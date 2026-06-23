import { defineStore } from 'pinia';
import { userStoreMe } from '@/store/userStore';

export interface Project {
  id: string;
  name: string;
  description: string;
  createdAt: Date;
  updatedAt: Date;
}

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [] as Project[],
    currentProjectId: null as string | null,
    expandedProjects: new Set<string>() as Set<string>,
  }),
  getters: {
    currentProject: (state): Project | null =>
      state.projects.find(p => p.id === state.currentProjectId) ?? null,
    isExpanded: (state) => (id: string) => state.expandedProjects.has(id),
  },
  actions: {
    async loadProjects(socket: any) {
      try {
        const userStore = userStoreMe();
        const result = await userStore.executeCommand('LoadProjects', {}, socket);
        if (result?.Data) {
          this.projects = result.Data.map((p: any) => ({
            id: p.id || p.Id,
            name: p.name || p.Name || '',
            description: p.description || p.Description || '',
            createdAt: new Date(p.createdAt || p.CreatedAt || p.createdat || Date.now()),
            updatedAt: new Date(p.updatedAt || p.UpdatedAt || p.updatedat || Date.now()),
          }));
        }
      } catch (error) {
        console.error('Error loading projects:', error);
      }
    },
    async saveProject(project: Partial<Project> & { id?: string }, socket: any) {
      const userStore = userStoreMe();
      const payload = { ...project, id: project.id || crypto.randomUUID() };
      await userStore.executeCommand('SaveProject', { project: payload }, socket);
      await this.loadProjects(socket);
      return payload;
    },
    async deleteProject(id: string, socket: any) {
      const userStore = userStoreMe();
      await userStore.executeCommand('DeleteProject', { id }, socket);
      if (this.currentProjectId === id) this.currentProjectId = null;
      this.expandedProjects.delete(id);
      this.projects = this.projects.filter(p => p.id !== id);
    },
    setCurrentProject(id: string | null) {
      this.currentProjectId = id;
    },
    toggleExpanded(id: string) {
      if (this.expandedProjects.has(id)) {
        this.expandedProjects.delete(id);
      } else {
        this.expandedProjects.add(id);
      }
    },
  },
});
