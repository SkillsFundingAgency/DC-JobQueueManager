# DC-JobQueueManager
This component is used to access the job queue. 
# Usage
Create an instance of JobQueueManager by passing in the implementation of DbContextOptions containing the connection string and other options to connect to JobScheduler database as mentioned in the dependencies section below.
# Dependencies 
* Create and empty database 
* publish ESFA.DC.JobQueueManager.Database project which will create a "Job" Table and and a stored proc "sp_GetJobByPriority"
