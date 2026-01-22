using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;


namespace RagnarokBot
{
    class Longboat
    {
        protected IGame game = Program.game;
        protected IFlag Flag;
        protected List<Viking> Crew = new List<Viking>();
        protected Dictionary<string, List<Viking>> roles = new Dictionary<string, List<Viking>>();
        public List<SpawnRequest> SpawnRequests = new List<SpawnRequest>();

        public Longboat( IFlag flag)
        {
            Flag = flag;

            
        }        
    }
}