using System;
using System.Collections.Generic;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Utils
{
    public static class ModelUtils
    {
        private static readonly Random Random = new Random();

        #region Constants

        internal static readonly List<string> CityPedModels = new List<string>
        {
            "s_m_y_cop_01",
            "s_f_y_cop_01"
        };

        internal static readonly List<string> CountyPedModels = new List<string>
        {
            "s_f_y_sheriff_01",
            "s_m_y_sheriff_01"
        };

        internal static readonly List<string> StatePedModels = new List<string>
        {
            "s_m_y_hwaycop_01"
        };

        internal static readonly List<string> CityVehicleModels = new List<string>
        {
            "POLICE",
            "POLICE2",
            "POLICE3"
        };

        internal static readonly List<string> CountyVehicleModels = new List<string>
        {
            "SHERIFF",
            "SHERIFF2"
        };

        internal static readonly List<string> StateVehicleModels = new List<string>
        {
            "POLICE3"
        };

        #endregion

        /// <summary>
        /// Get a local ped model for the given position.
        /// </summary>
        /// <param name="position">Set the position to get the local model for.</param>
        /// <returns>Returns the local ped model.</returns>
        public static Model GetLocalPed(Vector3 position)
        {
            var zone = GetZone(position);

            return IsCountyZone(zone)
                ? new Model(CityPedModels[Random.Next(CityPedModels.Count)])
                : new Model(CountyPedModels[Random.Next(CountyPedModels.Count)]);
        }

        /// <summary>
        /// Get a local vehicle model for the given position.
        /// </summary>
        /// <param name="position">Set the position to get the local model for.</param>
        /// <returns>Returns the local vehicle model.</returns>
        public static Model GetLocalVehicle(Vector3 position)
        {
            var zone = GetZone(position);

            return IsCountyZone(zone)
                ? new Model(CityVehicleModels[Random.Next(CityVehicleModels.Count)])
                : new Model(CountyVehicleModels[Random.Next(CountyVehicleModels.Count)]);
        }

        private static bool IsCountyZone(WorldZone zone)
        {
            return zone.County == EWorldZoneCounty.BlaineCounty || zone.County == EWorldZoneCounty.LosSantosCounty;
        }

        private static WorldZone GetZone(Vector3 position)
        {
            return Functions.GetZoneAtPosition(position);
        }
    }
}