<script setup>
import { ref, onMounted, computed, getCurrentInstance } from 'vue';
import { useRouter } from 'vue-router';
import { toast } from 'vue-sonner';
import { BarChart2, LayoutDashboard, Search, Plus, Trash2, ExternalLink, Share2, Copy, Globe, Lock, RefreshCw, FileText, Play, ChevronDown } from 'lucide-vue-next';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useDashboardStore } from '@/store/dashboardStore';
import { userStoreMe } from '@/store/userStore';

const router = useRouter();
const dashboardStore = useDashboardStore();
const userStore = userStoreMe();
const { proxy } = getCurrentInstance();

const searchQuery = ref('');
const loading = ref(false);
const dashboards = ref([]);
const userReports = ref([]);
const reportsLoading = ref(false);
const activeTab = ref('dashboards');

const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:9876';

// Share dialog
const showShareDialog = ref(false);
const sharingItem = ref(null);
const shareToken = ref('');
const shareUrl = computed(() =>
    shareToken.value ? `${window.location.origin}/public/${shareToken.value}` : ''
);

// Report result viewer
const showResultDialog = ref(false);
const reportResult = ref(null);
const runningReportId = ref(null);

const filteredDashboards = computed(() => {
    const q = searchQuery.value.toLowerCase();
    return dashboards.value.filter(d => (d.title || d.name || '').toLowerCase().includes(q));
});

const filteredReports = computed(() => {
    const q = searchQuery.value.toLowerCase();
    return userReports.value.filter(r => (r.name || '').toLowerCase().includes(q));
});

async function loadDashboards() {
    loading.value = true;
    try {
        await dashboardStore.loadDashboards(proxy.$socket);
        dashboards.value = dashboardStore.dashboards.map(raw => {
            const config = typeof raw.config === 'string' ? tryParse(raw.config) : raw.config;
            return {
                id: raw.id || raw.Id,
                title: config?.title || raw.name || raw.Name || 'Untitled',
                components: config?.components || [],
                shareToken: raw.sharetoken || raw.ShareToken || config?.shareToken || null,
                isPublic: !!(raw.ispublic || raw.IsPublic || config?.isPublic),
                timestamp: config?.timestamp || raw.createdat || raw.CreatedAt
            };
        });
    } catch (e) {
        toast.error('Failed to load dashboards');
    } finally {
        loading.value = false;
    }
}

