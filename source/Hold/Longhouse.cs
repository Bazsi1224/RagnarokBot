using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;
using System.Reflection.Metadata;

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

        Position[] RestPosition = new Position[2];

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

            RestPosition[0] = GetRelativePosition(  2,  2 );
            RestPosition[1] = GetRelativePosition( -2, -2 );

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
                BodyPartType[] body = [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry, BodyPartType.Move, BodyPartType.Carry];
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

        public void Run()
        {
            Watchtowers();
            RunHoarders();

            if (Room.Storage == null)
            {
                RunInPrePhase();
                return;
            }
        }

        public void RunInPrePhase()
        {

            var constructionSites = Room.Find<IConstructionSite>(true);
            List<IConstructionSite> siteList = new List<IConstructionSite>();

            foreach (IConstructionSite site in constructionSites)
                if (Position.LinearDistanceTo(site.RoomPosition.Position) <= 3)
                    siteList.Add(site);

            if (siteList.Count > 0)
            {

                IConstructionSite site = siteList[0];
                hold.RequestTask(
                new WorkerTask()
                {
                    taskId = "Empty_Container_" + settlementName,
                    target = site,
                    Type = TaskType.Build,
                    ResourceType = ResourceType.Energy.ToString(),
                    Severity = 1,
                    ResourceNeed = site.ProgressTotal - site.Progress,
                    amount = site.ProgressTotal - site.Progress
                }
                );

            }

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
                RunHoarder(hoarder);
            }
        }            

        void RunHoarder( Viking hoarder )
        {
            
            if( hoarder.store[ ResourceType.Energy.ToString() ] == 0 )
            {
                WorkerTask task = hold.GetEnergy();

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
                
                hoarder.MoveTo( RestPosition[ roles[Constants.ROLE_HOARDER].IndexOf(hoarder) ] );
                
            }
        
        }
    }
}