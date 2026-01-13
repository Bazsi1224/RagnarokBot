using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;
using System.ComponentModel;

namespace RagnarokBot
{
    public class Pond : Settlement
    {
        const int SIZE = 5;

        public ISource Source;
        int Number;
        IStructureLink Link;
        IStructureContainer Container;
        IConstructionSite Site;

        WorkerTask DepositTask;

        int places = 0;
        public int StoredEnergy = 0;
        public double Output = 0;

        public Pond(ISource source, int number, Hold hold) : base(hold)
        {
            Number = number;
            settlementName = $"Fishing camp {number}";

            Source = source;
            LookForPopulation();

            Width = SIZE;
            Height = SIZE;
            Position = Source.RoomPosition.Position;

            FindStructures();

            if (Container != null &&
                Container.Store.GetUsedCapacity(ResourceType.Energy) > 0)
            {
                int energy = Container.Store.GetUsedCapacity(ResourceType.Energy) ?? 0;

                hold.RequestTask(
                    new WorkerTask()
                    {
                        taskId = "Empty_Container_" + settlementName,
                        target = Container,
                        Type = TaskType.Collect,
                        ResourceType = Constants.RESOURCE_CAPACITY,
                        Severity = 0,
                        ResourceNeed = energy,
                        amount = energy
                    }
                    );
            }

            foreach (Viking fisher in roles[Constants.ROLE_FISHER])
                if (fisher.Pos.IsNextTo(Source.RoomPosition.Position))
                    Output += fisher.HarvestCapacity;

            Output = Math.Min(10.0, Output);

            //Console.WriteLine( $"{settlementName} capacity is {Output}" );
        }

        public List<SpawnRequest> GetSpawnRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();

            int HarvestCapacity = 0;

            foreach( Viking fisher in roles[Constants.ROLE_FISHER] )
                HarvestCapacity += fisher.HarvestCapacity;
            
            if ( HarvestCapacity < 10 )
            {
                BodyPartType[] body = GetWorkerBody();
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", "Fisher");
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
            RunFishers();


            Room.Visual.Text(
                Output.ToString("0.00"),
                new FractionalPosition( (double)Position.X, (double)Position.Y ),
                new TextVisualStyle()
                {
                    Color = new Color(255, 0, 255, 0),
                    Opacity = 1.0
                }
            );
        }

        void RunFishers()
        {
            string debugMessage = "";

            foreach (Viking fisher in roles[Constants.ROLE_FISHER])
            {
                if (fisher.Store[Constants.RESOURCE_CAPACITY] == 0)
                    DepositFisher(fisher);
                else
                    fisher.Harvest(Source);
            }
        }

        void DepositFisher(Viking fisher)
        {
            if (Container != null)
            {
                if (Container.Hits < Container.HitsMax)
                {
                    fisher.Repair(Container);
                    return;

                }

                fisher.Transfer(Container, ResourceType.Energy, fisher.Store[ResourceType.Energy.ToString()]);
                return;
            }


            if (Site != null)
            {
                fisher.Build(Site);
                return;
            }


        }

        void FindStructures()
        {
            if (Container == null)
            {
                var containers = Room.Find<IStructureContainer>();

                foreach (IStructureContainer container in containers)
                {
                    if (Source.RoomPosition.Position.LinearDistanceTo(container.RoomPosition.Position) <= 2)
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

                    if (Source.RoomPosition.Position.LinearDistanceTo(containerSite.RoomPosition.Position) <= 2 &&
                        containerSite.StructureType.ToString() == "ScreepsDotNet.API.World.IStructureContainer")
                    {
                        Site = containerSite;
                        return;
                    }
                }
            }
        }

        public WorkerTask GetEnergy()
        {
            if( DepositTask != null ) return DepositTask;

            if (Container != null)
            {
                int energy = Container.Store.GetUsedCapacity(ResourceType.Energy) ?? 0;

                if (energy > 0)
                {
                    DepositTask = new WorkerTask()
                    {
                        taskId = "Empty_Container_" + settlementName,
                        target = Container,
                        Type = TaskType.Collect,
                        ResourceType = Constants.RESOURCE_CAPACITY,
                        Severity = 0,
                        ResourceNeed = energy,
                        amount = energy
                    };
                    return DepositTask;
                }


            }

            return new WorkerTask()
            {
                taskId = "Fish_pond_" + settlementName,
                        target = Source,
                        Type = TaskType.Fish,
                        ResourceType = Constants.RESOURCE_CAPACITY,
                        Severity = 0,
                        ResourceNeed = 3000,
                        amount = 3000
                    };
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
    }
}