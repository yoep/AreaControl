using System;
using System.Linq;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Utils
{
    public static class ModIntegrationUtil
    {
        public static bool IsModLoaded(string assemblyQualifiedName)
        {
            Assert.NotNull(assemblyQualifiedName, "assemblyQualifiedName cannot be null");
            try
            {
                return Functions.GetAllUserPlugins()
                    .Any(x => x.ToString().Contains(assemblyQualifiedName));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}