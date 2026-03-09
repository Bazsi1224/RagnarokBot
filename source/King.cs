using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;
using ScreepsDotNet;

namespace RagnarokBot
{   

    public class King
    {
        IGame game = Program.game;
        List<Hold> holds = new List<Hold>();
        List<Longboat> longboats = new List<Longboat>();

        public void Loop()
        {
            foreach( IFlag flag in game.Flags.Values )
            {
                try{
                switch( flag.Color )
                {
                    case FlagColor.Green:
                        Conqer conqer = new Conqer( flag );
                        longboats.Add( conqer );
                        break;

                    default:
                        break;
                }
                }
                catch(Exception exp)
                {
                    Console.WriteLine("Longboat exception " + exp);
                }
            }


            if( Program.game.Memory.TryGetObject("creeps", out IMemoryObject creepsMemory ) )
                foreach( string vikingName in creepsMemory.Keys )
                {
                    if( !Program.game.Creeps.ContainsKey( vikingName ) )
                    {
                        Console.WriteLine( $"Removing memory of deceased viking {vikingName}" );
                        creepsMemory.ClearValue( vikingName );
                    }
                }

            if( Program.game.Memory.TryGetObject("flags", out IMemoryObject flagsMemory ) )
                foreach( string flagName in flagsMemory.Keys )
                {
                    if( !Program.game.Flags.ContainsKey( flagName ) )
                    {
                        Console.WriteLine( $"Removing memory of deceased longboat {flagName}" );
                        flagsMemory.ClearValue( flagName );
                    }
                }
            
            foreach( IStructureSpawn spawn in Program.game.Spawns.Values )
                holds.Add( new Hold( spawn.Room ) );
            
            AssignHoldsToLongboats();

            foreach( Hold hold in holds )                
                hold.Run();                       
            

            Console.WriteLine($"King year of {Program.game.Time} ended gracefully! CPU:{ Program.game.Cpu.GetUsed() }");
        }

        void AssignHoldsToLongboats()
        {
            foreach( Hold hold in holds )
            {
                foreach( Longboat longboat in longboats )
                    hold.LongboatsToSupport.Add( longboat );
            }
        }

    };
}