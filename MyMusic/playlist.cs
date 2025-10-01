using MyMusic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.LinkLabel;

namespace MyMusic
{
    public class playlist
    {
        public String name { get; set; }
        public String filePath;
        public List<Song> songsAdded = new List<Song>();
        public int Length;
        public playlist(String Name)
        {
            name = Name;
            string dataPath = Application.StartupPath;
            filePath = dataPath + "\\" + Name + ".txt";

        }
        public override string ToString()
        {
            return $"{name}, Songs: {getLength()}";
        }
        public void AddingSong(Song s)
        {
            if (!songsAdded.Contains(s))
            {
                songsAdded.Add(s);
                getLength();
                WriteListToFile(filePath);
            }
            else
            {

                MessageBox.Show($"Song {s.name} is already in the playlist.");
            }

        }
        public int getLength()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} was not found.");
            }

            int lineCount = 0;
            using (StreamReader file = new StreamReader(filePath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lineCount++;
                    }
                }
            }

            if (lineCount == 0) { return 0; }

            return lineCount;
        }

        public void RemovingSong(Song s)
        {
            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);
                using (StringWriter sw = new StringWriter())
                {
                    foreach (string line in lines)
                    {
                        if (!line.Contains(s.name))
                        {
                            // If the line does not contain the string, write it to the StringWriter
                            sw.WriteLine(line);
                        }
                    }

                    // Write the new content back to the file
                    File.WriteAllText(filePath, sw.ToString());
                }

                Console.WriteLine("Line removed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void WriteListToFile(string filePath)
        {
            //i am making a list with existing songs to see if there is already a song in a playlist
            try
            {
                List<string> existingSongs = new List<string>();
                if (File.Exists(filePath))
                {
                    existingSongs = File.ReadAllLines(filePath).ToList();
                }

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    foreach (Song s in songsAdded.ToList())
                    {
                        if (!existingSongs.Contains(s.name))
                        {
                            writer.WriteLine(s.name);
                            songsAdded.Remove(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while writing the file: {ex.Message}");
            }
        }
    }
}