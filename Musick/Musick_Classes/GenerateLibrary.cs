﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musick.Musick_Classes
{
    class GenerateLibrary
    {
        public static ObservableCollection<Song> Create(string directory)
        {
            ObservableCollection<Song> tempLibrary = new ObservableCollection<Song>();
            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (file.Contains(".mp3") || file.Contains(".wma") || file.Contains(".wav"))
                {
                    tempLibrary.Add(GenerateSong(file));
                }
            }
            return tempLibrary;
        }
        
        public static Song GenerateSong(string file)
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

        public static LibraryFile CreateLibraryEntry(ObservableCollection<Song> libSourceToUse, string tempMusicLibraryFile)
        {
            
            var tempPathList = libSourceToUse.Select(x => x.FileLocation).ToList();
            var MatchingChars =
                from len in Enumerable.Range(0, tempPathList.Min(s => s.Length)).Reverse()
                let possibleMatch = tempPathList.First().Substring(0, len)
                where tempPathList.All(f => f.StartsWith(possibleMatch))
                select possibleMatch;
            string tempSource = Path.GetDirectoryName(MatchingChars.First());
            string tempLibName = System.IO.Path.GetFileNameWithoutExtension(tempMusicLibraryFile);
            string tempFileLoc = tempMusicLibraryFile;
            LibraryFile tempLibFile = new LibraryFile(tempFileLoc, tempLibName, tempSource);
            return tempLibFile;
        }
    }
}
