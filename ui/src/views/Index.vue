<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useDashboardStore } from '@/store/dashboardStore';
import { useProjectStore } from '@/store/projectStore';
import { userStoreMe } from '@/store/userStore';
import { getCurrentInstance } from 'vue';
import {
    LayoutDashboard, FileCode2, Database, FileText, BarChart2, Clock, Plus, ArrowRight,
    Table, FileSpreadsheet, Code, BarChart, FolderOpen, AlignLeft, MousePointerClick, X
} from 'lucide-vue-next';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

const router = useRouter();
const dashboardStore = useDashboardStore();
const projectStore = useProjectStore();
const userStore = userStoreMe();
const { proxy } = getCurrentInstance();

const recentDashboards = ref([]);
const loading = ref(true);

// New project dialog (local to home)
const showNewProject = ref(false);
const newProjectName = ref('');
const newProjectDesc = ref('');
const savingProject = ref(false);

const greeting = computed(() => {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 18) return 'Good afternoon';
    return 'Good evening';
});

const projectSubItems = [
    { icon: Database, key: 'databases', label: 'Databases', path: '/pages/databases' },
    { icon: Table, key: 'datasets', label: 'Datasets', path: '/pages/dataset' },
    { icon: Code, key: 'sqleditor', label: 'SQL', path: '/pages/sqleditor' },
    { icon: FileCode2, key: 'cseditor', label: 'Scripts', path: '/pages/cseditor' },
    { icon: FileSpreadsheet, key: 'myexcel', label: 'Excel', path: '/pages/myexcel' },
    { icon: BarChart, key: 'dashboard', label: 'Dashboards', path: '/pages/dashboard' },
];

const navigateToProjectItem = (project, path) => {
    projectStore.setCurrentProject(project.id);
    router.push(path);
};

const selectProject = (project) => {
    projectStore.setCurrentProject(project.id);
    router.push('/pages/dashboard');
};

const openNewProject = () => {
    newProjectName.value = '';
    newProjectDesc.value = '';
    showNewProject.value = true;
};

const createProject = async () => {
    if (!newProjectName.value.trim()) return;
    savingProject.value = true;
    try {
        await projectStore.saveProject(
            { name: newProjectName.value.trim(), description: newProjectDesc.value.trim() },
            proxy.$socket
        );
        showNewProject.value = false;
    } catch (e) {
        console.error(e);
    } finally {
        savingProject.value = false;
    }
};

function tryParse(str) {
    try { return JSON.parse(str); } catch { return null; }
}

onMounted(async () => {
    try {
        await dashboardStore.loadDashboards(proxy.$socket);
        recentDashboards.value = dashboardStore.dashboards
            .map(raw => {
                const config = typeof raw.config === 'string' ? tryParse(raw.config) : raw.config;
                return {
                    id: raw.id || raw.Id,
                    title: config?.title || raw.name || raw.Name || 'Untitled',
                    widgetCount: config?.components?.length || 0,
                    timestamp: config?.timestamp || raw.createdat || raw.CreatedAt,
                };
            })
            .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
            .slice(0, 4);
    } catch (_) {}
    loading.value = false;
});

function formatDate(ts) {
    if (!ts) return '';
    const d = new Date(ts);
    const now = new Date();
    const diff = now - d;
    if (diff < 60000) return 'just now';
    if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`;
    if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`;
    return d.toLocaleDateString();
}
</script>

