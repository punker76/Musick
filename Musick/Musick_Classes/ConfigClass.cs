using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    public class ConfigClass
    {
        public static string appDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Musick"); // Local Appdata location.

    }
}
