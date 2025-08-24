export type SocketStore = {
  isConnected: boolean;
  message: string;
  reconnectError: boolean;
  heartBeatInterval: number;
  heartBeatTimer: number;
};




  

export interface BaseMessage {
  id: string;
  timestamp: string;
  senderId?: string;
  Status?: string;
  receiverId?: string | null;
}

export interface MessageEnvelope {
  type: string;
  data: Message;
  timestamp: string;
}

export interface HeartbeatMessage extends BaseMessage {
  serverTimestamp: number;
  status: string;
}
export interface ResponseMessage extends BaseMessage {
  requestId: string;
  status: string;
  data: any;
  errorMessage: string | null;
}

export type Message =  ResponseMessage | HeartbeatMessage;