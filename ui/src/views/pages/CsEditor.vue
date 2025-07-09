<template>
  <Toolbar>
    <template #start>
      
        <Button icon="pi pi-caret-left" class="mr-2" severity="secondary" text  @click="handleUndo"/>
        <Button icon="pi pi-caret-right" class="mr-2" severity="secondary" text   @click="handleRedo"/>
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
        :mode="config.mode"
        :language="config.language"
        @update="handleStateUpdate"
        @ready="handleReady"
        @focus="log('focus', $event)"
        @blur="log('blur', $event)"
      />
      
    </div>
   
  </div>

</template>


<script setup>
  import { defineComponent, reactive, shallowRef, computed, watch, onMounted, ref } from 'vue'
  import { EditorView, ViewUpdate } from '@codemirror/view'
  import { redo, undo } from '@codemirror/commands'
  import { Codemirror } from 'vue-codemirror'
  import { csharp } from "@replit/codemirror-lang-csharp";
const MyTitle = ref();
const log = console.log
const code =ref('')
const extensions =  [csharp()]
  const config = reactive({
        disabled: false,
        indentWithTab: true,
        mode: "javascript",
        tabSize: 1,
        autofocus: true,
        height: '500px',
        language: 'csharp'
      })
    


   

      const cmView = shallowRef({EditorView})
      const handleReady = ({ view }) => {
        cmView.value = view
      }

      const handleUndo = () => {
        undo({
          state: cmView.value.state,
          dispatch: cmView.value.dispatch
        })
      }

      const handleRedo = () => {
        redo({
          state: cmView.value.state,
          dispatch: cmView.value.dispatch
        })
      }

      const state = reactive({
        lines: null,
        cursor: null,
        selected: null,
        length: null 
      })

      const handleStateUpdate = (viewUpdate) => {
        // selected
        const ranges = viewUpdate.state.selection.ranges
        state.selected = ranges.reduce((plus, range) => plus + range.to - range.from, 0)
        state.cursor = ranges[0].anchor
        // length
        state.length = viewUpdate.state.doc.length
        state.lines = viewUpdate.state.doc.lines
        // log('viewUpdate', viewUpdate)
      }



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