async function loadUserReports() {
    reportsLoading.value = true;
    try {
        const token = localStorage.getItem('crs_token');
        const resp = await fetch(`${apiUrl}/api/reports/type/my`, {
            headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        if (!resp.ok) throw new Error('Failed to load reports');
        userReports.value = await resp.json();
    } catch (e) {
        toast.error('Failed to load reports');
    } finally {
        reportsLoading.value = false;
    }
}

function tryParse(str) {
    try { return JSON.parse(str); } catch { return null; }
}

function openDashboard(dash) {
    router.push({ path: '/pages/dashboard', query: { id: dash.id } });
}

async function deleteDashboard(dash) {
    try {
        await dashboardStore.deleteDashboard(dash.id, proxy.$socket);
        dashboards.value = dashboards.value.filter(d => d.id !== dash.id);
        toast.success('Dashboard deleted');
    } catch {
        toast.error('Failed to delete dashboard');
    }
}

async function deleteReport(report) {
    try {
        const token = localStorage.getItem('crs_token');
        const resp = await fetch(`${apiUrl}/api/reports/${report.reportId || report.ReportId}`, {
            method: 'DELETE',
            headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        if (!resp.ok) throw new Error('Delete failed');
        userReports.value = userReports.value.filter(r => (r.reportId || r.ReportId) !== (report.reportId || report.ReportId));
        toast.success('Report deleted');
    } catch {
        toast.error('Failed to delete report');
    }
}

async function executeReport(report) {
    runningReportId.value = report.reportId || report.ReportId;
    try {
        const token = localStorage.getItem('crs_token');
        const resp = await fetch(`${apiUrl}/api/reports/${runningReportId.value}/execute`, {
            method: 'POST',
            headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        if (!resp.ok) throw new Error('Execution failed');
        const result = await resp.json();
        reportResult.value = result;
        showResultDialog.value = true;
    } catch (e) {
        toast.error('Failed to execute report', { description: e.message });
    } finally {
        runningReportId.value = null;
    }
}

function openShareDialog(dash) {
    sharingItem.value = dash;
    shareToken.value = dash.shareToken || '';
    showShareDialog.value = true;
}

async function enableSharing() {
    if (!sharingItem.value) return;
    try {
        const result = await userStore.executeCommand(
            'ShareDashboard', { id: sharingItem.value.id, enable: true }, proxy.$socket
        );
        const token = result?.Data?.shareToken;
        if (token) {
            shareToken.value = token;
            sharingItem.value.shareToken = token;
            sharingItem.value.isPublic = true;
            const idx = dashboards.value.findIndex(d => d.id === sharingItem.value.id);
            if (idx >= 0) dashboards.value[idx] = { ...dashboards.value[idx], shareToken: token, isPublic: true };
        }
        toast.success('Dashboard is now public');
    } catch {
        toast.error('Failed to enable sharing');
    }
}

async function disableSharing() {
    if (!sharingItem.value) return;
    try {
        await userStore.executeCommand(
            'ShareDashboard', { id: sharingItem.value.id, enable: false }, proxy.$socket
        );
        shareToken.value = '';
        sharingItem.value.shareToken = null;
        sharingItem.value.isPublic = false;
        const idx = dashboards.value.findIndex(d => d.id === sharingItem.value.id);
        if (idx >= 0) dashboards.value[idx] = { ...dashboards.value[idx], shareToken: null, isPublic: false };
        toast.success('Sharing disabled');
    } catch {
        toast.error('Failed to disable sharing');
    }
}

function copyShareUrl() {
    navigator.clipboard.writeText(shareUrl.value);
    toast.success('Link copied to clipboard');
}

function formatDate(ts) {
    if (!ts) return '—';
    return new Date(ts).toLocaleDateString();
}

async function handleTabChange(tab) {
    activeTab.value = tab;
    if (tab === 'reports' && userReports.value.length === 0) {
        await loadUserReports();
    }
}

onMounted(loadDashboards);
</script>

<template>
    <div class="reports-page p-2">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
            <div>
                <h1 class="text-2xl font-bold">Reports & Dashboards</h1>
                <p class="text-muted-foreground text-sm mt-1">Manage and share your analytics</p>
            </div>
            <div class="flex items-center gap-2">
                <Button variant="outline" size="icon" @click="activeTab === 'dashboards' ? loadDashboards() : loadUserReports()" :disabled="loading || reportsLoading" title="Refresh">
                    <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading || reportsLoading }" />
                </Button>
                <Button @click="router.push('/pages/dashboard')" class="gap-2">
                    <Plus class="w-4 h-4" />
                    New Dashboard
                </Button>
            </div>
        </div>

        <!-- Search -->
        <div class="relative mb-4 max-w-sm">
            <Search class="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input v-model="searchQuery" placeholder="Search..." class="pl-9" />
        </div>

        <!-- Tabs -->
        <Tabs :model-value="activeTab" @update:model-value="handleTabChange">
            <TabsList class="mb-4">
                <TabsTrigger value="dashboards">
                    <LayoutDashboard class="w-4 h-4 mr-1.5" />
                    Dashboards
                    <Badge v-if="dashboards.length" variant="secondary" class="ml-1.5 text-xs">{{ dashboards.length }}</Badge>
                </TabsTrigger>
                <TabsTrigger value="reports">
                    <FileText class="w-4 h-4 mr-1.5" />
                    SQL Reports
                    <Badge v-if="userReports.length" variant="secondary" class="ml-1.5 text-xs">{{ userReports.length }}</Badge>
                </TabsTrigger>
            </TabsList>

            <!-- Dashboards Tab -->
            <TabsContent value="dashboards">
                <div v-if="!loading && filteredDashboards.length === 0" class="text-center py-16 text-muted-foreground">
                    <LayoutDashboard class="w-12 h-12 mx-auto mb-3 opacity-30" />
                    <p class="text-lg font-medium">No dashboards yet</p>
                    <p class="text-sm mt-1">Create your first dashboard to get started</p>
                    <Button class="mt-4 gap-2" @click="router.push('/pages/dashboard')">
                        <Plus class="w-4 h-4" /> Create Dashboard
                    </Button>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                    <div
                        v-for="dash in filteredDashboards"
                        :key="dash.id"
                        class="bg-card border rounded-xl p-4 flex flex-col gap-3 hover:shadow-md transition-shadow cursor-pointer group"
                        @click="openDashboard(dash)"
                    >
                        <div class="bg-muted/50 rounded-lg h-32 flex items-center justify-center">
                            <LayoutDashboard class="w-10 h-10 text-muted-foreground/40" />
                        </div>

                        <div class="flex-1">
                            <div class="flex items-start justify-between gap-2">
                                <h3 class="font-semibold text-sm leading-tight line-clamp-2">{{ dash.title }}</h3>
                                <Badge v-if="dash.isPublic" variant="secondary" class="shrink-0 gap-1 text-xs">
                                    <Globe class="w-3 h-3" />Public
                                </Badge>
                            </div>
                            <p class="text-xs text-muted-foreground mt-1">
                                {{ dash.components?.length || 0 }} widget{{ (dash.components?.length || 0) !== 1 ? 's' : '' }} · {{ formatDate(dash.timestamp) }}
                            </p>
                        </div>

                        <div class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity" @click.stop>
                            <Button variant="ghost" size="icon" class="h-8 w-8" title="Open" @click="openDashboard(dash)">
                                <ExternalLink class="w-3.5 h-3.5" />
                            </Button>
                            <Button variant="ghost" size="icon" class="h-8 w-8" title="Share" @click="openShareDialog(dash)">
                                <Share2 class="w-3.5 h-3.5" />
                            </Button>
                            <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive hover:text-destructive" title="Delete" @click="deleteDashboard(dash)">
                                <Trash2 class="w-3.5 h-3.5" />
                            </Button>
                        </div>
                    </div>
                </div>
            </TabsContent>

            <!-- SQL Reports Tab -->
            <TabsContent value="reports">
                <div v-if="reportsLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <div v-for="n in 3" :key="n" class="bg-card border rounded-xl p-4 h-36 animate-pulse" />
                </div>

                <div v-else-if="filteredReports.length === 0" class="text-center py-16 text-muted-foreground">
                    <FileText class="w-12 h-12 mx-auto mb-3 opacity-30" />
                    <p class="text-lg font-medium">No saved reports</p>
                    <p class="text-sm mt-1">Run a script in the C# Editor and save it as a report</p>
                    <Button class="mt-4 gap-2" @click="router.push('/pages/cseditor')">
                        <Plus class="w-4 h-4" /> Open C# Editor
                    </Button>
                </div>

                <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    <div
                        v-for="report in filteredReports"
                        :key="report.reportId || report.ReportId"
                        class="bg-card border rounded-xl p-4 flex flex-col gap-3 group"
                    >
                        <div class="flex items-start justify-between gap-2">
                            <div>
                                <h3 class="font-semibold text-sm">{{ report.name || report.Name || 'Untitled Report' }}</h3>
                                <p class="text-xs text-muted-foreground mt-0.5">
                                    {{ report.description || 'SQL / FunctEngine report' }}
                                </p>
                            </div>
                            <Badge variant="outline" class="text-xs shrink-0">Report</Badge>
                        </div>

                        <p class="text-xs text-muted-foreground">
                            Created {{ formatDate(report.createdAt || report.CreatedAt) }}
                        </p>

                        <div class="flex gap-2 mt-auto">
                            <Button size="sm" class="gap-1 flex-1" @click="executeReport(report)" :disabled="runningReportId === (report.reportId || report.ReportId)">
                                <Play class="w-3.5 h-3.5" :class="runningReportId === (report.reportId || report.ReportId) ? 'animate-spin' : ''" />
                                Run
                            </Button>
                            <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive hover:text-destructive" title="Delete" @click="deleteReport(report)">
                                <Trash2 class="w-3.5 h-3.5" />
                            </Button>
                        </div>
                    </div>
                </div>
            </TabsContent>
        </Tabs>

        <!-- Share Dialog -->
        <Dialog v-model:open="showShareDialog">
            <DialogContent class="max-w-md">
                <DialogHeader>
                    <DialogTitle class="flex items-center gap-2">
                        <Share2 class="w-4 h-4" />
                        Share "{{ sharingItem?.title }}"
                    </DialogTitle>
                    <DialogDescription>
                        Generate a public link anyone can use to view this dashboard without logging in.
                    </DialogDescription>
                </DialogHeader>

                <div class="py-4 space-y-4">
                    <div v-if="shareToken" class="space-y-2">
                        <Label>Public Link</Label>
                        <div class="flex gap-2">
                            <Input :model-value="shareUrl" readonly class="font-mono text-xs" />
                            <Button variant="outline" size="icon" @click="copyShareUrl" title="Copy link">
                                <Copy class="w-4 h-4" />
                            </Button>
                        </div>
                        <p class="text-xs text-muted-foreground">Anyone with this link can view the dashboard.</p>
                    </div>

                    <div v-if="!shareToken" class="text-center py-4">
                        <Lock class="w-8 h-8 mx-auto mb-2 text-muted-foreground" />
                        <p class="text-sm text-muted-foreground">This dashboard is currently private.</p>
                    </div>
                </div>

                <DialogFooter>
                    <Button v-if="!shareToken" @click="enableSharing" class="gap-2 w-full">
                        <Globe class="w-4 h-4" />
                        Generate Public Link
                    </Button>
                    <div v-else class="flex gap-2 w-full">
                        <Button variant="destructive" @click="disableSharing" class="gap-2 flex-1">
                            <Lock class="w-4 h-4" />
                            Revoke Access
                        </Button>
                        <Button variant="outline" @click="showShareDialog = false" class="flex-1">Done</Button>
                    </div>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <!-- Report Result Dialog -->
        <Dialog v-model:open="showResultDialog">
            <DialogContent class="max-w-2xl max-h-[80vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle class="flex items-center gap-2">
                        <BarChart2 class="w-4 h-4" />
                        Report Results
                    </DialogTitle>
                </DialogHeader>
                <div v-if="reportResult" class="py-2 space-y-3">
                    <div class="text-xs text-muted-foreground">
                        Executed at {{ new Date(reportResult.executedAt || Date.now()).toLocaleString() }}
                    </div>
                    <pre class="bg-muted rounded-md p-3 text-xs overflow-x-auto whitespace-pre-wrap">{{ JSON.stringify(reportResult.data, null, 2) }}</pre>
                </div>
                <DialogFooter>
                    <Button variant="outline" @click="showResultDialog = false">Close</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
