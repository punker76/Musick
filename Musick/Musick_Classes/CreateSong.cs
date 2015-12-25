using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    class CreateSong
    {
        public static Song Create(string file)
        {
            var tagFile = TagLib.File.Create(file);
            string tempTitle;
            string tempArtist;
            string tempAlbum;
            string tempGenre;
            string tempYear;

            tempTitle = (tagFile.Tag.Title != null) ? tagFile.Tag.Title : Path.GetFileName(file);
            tempArtist = (tagFile.Tag.FirstPerformer != null) ? tagFile.Tag.FirstPerformer : "[No Artist]";
            tempAlbum = (tagFile.Tag.Album != null) ? tagFile.Tag.Album : "[No Album]";
            tempGenre = (tagFile.Tag.FirstGenre != null) ? tagFile.Tag.FirstGenre : "[No Genre]";
            tempYear = (tagFile.Tag.Year.ToString() != "0") ? tagFile.Tag.Year.ToString() : "[No Year]";

            Song tempSong = new Song(file, tempTitle, tempArtist, tempAlbum, tempGenre, tempYear);
            return tempSong;
        }
    }
}
