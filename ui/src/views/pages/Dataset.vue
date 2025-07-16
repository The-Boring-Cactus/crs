<template>
  <Toolbar>
    <template #start>
      <Button icon="pi pi-th-large" label="New Data" class="mr-2" severity="secondary" text   @click="newDoc"/>  
      <Button icon="pi pi-save" label="Save Data" class="mr-2" severity="secondary" text   @click="datasave"/>
      <Button icon="pi pi-arrow-circle-right"  label="Add New Column"  class="mr-2" severity="secondary" text  @click="createnewCol"/>
      <Button icon="pi pi-arrow-circle-down" label="Add New Row" class="mr-2" severity="secondary" text   @click="createnewRow"/>
        
    </template>

    <template #center>
        <Inplace  v-tooltip="'Change Name'" >
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
    </template>
</Toolbar>
  <div width="100%">
    <vue-excel-editor v-if="renderComponent" v-model="jsondata"  no-footer  no-header-edit />
  </div>
</template>


<script setup>
  import { nextTick, ref } from 'vue'
import { getCurrentInstance } from 'vue'
const instance = getCurrentInstance();

const MyTitle = ref();

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