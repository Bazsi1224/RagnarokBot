using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{

    public static class Constants
    {
        public const string ROLE_WORKER = "Worker";
        public const string ROLE_FISHER = "Fisher";
        public const string ROLE_HOARDER = "Hoarder";
        public static readonly string[] ROLES = { ROLE_WORKER, ROLE_FISHER, ROLE_HOARDER };


        public const string RESOURCE_CAPACITY = "Capacity";
    }


    public enum TaskType
    {
        Fish,
        Build,
        Repair,
        Pray,
        Fill,
        Collect,
        Pickup,
        Take,
        Give
    }



    public class WorkerTask : IComparable<WorkerTask>
    {
        public string taskId;
        public IRoomObject target;
        public TaskType Type;
        public double Severity = 0;
        public List<Viking> assignedWorkers = new List<Viking>();
        public string ResourceType = "";
        public int ResourceNeed = 0;
        public int CapacityNeed = 0;
        public int amount = 0;
        public int CompareTo( WorkerTask? other )
        {
            if( other == null ) return 1;
            return other.Severity.CompareTo( this.Severity );
        }
    }

    public struct SpawnRequest
    {
        public BodyPartType[] Body;
        public IMemoryObject InitialMemory;
    }
}