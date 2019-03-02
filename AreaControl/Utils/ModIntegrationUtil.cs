using System.Linq;
using System.Reflection;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Utils
{
    public static class ModIntegrationUtil
    {
        public static bool IsModLoaded(Assembly mod)
        {
            Assert.NotNull(mod, "mod cannot be null");
            return Functions.GetAllUserPlugins()
                .Any(x => x == mod);
        }
    }
}