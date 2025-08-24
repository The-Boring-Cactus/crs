<template>
  <Toolbar>
    <template #start>
      Name: 
       <Inplace  v-tooltip="'Click to Change'" >
            <template #display >
                {{ MyTitle || 'No Name' }}
            </template>
            <template #content="{ closeCallback }">
                <span class="inline-flex items-center gap-2">
                    <InputText v-model="MyTitle" autofocus />
                    <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
                </span>
            </template>
        </Inplace>    
      <Menubar :model="items" />
    </template>

    
</Toolbar>
  <div width="100%">
    <vue-excel-editor v-if="renderComponent" v-model="jsondata"  :no-footer="true" :no-paging="true"
    :no-num-col="false"
    :allow-add-col="false"
    :no-finding="true"
    :no-sorting="false"
    :no-filtering="true"
    :disable-panel-setting="true"
    :no-header-edit="true" />
  </div>
</template>


<script setup>
  import { nextTick, ref } from 'vue'
import { getCurrentInstance } from 'vue'


const instance = getCurrentInstance();

const MyTitle = ref();

const items = ref([
    {
        label: 'File',
        icon: 'pi pi-file',
        items: [
            {
                label: 'Clear Data',
                icon: 'pi pi-plus',
                command: () => {
                    newDoc();
                }
            },
            {
                label: 'Save',
                icon: 'pi pi-save',
                command: () => {
                   datasave();
                }
            }
        ]
    },
    {
        label: 'Data Options',
        icon: 'pi pi-pencil',
        items: [
            {
                label: 'Add new column',
                icon: 'pi pi-plus',
                command: () => {
                    createnewCol();
                }
            },
            {
                label: 'Add new row',
                icon: 'pi pi-plus',
                command: () => {
                   createnewRow();
                }
            }
        ]
    }
]);

function addProperties(data, newProperties) {
  data.forEach(item => {
    Object.assign(item, newProperties);
  });
  return data;
}

function addRow(data) {
  var newRow = {};
  Object.keys(data[0]).forEach(key => {
    newRow[key] = '';
  });
  data.push(newRow);
  return data;
}

const renderComponent = ref(true);

var jsondata = ref([
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''}
        ]);
  const createnewRow = async () => {
      var t = addRow(jsondata.value) ;
      
      jsondata.value = t;
      renderComponent.value = false;
      await nextTick();
      renderComponent.value = true;
    }
  const createnewCol = async () => {
      var i = Object.keys(jsondata.value[0]).length-1;
      var c = {['c' + i]: ''};
      var t = addProperties(jsondata.value, c) ;
      
      jsondata.value = t;
      renderComponent.value = false;
      await nextTick();
      renderComponent.value = true;
    }
  
  const newDoc = async () => {
      MyTitle.value = '';
      var t = ref([
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''},
            {c0: '',c1: '', c2: '',    c3: '', c4: '', c5: '', c6: ''}
        ]);
      jsondata.value = t.value;
      renderComponent.value = false;
      await nextTick();
      renderComponent.value = true;
    }

 const datasave = () => {
       console.log(jsondata.value);
      }
</script>
<style lang="scss" scoped>
 

</style>