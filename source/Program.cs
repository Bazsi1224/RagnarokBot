using System;
using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        public static IGame? game;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
                game = new Native.World.NativeGame();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Loop()
        {
            if (game == null) { return; }
            try
            {
                game.Tick();

                RagnarokBot.King king = new RagnarokBot.King();
                king.Loop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}