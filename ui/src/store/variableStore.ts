import { defineStore } from 'pinia';
import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';

function storageKey(projectId: string | null) {
    return `crs_vars_${projectId || 'global'}`;
}

export const useVariableStore = defineStore('variables', {
    state: () => ({
        definitions: [] as any[],
        values: {} as Record<string, string>
    }),
    getters: {
        getValue: (state) => (name: string): string => {
            if (state.values[name] !== undefined) return state.values[name];
            const def = state.definitions.find((d: any) => d.name === name);
            return def?.defaultValue ?? '';
        },
        detectInSql: () => (sql: string): string[] => {
            const matches = [...sql.matchAll(/\{\{(\w+)\}\}/g)];
            return [...new Set(matches.map(m => m[1]))];
        }
    },
    actions: {
        setValue(name: string, value: string) {
            this.values[name] = value;
            const projectStore = useProjectStore();
            try {
                const key = storageKey(projectStore.currentProjectId);
                const stored = JSON.parse(localStorage.getItem(key) || '{}');
                stored[name] = value;
                localStorage.setItem(key, JSON.stringify(stored));
            } catch {}
        },
        loadFromStorage() {
            const projectStore = useProjectStore();
            try {
                const key = storageKey(projectStore.currentProjectId);
                const stored = JSON.parse(localStorage.getItem(key) || '{}');
                this.values = { ...this.values, ...stored };
            } catch {}
        },
        async loadDefinitions(socket: any) {
            const userStore = userStoreMe();
            const projectStore = useProjectStore();
            try {
                const params: any = {};
                if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
                const result = await userStore.executeCommand('LoadVariables', params, socket);
                this.definitions = (result?.Data || []).map((d: any) => ({
                    id: d.id || d.Id,
                    name: d.name || d.Name || '',
                    label: d.label || d.Label || '',
                    type: d.type || d.Type || 'input',
                    defaultValue: d.defaultvalue ?? d.defaultValue ?? d.DefaultValue ?? '',
                    dropdownSource: d.dropdownsource ?? d.dropdownSource ?? d.DropdownSource ?? 'static',
                    dropdownValues: d.dropdownvalues ?? d.dropdownValues ?? d.DropdownValues ?? '',
                    dropdownQuery: d.dropdownquery ?? d.dropdownQuery ?? d.DropdownQuery ?? '',
                    dropdownConnectionId: d.dropdownconnectionid ?? d.dropdownConnectionId ?? d.DropdownConnectionId ?? '',
                    projectId: d.projectid ?? d.projectId ?? d.ProjectId ?? null
                }));
                this.loadFromStorage();
                for (const def of this.definitions) {
                    // Use falsy check so empty-string values (e.g. from a prior bug) also get restored
                    if (!this.values[def.name] && def.defaultValue) {
                        this.values[def.name] = def.defaultValue;
                    }
                }
            } catch {
                this.definitions = [];
            }
        },
        async saveDefinition(varDef: any, socket: any) {
            const userStore = userStoreMe();
            const projectStore = useProjectStore();
            const obj = { ...varDef, projectId: projectStore.currentProjectId || undefined };
            await userStore.executeCommand('SaveVariable', { variable: obj }, socket);
            await this.loadDefinitions(socket);
        },
        async deleteDefinition(id: string, socket: any) {
            const userStore = userStoreMe();
            await userStore.executeCommand('DeleteVariable', { id }, socket);
            this.definitions = this.definitions.filter((d: any) => d.id !== id);
        },
        substituteInSql(sql: string, _dbType: string): string {
            return sql.replace(/\{\{(\w+)\}\}/g, (_, name) => {
                const val = this.getValue(name);
                const def = this.definitions.find((d: any) => d.name === name);
                const type = def?.type || 'input';
                if (type === 'input' && val !== '' && !isNaN(Number(val))) {
                    return val;
                }
                const escaped = val.replace(/'/g, "''");
                return `'${escaped}'`;
            });
        },
        getValuesDict(): Record<string, string> {
            const result: Record<string, string> = {};
            for (const def of this.definitions) {
                result[def.name] = this.getValue(def.name);
            }
            return result;
        },
        async resolveDropdownOptions(def: any, socket: any): Promise<string[]> {
            if (!def || def.dropdownSource !== 'sql') {
                return (def?.dropdownValues || '').split(',').map((s: string) => s.trim()).filter(Boolean);
            }
            try {
                const userStore = userStoreMe();
                const result = await userStore.executeCommand('ResolveDropdownQuery', {
                    database: def.dropdownConnectionId,
                    query: def.dropdownQuery
                }, socket);
                return result?.Data || [];
            } catch {
                return [];
            }
        }
    }
});
