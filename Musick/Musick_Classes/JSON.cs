using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    class JSON
    {
        public static JsonSerializer serializer = new JsonSerializer(); // Everyone use the same damn serializer.

        // Serializes settings to file.
        public static void SerializeSettings(UserSettings settingsToSerialize)
        {
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(ConfigClass.settingsFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, settingsToSerialize);
            }
        }

        // Deserializes settings from file into a UserSettings object.
        public static UserSettings DeserializeSettings()
        {
            UserSettings tempSettings = new UserSettings();
            using (StreamReader sr = System.IO.File.OpenText(ConfigClass.settingsFile))
            using (JsonTextReader jsonTR = new JsonTextReader(sr))
            {
                tempSettings = serializer.Deserialize<UserSettings>(jsonTR);
            }
            return tempSettings;
        }

        // Serializes a library from an ObservableCollection of songs.
        public static void SerializeLibrary(string fileToSerialize, ObservableCollection<Song> libraryToSerialize)
        {
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(fileToSerialize))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, libraryToSerialize);
            }
        }

        // Deserializes a library into an ObservableCollection of songs.
        public static ObservableCollection<Song> DeserializeLibrary(string file)
        {
            ObservableCollection<Song> tempLibrary = new ObservableCollection<Song>();
            using (StreamReader sr = System.IO.File.OpenText(file))
            using (JsonTextReader jsonTR = new JsonTextReader(sr))
            {
                tempLibrary = serializer.Deserialize<ObservableCollection<Song>>(jsonTR);
            }

            return tempLibrary;
        }
    }
}
