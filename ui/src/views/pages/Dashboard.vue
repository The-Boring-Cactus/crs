<script setup>
import { markRaw } from 'vue';
import {
    Upload,
    Printer,
    PieChart,
    Palette,
    Pencil,
    FolderOpen,
    ChevronLeft,
    Compass,
    Settings,
    Plus,
    BarChart,
    Network,
    Eraser,
    LineChart,
    Image,
    Target,
    Type,
    Download,
    Minus,
    Table as TableIcon,
    Trash2,
    ChevronDown,
    CircleDot,
    RefreshCw,
    Circle,
    ZoomIn,
    ChevronRight,
    Save,
    X,
    ArrowUpDown,
    Code,
    Share2,
    Globe,
    Lock,
    Copy,
    FileText,
    Clock,
    Database,
    Hash,
    Terminal,
    BarChart2 as BarChart2Icon,
    Braces,
    Link,
    Waves
} from 'lucide-vue-next';
import GridLayout from '@/components/draggable/GridLayout.vue';
import GridItem from '@/components/draggable/GridItem.vue';
import BaseChart from '@/components/BaseChart.vue';
import BokehChart from '@/components/BokehChart.vue';
import MarkdownReport from '@/components/MarkdownReport.vue';
import ExportMenu from '@/components/ExportMenu.vue';
import FormulaBlock from '@/components/FormulaBlock.vue';
import { buildBokehJson } from '@/helpers/bokehUtils';
import { nextTick, ref, watch, computed, getCurrentInstance, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import { toast as sonnerToast } from 'vue-sonner';
import { useDashboardStore } from '@/store/dashboardStore';

const toast = {
    add: (options) => {
        const { severity, summary, detail, life } = options;
        const opts = { description: detail, duration: life };
        if (severity === 'success') sonnerToast.success(summary, opts);
        else if (severity === 'error') sonnerToast.error(summary, opts);
        else if (severity === 'warn' || severity === 'warning') sonnerToast.warning(summary, opts);
        else if (severity === 'info') sonnerToast.info(summary, opts);
        else sonnerToast(summary, opts);
    }
};
import { Toaster } from '@/components/ui/sonner';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Checkbox } from '@/components/ui/checkbox';
import { Textarea } from '@/components/ui/textarea';
import { userStoreMe } from '@/store/userStore';
import { useProjectStore } from '@/store/projectStore';
import { useVariableStore } from '@/store/variableStore';

const userStore = userStoreMe();
const projectStore = useProjectStore();
const variableStore = useVariableStore();

const editingTitle = ref(false);
const titleInputRef = ref(null);

watch(editingTitle, (val) => {
    if (val) {
        nextTick(() => {
            titleInputRef.value?.$el?.focus();
        });
    }
});
const renderComponent = ref(true);
const dashboardId = ref(null);
const showLoadDialog = ref(false);

const dashboardStore = useDashboardStore();
const { proxy } = getCurrentInstance();
const route = useRoute();

// SQL widget picker
const showSqlPickerDialog = ref(false);
const savedSqlScripts = ref([]);
const selectedSqlScript = ref(null);

// CS script picker
const showCsPickerDialog = ref(false);
const savedCsScripts = ref([]);
const selectedCsScript = ref(null);
const csPickerMode = ref('output'); // 'output' | 'variable'
// Set when the picker is rebinding an existing FunctOutput widget to a
// (possibly different) script, instead of adding a brand-new widget.
const csPickerBindTarget = ref(null);
const csPickerRefreshInterval = ref(0);

let layout = ref({
    componentes: [{ x: 6, y: 0, w: 3, h: 1, i: '0', static: false, type: 'Text', value: 'Sample Text' }],
    formvalues: {
        name: 'New Dashboard',
        isGlobal: false
    }
});

const MyTitle = ref('Dashboard');

// Component menu configuration
const componentMenuItems = ref([
    {
        label: 'Basic Components',
        icon: markRaw(Palette),
        expanded: true,
        items: [
            {
                label: 'Text Element',
                icon: markRaw(Type),
                command: () => addcomponent('Text'),
                badge: 'Basic'
            },
            {
                label: 'Data Table',
                icon: markRaw(TableIcon),
                command: () => addcomponent('DataTable'),
                badge: 'Data'
            },
            {
                label: 'Tree Table',
                icon: markRaw(Network),
                command: () => addcomponent('TreeTable'),
                badge: 'Tree'
            },
            {
                label: 'Image',
                icon: markRaw(Image),
                command: () => addcomponent('Image'),
                badge: 'Media'
            },
            {
                label: 'Select Dropdown',
                icon: markRaw(ChevronDown),
                command: () => addcomponent('Select'),
                badge: 'Input'
            },
            {
                label: 'Input Text',
                icon: markRaw(Pencil),
                command: () => addcomponent('InputText'),
                badge: 'Input'
            }
        ]
    },
    {
        label: 'Chart Components',
        icon: markRaw(BarChart),
        expanded: true,
        items: [
            {
                label: 'Basic Charts',
                icon: markRaw(LineChart),
                items: [
                    {
                        label: 'Line Chart',
                        icon: markRaw(LineChart),
                        command: () => addcomponent('LineChart')
                    },
                    {
                        label: 'Bar Chart',
                        icon: markRaw(BarChart),
                        command: () => addcomponent('BarChart')
                    },
                    {
                        label: 'Area Chart',
                        icon: markRaw(LineChart),
                        command: () => addcomponent('AreaChart')
                    }
                ]
            },
            {
                label: 'Circular Charts',
                icon: markRaw(PieChart),
                items: [
                    {
                        label: 'Pie Chart',
                        icon: markRaw(PieChart),
                        command: () => addcomponent('PieChart')
                    },
                    {
                        label: 'Doughnut Chart',
                        icon: markRaw(Circle),
                        command: () => addcomponent('DoughnutChart')
                    },
                    {
                        label: 'Polar Area Chart',
                        icon: markRaw(Target),
                        command: () => addcomponent('PolarAreaChart')
                    }
                ]
            },
            {
                label: 'Advanced Charts',
                icon: markRaw(Compass),
                items: [
                    {
                        label: 'Radar Chart',
                        icon: markRaw(Compass),
                        command: () => addcomponent('RadarChart')
                    },
                    {
                        label: 'Scatter Plot',
                        icon: markRaw(CircleDot),
                        command: () => addcomponent('ScatterChart')
                    },
                    {
                        label: 'Bubble Chart',
                        icon: markRaw(Circle),
                        command: () => addcomponent('BubbleChart')
                    },
                    {
                        label: 'Mixed Chart',
                        icon: markRaw(BarChart),
                        command: () => addcomponent('MixedChart'),
                        badge: 'Pro'
                    }
                ]
            }
        ]
    },
    {
        label: 'Data Widgets',
        icon: markRaw(Database),
        expanded: true,
        items: [
            {
                label: 'SQL Visualization',
                icon: markRaw(BarChart2Icon),
                command: () => openSqlPickerDialog(),
                badge: 'SQL'
            },
            {
                label: 'CS Variable',
                icon: markRaw(Hash),
                command: () => openCsPickerDialog('variable'),
                badge: 'KPI'
            },
            {
                label: 'CS Script Output',
                icon: markRaw(Terminal),
                command: () => openCsPickerDialog('output'),
                badge: 'Live'
            }
        ]
    },
    {
        label: 'Dashboard Actions',
        icon: markRaw(Settings),
        items: [
            {
                label: 'Save To Server',
                icon: markRaw(Save),
                command: () => saveToServer(),
                badge: 'Cloud'
            },
            {
                label: 'Load From Server',
                icon: markRaw(FolderOpen),
                command: () => openLoadDialog()
            },
            {
                label: 'Export Local Layout',
                icon: markRaw(Download),
                command: () => saveDashboardLayout()
            },
            {
                label: 'Import Local Layout',
                icon: markRaw(FolderOpen),
                command: () => loadDashboardLayout()
            },
            {
                label: 'Reset Dashboard',
                icon: markRaw(RefreshCw),
                command: () => resetDashboard()
            },
            {
                label: 'Export Dashboard (All)',
                icon: markRaw(Download),
                command: () => exportDashboard()
            }
        ]
    }
]);

// Chart type mapping
const chartTypeMap = {
    LineChart: 'line',
    BarChart: 'bar',
    PieChart: 'pie',
    DoughnutChart: 'doughnut',
    RadarChart: 'radar',
    PolarAreaChart: 'polarArea',
    ScatterChart: 'scatter',
    BubbleChart: 'bubble',
    AreaChart: 'area',
    MixedChart: 'mixed'
};

// Chart types BokehChart can render (radar/polarArea/bubble/mixed have no Bokeh equivalent here)
const BOKEH_SUPPORTED_TYPES = ['line', 'bar', 'bar-h', 'area', 'pie', 'doughnut', 'scatter'];
function isBokehSupported(chartType) {
    return BOKEH_SUPPORTED_TYPES.includes(chartTypeMap[chartType] || chartType);
}

// Generate sample data for different chart types
function generateChartData(chartType) {
    const baseLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
    const baseData = [12, 19, 3, 17, 6, 13];

    switch (chartType) {
        case 'LineChart':
        case 'AreaChart':
            return {
                labels: baseLabels,
                datasets: [
                    {
                        label: 'Sales',
                        data: baseData,
                        borderColor: 'rgba(75, 192, 192, 1)',
                        backgroundColor: chartType === 'AreaChart' ? 'rgba(75, 192, 192, 0.2)' : 'transparent',
                        tension: 0.4
                    }
                ]
            };

        case 'BarChart':
            return {
                labels: baseLabels,
                datasets: [
                    {
                        label: 'Revenue',
                        data: baseData,
                        backgroundColor: ['rgba(255, 99, 132, 0.8)', 'rgba(54, 162, 235, 0.8)', 'rgba(255, 205, 86, 0.8)', 'rgba(75, 192, 192, 0.8)', 'rgba(153, 102, 255, 0.8)', 'rgba(255, 159, 64, 0.8)']
                    }
                ]
            };

        case 'PieChart':
        case 'DoughnutChart':
            return {
                labels: ['Product A', 'Product B', 'Product C', 'Product D', 'Product E'],
                datasets: [
                    {
                        data: [30, 25, 20, 15, 10],
                        backgroundColor: ['rgba(255, 99, 132, 0.8)', 'rgba(54, 162, 235, 0.8)', 'rgba(255, 205, 86, 0.8)', 'rgba(75, 192, 192, 0.8)', 'rgba(153, 102, 255, 0.8)']
                    }
                ]
            };

        case 'RadarChart':
            return {
                labels: ['Speed', 'Reliability', 'Comfort', 'Safety', 'Efficiency'],
                datasets: [
                    {
                        label: 'Performance',
                        data: [65, 59, 90, 81, 56],
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        fill: true
                    }
                ]
            };

        case 'PolarAreaChart':
            return {
                labels: ['Red', 'Green', 'Yellow', 'Grey', 'Blue'],
                datasets: [
                    {
                        data: [11, 16, 7, 3, 14],
                        backgroundColor: ['rgba(255, 99, 132, 0.8)', 'rgba(75, 192, 192, 0.8)', 'rgba(255, 205, 86, 0.8)', 'rgba(201, 203, 207, 0.8)', 'rgba(54, 162, 235, 0.8)']
                    }
                ]
            };

        case 'ScatterChart':
            return {
                datasets: [
                    {
                        label: 'Data Points',
                        data: [
                            { x: -10, y: 0 },
                            { x: 0, y: 10 },
                            { x: 10, y: 5 },
                            { x: 0.5, y: 5.5 },
                            { x: -5, y: 8 }
                        ],
                        backgroundColor: 'rgba(255, 99, 132, 0.6)'
                    }
                ]
            };

        case 'BubbleChart':
            return {
                datasets: [
                    {
                        label: 'Bubble Data',
                        data: [
                            { x: 20, y: 30, r: 15 },
                            { x: 40, y: 10, r: 10 },
                            { x: 15, y: 22, r: 25 },
                            { x: 35, y: 25, r: 12 }
                        ],
                        backgroundColor: 'rgba(255, 99, 132, 0.6)'
                    }
                ]
            };

        case 'MixedChart':
            return {
                labels: baseLabels,
                datasets: [
                    {
                        type: 'bar',
                        label: 'Bars',
                        data: baseData,
                        backgroundColor: 'rgba(255, 99, 132, 0.8)'
                    },
                    {
                        type: 'line',
                        label: 'Line',
                        data: [5, 10, 15, 8, 12, 18],
                        borderColor: 'rgba(54, 162, 235, 1)',
                        backgroundColor: 'transparent',
                        fill: false
                    }
                ]
            };

        default:
            return {
                labels: baseLabels,
                datasets: [
                    {
                        label: 'Data',
                        data: baseData,
                        backgroundColor: 'rgba(75, 192, 192, 0.8)'
                    }
                ]
            };
    }
}

// Always produces a unique i value even after components have been removed.
// Using length would re-issue an already-used ID once a component is deleted.
function nextComponentId() {
    const max = layout.value.componentes.reduce((m, c) => {
        const n = parseInt(c.i);
        return isNaN(n) ? m : Math.max(m, n);
    }, -1);
    return (max + 1).toString();
}

