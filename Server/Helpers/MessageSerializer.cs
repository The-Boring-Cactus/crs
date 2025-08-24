using Newtonsoft.Json;

namespace Server.Helpers;

public static class MessageSerializer
    {
        
        public static string Serialize(BaseMessage message)
        {
            var envelope = new
            {
                type = message.Type.ToString().ToLowerInvariant(),
                data = message,
                timestamp = message.Timestamp
            };
            
            return JsonConvert.SerializeObject(envelope);
        }

        public static BaseMessage Deserialize(string json)
        {
            try
            {
                dynamic document = JsonConvert.DeserializeObject(json);
                
                var messageType = document.type.ToString();
                var dataElement = document.data;
                var data = dataElement.ToString();
                return messageType?.ToLowerInvariant() switch
                {
                    "text" => JsonConvert.DeserializeObject<TextMessage>(data),
                    "command" => JsonConvert.DeserializeObject<CommandMessage>(data),
                    "response" => JsonConvert.DeserializeObject<ResponseMessage>(data),
                    "notification" => JsonConvert.DeserializeObject<NotificationMessage>(data),
                    "error" => JsonConvert.DeserializeObject<ErrorMessage>(data),
                    "authentication" => JsonConvert.DeserializeObject<AuthenticationMessage>(data),
                    "data" => JsonConvert.DeserializeObject<DataMessage>(data),
                    "heartbeat" => JsonConvert.DeserializeObject<HeartbeatMessage>(data),
                    _ => throw new NotSupportedException($"Message type '{messageType}' is not supported")
                };
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse message JSON", ex);
            }
        }
    }