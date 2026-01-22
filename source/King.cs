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
            foreach( IFlag flag in game.Flags.Values )
            {
                switch( flag.Color )
                {
                    case FlagColor.Green:
                        Conqer conqer = new Conqer( flag );
                        break;

                    default:
                        break;
                }
            }


            holds = new List<Hold>();

            
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
            
            foreach( IStructureSpawn spawn in Program.game.Spawns.Values )
                holds.Add( new Hold( spawn.Room ) );
            foreach( Hold hold in holds )
                hold.Run();           
            

            Console.WriteLine($"King year of {Program.game.Time} ended gracefully! CPU:{ Program.game.Cpu.GetUsed() }");
        }

    };
}