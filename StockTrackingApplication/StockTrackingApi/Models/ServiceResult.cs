namespace StockTrackingAuthAPI.Models;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ServiceResult<T> SuccessResult(T? data = default, string message = "") =>
        new() { Success = true, Message = message, Data = data };

    public static ServiceResult<T> ErrorResult(string message) =>
        new() { Success = false, Message = message };
}
