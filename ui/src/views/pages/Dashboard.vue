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
    Check,
    Image,
    Target,
    Type,
    Download,
    Minus,
    Table,
    Trash2,
    ChevronDown,
    CircleDot,
    RefreshCw,
    Circle,
    ZoomIn,
    CheckSquare,
    ChevronRight,
    Save,
    X,
    ArrowUpDown
} from 'lucide-vue-next';
import GridLayout from '@/components/draggable/GridLayout.vue';
import GridItem from '@/components/draggable/GridItem.vue';
import BaseChart from '@/components/BaseChart.vue';
import { nextTick, ref, watch } from 'vue';
import { toast as sonnerToast } from 'vue-sonner';

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
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Checkbox } from '@/components/ui/checkbox';

const visibleCompo = ref(false);
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
                label: 'Excel Editor',
                icon: markRaw(Table),
                command: () => addcomponent('ExcelEditor'),
                badge: 'Table'
            },
            {
                label: 'Data Table',
                icon: markRaw(Table),
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
                label: 'Toggle Button',
                icon: markRaw(CheckSquare),
                command: () => addcomponent('ToggleButton'),
                badge: 'Input'
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
        label: 'Dashboard Actions',
        icon: markRaw(Settings),
        items: [
            {
                label: 'Save Layout',
                icon: markRaw(Save),
                command: () => saveDashboardLayout(),
                badge: 'New'
            },
            {
                label: 'Load Layout',
                icon: markRaw(FolderOpen),
                command: () => loadDashboardLayout()
            },
            {
                label: 'Reset Dashboard',
                icon: markRaw(RefreshCw),
                command: () => resetDashboard()
            },
            {
                label: 'Export Dashboard',
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

const addcomponent = async (type) => {
    console.log('Adding component:', type);

    let newComponent;

    if (type === 'Text') {
        newComponent = {
            x: 1,
            y: 0,
            w: 3,
            h: 2,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'Text',
            value: 'Click to edit'
        };
    } else if (type === 'ExcelEditor') {
        newComponent = {
            x: 1,
            y: 0,
            w: 8,
            h: 8,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'ExcelEditor',
            title: 'Excel Editor',
            tableData: generateSampleTableData(),
            fields: generateSampleFields(),
            filterRow: true,
            noFooter: false,
            readonly: false
        };
    } else if (type === 'DataTable') {
        newComponent = {
            x: 1,
            y: 0,
            w: 10,
            h: 8,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'DataTable',
            title: 'Data Table',
            tableData: generateSampleCustomers(),
            columns: generateDataTableColumns(),
            globalFilter: '',
            selectedRows: []
        };
    } else if (type === 'TreeTable') {
        newComponent = {
            x: 1,
            y: 0,
            w: 10,
            h: 8,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'TreeTable',
            title: 'Tree Table',
            treeData: generateTreeData(),
            columns: generateTreeTableColumns(),
            expandedKeys: {}
        };
    } else if (type === 'Image') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 5,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'Image',
            title: 'Image Component',
            src: 'https://via.placeholder.com/400x300?text=Sample+Image',
            alt: 'Sample image',
            preview: true,
            width: '100%',
            height: 'auto'
        };
    } else if (type === 'ToggleButton') {
        newComponent = {
            x: 1,
            y: 0,
            w: 3,
            h: 3,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'ToggleButton',
            title: 'Toggle Button',
            checked: false,
            onLabel: 'ON',
            offLabel: 'OFF',
            onIcon: markRaw(Check),
            offIcon: markRaw(X)
        };
    } else if (type === 'Select') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 3,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'Select',
            title: 'Select Dropdown',
            selectedValue: null,
            options: generateSelectOptions(),
            placeholder: 'Select an option',
            optionLabel: 'name',
            optionValue: 'code'
        };
    } else if (type === 'InputText') {
        newComponent = {
            x: 1,
            y: 0,
            w: 4,
            h: 2,
            i: layout.value.componentes.length.toString(),
            static: false,
            type: 'InputText',
            title: 'Input Text',
            value: '',
            placeholder: 'Enter text here...',
            size: 'medium'
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
            i: layout.value.componentes.length.toString(),
            static: false,
            type: type,
            title: getDefaultChartTitle(type),
            chartData: generateChartData(type),
            showLegend: true,
            showDataLabels: true
        };
    }

    if (newComponent) {
        layout.value.componentes.push(newComponent);
        renderComponent.value = false;
        await nextTick();
        renderComponent.value = true;

        // Close drawer after adding component
        visibleCompo.value = false;

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
        ExcelEditor: 'Excel Editor',
        DataTable: 'Data Table',
        TreeTable: 'Tree Table',
        Image: 'Image',
        ToggleButton: 'Toggle Button',
        Select: 'Select Dropdown',
        InputText: 'Input Text'
    };
    return titles[type] || type;
}

