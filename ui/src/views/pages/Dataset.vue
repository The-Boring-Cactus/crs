<template>
  <Toolbar>
    <template #start>
      
        <Button icon="pi pi-caret-left" class="mr-2" severity="secondary" text  @click="createnewCol"/>
        <Button icon="pi pi-caret-right" class="mr-2" severity="secondary" text   @click="datasave"/>
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
  <div width="100%">
    <vue-excel-editor v-if="renderComponent" v-model="jsondata"  no-footer no-num-col no-header-edit />
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

  const createnewCol = async () => {
       var i = Object.keys(jsondata.value[0]).length-1;
       var c = {['c' + i]: ''};
       var t = addProperties(jsondata.value, c) ;
       
       jsondata.value = t;
       renderComponent.value = false;
       await nextTick();
       renderComponent.value = true;
      }

 const datasave = () => {
       console.log(jsondata);
      }
</script>
<style lang="scss" scoped>
 

</style>