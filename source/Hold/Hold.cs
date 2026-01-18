using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;
using System.Net.Http.Headers;
using System.Reflection.Metadata;

namespace RagnarokBot
{

    public partial class Hold
    {
        IGame game = Program.game;
        public IRoom Room;
        public IWithStore PrimaryStorage;
        public List<Viking> Population = new List<Viking>();        
        Dictionary<string, List<Viking>> roles = new Dictionary<string, List<Viking>>();
        List<WorkerTask> tasks = new List<WorkerTask>();
        List<IStructureSpawn> spawns = new List<IStructureSpawn>();
        List<SpawnRequest> spawnRequests = new List<SpawnRequest>();
        Longhouse longHouse;
        Village[] villages;
        List<Pond> ponds = new List<Pond>();
        Shrine shrine;
        List<Settlement> settlements = new List<Settlement>();
        WorkerTask TakeFromStorageTask;

        public Hold(IRoom room)
        {
            Room = room;

            foreach (string role in Constants.ROLES)
                roles[role] = new List<Viking>();

            foreach (IStructureSpawn spawn in game.Spawns.Values)
                if (spawn.Room.Name == Room.Name)
                    spawns.Add(spawn);

            foreach (ICreep creep in game.Creeps.Values)
            {
                creep.Memory.TryGetString("hold", out string hold);
                if (hold == Room.Name)
                {
                    Viking viking;

                    creep.Memory.TryGetString("role", out string role);
                    creep.Memory.TryGetString("home", out string home);

                    if (role == null)
                        role = "Unassigned";

                    if (home == null)
                        home = "hold";

                    viking = new Viking(creep);

                    if (home == "hold")
                        roles[role].Add(viking);
                    Population.Add(new Viking(creep));


                }
            }

            if( Room.Storage != null )
                PrimaryStorage = Room.Storage;

            int i = 0;
            foreach (ISource source in Room.Find<ISource>(false))
            {
                Pond pond = new Pond(source, i++, this);
                ponds.Add(pond);
                settlements.Add(pond);
            }

            shrine = new Shrine(this);
            longHouse = new Longhouse(this);
            villages = new Village[2];
            villages[0] = new Village(this, 0);
            villages[1] = new Village(this, 1);

            settlements.Add(shrine);
            settlements.Add(longHouse);
            settlements.Add(villages[0]);
            settlements.Add(villages[1]);

        }

        public void Run()
        {
            DistributeEnergy();
            ManageRecruitment();

            RunWorkers();
            try { longHouse.Run(); } catch (Exception e) { Console.WriteLine(e); }
            try { shrine.Run(); } catch (Exception e) { Console.WriteLine(e); }
            foreach (Pond pond in ponds)
                try { pond.Run(); } catch (Exception e) { Console.WriteLine(e); }

            Report();
        }


        void RunWorkers()
        {
            string debugMessage = "";

            foreach (Viking worker in roles[Constants.ROLE_WORKER])
            {
                try
                {
                    RunWorker(worker);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error running worker {worker.Name}: {e.Message}");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(debugMessage);
                }
            }
        }

