using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Musick.Musick_Classes;

namespace Musick.Musick_Classes
{
    public class DefaultSettings
    {
        public static UserSettings set()
        {
            UserSettings tempDefaults = new UserSettings();
            tempDefaults.theme = "BaseDark";
            tempDefaults.accent = "Red";
            return tempDefaults;
        }
    }
}
