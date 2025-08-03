<template>
    <Toolbar>
        <template #start>
            <Button icon="pi pi-caret-left" class="mr-2" severity="secondary" text @click="handleUndo" />
            <Button icon="pi pi-caret-right" class="mr-2" severity="secondary" text @click="handleRedo" />
        </template>
        <template #center>
        <Inplace>
            <template #display>
                {{ MyTitle || 'No Name' }}
            </template>
            <template #content="{ closeCallback }">
                <span class="inline-flex items-center gap-2">
                    <InputText v-model="MyTitle" autofocus />
                    <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
                </span>
            </template>
        </Inplace>
        </template>
    </Toolbar>
    <div class="editor">
        <div class="main">
            <codemirror
                v-model="code"
                :style="{
          width: '100%',
          height: config.height,
          backgroundColor: '#fff',
          color: '#333'
        }"
        placeholder="Please enter the code."
        :extensions="extensions"
        :autofocus="config.autofocus"
        :disabled="config.disabled"
        :indent-with-tab="config.indentWithTab"
        :tab-size="config.tabSize"
        @update="handleStateUpdate"
        @ready="handleReady"
        @focus="log('focus', $event)"
        @blur="log('blur', $event)"
            />
        </div>
    </div>
    <div style="width: 100%">
        <vue-excel-editor v-model="jsondata" readonly no-footer no-header-edit disable-panel-setting disable-panel-filter no-sorting />
    </div>
</template>

<script setup>
import { reactive, shallowRef, ref } from 'vue';
import { redo, undo } from '@codemirror/commands';
import { Codemirror } from 'vue-codemirror';
import { sql } from '@codemirror/lang-sql';

// State
const MyTitle = ref();
const code = ref('');
const cmView = shallowRef();
const config = reactive({
    disabled: false,
    indentWithTab: true,
    tabSize: 1,
    autofocus: true,
    height: '200px'
});
const jsondata = [
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' },
    { c0: '', c1: '', c2: '', c3: '', c4: '', c5: '', c6: '' }
];

// This state is updated but not displayed. It can be used for debugging or status bars.
const state = reactive({
    lines: null,
    cursor: null,
    selected: null,
    length: null
});

// Constants
const log = console.log;
const extensions = [sql()];

// Handlers
const handleReady = ({ view }) => {
    cmView.value = view;
};

const handleUndo = () => {
    if (!cmView.value) return;
    undo({
        state: cmView.value.state,
        dispatch: cmView.value.dispatch
    });
};

const handleRedo = () => {
    if (!cmView.value) return;
    redo({
        state: cmView.value.state,
        dispatch: cmView.value.dispatch
    });
};

const handleStateUpdate = (viewUpdate) => {
    // Update selection, cursor, and document information
    const ranges = viewUpdate.state.selection.ranges;
    state.selected = ranges.reduce((plus, range) => plus + range.to - range.from, 0);
    state.cursor = ranges[0].anchor;
    state.length = viewUpdate.state.doc.length;
    state.lines = viewUpdate.state.doc.lines;
    // log('viewUpdate', viewUpdate)
};
</script>
<style lang="scss" scoped>
  .editor {

    .main {
      display: flex;
      width: 100%;

      .code {
        width: 30%;
        height: 100px;
        margin: 0;
        padding: 0.4em;
        overflow: scroll;
        font-family: monospace;
      }
    }

    .footer {
      height: 1rem;
      padding: 0 1em;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 90%;
      background-color: white;

      
      .infos {
        .item {
          margin-left: 1em;
          display: inline-block;
          font-feature-settings: 'tnum';
        }
      }
    }
  }
</style>