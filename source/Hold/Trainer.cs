using System;
using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace RagnarokBot
{
    public static class Trainer
    {
        public static string GetRandomName()
        {
            return NAMES[ new Random().Next( NAMES.Length ) ];
        }

        public static int GetBodysetCost( BodyPartType[] body )
        {
            int cost = 0;
            foreach( BodyPartType part in body )
                cost += ScreepsDotNet.Program.game.Constants.GetBodyPartCost( part );

            return cost;
        }

        static readonly string[] NAMES = {
            "Olaf", "Erik", "Leif", "Harald", "Ragnar", "Bjorn", "Ivar", "Sigurd", "Gunnar", "Ulf",
            "Astrid", "Freya", "Ingrid", "Sigrid", "Helga", "Thyra", "Gunnhild", "Kari", "Liv", "Runa",
            "Berak", "Eirik", "Finn", "Hakon", "Knut", "Magnus", "Roar", "Sven", "Toke", "Viggo", "Yngve",
            "Alfhild", "Brynhild", "Dagmar", "Eydis", "Frida", "Gudrun", "Hilda", "Jorunn", "Kelda", "Lagertha",
            "Torsten", "Vebjorn", "Agnar", "Bard", "Egil", "Halfdan", "Ketil", "Njord", "Orvar", "Sten", "Trygve"
        };
    }
}