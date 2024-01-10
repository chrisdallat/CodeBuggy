namespace CodeBuggy.Helpers
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public OperationResult()
        {
            Success = false;
            Message = string.Empty;
        }
    }
}
