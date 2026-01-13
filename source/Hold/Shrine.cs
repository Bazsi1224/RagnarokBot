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

            EnergyNeed = Controller.Level == 8 ? 15 : 5000;

            foreach (Viking priest in roles[Constants.ROLE_WORKER])
                EnergyUsed += priest.PrayCapacity;
        }

        public List<SpawnRequest> GetSpawnRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();

            if( roles[Constants.ROLE_WORKER].Count / 3 > roles[Constants.ROLE_HOARDER].Count )
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

            if (EnergyInput > EnergyUsed)
            {
                BodyPartType[] body = [BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work, BodyPartType.Work];
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
            if (priest.store[ResourceType.Energy.ToString()] > 0)
            {
                priest.Pray(Controller);
            }
            else
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
                foreach (Viking priest in roles[Constants.ROLE_WORKER])
                {
                    Viking bestViking = null;
                    int mostCapacity = 0;

                    if( priest.store[ Constants.RESOURCE_CAPACITY ] > mostCapacity )
                    {
                        mostCapacity = priest.store[ Constants.RESOURCE_CAPACITY ];
                        bestViking = priest;
                    }

                    if( bestViking != null )                    
                        hoarder.Transfer( priest.Creep, ResourceType.Energy, priest.store[ Constants.RESOURCE_CAPACITY ]);
                }
            }
        }
    }
}