// Excel Editor helper functions
function generateSampleFields() {
    return [
        {
            name: 'id',
            label: 'ID',
            type: 'number',
            width: '80px',
            readonly: true,
            keyField: true
        },
        {
            name: 'name',
            label: 'Name',
            type: 'string',
            width: '150px',
            autocomplete: true
        },
        {
            name: 'email',
            label: 'Email',
            type: 'string',
            width: '200px'
        },
        {
            name: 'department',
            label: 'Department',
            type: 'select',
            width: '140px',
            options: [
                { value: 'IT', text: 'Information Technology' },
                { value: 'HR', text: 'Human Resources' },
                { value: 'FIN', text: 'Finance' },
                { value: 'MKT', text: 'Marketing' },
                { value: 'OPS', text: 'Operations' }
            ]
        },
        {
            name: 'salary',
            label: 'Salary',
            type: 'number',
            width: '120px',
            toText: (value) =>
                new Intl.NumberFormat('en-US', {
                    style: 'currency',
                    currency: 'USD'
                }).format(value)
        },
        {
            name: 'joinDate',
            label: 'Join Date',
            type: 'date',
            width: '130px'
        },
        {
            name: 'active',
            label: 'Active',
            type: 'checkYN',
            width: '80px'
        },
        {
            name: 'notes',
            label: 'Notes',
            type: 'string',
            width: '200px'
        }
    ];
}

