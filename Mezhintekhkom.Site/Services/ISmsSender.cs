﻿namespace Mezhintekhkom.Site.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}