        void RunWorker(Viking worker)
        {
            if (worker.Store[Constants.RESOURCE_CAPACITY] == 0)
            {
                IStructureSpawn Spawn = new List<IStructureSpawn>(Room.Find<IStructureSpawn>(true))[0];

                if (Spawn.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                {
                    worker.Transfer(Spawn, ResourceType.Energy, worker.Store[ResourceType.Energy.ToString()]);
                    return;
                }
                
                List<IStructureExtension> extensions = new List<IStructureExtension>(Room.Find<IStructureExtension>(true));

                foreach( IStructureExtension ext in extensions)
                {
                    if (ext.Store.GetFreeCapacity(ResourceType.Energy) > 0 )
                    {
                        worker.Transfer(ext, ResourceType.Energy, worker.Store[ResourceType.Energy.ToString()]);
                        return;
                    }
                }
            }
            else
            {
                worker.Task = GetEnergy(worker);

                if (worker.Task == null) return;

                IStructure targetStructure;
                ICreep targetCreep;

                switch (worker.Task.Type)
                {
                    case TaskType.Collect:
                        targetStructure = worker.Task.target as IStructure;
                        if (targetStructure != null)
                        {
                            worker.Take(targetStructure, ResourceType.Energy, worker.Task.amount);
                        }
                        break;
                    case TaskType.Take:
                        targetCreep = worker.Task.target as ICreep;
                        if (targetCreep != null)
                        {
                            worker.Take(targetCreep, ResourceType.Energy, worker.Task.amount);
                        }
                        break;
                    case TaskType.Fish:
                        ISource targetSource = worker.Task.target as ISource;
                        if (targetSource != null)
                        {
                            worker.Harvest(targetSource);
                        }
                        break;
                    default:
                        Console.WriteLine($"Worker {worker.Name} has no task!");
                        break;
                }
            }

        }

        void Report()
        {
            IRoomVisual visual = Room.Visual;
            visual.Rect( new FractionalPosition(0, 0), 5, 5,new RectVisualStyle(){ Fill = new Color( 128, 255, 255, 255 ) } );
            
            #region Draw Energy Bars
            double w = 4.6;
            double x = 0.2;
            double energybarWidth = ponds.Count * 10;
            visual.Rect( new FractionalPosition(0.1, 0.1), w + 0.2, 0.8,new RectVisualStyle(){ Fill = new Color( 128, 0, 0, 0 ) } );

            double availableEnergy = 0.0;
            foreach (Pond pond in ponds)
                availableEnergy += pond.Output;
            visual.Rect( new FractionalPosition( x, 0.15), w * availableEnergy / energybarWidth, 0.35,new RectVisualStyle(){ Fill = new Color( 255, 255, 255, 0 ) } );
            
            visual.Rect( new FractionalPosition(x, 0.5), w * shrine.EnergyInput / energybarWidth , 0.35,new RectVisualStyle(){ Fill = new Color( 255, 128, 128, 255 ) } );
            x += w * shrine.EnergyInput / energybarWidth;

            visual.Rect( new FractionalPosition(x, 0.5), w * longHouse.EnergyInput / energybarWidth , 0.35,new RectVisualStyle(){ Fill = new Color( 255, 128, 255, 128 ) } );
            x += w * longHouse.EnergyInput / energybarWidth;

            double creepCost = 0;
            foreach (Viking viking in Population)
                creepCost += Trainer.GetBodysetCost(viking.Creep.Body );

            visual.Rect( new FractionalPosition(x, 0.5), w * creepCost / 1500.0 / energybarWidth , 0.35,new RectVisualStyle(){ Fill = new Color( 255, 192, 192, 192 ) } );


            visual.Text( "Energy", new FractionalPosition(0.2, 1.3), new TextVisualStyle(){ Color = new Color(255,255,255,255), Font = "0.5 Arial", Align = TextAlign.Left } );
            #endregion

            #region Draw Spawn Bar
            w = 4.6;
            x = 0.2;
            double spawnbarWidth = 1500.0 * spawns.Count;
            double utilization = 0.0;

            visual.Rect( new FractionalPosition(0.1, 1.5), w + 0.2, 0.22,new RectVisualStyle(){ Fill = new Color( 128, 0, 0, 0 ) } );

            double spawnCost = 0.0;
            foreach (Viking viking in Population)
                foreach( var bodypart in viking.Creep.Body )
                    spawnCost += 3.0;
            utilization += spawnCost;

            visual.Rect( new FractionalPosition(x, 1.55), w * spawnCost / spawnbarWidth , 0.1,new RectVisualStyle(){ Fill = new Color( 255, 128, 128, 0 ) } );
            x += w * spawnCost / spawnbarWidth;

            spawnCost = 0.0;
            foreach (var request in spawnRequests)
                foreach( var bodypart in request.Body )
                    spawnCost += 3.0;
            utilization += spawnCost;

            visual.Rect( new FractionalPosition(x, 1.55), w * spawnCost / spawnbarWidth , 0.1,new RectVisualStyle(){ Fill = new Color( 192, 140, 140, 0 ) } );
            x += w * spawnCost / spawnbarWidth;

            utilization /= spawnbarWidth;
            visual.Text( $"Spawn capacity: {utilization:F1}", new FractionalPosition(0.1, 2.1), new TextVisualStyle(){ Color = new Color(255,255,255,255), Font = "0.4 Arial", Align = TextAlign.Left } );
            #endregion

            visual.Text( $"Population: {Population.Count} (+{ spawnRequests.Count })", new FractionalPosition(0.1, 2.6), new TextVisualStyle(){ Color = new Color(255,255,255,255), Font = "0.4 Arial", Align = TextAlign.Left } );
            

            Console.WriteLine($"{Room.Name} finished gracefully");
        }

        public WorkerTask GetEnergy(Viking viking)
        {
            if( PrimaryStorage != null && 
                PrimaryStorage.Store.GetUsedCapacity( ResourceType.Energy ) > 0 )
            {
                if( TakeFromStorageTask != null )
                    return TakeFromStorageTask;
                TakeFromStorageTask = new WorkerTask()
                {
                    taskId = "Empty_Storage",
                        target = PrimaryStorage as IStructure,
                        Type = TaskType.Collect,
                        ResourceType = Constants.RESOURCE_CAPACITY,
                        Severity = 0,
                        ResourceNeed = viking.CarryCapacity,
                        amount = viking.CarryCapacity
                };
                return TakeFromStorageTask;
            }

            List<WorkerTask> availableTasks = new List<WorkerTask>();
            foreach (Pond pond in ponds)
            {
                WorkerTask task = pond.GetEnergy();
                if (task != null && task.ResourceNeed > 0)
                    availableTasks.Add(task);
            }

            if (availableTasks.Count == 0)
                return null;

            WorkerTask bestTask = null;
            int closestTask = 500;

            foreach (WorkerTask task in availableTasks)
                if (viking.Pos.LinearDistanceTo(task.target.RoomPosition.Position) < closestTask)
                {
                    closestTask = viking.Pos.LinearDistanceTo(task.target.RoomPosition.Position);
                    bestTask = task;
                }

            return bestTask;
        }

        void DistributeEnergy()
        {
            double availableEnergy = 0.0;
            foreach (Pond pond in ponds)
                availableEnergy += pond.Output;
            
            double energyLeft = availableEnergy;


            double creepCost = 0;
            foreach (Viking viking in Population)
                creepCost += Trainer.GetBodysetCost(viking.Creep.Body ) / 1500.0;
            
            energyLeft -= creepCost;

            if( longHouse.EnergyNeed > 0 ) 
            {
                longHouse.EnergyInput = Math.Min(8, 0.4 * energyLeft);
                energyLeft -= longHouse.EnergyInput;
            }
            
            if( Room.Storage != null )
            {
                if(Room.Storage.Store.GetUsedCapacity( ResourceType.Energy ) < 50000 )
                {
                    energyLeft -= Math.Min(10, 0.3 * energyLeft);
                }
                
            }
                

            shrine.EnergyInput = Math.Min(15, energyLeft);

            
        }

        void ManageRecruitment()
        {
            Dictionary<string, List<SpawnRequest>> requests = new Dictionary<string, List<SpawnRequest>>();

            requests.Add("self", OwnRecruitmentRequest());

            foreach (Pond pond in ponds)
                requests.Add(pond.Name, pond.GetSpawnRequest());

            requests.Add(longHouse.Name, longHouse.GetSpawnRequest());
            requests.Add(shrine.Name, shrine.GetSpawnRequest());

            foreach (var request in requests)    
                if (request.Value != null && request.Value.Count > 0)
                    spawnRequests.AddRange( request.Value );

            foreach (var request in requests)        
                if (request.Value != null && request.Value.Count > 0)
                {
                    longHouse.SpawnViking(request.Value[0]);
                    break;
                }

        }

        List<SpawnRequest> OwnRecruitmentRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();


            if (Population.Count <= 1)
            {
                BodyPartType[] body = [BodyPartType.Work, BodyPartType.Carry, BodyPartType.Move];
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_WORKER);
                initialMemory.SetValue("hold", Room.Name);

                request.Add(
                     new SpawnRequest()
                     {
                         Body = body,
                         InitialMemory = initialMemory
                     });
                return request;
            }

            return null;
        }
    };



}