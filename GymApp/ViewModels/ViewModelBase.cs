using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GymApp.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private string _statusMessage = string.Empty;
    private bool _canRetry;
    private bool _hasSelection;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool CanRetry
    {
        get => _canRetry;
        private set => SetProperty(ref _canRetry, value);
    }

    public bool HasSelection
    {
        get => _hasSelection;
        protected set => SetProperty(ref _hasSelection, value);
    }

    protected void ClearStatusMessage()
    {
        StatusMessage = string.Empty;
        CanRetry = false;
    }

    protected void SetStatusMessage(string message)
    {
        StatusMessage = message;
        CanRetry = false;
    }

    public virtual Task RetryAsync() => Task.CompletedTask;

    protected string GetCloudFailureMessage(Exception exception, string operation)
    {
        if (exception is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
            return "Your session has expired. Sign in again and retry.";

        if (exception is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
            return "You don't have permission to do that. Check your account access and try again.";

        if (exception is HttpRequestException { StatusCode: { } statusCode })
            return $"Couldn't {operation}: Firebase returned HTTP {(int)statusCode}. Please retry. If it continues, share this code with support.";

        if (exception is HttpRequestException or TaskCanceledException
            || exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase))
            return $"Couldn't {operation} because GymApp couldn't reach Firebase. Check your internet connection and try again.";

        if (exception.Message.Contains("session expired", StringComparison.OrdinalIgnoreCase))
            return "Your session has expired. Sign in again and retry.";

        return $"Couldn't {operation}. Please try again. If the problem continues, contact support.";
    }

    protected void ReportCloudFailure(Exception exception, string operation, bool canRetry = true)
    {
        Trace.TraceError($"GymApp couldn't {operation}: {exception}");
        StatusMessage = GetCloudFailureMessage(exception, operation);
        CanRetry = canRetry;
    }
}
