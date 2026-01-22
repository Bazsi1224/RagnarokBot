using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;


namespace RagnarokBot
{
    class Conqer : Longboat
    {
        int stage = 0;
        IRoom room;


        public Conqer( IFlag flag) : base(flag)
        {
            if( flag.Room == null )
                RunStage0();
            else
                RunStage1();
            
        }

        void RunStage0()
        {
            if( Crew.Count == 0 )
            { 
                BodyPartType[] body = [BodyPartType.Claim, BodyPartType.Move];
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_CONQUERER);
                initialMemory.SetValue("hold", Flag.Name);

                SpawnRequests.Add(
                     new SpawnRequest()
                     {
                         Body = body,
                         InitialMemory = initialMemory
                     });

            }
            else
            {
                foreach( Viking viking in Crew )
                {
                    if( viking.Creep.Room.Name != Flag.Room.Name )
                    {
                        viking.MoveTo( Flag.RoomPosition.Position );                    
                    }
                    else
                    {
                        if( viking.Role == Constants.ROLE_CONQUERER )
                            viking.Claim( Flag.Room.Controller );
                    }
                }
            }
        }

        void RunStage1()
        {
            Console.WriteLine( $"Conquer stage 1 not implemented yet!" );
        }
    }
}