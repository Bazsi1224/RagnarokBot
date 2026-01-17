using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;


namespace RagnarokBot
{
    public class Longhouse : Settlement
    {
        public const int WIDTH = 7;
        public const int HEIGHT = 7;
        IStructureSpawn Spawn;
        List<IStructureExtension> Houses = new List<IStructureExtension>();
        List<IStructureTower> Towers = new List<IStructureTower>();
        List<IStructure> Fillables = new List<IStructure>();
        bool spawnFree = true;
        Position[] RestPositions = new Position[4];


        public Longhouse(Hold hold) : base(hold)
        {
            Room = hold.Room;
            SettlementMemory = Room.Memory.GetOrCreateObject("longhouse");
            settlementName = "longhouse";

            Width = WIDTH;
            Height = HEIGHT;

            GetLocation();
            DrawVisual(new Color(30, 0, 255, 0)); // "rgba(0, 255, 0, 0.1)"

            BuildLonghouse();

            RestPositions[0] = GetRelativePosition(  2,  2 );
            RestPositions[1] = GetRelativePosition(  2, -2 );
            RestPositions[2] = GetRelativePosition( -2,  2 );
            RestPositions[3] = GetRelativePosition( -2, -2 );

            LookForPopulation();

            foreach (IStructureSpawn spawn in Room.Find<IStructureSpawn>(true))
            {
                Spawn = spawn;
                Fillables.Add(spawn);
            }

            foreach (IStructureExtension house in Room.Find<IStructureExtension>(true))
            {
                Houses.Add(house);
                Fillables.Add(house);
            }

            foreach (IStructureTower tower in Room.Find<IStructureTower>(true))
            {
                Towers.Add(tower);
                Fillables.Add(tower);
            }            
        }

        public override bool PlanPosition()
        {
            foreach (IStructureSpawn spawn in Room.Find<IStructureSpawn>(true))
            {
                Position = new Position(spawn.RoomPosition.Position.X, spawn.RoomPosition.Position.Y);
                Orientation = Direction.Top;
                return true;
            }
            return false;
        }

        void BuildLonghouse()
        {
            Position buildPos = Position;

            Room.CreateConstructionSite<IStructureSpawn>(buildPos);

            buildPos = new Position(Position.X + 3, Position.Y + 1);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 3, Position.Y + 2);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 3, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 2, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 1, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);

