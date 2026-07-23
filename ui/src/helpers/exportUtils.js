// Shared helpers for exporting tabular widget/query data to CSV or Excel (.xlsx).
import * as XLSX from 'xlsx';

// Derives {field, header} column definitions from the first row when the
// caller doesn't already have an explicit column list (e.g. plain arrays of
// objects coming from a DataTable widget).
function resolveColumns(rows, columns) {
    if (columns && columns.length) return columns.map(c => ({ field: c.field, header: c.header ?? c.field }));
    if (!rows || !rows.length) return [];
    return Object.keys(rows[0]).map(key => ({ field: key, header: key }));
}

function cellValue(row, field) {
    const value = row[field];
    return value === null || value === undefined ? '' : value;
}

function triggerDownload(blob, filename) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
}

// Appends today's date to a base filename, e.g. ("query-results", "xlsx") -> "query-results-2026-07-23.xlsx"
export function timestampedFilename(base, ext) {
    return `${base}-${new Date().toISOString().split('T')[0]}.${ext}`;
}

export function downloadCsv(rows, columns, filenameBase) {
    const cols = resolveColumns(rows, columns);
    const escape = (value) => {
        const str = String(value);
        return /[",\n]/.test(str) ? `"${str.replace(/"/g, '""')}"` : str;
    };
    const csv = [
        cols.map(c => escape(c.header)).join(','),
        ...rows.map(row => cols.map(c => escape(cellValue(row, c.field))).join(','))
    ].join('\n');

    triggerDownload(new Blob([csv], { type: 'text/csv' }), timestampedFilename(filenameBase, 'csv'));
}

export function downloadExcel(rows, columns, filenameBase, sheetName = 'Sheet1') {
    const cols = resolveColumns(rows, columns);
    const aoa = [
        cols.map(c => c.header),
        ...rows.map(row => cols.map(c => cellValue(row, c.field)))
    ];

    const worksheet = XLSX.utils.aoa_to_sheet(aoa);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, sheetName.substring(0, 31) || 'Sheet1');
    const buffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });

    triggerDownload(
        new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
        timestampedFilename(filenameBase, 'xlsx')
    );
}
