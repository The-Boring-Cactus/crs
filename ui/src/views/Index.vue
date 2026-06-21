<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useDashboardStore } from '@/store/dashboardStore';
import { userStoreMe } from '@/store/userStore';
import { getCurrentInstance } from 'vue';
import { LayoutDashboard, FileCode2, Database, FileText, BarChart2, Clock, Plus, ArrowRight } from 'lucide-vue-next';
import { Button } from '@/components/ui/button';

const router = useRouter();
const dashboardStore = useDashboardStore();
const userStore = userStoreMe();
const { proxy } = getCurrentInstance();

const recentDashboards = ref([]);
const loading = ref(true);

const greeting = computed(() => {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 18) return 'Good afternoon';
    return 'Good evening';
});

const quickActions = [
    { icon: LayoutDashboard, label: 'New Dashboard', desc: 'Build an interactive dashboard with charts and tables', path: '/pages/dashboard', color: 'text-violet-500' },
    { icon: FileCode2, label: 'C# Script Editor', desc: 'Write scripts to query data and generate reports', path: '/pages/cseditor', color: 'text-blue-500' },
    { icon: Database, label: 'Manage Databases', desc: 'Connect to PostgreSQL, MySQL, or SQL Server', path: '/pages/databases', color: 'text-green-500' },
    { icon: FileText, label: 'Reports Gallery', desc: 'View and share your saved dashboards and reports', path: '/pages/reports', color: 'text-orange-500' },
];

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
            <p class="text-muted-foreground mt-1">Welcome to CRS Reporter — your PowerBI-style analysis platform.</p>
        </div>

        <!-- Quick actions -->
        <section>
            <h2 class="text-sm font-semibold uppercase tracking-wider text-muted-foreground mb-3">Quick Actions</h2>
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div
                    v-for="action in quickActions"
                    :key="action.path"
                    class="bg-card border rounded-xl p-4 flex flex-col gap-3 cursor-pointer hover:shadow-md hover:border-primary/30 transition-all group"
                    @click="router.push(action.path)"
                >
                    <component :is="action.icon" class="w-7 h-7" :class="action.color" />
                    <div>
                        <div class="font-semibold text-sm">{{ action.label }}</div>
                        <div class="text-xs text-muted-foreground mt-0.5 line-clamp-2">{{ action.desc }}</div>
                    </div>
                    <ArrowRight class="w-4 h-4 text-muted-foreground group-hover:text-foreground ml-auto transition-colors" />
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
            <div class="bg-card border rounded-xl p-4 text-center">
                <div class="text-2xl font-bold text-orange-500">{{ userStore.name ? 1 : 0 }}</div>
                <div class="text-xs text-muted-foreground mt-1">Active Users</div>
            </div>
        </section>
    </div>
</template>
