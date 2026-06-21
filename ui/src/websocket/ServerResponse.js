export class ServerResponse {
    constructor() {}

    analizeMessage(message) {
        const MessageType = ['Text', 'Command', 'Notification', 'Response', 'Error', 'Heartbeat', 'Authentication', 'Data'];
        const MessageStatus = ['Success', 'Error', 'Pending', 'Timeout'];

        // Validar que el envelope tenga la estructura esperada
        if (!message || typeof message !== 'object') {
            console.error('Envelope inválido: debe ser un objeto');
            return null;
        }

        if (!message.Type || !message.Timestamp) {
            console.error('Envelope inválido: faltan propiedades requeridas (type, timestamp)');
            return null;
        }

        // Data is optional — some message types (DataMessage, NotificationMessage) use other fields
        if (message.Data !== undefined && typeof message.Data !== 'object') {
            console.error('Envelope inválido: data debe ser un objeto');
            return null;
        }

        try {
            // Validar que el timestamp sea válido
            const messageDate = new Date(message.Timestamp);
            if (isNaN(messageDate.getTime())) {
                console.error('Timestamp del mensaje inválido');
                return null;
            }

            // Retornar el mensaje deserializado
            return {
                id: message.Id,
                type: MessageType[message.Type],
                timestamp: message.Timestamp,
                Status: MessageStatus[message.Status],
                senderId: message.SenderId,
                // Incluir todas las demás propiedades del mensaje original
                ...Object.fromEntries(Object.entries(message).filter(([key]) => !['Id', 'Type', 'Timestamp', 'SenderId', 'Status'].includes(key)))
            };
        } catch (error) {
            console.error('Error al deserializar el mensaje:', error);
            return null;
        }
    }
}

export default new ServerResponse();
