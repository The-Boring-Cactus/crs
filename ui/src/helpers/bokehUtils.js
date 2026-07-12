/**
 * bokehUtils.js — Builds Bokeh 3.x "standalone embed item" JSON documents for
 * BokehChart.vue, from the same simplified { labels, datasets } shape used by
 * BaseChart.vue (Chart.js/ECharts-style data).
 *
 * Bokeh's wire format (as emitted by Python's bokeh.embed.json_item()) is an
 * undocumented, version-specific internal protocol: every model reference is
 * `{ type: 'object', name: '<ModelName>', id, attributes }` on first use and a
 * bare `{ id }` on reuse, glyph attributes are tagged `{ type: 'field'|'value', ... }`,
 * and column data is `{ type: 'map', entries: [[key, value], ...] }`. This was
 * reverse-engineered by diffing hand-built JSON against real `json_item()`
 * output from Bokeh 3.9.1 (matching the vendored BokehJS build) — BokehJS's
 * decoder rejects the older flat-`references`-array format used by pre-3.0
 * Bokeh, which is the format this file used to (incorrectly) produce.
 */

let _idCounter = 1000;
function uid() {
    return `p${++_idCounter}`;
}

const CATEGORY10 = [
    '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd',
    '#8c564b', '#e377c2', '#7f7f7f', '#bcbd22', '#17becf'
];

function model(name, attributes = {}) {
    const node = { type: 'object', name, id: uid() };
    if (attributes && Object.keys(attributes).length > 0) node.attributes = attributes;
    return node;
}
function ref(node) {
    return { id: node.id };
}
function field(f) {
    return { type: 'field', field: f };
}
function value(v) {
    return { type: 'value', value: v };
}

/**
 * Build a standalone Bokeh embed item from simplified chart data.
 *
 * @param {Object} options
 * @param {string} options.type - 'line' | 'bar' | 'bar-h' | 'scatter' | 'area' | 'pie' | 'doughnut'
 * @param {string[]} options.labels - Category labels for the X axis
 * @param {Object[]} options.datasets - Array of { label, data, color? }
 * @param {string} [options.title] - Plot title
 * @param {number} [options.width=600]
 * @param {number} [options.height=400]
 * @returns {Object} Bokeh embed item — pass to `Bokeh.embed.embed_item(item, targetId)`
 */
