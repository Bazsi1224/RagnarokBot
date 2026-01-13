using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{   

public partial class Hold
{

        void RunHoarders()
        {
            string debugMessage = "";

            foreach( Viking hoarder in roles[ Constants.ROLE_HOARDER ] )
            {
                try
                {
                    RunHoarder( hoarder );
                }
                catch( Exception e )
                {
                    Console.WriteLine( $"Error running hoarder {hoarder.Name}: {e.Message}" );
                    Console.WriteLine( e.StackTrace );
                    Console.WriteLine( debugMessage );
                }
            }


        }

        void RunHoarder( Viking hoarder )
        {
            

            if( hoarder.task == null )
            {
                Console.WriteLine( $"Hoarder {hoarder.Name} has no task!" );
                return;
            }

            ICreep targetCreep;
            IStructure targetStructure;

            switch( hoarder.task.Type )
            {
                case TaskType.Pray:
    
                case TaskType.Build:
                    break;
                case TaskType.Repair:
                    break;
                case TaskType.Collect:
                    targetStructure = hoarder.task.target as IStructure;
                    if( targetStructure != null )
                    {
                        hoarder.Take( targetStructure, ResourceType.Energy, hoarder.task.amount );
                    }
                    break;
                case TaskType.Give:
                    targetCreep = hoarder.task.target as ICreep;
                    if( targetCreep != null )
                    {
                        hoarder.Transfer( targetCreep, ResourceType.Energy, hoarder.task.amount );
                    }
                    break;
                case TaskType.Fill:
                    targetStructure = hoarder.task.target as IStructure;
                    if( targetStructure != null )
                    {
                        hoarder.Transfer( targetStructure, ResourceType.Energy, hoarder.task.amount );
                    }
                    break;
                case TaskType.Take:
                    targetCreep = hoarder.task.target as ICreep;
                    if( targetCreep != null )
                    {
                        hoarder.Take( targetCreep, ResourceType.Energy, hoarder.task.amount );
                    }
                    break;
                default:
                    Console.WriteLine( $"Hoarder {hoarder.Name} has no task!" );
                    break;
            }
        }


}
}