            buildPos = new Position(Position.X + 3, Position.Y - 1);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 3, Position.Y - 2);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 3, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 2, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X + 1, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);

            buildPos = new Position(Position.X - 3, Position.Y + 1);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 3, Position.Y + 2);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 3, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 2, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 1, Position.Y + 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);

            buildPos = new Position(Position.X - 3, Position.Y - 1);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 3, Position.Y - 2);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 3, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 2, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);
            buildPos = new Position(Position.X - 1, Position.Y - 3);
            Room.CreateConstructionSite<IStructureExtension>(buildPos);


            buildPos = new Position(Position.X, Position.Y - 2);
            Room.CreateConstructionSite<IStructureTower>(buildPos);

            buildPos = new Position(Position.X, Position.Y + 2);
            Room.CreateConstructionSite<IStructureTower>(buildPos);

            buildPos = new Position(Position.X - 2, Position.Y);
            Room.CreateConstructionSite<IStructurePowerSpawn>(buildPos);

            buildPos = new Position(Position.X + 2, Position.Y);
            Room.CreateConstructionSite<IStructureLink>(buildPos);

            buildPos = new Position(Position.X + 1, Position.Y + 1);
            Room.CreateConstructionSite<IStructureTerminal>(buildPos);
            buildPos = new Position(Position.X + 1, Position.Y - 1);
            Room.CreateConstructionSite<IStructureStorage>(buildPos);
            buildPos = new Position(Position.X - 1, Position.Y + 1);
            Room.CreateConstructionSite<IStructureFactory>(buildPos);
            buildPos = new Position(Position.X - 1, Position.Y - 1);
            Room.CreateConstructionSite<IStructureNuker>(buildPos);
        }

        public List<SpawnRequest> GetSpawnRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();

            if( roles[Constants.ROLE_HOARDER].Count < 2 )
            {
                BodyPartType[] body = GetHoarderBody();
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_HOARDER);
                initialMemory.SetValue("hold", Room.Name);
                initialMemory.SetValue("home", settlementName);

                request.Add(new SpawnRequest
                {
                    Body = body,
                    InitialMemory = initialMemory
                });
            }

            List<IConstructionSite> siteList = GetConstructions();

            if (siteList.Count > 0 && roles[ Constants.ROLE_WORKER ].Count == 0 )
            {
                BodyPartType[] body = GetWorkerBody();
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_WORKER);
                initialMemory.SetValue("hold", Room.Name);
                initialMemory.SetValue("home", settlementName);

                request.Add(new SpawnRequest
                {
                    Body = body,
                    InitialMemory = initialMemory
                });
            }

            if (siteList.Count > 0 && roles[Constants.ROLE_HOARDER].Count < 3 )
            {
                BodyPartType[] body = GetHoarderBody();
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_HOARDER);
                initialMemory.SetValue("hold", Room.Name);
                initialMemory.SetValue("home", settlementName);

                request.Add(new SpawnRequest
                {
                    Body = body,
                    InitialMemory = initialMemory
                });
            }


            return request;
        }

        public BodyPartType[] GetWorkerBody()
        {
            BodyPartType[][] stages = [
                [BodyPartType.Move, BodyPartType.Move, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work],
                [BodyPartType.Move, BodyPartType.Move, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work],
                [BodyPartType.Move, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work, BodyPartType.Work],
                [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work]
            ];
            
            foreach( BodyPartType[] stage in stages )
                if( Room.EnergyCapacityAvailable >= Trainer.GetBodysetCost(stage) )
                    return stage;

            return [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work];
        }

        public BodyPartType[] GetHoarderBody()
        {
            BodyPartType[][] stages = [
                [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry],
                [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry],
                [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry],
                [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry ],
            ];
            
            foreach( BodyPartType[] stage in stages )
                if( Room.EnergyCapacityAvailable >= Trainer.GetBodysetCost(stage) )
                    return stage;

            return [BodyPartType.Move, BodyPartType.Carry];
        }

        public void Run()
        {
            Watchtowers();
            RunHoarders();
            RunWorkers();

        }

        public void SpawnViking(SpawnRequest request)
        {
            if (!spawnFree) return;

            string name = Trainer.GetRandomName();
            var ret = Spawn.SpawnCreep(request.Body, $"{name} {game.Time % 1000}", new(memory: request.InitialMemory));
            spawnFree = false;
        }
        
        void Watchtowers()
        {
            foreach( IStructureTower tower in Towers )
            {
                var enemies = Room.Find<ICreep>(false);

                foreach( ICreep enemy in enemies )
                    tower.Attack( enemy );
            }
        }
    
        void RunHoarders()
        {
            foreach (Viking hoarder in roles[Constants.ROLE_HOARDER])
            {
                try{ RunHoarder(hoarder); } catch( Exception e ){ Console.WriteLine( $"Runnung hoarder {hoarder.Name} failed" ); Console.WriteLine( e ); }
            }
        }            

        void RunHoarder( Viking hoarder )
        {
            
            if( hoarder.Store[ ResourceType.Energy.ToString() ] == 0 )
            {
                WorkerTask task = hold.GetEnergy(hoarder);

                hoarder.Task = task;
                task.ResourceNeed -= hoarder.Store[ Constants.RESOURCE_CAPACITY ];

                if (task != null)
                {
                    IStructure targetStructure;
                    ICreep targetCreep;

                    switch (task.Type)
                    {
                        case TaskType.Collect:    
                                targetStructure = task.target as IStructure;
                                if (targetStructure != null)
                                {
                                    hoarder.Take(targetStructure, ResourceType.Energy, task.amount);
                                }
                            break;
                        case TaskType.Take:
                            targetCreep = task.target as ICreep;
                            if (targetCreep != null)
                            {
                                hoarder.Take(targetCreep, ResourceType.Energy,task.amount);
                            }
                            break;
                        case TaskType.Fish:
                            ISource targetSource = task.target as ISource;
                            if (targetSource != null)
                            {
                                hoarder.Harvest(targetSource);
                            }
                            break;
                        default:
                            Console.WriteLine($"Hoarder {hoarder.Name} has no task!");
                            break;
                    }

                }
            }
            else
            {
                foreach (IStructure toFill in Fillables )
                {
                    IStore store = (toFill as IWithStore).Store;

                    if( store.GetFreeCapacity( ResourceType.Energy ) > 0 )
                    {
                        hoarder.Transfer( toFill, ResourceType.Energy, store.GetFreeCapacity( ResourceType.Energy ));
                        return;
                    }
                }

                foreach ( Viking worker in roles[ Constants.ROLE_WORKER ] )
                {
                    IStore store = worker.Creep.Store;

                    if( store.GetFreeCapacity( ResourceType.Energy ) > 0 )
                    {
                        hoarder.Transfer( worker.Creep, ResourceType.Energy, store.GetFreeCapacity( ResourceType.Energy ));
                        return;
                    }
                }
                
                hoarder.MoveTo( RestPositions[ roles[Constants.ROLE_HOARDER].IndexOf(hoarder) ] );
                
            }
        
        }
    
        void RunWorkers()
        {
            foreach (Viking worker in roles[Constants.ROLE_WORKER])
            {
                try{ RunWorker(worker); } catch( Exception e ){ Console.WriteLine( $"Runnung hoarder {worker.Name} failed" ); Console.WriteLine( e ); }
            }
        }            

        void RunWorker( Viking worker )
        {   
            Position[] BuildPositions = [
                GetRelativePosition( 0,  1 ), 
                GetRelativePosition( 0, -1 )];
            

            if( worker.Store[ResourceType.Energy.ToString()] == 0 )
                return;

            for( int position_index = 0 ; position_index < BuildPositions.Length; position_index++ )
            {
                Position BuildPosition = BuildPositions[position_index];
                List<IConstructionSite> siteList = ListSitesForBuildPos(BuildPosition);

                if( siteList.Count > 0 ) 
                {
                    worker.MoveTo( BuildPosition );
                    foreach ( IConstructionSite site in siteList)
                    {
                        if( worker.Build( site, false ) == 0 )
                            return;
                    }
                }
            }            
        }

        List<IConstructionSite> ListSitesForBuildPos( Position buildPosition )
        {
            List<IConstructionSite> siteList = GetConstructions();
            List<IConstructionSite> response = new List<IConstructionSite>();

            foreach (IConstructionSite site in siteList)
            {
                if( buildPosition.LinearDistanceTo( site.RoomPosition.Position ) <= 3 )
                    response.Add( site );
            }

            return response;
        }

    }
}