export function buildBokehJson(options) {
    const { type, labels = [], datasets = [], title = '', width = 600, height = 400 } = options;

    const isBar = type === 'bar';
    const isBarH = type === 'bar-h';
    const isPie = type === 'pie' || type === 'doughnut';
    // Scatter data can arrive either as plain numbers aligned to `labels`
    // (CsEditor's Chart() output) or as {x, y} point pairs (SqlEditor's
    // scatter viz) — BaseChart.vue handles the same ambiguity the same way.
    const isXYScatter = type === 'scatter' && datasets[0]?.data?.length > 0 && typeof datasets[0].data[0] === 'object';

    // ── Ranges & scales ──
    // Labels are always category strings in this simplified data format, so
    // every chart type puts a FactorRange on the "labels" axis — x for
    // everything except horizontal bars, which flip labels onto y instead.
    // {x, y} scatter data is truly numeric on both axes instead.
    const categoricalX = !isBarH && !isPie && !isXYScatter;
    const categoricalY = isBarH && !isPie;

    const xRange = isPie
        ? model('Range1d', { start: -1.1, end: 1.1 })
        : categoricalX
            ? model('FactorRange', { factors: labels.slice() })
            : model('DataRange1d', {});
    const yRange = isPie
        ? model('Range1d', { start: -1.1, end: 1.1 })
        : categoricalY
            ? model('FactorRange', { factors: labels.slice() })
            : model('DataRange1d', {});
    const xScale = model(categoricalX ? 'CategoricalScale' : 'LinearScale');
    const yScale = model(categoricalY ? 'CategoricalScale' : 'LinearScale');

    const renderers = [];
    const legendItems = [];

    if (isPie) {
        const values = (datasets[0]?.data || []).map(Number);
        const total = values.reduce((a, b) => a + b, 0) || 1;
        const colors = labels.map((_, i) => CATEGORY10[i % CATEGORY10.length]);

        let cum = 0;
        const starts = [];
        const ends = [];
        values.forEach((v) => {
            starts.push((cum / total) * 2 * Math.PI);
            cum += v;
            ends.push((cum / total) * 2 * Math.PI);
        });

        const cds = model('ColumnDataSource', {
            data: {
                type: 'map',
                entries: [
                    ['labels', labels.slice()],
                    ['values', values],
                    ['start', starts],
                    ['end', ends],
                    ['colors', colors]
                ]
            }
        });

        const glyph = model('AnnularWedge', {
            x: value(0),
            y: value(0),
            inner_radius: value(type === 'doughnut' ? 0.45 : 0),
            outer_radius: value(0.85),
            start_angle: field('start'),
            end_angle: field('end'),
            fill_color: field('colors'),
            line_color: value('#ffffff'),
            line_width: value(2)
        });

        const renderer = model('GlyphRenderer', { data_source: cds, glyph });
        renderers.push(renderer);

        labels.forEach((label, i) => {
            legendItems.push(model('LegendItem', { label: value(label), renderers: [ref(renderer)], index: i }));
        });
    } else if (isXYScatter) {
        // {x, y} point-pair scatter: one CDS + Scatter glyph per series,
        // each with its own x/y columns (no shared category axis).
        datasets.forEach((ds, i) => {
            const color = ds.color || ds.borderColor ||
                (Array.isArray(ds.backgroundColor) ? ds.backgroundColor[0] : ds.backgroundColor) ||
                CATEGORY10[i % CATEGORY10.length];
            const label = ds.label || `Series ${i + 1}`;
            const points = ds.data || [];

            const cds = model('ColumnDataSource', {
                data: {
                    type: 'map',
                    entries: [
                        ['x', points.map((p) => Number(p.x))],
                        ['y', points.map((p) => Number(p.y))]
                    ]
                }
            });
            const glyph = model('Scatter', {
                x: field('x'), y: field('y'),
                size: value(10),
                fill_color: value(color), line_color: value('#ffffff'), fill_alpha: value(0.75)
            });
            const renderer = model('GlyphRenderer', { data_source: cds, glyph });
            renderers.push(renderer);
            legendItems.push(model('LegendItem', { label: value(label), renderers: [ref(renderer)] }));
        });
    } else {
        // Shared CDS across all series (line/bar/bar-h/area/scatter)
        const dataEntries = [['labels', labels.slice()]];
        datasets.forEach((ds, i) => {
            dataEntries.push([`y${i}`, (ds.data || []).map(Number)]);
        });
        const cds = model('ColumnDataSource', { data: { type: 'map', entries: dataEntries } });

        datasets.forEach((ds, i) => {
            const colName = `y${i}`;
            const color = ds.color || ds.borderColor ||
                (Array.isArray(ds.backgroundColor) ? ds.backgroundColor[0] : ds.backgroundColor) ||
                CATEGORY10[i % CATEGORY10.length];
            const label = ds.label || `Series ${i + 1}`;

            let glyph;
            if (type === 'scatter') {
                glyph = model('Scatter', {
                    x: field('labels'), y: field(colName),
                    size: value(10),
                    fill_color: value(color), line_color: value('#ffffff'), fill_alpha: value(0.75)
                });
            } else if (isBar) {
                glyph = model('VBar', {
                    x: field('labels'), top: field(colName), width: value(0.6),
                    fill_color: value(color), line_color: value('#ffffff')
                });
            } else if (isBarH) {
                glyph = model('HBar', {
                    y: field('labels'), right: field(colName), height: value(0.6),
                    fill_color: value(color), line_color: value('#ffffff')
                });
            } else if (type === 'area') {
                glyph = model('Patch', {
                    x: field('labels'), y: field(colName),
                    fill_color: value(color), fill_alpha: value(0.3),
                    line_color: value(color), line_width: value(2)
                });
            } else {
                // default: line (+ point markers)
                glyph = model('Line', {
                    x: field('labels'), y: field(colName),
                    line_color: value(color), line_width: value(2.5)
                });
            }

            const renderer = model('GlyphRenderer', { data_source: cds, glyph });
            renderers.push(renderer);

            if (type === 'line' || type === undefined) {
                const markerGlyph = model('Scatter', {
                    x: field('labels'), y: field(colName),
                    size: value(7), fill_color: value(color), line_color: value('#ffffff')
                });
                renderers.push(model('GlyphRenderer', { data_source: cds, glyph: markerGlyph }));
            }

            legendItems.push(model('LegendItem', { label: value(label), renderers: [ref(renderer)] }));
        });
    }

    // ── Axes & grids (pie charts hide them) ──
    const below = [];
    const left = [];
    const center = [];

    if (!isPie) {
        const xAxis = model(categoricalX ? 'CategoricalAxis' : 'LinearAxis');
        const yAxis = model(categoricalY ? 'CategoricalAxis' : 'LinearAxis');
        below.push(xAxis);
        left.push(yAxis);
        center.push(model('Grid', { axis: ref(xAxis) }));
        center.push(model('Grid', { dimension: 1, axis: ref(yAxis) }));
    }

    // ── Legend ──
    const showLegend = legendItems.length > 1 || (isPie && legendItems.length > 0);
    const right = [];
    if (showLegend) {
        right.push(model('Legend', {
            items: legendItems,
            location: 'top_left',
            orientation: isPie ? 'vertical' : 'horizontal',
            click_policy: 'hide'
        }));
    }

    // ── Toolbar ──
    const toolbar = model('Toolbar', {
        tools: [
            model('PanTool'),
            model('WheelZoomTool'),
            model('BoxZoomTool'),
            model('SaveTool'),
            model('ResetTool'),
            model('HoverTool')
        ]
    });

    const plotAttrs = {
        height,
        width,
        x_range: xRange,
        y_range: yRange,
        x_scale: xScale,
        y_scale: yScale,
        renderers,
        toolbar,
        toolbar_location: 'above',
        below,
        left,
        center,
        sizing_mode: 'stretch_both'
    };
    if (title) plotAttrs.title = model('Title', { text: title });
    if (right.length) plotAttrs.right = right;
    if (isPie) {
        plotAttrs.outline_line_color = null;
    }

    const plot = model('Plot', plotAttrs);

    return {
        root_id: plot.id,
        doc: {
            version: '3.9.1',
            title: title || '',
            defs: [],
            roots: [plot]
        }
    };
}

