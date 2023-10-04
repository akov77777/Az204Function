using System;

namespace Az204Function
{
    internal record QueueRequest(string Message, string StorageAccountName, string QueueName, string OutputQueueName);
}
