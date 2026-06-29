<script setup>
import { ref, onMounted, computed, getCurrentInstance } from 'vue';
import { useRouter } from 'vue-router';
import { toast } from 'vue-sonner';
import {
    BarChart2, LayoutDashboard, Search, Plus, Trash2, ExternalLink, Share2,
    Copy, Globe, Lock, RefreshCw, FileText, FolderOpen, ChevronDown, ChevronRight
} from 'lucide-vue-next';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';

const router = useRouter();
const userStore = userStoreMe();
const projectStore = useProjectStore();
const { proxy } = getCurrentInstance();

const searchQuery = ref('');
const loading = ref(false);
const dashboards = ref([]);
const reports = ref([]);
const collapsedGroups = ref(new Set());

// Share dialog
const showShareDialog = ref(false);
const sharingItem = ref(null);
const shareToken = ref('');
const shareUrl = computed(() =>
    shareToken.value ? `${window.location.origin}/share/${shareToken.value}` : ''
);

function tryParse(str) {
    try { return JSON.parse(str); } catch { return null; }
}

async function loadAll() {
    loading.value = true;
    try {
        await projectStore.loadProjects(proxy.$socket);

        const [dashResult, repResult] = await Promise.all([
            userStore.executeCommand('LoadDashboards', {}, proxy.$socket),
            userStore.executeCommand('LoadReports', {}, proxy.$socket),
        ]);

        dashboards.value = (dashResult?.Data || []).map(raw => {
            const config = typeof raw.config === 'string' ? tryParse(raw.config) : (raw.config || {});
            return {
                id: raw.id || raw.Id,
                name: config?.title || raw.name || raw.Name || 'Untitled',
                projectId: raw.projectid || raw.ProjectId || config?.projectId || null,
                components: config?.components || [],
                shareToken: raw.sharetoken || raw.ShareToken || config?.shareToken || null,
                isPublic: !!(raw.ispublic || raw.IsPublic || config?.isPublic),
                timestamp: config?.timestamp || raw.createdat || raw.CreatedAt,
                kind: 'dashboard',
            };
        });

        reports.value = (repResult?.Data || []).map(raw => {
            const config = typeof raw.config === 'string' ? tryParse(raw.config) : (raw.config || {});
            return {
                id: raw.id || raw.Id,
                name: raw.name || raw.Name || config?.name || 'Untitled Report',
                projectId: raw.projectid || raw.ProjectId || config?.projectId || null,
                shareToken: raw.sharetoken || raw.ShareToken || null,
                isPublic: !!(raw.ispublic || raw.IsPublic),
                timestamp: raw.createdat || raw.CreatedAt,
                kind: 'report',
            };
        });
    } catch (e) {
        toast.error('Failed to load data', { description: e.message });
    } finally {
        loading.value = false;
    }
}

const groupedItems = computed(() => {
    const q = searchQuery.value.toLowerCase();

    const fd = dashboards.value.filter(d => !q || d.name.toLowerCase().includes(q));
    const fr = reports.value.filter(r => !q || r.name.toLowerCase().includes(q));

    const groups = new Map();
    groups.set(null, { project: null, dashboards: [], reports: [] });
    projectStore.projects.forEach(p => groups.set(p.id, { project: p, dashboards: [], reports: [] }));

    fd.forEach(d => {
        const key = d.projectId && groups.has(d.projectId) ? d.projectId : null;
        groups.get(key).dashboards.push(d);
    });
    fr.forEach(r => {
        const key = r.projectId && groups.has(r.projectId) ? r.projectId : null;
        groups.get(key).reports.push(r);
    });

    return [...groups.values()].filter(g => g.dashboards.length > 0 || g.reports.length > 0);
});

function toggleGroup(key) {
    const k = key ?? '__none__';
    if (collapsedGroups.value.has(k)) collapsedGroups.value.delete(k);
    else collapsedGroups.value.add(k);
}
function isCollapsed(key) { return collapsedGroups.value.has(key ?? '__none__'); }

function openDashboard(dash) {
    router.push({ path: '/pages/dashboard', query: { id: dash.id } });
}

async function deleteDashboard(dash) {
    try {
        await userStore.executeCommand('DeleteDashboard', { id: dash.id }, proxy.$socket);
        dashboards.value = dashboards.value.filter(d => d.id !== dash.id);
        toast.success('Dashboard deleted');
    } catch {
        toast.error('Failed to delete dashboard');
    }
}

