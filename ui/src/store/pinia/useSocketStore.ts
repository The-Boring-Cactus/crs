import { App } from "vue";
import { defineStore } from "pinia";
import { setupStore } from "@/store/pinia/store";
import { SocketStore, Message, MessageEnvelope } from "@/type/PiniaType";


 



export const useSocketStore = (app: App<Element>) => {
  return defineStore({
    id: "socket",
    state: (): SocketStore => ({
      isConnected: false,
      message: "",
      reconnectError: false,
      heartBeatInterval: 50000,
      heartBeatTimer: 0
    }),
    actions: {
      SOCKET_ONOPEN(event: any) {
        //console.log("successful websocket connection");
        app.config.globalProperties.$socket = event.currentTarget;
        this.isConnected = true;
        this.heartBeatTimer = window.setInterval(() => {
          const iddata = 'client_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();

          const message2: Message = {
            id: iddata,
            Status: 'Alive',
            timestamp: new Date().toISOString(),
            senderId: iddata
          } as Message;

          const envelope: MessageEnvelope = {
            type: 'Heartbeat'.toLowerCase(),
            data: message2,
            timestamp: message2.timestamp
          };
          this.isConnected &&
            app.config.globalProperties.$socket.sendObj(envelope);
        }, this.heartBeatInterval);
      },
      SOCKET_ONCLOSE(event: any) {
        this.isConnected = false;
        window.clearInterval(this.heartBeatTimer);
        this.heartBeatTimer = 0;
        //console.log("Closed: " + new Date());
        //console.log(event);
      },
      SOCKET_ONERROR(event: any) {
        //console.error(event);
      },
      SOCKET_ONMESSAGE(message: any) {
       // console.log(message);
        this.message = message;
      },
      SOCKET_RECONNECT(count: any) {
        //console.info("Reconecting...", count);
      },
      SOCKET_RECONNECT_ERROR() {
        this.reconnectError = true;
      }
    }
  })();
};


export function useSocketStoreWithOut(app: App<Element>) {
  setupStore(app);
  return useSocketStore(app);
}