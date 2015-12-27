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


            #region Library window settings
            tempDefaults.libraryHeight = 380;
            tempDefaults.libraryWidth = 1000;
            tempDefaults.libraryTop = 20;
            tempDefaults.libraryLeft = 380;
            #endregion

            #region Player window settings
            // Player window position
            tempDefaults.playerLeft = 20;
            tempDefaults.playerTop = 20;

            // Volume
            tempDefaults.volumeValue = 0.5;

            // Shuffle
            tempDefaults.shuffleEnabled = false;
            #endregion

            return tempDefaults;
        }
    }
}