const addcomponent = async (type) => {
    console.log('Adding component:', type);

    let newComponent;

    if (type === 'Text') {
        newComponent = {
            x: 1,
            y: 0,
            w: 3,
            h: 2,
            i: nextComponentId(),
            static: false,
            type: 'Text',
            value: 'Click to edit'
        };
    } else if (type === 'DataTable') {
        newComponent = {
            x: 1,
            y: 0,
            w: 10,
            h: 8,
            i: nextComponentId(),
            static: false,
            type: 'DataTable',
            title: 'Data Table',
            tableData: generateSampleCustomers(),
            columns: generateDataTableColumns(),
            globalFilter: '',
            selectedRows: [],
            scriptCode: ''
        };
    } else if (type === 'TreeTable') {
        newComponent = {
            x: 1,
            y: 0,
            w: 10,
            h: 8,
            i: nextComponentId(),
            static: false,
            type: 'TreeTable',
            title: 'Tree Table',
            treeData: generateTreeData(),
            columns: generateTreeTableColumns(),
            expandedKeys: {},
            scriptCode: '',
            refreshInterval: 0
        };
    } else if (type === 'Image') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 5,
            i: nextComponentId(),
            static: false,
            type: 'Image',
            title: 'Image Component',
            src: 'https://via.placeholder.com/400x300?text=Sample+Image',
            alt: 'Sample image',
            preview: true,
            width: '100%',
            height: 'auto'
        };
    } else if (type === 'Select') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 3,
            i: nextComponentId(),
            static: false,
            type: 'Select',
            title: 'Select Dropdown',
            selectedValue: null,
            options: [],
            optionsSource: 'csv',
            csvValues: '',
            sqlDatabase: '',
            sqlQuery: '',
            placeholder: 'Select an option'
        };
    } else if (type === 'InputText') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 2,
            i: nextComponentId(),
            static: false,
            type: 'InputText',
            title: 'Input Text',
            value: '',
            placeholder: 'Enter text here...',
            size: 'medium'
        };
    } else if (type === 'Variable') {
        newComponent = {
            x: 1, y: 0, w: 3, h: 3,
            i: nextComponentId(),
            static: false,
            type: 'Variable',
            label: 'KPI',
            value: '0',
            unit: '',
            description: '',
            scriptCode: '',
            refreshInterval: 0
        };
    } else if (type === 'FunctOutput') {
        newComponent = {
            x: 1, y: 0, w: 6, h: 7,
            i: nextComponentId(),
            static: false,
            type: 'FunctOutput',
            title: 'Script Output',
            outputType: null,
            chartType: 'bar',
            chartData: null,
            tableColumns: [],
            tableData: [],
            statReportData: null,
            printOutput: '',
            scriptCode: '',
            refreshInterval: 0
        };
    } else if (isChartType(type)) {
        // Chart components get larger default sizes
        const chartSizes = {
            PieChart: { w: 4, h: 6 },
            DoughnutChart: { w: 4, h: 6 },
            RadarChart: { w: 5, h: 6 },
            PolarAreaChart: { w: 4, h: 6 },
            ScatterChart: { w: 5, h: 5 },
            BubbleChart: { w: 5, h: 5 },
            default: { w: 6, h: 5 }
        };

        const size = chartSizes[type] || chartSizes.default;

        newComponent = {
            x: 1,
            y: 0,
            w: size.w,
            h: size.h,
            i: nextComponentId(),
            static: false,
            type: type,
            title: getDefaultChartTitle(type),
            chartData: generateChartData(type),
            showLegend: true,
            showDataLabels: true,
            scriptCode: ''
        };
    }

    if (newComponent) {
        layout.value.componentes.push(newComponent);
        renderComponent.value = false;
        await nextTick();
        renderComponent.value = true;

        toast.add({
            severity: 'success',
            summary: 'Component Added',
            detail: `${getDefaultChartTitle(type) || type} has been added to the dashboard`,
            life: 3000
        });
    }
};

// Helper functions
function isChartType(type) {
    return Object.keys(chartTypeMap).includes(type);
}

function getChartType(componentType) {
    return chartTypeMap[componentType] || 'line';
}

function getDefaultChartTitle(type) {
    const titles = {
        LineChart: 'Line Chart',
        BarChart: 'Bar Chart',
        PieChart: 'Pie Chart',
        DoughnutChart: 'Doughnut Chart',
        RadarChart: 'Radar Chart',
        PolarAreaChart: 'Polar Area Chart',
        ScatterChart: 'Scatter Plot',
        BubbleChart: 'Bubble Chart',
        AreaChart: 'Area Chart',
        MixedChart: 'Mixed Chart',
        Text: 'Text Element',
        DataTable: 'Data Table',
        TreeTable: 'Tree Table',
        Image: 'Image',
        Select: 'Select Dropdown',
        InputText: 'Input Text',
        Variable: 'Variable / KPI',
        FunctOutput: 'Script Output',
        SqlWidget: 'SQL Visualization'
    };
    return titles[type] || type;
}

function getChartHeight(gridHeight) {
    // Convert grid height to pixel height (each grid unit is approximately 40px + margin)
    return `${gridHeight * 40 - 60}px`;
}

function refreshChartData(item) {
    item.chartData = generateChartData(item.type);
    toast.add({
        severity: 'info',
        summary: 'Data Refreshed',
        detail: `${item.title || item.type} data has been refreshed`,
        life: 2000
    });
}

const settingsDialogVisible = ref(false);
const selectedChartItem = ref(null);

function editChartSettings(item) {
    selectedChartItem.value = item;
    settingsDialogVisible.value = true;
}

function closeSettingsDialog() {
    settingsDialogVisible.value = false;
    selectedChartItem.value = null;
}

function removeComponent(itemId) {
    const index = layout.value.componentes.findIndex((item) => item.i === itemId);
    if (index !== -1) {
        const component = layout.value.componentes[index];
        layout.value.componentes.splice(index, 1);

        toast.add({
            severity: 'warn',
            summary: 'Component Removed',
            detail: `${component.title || component.type} has been removed`,
            life: 2000
        });
    }
}

function onChartClick(event) {
    console.log('Chart clicked:', event);
    toast.add({
        severity: 'info',
        summary: 'Chart Interaction',
        detail: `Clicked on ${event.label || 'chart element'}`,
        life: 2000
    });
}

// DataTable helper functions
function generateSampleCustomers() {
    const countries = ['USA', 'Canada', 'UK', 'Germany', 'France', 'Italy', 'Spain', 'Australia'];
    const companies = ['TechCorp', 'DataSoft', 'InnovateLtd', 'GlobalTech', 'FutureSys', 'SmartCode', 'NextGen', 'ProTech'];
    const statuses = ['active', 'inactive', 'pending'];

    return Array.from({ length: 25 }, (_, index) => ({
        id: index + 1,
        name: `Customer ${index + 1}`,
        company: companies[Math.floor(Math.random() * companies.length)],
        country: countries[Math.floor(Math.random() * countries.length)],
        email: `customer${index + 1}@${companies[Math.floor(Math.random() * companies.length)].toLowerCase()}.com`,
        status: statuses[Math.floor(Math.random() * statuses.length)],
        revenue: Math.floor(Math.random() * 100000) + 10000,
        joinDate: new Date(2020 + Math.floor(Math.random() * 4), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toLocaleDateString()
    }));
}

function generateDataTableColumns() {
    return [
        { field: 'id', header: 'ID' },
        { field: 'name', header: 'Name' },
        { field: 'company', header: 'Company' },
        { field: 'country', header: 'Country' },
        { field: 'email', header: 'Email' },
        { field: 'status', header: 'Status' },
        { field: 'revenue', header: 'Revenue' },
        { field: 'joinDate', header: 'Join Date' }
    ];
}

function getStatusSeverity(status) {
    switch (status) {
        case 'active':
            return 'success';
        case 'inactive':
            return 'danger';
        case 'pending':
            return 'warning';
        default:
            return 'info';
    }
}

function getDataTableHeight(gridHeight) {
    return `${gridHeight * 40 - 100}px`;
}

function addDataTableRow(item) {
    const newId = Math.max(...item.tableData.map((row) => row.id || 0)) + 1;
    const newRow = {
        id: newId,
        name: `New Customer ${newId}`,
        company: 'New Company',
        country: 'USA',
        email: `customer${newId}@newcompany.com`,
        status: 'pending',
        revenue: 50000,
        joinDate: new Date().toLocaleDateString()
    };
    item.tableData.push(newRow);

    toast.add({
        severity: 'success',
        summary: 'Row Added',
        detail: 'New customer has been added to the table',
        life: 2000
    });
}

// Converts flat Table() output rows into TreeTable's nested {key, data, children}
// shape, using "id"/"parentId" fields on each row to establish parent/child
// links (rows without a matching parentId become root nodes).
function buildTreeFromRows(rows, dataFields) {
    const byId = new Map();
    rows.forEach((row) => {
        const data = {};
        dataFields.forEach((f) => { data[f] = row[f]; });
        byId.set(String(row.id), { key: String(row.id), data, children: [] });
    });

    const roots = [];
    rows.forEach((row) => {
        const node = byId.get(String(row.id));
        const parentId = row.parentId;
        const parent = parentId !== undefined && parentId !== null && parentId !== '' ? byId.get(String(parentId)) : null;
        if (parent) parent.children.push(node);
        else roots.push(node);
    });

    // Match generateTreeData()'s convention: leaf nodes have no children key.
    const strip = (node) => {
        if (node.children.length === 0) delete node.children;
        else node.children.forEach(strip);
    };
    roots.forEach(strip);
    return roots;
}

// TreeTable helper functions
function generateTreeData() {
    return [
        {
            key: '0',
            data: {
                name: 'Applications',
                size: '100kb',
                type: 'Folder'
            },
            children: [
                {
                    key: '0-0',
                    data: {
                        name: 'Vue',
                        size: '25kb',
                        type: 'Folder'
                    },
                    children: [
                        {
                            key: '0-0-0',
                            data: {
                                name: 'vue.app',
                                size: '10kb',
                                type: 'Application'
                            }
                        },
                        {
                            key: '0-0-1',
                            data: {
                                name: 'native.app',
                                size: '15kb',
                                type: 'Application'
                            }
                        }
                    ]
                },
                {
                    key: '0-1',
                    data: {
                        name: 'editor.app',
                        size: '25kb',
                        type: 'Application'
                    }
                }
            ]
        },
        {
            key: '1',
            data: {
                name: 'Documents',
                size: '75kb',
                type: 'Folder'
            },
            children: [
                {
                    key: '1-0',
                    data: {
                        name: 'Work',
                        size: '55kb',
                        type: 'Folder'
                    },
                    children: [
                        {
                            key: '1-0-0',
                            data: {
                                name: 'Expenses.doc',
                                size: '30kb',
                                type: 'Document'
                            }
                        },
                        {
                            key: '1-0-1',
                            data: {
                                name: 'Resume.doc',
                                size: '25kb',
                                type: 'Document'
                            }
                        }
                    ]
                },
                {
                    key: '1-1',
                    data: {
                        name: 'Home',
                        size: '20kb',
                        type: 'Folder'
                    },
                    children: [
                        {
                            key: '1-1-0',
                            data: {
                                name: 'Invoices.txt',
                                size: '20kb',
                                type: 'Text'
                            }
                        }
                    ]
                }
            ]
        }
    ];
}

function generateTreeTableColumns() {
    return [
        { field: 'name', header: 'Name', expander: true },
        { field: 'size', header: 'Size' },
        { field: 'type', header: 'Type' }
    ];
}

function getTreeTableHeight(gridHeight) {
    return `${gridHeight * 40 - 100}px`;
}

function expandAllTreeNodes(item) {
    const expandRecursively = (nodes, keys = {}) => {
        nodes.forEach((node) => {
            if (node.children && node.children.length > 0) {
                keys[node.key] = true;
                expandRecursively(node.children, keys);
            }
        });
        return keys;
    };

    item.expandedKeys = expandRecursively(item.treeData);

    toast.add({
        severity: 'info',
        summary: 'Nodes Expanded',
        detail: 'All tree nodes have been expanded',
        life: 2000
    });
}

function collapseAllTreeNodes(item) {
    item.expandedKeys = {};

    toast.add({
        severity: 'info',
        summary: 'Nodes Collapsed',
        detail: 'All tree nodes have been collapsed',
        life: 2000
    });
}

// Image helper functions
function refreshImage(item) {
    const imageUrls = [
        'https://via.placeholder.com/400x300?text=Sample+Image+1',
        'https://via.placeholder.com/400x300?text=Sample+Image+2',
        'https://via.placeholder.com/400x300?text=Sample+Image+3',
        'https://via.placeholder.com/400x300?text=Sample+Image+4'
    ];

    item.src = imageUrls[Math.floor(Math.random() * imageUrls.length)];

    toast.add({
        severity: 'info',
        summary: 'Image Refreshed',
        detail: 'Image source has been updated',
        life: 2000
    });
}

// Opens a file picker, reads the chosen image as a base64 data URL, and
// stores it directly on the widget so it's saved with the rest of the
// dashboard JSON (no separate asset storage/upload endpoint needed).
function selectImageFile(item) {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = 'image/*';
    fileInput.style.display = 'none';

    fileInput.onchange = (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                item.src = e.target.result;
                item.alt = file.name;
                toast.add({ severity: 'success', summary: 'Image Selected', detail: file.name, life: 2000 });
            };
            reader.onerror = () => {
                toast.add({ severity: 'error', summary: 'Read Failed', detail: 'Could not read the selected image file.', life: 3000 });
            };
            reader.readAsDataURL(file);
        }
        document.body.removeChild(fileInput);
    };

    document.body.appendChild(fileInput);
    fileInput.click();
}

// Select widget helpers
// Applies comma-separated text to item.options immediately (CSV mode).
function applyCsvOptions(item) {
    item.options = (item.csvValues || '').split(',').map(s => s.trim()).filter(Boolean);
}

// Executes a SQL query and stores first-column values as item.options (SQL mode).
async function loadSqlSelectOptions(item) {
    if (!item.sqlDatabase || !item.sqlQuery?.trim()) {
        toast.add({ severity: 'warn', summary: 'Missing Config', detail: 'Select a database and enter a SQL query.', life: 3000 });
        return;
    }
    item.sqlLoading = true;
    try {
        const result = await userStore.executeCommand('ExecuteSql', {
            database: item.sqlDatabase,
            code: item.sqlQuery
        }, proxy.$socket);
        const rows = result?.Data?.rows || [];
        const columns = result?.Data?.columns || [];
        const firstCol = columns[0]?.field;
        item.options = rows.map(r => String(r[firstCol] ?? '')).filter(Boolean);
        toast.add({ severity: 'success', summary: 'Options Loaded', detail: `${item.options.length} option(s) loaded`, life: 2000 });
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Query Failed', detail: error.message, life: 3000 });
    } finally {
        item.sqlLoading = false;
    }
}

// Database connections available to the Select widget SQL panel.
const selectDatabases = ref([]);
async function ensureSelectDatabases() {
    if (selectDatabases.value.length > 0) return;
    try {
        const result = await userStore.executeCommand('LoadDatabaseConnections', {}, proxy.$socket);
        selectDatabases.value = (result?.Data || []).map(d => ({
            id: d.id || d.Id,
            name: d.name || d.Name || 'Unnamed'
        }));
    } catch { selectDatabases.value = []; }
}

function onSelectChange(item) {
    toast.add({ severity: 'info', summary: 'Selection Changed', detail: `Selected: ${item.selectedValue || 'None'}`, life: 2000 });
}

// Select widget config dialog
const selectConfigDialog = ref(false);
const selectConfigItem = ref(null);
function openSelectConfig(item) {
    selectConfigItem.value = item;
    selectConfigDialog.value = true;
    ensureSelectDatabases();
}

