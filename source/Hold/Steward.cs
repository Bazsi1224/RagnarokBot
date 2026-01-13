
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
                if( task.Type != TaskType.Take &&
                    task.Type != TaskType.Fill &&
                    task.Type != TaskType.Collect &&
                    task.Type != TaskType.Pickup &&
                    task.Type != TaskType.Give )
                    continue;

                foreach (Viking worker in roles[Constants.ROLE_HOARDER])
                {
                    
                        if (worker.store[task.ResourceType.ToString()] > 0 && task.ResourceNeed > 0 && worker.task == null)
                        {
                            task.assignedWorkers.Add(worker);
                            task.ResourceNeed -= worker.store[task.ResourceType.ToString()];
                            worker.task = task;
                            //Console.WriteLine( $"Assigned worker {worker.Name} to task {task.taskId}" );
                        }
                }                
            }


            foreach (WorkerTask task in tasks)
            {
                if( task.Type == TaskType.Give )
                    continue;

                foreach (Viking worker in roles[Constants.ROLE_WORKER])
                {
                    if (worker.store[task.ResourceType.ToString()] > 0 && task.ResourceNeed > 0 && worker.task == null)
                    {
                        task.assignedWorkers.Add(worker);
                        task.ResourceNeed -= worker.store[task.ResourceType.ToString()];
                        worker.task = task;
                        //Console.WriteLine( $"Assigned worker {worker.Name} to task {task.taskId}" );
                    }
                }
            }

            //Workers without energy shall go fishing
            foreach (Viking worker in roles[Constants.ROLE_WORKER])
            {
                if ( worker.task == null )
                {
                    worker.task = 
                        new WorkerTask()
                        {
                            taskId = "Fish_" + worker.Name,
                            target = ponds[0].Source,
                            Type = TaskType.Fish,
                            ResourceType = ResourceType.Energy.ToString(),
                            Severity = 0.5,
                            ResourceNeed = worker.store[Constants.RESOURCE_CAPACITY],
                            amount = worker.store[Constants.RESOURCE_CAPACITY]
                        }
                    ;
        
                }
            }

/*
            foreach (WorkerTask task in tasks)
            {
                IRoomVisual visual = Room.Visual;

                Color strokeColor = Color.Grey;

                visual.Circle( new FractionalPosition( 
                    task.target.RoomPosition.Position.X, 
                    task.target.RoomPosition.Position.Y ),
                    new CircleVisualStyle()
                    {
                        Fill = Color.Transparent,
                        Radius = 0.5,
                        Stroke = strokeColor
                    } );

            }
            */

            
        }


        void ListTasks()
        {


            foreach (Viking worker in roles[Constants.ROLE_WORKER])
                if (worker.store[Constants.RESOURCE_CAPACITY] > 0)
                {
                    tasks.Add(
                        new WorkerTask()
                        {
                            taskId = "Fill_Worker_" + worker.Name,
                            target = worker.Creep,
                            Type = TaskType.Give,
                            ResourceType = ResourceType.Energy.ToString(),
                            Severity = 0.5,
                            ResourceNeed = worker.store[Constants.RESOURCE_CAPACITY],
                            amount = worker.store[Constants.RESOURCE_CAPACITY]
                        }
                    );
                }


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