﻿namespace Infrastructure.MessageBroker;

[AddOptions("MessageBroker")]
public sealed class MessageBrokerSettings
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}