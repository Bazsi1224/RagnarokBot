using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;
using ScreepsDotNet;

namespace RagnarokBot
{   

    public class King
    {
        IGame game = Program.game;
        List<Hold> holds;

        public void Loop()
        {

            holds = new List<Hold>();

            foreach( IStructureSpawn spawn in Program.game.Spawns.Values )
                holds.Add( new Hold( spawn.Room ) );
            
            IMemoryObject creepsMemory;

            if( Program.game.Memory.TryGetObject("creeps", out creepsMemory ) )
                foreach( string vikingName in creepsMemory.Keys )
                {
                    if( !Program.game.Creeps.ContainsKey( vikingName ) )
                    {
                        Console.WriteLine( $"Removing memory of deceased viking {vikingName}" );
                        creepsMemory.ClearValue( vikingName );
                    }
                }
            
            Run();

            Console.WriteLine($"King year of {Program.game.Time} ended gracefully! CPU:{ Program.game.Cpu.GetUsed() }");
        }


        void Run()
        {
            foreach( Hold hold in holds )
                hold.Run();
        }

    };
}