// Variable binding
const bindingDialogItem = ref(null);
const bindingDialogOpen = ref(false);
function openBindDialog(item) {
    bindingDialogItem.value = item;
    bindingDialogOpen.value = true;
}
function setVariableBinding(varName) {
    if (bindingDialogItem.value) bindingDialogItem.value.boundVariable = varName || undefined;
    bindingDialogOpen.value = false;
    // Re-resolve options for the newly bound variable
    if (varName) {
        const def = variableStore.definitions.find(d => d.name === varName);
        if (def?.type === 'dropdown') {
            variableStore.resolveDropdownOptions(def, proxy.$socket).then(opts => {
                varOptions.value[varName] = opts;
            });
        }
    }
}

// Resolved dropdown options for bound variables (variable name → string[])
const varOptions = ref({});

async function resolveAllVarOptions() {
    const seen = new Set();
    for (const item of layout.value.componentes) {
        if (!item.boundVariable || seen.has(item.boundVariable)) continue;
        seen.add(item.boundVariable);
        const def = variableStore.definitions.find(d => d.name === item.boundVariable);
        if (def?.type === 'dropdown') {
            varOptions.value[item.boundVariable] = await variableStore.resolveDropdownOptions(def, proxy.$socket);
        }
    }
}

// Re-resolve whenever variable definitions are (re)loaded
watch(() => variableStore.definitions.length, resolveAllVarOptions);

// InputText helper functions
function clearInputText(item) {
    item.value = '';

    toast.add({
        severity: 'info',
        summary: 'Text Cleared',
        detail: 'Input text has been cleared',
        life: 2000
    });
}

function onInputTextChange(item, event) {
    console.log('Input text changed:', event.target.value);
    // You can add validation or other logic here
}

// Dashboard Actions
function saveDashboardLayout() {
    try {
        const layoutData = {
            title: MyTitle.value,
            components: layout.value.componentes,
            timestamp: new Date().toISOString(),
            version: '1.0'
        };

        const dataStr = JSON.stringify(layoutData, null, 2);
        const blob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `dashboard-layout-${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        URL.revokeObjectURL(url);

        toast.add({
            severity: 'success',
            summary: 'Layout Saved',
            detail: 'Dashboard layout has been exported successfully',
            life: 3000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Save Failed',
            detail: 'Failed to save dashboard layout',
            life: 3000
        });
    }
}

async function saveToServer() {
    try {
        const layoutData = {
            id: dashboardId.value,
            name: MyTitle.value,
            title: MyTitle.value,
            projectId: projectStore.currentProjectId || undefined,
            shareToken: layout.value.formvalues?.shareToken || undefined,
            isPublic: layout.value.formvalues?.isPublic || false,
            components: layout.value.componentes,
            timestamp: new Date().toISOString()
        };

        const saveResult = await dashboardStore.saveDashboard(layoutData, proxy.$socket);
        if (saveResult?.Data?.id && !dashboardId.value) dashboardId.value = saveResult.Data.id;

        toast.add({
            severity: 'success',
            summary: 'Saved to Server',
            detail: 'Dashboard layout has been saved successfully to the server',
            life: 3000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Save Failed',
            detail: 'Failed to save dashboard to the server',
            life: 3000
        });
    }
}

async function openLoadDialog() {
    await dashboardStore.loadDashboards(proxy.$socket);
    showLoadDialog.value = true;
}

// Run after any dashboard load (server or file) to restore dynamic state that
// is not fully captured in the saved JSON.
async function afterLoadComponents(items) {
    // 1. Migrate Select widgets that were saved before the CSV/SQL rework
    //    (options were stored as objects; new format is always flat strings).
    for (const item of items) {
        if (item.type !== 'Select') continue;
        // Backfill missing fields so old saves work without reconfiguring
        if (!item.optionsSource) item.optionsSource = 'csv';
        if (item.csvValues === undefined) item.csvValues = '';
        if (item.sqlDatabase === undefined) item.sqlDatabase = '';
        if (item.sqlQuery === undefined) item.sqlQuery = '';
        // Convert object options (old format: [{name,code}]) → flat strings
        if (Array.isArray(item.options) && item.options.some(o => o !== null && typeof o === 'object')) {
            const lk = item.optionLabel || 'label';
            const vk = item.optionValue || 'value';
            item.options = item.options.map(o =>
                typeof o === 'string' ? o :
                (o[lk] || o[vk] || o['name'] || o['code'] || String(o))
            );
            item.optionsSource = 'csv';
            item.csvValues = item.options.join(', ');
        }
        // Re-execute SQL queries so options are always fresh after a load
        if (item.optionsSource === 'sql' && item.sqlDatabase && item.sqlQuery?.trim()) {
            loadSqlSelectOptions(item); // intentionally not awaited — runs in background
        }
    }

    // 2. Re-resolve dropdown options for bound variables.
    //    The watch on definitions.length only fires when definitions change,
    //    so it won't trigger if definitions were already loaded before this load.
    await resolveAllVarOptions();

    // 3. Resume auto-refresh timers for script-bound widgets — timers are
    //    runtime-only state and don't survive a reload otherwise.
    for (const item of items) {
        if (item.scriptId && item.refreshInterval > 0) startAutoRefresh(item);
    }
}

async function loadFromServer(dash) {
    // Components may be directly on the object or inside the config JSON string
    let components = dash.components;
    let title = dash.title || dash.name || 'Imported Dashboard';
    let shareToken = dash.shareToken || dash.sharetoken || null;
    let isPublic = dash.isPublic ?? dash.ispublic ?? false;

    if (!components && dash.config) {
        try {
            const cfg = typeof dash.config === 'string' ? JSON.parse(dash.config) : dash.config;
            components = cfg.components;
            if (cfg.title || cfg.name) title = cfg.title || cfg.name;
            if (!shareToken) shareToken = cfg.shareToken || cfg.sharetoken || null;
            if (!isPublic) isPublic = cfg.isPublic ?? cfg.ispublic ?? false;
        } catch { /* malformed config — leave components undefined */ }
    }

    if (dash && components) {
        dashboardId.value = dash.id || dash.Id;
        MyTitle.value = title;
        layout.value.componentes = components;
        layout.value.formvalues = {
            ...layout.value.formvalues,
            name: title,
            shareToken: shareToken,
            isPublic: isPublic
        };
        currentShareToken.value = shareToken || '';
        showLoadDialog.value = false;

        // Restore dynamic state (variable options, Select settings migration)
        await afterLoadComponents(components);

        // Force re-render
        renderComponent.value = false;
        await nextTick();
        renderComponent.value = true;

        toast.add({
            severity: 'success',
            summary: 'Layout Loaded',
            detail: 'Dashboard layout has been loaded successfully from server',
            life: 3000
        });
    }
}

// Loads the dashboard referenced by ?id=... in the route (e.g. from the
// Welcome screen's "Recent Dashboards" list), which links here without
// going through the in-page Load dialog.
async function loadDashboardFromRoute() {
    const id = route.query.id;
    if (!id) return;

    await dashboardStore.loadDashboards(proxy.$socket);
    const dash = dashboardStore.dashboards.find((d) => (d.id || d.Id) === id);

    if (dash) {
        await loadFromServer(dash);
    } else {
        toast.add({
            severity: 'error',
            summary: 'Dashboard Not Found',
            detail: 'The requested dashboard could not be loaded.',
            life: 3000
        });
    }
}

function loadDashboardLayout() {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.json';
    fileInput.style.display = 'none';

    fileInput.onchange = async (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = async (e) => {
                try {
                    const layoutData = JSON.parse(e.target.result);

                    if (layoutData.components && Array.isArray(layoutData.components)) {
                        MyTitle.value = layoutData.title || 'Imported Dashboard';
                        layout.value.componentes = layoutData.components;

                        // Restore dynamic state (variable options, Select settings migration)
                        await afterLoadComponents(layoutData.components);

                        // Force re-render
                        renderComponent.value = false;
                        await nextTick();
                        renderComponent.value = true;

                        toast.add({
                            severity: 'success',
                            summary: 'Layout Loaded',
                            detail: 'Dashboard layout has been imported successfully',
                            life: 3000
                        });
                    } else {
                        throw new Error('Invalid layout format');
                    }
                } catch (error) {
                    toast.add({
                        severity: 'error',
                        summary: 'Load Failed',
                        detail: 'Invalid layout file format',
                        life: 3000
                    });
                }
            };
            reader.readAsText(file);
        }
        document.body.removeChild(fileInput);
    };

    document.body.appendChild(fileInput);
    fileInput.click();
}

function resetDashboard() {
    layout.value.componentes = [{ x: 6, y: 0, w: 3, h: 1, i: '0', static: false, type: 'Text', value: 'Sample Text' }];
    MyTitle.value = 'Dashboard';

    toast.add({
        severity: 'info',
        summary: 'Dashboard Reset',
        detail: 'Dashboard has been reset to default state',
        life: 3000
    });
}

function exportDashboard() {
    try {
        const exportData = {
            dashboard: {
                title: MyTitle.value,
                components: layout.value.componentes,
                createdAt: new Date().toISOString()
            },
            metadata: {
                version: '1.0',
                componentCount: layout.value.componentes.length,
                chartCount: layout.value.componentes.filter((c) => isChartType(c.type)).length,
                dataTableCount: layout.value.componentes.filter((c) => c.type === 'DataTable').length,
                treeTableCount: layout.value.componentes.filter((c) => c.type === 'TreeTable').length,
                inputCount: layout.value.componentes.filter((c) => ['Select', 'InputText'].includes(c.type)).length,
                imageCount: layout.value.componentes.filter((c) => c.type === 'Image').length
            }
        };

        const dataStr = JSON.stringify(exportData, null, 2);
        const blob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `dashboard-export-${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        URL.revokeObjectURL(url);

        toast.add({
            severity: 'success',
            summary: 'Dashboard Exported',
            detail: `Exported dashboard with ${exportData.metadata.componentCount} components`,
            life: 3000
        });
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Export Failed',
            detail: 'Failed to export dashboard',
            life: 3000
        });
    }
}

const itemTitle = (item) => {
    let result = item.i;
    if (item.static) {
        result += ' - Static';
    }
    return result;
};

// Widget script execution state
// Share dialog
const showShareDialog = ref(false);
const currentShareToken = ref('');
const shareUrl = computed(() =>
    currentShareToken.value ? `${window.location.origin}/share/${currentShareToken.value}` : ''
);

async function openShareDialog() {
    // Ensure dashboard is saved before sharing
    if (!dashboardId.value) await saveToServer();
    currentShareToken.value = layout.value.formvalues?.shareToken || '';
    showShareDialog.value = true;
}

async function enableSharing() {
    if (!dashboardId.value) return;
    try {
        const result = await userStore.executeCommand('ShareDashboard', { id: dashboardId.value, enable: true }, proxy.$socket);
        const token = result?.Data?.shareToken;
        if (token) {
            currentShareToken.value = token;
            layout.value.formvalues = { ...layout.value.formvalues, shareToken: token, isPublic: true };
        }
        sonnerToast.success('Dashboard is now public');
    } catch { sonnerToast.error('Failed to enable sharing'); }
}

async function disableSharing() {
    if (!dashboardId.value) return;
    try {
        await userStore.executeCommand('ShareDashboard', { id: dashboardId.value, enable: false }, proxy.$socket);
        currentShareToken.value = '';
        layout.value.formvalues = { ...layout.value.formvalues, shareToken: null, isPublic: false };
        sonnerToast.success('Sharing disabled');
    } catch { sonnerToast.error('Failed to disable sharing'); }
}

function copyShareUrl() {
    navigator.clipboard.writeText(shareUrl.value);
    sonnerToast.success('Link copied to clipboard');
}

const widgetScriptDialog = ref(false);
const editingScriptWidget = ref(null);
const widgetExecutingId = ref(null);

// StatReport viewer
const showStatReportDialog = ref(false);
const viewingStatReport = ref(null);

function viewWidgetStatReport(item) {
    viewingStatReport.value = item.statReportData;
    showStatReportDialog.value = true;
}

// Auto-refresh timers per widget
const widgetTimers = {};

function startAutoRefresh(item) {
    if (!item.refreshInterval || item.refreshInterval <= 0) return;
    stopAutoRefresh(item);
    widgetTimers[item.i] = setInterval(() => {
        runWidgetScript(item);
    }, item.refreshInterval * 60 * 1000);
}

function stopAutoRefresh(item) {
    if (widgetTimers[item.i]) {
        clearInterval(widgetTimers[item.i]);
        delete widgetTimers[item.i];
    }
}

function applyWidgetScript() {
    const item = editingScriptWidget.value;
    widgetScriptDialog.value = false;
    if (item) {
        startAutoRefresh(item);
        runWidgetScript(item);
    }
}

const handleWidgetOutput = (e) => {
    const response = e.detail;
    const dataType = response.DataType || response.dataType;
    const payload = response.Payload || response.payload;
    if (!dataType || !payload || !widgetExecutingId.value) return;

    const widget = layout.value.componentes.find(c => c.i === widgetExecutingId.value);
    if (!widget) return;

    // Legacy handling for existing chart / table widgets
    if (dataType === 'Chart' && isChartType(widget.type)) {
        widget.chartData = { labels: payload.labels || [], datasets: payload.datasets || [] };
    } else if (dataType === 'Table' && widget.type === 'DataTable') {
        widget.columns = (payload.columns || []).map(col => ({ field: col, header: col }));
        widget.tableData = payload.rows || [];
    } else if (dataType === 'Table' && widget.type === 'TreeTable') {
        // "id"/"parentId" columns establish the tree; everything else is displayed data.
        const dataFields = (payload.columns || []).filter((c) => c !== 'id' && c !== 'parentId');
        widget.columns = dataFields.map((f, i) => ({ field: f, header: f, expander: i === 0 }));
        widget.treeData = buildTreeFromRows(payload.rows || [], dataFields);
        widget.expandedKeys = {};
    } else if (dataType === 'StatReport' && widget.type !== 'FunctOutput') {
        widget.statReportData = payload;
        showStatReportDialog.value = true;
        viewingStatReport.value = payload;
    }

    // FunctOutput: accept any structured output type
    if (widget.type === 'FunctOutput') {
        if (dataType === 'Chart') {
            widget.outputType = 'chart';
            widget.chartType = payload.chartType || 'bar';
            widget.chartData = { labels: payload.labels || [], datasets: payload.datasets || [] };
        } else if (dataType === 'Table') {
            widget.outputType = 'table';
            widget.tableColumns = (payload.columns || []).map(col => ({ field: col, header: col }));
            widget.tableData = payload.rows || [];
        } else if (dataType === 'StatReport') {
            widget.outputType = 'statreport';
            widget.statReportData = payload;
        } else if (dataType === 'Value') {
            widget.outputType = 'value';
            widget.valueData = payload;
        } else if (dataType === 'Markdown') {
            widget.outputType = 'markdown';
            widget.markdownData = payload;
        } else if (dataType === 'Formula') {
            widget.outputType = 'formula';
            widget.formulaData = payload;
        }
    }

    // Variable: Value output updates the displayed scalar
    if (widget.type === 'Variable' && dataType === 'Value') {
        widget.value = payload.value ?? payload;
        if (payload.label && !widget.label) widget.label = payload.label;
        if (payload.unit) widget.unit = payload.unit;
    }
};

