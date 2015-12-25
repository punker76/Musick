using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    public class ConfigClass
    {
        public static string appDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Musick"); // Local Appdata location.
        public static string appLibraryFolder = System.IO.Path.Combine( appDataFolder, "Library"); // Local library location.
        public static string appSettingsFolder = System.IO.Path.Combine(appDataFolder, "Settings"); // Local settings location.
    }
}
