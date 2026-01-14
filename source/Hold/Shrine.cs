using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;
using System.Reflection.Metadata;

namespace RagnarokBot
{
    public class Shrine : Settlement
    {
        const int SIZE = 5;
        IStructureController Controller;
        IStructureContainer Container;
        IConstructionSite Site;
        public double EnergyUsed = 0;
        public double EnergyNeed = 0;
        public double EnergyInput = 0;

        public Shrine(Hold hold) : base(hold)
        {
            Controller = Room.Controller;
            SettlementMemory = Room.Memory.GetOrCreateObject("shrine");
            settlementName = "shrine";

            Width = SIZE;
            Height = SIZE;
            Position = Controller.RoomPosition.Position;

            LookForPopulation();
            FindContainer();

            EnergyNeed = Controller.Level == 8 ? 15 : 5000;

            foreach (Viking priest in roles[Constants.ROLE_WORKER])
                EnergyUsed += priest.PrayCapacity;
        }

        public void FindContainer()
        {
            if (Container == null)
            {
                var containers = Room.Find<IStructureContainer>();

                foreach (IStructureContainer container in containers)
                {
                    if (Controller.RoomPosition.Position.LinearDistanceTo(container.RoomPosition.Position) <= 2)
                    {
                        Container = container;
                        return;
                    }
                }
            }

            if (Site == null)
            {
                var containerSites = Room.Find<IConstructionSite>(true);

                foreach (IConstructionSite containerSite in containerSites)
                {

                    if (Controller.RoomPosition.Position.LinearDistanceTo(containerSite.RoomPosition.Position) <= 2 &&
                        containerSite.StructureType.ToString().EndsWith("IStructureContainer") )
                    {
                        Site = containerSite;
                        return;
                    }
                }
            }

            IRoomTerrain terrain = Room.GetTerrain();
            Position ControllerPosition = Controller.RoomPosition.Position;

            for( int i = -2; i <= 2; i++ )
            {
                if( CheckPositionForContainer( new Position( ControllerPosition.X - 2, ControllerPosition.Y + i ), terrain ) )
                {
                    Room.CreateConstructionSite<IStructureContainer>(new Position( ControllerPosition.X - 2, ControllerPosition.Y + i ));
                    return;
                }
                if( CheckPositionForContainer( new Position( ControllerPosition.X + 2, ControllerPosition.Y + i ), terrain ) )
                {
                    Room.CreateConstructionSite<IStructureContainer>(new Position( ControllerPosition.X + 2, ControllerPosition.Y + i ));
                    return;
                }
                if( CheckPositionForContainer( new Position( ControllerPosition.X + i, ControllerPosition.Y - 2 ), terrain ) )
                {
                    Room.CreateConstructionSite<IStructureContainer>(new Position( ControllerPosition.X + i, ControllerPosition.Y - 2 ));
                    return;
                }
                if( CheckPositionForContainer( new Position( ControllerPosition.X + i, ControllerPosition.Y + 2 ), terrain ) )
                {
                    Room.CreateConstructionSite<IStructureContainer>(new Position( ControllerPosition.X + i, ControllerPosition.Y + 2 ));
                    return;
                }
            }

        }

        bool CheckPositionForContainer( Position position, IRoomTerrain terrain )
        {
            for( int x = -1; x <= 1; x++ )
                for( int y = -1; y <= 1; y++ )
                    if( terrain[ new Position( position.X + x, position.Y + y ) ] == Terrain.Wall ) return false;                    
            
            return true;
        }

        public List<SpawnRequest> GetSpawnRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();

            if( roles[Constants.ROLE_WORKER].Count / 2 > roles[Constants.ROLE_HOARDER].Count )
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

            if (EnergyInput > EnergyUsed)
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
            foreach (Viking priest in roles[Constants.ROLE_WORKER])
            {
                try { RunPriest(priest); }
                catch (Exception e)
                {
                    Console.WriteLine($"Running {priest.Name} failed");
                    Console.WriteLine(e);
                }
            }

            foreach (Viking hoarder in roles[Constants.ROLE_HOARDER])
            {
                try { RunHoarder(hoarder); }
                catch (Exception e)
                {
                    Console.WriteLine($"Running {hoarder.Name} failed");
                    Console.WriteLine(e);
                }
            }

            Room.Visual.Text(
                EnergyUsed.ToString("0.00"),
                new FractionalPosition( (double)Position.X, (double)Position.Y ),
                new TextVisualStyle()
                {
                    Color = new Color(255, 255, 0, 0),
                    Opacity = 1.0
                }
            );
        }

        void RunPriest(Viking priest)
        {
            if (priest.Store[ResourceType.Energy.ToString()] > 0)
            {
                // Priest #2 does the chores
                if( roles[Constants.ROLE_WORKER].IndexOf( priest ) == 1 )
                {
                    if(Site != null)
                    {
                        priest.Build(Site);
                        return;
                    }                   
                    
                    if( Container != null &&
                        Container.Hits < Container.HitsMax )
                        priest.Repair( Container );

                }
                
                priest.Pray(Controller);
            }
            else
            {
                WorkerTask task = hold.GetEnergy();

                priest.Task = task;
                task.ResourceNeed -= priest.Store[ Constants.RESOURCE_CAPACITY ];

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
                                    priest.Take(targetStructure, ResourceType.Energy, task.amount);
                                }
                            break;
                        case TaskType.Take:
                            targetCreep = task.target as ICreep;
                            if (targetCreep != null)
                            {
                                priest.Take(targetCreep, ResourceType.Energy,task.amount);
                            }
                            break;
                        case TaskType.Fish:
                            ISource targetSource = task.target as ISource;
                            if (targetSource != null)
                            {
                                priest.Harvest(targetSource);
                            }
                            break;
                        default:
                            Console.WriteLine($"Priest {priest.Name} has no task!");
                            break;
                    }

                }
            }
        }
    
        void RunHoarder( Viking hoarder )
        {
            if( hoarder.Store[ ResourceType.Energy.ToString() ] == 0 )
            {
                WorkerTask task = hold.GetEnergy();

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
                foreach (Viking priest in roles[Constants.ROLE_WORKER])
                {
                    Viking bestViking = null;
                    int mostCapacity = 0;

                    if( priest.Store[ Constants.RESOURCE_CAPACITY ] > mostCapacity )
                    {
                        mostCapacity = priest.Store[ Constants.RESOURCE_CAPACITY ];
                        bestViking = priest;
                    }

                    if( bestViking != null )                    
                        hoarder.Transfer( priest.Creep, ResourceType.Energy, priest.Store[ Constants.RESOURCE_CAPACITY ]);
                }
            }
        }
    }
}