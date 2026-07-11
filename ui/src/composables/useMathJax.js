// Lazily loads a self-hosted MathJax build (ui/public/vendor/mathjax/tex-chtml.js)
// and exposes a typeset() helper. Self-hosted rather than an npm dependency
// because MathJax's component architecture doesn't bundle cleanly with Vite;
// its own docs recommend consuming the combined build via a <script> tag.
// Pinned to MathJax 3's plain tex-chtml.js (not v4, and not tex-chtml-full):
// v4's combined bundle unconditionally spins up a speech/accessibility Worker
// that we don't vendor, and its failure hangs typesetPromise() forever.
let loadPromise = null;

function loadMathJax() {
    if (loadPromise) return loadPromise;

    loadPromise = new Promise((resolve, reject) => {
        if (window.MathJax && window.MathJax.typesetPromise) {
            resolve(window.MathJax);
            return;
        }

        window.MathJax = {
            tex: {
                inlineMath: [['$', '$']],
                displayMath: [['$$', '$$']]
            },
            options: {
                skipHtmlTags: ['script', 'noscript', 'style', 'textarea', 'pre', 'code'],
                enableMenu: false
            },
            startup: {
                typeset: false,
                ready() {
                    window.MathJax.startup.defaultReady();
                    resolve(window.MathJax);
                }
            }
        };

        const script = document.createElement('script');
        script.src = '/vendor/mathjax/tex-chtml.js';
        script.async = true;
        script.onerror = () => reject(new Error('Failed to load MathJax'));
        document.head.appendChild(script);
    });

    return loadPromise;
}

// MathJax's typesetPromise isn't safe to invoke concurrently -- if multiple
// report widgets mount around the same tick, overlapping calls can drop
// output for all but one of them. Serialize every call through one shared
// queue so each waits for the previous to fully finish first.
let typesetQueue = Promise.resolve();

export function useMathJax() {
    function typeset(elements) {
        const result = typesetQueue.then(() => loadMathJax()).then((MathJax) => MathJax.typesetPromise(elements));
        // Keep the shared queue alive even if this call fails, so one broken
        // typeset doesn't wedge every report widget queued after it.
        typesetQueue = result.catch(() => {});
        return result;
    }

    return { typeset };
}
