export class WebSocketMessageClient {
    constructor(socket) {
        this.socket = socket;
    }

    generateId() {
        return 'client_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
    }
    sendMessage(messageType, data) {
       
        const message = {
            ...data,
            id: data.id || this.generateId(),
            type: messageType,
            timestamp: new Date().toISOString(),
            senderId: this.clientId
        };

        const envelope = {
            type: messageType.toLowerCase(),
            data: message,
            timestamp: message.timestamp
        };

        this.socket.sendObj(envelope);
        return message.id;
    }

    // Métodos específicos para cada tipo de mensaje
    sendText(content, receiverId = null) {
        return this.sendMessage('Text', {
            content: content,
            receiverId: receiverId
        });
    }

    sendCommand(command, parameters = {}, receiverId = null) {
        return this.sendMessage('Command', {
            command: command,
            parameters: parameters,
            receiverId: receiverId
        });
    }

    sendResponse(requestId, status, data = null, errorMessage = null) {
        return this.sendMessage('Response', {
            requestId: requestId,
            status: status,
            data: data,
            errorMessage: errorMessage
        });
    }

    sendNotification(title, content, category = null, priority = 1) {
        return this.sendMessage('Notification', {
            title: title,
            content: content,
            category: category,
            priority: priority
        });
    }

    sendError(errorCode, errorDescription, errorDetails = null) {
        return this.sendMessage('Error', {
            errorCode: errorCode,
            errorDescription: errorDescription,
            errorDetails: errorDetails
        });
    }

    sendData(dataType, payload, metadata = {}) {
        return this.sendMessage('Data', {
            dataType: dataType,
            payload: payload,
            metadata: metadata
        });
    }

    sendAuthentication( username = null, password = null) {
        return this.sendMessage('Authentication', {
            Password: password,
            Username: username
        });
    }

    sendHeartbeat() {
        return this.sendMessage('Heartbeat', {
            serverTimestamp: Date.now(),
            status: 'alive'
        });
    }
    
    
}  

export default new WebSocketMessageClient();