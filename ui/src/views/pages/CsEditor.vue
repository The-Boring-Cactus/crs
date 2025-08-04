<template>
          <Toast />
    <Toolbar>
        <template #start>
            <Button icon="pi pi-caret-left" class="mr-2" severity="secondary" text @click="handleUndo" />
            <Button icon="pi pi-caret-right" class="mr-2" severity="secondary" text @click="handleRedo" />
            <Button icon="pi pi-save" class="mr-2" severity="secondary" text @click="handleSave" />
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
            <codemirror v-model="code" :style="{
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
    <div class="card flex flex-wrap justify-center items-end gap-4">
    <FloatLabel variant="in">
            <Textarea id="over_label" v-model="debugText" rows="5" cols="150" style="resize: none" />
            <label for="in_label">Debug Status</label>
        </FloatLabel>
        </div>
</template>

<script setup>
import { reactive, shallowRef, ref } from 'vue';
import { redo, undo } from '@codemirror/commands';
import { Codemirror } from 'vue-codemirror';
import { csharp } from '@replit/codemirror-lang-csharp';
import { getCurrentInstance } from 'vue';
import { useToast } from "primevue/usetoast";
const toast = useToast();

const debugText = ref('');

const { proxy } = getCurrentInstance();

// State
const MyTitle = ref();
const code = ref('');
const cmView = shallowRef();
const config = reactive({
    disabled: false,
    indentWithTab: true,
    tabSize: 1,
    autofocus: true,
    height: '500px'
});

proxy.$socket.onmessage =  (data) => {
  const payload = JSON.parse(data.data);
  console.log(payload);
  if(payload.TypeMsg=="FinishCode"){
    if(payload.status==="Fail"){
        toast.add({ severity: 'error', summary: 'Error', detail: 'An error occurred', life: 3000 });
    }
  }
  if(payload.TypeMsg=="Debug"){
    debugText.value = debugText.value + '\n' + payload.data;
  }
}


// This state is updated but not displayed. It can be used for debugging or status bars.
const state = reactive({
    lines: null,
    cursor: null,
    selected: null,
    length: null
});

// Constants
const log = console.log
const extensions = [csharp()];

// Handlers
const handleReady = ({ view }) => {
    cmView.value = view;
};

const handleSave = () => {
    debugText.value ='';
     proxy.$socket.sendObj({
          type: "CodeScript",
          data: code.value,
          name: MyTitle.value
        });
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

</style>