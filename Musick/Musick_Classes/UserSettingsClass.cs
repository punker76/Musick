using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    public class UserSettings
    {
        // Application theme
        public string theme { get; set; }
        public string accent { get; set; }
        
        // Player window Position
        public double playerLeft { get; set; }
        public double playerTop { get; set; }

        // Volume
        public double volumeValue { get; set; }

        // Shuffle
        public bool shuffleEnabled { get; set; }

        // Library window Size/Position   
        public double libraryWidth { get; set; }
        public double libraryHeight { get; set; }
        public double libraryLeft { get; set; }
        public double libraryTop { get; set; }
    }
}