async function deleteReport(report) {
    try {
        await userStore.executeCommand('DeleteReport', { id: report.id }, proxy.$socket);
        reports.value = reports.value.filter(r => r.id !== report.id);
        toast.success('Report deleted');
    } catch {
        toast.error('Failed to delete report');
    }
}

function openShareDialog(item) {
    sharingItem.value = item;
    shareToken.value = item.shareToken || '';
    showShareDialog.value = true;
}

async function enableSharing() {
    if (!sharingItem.value) return;
    const cmd = sharingItem.value.kind === 'dashboard' ? 'ShareDashboard' : 'ShareReport';
    try {
        const result = await userStore.executeCommand(cmd, { id: sharingItem.value.id, enable: true }, proxy.$socket);
        const token = result?.Data?.shareToken;
        if (token) {
            shareToken.value = token;
            sharingItem.value.shareToken = token;
            sharingItem.value.isPublic = true;
        }
        toast.success('Link generated');
    } catch {
        toast.error('Failed to enable sharing');
    }
}

async function disableSharing() {
    if (!sharingItem.value) return;
    const cmd = sharingItem.value.kind === 'dashboard' ? 'ShareDashboard' : 'ShareReport';
    try {
        await userStore.executeCommand(cmd, { id: sharingItem.value.id, enable: false }, proxy.$socket);
        shareToken.value = '';
        sharingItem.value.shareToken = null;
        sharingItem.value.isPublic = false;
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

onMounted(loadAll);
</script>

<template>
    <div class="p-4 space-y-4">
        <!-- Header -->
        <div class="flex items-center justify-between">
            <div>
                <h1 class="text-2xl font-bold">Reports & Dashboards</h1>
                <p class="text-muted-foreground text-sm mt-0.5">Organized by project</p>
            </div>
            <div class="flex items-center gap-2">
                <Button variant="outline" size="icon" @click="loadAll" :disabled="loading" title="Refresh">
                    <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
                </Button>
                <Button @click="router.push('/pages/dashboard')" class="gap-2">
                    <Plus class="w-4 h-4" /> New Dashboard
                </Button>
            </div>
        </div>

        <!-- Search -->
        <div class="relative max-w-sm">
            <Search class="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input v-model="searchQuery" placeholder="Search dashboards & reports..." class="pl-9" />
        </div>

        <!-- Loading skeleton -->
        <div v-if="loading" class="space-y-4">
            <div v-for="n in 2" :key="n" class="space-y-2">
                <div class="h-8 w-48 bg-muted rounded animate-pulse" />
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                    <div v-for="m in 3" :key="m" class="bg-card border rounded-xl h-40 animate-pulse" />
                </div>
            </div>
        </div>

        <!-- Empty state -->
        <div v-else-if="groupedItems.length === 0" class="text-center py-20 text-muted-foreground">
            <BarChart2 class="w-12 h-12 mx-auto mb-3 opacity-30" />
            <p class="text-lg font-medium">Nothing here yet</p>
            <p class="text-sm mt-1">Create a dashboard to get started</p>
            <Button class="mt-4 gap-2" @click="router.push('/pages/dashboard')">
                <Plus class="w-4 h-4" /> Create Dashboard
            </Button>
        </div>

        <!-- Project Groups -->
        <div v-else class="space-y-6">
            <div v-for="group in groupedItems" :key="group.project?.id ?? '__none__'" class="space-y-3">
                <!-- Group Header -->
                <button
                    class="flex items-center gap-2 w-full text-left group"
                    @click="toggleGroup(group.project?.id)"
                >
                    <div class="flex items-center gap-2 flex-1">
                        <FolderOpen class="w-4 h-4 text-primary shrink-0" />
                        <span class="font-semibold text-sm">
                            {{ group.project?.name ?? 'Global / No Project' }}
                        </span>
                        <Badge variant="secondary" class="text-xs">
                            {{ group.dashboards.length + group.reports.length }}
                        </Badge>
                    </div>
                    <ChevronDown v-if="!isCollapsed(group.project?.id)" class="w-4 h-4 text-muted-foreground" />
                    <ChevronRight v-else class="w-4 h-4 text-muted-foreground" />
                </button>

                <div v-show="!isCollapsed(group.project?.id)" class="space-y-4 pl-6 border-l border-border">
                    <!-- Dashboards in group -->
                    <div v-if="group.dashboards.length > 0">
                        <p class="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-2 flex items-center gap-1.5">
                            <LayoutDashboard class="w-3 h-3" /> Dashboards
                        </p>
                        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                            <div
                                v-for="dash in group.dashboards"
                                :key="dash.id"
                                class="bg-card border rounded-xl p-4 flex flex-col gap-3 hover:shadow-md transition-shadow cursor-pointer group/card"
                                @click="openDashboard(dash)"
                            >
                                <div class="bg-muted/50 rounded-lg h-24 flex items-center justify-center">
                                    <LayoutDashboard class="w-8 h-8 text-muted-foreground/40" />
                                </div>
                                <div class="flex-1">
                                    <div class="flex items-start justify-between gap-2">
                                        <h3 class="font-semibold text-sm leading-tight line-clamp-2">{{ dash.name }}</h3>
                                        <Badge v-if="dash.isPublic" variant="secondary" class="shrink-0 gap-1 text-xs">
                                            <Globe class="w-3 h-3" /> Public
                                        </Badge>
                                    </div>
                                    <p class="text-xs text-muted-foreground mt-1">
                                        {{ dash.components?.length || 0 }} widget{{ (dash.components?.length || 0) !== 1 ? 's' : '' }} · {{ formatDate(dash.timestamp) }}
                                    </p>
                                </div>
                                <div class="flex gap-1 opacity-0 group-hover/card:opacity-100 transition-opacity" @click.stop>
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
                    </div>

                    <!-- Reports in group -->
                    <div v-if="group.reports.length > 0">
                        <p class="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-2 flex items-center gap-1.5">
                            <FileText class="w-3 h-3" /> SQL Reports
                        </p>
                        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                            <div
                                v-for="report in group.reports"
                                :key="report.id"
                                class="bg-card border rounded-xl p-4 flex flex-col gap-3 group/card"
                            >
                                <div class="bg-muted/50 rounded-lg h-24 flex items-center justify-center">
                                    <FileText class="w-8 h-8 text-muted-foreground/40" />
                                </div>
                                <div class="flex-1">
                                    <div class="flex items-start justify-between gap-2">
                                        <h3 class="font-semibold text-sm leading-tight line-clamp-2">{{ report.name }}</h3>
                                        <Badge v-if="report.isPublic" variant="secondary" class="shrink-0 gap-1 text-xs">
                                            <Globe class="w-3 h-3" /> Public
                                        </Badge>
                                    </div>
                                    <p class="text-xs text-muted-foreground mt-1">{{ formatDate(report.timestamp) }}</p>
                                </div>
                                <div class="flex gap-1 opacity-0 group-hover/card:opacity-100 transition-opacity">
                                    <Button variant="ghost" size="icon" class="h-8 w-8" title="Share" @click="openShareDialog(report)">
                                        <Share2 class="w-3.5 h-3.5" />
                                    </Button>
                                    <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive hover:text-destructive" title="Delete" @click="deleteReport(report)">
                                        <Trash2 class="w-3.5 h-3.5" />
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Share Dialog -->
        <Dialog v-model:open="showShareDialog">
            <DialogContent class="max-w-md">
                <DialogHeader>
                    <DialogTitle class="flex items-center gap-2">
                        <Share2 class="w-4 h-4" />
                        Share "{{ sharingItem?.name }}"
                    </DialogTitle>
                    <DialogDescription>
                        Generate a public link anyone can view without logging in.
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
                        <p class="text-xs text-muted-foreground">Anyone with this link can view this item.</p>
                    </div>
                    <div v-else class="text-center py-4">
                        <Lock class="w-8 h-8 mx-auto mb-2 text-muted-foreground" />
                        <p class="text-sm text-muted-foreground">This item is currently private.</p>
                    </div>
                </div>

                <DialogFooter>
                    <Button v-if="!shareToken" @click="enableSharing" class="gap-2 w-full">
                        <Globe class="w-4 h-4" /> Generate Public Link
                    </Button>
                    <div v-else class="flex gap-2 w-full">
                        <Button variant="destructive" @click="disableSharing" class="gap-2 flex-1">
                            <Lock class="w-4 h-4" /> Revoke Access
                        </Button>
                        <Button variant="outline" @click="showShareDialog = false" class="flex-1">Done</Button>
                    </div>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
