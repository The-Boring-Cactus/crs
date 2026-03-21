<script setup>
import { toast } from 'vue-sonner';
import { onMounted, ref } from 'vue';
import { getCoreRowModel, useVueTable, getPaginationRowModel, getSortedRowModel, getFilteredRowModel } from '@tanstack/vue-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Textarea } from '@/components/ui/textarea';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { Plus, Trash2, Upload, Search, ArrowUp, ArrowDown, Image as ImageIcon, Star, Pencil, AlertTriangle } from 'lucide-vue-next';

onMounted(() => {
    // Mock data for products
    products.value = [
        { id: '1000', code: 'f230fh0g3', name: 'Bamboo Watch', description: 'Product Description', image: 'bamboo-watch.jpg', price: 65, category: 'Accessories', quantity: 24, inventoryStatus: 'INSTOCK', rating: 5 },
        { id: '1001', code: 'nvklal433', name: 'Black Watch', description: 'Product Description', image: 'black-watch.jpg', price: 72, category: 'Accessories', quantity: 61, inventoryStatus: 'INSTOCK', rating: 4 },
        { id: '1002', code: 'zz21cz3c1', name: 'Blue Band', description: 'Product Description', image: 'blue-band.jpg', price: 79, category: 'Fitness', quantity: 2, inventoryStatus: 'LOWSTOCK', rating: 3 },
        { id: '1003', code: '244wgerg2', name: 'Blue T-Shirt', description: 'Product Description', image: 'blue-t-shirt.jpg', price: 29, category: 'Clothing', quantity: 25, inventoryStatus: 'INSTOCK', rating: 5 },
        { id: '1004', code: 'h456wer53', name: 'Bracelet', description: 'Product Description', image: 'bracelet.jpg', price: 15, category: 'Accessories', quantity: 73, inventoryStatus: 'INSTOCK', rating: 4 },
        { id: '1005', code: 'av2231fwg', name: 'Brown Purse', description: 'Product Description', image: 'brown-purse.jpg', price: 120, category: 'Accessories', quantity: 0, inventoryStatus: 'OUTOFSTOCK', rating: 4 },
        { id: '1006', code: 'bib36pfvm', name: 'Chakra Bracelet', description: 'Product Description', image: 'chakra-bracelet.jpg', price: 32, category: 'Accessories', quantity: 5, inventoryStatus: 'LOWSTOCK', rating: 3 },
        { id: '1007', code: 'mbvjkgip5', name: 'Galaxy Earrings', description: 'Product Description', image: 'galaxy-earrings.jpg', price: 34, category: 'Accessories', quantity: 23, inventoryStatus: 'INSTOCK', rating: 5 },
        { id: '1008', code: 'vbb124btr', name: 'Game Controller', description: 'Product Description', image: 'game-controller.jpg', price: 99, category: 'Electronics', quantity: 2, inventoryStatus: 'LOWSTOCK', rating: 4 },
        { id: '1009', code: 'cm230f032', name: 'Gaming Set', description: 'Product Description', image: 'gaming-set.jpg', price: 299, category: 'Electronics', quantity: 63, inventoryStatus: 'INSTOCK', rating: 3 }
    ];
});

const dt = ref();
const products = ref([]);
const productDialog = ref(false);
const deleteProductDialog = ref(false);
const deleteProductsDialog = ref(false);
const product = ref({});
const selectedProducts = ref();
const globalFilter = ref('');
const submitted = ref(false);
const statuses = ref([
    { label: 'INSTOCK', value: 'instock' },
    { label: 'LOWSTOCK', value: 'lowstock' },
    { label: 'OUTOFSTOCK', value: 'outofstock' }
]);

function formatCurrency(value) {
    if (value) return value.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
    return;
}

function openNew() {
    product.value = {};
    submitted.value = false;
    productDialog.value = true;
}

function hideDialog() {
    productDialog.value = false;
    submitted.value = false;
}

function saveProduct() {
    submitted.value = true;

    if (product?.value.name?.trim()) {
        if (product.value.id) {
            product.value.inventoryStatus = product.value.inventoryStatus.value ? product.value.inventoryStatus.value : product.value.inventoryStatus;
            products.value[findIndexById(product.value.id)] = product.value;
            toast.success('Product Updated', { description: 'Successful' });
        } else {
            product.value.id = createId();
            product.value.code = createId();
            product.value.image = 'product-placeholder.svg';
            product.value.inventoryStatus = product.value.inventoryStatus ? product.value.inventoryStatus.value : 'INSTOCK';
            products.value.push(product.value);
            toast.success('Product Created', { description: 'Successful' });
        }

        productDialog.value = false;
        product.value = {};
    }
}

