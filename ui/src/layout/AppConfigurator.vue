<script setup>
import { useLayout } from '@/layout/composables/layout';
import { ref } from 'vue';

const { layoutConfig } = useLayout();

const menuMode = ref(layoutConfig.menuMode);
const menuModeOptions = ref([
    { label: 'Static', value: 'static' },
    { label: 'Overlay', value: 'overlay' }
]);

function onMenuModeChange() {
    layoutConfig.menuMode = menuMode.value;
}
</script>

<template>
    <div class="config-panel hidden absolute top-[3.25rem] right-0 w-64 p-4 bg-background border rounded-md origin-top shadow-md z-50 text-foreground">
        <div class="flex flex-col gap-4">
            <div class="flex flex-col gap-2">
                <span class="text-sm text-muted-foreground font-semibold">Menu Mode</span>
                <div class="flex bg-muted p-1 rounded-md mb-2 w-fit">
                    <button
                        v-for="opt in menuModeOptions"
                        :key="opt.value"
                        @click="
                            menuMode = opt.value;
                            onMenuModeChange();
                        "
                        class="px-3 py-1 text-sm font-medium rounded-sm transition-colors"
                        :class="menuMode === opt.value ? 'bg-background text-foreground shadow-sm' : 'text-muted-foreground hover:bg-muted-foreground/10'"
                    >
                        {{ opt.label }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
