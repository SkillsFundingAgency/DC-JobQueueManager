namespace ESFA.DC.JobQueueManager.Models.Enums
{
    public enum JobStatus
    {
        Ready = 1,
        MovedForProcessing = 2,
        Processing = 3,
        Completed = 4,
        FailedRetry = 5,
        Failed = 6,
        Paused = 7
    }
}