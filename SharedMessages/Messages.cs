namespace SharedMessages;

public class PingMessage
{  
    public string CorrelationId { get; set; }
    public string Message { get; set; }
}

public class PongMessage
{
    public string CorrelationId { get; set; }
    public string Message { get; set; }
}