using System;
using System.Collections.Generic;
using ScreepsDotNet;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{
    public class Viking
    {
        static IGame game = ScreepsDotNet.Program.game;
        public ICreep Creep;
        public string Role;
        public string Home;
        public WorkerTask Task;
        public Dictionary<string, int> Store = new Dictionary<string, int>();

        public Position Pos
        {
            get { return Creep.RoomPosition.Position; }
        }

        public string Name
        {
            get { return Creep.Name; }
        }

        public int HarvestCapacity
        {
            get { return Creep.BodyType[BodyPartType.Work] * game.Constants.Creep.HarvestPower; }
        }
        public int PrayCapacity
        {
            get { return Creep.BodyType[BodyPartType.Work] * game.Constants.Creep.UpgradeControllerPower; }
        }
        public int BuildCapacity
        {
            get { return Creep.BodyType[BodyPartType.Work] * game.Constants.Creep.BuildPower; }
        }

        public Viking( ICreep? creep )
        {
            Creep = creep;

            creep.Memory.TryGetString( "role", out Role );

            if( Role == null )
                Role = "Unassigned";
            
            creep.Memory.TryGetString( "home", out Home );

            if( Role == null )
                Role = "Hold";

            Store[ResourceType.Energy.ToString()] = Creep.Store.GetUsedCapacity( ResourceType.Energy ) ?? 0;
            Store[Constants.RESOURCE_CAPACITY] = Creep.Store.GetFreeCapacity( ResourceType.Energy ) ?? 0;
        }

        public int MoveTo( Position target )
        {
            Creep.MoveTo( target );
            return 1;
        }

        public int Harvest( ISource source )
        {
            if( source == null )
            {
                Console.WriteLine( "No source to harvest from!" );
                return -1;
            }

            CreepHarvestResult ret = Creep.Harvest( source );

            switch( ret )
            {
                case CreepHarvestResult.NotInRange:
                    Creep.MoveTo( source.RoomPosition );
                    return 1;
                case CreepHarvestResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Harvest failed: {ret}" );
                    return -1;
            }
        }

        public int Pray( IStructureController controller )
        {
            if( controller == null )
            {
                Console.WriteLine( "No controller to pray to!" );
                return -1;
            }

            CreepUpgradeControllerResult ret = Creep.UpgradeController( controller );

            switch( ret )
            {
                case CreepUpgradeControllerResult.NotInRange:
                    Creep.MoveTo( controller.RoomPosition );
                    return 1;
                case CreepUpgradeControllerResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Pray failed: {ret}" );
                    return -1;
            }
        }

        public int Transfer( IStructure target, ResourceType resource, int? amount = -1 )
        {
            if( target == null )
            {
                Console.WriteLine( "No target to transfer to!" );
                return -1;
            }            

            if( Store[resource.ToString()] < amount )
            {
                amount = Store[resource.ToString()];
            }

            IWithStore targetwstore = target as IWithStore;
            if( targetwstore.Store.GetFreeCapacity(resource) < amount )
            {
                amount = targetwstore.Store.GetFreeCapacity(resource);
            }

            CreepTransferResult ret = Creep.Transfer( target, resource, amount );

            switch( ret )
            {
                case CreepTransferResult.NotInRange:
                    Creep.MoveTo( target.RoomPosition );
                    return 1;
                case CreepTransferResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Transfer failed: {ret}" );
                    return -1;
            }
        }

        public int Transfer( ICreep target, ResourceType resource, int? amount = -1 )
        {
            if( target == null )
            {
                Console.WriteLine( "No target to transfer to!" );
                return -1;
            }

            if( Store[resource.ToString()] < amount )
            {
                amount = Store[resource.ToString()];
            }
            if( target.Store.GetFreeCapacity(resource) < amount )
            {
                amount = target.Store.GetFreeCapacity(resource);
            }

            CreepTransferResult ret = Creep.Transfer( target, resource, amount );

            switch( ret )
            {
                case CreepTransferResult.NotInRange:
                    Creep.MoveTo( target.RoomPosition );
                    return 1;
                case CreepTransferResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Transfer failed: {ret}" );
                    return -1;
            }
        }

        public int Pickup( IResource resource )
        {
            if( resource == null )
            {
                Console.WriteLine( "No resource to pick up!" );
                return -1;
            }

            CreepPickupResult ret = Creep.Pickup( resource );

            switch( ret )
            {
                case CreepPickupResult.NotInRange:
                    Creep.MoveTo( resource.RoomPosition );
                    return 1;
                case CreepPickupResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Pickup failed: {ret}" );
                    return -1;
            }
        }

        public void Take( ICreep targetCreep, ResourceType resource, int? amount = -1 )
        {
            if( targetCreep == null )
            {
                Console.WriteLine( "No target viking to take from!" );
                return;
            }

            if( Store[Constants.RESOURCE_CAPACITY] < amount )
            {
                amount = Store[Constants.RESOURCE_CAPACITY];
            }
            
            CreepTransferResult ret = targetCreep.Transfer( this.Creep, resource, amount );

            switch( ret )
            {
                case CreepTransferResult.NotInRange:
                    Creep.MoveTo( targetCreep.RoomPosition );
                    return;
                case CreepTransferResult.Ok:
                    return;
                default:
                    Console.WriteLine( $"Take failed: {ret}" );
                    return;
            }
        }

        public void Take( IStructure target, ResourceType resource, int? amount = -1 )
        {
            if( target == null )
            {
                Console.WriteLine( "No target viking to take from!" );
                return;
            }

            if( Store[Constants.RESOURCE_CAPACITY] < amount )
            {
                amount = Store[Constants.RESOURCE_CAPACITY];
            }
            
            CreepWithdrawResult ret = this.Creep.Withdraw( target, resource, amount );

            switch( ret )
            {
                case CreepWithdrawResult.NotInRange:
                    Creep.MoveTo( target.RoomPosition );
                    return;
                case CreepWithdrawResult.Ok:
                    return;
                default:
                    Console.WriteLine( $"Take failed: {ret}" );
                    return;
            }
        }


        public int Build( IConstructionSite targetConstructionSite, bool moveToSite = true )
        {
            if( targetConstructionSite == null )
            {
                Console.WriteLine( "No target construction site to build!" );
                return -1;
            }

            CreepBuildResult ret = Creep.Build( targetConstructionSite );

            switch( ret )
            {
                case CreepBuildResult.NotInRange:
                    if( moveToSite ) 
                    {
                        Creep.MoveTo( targetConstructionSite.RoomPosition );
                        return 1;
                    }
                    return -1;
                case CreepBuildResult.Ok:
                    return 0;
                default:
                    Console.WriteLine( $"Build failed: {ret}" );
                    return -1;
            }
        }
        public void Repair( IStructure targetStructure )
        {
            if( targetStructure == null )
            {
                Console.WriteLine( "No target structure to repair!" );
                return;
            }

            CreepRepairResult ret = Creep.Repair( targetStructure );
            switch( ret )
            {
                case CreepRepairResult.NotInRange:
                    Creep.MoveTo( targetStructure.RoomPosition );
                    return;
                case CreepRepairResult.Ok:
                    return;
                default:
                    Console.WriteLine( $"Repair failed: {ret}" );
                    return;
            }
        }
    }
}