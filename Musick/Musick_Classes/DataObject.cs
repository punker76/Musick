using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Musick.Musick_Classes
{
    public class Song
    {
        public string FileLocation { get; set; }
        public string SongTitle { get; set; }
        public string SongArtist { get; set; }
        public string SongAlbum { get; set; }
        public string SongGenre { get; set; }
        public string SongYear { get; set; }

        public Song() { }

        public Song(string filelocation, string songtitle, string songartist, string songalbum, string songgenre, string songyear)
        {

            FileLocation = filelocation;
            SongTitle = songtitle;
            SongArtist = songartist;
            SongAlbum = songalbum;
            SongGenre = songgenre;
            SongYear = songyear;
        }
    }

    public class LibraryFile
    {
        public string libraryFile { get; set; }
        public string libraryName { get; set; }

        public LibraryFile() { }

        public LibraryFile(string libraryfile, string libraryname)
        {
            libraryFile = libraryfile;
            libraryName = libraryname;
        }
    }
}
