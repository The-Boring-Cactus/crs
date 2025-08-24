import { App } from "vue";
import { defineStore } from "pinia";

export const userStoreMe = defineStore({
    id: "user",
    state: () => ({
      
      authv: false,
      namev: '',
      levelv: '',
      functionsv: []
    }),
    getters: {
        auth: (state) => state.authv,
        name: (state) => state.namev,
        level: (state) => state.levelv,
        functions: (state) => state.functionsv
    },
    actions: {
      setCurr(auth: boolean, name: string, level: string, functions: string[]) {
        this.authv = auth;
        this.namev = name;
        this.levelv = level;
        this.functionsv = functions;
      }
    }
  });
