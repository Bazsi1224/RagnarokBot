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
        public List<Viking> Population = new List<Viking>();
        Dictionary<string, List<Viking>> roles = new Dictionary<string, List<Viking>>();
        List<WorkerTask> tasks = new List<WorkerTask>();
        List<IStructureSpawn> spawns = new List<IStructureSpawn>();

        Longhouse longHouse;
        Village[] villages;
        List<Pond> ponds = new List<Pond>();
        Shrine shrine;

        public Hold(IRoom room)
        {
            Room = room;

            foreach (string role in Constants.ROLES)
                roles[role] = new List<Viking>();

            foreach (IStructureSpawn spawn in game.Spawns.Values)
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

            int i = 0;
            foreach (ISource source in Room.Find<ISource>(false))
            {
                ponds.Add(new Pond(source, i++, this));
            }

            shrine = new Shrine(this);
            longHouse = new Longhouse(this);
            villages = new Village[2];
            villages[0] = new Village(this, 0);
            villages[1] = new Village(this, 1);
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
            Console.WriteLine($"{Room.Name} finished gracefully");
        }

        public WorkerTask GetEnergy(Viking viking)
        {
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

            if( longHouse.EnergyNeed > 0 ) 
            {
                longHouse.EnergyInput = Math.Min(8, 0.4 * energyLeft);
                energyLeft -= longHouse.EnergyInput;
            }
            
            if( Room.Storage != null )
                shrine.EnergyInput = Math.Min(8, 0.2 * energyLeft);
            else
                shrine.EnergyInput = Math.Min(14, 0.6 * energyLeft);
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