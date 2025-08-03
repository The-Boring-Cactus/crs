<template>
<Drawer v-model:visible="visibleCompo" header="Elements" position="right">
     <Button icon="pi pi-upload" severity="secondary" label="Title" text @click="addcomponent('Title')"/>

</Drawer>

        
    
<Toolbar>
    <template #start>
      
        <Button icon="pi pi-plus" label="Add Elements" class="mr-2" severity="secondary" text  @click="visibleCompo = true"/>
        <Button icon="pi pi-print" class="mr-2" severity="secondary" text />
        <Button icon="pi pi-upload" severity="secondary" text @click="addcomponent(2)"/>
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
  <grid-layout 
    v-model:layout="layout.componentes"
    :col-num="15"
    :row-height="40"
    is-draggable
    is-resizable
    vertical-compact
    use-css-transforms
  >
    <grid-item 
      v-for="item in layout.componentes"
      :static="item.static"
      :x="item.x"
      :y="item.y"
      :w="item.w"
      :h="item.h"
      :i="item.i"
      :key="item.i"
    >
      <Inplace v-if="item.type === 'Texto'" class="center">
        
            <template #display>
                {{ item.value || 'Click to Edit' }}
            </template>
            <template #content="{ closeCallback }">
                <span class="inline-flex items-center gap-1">
                    <InputText v-model="item.value" autofocus />
                    <Button icon="pi pi-times" text severity="danger" @click="closeCallback" />
                </span>
            </template>
        </Inplace>
    </grid-item>
  </grid-layout>
</template>

<script setup>
import GridLayout from '@/components/draggable/GridLayout.vue'
import GridItem from '@/components/draggable/GridItem.vue'
import { ref } from 'vue'
const visibleCompo = ref(false);

let layout = ref(
    {
        componentes: [
          {"x":6,"y":0,"w":3,"h":1,"i":"0", static: false, type:"Texto", value:""},
        ],
        formvalues: {
            name: 'New Form',
            isGlobal: false
        }        
    })



const MyTitle = ref();

 const addcomponent = async (type) => {
  console.log(type);
  layout.value.componentes.push({"x":1,"y":0,"w":2,"h":2,"i":layout.value.componentes.length, static: false, type:"Texto", value:""});
 }

const itemTitle = (item) => {
  let result = item.i;
  if (item.static) {
      result += " - Static";
  }
  return result;
}
</script>
<style scoped>
.vue-grid-layout {
  background: #ffffff;
}
.vue-grid-item:not(.vue-grid-placeholder) {
  background: #ffffff;
  border: 1px dashed rgb(189, 188, 188);
}
.vue-grid-item .resizing {
  opacity: 0.9;
}
.vue-grid-item .static {
  background: rgb(255, 255, 255);
}
.vue-grid-item .center {
  
  text-align: center;
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  margin: auto;
}
.vue-grid-item .no-drag {
  height: 100%;
  width: 100%;
}
.vue-grid-item .minMax {
  font-size: 12px;
}
.vue-grid-item .add {
  cursor: pointer;
}
.vue-draggable-handle {
  position: absolute;
  width: 20px;
  height: 20px;
  top: 0;
  left: 0;
  
  background-position: bottom right;
  padding: 0 8px 8px 0;
  background-repeat: no-repeat;
  background-origin: content-box;
  box-sizing: border-box;
  cursor: pointer;
}
</style>