namespace TaskMaster.Models;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result Success()
    {
        return new Result { IsSuccess = true };
    }

    public static Result Failure(string errorMessage)
    {
        return new Result { IsSuccess = false, ErrorMessage = errorMessage };
    }
}

