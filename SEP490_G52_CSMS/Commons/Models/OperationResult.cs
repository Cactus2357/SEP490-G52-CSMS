namespace SEP490_G52_CSMS.Commons.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static OperationResult Ok(string? message = null)
        {
            return new OperationResult
            {
                Success = true,
                Message = message ?? string.Empty
            };
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
