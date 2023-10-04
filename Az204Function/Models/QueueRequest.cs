using System;

namespace Az204Function.Models
{
    internal record QueueRequest(string Message, string QueueName, string OutputQueueName);
}
