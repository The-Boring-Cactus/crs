<script setup>
import { toast } from 'vue-sonner';
import { Button } from '@/components/ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Download, FileSpreadsheet, FileText } from 'lucide-vue-next';
import { downloadCsv, downloadExcel } from '@/helpers/exportUtils';

// Reusable "Export" dropdown (CSV / Excel) for any tabular widget or query
// result. `rows` is a plain array of objects; `columns` is optional
// [{ field, header }] -- when omitted, columns are derived from the first row.
const props = defineProps({
    rows: { type: Array, default: () => [] },
    columns: { type: Array, default: null },
    filename: { type: String, default: 'export' },
    sheetName: { type: String, default: 'Sheet1' },
    disabled: { type: Boolean, default: false },
    size: { type: String, default: 'sm' },
    variant: { type: String, default: 'outline' },
    iconOnly: { type: Boolean, default: false }
});

const isDisabled = () => props.disabled || !props.rows || props.rows.length === 0;

function exportCsv() {
    downloadCsv(props.rows, props.columns, props.filename);
    toast.success('Export Complete', { description: 'Exported to CSV file' });
}

function exportExcel() {
    downloadExcel(props.rows, props.columns, props.filename, props.sheetName);
    toast.success('Export Complete', { description: 'Exported to Excel file' });
}
</script>

<template>
    <DropdownMenu>
        <DropdownMenuTrigger as-child>
            <Button :variant="variant" :size="size" :disabled="isDisabled()" class="gap-2" title="Export">
                <Download class="w-3.5 h-3.5" />
                <span v-if="!iconOnly">Export</span>
            </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
            <DropdownMenuItem class="gap-2" @click="exportCsv">
                <FileText class="w-3.5 h-3.5" /> Export as CSV
            </DropdownMenuItem>
            <DropdownMenuItem class="gap-2" @click="exportExcel">
                <FileSpreadsheet class="w-3.5 h-3.5" /> Export as Excel
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenu>
</template>