<template>
    <div class="home-page max-w-5xl mx-auto space-y-8">
        <!-- Welcome header -->
        <div class="pt-2">
            <h1 class="text-3xl font-bold">{{ greeting }}, {{ userStore.name || 'User' }}</h1>
            <p class="text-muted-foreground mt-1">Welcome to CRS Reporter — your analysis platform.</p>
        </div>

        <!-- Projects section -->
        <section>
            <div class="flex items-center justify-between mb-3">
                <h2 class="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Your Projects</h2>
                <Button variant="ghost" size="sm" class="gap-1 text-xs" @click="openNewProject">
                    <Plus class="w-3 h-3" /> New Project
                </Button>
            </div>

            <!-- Empty state: onboarding instructions -->
            <div v-if="projectStore.projects.length === 0" class="bg-card border rounded-xl p-8">
                <div class="text-center mb-6">
                    <FolderOpen class="w-12 h-12 mx-auto mb-3 opacity-20" />
                    <h3 class="font-semibold text-lg mb-1">No projects yet</h3>
                    <p class="text-sm text-muted-foreground max-w-sm mx-auto">
                        Projects keep your databases, datasets, scripts, and dashboards organized in one place.
                    </p>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-2xl mx-auto">
                    <div class="bg-muted/40 rounded-lg p-4 border flex flex-col gap-2">
                        <Plus class="w-5 h-5 text-primary" />
                        <p class="font-medium text-sm">Create a project</p>
                        <p class="text-xs text-muted-foreground leading-relaxed">
                            Click <strong>New Project</strong> above, or hover the left sidebar — you'll see a <strong>"+"</strong> next to the Projects header.
                        </p>
                    </div>
                    <div class="bg-muted/40 rounded-lg p-4 border flex flex-col gap-2">
                        <AlignLeft class="w-5 h-5 text-blue-500" />
                        <p class="font-medium text-sm">Navigate with the sidebar</p>
                        <p class="text-xs text-muted-foreground leading-relaxed">
                            Hover the left sidebar to expand it. Click a project to open its sub-menu: <strong>Databases, Datasets, SQL, Scripts, Excel,</strong> and <strong>Dashboards</strong>.
                        </p>
                    </div>
                    <div class="bg-muted/40 rounded-lg p-4 border flex flex-col gap-2">
                        <Database class="w-5 h-5 text-green-500" />
                        <p class="font-medium text-sm">Connect a database</p>
                        <p class="text-xs text-muted-foreground leading-relaxed">
                            After creating a project, go to <strong>Databases</strong> to add a PostgreSQL, MySQL, or SQL Server connection.
                        </p>
                    </div>
                    <div class="bg-muted/40 rounded-lg p-4 border flex flex-col gap-2">
                        <MousePointerClick class="w-5 h-5 text-violet-500" />
                        <p class="font-medium text-sm">Build dashboards</p>
                        <p class="text-xs text-muted-foreground leading-relaxed">
                            Use <strong>Datasets</strong> to query your data, then drag widgets onto a <strong>Dashboard</strong> to build interactive reports.
                        </p>
                    </div>
                </div>

                <div class="text-center mt-6">
                    <Button class="gap-2" @click="openNewProject">
                        <Plus class="w-4 h-4" /> Create your first project
                    </Button>
                </div>
            </div>

            <!-- Projects grid -->
            <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                <div
                    v-for="project in projectStore.projects"
                    :key="project.id"
                    class="bg-card border rounded-xl p-4 flex flex-col gap-3 cursor-pointer hover:shadow-md transition-all group"
                    :class="projectStore.currentProjectId === project.id
                        ? 'border-primary/40 bg-primary/5 hover:border-primary/60'
                        : 'hover:border-primary/30'"
                    @click="selectProject(project)"
                >
                    <div class="flex items-start justify-between gap-2">
                        <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                            <span class="text-sm font-bold text-primary">{{ project.name.charAt(0).toUpperCase() }}</span>
                        </div>
                        <span
                            v-if="projectStore.currentProjectId === project.id"
                            class="text-[10px] font-semibold uppercase tracking-wide bg-primary/10 text-primary rounded px-1.5 py-0.5 shrink-0"
                        >Active</span>
                    </div>
                    <div>
                        <div class="font-semibold text-sm">{{ project.name }}</div>
                        <div class="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                            {{ project.description || 'No description' }}
                        </div>
                    </div>
                    <!-- Sub-item quick links -->
                    <div class="flex gap-1 flex-wrap mt-auto pt-1 border-t border-border/50">
                        <button
                            v-for="item in projectSubItems"
                            :key="item.key"
                            class="flex items-center gap-1 px-2 py-0.5 rounded bg-muted/60 hover:bg-muted text-xs text-muted-foreground hover:text-foreground transition-colors"
                            :title="item.label"
                            @click.stop="navigateToProjectItem(project, item.path)"
                        >
                            <component :is="item.icon" class="w-3 h-3" />
                            {{ item.label }}
                        </button>
                    </div>
                </div>
            </div>
        </section>

        <!-- Recent dashboards -->
        <section>
            <div class="flex items-center justify-between mb-3">
                <h2 class="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Recent Dashboards</h2>
                <Button variant="ghost" size="sm" class="gap-1 text-xs" @click="router.push('/pages/reports')">
                    View all <ArrowRight class="w-3 h-3" />
                </Button>
            </div>

            <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div v-for="n in 4" :key="n" class="bg-card border rounded-xl p-4 h-28 animate-pulse" />
            </div>

            <div v-else-if="recentDashboards.length === 0" class="bg-card border rounded-xl p-8 text-center text-muted-foreground">
                <LayoutDashboard class="w-10 h-10 mx-auto mb-2 opacity-30" />
                <p class="text-sm">No dashboards yet.</p>
                <Button class="mt-3 gap-2" size="sm" @click="router.push('/pages/dashboard')">
                    <Plus class="w-4 h-4" /> Create your first dashboard
                </Button>
            </div>

            <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div
                    v-for="dash in recentDashboards"
                    :key="dash.id"
                    class="bg-card border rounded-xl p-4 flex flex-col gap-2 cursor-pointer hover:shadow-md transition-shadow"
                    @click="router.push({ path: '/pages/dashboard', query: { id: dash.id } })"
                >
                    <div class="bg-muted/40 rounded-lg h-20 flex items-center justify-center">
                        <BarChart2 class="w-8 h-8 text-muted-foreground/40" />
                    </div>
                    <div class="font-medium text-sm line-clamp-1">{{ dash.title }}</div>
                    <div class="flex items-center gap-1 text-xs text-muted-foreground">
                        <Clock class="w-3 h-3" />
                        {{ formatDate(dash.timestamp) }} · {{ dash.widgetCount }} widget{{ dash.widgetCount !== 1 ? 's' : '' }}
                    </div>
                </div>
            </div>
        </section>

        <!-- Stats row -->
        <section class="grid grid-cols-2 sm:grid-cols-4 gap-4 pb-4">
            <div class="bg-card border rounded-xl p-4 text-center">
                <div class="text-2xl font-bold text-violet-500">{{ projectStore.projects.length }}</div>
                <div class="text-xs text-muted-foreground mt-1">Projects</div>
            </div>
            <div class="bg-card border rounded-xl p-4 text-center">
                <div class="text-2xl font-bold text-violet-500">{{ dashboardStore.dashboards.length }}</div>
                <div class="text-xs text-muted-foreground mt-1">Dashboards</div>
            </div>
            <div class="bg-card border rounded-xl p-4 text-center">
                <div class="text-2xl font-bold text-blue-500">
                    {{ dashboardStore.dashboards.reduce((sum, d) => {
                        const c = typeof d.config === 'string' ? (JSON.parse(d.config || '{}') || {}) : (d.config || {});
                        return sum + (c.components?.length || 0);
                    }, 0) }}
                </div>
                <div class="text-xs text-muted-foreground mt-1">Total Widgets</div>
            </div>
            <div class="bg-card border rounded-xl p-4 text-center">
                <div class="text-2xl font-bold text-green-500">
                    {{ dashboardStore.dashboards.filter(d => {
                        const c = typeof d.config === 'string' ? (JSON.parse(d.config || '{}') || {}) : (d.config || {});
                        return (d.ispublic || d.IsPublic || c.isPublic);
                    }).length }}
                </div>
                <div class="text-xs text-muted-foreground mt-1">Public Links</div>
            </div>
        </section>
    </div>

    <!-- New Project Dialog -->
    <div v-if="showNewProject" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="showNewProject = false">
        <div class="bg-zinc-950 border border-zinc-800 rounded-2xl p-6 w-full max-w-sm shadow-xl text-zinc-100 mx-4">
            <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold">New Project</h3>
                <button @click="showNewProject = false" class="text-zinc-500 hover:text-zinc-300 transition-colors">
                    <X class="w-4 h-4" />
                </button>
            </div>
            <div class="flex flex-col gap-3">
                <div>
                    <Label class="text-zinc-400 text-xs mb-1 block">Name</Label>
                    <Input
                        v-model="newProjectName"
                        placeholder="Project name"
                        class="bg-zinc-900 border-zinc-800 text-zinc-100"
                        @keyup.enter="createProject"
                    />
                </div>
                <div>
                    <Label class="text-zinc-400 text-xs mb-1 block">Description</Label>
                    <Input
                        v-model="newProjectDesc"
                        placeholder="Optional description"
                        class="bg-zinc-900 border-zinc-800 text-zinc-100"
                    />
                </div>
                <div class="flex gap-2 mt-2">
                    <Button class="flex-1" :disabled="!newProjectName.trim() || savingProject" @click="createProject">
                        {{ savingProject ? 'Creating...' : 'Create' }}
                    </Button>
                    <Button variant="outline" class="flex-1" @click="showNewProject = false">Cancel</Button>
                </div>
            </div>
        </div>
    </div>
</template>