const handleWidgetExecutionComplete = () => {
    widgetExecutingId.value = null;
};

onMounted(() => {
    window.addEventListener('socket-output', handleWidgetOutput);
    window.addEventListener('socket-execution-complete', handleWidgetExecutionComplete);
    variableStore.loadDefinitions(proxy.$socket);
    loadDashboardFromRoute();
});

onUnmounted(() => {
    window.removeEventListener('socket-output', handleWidgetOutput);
    window.removeEventListener('socket-execution-complete', handleWidgetExecutionComplete);
    Object.keys(widgetTimers).forEach(id => clearInterval(widgetTimers[id]));
});

// Fetches the current saved content of a CS script by id, so a widget bound
// to a script always runs its latest version rather than a frozen copy.
async function resolveScriptCodeById(scriptId) {
    try {
        const params = { language: 'csharp' };
        if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
        const result = await userStore.executeCommand('LoadScripts', params, proxy.$socket);
        const match = (result?.Data || []).find(s => (s.id || s.Id) === scriptId);
        return match ? (match.code ?? match.Code ?? '') : null;
    } catch {
        return null;
    }
}

async function runWidgetScript(item) {
    let codeToRun = item.scriptCode;
    if (item.scriptId) {
        const fresh = await resolveScriptCodeById(item.scriptId);
        if (fresh !== null) {
            codeToRun = fresh;
            item.scriptCode = fresh;
        }
    }

    if (!codeToRun || !codeToRun.trim()) {
        toast.add({ severity: 'warn', summary: 'No Script', detail: 'Select a script for this widget first (click the script icon)', life: 3000 });
        return;
    }
    widgetExecutingId.value = item.i;
    try {
        await userStore.executeCommand('ExecuteCs', { code: codeToRun, variables: variableStore.getValuesDict() }, proxy.$socket);
    } catch (error) {
        widgetExecutingId.value = null;
        toast.add({ severity: 'error', summary: 'Execution Failed', detail: error.message, life: 3000 });
    }
}

// Sample Chart() calls per chart type, shown as a starting point when a
// chart widget's script is bound for the first time.
const CHART_SCRIPT_EXAMPLES = {
    bar: `Chart("bar", Array("Jan", "Feb", "Mar", "Apr"), Array(12, 19, 7, 15), "Monthly Sales");`,
    line: `Chart("line", Array("Jan", "Feb", "Mar", "Apr"), Array(5, 9, 4, 12), "Trend");`,
    area: `Chart("area", Array("Jan", "Feb", "Mar", "Apr"), Array(3, 8, 5, 10), "Cumulative Usage");`,
    pie: `Chart("pie", Array("Red", "Green", "Blue"), Array(30, 45, 25), "Color Split");`,
    doughnut: `Chart("doughnut", Array("Desktop", "Mobile", "Tablet"), Array(55, 35, 10), "Traffic Source");`,
    scatter: `Chart("scatter", Array("1", "2", "3", "4"), Array(2, 7, 3, 9), "Correlation");`,
    bubble: `Chart("bubble", Array("A", "B", "C"), Array(4, 8, 6), "Bubble Sizes");`,
    radar: `Chart("radar", Array("Speed", "Power", "Range", "Comfort"), Array(70, 85, 60, 90), "Vehicle Profile");`,
    polarArea: `Chart("polarArea", Array("N", "E", "S", "W"), Array(20, 35, 15, 30), "Wind Direction");`,
    mixed: `Chart("bar", Array("Jan", "Feb", "Mar"), Array(10, 20, 15), "Revenue");`
};

// Sample Table() call demonstrating the "id"/"parentId" convention TreeTable's
// script binding expects (see buildTreeFromRows / handleWidgetOutput above).
const TREETABLE_SCRIPT_EXAMPLE = `// Each row needs an "id" and a "parentId" ("" = root node).
var columns = Array("id", "parentId", "name", "size");
var rows = Array(
    DataTableLibrary.MakeRow(columns, Array("1", "", "Engineering", "120")),
    DataTableLibrary.MakeRow(columns, Array("2", "1", "Backend", "45")),
    DataTableLibrary.MakeRow(columns, Array("3", "1", "Frontend", "38")),
    DataTableLibrary.MakeRow(columns, Array("4", "2", "Platform Team", "12"))
);
Table(rows, "Org Chart");`;

function exampleScriptFor(item) {
    if (item.type === 'TreeTable') return TREETABLE_SCRIPT_EXAMPLE;
    if (isChartType(item.type)) return CHART_SCRIPT_EXAMPLES[getChartType(item.type)] || CHART_SCRIPT_EXAMPLES.bar;
    return '';
}

function openWidgetScriptEditor(item) {
    editingScriptWidget.value = item;
    if (!item.scriptCode) {
        const example = exampleScriptFor(item);
        if (example) item.scriptCode = example;
    }
    widgetScriptDialog.value = true;
}

// ── SQL Widget helpers ──────────────────────────────────────────────────────
async function openSqlPickerDialog() {
    try {
        const params = { language: 'sql' };
        if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
        const result = await userStore.executeCommand('LoadScripts', params, proxy.$socket);
        savedSqlScripts.value = (result?.Data || []).map(s => ({
            id: s.id || s.Id,
            name: s.name || s.Name || 'Untitled',
            database: s.databaseconnectionid || s.DatabaseConnectionId || s.database || '',
            code: s.code || s.Code || '',
            visualization: s.visualization || s.Visualization || null
        }));
    } catch { savedSqlScripts.value = []; }
    showSqlPickerDialog.value = true;
}

async function addSqlWidget() {
    if (!selectedSqlScript.value) return;
    const script = selectedSqlScript.value;
    const newComponent = {
        x: 1, y: 0, w: 6, h: 7,
        i: nextComponentId(),
        static: false,
        type: 'SqlWidget',
        title: script.name,
        sqlScriptId: script.id,
        sqlScriptName: script.name,
        databaseId: script.database,
        sqlCode: script.code,
        visualization: script.visualization || '{"type":"table"}',
        queryResults: [],
        queryColumns: [],
        loading: false
    };
    layout.value.componentes.push(newComponent);
    showSqlPickerDialog.value = false;
    selectedSqlScript.value = null;
    await refreshSqlWidget(newComponent);
}

async function refreshSqlWidget(item) {
    if (!item.databaseId || !item.sqlCode) {
        toast.add({ severity: 'warn', summary: 'No Query', detail: 'This widget has no database or SQL code configured.' });
        return;
    }
    item.loading = true;
    try {
        const result = await userStore.executeCommand('ExecuteSql', {
            database: item.databaseId,
            code: variableStore.substituteInSql(item.sqlCode, '')
        }, proxy.$socket);
        if (result?.Data) {
            item.queryResults = result.Data.rows || [];
            item.queryColumns = result.Data.columns || [];
        }
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Query Failed', detail: error.message });
    } finally {
        item.loading = false;
    }
}

function getSqlWidgetViz(item) {
    try { return JSON.parse(item.visualization || '{"type":"table"}'); }
    catch { return { type: 'table' }; }
}

function getSqlWidgetChartData(item) {
    const viz = getSqlWidgetViz(item);
    const rows = item.queryResults || [];
    const columns = item.queryColumns || [];
    if (!rows.length || !columns.length) return null;

    const labelCol = viz.labelColumn || columns[0]?.field || '';
    const valueCols = (viz.valueColumns || []).length > 0
        ? viz.valueColumns
        : columns.filter(c => c.field !== labelCol).slice(0, 4).map(c => c.field);
    if (!valueCols.length) return null;

    const palette = ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272'];
    const vizType = viz.type;

    if (vizType === 'pie') {
        const col = valueCols[0] || labelCol;
        return {
            labels: rows.map(r => String(r[labelCol] ?? '')),
            datasets: [{ label: col, data: rows.map(r => Number(r[col]) || 0), backgroundColor: palette, borderColor: palette, borderWidth: 1 }]
        };
    }

    const labels = rows.map(r => String(r[labelCol] ?? ''));
    const datasets = valueCols.map((col, i) => ({
        label: columns.find(c => c.field === col)?.header || col,
        data: rows.map(r => Number(r[col]) || 0),
        backgroundColor: palette[i % palette.length] + 'bb',
        borderColor: palette[i % palette.length],
        borderWidth: 1,
        fill: vizType === 'area'
    }));
    return { labels, datasets };
}

function getSqlWidgetPivotData(item) {
    const viz = getSqlWidgetViz(item);
    const rows = item.queryResults || [];
    const { pivotRowField, pivotColField, pivotValueField, pivotAggregation = 'sum' } = viz;
    if (!rows.length || !pivotRowField || !pivotColField || !pivotValueField) return null;

    const colValues = [...new Set(rows.map(r => String(r[pivotColField] ?? '')))].sort();
    const rowValues = [...new Set(rows.map(r => String(r[pivotRowField] ?? '')))].sort();

    const agg = (matchingRows) => {
        if (!matchingRows.length) return null;
        const nums = matchingRows.map(r => Number(r[pivotValueField]) || 0);
        switch (pivotAggregation) {
            case 'sum': return nums.reduce((a, b) => a + b, 0);
            case 'avg': return +(nums.reduce((a, b) => a + b, 0) / nums.length).toFixed(2);
            case 'count': return matchingRows.length;
            case 'min': return Math.min(...nums);
            case 'max': return Math.max(...nums);
            default: return nums[0];
        }
    };

    return {
        columns: colValues,
        rows: rowValues.map(rowVal => {
            const values = {};
            for (const colVal of colValues) {
                const matching = rows.filter(r =>
                    String(r[pivotRowField] ?? '') === rowVal &&
                    String(r[pivotColField] ?? '') === colVal
                );
                values[colVal] = agg(matching);
            }
            return { label: rowVal, values };
        })
    };
}

// ── CS Script Widget helpers ───────────────────────────────────────────────
// `existingItem` is set when re-binding an already-placed FunctOutput widget
// to a (possibly different) saved script; left undefined when adding a new
// widget from the sidebar.
async function openCsPickerDialog(mode = 'output', existingItem = null) {
    csPickerMode.value = mode;
    csPickerBindTarget.value = existingItem;
    csPickerRefreshInterval.value = existingItem?.refreshInterval || 0;
    try {
        const params = { language: 'csharp' };
        if (projectStore.currentProjectId) params.projectId = projectStore.currentProjectId;
        const result = await userStore.executeCommand('LoadScripts', params, proxy.$socket);
        savedCsScripts.value = (result?.Data || []).map(s => ({
            id: s.id || s.Id,
            name: s.name || s.Name || 'Untitled',
            code: s.code || s.Code || ''
        }));
    } catch { savedCsScripts.value = []; }
    selectedCsScript.value = existingItem?.scriptId
        ? savedCsScripts.value.find(s => s.id === existingItem.scriptId) || null
        : null;
    showCsPickerDialog.value = true;
}

function confirmCsPickerSelection() {
    if (!selectedCsScript.value) return;
    const script = selectedCsScript.value;

    // Re-binding an existing FunctOutput widget to the selected script.
    if (csPickerBindTarget.value) {
        const item = csPickerBindTarget.value;
        item.scriptId = script.id;
        item.scriptCode = script.code;
        if (!item.title) item.title = script.name;
        item.refreshInterval = csPickerRefreshInterval.value;

        showCsPickerDialog.value = false;
        selectedCsScript.value = null;
        csPickerBindTarget.value = null;

        startAutoRefresh(item);
        runWidgetScript(item);
        return;
    }

    const isVariable = csPickerMode.value === 'variable';

    const newComponent = isVariable
        ? {
            x: 0, y: 0, w: 3, h: 3,
            i: nextComponentId(),
            static: false,
            type: 'Variable',
            label: script.name,
            value: '',
            unit: '',
            description: 'From: ' + script.name,
            scriptId: script.id,
            scriptCode: script.code,
            refreshInterval: csPickerRefreshInterval.value
          }
        : {
            x: 0, y: 0, w: 6, h: 7,
            i: nextComponentId(),
            static: false,
            type: 'FunctOutput',
            title: script.name,
            scriptId: script.id,
            scriptCode: script.code,
            outputType: null,
            chartType: 'bar',
            chartData: null,
            tableColumns: [],
            tableData: [],
            statReportData: null,
            printOutput: '',
            refreshInterval: csPickerRefreshInterval.value
          };

    layout.value.componentes.push(newComponent);
    showCsPickerDialog.value = false;
    selectedCsScript.value = null;
    startAutoRefresh(newComponent);
    runWidgetScript(newComponent);
}

// ── DataTable helpers (used in template) ──────────────────────────────────
function getStatusVariant(status) {
    if (status === 'active') return 'default';
    if (status === 'inactive') return 'destructive';
    return 'secondary';
}

function toggleAllRows(item, val) {
    item.selectedRows = val ? [...item.tableData] : [];
}

function isSelected(item, row) {
    return item.selectedRows?.some(r => r.id === row.id) ?? false;
}

function toggleRowSelection(item, row, val) {
    if (!item.selectedRows) item.selectedRows = [];
    if (val) item.selectedRows.push(row);
    else item.selectedRows = item.selectedRows.filter(r => r.id !== row.id);
}

function toggleTreeNode(item, node) {
    if (item.expandedKeys[node.key]) delete item.expandedKeys[node.key];
    else item.expandedKeys[node.key] = true;
}

function isNodeExpanded(item, node) { return !!item.expandedKeys[node.key]; }

// Flattens item.treeData into a display-ready list of { node, level } pairs,
// recursing into a node's children only while it's present in expandedKeys.
// Table rows are rendered from this flat list instead of iterating treeData
// directly, so expanding a node actually reveals its children.
function getVisibleTreeNodes(item) {
    const result = [];
    function walk(nodes, level) {
        for (const node of nodes) {
            result.push({ node, level });
            if (node.children?.length > 0 && item.expandedKeys?.[node.key]) {
                walk(node.children, level + 1);
            }
        }
    }
    walk(item.treeData || [], 0);
    return result;
}

// Flattens the currently-visible (expanded) tree rows into plain objects for export.
function getTreeExportRows(item) {
    return getVisibleTreeNodes(item).map(entry => entry.node.data);
}

