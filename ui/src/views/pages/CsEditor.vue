<template>
          <Toast />
    <Toolbar>
        <template #start>
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
            <CodeMirrorEditor 
            :initial-code="code" 
            :code-functions="codeFunctions"
            :initial-language="'csharp'"
            @update:code="handleCodeChange"
            @language-changed="handleLanguageChange"
        />
        </div>
    </div>
    <br/>
    <Card role="region" class="m-0">
    <template #title>Debug</template>
    <template #content>
        <Textarea  v-model="debugText" rows="5" cols="200" autoResize/>
    </template>
    </Card>


</template>

<script setup>
import { shallowRef, ref } from 'vue';
import { getCurrentInstance } from 'vue';
import { useToast } from "primevue/usetoast";
import CodeMirrorEditor from "@/components/CodeMirrorEditor.vue";

import {userStoreMe} from "@/store/userStore";


import {WebSocketMessageClient} from "@/websocket/WebSocketMessageClient";
import {ServerResponse} from "@/websocket/ServerResponse";



const toast = useToast();

const debugText = ref('');

const { proxy } = getCurrentInstance();

const client = new WebSocketMessageClient(proxy.$socket);

const userStore = userStoreMe();

// State
const MyTitle = ref();
const code = ref('');

const codeFunctions = ref(userStore.functions);


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





const handleSave = () => {
     console.log('Content:', code.value)
    debugText.value ='';
     proxy.$socket.sendObj({
          type: "CodeScript",
          data: code.value,
          name: MyTitle.value
        });
};

const handleCodeChange = (newCode) => {
    code.value = newCode;
  
};

const handleLanguageChange = (language) => {
     console.log('Lenguaje cambiado a:', language)
}


</script>
<style lang="scss" scoped>

</style>