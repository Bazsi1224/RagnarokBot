using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;


namespace RagnarokBot
{
    class Conqer : Longboat
    {
        IRoom Room;


        public Conqer(IFlag flag) : base(flag)
        {
            Room = flag.Room;
            if (flag.Room == null)
            {
                RunStage0();
                return;
            }

            if (!flag.Room.Controller.My)
            {
                RunStage0();
                return;
            }

            RunStage1();


        }

        void RunStage0()
        {
            if (roles[Constants.ROLE_CONQUERER].Count == 0)
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

            foreach (Viking viking in Crew)
            {

                if (viking.Creep.Room.Name != Flag.RoomPosition.RoomName)
                {
                    viking.MoveTo(Flag.LocalPosition);
                }
                else
                {
                    if (viking.Role == Constants.ROLE_CONQUERER)
                        viking.Claim(Flag.Room.Controller);
                }
            }

        }

        void RunStage1()
        {
            if (roles[Constants.ROLE_WORKER].Count < 5 )
            {
                BodyPartType[] body = [BodyPartType.Move, BodyPartType.Move, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work];
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_WORKER);
                initialMemory.SetValue("hold", Flag.Name);

                SpawnRequests.Add(
                     new SpawnRequest()
                     {
                         Body = body,
                         InitialMemory = initialMemory
                     });

            }

            Flag.Room.CreateConstructionSite<IStructureSpawn>( Flag.RoomPosition.Position );

            foreach (Viking viking in Crew)
            {
                if (viking.Creep.Room.Name != Flag.Room.Name)
                {
                    viking.MoveTo(Flag.RoomPosition);
                }
                else
                {
                    RunWorker( viking );
                }
            }

        }

        void RunWorker(Viking worker)
        {
            if (worker.Store[Constants.RESOURCE_CAPACITY] == 0)
            {
                IConstructionSite Spawn = new List<IConstructionSite>(Room.Find<IConstructionSite>(true))[0];

                worker.Build( Spawn );
            }
            else
            {
                foreach( ISource source in Room.Find<ISource>(false) )
                {
                    worker.Harvest(source);
                    return;
                }
            }

        }

    }
}