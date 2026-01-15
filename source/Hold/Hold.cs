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

        public Hold( IRoom room )
        {
            Room = room;

            foreach( string role in Constants.ROLES )
                roles[role] = new List<Viking>();

            foreach( IStructureSpawn spawn in game.Spawns.Values )
                spawns.Add( spawn );

            foreach (ICreep creep in game.Creeps.Values )
            {
                creep.Memory.TryGetString("hold", out string hold );
                if( hold == Room.Name )
                {
                    Viking viking;
                    
                    creep.Memory.TryGetString("role", out string role );
                    creep.Memory.TryGetString("home", out string home );

                    if( role == null )
                        role = "Unassigned";
                    
                    if( home == null )
                        home = "hold";

                    viking = new Viking( creep );

                    if( home == "hold" )
                        roles[role].Add( viking );          
                    Population.Add( new Viking( creep ) );


                }
            }
            
            int i = 0;
            foreach( ISource source in Room.Find<ISource>(false) )
            {
                ponds.Add( new Pond(source, i++, this) );
            }

            shrine = new Shrine(this);
            longHouse = new Longhouse( this );
            villages = new Village[2];
            villages[0] = new Village( this, 0 );
            villages[1] = new Village( this, 1 );
        }

        public void Run()
        {
            ManageRecruitment();

            RunWorkers();
            try{ longHouse.Run();} catch( Exception e ) { Console.WriteLine( e ); }
            try{ shrine.Run();} catch( Exception e ) { Console.WriteLine( e ); }        
            foreach( Pond pond in ponds )
                try{ pond.Run();} catch( Exception e ) { Console.WriteLine( e ); }

            Report();            
        }


        void RunWorkers()
        {
            string debugMessage = "";

            foreach( Viking worker in roles[ Constants.ROLE_WORKER ] )
            {
                try
                {
                    RunWorker( worker );
                }
                catch( Exception e )
                {
                    Console.WriteLine( $"Error running worker {worker.Name}: {e.Message}" );
                    Console.WriteLine( e.StackTrace );
                    Console.WriteLine( debugMessage );
                }
            }
        }

        void RunWorker( Viking worker )
        {
            if( worker.Store[Constants.RESOURCE_CAPACITY] == 0 )
            {
                IStructureSpawn Spawn = new List<IStructureSpawn>(Room.Find<IStructureSpawn>(true))[0];

                worker.Transfer( Spawn, ResourceType.Energy, worker.Store[ResourceType.Energy.ToString()] );
            }
            else
            {
                worker.Task = GetEnergy();

                if( worker.Task == null ) return;

                IStructure targetStructure;
                ICreep targetCreep;

                switch( worker.Task.Type )
                {
                    case TaskType.Collect:
                        targetStructure = worker.Task.target as IStructure;
                        if( targetStructure != null )
                        {
                            worker.Take( targetStructure, ResourceType.Energy, worker.Task.amount );
                        }
                        break;
                    case TaskType.Take:
                        targetCreep = worker.Task.target as ICreep;
                        if( targetCreep != null )
                        {
                            worker.Take( targetCreep, ResourceType.Energy, worker.Task.amount );
                        }
                        break;
                    case TaskType.Fish:
                        ISource targetSource = worker.Task.target as ISource;
                        if( targetSource != null )
                        {
                            worker.Harvest( targetSource );
                        }
                        break;
                    default:
                        Console.WriteLine( $"Worker {worker.Name} has no task!" );
                        break;
                }
            }

        }

        void Report()
        {
            Console.WriteLine( $"{Room.Name} finished gracefully" );
        }

        public void RequestTask( WorkerTask task )
        {
            //Console.WriteLine( $"task {task.taskId} requested" );
            tasks.Add(task);
        }

        public WorkerTask GetEnergy()
        {
            foreach (Pond pond in ponds)
            {
                WorkerTask task = pond.GetEnergy();    
                if( task != null && task.ResourceNeed > 0 )
                    return task;            
            }
            return null;          
        }
    
        void ManageRecruitment()
        {
            double availableEnergy = 0.0;
            foreach( Pond pond in ponds )
                availableEnergy += pond.Output;

            shrine.EnergyInput = 0.8 * availableEnergy;

            Dictionary<string, List<SpawnRequest>> requests = new Dictionary<string, List<SpawnRequest>>();            

            requests.Add( "self", OwnRecruitmentRequest());

            foreach( Pond pond in ponds )
                requests.Add( pond.Name, pond.GetSpawnRequest());

            requests.Add( longHouse.Name, longHouse.GetSpawnRequest());
            requests.Add( shrine.Name, shrine.GetSpawnRequest());
            

            foreach( var request in requests )
                if( request.Value != null && request.Value.Count > 0 )
                {
                    longHouse.SpawnViking(request.Value[0]);
                    break;
                }
            
        }

        List<SpawnRequest> OwnRecruitmentRequest()
        {
            List<SpawnRequest> request = new List<SpawnRequest>();


            if( Population.Count <= 1 )
            {
                BodyPartType[] body = [BodyPartType.Work, BodyPartType.Carry, BodyPartType.Move];
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", Constants.ROLE_WORKER);
                initialMemory.SetValue("hold", Room.Name );
                
                request.Add(
                     new SpawnRequest()
                {
                Body = body, 
                InitialMemory = initialMemory} );                
                return request;
            }
            
            return null;
        }
    };



}