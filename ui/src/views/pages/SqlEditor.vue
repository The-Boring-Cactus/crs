<template>
  <div class="editor">
     <Menubar :model="items" />
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
    <div class="footer">
      
      <div class="infos">
        <span class="item">Spaces: {{ config.tabSize }}</span>
        <span class="item">Length: {{ state.length }}</span>
        <span class="item">Lines: {{ state.lines }}</span>
        <span class="item">Cursor: {{ state.cursor }}</span>
        <span class="item">Selected: {{ state.selected }}</span>
      </div>
    </div>
    <br/>
  </div>
   <DataTable :value="products" tableStyle="min-width: 50rem">
        <Column v-for="col of columns" :key="col.field" :field="col.field" :header="col.header"></Column>
    </DataTable>
</template>


<script setup>
  import { defineComponent, reactive, shallowRef, computed, watch, onMounted, ref } from 'vue'
  import { EditorView, ViewUpdate } from '@codemirror/view'
  import { redo, undo } from '@codemirror/commands'
  import { Codemirror } from 'vue-codemirror'
  import { sql } from '@codemirror/lang-sql'

const log = console.log
const code =ref('')
const extensions =  [sql()]
  const config = reactive({
        disabled: false,
        indentWithTab: true,
        tabSize: 1,
        autofocus: true,
        height: '200px',
        language: 'sql'
      })
    
const products = [
                {
                    id: '1000',
                    code: 'f230fh0g3',
                    name: 'Bamboo Watch',
                    description: 'Product Description',
                    image: 'bamboo-watch.jpg',
                    price: 65,
                    category: 'Accessories',
                    quantity: 24,
                    inventoryStatus: 'INSTOCK',
                    rating: 5
                },
                {
                    id: '1001',
                    code: 'nvklal433',
                    name: 'Black Watch',
                    description: 'Product Description',
                    image: 'black-watch.jpg',
                    price: 72,
                    category: 'Accessories',
                    quantity: 61,
                    inventoryStatus: 'INSTOCK',
                    rating: 4
                },
                {
                    id: '1002',
                    code: 'zz21cz3c1',
                    name: 'Blue Band',
                    description: 'Product Description',
                    image: 'blue-band.jpg',
                    price: 79,
                    category: 'Fitness',
                    quantity: 2,
                    inventoryStatus: 'LOWSTOCK',
                    rating: 3
                },
                {
                    id: '1003',
                    code: '244wgerg2',
                    name: 'Blue T-Shirt',
                    description: 'Product Description',
                    image: 'blue-t-shirt.jpg',
                    price: 29,
                    category: 'Clothing',
                    quantity: 25,
                    inventoryStatus: 'INSTOCK',
                    rating: 5
                },
                {
                    id: '1004',
                    code: 'h456wer53',
                    name: 'Bracelet',
                    description: 'Product Description',
                    image: 'bracelet.jpg',
                    price: 15,
                    category: 'Accessories',
                    quantity: 73,
                    inventoryStatus: 'INSTOCK',
                    rating: 4
                },
                {
                    id: '1005',
                    code: 'av2231fwg',
                    name: 'Brown Purse',
                    description: 'Product Description',
                    image: 'brown-purse.jpg',
                    price: 120,
                    category: 'Accessories',
                    quantity: 0,
                    inventoryStatus: 'OUTOFSTOCK',
                    rating: 4
                },
                {
                    id: '1006',
                    code: 'bib36pfvm',
                    name: 'Chakra Bracelet',
                    description: 'Product Description',
                    image: 'chakra-bracelet.jpg',
                    price: 32,
                    category: 'Accessories',
                    quantity: 5,
                    inventoryStatus: 'LOWSTOCK',
                    rating: 3
                },
                {
                    id: '1007',
                    code: 'mbvjkgip5',
                    name: 'Galaxy Earrings',
                    description: 'Product Description',
                    image: 'galaxy-earrings.jpg',
                    price: 34,
                    category: 'Accessories',
                    quantity: 23,
                    inventoryStatus: 'INSTOCK',
                    rating: 5
                },
                {
                    id: '1008',
                    code: 'vbb124btr',
                    name: 'Game Controller',
                    description: 'Product Description',
                    image: 'game-controller.jpg',
                    price: 99,
                    category: 'Electronics',
                    quantity: 2,
                    inventoryStatus: 'LOWSTOCK',
                    rating: 4
                },
                {
                    id: '1009',
                    code: 'cm230f032',
                    name: 'Gaming Set',
                    description: 'Product Description',
                    image: 'gaming-set.jpg',
                    price: 299,
                    category: 'Electronics',
                    quantity: 63,
                    inventoryStatus: 'INSTOCK',
                    rating: 3
                },
                {
                    id: '1010',
                    code: 'plb34234v',
                    name: 'Gold Phone Case',
                    description: 'Product Description',
                    image: 'gold-phone-case.jpg',
                    price: 24,
                    category: 'Accessories',
                    quantity: 0,
                    inventoryStatus: 'OUTOFSTOCK',
                    rating: 4
                },
                {
                    id: '1011',
                    code: '4920nnc2d',
                    name: 'Green Earbuds',
                    description: 'Product Description',
                    image: 'green-earbuds.jpg',
                    price: 89,
                    category: 'Electronics',
                    quantity: 23,
                    inventoryStatus: 'INSTOCK',
                    rating: 4
                },
                {
                    id: '1012',
                    code: '250vm23cc',
                    name: 'Green T-Shirt',
                    description: 'Product Description',
                    image: 'green-t-shirt.jpg',
                    price: 49,
                    category: 'Clothing',
                    quantity: 74,
                    inventoryStatus: 'INSTOCK',
                    rating: 5
                }]
const columns = [
    { field: 'code', header: 'Code' },
    { field: 'name', header: 'Name' },
    { field: 'category', header: 'Category' },
    { field: 'quantity', header: 'Quantity' }
];

   

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

const items = ref([
    {
        label: 'Undo',
        icon: 'pi pi-chevron-circle-left',
        command: handleUndo
    },
    {
        label: 'Redo',
        icon: 'pi pi-chevron-circle-right',
        command: handleRedo
    
    }
])

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