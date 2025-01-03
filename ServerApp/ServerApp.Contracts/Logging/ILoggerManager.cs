﻿namespace ServerApp.Contracts.Logging;

public interface ILoggerManager
{
    void LogInfo(string message);
    void LogWarn(string message);
    void LogDebug(string message);
    void LogError(string message);
    void LogError(Exception ex, string message);
}