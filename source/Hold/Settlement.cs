using System;
using System.Collections.Generic;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet;

namespace RagnarokBot
{
    public class Settlement
{       protected string settlementName = "settlement";
        protected IGame game = Program.game;
        protected Hold hold;
        protected IRoom Room;
        protected IMemoryObject SettlementMemory;
        protected Position Position;
        protected Direction Orientation;
        public List<Viking> Population = new List<Viking>();
        protected Dictionary<string, List<Viking>> roles = new Dictionary<string, List<Viking>>();

        public int Width = 1;
        public int Height = 1;

        public string Name {get { return settlementName; }}

        public Settlement(Hold hold)
        {
            this.hold = hold;
            Room = hold.Room;
        }

        protected void GetLocation( )
        {
            if( SettlementMemory == null )
                return;

            if( SettlementMemory.TryGetString( "position", out string posString ) )
            {
                var coords = posString.Split(',');
                Position = new Position( int.Parse( coords[0] ), int.Parse( coords[1] ) );
            }
            else
            {
                PlanPosition( );
                SettlementMemory.SetValue( "orientation", $"{(int)Orientation}" );
                SettlementMemory.SetValue( "position", $"{Position.X},{Position.Y}" );
            }


            if( SettlementMemory.TryGetString( "orientation", out string orientationString ) )
            {
                Orientation = (Direction)int.Parse( orientationString );
            }
            else
            {
                PlanPosition( );    
                SettlementMemory.SetValue( "orientation", $"{(int)Orientation}" );
                SettlementMemory.SetValue( "position", $"{Position.X},{Position.Y}" );  
            }

            
        }

        public virtual bool PlanPosition( ) { return false; }

        protected void DrawVisual( Color fillColor )
        {
            int w = Width;
            int h = Height;

            if( Orientation == Direction.Left || Orientation == Direction.Right )
            {
                w = Height;
                h = Width;
            }

            IRoomVisual visual = Room.Visual;
            visual.Rect(
                new FractionalPosition( Position.X - (w/2.0) , Position.Y - (h/2.0) ),
                w,h,
                new RectVisualStyle()
                {
                    Fill = fillColor
                });
        }

        protected void LookForPopulation( )
        {
            foreach( string role in Constants.ROLES )
                roles[role] = new List<Viking>(); 

            foreach( Viking viking in hold.Population )
            {
                if( viking.Home == settlementName)
                {
                    Population.Add( viking );
                    roles[viking.Role].Add( viking ); 
                }
            }
        }

        public bool IsPositionInside( Position pos )
        {        
            switch( Orientation )
            {
                case Direction.Top:
                    return Math.Abs( pos.X - Position.X ) < Width && Math.Abs( pos.Y - Position.Y ) < Height;
                case Direction.Right:
                    return Math.Abs( pos.X - Position.X ) < Height && Math.Abs( pos.Y - Position.Y ) < Width;
                case Direction.Bottom:
                    return Math.Abs( pos.X - Position.X ) < Width && Math.Abs( pos.Y - Position.Y ) < Height;
                case Direction.Left:
                    return Math.Abs( pos.X - Position.X ) < Height && Math.Abs( pos.Y - Position.Y ) < Width;
                default:
                    return false;
            }
        }

        public Position GetRelativePosition( int xOffset, int yOffset )
        {
            int x = Position.X;
            int y = Position.Y;

            switch( Orientation )
            {
                case Direction.Top:
                    x += xOffset;
                    y += yOffset;
                    break;
                case Direction.Right:
                    x += yOffset;
                    y -= xOffset;
                    break;
                case Direction.Bottom:
                    x -= xOffset;
                    y -= yOffset;
                    break;
                case Direction.Left:
                    x -= yOffset;
                    y += xOffset;
                    break;
            }

            return new Position( x, y );
        }

}
}