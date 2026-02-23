using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Helpers
{
    public static class EntityType
    {
        public const string Agent = "A";
        public const string Client = "C";
        public const string Owner = "O";

        public static string GetEntityType(Type t)
        {
            if (t == typeof(Agent))
                return EntityType.Agent;
            if (t == typeof(Client))
                return EntityType.Client;
            if (t == typeof(Owner))
                return EntityType.Owner;

            return "";
        }

        public static bool IsEntityFilterType(Type t, CPiEntityType entityFilterType)
        {
            if (entityFilterType == CPiEntityType.None)
                return false;
            if (entityFilterType == CPiEntityType.Agent && t == typeof(Agent))
                return true;
            if (entityFilterType == CPiEntityType.Client && t == typeof(Client))
                return true;
            if (entityFilterType == CPiEntityType.Owner && t == typeof(Owner))
                return true;

            return false;
        }
    }
}
