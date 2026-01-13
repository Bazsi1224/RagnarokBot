using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{
    public partial class Hold
    {
        void RunWorkers()
        {
            string debugMessage = "";

            foreach( Viking worker in roles[ Constants.ROLE_WORKER ] )
            {
                try
                {
                    RunWorker( worker );
                }
                catch( Exception e )
                {
                    Console.WriteLine( $"Error running worker {worker.Name}: {e.Message}" );
                    Console.WriteLine( e.StackTrace );
                    Console.WriteLine( debugMessage );
                }
            }


        }

        void RunWorker( Viking worker )
        {            
            if( worker.Pos.IsNextTo( ponds[0].Source.RoomPosition.Position ) && worker.Store[Constants.RESOURCE_CAPACITY] > 0 )
            {
                worker.Harvest( ponds[0].Source );
                return;
            }

            if( worker.Task == null )
            {
                Console.WriteLine( $"Worker {worker.Name} has no task!" );
                return;
            }

            IStructure targetStructure;
            ICreep targetCreep;

            switch( worker.Task.Type )
            {
                case TaskType.Pray:
                    IStructureController controller = worker.Task.target as IStructureController;
                    if( controller != null )
                    {
                        var result = worker.Pray( controller );
                    }
                    break;
                case TaskType.Build:
                    IConstructionSite targetSite = worker.Task.target as IConstructionSite;
                    if( targetSite != null )
                    {
                        worker.Build( targetSite );
                    }
                    break;
                case TaskType.Repair:
                    break;
                case TaskType.Collect:
                    targetStructure = worker.Task.target as IStructure;
                    if( targetStructure != null )
                    {
                        worker.Take( targetStructure, ResourceType.Energy, worker.Task.amount );
                    }
                    break;
                case TaskType.Fill:
                    targetStructure = worker.Task.target as IStructure;
                    if( targetStructure != null )
                    {
                        worker.Transfer( targetStructure, ResourceType.Energy, worker.Task.amount );
                    }
                    break;
                case TaskType.Take:
                    targetCreep = worker.Task.target as ICreep;
                    if( targetCreep != null )
                    {
                        worker.Take( targetCreep, ResourceType.Energy, worker.Task.amount );
                    }
                    break;
                case TaskType.Fish:
                    ISource targetSource = worker.Task.target as ISource;
                    if( targetSource != null )
                    {
                        worker.Harvest( targetSource );
                    }
                    break;
                default:
                    Console.WriteLine( $"Worker {worker.Name} has no task!" );
                    break;
            }
        }


    }

}