/**
 * Recursively walks a Bokeh embed item's document tree, invoking `visit(node)`
 * for every model node (`{ type: 'object', name, id, attributes }`). Safe to
 * call on the tree unconditionally — bare `{ id }` refs and leaf field/value
 * wrappers have no `name` and are simply skipped over.
 */
export function walkBokehModels(node, visit) {
    if (Array.isArray(node)) {
        node.forEach((n) => walkBokehModels(n, visit));
    } else if (node && typeof node === 'object') {
        if (node.type === 'object' && node.name) {
            visit(node);
            if (node.attributes) walkBokehModels(node.attributes, visit);
        } else {
            Object.values(node).forEach((v) => walkBokehModels(v, visit));
        }
    }
}

/**
 * Mutates a Bokeh embed item's document in place to apply dark-theme colors.
 */
export function applyBokehDarkTheme(item) {
    try {
        walkBokehModels(item?.doc?.roots, (node) => {
            const attrs = (node.attributes ??= {});
            if (node.name === 'Plot') {
                attrs.background_fill_color = '#18181b';
                attrs.border_fill_color = '#18181b';
                attrs.outline_line_color = '#27272a';
            } else if (['LinearAxis', 'CategoricalAxis', 'DatetimeAxis'].includes(node.name)) {
                attrs.axis_line_color = '#52525b';
                attrs.major_label_text_color = '#a1a1aa';
                attrs.major_tick_line_color = '#52525b';
            } else if (node.name === 'Grid') {
                attrs.grid_line_color = '#27272a';
            } else if (node.name === 'Legend') {
                attrs.background_fill_color = '#27272a';
                attrs.border_line_color = '#3f3f46';
                attrs.label_text_color = '#a1a1aa';
            } else if (node.name === 'Title') {
                attrs.text_color = '#f4f4f5';
            }
        });
    } catch (e) {
        console.warn('Bokeh dark theme application warning:', e);
    }
}

/** Quick helper: generate a simple line chart Bokeh item. */
export function simpleLineChart(labels, values, title = '', color = '#1f77b4') {
    return buildBokehJson({ type: 'line', labels, datasets: [{ label: title || 'Value', data: values, color }], title });
}

/** Quick helper: generate a simple bar chart Bokeh item. */
export function simpleBarChart(labels, values, title = '', color = '#1f77b4') {
    return buildBokehJson({ type: 'bar', labels, datasets: [{ label: title || 'Value', data: values, color }], title });
}

/** Quick helper: generate a simple pie chart Bokeh item. */
export function simplePieChart(labels, values, title = '', doughnut = false) {
    return buildBokehJson({ type: doughnut ? 'doughnut' : 'pie', labels, datasets: [{ data: values }], title, width: 500, height: 450 });
}

/** Quick helper: generate a scatter plot Bokeh item. dataPoints: [{x, y}, ...] */
export function scatterPlot(dataPoints, title = '', color = '#d62728') {
    const labels = dataPoints.map((p) => String(p.x));
    const values = dataPoints.map((p) => p.y);
    return buildBokehJson({ type: 'scatter', labels, datasets: [{ label: title || 'Data', data: values, color }], title });
}

/** Quick helper: generate a simple area chart Bokeh item. */
export function simpleAreaChart(labels, values, title = '', color = '#2ca02c') {
    return buildBokehJson({ type: 'area', labels, datasets: [{ label: title || 'Value', data: values, color }], title });
}