function generateSampleTableData() {
    const departments = ['IT', 'HR', 'FIN', 'MKT', 'OPS'];
    const names = ['John Smith', 'Jane Doe', 'Mike Johnson', 'Sarah Wilson', 'David Brown', 'Lisa Davis', 'Robert Miller', 'Emily Garcia', 'Michael Rodriguez', 'Jennifer Martinez'];

    return Array.from({ length: 10 }, (_, index) => ({
        $id: index + 1,
        id: index + 1,
        name: names[index] || `Employee ${index + 1}`,
        email: `${names[index]?.toLowerCase().replace(' ', '.') || `employee${index + 1}`}@company.com`,
        department: departments[Math.floor(Math.random() * departments.length)],
        salary: Math.floor(Math.random() * 100000) + 40000,
        joinDate: new Date(2020 + Math.floor(Math.random() * 4), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString().split('T')[0],
        active: Math.random() > 0.2 ? 'Y' : 'N',
        notes: Math.random() > 0.5 ? 'Good performance' : ''
    }));
}

function getChartHeight(gridHeight) {
    // Convert grid height to pixel height (each grid unit is approximately 40px + margin)
    return `${gridHeight * 40 - 60}px`;
}

function getExcelHeight(gridHeight) {
    // Convert grid height to pixel height for Excel editor (accounting for header)
    return `${gridHeight * 40 - 80}px`;
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

// Excel Editor Functions
function addExcelRow(item) {
    const newId = Math.max(...item.tableData.map((row) => row.id || 0)) + 1;
    const newRow = {
        $id: newId,
        id: newId,
        name: '',
        email: '',
        department: 'IT',
        salary: 50000,
        joinDate: new Date().toISOString().split('T')[0],
        active: 'Y',
        notes: ''
    };
    item.tableData.push(newRow);

    toast.add({
        severity: 'success',
        summary: 'Row Added',
        detail: 'New row has been added to the table',
        life: 2000
    });
}

function exportExcelData(item) {
    try {
        // Export as CSV for better Excel compatibility
        const csv = convertToCSV(item.tableData);
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${item.title || 'excel-data'}-${new Date().toISOString().split('T')[0]}.csv`;
        link.click();
        URL.revokeObjectURL(url);

        toast.add({
            severity: 'success',
            summary: 'Data Exported',
            detail: 'Data has been exported to CSV format',
            life: 3000
        });
    } catch (error) {
        // Fallback to JSON export if CSV conversion fails
        const data = JSON.stringify(item.tableData, null, 2);
        const blob = new Blob([data], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${item.title || 'excel-data'}-${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        URL.revokeObjectURL(url);

        toast.add({
            severity: 'success',
            summary: 'Data Exported',
            detail: 'Data has been exported as JSON format',
            life: 3000
        });
    }
}

function importExcelData(item) {
    // Create hidden file input
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.json,.csv';
    fileInput.style.display = 'none';

    fileInput.onchange = (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                try {
                    const fileContent = e.target.result;
                    let importedData;

                    if (file.name.toLowerCase().endsWith('.csv')) {
                        // Parse CSV file
                        importedData = parseCSV(fileContent);
                    } else {
                        // Parse JSON file
                        importedData = JSON.parse(fileContent);
                    }

                    if (Array.isArray(importedData) && importedData.length > 0) {
                        // Ensure each row has required properties and generate IDs if missing
                        const processedData = importedData.map((row, index) => ({
                            $id: row.$id || row.id || index + 1,
                            id: row.id || index + 1,
                            ...row
                        }));

                        item.tableData = processedData;
                        toast.add({
                            severity: 'success',
                            summary: 'Data Imported',
                            detail: `Successfully imported ${processedData.length} rows`,
                            life: 3000
                        });
                    } else {
                        throw new Error('Invalid data format or empty file');
                    }
                } catch (error) {
                    toast.add({
                        severity: 'error',
                        summary: 'Import Failed',
                        detail: 'Invalid file format. Please upload a valid CSV or JSON file.',
                        life: 5000
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

function resetExcelData(item) {
    item.tableData = generateSampleTableData();
    toast.add({
        severity: 'info',
        summary: 'Data Reset',
        detail: 'Excel data has been reset to sample data',
        life: 2000
    });
}

function onExcelCellEditComplete(event) {
    const { data, newValue, field } = event;
    data[field] = newValue;

    toast.add({
        severity: 'success',
        summary: 'Cell Updated',
        detail: `Updated ${field}`,
        life: 1000
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

function exportDataTable(item) {
    const csv = convertToCSV(item.tableData);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${item.title || 'datatable'}-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);

    toast.add({
        severity: 'success',
        summary: 'Data Exported',
        detail: 'Data has been exported to CSV',
        life: 3000
    });
}

function convertToCSV(data) {
    if (!data || data.length === 0) return '';

    const headers = Object.keys(data[0]).join(',');
    const rows = data.map((row) =>
        Object.values(row)
            .map((value) => (typeof value === 'string' && value.includes(',') ? `"${value}"` : value))
            .join(',')
    );

    return [headers, ...rows].join('\n');
}

function parseCSV(csvText) {
    const lines = csvText.trim().split('\n');
    if (lines.length < 2) return [];

    const headers = lines[0].split(',').map((header) => header.trim().replace(/"/g, ''));
    const data = [];

    for (let i = 1; i < lines.length; i++) {
        const values = lines[i].split(',').map((value) => value.trim().replace(/"/g, ''));
        const row = {};

        headers.forEach((header, index) => {
            let value = values[index] || '';

            // Try to convert numeric values
            if (!isNaN(value) && value !== '') {
                value = Number(value);
            }

            // Convert boolean-like values
            if (value === 'true') value = true;
            if (value === 'false') value = false;
            if (value === 'Y') value = 'Y';
            if (value === 'N') value = 'N';

            row[header] = value;
        });

        data.push(row);
    }

    return data;
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

// ToggleButton helper functions
function onToggleChange(item, event) {
    toast.add({
        severity: 'info',
        summary: 'Toggle Changed',
        detail: `Toggle is now ${event.value ? 'ON' : 'OFF'}`,
        life: 2000
    });
}

// Select helper functions
function generateSelectOptions() {
    return [
        { name: 'Australia', code: 'AU' },
        { name: 'Brazil', code: 'BR' },
        { name: 'China', code: 'CN' },
        { name: 'Egypt', code: 'EG' },
        { name: 'France', code: 'FR' },
        { name: 'Germany', code: 'DE' },
        { name: 'India', code: 'IN' },
        { name: 'Japan', code: 'JP' },
        { name: 'Spain', code: 'ES' },
        { name: 'United States', code: 'US' }
    ];
}

function refreshSelectOptions(item) {
    const additionalOptions = [
        { name: 'Canada', code: 'CA' },
        { name: 'Mexico', code: 'MX' },
        { name: 'Italy', code: 'IT' },
        { name: 'United Kingdom', code: 'GB' }
    ];

    // Add some new options
    item.options = [...item.options, ...additionalOptions.filter((opt) => !item.options.find((existing) => existing.code === opt.code))];

    toast.add({
        severity: 'info',
        summary: 'Options Refreshed',
        detail: 'New options have been added to the dropdown',
        life: 2000
    });
}

function onSelectChange(item, event) {
    toast.add({
        severity: 'info',
        summary: 'Selection Changed',
        detail: `Selected: ${getSelectedOptionLabel(item)}`,
        life: 2000
    });
}

function getSelectedOptionLabel(item) {
    if (!item.selectedValue) return 'None';
    const option = item.options.find((opt) => opt[item.optionValue] === item.selectedValue);
    return option ? option[item.optionLabel] : item.selectedValue;
}

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
                excelCount: layout.value.componentes.filter((c) => c.type === 'ExcelEditor').length,
                dataTableCount: layout.value.componentes.filter((c) => c.type === 'DataTable').length,
                treeTableCount: layout.value.componentes.filter((c) => c.type === 'TreeTable').length,
                inputCount: layout.value.componentes.filter((c) => ['ToggleButton', 'Select', 'InputText'].includes(c.type)).length,
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
</script>

<template>
    <Sheet v-model:open="visibleCompo">
        <SheetContent class="w-[320px] sm:w-[540px]">
            <SheetHeader>
                <SheetTitle>Dashboard Elements</SheetTitle>
                <SheetDescription> Drag and drop components to rearrange them on your dashboard </SheetDescription>
            </SheetHeader>
            <div class="component-menu-container mt-4">
                <Accordion type="single" class="w-full" collapsible>
                    <AccordionItem v-for="(menuGroup, index) in componentMenuItems" :key="index" :value="`item-${index}`">
                        <AccordionTrigger>
                            <div class="flex items-center gap-2">
                                <component :is="menuGroup.icon" class="w-4 h-4"></component>
                                <span>{{ menuGroup.label }}</span>
                            </div>
                        </AccordionTrigger>
                        <AccordionContent>
                            <div class="flex flex-col gap-2">
                                <div v-for="item in menuGroup.items" :key="item.label">
                                    <Button variant="ghost" class="w-full justify-start text-left" @click="item.command && item.command()">
                                        <div class="flex items-center gap-2 w-full">
                                            <component :is="item.icon" class="w-4 h-4"></component>
                                            <span>{{ item.label }}</span>
                                            <Badge v-if="item.badge" class="ml-auto" variant="secondary">{{ item.badge }}</Badge>
                                        </div>
                                    </Button>
                                    <!-- Handle nested items if needed for charts -->
                                    <div v-if="item.items" class="pl-4 mt-2 flex flex-col gap-1 border-l ml-2">
                                        <Button v-for="subItem in item.items" :key="subItem.label" variant="ghost" size="sm" class="w-full justify-start text-left text-sm" @click="subItem.command && subItem.command()">
                                            <div class="flex items-center gap-2 w-full">
                                                <component :is="subItem.icon" class="w-4 h-4"></component>
                                                <span>{{ subItem.label }}</span>
                                                <Badge v-if="subItem.badge" class="ml-auto text-[10px]" variant="secondary">{{ subItem.badge }}</Badge>
                                            </div>
                                        </Button>
                                    </div>
                                </div>
                            </div>
                        </AccordionContent>
                    </AccordionItem>
                </Accordion>
            </div>
        </SheetContent>
    </Sheet>

    <div class="flex items-center justify-between p-2 border-b mb-4">
        <div class="flex items-center gap-2">
            <Button variant="outline" size="sm" @click="visibleCompo = true"> <Plus class="w-4 h-4 mr-2" /> Add Elements </Button>
            <Button variant="ghost" size="icon">
                <Printer class="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon" @click="addcomponent(2)">
                <Upload class="w-4 h-4" />
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

        <div class="w-[150px]"></div>
        <!-- Spacer for flex-between balance -->
    </div>
    <grid-layout v-model:layout="layout.componentes" :col-num="15" :row-height="40" :auto-size="true" is-draggable is-resizable vertical-compact use-css-transforms v-if="renderComponent">
        <grid-item v-for="item in layout.componentes" :static="item.static" :x="item.x" :y="item.y" :w="item.w" :h="item.h" :i="item.i" :key="item.i" class="grid-item-container">
            <!-- Text Component -->
            <div v-if="item.type === 'Text'" class="w-full h-full flex items-center justify-center">
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
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="refreshChartData(item)" title="Refresh Data">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="editChartSettings(item)" title="Settings">
                            <Settings class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <BaseChart
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
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="exportDataTable(item)" title="Export CSV">
                            <Download class="w-3 h-3 text-xs" />
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
                            <!-- Flattened Tree Rendering Implementation Strategy (Requires logic updates) -->
                            <TableRow v-for="(node, index) in item.treeData" :key="node.key || index" class="hover:bg-muted/30 transition-colors">
                                <TableCell v-for="(col, colIndex) in item.columns" :key="col.field">
                                    <div class="flex items-center gap-2" :style="{ paddingLeft: col.expander ? `${(node.level || 0) * 1.5}rem` : '0' }">
                                        <Button v-if="col.expander && node.children && node.children.length > 0" variant="ghost" size="icon" class="h-5 w-5 p-0" @click="toggleTreeNode(item, node)">
                                            <component :is="isNodeExpanded(item, node) ? ChevronDown : ChevronRight" class="w-3 h-3 text-[10px]" />
                                        </Button>
                                        <span v-else-if="col.expander" class="w-5 inline-block"></span>
                                        <span>{{ node.data[col.field] }}</span>
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
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="item.preview = !item.preview" :title="item.preview ? 'Disable Preview' : 'Enable Preview'">
                            <ZoomIn class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="refreshImage(item)" title="Refresh Image">
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

            <!-- ToggleButton Component -->
            <div v-else-if="item.type === 'ToggleButton'" class="toggle-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="toggle-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Toggle Button' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="toggle-controls">
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="toggle-content flex-1 flex flex-col items-center justify-center gap-4">
                    <div class="flex items-center space-x-2">
                        <Switch
                            :id="`switch-${item.i}`"
                            :checked="item.checked"
                            @update:checked="
                                (val) => {
                                    item.checked = val;
                                    onToggleChange(item, val);
                                }
                            "
                        />
                        <Label :for="`switch-${item.i}`">{{ item.checked ? item.onLabel || 'ON' : item.offLabel || 'OFF' }}</Label>
                    </div>
                    <div class="toggle-status text-sm text-muted-foreground">Status: {{ item.checked ? 'ON' : 'OFF' }}</div>
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
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="refreshSelectOptions(item)" title="Refresh Options">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="select-content flex-1 flex flex-col gap-2 p-2">
                    <!-- Native Select for simplicity over building out the full Combobox here -->
                    <select
                        class="flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        v-model="item.selectedValue"
                        @change="(e) => onSelectChange(item, e)"
                    >
                        <option value="" disabled selected v-if="item.placeholder">{{ item.placeholder }}</option>
                        <option v-for="opt in item.options" :key="opt[item.optionValue || 'value'] || opt" :value="opt[item.optionValue || 'value'] || opt">
                            {{ opt[item.optionLabel || 'label'] || opt }}
                        </option>
                    </select>
                    <div class="select-value text-sm text-muted-foreground mt-2" v-if="item.selectedValue">Selected: {{ getSelectedOptionLabel(item) }}</div>
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
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="input-content flex-1 flex flex-col gap-2 justify-center p-2">
                    <Input v-model="item.value" :placeholder="item.placeholder" class="w-full" @input="(e) => onInputTextChange(item, e)" />
                    <div class="input-info text-xs text-muted-foreground mt-1" v-if="item.value">Length: {{ item.value.length }} characters</div>
                </div>
            </div>

            <!-- Excel Editor Component -->
            <div v-else-if="item.type === 'ExcelEditor'" class="excel-container flex flex-col h-full border rounded-md p-2 bg-card">
                <div class="excel-header flex justify-between items-center mb-2">
                    <div v-if="!item.editing" class="cursor-pointer hover:underline font-medium text-sm" @click="item.editing = true">
                        {{ item.title || 'Excel Editor' }}
                    </div>
                    <div v-else class="flex items-center gap-1">
                        <Input v-model="item.title" class="h-7 text-sm" @keyup.enter="item.editing = false" />
                        <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="item.editing = false">
                            <X class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                    <div class="excel-controls flex gap-1 items-center">
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-green-600 hover:text-green-700" @click="addExcelRow(item)" title="Add Row">
                            <Plus class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="exportExcelData(item)" title="Export Data">
                            <Download class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="importExcelData(item)" title="Import Data">
                            <Upload class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7" @click="resetExcelData(item)" title="Reset Data">
                            <RefreshCw class="w-3 h-3 text-xs" />
                        </Button>
                        <Button variant="ghost" size="icon" class="h-7 w-7 text-destructive hover:bg-destructive/10" @click="removeComponent(item.i)" title="Remove">
                            <Trash2 class="w-3 h-3 text-xs" />
                        </Button>
                    </div>
                </div>

                <div class="overflow-auto border rounded-md flex-1 custom-scroll" :style="{ maxHeight: getExcelHeight(item.h) }">
                    <Table>
                        <TableHeader class="sticky top-0 bg-secondary/80 backdrop-blur-sm z-10">
                            <TableRow>
                                <TableHead v-for="field in item.fields" :key="field.name" class="font-semibold text-xs min-w-[120px]">
                                    {{ field.label }}
                                </TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            <TableRow v-for="(data, index) in item.tableData" :key="data.id || index" class="hover:bg-muted/30 transition-colors">
                                <TableCell v-for="field in item.fields" :key="field.name" class="p-1">
                                    <template v-if="!field.readonly">
                                        <Input
                                            v-if="field.type === 'string'"
                                            v-model="data[field.name]"
                                            @keydown.enter="$event.target.blur()"
                                            class="h-8 text-xs"
                                            @blur="onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] })"
                                        />
                                        <Input
                                            v-else-if="field.type === 'number'"
                                            type="number"
                                            v-model="data[field.name]"
                                            @keydown.enter="$event.target.blur()"
                                            class="h-8 text-xs"
                                            @blur="onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] })"
                                        />
                                        <Input v-else-if="field.type === 'date'" type="date" v-model="data[field.name]" class="h-8 text-xs w-full block" @blur="onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] })" />
                                        <select
                                            v-else-if="field.type === 'select'"
                                            v-model="data[field.name]"
                                            class="flex h-8 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-1 text-xs ring-offset-background focus:outline-none focus:ring-1 focus:ring-ring"
                                            @change="onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] })"
                                        >
                                            <option v-for="opt in field.options" :key="opt.value" :value="opt.value">{{ opt.text }}</option>
                                        </select>
                                        <div v-else-if="field.type === 'checkYN'" class="flex items-center justify-center h-full">
                                            <Checkbox
                                                :checked="data[field.name] === 'Y' || data[field.name] === true"
                                                @update:checked="
                                                    (val) => {
                                                        data[field.name] = val ? 'Y' : 'N';
                                                        onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] });
                                                    }
                                                "
                                            />
                                        </div>
                                        <Input v-else v-model="data[field.name]" @keydown.enter="$event.target.blur()" class="h-8 text-xs" @blur="onExcelCellEditComplete({ data, field: field.name, newValue: data[field.name] })" />
                                    </template>
                                    <template v-else>
                                        <div class="px-2 py-1 text-sm">
                                            <span v-if="field.type === 'checkYN'">
                                                {{ data[field.name] === 'Y' || data[field.name] === true ? 'Yes' : 'No' }}
                                            </span>
                                            <span v-else-if="field.toText">
                                                {{ field.toText(data[field.name]) }}
                                            </span>
                                            <span v-else>
                                                {{ data[field.name] }}
                                            </span>
                                        </div>
                                    </template>
                                </TableCell>
                            </TableRow>
                            <TableRow v-if="!item.tableData || item.tableData.length === 0">
                                <TableCell :colspan="item.fields.length" class="text-center py-4 text-muted-foreground">No data available</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </div>
                <!-- Pagination Placeholder -->
                <div class="flex items-center justify-between px-2 py-1 border-t text-xs text-muted-foreground bg-background">
                    <div>Showing {{ item.tableData?.length || 0 }} entries</div>
                    <div class="flex gap-1">
                        <Button variant="outline" size="icon" class="h-6 w-6"><ChevronLeft class="w-3 h-3 text-[10px]" /></Button>
                        <Button variant="outline" size="icon" class="h-6 w-6"><ChevronRight class="w-3 h-3 text-[10px]" /></Button>
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

.excel-editor .vue-excel-editor,
.excel-editor .systable {
    background: var(--p-surface-900);
    color: var(--p-text-color);
}

.excel-editor .systable th {
    background: var(--p-surface-700);
    color: var(--p-text-color);
    border-color: var(--p-surface-border);
}

.excel-editor .systable td {
    background: var(--p-surface-800);
    color: var(--p-text-color);
    border-color: var(--p-surface-border);
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

/* Excel Editor specific styles */
.excel-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.excel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.excel-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.excel-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.excel-container:hover .excel-controls {
    opacity: 1;
}

.excel-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.excel-editor {
    flex: 1;
    min-height: 0;
    border: 1px solid var(--surface-border);
    border-radius: 6px;
    overflow: hidden;
}

/* Override Vue Excel Editor styles for theme integration */
.excel-editor .vue-excel-editor {
    background: var(--surface-card);
    color: var(--text-color);
}

.excel-editor .systable {
    background: var(--surface-card);
    color: var(--text-color);
}

.excel-editor .systable th {
    background: var(--surface-section);
    color: var(--text-color);
    border-color: var(--surface-border);
}

.excel-editor .systable td {
    border-color: var(--surface-border);
    color: var(--text-color);
}

.excel-editor .systable tr:hover {
    background: var(--surface-hover);
}

.excel-editor .systable tr.select {
    background: var(--primary-color) !important;
    color: var(--primary-color-text) !important;
}

/* Responsive adjustments for Excel Editor */
@media (max-width: 768px) {
    .excel-container {
        padding: 0.5rem;
    }

    .excel-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .excel-controls {
        opacity: 1;
        align-self: flex-end;
        flex-wrap: wrap;
    }

    .excel-controls .p-button {
        padding: 0.5rem;
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

/* ToggleButton specific styles */
.toggle-container {
    height: 100%;
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0.75rem;
    box-sizing: border-box;
}

.toggle-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
    min-height: 2rem;
    flex-shrink: 0;
}

.toggle-header h5 {
    color: var(--text-color);
    font-size: 1rem;
    font-weight: 600;
    margin: 0;
    cursor: pointer;
}

.toggle-controls {
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.toggle-container:hover .toggle-controls {
    opacity: 1;
}

.toggle-controls .p-button {
    min-width: auto;
    padding: 0.25rem;
}

.toggle-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 1rem;
}

.custom-toggle {
    font-size: 1.1rem;
}

.toggle-status {
    color: var(--text-color-secondary);
    font-size: 0.9rem;
    font-weight: 500;
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
    .toggle-header,
    .select-header,
    .input-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .treetable-controls,
    .image-controls,
    .toggle-controls,
    .select-controls,
    .input-controls {
        opacity: 1;
        align-self: flex-end;
    }
}
</style>
