import { App } from "vue";
import { defineStore } from "pinia";
import { WebSocketMessageClient } from '@/websocket/WebSocketMessageClient';
import { ServerResponse } from '@/websocket/ServerResponse';

const pendingRequests = new Map<string, { resolve: Function, reject: Function }>();
let isListenerSet = false;

function setupGlobalListener(socket: any) {
  if (!isListenerSet && socket) {
    socket.onmessage = (event: any) => {
      const responseHandler = new ServerResponse();
      let jResponse;
      try {
        jResponse = JSON.parse(event.data);
      } catch (e) { return; }

      var response = responseHandler.analizeMessage(jResponse);
      if (!response) return;
      console.log(response);
      if (response.type === 'Response') {
        // Authenticate uses a different flow (type 3 response, maybe missing RequestId), 
        // but new commands will use RequestId.
        const reqId = response.RequestId || response.id; // fallback to response id
        const p = pendingRequests.get(reqId);
        if (p) {
          if (response.Status === 'Success') p.resolve(response);
          else p.reject(new Error(response.ErrorMessage || "Error"));
          pendingRequests.delete(reqId);
        } else {
          // If we can't find a matching request, maybe it's the authenticate call (legacy)
          // We can resolve all 'authenticate' pending requests if it's a success
          for (let [key, val] of pendingRequests.entries()) {
            if (key === 'auth') {
              if (response.Status === 'Success') val.resolve(response);
              else val.reject(new Error("Authentication failed"));
              pendingRequests.delete(key);
            }
          }
        }
      } else if (response.type === 'Notification') {
        const category = response.Category || response.category;
        if (category === 'Debug') {
          window.dispatchEvent(new CustomEvent('socket-debug', { detail: response.Content || response.content }));
        } else if (category === 'ExecutionComplete') {
          window.dispatchEvent(new CustomEvent('socket-execution-complete', { detail: response }));
        } else {
          window.dispatchEvent(new CustomEvent('socket-notification', { detail: response }));
        }
      } else if (response.type === 'Data') {
        // Structured output from script execution (Table, Chart, etc.)
        window.dispatchEvent(new CustomEvent('socket-output', { detail: response }));
      }
    };
    isListenerSet = true;
  }
}

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
      },
      authenticate(loginStr: string, passwordStr: string, socket: any): Promise<any> {
        setupGlobalListener(socket);
        return new Promise((resolve, reject) => {
          pendingRequests.set('auth', { resolve: (res: any) => {
             if (res.Status === 'Error') { reject(new Error('Authentication failed')); return; }
             if (res.Data?.Token) localStorage.setItem('crs_token', res.Data.Token);
             this.setCurr(true, 'User', 'admin', res.Data?.Functions || []);
             resolve(res);
          }, reject });

          const client = new WebSocketMessageClient(socket);
          client.sendAuthentication(loginStr, passwordStr);
        });
      },
      restoreSession(socket: any): Promise<any> {
        const token = localStorage.getItem('crs_token');
        if (!token) return Promise.reject(new Error('No stored token'));
        setupGlobalListener(socket);
        return new Promise((resolve, reject) => {
          pendingRequests.set('auth', { resolve: (res: any) => {
             if (res.Status === 'Error') {
               localStorage.removeItem('crs_token');
               reject(new Error('Token expired'));
               return;
             }
             if (res.Data?.Token) localStorage.setItem('crs_token', res.Data.Token);
             this.setCurr(true, 'User', 'admin', res.Data?.Functions || []);
             resolve(res);
          }, reject });
          const client = new WebSocketMessageClient(socket);
          client.sendTokenAuthentication(token);
        });
      },
      executeCommand(command: string, parameters: any, socket: any): Promise<any> {
        setupGlobalListener(socket);
        return new Promise((resolve, reject) => {
          const client = new WebSocketMessageClient(socket);
          const reqId = client.sendCommand(command, parameters);
          pendingRequests.set(reqId, { resolve, reject });
        });
      }
    }
  });
