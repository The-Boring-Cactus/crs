<template>
    <div>
        <div class="card">
             <Toolbar class="mb-6">
                <template #start>
                    <Button label="New" icon="pi pi-plus" severity="secondary" class="mr-2"  />
                    <Button label="Delete" icon="pi pi-trash" severity="secondary"  />
                </template>
            </Toolbar>
            <DataTable></DataTable>
        </div>
    </div>
</template>

<script setup>
import { FilterMatchMode } from '@primevue/core/api';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';

const toast = useToast();
const dt = ref();

</script>