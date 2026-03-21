<script setup>
import AppConfigurator from '@/layout/AppConfigurator.vue';
import { Palette } from 'lucide-vue-next';
import { Button } from '@/components/ui/button';
</script>

<template>
    <div class="fixed flex gap-4 top-8 right-8 z-50">
        <div class="relative">
            <Button
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                type="button"
                variant="outline"
                size="icon"
                class="rounded-full w-10 h-10 shadow-md bg-background"
            >
                <Palette class="w-5 h-5" />
            </Button>
            <AppConfigurator />
        </div>
    </div>
</template>