// Rows/columns to hand to ExportMenu for a SqlWidget -- mirrors whichever
// tabular view (table or pivot) is currently configured for the widget.
function getSqlWidgetExportData(item) {
    const viz = getSqlWidgetViz(item);
    if (viz.type === 'pivot') {
        const pivot = getSqlWidgetPivotData(item);
        if (!pivot) return { rows: [], columns: [] };
        const columns = [
            { field: '__row', header: viz.pivotRowField || 'Row' },
            ...pivot.columns.map(c => ({ field: c, header: c }))
        ];
        const rows = pivot.rows.map(row => ({ __row: row.label, ...row.values }));
        return { rows, columns };
    }
    return { rows: item.queryResults || [], columns: item.queryColumns || [] };
}
</script>

<template>
    <div class="flex items-center justify-between p-2 border-b mb-3">
        <div class="flex items-center gap-2">
            <Button variant="ghost" size="sm" class="gap-1" @click="saveToServer" title="Save to server">
                <Save class="w-4 h-4" /><span class="hidden sm:inline">Save</span>
            </Button>
            <Button variant="ghost" size="sm" class="gap-1" @click="openLoadDialog" title="Load from server">
                <FolderOpen class="w-4 h-4" /><span class="hidden sm:inline">Load</span>
            </Button>
        </div>

        <div class="flex items-center justify-center">
            <div v-if="!editingTitle" class="cursor-pointer hover:bg-muted p-2 rounded-md transition-colors" @click="editingTitle = true">
                <h2 class="text-lg font-semibold">{{ MyTitle || 'No Name' }}</h2>
            </div>
            <div v-else class="flex items-center gap-2">
                <Input v-model="MyTitle" ref="titleInputRef" @keyup.enter="editingTitle = false" @blur="editingTitle = false" class="w-48" />
                <Button variant="ghost" size="icon" class="text-destructive h-8 w-8" @click.stop="editingTitle = false">
                    <X class="w-4 h-4" />
                </Button>
            </div>
        </div>

        <div class="flex items-center gap-2">
            <Button variant="outline" size="sm" class="gap-2" @click="openShareDialog">
                <Share2 class="w-4 h-4" />
                Share
            </Button>
        </div>
    </div>

    <!-- Elements Toolbar: add dashboard widgets, organized by category -->
    <div class="flex items-center gap-1.5 mb-4 bg-muted/30 p-2 border rounded-md flex-wrap">
        <span class="text-xs font-medium text-muted-foreground px-1">Add:</span>
        <DropdownMenu v-for="menuGroup in componentMenuItems" :key="menuGroup.label">
            <DropdownMenuTrigger as-child>
                <Button variant="outline" size="sm" class="gap-1.5 h-8">
                    <component :is="menuGroup.icon" class="w-3.5 h-3.5" />
                    {{ menuGroup.label }}
                    <ChevronDown class="w-3 h-3 opacity-60" />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" class="w-72">
                <DropdownMenuLabel>{{ menuGroup.label }}</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <template v-for="item in menuGroup.items" :key="item.label">
                    <DropdownMenuSub v-if="item.items">
                        <DropdownMenuSubTrigger class="gap-2">
                            <component :is="item.icon" class="w-4 h-4"></component>
                            <span>{{ item.label }}</span>
                        </DropdownMenuSubTrigger>
                        <DropdownMenuSubContent>
                            <DropdownMenuItem v-for="subItem in item.items" :key="subItem.label" class="gap-2" @click="subItem.command && subItem.command()">
                                <component :is="subItem.icon" class="w-4 h-4"></component>
                                <span>{{ subItem.label }}</span>
                                <Badge v-if="subItem.badge" class="ml-auto text-[10px]" variant="secondary">{{ subItem.badge }}</Badge>
                            </DropdownMenuItem>
                        </DropdownMenuSubContent>
                    </DropdownMenuSub>
                    <DropdownMenuItem v-else class="gap-2" @click="item.command && item.command()">
                        <component :is="item.icon" class="w-4 h-4"></component>
                        <span>{{ item.label }}</span>
                        <Badge v-if="item.badge" class="ml-auto text-[10px]" variant="secondary">{{ item.badge }}</Badge>
                    </DropdownMenuItem>
                </template>
            </DropdownMenuContent>
        </DropdownMenu>
    </div>
    <grid-layout v-model:layout="layout.componentes" :col-num="15" :row-height="40" :auto-size="true" is-draggable is-resizable vertical-compact use-css-transforms v-if="renderComponent">
        <grid-item v-for="item in layout.componentes" :static="item.static" :x="item.x" :y="item.y" :w="item.w" :h="item.h" :i="item.i" :key="item.i" class="grid-item-container">
            <!-- Text Component -->
            <div v-if="item.type === 'Text'" class="relative group w-full h-full flex items-center justify-center">
                <Button variant="ghost" size="icon" class="absolute top-1 right-1 h-6 w-6 text-destructive hover:bg-destructive/10 opacity-0 group-hover:opacity-100 transition-opacity z-10" @click="removeComponent(item.i)" title="Remove">
                    <Trash2 class="w-3 h-3 text-xs" />
                </Button>
                <div
                    v-if="!item.editing"
                    class="cursor-pointer p-2 w-full text-center hover:bg-muted/50 rounded"
                    @click="
                        item.editing = true;
                        nextTick(() => $refs['textInput' + item.i]?.[0]?.$el?.focus());
                    "
                >
                    <span class="text-lg">{{ item.value || 'Click to Edit' }}</span>
                </div>
                <div v-else class="flex items-center gap-2 w-full px-2">
                    <Input v-model="item.value" :ref="'textInput' + item.i" @keyup.enter="item.editing = false" class="w-full" />
                    <Button variant="ghost" size="icon" class="text-destructive shrink-0" @click="item.editing = false">
                        <X class="w-4 h-4" />
                    </Button>
                </div>
            </div>

            <!-- Chart Components -->
            <div v-else-if="isChartType(item.type)" class="chart-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="chart-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || getDefaultChartTitle(item.type) }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="chart-controls flex gap-1">
                        <Button v-if="isBokehSupported(item.type)" variant="ghost" size="icon" class="h-7 w-7" :class="item.renderEngine === 'bokeh' ? 'text-primary' : ''" @click="item.renderEngine = item.renderEngine === 'bokeh' ? 'echarts' : 'bokeh'" :title="item.renderEngine === 'bokeh' ? 'Using Bokeh (click for ECharts)' : 'Using ECharts (click for Bokeh)'">
                            <Waves class="w-3 h-3 text-xs" />
                        </Button>
                        <Button v-if="item.statReportData" variant="ghost" size="icon" class="h-7 w-7 text-violet-500" @click="viewWidgetStatReport(item)" title="View Stat Report">
                            <FileText class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="runWidgetScript(item)" :title="item.scriptCode ? 'Run Script' : 'No script bound'" :class="widgetExecutingId === item.i ? 'animate-spin text-primary' : ''">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="openWidgetScriptEditor(item)" title="Bind Script">
                            <Code class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <BokehChart
                    v-if="item.renderEngine === 'bokeh' && isBokehSupported(item.type)"
                    :bokeh-json="buildBokehJson({ type: getChartType(item.type), labels: item.chartData?.labels, datasets: item.chartData?.datasets, title: item.title })"
                    :height="getChartHeight(item.h)"
                    :show-header="false"
                    :show-footer="false"
                />
                <BaseChart
                    v-else
                    :type="getChartType(item.type)"
                    :data="item.chartData"
                    :height="getChartHeight(item.h)"
                    :show-header="false"
                    :show-footer="false"
                    :show-controls="false"
                    :show-legend="item.showLegend !== false"
                    :show-data-labels="item.showDataLabels !== false"
                    @chart-clicked="onChartClick"
                />
            </div>

            <!-- DataTable Component -->
            <div v-else-if="item.type === 'DataTable'" class="datatable-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="datatable-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Data Table' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="datatable-controls flex gap-1 items-center">
                        <Input v-model="item.globalFilter" placeholder="Search..." class="h-7 w-32 text-xs" />
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-green-600 hover:text-green-700" @click="addDataTableRow(item)" title="Add Row">
                            <Plus class="w-3 h-3 text-xs" />
                        </Button>
                        <ExportMenu :rows="item.tableData" :columns="item.columns" :filename="item.title || 'datatable'" icon-only variant="ghost" size="icon-sm" />
                        <Button v-if="item.statReportData" variant="ghost" size="icon" class="h-7 w-7 text-violet-500" @click="viewWidgetStatReport(item)" title="View Stat Report">
                            <FileText class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="runWidgetScript(item)" :title="item.scriptCode ? 'Run Script' : 'No script bound'">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="openWidgetScriptEditor(item)" title="Bind Script">
                            <Code class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="overflow-auto border rounded-md flex-1 custom-scroll" :style="{ maxHeight: getDataTableHeight(item.h) }">
                    <Table>
                        <TableHeader class="sticky top-0 bg-secondary/80 backdrop-blur-sm z-10">
                            <TableRow>
                                <TableHead class="w-[50px]">
                                    <Checkbox @update:checked="(val) => toggleAllRows(item, val)" />
                                </TableHead>
                                <TableHead v-for="col in item.columns" :key="col.field" class="cursor-pointer hover:bg-muted/50 transition-colors select-none font-semibold">
                                    <div class="flex items-center gap-2">
                                        {{ col.header }}
                                        <ArrowUpDown class="w-3 h-3 text-[10px] text-muted-foreground" />
                                    </div>
                                </TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <!-- Temporary simplified render, ideally needs a computed property for filtered/paginated data -->
                            <TableRow v-for="(row, index) in item.tableData" :key="row.id || index" class="hover:bg-muted/30 transition-colors">
                                <TableCell>
                                    <Checkbox :checked="isSelected(item, row)" @update:checked="(val) => toggleRowSelection(item, row, val)" />
                                </TableCell>
                                <TableCell v-for="col in item.columns" :key="col.field">
                                    <template v-if="col.field === 'status'">
                                        <Badge :variant="getStatusVariant(row[col.field])">{{ row[col.field] }}</Badge>
                                    </template>
                                    <template v-else>
                                        {{ row[col.field] }}
                                    </template>
                                </TableCell>
                            </TableRow>
                            <TableRow v-if="!item.tableData || item.tableData.length === 0">
                                <TableCell :colspan="item.columns.length + 1" class="text-center py-4 text-muted-foreground">No data available</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </div>
                <!-- Pagination Placeholder -->
                <div class="flex items-center justify-between px-2 py-1 mt-2 border-t text-xs text-muted-foreground">
                    <div>Showing {{ item.tableData?.length || 0 }} entries</div>
                    <div class="flex gap-1">
                        <Button variant="outline" size="icon" class="h-6 w-6"><ChevronLeft class="w-3 h-3 text-[10px]" /></Button>
                        <Button variant="outline" size="icon" class="h-6 w-6"><ChevronRight class="w-3 h-3 text-[10px]" /></Button>
                    </div>
                </div>
            </div>

            <!-- TreeTable Component -->
            <div v-else-if="item.type === 'TreeTable'" class="treetable-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="treetable-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Tree Table' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="treetable-controls flex gap-1">
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-green-600 hover:text-green-700" @click="expandAllTreeNodes(item)" title="Expand All">
                            <Plus class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="collapseAllTreeNodes(item)" title="Collapse All">
                            <Minus class="w-3 h-3 text-xs" />
                        </Button>
                        <ExportMenu :rows="getTreeExportRows(item)" :columns="item.columns" :filename="item.title || 'treetable'" icon-only variant="ghost" size="icon-sm" />
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="runWidgetScript(item)" :title="item.scriptCode ? 'Run Script' : 'No script bound'" :class="widgetExecutingId === item.i ? 'animate-spin text-primary' : ''">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="openWidgetScriptEditor(item)" title="Bind Script">
                            <Code class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="overflow-auto border rounded-md flex-1 custom-scroll" :style="{ maxHeight: getTreeTableHeight(item.h) }">
                    <Table>
                        <TableHeader class="sticky top-0 bg-secondary/80 backdrop-blur-sm z-10">
                            <TableRow>
                                <TableHead v-for="col in item.columns" :key="col.field" class="font-semibold text-xs">
                                    {{ col.header }}
                                </TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow v-for="entry in getVisibleTreeNodes(item)" :key="entry.node.key" class="hover:bg-muted/30 transition-colors">
                                <TableCell v-for="col in item.columns" :key="col.field">
                                    <div class="flex items-center gap-2" :style="{ paddingLeft: col.expander ? `${entry.level * 1.5}rem` : '0' }">
                                        <Button v-if="col.expander && entry.node.children && entry.node.children.length > 0" variant="ghost" size="icon" class="h-5 w-5 p-0" @click="toggleTreeNode(item, entry.node)">
                                            <component :is="isNodeExpanded(item, entry.node) ? ChevronDown : ChevronRight" class="w-3 h-3 text-[10px]" />
                                        </Button>
                                        <span v-else-if="col.expander" class="w-5 inline-block"></span>
                                        <span>{{ entry.node.data[col.field] }}</span>
                                    </div>
                                </TableCell>
                            </TableRow>
                            <TableRow v-if="!item.treeData || item.treeData.length === 0">
                                <TableCell :colspan="item.columns.length" class="text-center py-4 text-muted-foreground">No data available</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </div>
            </div>

            <!-- Image Component -->
            <div v-else-if="item.type === 'Image'" class="image-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="image-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Image' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="image-controls flex gap-1">
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="selectImageFile(item)" title="Select Image">
                            <Upload class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="item.preview = !item.preview" :title="item.preview ? 'Disable Preview' : 'Enable Preview'">
                            <ZoomIn class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="refreshImage(item)" title="Refresh Sample Image">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="image-content flex-1 flex items-center justify-center overflow-hidden bg-muted/20 rounded">
                    <img :src="item.src" :alt="item.alt" class="object-contain max-h-full max-w-full" :class="{ 'cursor-zoom-in': item.preview, 'w-full h-full object-cover': !item.preview }" />
                </div>
            </div>

            <!-- Select Component -->
            <div v-else-if="item.type === 'Select'" class="select-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="select-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Select Dropdown' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="select-controls flex gap-1">
                        <Button variant="ghost" size="icon" class="h-7 w-7" title="Configure Options" @click="openSelectConfig(item)">
                            <Settings class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" :class="item.boundVariable ? 'text-primary' : ''" @click="openBindDialog(item)" title="Bind to Variable">
                            <Braces class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="select-content flex-1 flex flex-col gap-2 p-2">
                    <div v-if="item.boundVariable" class="flex items-center gap-1 mb-1">
                        <span class="text-xs text-muted-foreground">Bound to:</span>
                        <Badge variant="outline" class="text-xs py-0 h-5">{{ item.boundVariable }}</Badge>
                    </div>
                    <select
                        class="flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        :value="item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue || '') : (item.selectedValue || '')"
                        @change="(e) => { if (item.boundVariable) variableStore.setValue(item.boundVariable, e.target.value); else { item.selectedValue = e.target.value; onSelectChange(item); } }"
                    >
                        <option value="" disabled>{{ item.placeholder || 'Select an option…' }}</option>
                        <template v-if="item.boundVariable && varOptions[item.boundVariable]">
                            <option v-for="opt in varOptions[item.boundVariable]" :key="opt" :value="opt">{{ opt }}</option>
                        </template>
                        <template v-else>
                            <option v-for="opt in item.options" :key="opt" :value="opt">{{ opt }}</option>
                        </template>
                    </select>
                    <div class="select-value text-sm text-muted-foreground mt-2"
                         v-if="item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue) : item.selectedValue">
                        Selected: {{ item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue || '') : item.selectedValue }}
                    </div>
                </div>
            </div>

            <!-- InputText Component -->
            <div v-else-if="item.type === 'InputText'" class="input-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="input-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Input Text' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="input-controls flex gap-1">
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="clearInputText(item)" title="Clear Text">
                            <Eraser class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" :class="item.boundVariable ? 'text-primary' : ''" @click="openBindDialog(item)" title="Bind to Variable">
                            <Braces class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="input-content flex-1 flex flex-col gap-2 justify-center p-2">
                    <div v-if="item.boundVariable" class="flex items-center gap-1 mb-1">
                        <span class="text-xs text-muted-foreground">Bound to:</span>
                        <Badge variant="outline" class="text-xs py-0 h-5">{{ item.boundVariable }}</Badge>
                    </div>
                    <input
                        type="text"
                        class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                        :value="item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue || '') : item.value"
                        :placeholder="item.placeholder"
                        @input="e => { if (item.boundVariable) variableStore.setValue(item.boundVariable, e.target.value); else item.value = e.target.value; }"
                    />
                    <div class="input-info text-xs text-muted-foreground mt-1"
                         v-if="item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue) : item.value">
                        Length: {{ (item.boundVariable ? (variableStore.values[item.boundVariable] || variableStore.definitions.find(d => d.name === item.boundVariable)?.defaultValue || '') : item.value).length }} characters
                    </div>
                </div>
            </div>

            <!-- Excel Editor Component -->
            <!-- SqlWidget: execute a saved SQL query and display with configured visualization -->
            <div v-else-if="item.type === 'SqlWidget'" class="sql-widget-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="flex justify-between items-center mb-2 min-h-[2rem]">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm truncate max-w-[60%]" @click="item.editing = true">
                        {{ item.title || item.sqlScriptName || 'SQL Query' }}
                    </div>
                    <div v-else class="flex items-center gap-1 flex-1 mr-2">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive shrink-0" @click="item.editing = false"><X class="w-3 h-3" /></Button>
                    </div>
                    <div class="sql-widget-controls flex gap-1 shrink-0">
                        <span v-if="item.loading" class="text-xs text-muted-foreground mr-1 self-center">Running…</span>
                        <ExportMenu
                            v-if="['table', 'pivot'].includes(getSqlWidgetViz(item).type)"
                            :rows="getSqlWidgetExportData(item).rows"
                            :columns="getSqlWidgetExportData(item).columns"
                            :filename="item.title || item.sqlScriptName || 'sql-widget'"
                            icon-only variant="ghost" size="icon-sm"
                        />
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="refreshSqlWidget(item)" title="Run Query">
                            <RefreshCw class="w-3 h-3" :class="item.loading ? 'animate-spin' : ''" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3" />
                        </Button>
                    </div>
                </div>

                <div class="flex-1 overflow-auto min-h-0">
                    <!-- Table view -->
                    <div v-if="getSqlWidgetViz(item).type === 'table'" class="h-full overflow-auto border rounded-md bg-background">
                        <table v-if="item.queryResults?.length" class="text-xs w-full border-collapse">
                            <thead class="sticky top-0 bg-secondary z-10">
                                <tr>
                                    <th v-for="col in item.queryColumns" :key="col.field" class="border border-border px-2 py-1 text-left font-semibold whitespace-nowrap">{{ col.header }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, ri) in item.queryResults" :key="ri" class="hover:bg-muted/40 transition-colors">
                                    <td v-for="col in item.queryColumns" :key="col.field" class="border border-border px-2 py-1">{{ row[col.field] ?? 'NULL' }}</td>
                                </tr>
                            </tbody>
                        </table>
                        <div v-else class="flex items-center justify-center h-full text-muted-foreground text-xs p-4">
                            Click the refresh button to run the query
                        </div>
                    </div>

                    <!-- Pivot view -->
                    <div v-else-if="getSqlWidgetViz(item).type === 'pivot'" class="h-full overflow-auto border rounded-md bg-background">
                        <table v-if="getSqlWidgetPivotData(item)" class="text-xs w-full border-collapse">
                            <thead class="sticky top-0 bg-secondary z-10">
                                <tr>
                                    <th class="border border-border px-2 py-1 text-left font-semibold">Row</th>
                                    <th v-for="col in getSqlWidgetPivotData(item).columns" :key="col" class="border border-border px-2 py-1 text-center font-semibold">{{ col }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="row in getSqlWidgetPivotData(item).rows" :key="row.label" class="hover:bg-muted/40 transition-colors">
                                    <td class="border border-border px-2 py-1 font-medium">{{ row.label }}</td>
                                    <td v-for="col in getSqlWidgetPivotData(item).columns" :key="col" class="border border-border px-2 py-1 text-right">{{ row.values[col] !== null ? row.values[col] : '—' }}</td>
                                </tr>
                            </tbody>
                        </table>
                        <div v-else class="flex items-center justify-center h-full text-muted-foreground text-xs p-4">
                            Configure pivot fields in the SQL Editor script first
                        </div>
                    </div>

                    <!-- Chart view (line, bar, bar-h, area, pie, scatter, waterfall) -->
                    <div v-else class="h-full flex items-center justify-center">
                        <BokehChart
                            v-if="getSqlWidgetChartData(item) && getSqlWidgetViz(item).engine === 'bokeh' && isBokehSupported(getSqlWidgetViz(item).type)"
                            :bokeh-json="buildBokehJson({ type: getSqlWidgetViz(item).type || 'bar', labels: getSqlWidgetChartData(item).labels, datasets: getSqlWidgetChartData(item).datasets })"
                            :height="getChartHeight(item.h - 1)"
                            :show-header="false"
                            :show-footer="false"
                            class="w-full"
                        />
                        <BaseChart
                            v-else-if="getSqlWidgetChartData(item)"
                            :type="getSqlWidgetViz(item).type || 'bar'"
                            :data="getSqlWidgetChartData(item)"
                            :height="getChartHeight(item.h - 1)"
                            :show-header="false"
                            :show-footer="false"
                            :show-controls="false"
                            :show-legend="true"
                            class="w-full"
                        />
                        <div v-else class="text-muted-foreground text-xs text-center p-4">
                            <BarChart2Icon class="w-6 h-6 mx-auto mb-1 opacity-40" />
                            Click refresh to load data
                        </div>
                    </div>
                </div>
            </div>

            <!-- Variable / KPI Widget -->
            <div v-else-if="item.type === 'Variable'" class="variable-widget flex flex-col h-full border rounded-md p-3 bg-card">
                <div class="flex justify-between items-start">
                    <div
                        v-if="!item.editingLabel"
                        class="text-xs font-semibold uppercase tracking-wider text-muted-foreground cursor-pointer hover:text-foreground transition-colors"
                        @click="item.editingLabel = true"
                    >
                        {{ item.label || 'Click to set label' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.label" class="h-6 text-xs w-28" autofocus @blur="item.editingLabel = false" @keyup.enter="item.editingLabel = false" />
                    </div>
                    <div class="variable-controls flex gap-0.5">
                        <Button variant="ghost" size="icon" class="h-6 w-6" @click="runWidgetScript(item)" :title="item.scriptCode ? 'Run Script' : 'No script bound'" :class="widgetExecutingId === item.i ? 'animate-spin text-primary' : ''">
                            <RefreshCw class="w-3 h-3" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-6 w-6" @click="openWidgetScriptEditor(item)" title="Bind Script">
                            <Code class="w-3 h-3" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3" />
                        </Button>
                    </div>
                </div>
                <div class="flex-1 flex flex-col items-center justify-center gap-1">
                    <div v-if="!item.editingValue" class="cursor-pointer tabular-nums font-bold leading-none" :class="item.h >= 4 ? 'text-4xl' : 'text-2xl'" @click="item.editingValue = true">
                        {{ item.value !== undefined && item.value !== null && item.value !== '' ? item.value : '—' }}
                        <span v-if="item.unit" class="text-base font-normal text-muted-foreground ml-1">{{ item.unit }}</span>
                    </div>
                    <div v-else class="flex items-center gap-2">
                        <Input v-model="item.value" class="h-9 text-xl w-36 text-center font-bold tabular-nums" autofocus @blur="item.editingValue = false" @keyup.enter="item.editingValue = false" />
                        <Input v-model="item.unit" class="h-9 text-sm w-16 text-center" placeholder="unit" @blur="item.editingValue = false" />
                    </div>
                    <p v-if="item.description" class="text-xs text-muted-foreground text-center">{{ item.description }}</p>
                </div>
            </div>

            <!-- FunctEngine Output Widget -->
            <div v-else-if="item.type === 'FunctOutput'" class="funct-output-widget flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="flex justify-between items-center mb-2 min-h-[2rem]">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Script Output' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false"><X class="w-3 h-3" /></Button>
                    </div>
                    <div class="funct-controls flex gap-1">
                        <Button v-if="item.outputType === 'chart' && isBokehSupported(item.chartType)" variant="ghost" size="icon" class="h-7 w-7" :class="item.renderEngine === 'bokeh' ? 'text-primary' : ''" @click="item.renderEngine = item.renderEngine === 'bokeh' ? 'echarts' : 'bokeh'" :title="item.renderEngine === 'bokeh' ? 'Using Bokeh (click for ECharts)' : 'Using ECharts (click for Bokeh)'">
                            <Waves class="w-3 h-3" />
                        </Button>
                        <Button v-if="item.outputType === 'statreport'" variant="ghost" size="icon" class="h-7 w-7 text-violet-500" @click="viewWidgetStatReport(item)" title="View Full Report">
                            <FileText class="w-3 h-3" />
                        </Button>
                        <ExportMenu v-if="item.outputType === 'table'" :rows="item.tableData" :columns="item.tableColumns" :filename="item.title || 'script-output'" icon-only variant="ghost" size="icon-sm" />
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="runWidgetScript(item)" :title="item.scriptId ? 'Run Script' : 'No script — select one first'" :class="widgetExecutingId === item.i ? 'animate-spin text-primary' : ''">
                            <RefreshCw class="w-3 h-3" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="openCsPickerDialog('output', item)" :title="item.scriptId ? 'Change Script' : 'Select Script'">
                            <Code class="w-3 h-3" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3" />
                        </Button>
                    </div>
                </div>

                <div class="flex-1 overflow-auto min-h-0">
                    <!-- Empty state -->
                    <div v-if="!item.outputType" class="flex flex-col items-center justify-center h-full text-muted-foreground gap-2 p-4">
                        <Terminal class="w-8 h-8 opacity-30" />
                        <p class="text-xs text-center">Bind a FunctEngine script and click Run.<br/>Supports <code class="bg-muted px-1 rounded">Table()</code>, <code class="bg-muted px-1 rounded">Chart()</code>, <code class="bg-muted px-1 rounded">StatReport()</code>, <code class="bg-muted px-1 rounded">Value()</code>, <code class="bg-muted px-1 rounded">Markdown()</code>, <code class="bg-muted px-1 rounded">Formula()</code></p>
                    </div>

                    <!-- Chart output -->
                    <div v-else-if="item.outputType === 'chart'" class="h-full">
                        <BokehChart
                            v-if="item.chartData && item.renderEngine === 'bokeh' && isBokehSupported(item.chartType)"
                            :bokeh-json="buildBokehJson({ type: item.chartType || 'bar', labels: item.chartData.labels, datasets: item.chartData.datasets })"
                            :height="getChartHeight(item.h - 1)"
                            :show-header="false"
                            :show-footer="false"
                            class="w-full"
                        />
                        <BaseChart
                            v-else-if="item.chartData"
                            :type="item.chartType || 'bar'"
                            :data="item.chartData"
                            :height="getChartHeight(item.h - 1)"
                            :show-header="false"
                            :show-footer="false"
                            :show-controls="false"
                            :show-legend="true"
                            class="w-full"
                        />
                    </div>

                    <!-- Table output -->
                    <div v-else-if="item.outputType === 'table'" class="h-full overflow-auto border rounded-md bg-background">
                        <table v-if="item.tableData?.length" class="text-xs w-full border-collapse">
                            <thead class="sticky top-0 bg-secondary z-10">
                                <tr>
                                    <th v-for="col in item.tableColumns" :key="col.field" class="border border-border px-2 py-1 text-left font-semibold">{{ col.header }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, ri) in item.tableData" :key="ri" class="hover:bg-muted/40 transition-colors">
                                    <td v-for="col in item.tableColumns" :key="col.field" class="border border-border px-2 py-1">{{ row[col.field] }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- StatReport output (compact preview) -->
                    <div v-else-if="item.outputType === 'statreport' && item.statReportData" class="text-xs space-y-2 p-1 overflow-auto h-full">
                        <h4 class="font-semibold text-sm border-b pb-1">{{ item.statReportData.title }}</h4>
                        <div v-for="(section, si) in (item.statReportData.sections || []).slice(0, 5)" :key="si" class="space-y-1">
                            <p v-if="section.heading" class="font-medium text-muted-foreground">{{ section.heading }}</p>
                            <div v-if="section.type === 'table'" class="overflow-x-auto">
                                <table class="border-collapse w-full">
                                    <thead><tr><th v-for="h in section.headers" :key="h" class="px-1 py-0.5 bg-muted border text-left">{{ h }}</th></tr></thead>
                                    <tbody><tr v-for="(row, ri) in section.rows" :key="ri" class="border-t"><td v-for="(cell, ci) in row" :key="ci" class="px-1 py-0.5 border">{{ typeof cell === 'number' ? cell.toFixed(3) : cell }}</td></tr></tbody>
                                </table>
                            </div>
                            <p v-else class="text-muted-foreground whitespace-pre-wrap">{{ section.content || section.text }}</p>
                        </div>
                        <p v-if="(item.statReportData.sections || []).length > 5" class="text-muted-foreground italic text-center mt-1">
                            ⋯ {{ item.statReportData.sections.length - 5 }} more sections — click the report icon to view all
                        </p>
                    </div>

                    <!-- Value output -->
                    <div v-else-if="item.outputType === 'value' && item.valueData" class="h-full flex flex-col items-center justify-center gap-1">
                        <div class="tabular-nums font-bold leading-none" :class="item.h >= 4 ? 'text-4xl' : 'text-2xl'">
                            {{ item.valueData.value }}
                            <span v-if="item.valueData.unit" class="text-base font-normal text-muted-foreground ml-1">{{ item.valueData.unit }}</span>
                        </div>
                        <div v-if="item.valueData.label" class="text-xs text-muted-foreground uppercase tracking-wider">{{ item.valueData.label }}</div>
                    </div>

                    <!-- Markdown output -->
                    <div v-else-if="item.outputType === 'markdown' && item.markdownData" class="h-full overflow-auto p-2">
                        <div v-if="item.markdownData.title" class="font-semibold text-sm mb-2 pb-2 border-b">{{ item.markdownData.title }}</div>
                        <MarkdownReport :content="item.markdownData.content" />
                    </div>

                    <!-- Formula output -->
                    <div v-else-if="item.outputType === 'formula' && item.formulaData" class="h-full flex items-center justify-center p-2 overflow-auto">
                        <FormulaBlock :latex="item.formulaData.latex" :label="item.formulaData.label" />
                    </div>
                </div>
            </div>
        </grid-item>
    </grid-layout>

    <!-- Chart Settings Dialog -->
    <Dialog :open="settingsDialogVisible" @update:open="settingsDialogVisible = $event">
        <DialogContent class="sm:max-w-[425px]">
            <DialogHeader>
                <DialogTitle>Chart Settings</DialogTitle>
                <DialogDescription> Configure display settings for this chart component. </DialogDescription>
            </DialogHeader>

            <div class="grid gap-4 py-4" v-if="selectedChartItem">
                <div class="flex items-center space-x-2">
                    <Switch id="legend" :checked="selectedChartItem.showLegend" @update:checked="(val) => (selectedChartItem.showLegend = val)" />
                    <Label for="legend">Show Legend</Label>
                </div>
                <div class="flex items-center space-x-2">
                    <Switch id="labels" :checked="selectedChartItem.showDataLabels" @update:checked="(val) => (selectedChartItem.showDataLabels = val)" />
                    <Label for="labels">Show Data Labels</Label>
                </div>
            </div>

            <DialogFooter>
                <Button @click="closeSettingsDialog">Done</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- Load Dialog -->
    <Dialog v-model:open="showLoadDialog">
        <DialogContent class="sm:max-w-[425px]">
            <DialogHeader>
                <DialogTitle>Load Dashboard from Server</DialogTitle>
            </DialogHeader>
            <div class="flex flex-col gap-4 py-4 max-h-[300px] overflow-y-auto">
                <div v-if="dashboardStore.dashboards.length === 0" class="text-center text-muted-foreground p-4">
                    No dashboards saved on the server.
                </div>
                <div v-for="dash in dashboardStore.dashboards" :key="dash.id" class="flex justify-between items-center p-3 border rounded hover:bg-muted cursor-pointer" @click="loadFromServer(dash)">
                    <div>
                        <div class="font-medium">{{ dash.name || dash.title || 'Untitled' }}</div>
                        <div class="text-xs text-muted-foreground">{{ new Date(dash.createdat || dash.CreatedAt || dash.timestamp || Date.now()).toLocaleDateString() }}</div>
                    </div>
                    <Button variant="ghost" size="sm" @click.stop="loadFromServer(dash)">Load</Button>
                </div>
            </div>
        </DialogContent>
    </Dialog>

    <!-- Share Dialog -->
    <Dialog v-model:open="showShareDialog">
        <DialogContent class="max-w-md">
            <DialogHeader>
                <DialogTitle class="flex items-center gap-2"><Share2 class="w-4 h-4" /> Share Dashboard</DialogTitle>
            </DialogHeader>
            <div class="py-4 space-y-4">
                <div v-if="currentShareToken" class="space-y-2">
                    <label class="text-sm font-medium">Public Link</label>
                    <div class="flex gap-2">
                        <Input :model-value="shareUrl" readonly class="font-mono text-xs" />
                        <Button variant="outline" size="icon" @click="copyShareUrl" title="Copy"><Copy class="w-4 h-4" /></Button>
                    </div>
                    <p class="text-xs text-muted-foreground">Anyone with this link can view without logging in.</p>
                </div>
                <div v-else class="text-center py-4">
                    <Lock class="w-8 h-8 mx-auto mb-2 text-muted-foreground" />
                    <p class="text-sm text-muted-foreground">This dashboard is private. Generate a link to share it publicly.</p>
                </div>
            </div>
            <DialogFooter>
                <Button v-if="!currentShareToken" @click="enableSharing" class="gap-2 w-full">
                    <Globe class="w-4 h-4" /> Generate Public Link
                </Button>
                <div v-else class="flex gap-2 w-full">
                    <Button variant="destructive" @click="disableSharing" class="gap-2 flex-1"><Lock class="w-4 h-4" /> Revoke</Button>
                    <Button variant="outline" @click="showShareDialog = false" class="flex-1">Done</Button>
                </div>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- Widget Script Editor Dialog -->
    <Dialog :open="widgetScriptDialog" @update:open="widgetScriptDialog = $event">
        <DialogContent class="sm:max-w-[700px]">
            <DialogHeader>
                <DialogTitle>Bind Script to Widget: {{ editingScriptWidget?.title || editingScriptWidget?.type }}</DialogTitle>
            </DialogHeader>
            <div class="py-2 text-sm text-muted-foreground mb-2">
                <template v-if="editingScriptWidget?.type === 'TreeTable'">
                    Write a script that calls <code class="bg-muted px-1 rounded">Table(rows, "title")</code> with rows containing an <code class="bg-muted px-1 rounded">id</code> and <code class="bg-muted px-1 rounded">parentId</code> column — a sample is pre-filled below.
                </template>
                <template v-else>
                    Write a script using <code class="bg-muted px-1 rounded">Table(data, "title")</code>, <code class="bg-muted px-1 rounded">Chart("bar", labels, values, "title")</code>, or <code class="bg-muted px-1 rounded">StatReport("title", sections)</code>.
                </template>
            </div>
            <div v-if="editingScriptWidget">
                <Textarea
                    v-model="editingScriptWidget.scriptCode"
                    placeholder="// Example:&#10;var data = ExecuteQuery(&quot;mydb&quot;, &quot;SELECT name, value FROM my_table&quot;);&#10;Chart(&quot;bar&quot;, Array(&quot;A&quot;,&quot;B&quot;,&quot;C&quot;), Array(10,20,30), &quot;My Chart&quot;);"
                    class="font-mono text-sm h-64 resize-none"
                />
                <div class="flex items-center gap-2 mt-3">
                    <Clock class="w-4 h-4 text-muted-foreground" />
                    <span class="text-sm text-muted-foreground">Auto-refresh every</span>
                    <Input
                        type="number"
                        :model-value="editingScriptWidget.refreshInterval || 0"
                        @update:model-value="val => editingScriptWidget.refreshInterval = parseInt(val) || 0"
                        class="h-7 w-20 text-sm"
                        min="0"
                    />
                    <span class="text-sm text-muted-foreground">minutes (0 = off)</span>
                </div>
            </div>
            <DialogFooter class="gap-2">
                <Button variant="outline" @click="widgetScriptDialog = false">Cancel</Button>
                <Button @click="applyWidgetScript">
                    Save &amp; Run
                </Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- SQL Script Picker Dialog -->
    <Dialog v-model:open="showSqlPickerDialog">
        <DialogContent class="sm:max-w-[600px]">
            <DialogHeader>
                <DialogTitle>Add SQL Visualization Widget</DialogTitle>
                <DialogDescription>Select a saved SQL script. Its visualization settings (chart type, columns) will be used in the widget.</DialogDescription>
            </DialogHeader>
            <div class="py-2 max-h-[400px] overflow-auto border rounded-md">
                <div v-if="savedSqlScripts.length === 0" class="text-center text-muted-foreground p-6">
                    No SQL scripts found for the current project.
                </div>
                <div
                    v-for="script in savedSqlScripts" :key="script.id"
                    class="flex items-center justify-between px-3 py-2.5 border-b last:border-b-0 cursor-pointer hover:bg-muted transition-colors"
                    :class="selectedSqlScript?.id === script.id ? 'bg-primary/10 border-l-2 border-l-primary' : ''"
                    @click="selectedSqlScript = script"
                >
                    <div class="flex items-center gap-3 min-w-0">
                        <div class="h-4 w-4 rounded-full border border-primary flex items-center justify-center shrink-0">
                            <div v-if="selectedSqlScript?.id === script.id" class="h-2 w-2 rounded-full bg-primary"></div>
                        </div>
                        <div class="min-w-0">
                            <p class="font-medium text-sm truncate">{{ script.name }}</p>
                            <p class="text-xs text-muted-foreground truncate">{{ script.database || 'No database set' }}</p>
                        </div>
                    </div>
                    <Badge variant="outline" class="text-xs capitalize shrink-0 ml-2">
                        {{ (() => { try { return JSON.parse(script.visualization || '{}').type || 'table'; } catch { return 'table'; } })() }}
                    </Badge>
                </div>
            </div>
            <DialogFooter>
                <Button variant="outline" @click="showSqlPickerDialog = false; selectedSqlScript = null">Cancel</Button>
                <Button @click="addSqlWidget" :disabled="!selectedSqlScript">Add Widget</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- CS Script Picker Dialog -->
    <Dialog v-model:open="showCsPickerDialog">
        <DialogContent class="sm:max-w-[600px]">
            <DialogHeader>
                <DialogTitle>
                    {{ csPickerBindTarget ? 'Select Script to Execute'
                        : csPickerMode === 'variable' ? 'Add CS Variable Widget' : 'Add CS Script Output Widget' }}
                </DialogTitle>
                <DialogDescription>
                    {{ csPickerMode === 'variable'
                        ? 'Select a saved CS script that calls Value(result, "label") to display a KPI metric on the dashboard.'
                        : 'Select a saved CS script to execute. Its Table(), Chart(), and StatReport() outputs will be displayed in the widget — the widget always runs the script\'s current saved content, so edits made later in the CS Editor take effect automatically.' }}
                </DialogDescription>
            </DialogHeader>
            <div class="py-2 max-h-[400px] overflow-auto border rounded-md">
                <div v-if="savedCsScripts.length === 0" class="text-center text-muted-foreground p-6">
                    No C# scripts found for the current project.
                </div>
                <div
                    v-for="script in savedCsScripts" :key="script.id"
                    class="flex items-center gap-3 px-3 py-2.5 border-b last:border-b-0 cursor-pointer hover:bg-muted transition-colors"
                    :class="selectedCsScript?.id === script.id ? 'bg-primary/10 border-l-2 border-l-primary' : ''"
                    @click="selectedCsScript = script"
                >
                    <div class="h-4 w-4 rounded-full border border-primary flex items-center justify-center shrink-0">
                        <div v-if="selectedCsScript?.id === script.id" class="h-2 w-2 rounded-full bg-primary"></div>
                    </div>
                    <div class="min-w-0 flex-1">
                        <p class="font-medium text-sm truncate">{{ script.name }}</p>
                        <p class="text-xs text-muted-foreground font-mono truncate">{{ (script.code || '').split('\n')[0].substring(0, 60) }}</p>
                    </div>
                    <Badge variant="outline" class="text-xs shrink-0">CS</Badge>
                </div>
            </div>
            <div v-if="csPickerMode === 'output'" class="flex items-center gap-2">
                <Clock class="w-4 h-4 text-muted-foreground" />
                <span class="text-sm text-muted-foreground">Auto-refresh every</span>
                <Input
                    type="number"
                    :model-value="csPickerRefreshInterval"
                    @update:model-value="val => csPickerRefreshInterval = parseInt(val) || 0"
                    class="h-7 w-20 text-sm"
                    min="0"
                />
                <span class="text-sm text-muted-foreground">minutes (0 = off)</span>
            </div>
            <DialogFooter>
                <Button variant="outline" @click="showCsPickerDialog = false; selectedCsScript = null; csPickerBindTarget = null">Cancel</Button>
                <Button @click="confirmCsPickerSelection" :disabled="!selectedCsScript">
                    {{ csPickerBindTarget ? 'Select & Run' : csPickerMode === 'variable' ? 'Add Variable' : 'Add Output Widget' }}
                </Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- StatReport Viewer Dialog -->
    <Dialog v-model:open="showStatReportDialog">
        <DialogContent class="max-w-2xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
                <DialogTitle class="flex items-center gap-2">
                    <FileText class="w-4 h-4 text-violet-500" />
                    {{ viewingStatReport?.title || 'Statistical Report' }}
                </DialogTitle>
            </DialogHeader>
            <div v-if="viewingStatReport" class="py-2 space-y-4">
                <div v-for="(section, si) in viewingStatReport.sections" :key="si" class="space-y-1">
                    <h3 v-if="section.heading" class="font-semibold text-sm border-b pb-1">{{ section.heading }}</h3>
                    <div v-if="section.type === 'table' && Array.isArray(section.rows)" class="overflow-x-auto">
                        <table class="w-full text-xs border-collapse">
                            <thead v-if="section.headers">
                                <tr>
                                    <th v-for="h in section.headers" :key="h" class="text-left px-2 py-1 bg-muted font-medium border">{{ h }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, ri) in section.rows" :key="ri" class="border-t">
                                    <td v-for="(cell, ci) in row" :key="ci" class="px-2 py-1 border">{{ typeof cell === 'number' ? cell.toFixed(4) : cell }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <p v-else class="text-sm text-muted-foreground whitespace-pre-wrap">{{ section.content || section.text }}</p>
                </div>
            </div>
            <DialogFooter>
                <Button variant="outline" @click="showStatReportDialog = false">Close</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <!-- Bind Variable Dialog -->
    <!-- Select widget options configuration dialog -->
    <Dialog v-model:open="selectConfigDialog">
        <DialogContent class="max-w-md">
            <DialogHeader>
                <DialogTitle>Configure Select Options</DialogTitle>
                <DialogDescription>
                    Choose how options are provided for "{{ selectConfigItem?.title || 'Select Dropdown' }}".
                </DialogDescription>
            </DialogHeader>

            <div v-if="selectConfigItem" class="flex flex-col gap-4 py-2">
                <!-- Source selector -->
                <div class="flex items-center gap-3">
                    <Label class="w-24 shrink-0 text-sm">Source</Label>
                    <select v-model="selectConfigItem.optionsSource"
                        class="flex-1 h-9 rounded-md border border-input bg-background px-3 text-sm">
                        <option value="csv">Comma-separated values</option>
                        <option value="sql">SQL query</option>
                    </select>
                </div>

                <!-- CSV mode -->
                <div v-if="selectConfigItem.optionsSource !== 'sql'" class="flex flex-col gap-2">
                    <Label class="text-sm">Values</Label>
                    <Textarea
                        v-model="selectConfigItem.csvValues"
                        placeholder="Option A, Option B, Option C"
                        class="resize-none h-20"
                        @input="applyCsvOptions(selectConfigItem)"
                    />
                    <span class="text-xs text-muted-foreground">
                        {{ selectConfigItem.options.length }} option(s) — separated by commas
                    </span>
                </div>

                <!-- SQL mode -->
                <div v-else class="flex flex-col gap-2">
                    <Label class="text-sm">Database connection</Label>
                    <select v-model="selectConfigItem.sqlDatabase"
                        class="h-9 rounded-md border border-input bg-background px-3 text-sm">
                        <option value="">Select a connection…</option>
                        <option v-for="db in selectDatabases" :key="db.id" :value="db.id">{{ db.name }}</option>
                    </select>
                    <Label class="text-sm">Query <span class="text-muted-foreground font-normal">(first column used as options)</span></Label>
                    <Textarea
                        v-model="selectConfigItem.sqlQuery"
                        placeholder="SELECT option_value FROM table ORDER BY 1"
                        class="resize-none h-20 font-mono text-sm"
                    />
                    <div class="flex items-center gap-2">
                        <Button variant="outline" size="sm" :disabled="selectConfigItem.sqlLoading" @click="loadSqlSelectOptions(selectConfigItem)">
                            <RefreshCw class="w-3.5 h-3.5 mr-1.5" :class="selectConfigItem.sqlLoading ? 'animate-spin' : ''" />
                            Load Options
                        </Button>
                        <span v-if="selectConfigItem.options.length" class="text-xs text-muted-foreground">
                            {{ selectConfigItem.options.length }} option(s) loaded
                        </span>
                    </div>
                </div>

                <!-- Placeholder -->
                <div class="flex items-center gap-3">
                    <Label class="w-24 shrink-0 text-sm">Placeholder</Label>
                    <Input v-model="selectConfigItem.placeholder" class="flex-1 h-9" placeholder="Select an option…" />
                </div>
            </div>

            <DialogFooter>
                <Button variant="outline" @click="selectConfigDialog = false">Done</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>

    <Dialog v-model:open="bindingDialogOpen">
        <DialogContent class="max-w-sm">
            <DialogHeader>
                <DialogTitle>Bind to Variable</DialogTitle>
                <DialogDescription>Select a project variable to bind this widget to. The widget will display and update the variable's current value.</DialogDescription>
            </DialogHeader>
            <div class="space-y-2 py-2">
                <button
                    class="w-full text-left px-3 py-2 rounded-md border text-sm hover:bg-muted transition-colors flex items-center gap-2"
                    :class="!bindingDialogItem?.boundVariable ? 'border-primary bg-primary/5' : ''"
                    @click="setVariableBinding(null)"
                >
                    <X class="w-4 h-4 text-muted-foreground" />
                    <span class="text-muted-foreground">No binding (standalone widget)</span>
                </button>
                <button
                    v-for="v in variableStore.definitions"
                    :key="v.id"
                    class="w-full text-left px-3 py-2 rounded-md border text-sm hover:bg-muted transition-colors flex items-center justify-between"
                    :class="bindingDialogItem?.boundVariable === v.name ? 'border-primary bg-primary/5' : ''"
                    @click="setVariableBinding(v.name)"
                >
                    <div class="flex items-center gap-2">
                        <Braces class="w-4 h-4 text-primary" />
                        <span class="font-medium">{{ v.name }}</span>
                        <span class="text-muted-foreground text-xs" v-if="v.label && v.label !== v.name">{{ v.label }}</span>
                    </div>
                    <Badge variant="outline" class="text-xs">{{ v.type }}</Badge>
                </button>
                <p v-if="variableStore.definitions.length === 0" class="text-sm text-muted-foreground text-center py-4">
                    No variables defined for this project. Create variables in the SQL Editor (Variables button).
                </p>
            </div>
        </DialogContent>
    </Dialog>
</template>
<style>
.vue-grid-layout {
    background: var(--surface-ground);
    min-height: calc(100vh - 200px);
    padding: 1rem;
}

.vue-grid-item:not(.vue-grid-placeholder) {
    background: var(--surface-card);
    border: 1px solid var(--surface-border);
    border-radius: 8px;
    box-shadow: var(--shadow-1);
    overflow: hidden;
}

.vue-grid-item:hover {
    border-color: var(--primary-color);
    box-shadow: var(--shadow-3);
}

.vue-grid-item .resizing {
    opacity: 0.9;
    border-color: var(--primary-color);
}

.vue-grid-item .static {
    background: var(--surface-card);
    opacity: 0.8;
}

.vue-grid-item .center {
    text-align: center;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    margin: auto;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1rem;
}

.vue-grid-item .no-drag {
    height: 100%;
    width: 100%;
}

.vue-grid-item .minMax {
    font-size: 12px;
}

.vue-grid-item .add {
    cursor: pointer;
}

.vue-draggable-handle {
    position: absolute;
    width: 20px;
    height: 20px;
    top: 0;
    left: 0;
    background-position: bottom right;
    padding: 0 8px 8px 0;
    background-repeat: no-repeat;
    background-origin: content-box;
    box-sizing: border-box;
    cursor: pointer;
    z-index: 10;
}

/* Chart-specific styles */
.grid-item-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
}

.chart-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.chart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.chart-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.chart-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.chart-container:hover .chart-controls {
    opacity: 1;
}

.chart-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

/* Responsive adjustments */
@media (max-width: 768px) {
    .vue-grid-layout {
        padding: 0.5rem;
    }

    .chart-container {
        padding: 0.5rem;
    }

    .chart-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .chart-controls {
        opacity: 1;
        align-self: flex-end;
    }
}

/* Theme-aware grid styling */
.vue-grid-placeholder {
    background: var(--p-primary-color) !important;
    opacity: 0.2;
    border-radius: 8px;
}

.custom-datatable,
.custom-treetable {
    background: var(--p-surface-800);

    .p-datatable-header,
    .p-treetable-header {
        background: var(--p-surface-700);
        color: var(--p-text-color);
    }

    .p-datatable-thead > tr > th,
    .p-treetable-thead > tr > th {
        background: var(--p-surface-700);
        color: var(--p-text-color);
    }

    .p-datatable-tbody > tr,
    .p-treetable-tbody > tr {
        background: var(--p-surface-800);
        color: var(--p-text-color);
    }

    .p-datatable-tbody > tr:hover,
    .p-treetable-tbody > tr:hover {
        background: var(--p-surface-700);
    }
}

/* Text component styling */
.center .p-inplace-display {
    width: 100%;
    color: var(--text-color);
    font-size: 1rem;
    word-wrap: break-word;
}

.center .p-inplace-content {
    width: 100%;
}

/* Drawer styling */
.p-drawer .p-drawer-content {
    padding: 0;
}

.component-menu-container {
    height: 100%;
    display: flex;
    flex-direction: column;
}

.component-panel-menu {
    flex: 1;
    border: none;
    background: transparent;
}

.component-panel-menu .p-panelmenu-panel {
    border: none;
    border-radius: 0;
    margin-bottom: 0;
}

.component-panel-menu .p-panelmenu-header {
    background: var(--surface-100);
    border: none;
    border-bottom: 1px solid var(--surface-border);
    padding: 0;
}

.component-panel-menu .p-panelmenu-header-content {
    padding: 1rem;
    font-weight: 600;
    color: var(--text-color);
    border-radius: 0;
}

.component-panel-menu .p-panelmenu-header-content:hover {
    background: var(--surface-200);
}

.component-panel-menu .p-panelmenu-content {
    border: none;
    background: var(--surface-card);
    padding: 0;
}

.menu-item-content {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    cursor: pointer;
    transition: background-color 0.2s ease;
    border-bottom: 1px solid var(--surface-border);
}

.menu-item-content:hover {
    background: var(--surface-hover);
}

.menu-item-content.menu-item-action {
    cursor: pointer;
}

.menu-item-content.menu-item-action:hover {
    background: var(--primary-color);
    color: var(--primary-color-text);
}

.menu-item-content.menu-item-action:active {
    background: var(--primary-600);
}

.menu-item-icon {
    width: 1.5rem;
    color: var(--text-color-secondary);
    margin-right: 0.75rem;
    font-size: 1rem;
}

.menu-item-content.menu-item-action:hover .menu-item-icon {
    color: var(--primary-color-text);
}

.menu-item-label {
    flex: 1;
    color: var(--text-color);
    font-size: 0.9rem;
}

.menu-item-content.menu-item-action:hover .menu-item-label {
    color: var(--primary-color-text);
    font-weight: 500;
}

.menu-item-badge {
    background: var(--primary-color);
    color: var(--primary-color-text);
    padding: 0.25rem 0.5rem;
    border-radius: 1rem;
    font-size: 0.75rem;
    font-weight: 600;
    margin-left: 0.5rem;
}

.menu-item-content.menu-item-action:hover .menu-item-badge {
    background: var(--surface-card);
    color: var(--primary-color);
}

.menu-footer {
    padding: 1rem;
    border-top: 1px solid var(--surface-border);
    background: var(--surface-50);
    text-align: center;
}

/* Nested menu items styling */
.p-panelmenu .p-panelmenu-panel .p-panelmenu-panel {
    margin-left: 1rem;
    border-left: 2px solid var(--surface-border);
}

.p-panelmenu .p-panelmenu-panel .p-panelmenu-panel .p-panelmenu-header {
    background: transparent;
    border-bottom: none;
}

.p-panelmenu .p-panelmenu-panel .p-panelmenu-panel .p-panelmenu-header-content {
    padding: 0.5rem 1rem;
    font-weight: 500;
    font-size: 0.9rem;
}

/* Special styling for category headers without actions */
.menu-item-content:not(.menu-item-action) {
    background: var(--surface-100);
    font-weight: 600;
    border-bottom: 2px solid var(--surface-border);
}

.menu-item-content:not(.menu-item-action):hover {
    background: var(--surface-200);
}

/* Responsive adjustments */
@media (max-width: 768px) {
    .menu-item-content {
        padding: 1rem;
    }

    .menu-item-icon {
        width: 1.25rem;
        margin-right: 1rem;
    }

    .menu-item-label {
        font-size: 1rem;
    }
}

/* DataTable specific styles */
.datatable-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.datatable-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
    gap: 1rem;
}

.datatable-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.datatable-controls {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.datatable-container:hover .datatable-controls {
    opacity: 1;
}

.datatable-controls .search-input {
    width: 150px;
}

.datatable-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.custom-datatable {
    flex: 1;
    min-height: 0;
    border: 1px solid var(--surface-border);
    border-radius: 6px;
    overflow: hidden;
}

.custom-datatable .p-datatable-header {
    background: var(--surface-section);
    color: var(--text-color);
    border-bottom: 1px solid var(--surface-border);
}

.custom-datatable .p-datatable-thead > tr > th {
    background: var(--surface-section);
    color: var(--text-color);
    border-color: var(--surface-border);
}

.custom-datatable .p-datatable-tbody > tr {
    background: var(--surface-card);
    color: var(--text-color);
}

.custom-datatable .p-datatable-tbody > tr:hover {
    background: var(--surface-hover);
}

.custom-datatable .p-datatable-tbody > tr.p-highlight {
    background: var(--primary-color) !important;
    color: var(--primary-color-text) !important;
}

/* TreeTable specific styles */
.treetable-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.treetable-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.treetable-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.treetable-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.treetable-container:hover .treetable-controls {
    opacity: 1;
}

.treetable-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.custom-treetable {
    flex: 1;
    min-height: 0;
    border: 1px solid var(--surface-border);
    border-radius: 6px;
    overflow: hidden;
}

.custom-treetable .p-treetable-header {
    background: var(--surface-section);
    color: var(--text-color);
    border-bottom: 1px solid var(--surface-border);
}

.custom-treetable .p-treetable-thead > tr > th {
    background: var(--surface-section);
    color: var(--text-color);
    border-color: var(--surface-border);
}

.custom-treetable .p-treetable-tbody > tr {
    background: var(--surface-card);
    color: var(--text-color);
}

.custom-treetable .p-treetable-tbody > tr:hover {
    background: var(--surface-hover);
}

/* Image specific styles */
.image-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.image-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.image-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.image-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.image-container:hover .image-controls {
    opacity: 1;
}

.image-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.image-content {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 0;
}

.custom-image {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    border-radius: 6px;
    box-shadow: var(--shadow-2);
}

/* Select specific styles */
.select-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.select-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.select-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.select-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.select-container:hover .select-controls {
    opacity: 1;
}

.select-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.select-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 1rem;
}

.custom-select {
    width: 100%;
}

.select-value {
    color: var(--text-color-secondary);
    font-size: 0.9rem;
    font-weight: 500;
    padding: 0.5rem;
    background: var(--surface-100);
    border-radius: 4px;
    text-align: center;
}

/* InputText specific styles */
.input-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.input-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.input-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.input-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.input-container:hover .input-controls {
    opacity: 1;
}

.input-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.input-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 1rem;
}

.custom-input {
    width: 100%;
    font-size: 1rem;
}

.input-info {
    color: var(--text-color-secondary);
    font-size: 0.8rem;
    text-align: right;
}

/* SQL Widget */
.sql-widget-container:hover .sql-widget-controls { opacity: 1; }
.sql-widget-controls { opacity: 0; transition: opacity 0.2s; }

/* Variable Widget */
.variable-widget:hover .variable-controls { opacity: 1; }
.variable-controls { opacity: 0; transition: opacity 0.2s; }

/* FunctOutput Widget */
.funct-output-widget:hover .funct-controls { opacity: 1; }
.funct-controls { opacity: 0; transition: opacity 0.2s; }

/* Responsive adjustments for new components */
@media (max-width: 768px) {
    .datatable-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .datatable-controls {
        opacity: 1;
        align-self: flex-end;
        flex-wrap: wrap;
    }

    .datatable-controls .search-input {
        width: 120px;
    }

    .treetable-header,
    .image-header,
    .select-header,
    .input-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .treetable-controls,
    .image-controls,
    .select-controls,
    .input-controls {
        opacity: 1;
        align-self: flex-end;
    }
}
</style>
