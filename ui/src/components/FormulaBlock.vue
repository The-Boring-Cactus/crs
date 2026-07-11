<script setup>
import { ref, watch, nextTick, computed } from 'vue';
import { useMathJax } from '@/composables/useMathJax';

const props = defineProps({
    latex: { type: String, default: '' },
    label: { type: String, default: '' }
});

const containerRef = ref(null);
const { typeset } = useMathJax();

const displayMath = computed(() => `$$${props.latex}$$`);

async function render() {
    await nextTick();
    if (containerRef.value) {
        try {
            await typeset([containerRef.value]);
        } catch (e) {
            console.error('MathJax typeset failed:', e);
        }
    }
}

watch(() => props.latex, render, { immediate: true });
</script>

<template>
    <div class="formula-block">
        <div ref="containerRef" class="formula-content">{{ displayMath }}</div>
        <div v-if="label" class="formula-label">{{ label }}</div>
    </div>
</template>

<style scoped>
.formula-block {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.35rem;
    padding: 1rem;
    background: hsl(var(--muted) / 0.4);
    border-radius: 0.5rem;
}
.formula-content {
    font-size: 1.1rem;
    overflow-x: auto;
    max-width: 100%;
}
.formula-label {
    font-size: 0.75rem;
    color: hsl(var(--muted-foreground));
    text-align: center;
}
</style>
