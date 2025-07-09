<template>
<Drawer v-model:visible="visibleCompo" header="Components" position="right">
    <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.</p>
</Drawer>

        
    
<Toolbar>
    <template #start>
      
        <Button icon="pi pi-plus" class="mr-2" severity="secondary" text  @click="visibleCompo = true"/>
        <Button icon="pi pi-print" class="mr-2" severity="secondary" text />
        <Button icon="pi pi-upload" severity="secondary" text />
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
    v-model:layout="layout"
    :col-num="12"
    :row-height="30"
    is-draggable
    is-resizable
    vertical-compact
    use-css-transforms
  >
    <grid-item 
      v-for="item in layout"
      :static="item.static"
      :x="item.x"
      :y="item.y"
      :w="item.w"
      :h="item.h"
      :i="item.i"
      :key="item.i"
    >
      <span class="text">{{itemTitle(item)}}</span>
    </grid-item>
  </grid-layout>
</template>

<script setup>
import GridLayout from '@/components/draggable/GridLayout.vue'
import GridItem from '@/components/draggable/GridItem.vue'
import { ref } from 'vue'
const visibleCompo = ref(false);



const MyTitle = ref();

const layout = ref([
    {"x":0,"y":0,"w":2,"h":2,"i":"0", static: false},
    {"x":2,"y":0,"w":2,"h":4,"i":"1", static: false},
    {"x":4,"y":0,"w":2,"h":5,"i":"2", static: false},
    {"x":6,"y":0,"w":2,"h":3,"i":"3", static: false}
])
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
  border: 1px solid black;
}
.vue-grid-item .resizing {
  opacity: 0.9;
}
.vue-grid-item .static {
  background: rgb(255, 255, 255);
}
.vue-grid-item .text {
  font-size: 24px;
  text-align: center;
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  margin: auto;
  height: 100%;
  width: 100%;
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