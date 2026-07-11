<script setup>
import { ref, watch, nextTick } from 'vue';
import { marked } from 'marked';
import DOMPurify from 'dompurify';
import { useMathJax } from '@/composables/useMathJax';

const props = defineProps({
    content: { type: String, default: '' }
});

const containerRef = ref(null);
const renderedHtml = ref('');
const { typeset } = useMathJax();

// marked's own escaping rules eat backslashes and underscores (e.g. \beta_0
// becomes eta_0) before MathJax ever sees them, since both are meaningful
// Markdown syntax too. Pull $...$/$$...$$ segments out before parsing and
// splice the original, untouched LaTeX back in afterward.
function protectMath(markdown) {
    const placeholders = [];
    const replaced = markdown.replace(/\$\$([\s\S]+?)\$\$|\$([^\n$]+?)\$/g, (match) => {
        placeholders.push(match);
        return `@@MATHJAX_PLACEHOLDER_${placeholders.length - 1}@@`;
    });
    return { replaced, placeholders };
}

function restoreMath(html, placeholders) {
    return html.replace(/@@MATHJAX_PLACEHOLDER_(\d+)@@/g, (_, i) =>
        placeholders[Number(i)].replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    );
}

async function render() {
    const { replaced, placeholders } = protectMath(props.content || '');
    const rawHtml = restoreMath(marked.parse(replaced, { breaks: true }), placeholders);
    renderedHtml.value = DOMPurify.sanitize(rawHtml);
    await nextTick();
    if (containerRef.value) {
        try {
            await typeset([containerRef.value]);
        } catch (e) {
            console.error('MathJax typeset failed:', e);
        }
    }
}

watch(() => props.content, render, { immediate: true });
</script>

<template>
    <div ref="containerRef" class="markdown-report" v-html="renderedHtml"></div>
</template>

<style scoped>
.markdown-report {
    font-size: 0.875rem;
    line-height: 1.6;
    color: hsl(var(--foreground));
}
.markdown-report :deep(h1) {
    font-size: 1.5rem;
    font-weight: 700;
    margin: 0.75rem 0 0.5rem;
}
.markdown-report :deep(h2) {
    font-size: 1.25rem;
    font-weight: 600;
    margin: 0.65rem 0 0.4rem;
}
.markdown-report :deep(h3) {
    font-size: 1.1rem;
    font-weight: 600;
    margin: 0.5rem 0 0.35rem;
}
.markdown-report :deep(p) {
    margin: 0.5rem 0;
}
.markdown-report :deep(ul),
.markdown-report :deep(ol) {
    margin: 0.5rem 0 0.5rem 1.5rem;
}
.markdown-report :deep(li) {
    margin: 0.15rem 0;
}
.markdown-report :deep(code) {
    background: hsl(var(--muted));
    padding: 0.1rem 0.3rem;
    border-radius: 0.25rem;
    font-size: 0.85em;
}
.markdown-report :deep(pre) {
    background: hsl(var(--muted));
    padding: 0.75rem;
    border-radius: 0.375rem;
    overflow-x: auto;
    margin: 0.5rem 0;
}
.markdown-report :deep(pre code) {
    background: transparent;
    padding: 0;
}
.markdown-report :deep(blockquote) {
    border-left: 3px solid hsl(var(--border));
    padding-left: 0.75rem;
    color: hsl(var(--muted-foreground));
    margin: 0.5rem 0;
}
.markdown-report :deep(table) {
    border-collapse: collapse;
    width: 100%;
    margin: 0.5rem 0;
    font-size: 0.85em;
}
.markdown-report :deep(th),
.markdown-report :deep(td) {
    border: 1px solid hsl(var(--border));
    padding: 0.35rem 0.6rem;
    text-align: left;
}
.markdown-report :deep(th) {
    background: hsl(var(--muted));
}
.markdown-report :deep(a) {
    color: hsl(var(--primary));
    text-decoration: underline;
}
.markdown-report :deep(hr) {
    border: none;
    border-top: 1px solid hsl(var(--border));
    margin: 0.75rem 0;
}
.markdown-report :deep(img) {
    max-width: 100%;
}
</style>
