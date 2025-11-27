namespace TaskMaster.Models.Enums;

public enum TaskItemStatus
{
    Analysis,
    WaitingForDev,
    InDev,
    WaitingForReview,
    InReview,
    WaitingForTesting,
    InTesting,
    Done
}
