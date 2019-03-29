using System;
using System.Linq;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Utils
{
    public static class ModIntegrationUtil
    {
        public static bool IsModLoaded(string assemblyName)
        {
            Assert.NotNull(assemblyName, "assemblyName cannot be null");
            try
            {
                return Functions.GetAllUserPlugins().Any(x => x.GetName().Name.Equals(assemblyName));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}