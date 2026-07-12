// Lazily loads the self-hosted BokehJS core build (ui/public/vendor/bokeh/bokeh.min.js)
// and resolves with the resulting `window.Bokeh` global. Self-hosted rather than
// an npm dependency for the same reason as MathJax (see useMathJax.js): there is
// no clean bundler-friendly build, and Bokeh's own docs recommend consuming the
// prebuilt UMD bundle via a <script> tag. Only the core bundle is vendored —
// BokehChart only calls `Bokeh.embed.embed_item()`, which doesn't need the
// separate widgets/tables/api companion bundles.
let loadPromise = null;

export function loadBokeh() {
    if (loadPromise) return loadPromise;

    loadPromise = new Promise((resolve, reject) => {
        if (window.Bokeh && window.Bokeh.embed) {
            resolve(window.Bokeh);
            return;
        }

        const script = document.createElement('script');
        script.src = '/vendor/bokeh/bokeh.min.js';
        script.async = true;
        script.onload = () => resolve(window.Bokeh);
        script.onerror = () => reject(new Error('Failed to load BokehJS'));
        document.head.appendChild(script);
    });

    return loadPromise;
}
