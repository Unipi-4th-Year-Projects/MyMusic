using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace MyMusic
{
    public class Song
    {
        public string savePath { get; set; }

        public string name { get; set; }

        public string artist { get; set; }

        private int mins { get; set; }

        private int secs { get; set; }

        private string album { get; set; }

        private int dateofRelease { get; set; }

        private string language { get; set; }

        private string genre { get; set; }

        public bool playingNow = false;
        public Song(string Path, string Name, string Artist, int Mins, int Secs, string Album, int DOR, string Language, string Genre)
        {
            savePath = Path;
            name = Name;
            artist = Artist;
            mins = Mins;
            secs = Secs;
            album = Album;
            dateofRelease = DOR;
            language = Language;
            genre = Genre;
        }

        public override string ToString()
        {
            return $"Song: {name} by {artist}, Duration: {mins} mins {secs} secs, Album: {album}, Release Date: {dateofRelease}, Language: {language}, Genre: {genre}";
        }

        public string ToStringHalf()
        {
            return $"{name}";
        }

        WindowsMediaPlayer player = new WindowsMediaPlayer();

        public void playSong(string savePath)
        {
            if (player.URL != $@"{savePath}")
            {
                player.URL = $@"{savePath}";
            }
            if (playingNow == true)
            {
                player.controls.play();
                
            }
        }

        public void pauseSong(string savePath)
        {
            player.controls.pause();
        }

        public void AddToPlaylist(string Name)
        {

        }
        public void changeVol(int x)
        {
            player.settings.volume = x;
        }
    }
}