
using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{

    public partial class Hold
    {
        void AssignTasks()
        {
            ListTasks();


            foreach (WorkerTask task in tasks)
            {
                if( task.Type == TaskType.Give )
                    continue;

                foreach (Viking worker in roles[Constants.ROLE_WORKER])
                {
                    if (worker.Store[task.ResourceType.ToString()] > 0 && task.ResourceNeed > 0 && worker.Task == null)
                    {
                        task.assignedWorkers.Add(worker);
                        task.ResourceNeed -= worker.Store[task.ResourceType.ToString()];
                        worker.Task = task;
                        //Console.WriteLine( $"Assigned worker {worker.Name} to task {task.TaskId}" );
                    }
                }
            }

            //Workers without energy shall go fishing
            foreach (Viking worker in roles[Constants.ROLE_WORKER])
            {
                if ( worker.Task == null )
                {
                    worker.Task = 
                        new WorkerTask()
                        {
                            taskId = "Fish_" + worker.Name,
                            target = ponds[0].Source,
                            Type = TaskType.Fish,
                            ResourceType = ResourceType.Energy.ToString(),
                            Severity = 0.5,
                            ResourceNeed = worker.Store[Constants.RESOURCE_CAPACITY],
                            amount = worker.Store[Constants.RESOURCE_CAPACITY]
                        }
                    ;
        
                }
            }
            
        }


        void ListTasks()
        {


            tasks.Add(
                new WorkerTask()
                {
                    taskId = "Pray",
                    target = Room.Controller,
                    Type = TaskType.Pray,
                    ResourceType = ResourceType.Energy.ToString(),
                    Severity = 0.05,
                    //Room.Controller.TicksToDowngrade == 0 ? 0.05 :
                    // 1.0 - Room.Controller.TicksToDowngrade / game.Constants.Controller.Downgrade[ Room.Controller.Level ] ,
                    ResourceNeed = 5000
                }
            );



            tasks.Sort((x, y) => y.Severity.CompareTo(x.Severity));

        }
    }
}