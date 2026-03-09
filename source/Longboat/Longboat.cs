using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;


namespace RagnarokBot
{
    public class Longboat
    {
        protected IGame game = Program.game;
        protected IFlag Flag;
        protected List<Viking> Crew = new List<Viking>();
        protected IMemoryObject memory;
        protected Dictionary<string, List<Viking>> roles = new Dictionary<string, List<Viking>>();
        public List<SpawnRequest> SpawnRequests = new List<SpawnRequest>();

        public string Name { get { return Flag.Name; } }

        public Longboat( IFlag flag)
        {
            Flag = flag;

            memory = flag.Memory;
            LookForCrew();
        }      

        protected void LookForCrew( )
        {
            foreach( string role in Constants.ROLES )
                roles.Add( role, new List<Viking>() );

            foreach (ICreep creep in game.Creeps.Values)
            {
                creep.Memory.TryGetString("hold", out string hold);
                if (hold == Flag.Name)
                {
                    Viking viking;

                    creep.Memory.TryGetString("role", out string role);
                    creep.Memory.TryGetString("home", out string home);

                    if (role == null)
                        role = "Unassigned";

                    if (home == null)
                        home = "hold";

                    viking = new Viking(creep);

                    Crew.Add( viking );
                    roles[role].Add(viking);                    

                }
            }

            Console.WriteLine( $"Found {Crew.Count} vikings for {Flag.Name}" );
            
        }
  
    }
}