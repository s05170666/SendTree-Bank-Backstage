namespace 專題Employee_Version1.Models.ServiceModels
{
    // 指令執行結果（供 Controller 判斷流程）
    public class LoanCommandResult
    {
        // 是否執行成功
        public bool Success { get; set; }

        // 錯誤或補充訊息
        public string Message { get; set; }
    }
}