function editProduct(prod) {
    product.value = { ...prod };
    productDialog.value = true;
}

function confirmDeleteProduct(prod) {
    product.value = prod;
    deleteProductDialog.value = true;
}

function deleteProduct() {
    products.value = products.value.filter((val) => val.id !== product.value.id);
    deleteProductDialog.value = false;
    product.value = {};
    toast.success('Product Deleted', { description: 'Successful' });
}

function findIndexById(id) {
    let index = -1;
    for (let i = 0; i < products.value.length; i++) {
        if (products.value[i].id === id) {
            index = i;
            break;
        }
    }

    return index;
}

function createId() {
    let id = '';
    var chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    for (var i = 0; i < 5; i++) {
        id += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return id;
}

function exportCSV() {
    dt.value.exportCSV();
}

function confirmDeleteSelected() {
    deleteProductsDialog.value = true;
}

function deleteSelectedProducts() {
    products.value = products.value.filter((val) => !selectedProducts.value.includes(val));
    deleteProductsDialog.value = false;
    selectedProducts.value = [];
    toast.success('Products Deleted', { description: 'Successful' });
}

function getStatusLabel(status) {
    switch (status) {
        case 'INSTOCK':
            return 'default';

        case 'LOWSTOCK':
            return 'secondary';

        case 'OUTOFSTOCK':
            return 'destructive';

        default:
            return 'outline';
    }
}

// TanStack table setup
const columns = [
    {
        id: 'select',
        header: () => {
            // Setup checkbox
            return {};
        },
        cell: () => {
            return {};
        }
    },
    {
        accessorKey: 'code',
        header: 'Code'
    },
    {
        accessorKey: 'name',
        header: 'Name'
    },
    {
        accessorKey: 'image',
        header: 'Image'
    },
    {
        accessorKey: 'price',
        header: 'Price',
        cell: (info) => formatCurrency(info.getValue())
    },
    {
        accessorKey: 'category',
        header: 'Category'
    },
    {
        accessorKey: 'rating',
        header: 'Reviews'
    },
    {
        accessorKey: 'inventoryStatus',
        header: 'Status'
    },
    {
        id: 'actions',
        header: 'Actions'
    }
];

const sorting = ref([]);
const rowSelection = ref({});

const table = useVueTable({
    get data() {
        return products.value;
    },
    columns,
    state: {
        get sorting() {
            return sorting.value;
        },
        get globalFilter() {
            return globalFilter.value;
        },
        get rowSelection() {
            return rowSelection.value;
        }
    },
    onSortingChange: (updaterOrValue) => {
        sorting.value = typeof updaterOrValue === 'function' ? updaterOrValue(sorting.value) : updaterOrValue;
    },
    onGlobalFilterChange: (setFilter) => {
        globalFilter.value = typeof setFilter === 'function' ? setFilter(globalFilter.value) : setFilter;
    },
    onRowSelectionChange: (updaterOrValue) => {
        rowSelection.value = typeof updaterOrValue === 'function' ? updaterOrValue(rowSelection.value) : updaterOrValue;
        selectedProducts.value = table.getSelectedRowModel().rows.map((r) => r.original);
    },
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel()
});
</script>

<template>
    <div>
        <div class="card bg-card p-6 rounded-lg border shadow-sm">
            <div class="flex flex-wrap gap-2 items-center justify-between mb-6">
                <div class="flex gap-2">
                    <Button variant="default" @click="openNew" class="gap-2"> <Plus class="w-4 h-4" /> New </Button>
                    <Button variant="destructive" @click="confirmDeleteSelected" :disabled="!selectedProducts || !selectedProducts.length" class="gap-2"> <Trash2 class="w-4 h-4" /> Delete </Button>
                </div>

                <div class="flex gap-2 items-center">
                    <Button variant="outline" @click="exportCSV($event)" class="gap-2"> <Upload class="w-4 h-4" /> Export </Button>
                    <div class="relative">
                        <Search class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
                        <Input v-model="globalFilter" placeholder="Search..." class="pl-10" />
                    </div>
                </div>
            </div>

            <div class="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow v-for="headerGroup in table.getHeaderGroups()" :key="headerGroup.id">
                            <TableHead v-for="header in headerGroup.headers" :key="header.id" :colSpan="header.colSpan" class="cursor-pointer select-none" @click="header.column.getToggleSortingHandler()?.($event)">
                                <div class="flex items-center gap-2">
                                    <template v-if="header.id === 'select'">
                                        <Checkbox :checked="table.getIsAllPageRowsSelected() || (table.getIsSomePageRowsSelected() && 'indeterminate')" @update:checked="(value) => table.toggleAllPageRowsSelected(!!value)" aria-label="Select all" />
                                    </template>
                                    <template v-else-if="!header.isPlaceholder">
                                        {{ header.column.columnDef.header }}
                                        <ArrowUp v-if="header.column.getIsSorted() === 'asc'" class="w-3 h-3 ml-1" />
                                        <ArrowDown v-else-if="header.column.getIsSorted() === 'desc'" class="w-3 h-3 ml-1" />
                                    </template>
                                </div>
                            </TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        <TableRow v-for="row in table.getRowModel().rows" :key="row.id" :data-state="row.getIsSelected() ? 'selected' : undefined">
                            <TableCell v-for="cell in row.getVisibleCells()" :key="cell.id">
                                <template v-if="cell.column.id === 'select'">
                                    <Checkbox :checked="row.getIsSelected()" @update:checked="(value) => row.toggleSelected(!!value)" aria-label="Select row" />
                                </template>
                                <template v-else-if="cell.column.id === 'image'">
                                    <!-- <img :src="`https://primefaces.org/cdn/primevue/images/product/${row.original.image}`" :alt="row.original.image" class="rounded" style="width: 64px" /> -->
                                    <div class="h-10 w-10 bg-muted rounded-md flex items-center justify-center text-muted-foreground">
                                        <ImageIcon class="w-6 h-6" />
                                    </div>
                                </template>
                                <template v-else-if="cell.column.id === 'price'">
                                    {{ formatCurrency(row.original.price) }}
                                </template>
                                <template v-else-if="cell.column.id === 'rating'">
                                    <!-- Simple star rating placeholder -->
                                    <div class="flex text-yellow-500">
                                        <Star v-for="i in 5" :key="i" class="w-4 h-4 mr-1" :class="i <= Math.round(row.original.rating) ? 'fill-current' : ''" />
                                    </div>
                                </template>
                                <template v-else-if="cell.column.id === 'inventoryStatus'">
                                    <Badge :variant="getStatusLabel(row.original.inventoryStatus)">
                                        {{ row.original.inventoryStatus }}
                                    </Badge>
                                </template>
                                <template v-else-if="cell.column.id === 'actions'">
                                    <div class="flex gap-2">
                                        <Button variant="ghost" size="icon" class="h-8 w-8 rounded-full" @click="editProduct(row.original)">
                                            <Pencil class="w-4 h-4" />
                                        </Button>
                                        <Button variant="ghost" size="icon" class="h-8 w-8 rounded-full text-destructive hover:text-destructive hover:bg-destructive/10" @click="confirmDeleteProduct(row.original)">
                                            <Trash2 class="w-4 h-4" />
                                        </Button>
                                    </div>
                                </template>
                                <template v-else>
                                    {{ row.getValue(cell.column.id) }}
                                </template>
                            </TableCell>
                        </TableRow>
                        <TableRow v-if="table.getRowModel().rows.length === 0">
                            <TableCell :colspan="columns.length" class="h-24 text-center"> No results. </TableCell>
                        </TableRow>
                    </TableBody>
                </Table>
            </div>

            <div class="flex items-center justify-between mt-4">
                <div class="flex-1 text-sm text-muted-foreground">{{ table.getFilteredSelectedRowModel().rows.length }} of {{ table.getFilteredRowModel().rows.length }} row(s) selected.</div>
                <div class="flex items-center space-x-2">
                    <Button variant="outline" size="sm" @click="table.previousPage()" :disabled="!table.getCanPreviousPage()"> Previous </Button>
                    <Button variant="outline" size="sm" @click="table.nextPage()" :disabled="!table.getCanNextPage()"> Next </Button>
                </div>
            </div>
        </div>

        <Dialog :open="productDialog" @update:open="productDialog = $event">
            <DialogContent class="sm:max-w-[450px]">
                <DialogHeader>
                    <DialogTitle>Product Details</DialogTitle>
                </DialogHeader>
                <div class="flex flex-col gap-6 py-4">
                    <div v-if="product.image && product.image !== 'product-placeholder.svg'" class="h-[150px] w-[150px] m-auto mb-4 bg-muted rounded-md flex items-center justify-center text-muted-foreground">
                        <ImageIcon class="w-10 h-10 text-muted-foreground" />
                    </div>

                    <div class="space-y-2">
                        <Label for="name" class="font-bold">Name</Label>
                        <Input id="name" v-model.trim="product.name" required autofocus :class="{ 'border-red-500': submitted && !product.name }" />
                        <small v-if="submitted && !product.name" class="text-red-500">Name is required.</small>
                    </div>

                    <div class="space-y-2">
                        <Label for="description" class="font-bold">Description</Label>
                        <Textarea id="description" v-model="product.description" required rows="3" />
                    </div>

                    <div class="space-y-2">
                        <Label for="inventoryStatus" class="font-bold">Inventory Status</Label>
                        <Select v-model="product.inventoryStatus">
                            <SelectTrigger>
                                <SelectValue placeholder="Select a Status" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectGroup>
                                    <SelectItem v-for="status in statuses" :key="status.value" :value="status.value">
                                        {{ status.label }}
                                    </SelectItem>
                                </SelectGroup>
                            </SelectContent>
                        </Select>
                    </div>

                    <div class="space-y-3">
                        <Label class="font-bold">Category</Label>
                        <RadioGroup v-model="product.category" class="grid grid-cols-2 gap-4">
                            <div class="flex items-center space-x-2">
                                <RadioGroupItem id="category1" value="Accessories" />
                                <Label for="category1" class="font-normal">Accessories</Label>
                            </div>
                            <div class="flex items-center space-x-2">
                                <RadioGroupItem id="category2" value="Clothing" />
                                <Label for="category2" class="font-normal">Clothing</Label>
                            </div>
                            <div class="flex items-center space-x-2">
                                <RadioGroupItem id="category3" value="Electronics" />
                                <Label for="category3" class="font-normal">Electronics</Label>
                            </div>
                            <div class="flex items-center space-x-2">
                                <RadioGroupItem id="category4" value="Fitness" />
                                <Label for="category4" class="font-normal">Fitness</Label>
                            </div>
                        </RadioGroup>
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="space-y-2">
                            <Label for="price" class="font-bold">Price</Label>
                            <div class="relative">
                                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">$</span>
                                <Input id="price" type="number" v-model="product.price" class="pl-7" />
                            </div>
                        </div>
                        <div class="space-y-2">
                            <Label for="quantity" class="font-bold">Quantity</Label>
                            <Input id="quantity" type="number" v-model="product.quantity" />
                        </div>
                    </div>
                </div>

                <DialogFooter>
                    <Button variant="outline" @click="hideDialog">Cancel</Button>
                    <Button @click="saveProduct">Save</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <Dialog :open="deleteProductDialog" @update:open="deleteProductDialog = $event">
            <DialogContent class="sm:max-w-[450px]">
                <DialogHeader>
                    <DialogTitle>Confirm</DialogTitle>
                </DialogHeader>
                <div class="flex items-center gap-4 py-4">
                    <AlertTriangle class="w-10 h-10 text-yellow-500" />
                    <span v-if="product">
                        Are you sure you want to delete <b>{{ product.name }}</b
                        >?
                    </span>
                </div>
                <DialogFooter>
                    <Button variant="outline" @click="deleteProductDialog = false">No</Button>
                    <Button variant="destructive" @click="deleteProduct">Yes</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>

        <Dialog :open="deleteProductsDialog" @update:open="deleteProductsDialog = $event">
            <DialogContent class="sm:max-w-[450px]">
                <DialogHeader>
                    <DialogTitle>Confirm</DialogTitle>
                </DialogHeader>
                <div class="flex items-center gap-4 py-4">
                    <AlertTriangle class="w-10 h-10 text-yellow-500" />
                    <span v-if="product">Are you sure you want to delete the selected products?</span>
                </div>
                <DialogFooter>
                    <Button variant="outline" @click="deleteProductsDialog = false">No</Button>
                    <Button variant="destructive" @click="deleteSelectedProducts">Yes</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    </div>
</template>
