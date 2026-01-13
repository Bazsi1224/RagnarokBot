using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;

namespace RagnarokBot
{
    public class Village : Settlement
    {
        public const int WIDTH = 5;
        public const int HEIGHT = 7;

        public Village( Hold hold, int number ) : base( hold )
        {

            Width = WIDTH;
            Height = HEIGHT;

            SettlementMemory = Room.Memory.GetOrCreateObject($"village{number}");
            GetLocation( );
            //DrawVisual( new Color(30, 255, 255, 0) ); // "rgba(255, 255, 0, 0.1)"

        }

        public override bool PlanPosition( )
        {
            foreach( IStructureSpawn spawn in Room.Find<IStructureSpawn>( true ) )
                Position = spawn.RoomPosition.Position;
            
            Orientation = Direction.Left;
            return true;
        }


    }
}