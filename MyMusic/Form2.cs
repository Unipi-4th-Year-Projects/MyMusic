using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using NAudio.Wave;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyMusic
{
    public partial class Form2 : Form
    {
        public Form2(playlist allP)
        {
            InitializeComponent();
            AllP = Form1.All;
        }

        public String connectionString = "Data source=songs.db;Version=3";
        SQLiteConnection connection;
        playlist AllP;

        private void Form2_Load(object sender, EventArgs e)
        {
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            String createTableSQL = "CREATE TABLE IF NOT EXISTS Song(" +
                "Path Text,Name Text,Artist Text,Mins integer,Secs Integer,Album Text,Release Integer,Language Text,Genre Text)";
            SQLiteCommand command = new SQLiteCommand(createTableSQL, connection);
            command.ExecuteNonQuery();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            openFileDialog1.Title = "Select a File";
            openFileDialog1.Filter = "Audio Files|*.mp3;*.wav;*.ogg;*.flac;*.m4a;*.mp4;*.wma;*.aac;";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedSong = openFileDialog1.FileName;
                var songFile = new AudioFileReader(selectedSong);
                int mins = songFile.TotalTime.Minutes;
                int secs = songFile.TotalTime.Seconds;
                Song song1 = new Song(selectedSong, textBox1.Text, textBox2.Text, mins, secs, textBox3.Text, int.Parse(textBox4.Text), textBox5.Text, textBox6.Text);
                AllP.AddingSong(song1);
                AllP.getLength();
                String insertSQL = "Insert into Song(" +
                    "Path,Name,Artist,Mins,Secs,Album,Release,Language,Genre)" +
                    "values(@path,@name,@artist,@mins,@secs,@album,@release,@language,@genre)";
                SQLiteCommand command = new SQLiteCommand(insertSQL, connection);
                command.Parameters.AddWithValue("path", selectedSong);
                command.Parameters.AddWithValue("name", textBox1.Text);
                command.Parameters.AddWithValue("artist", textBox2.Text);
                command.Parameters.AddWithValue("mins", mins);
                command.Parameters.AddWithValue("secs", secs);
                command.Parameters.AddWithValue("album", textBox3.Text);
                command.Parameters.AddWithValue("release", int.Parse(textBox4.Text));
                command.Parameters.AddWithValue("language", textBox5.Text);
                command.Parameters.AddWithValue("genre", textBox6.Text);
                command.ExecuteNonQuery();
                connection.Close();
                MessageBox.Show("Added!");
                this.Close();
                Application.Restart();
            }

        }

        void Form2Closed(object sender, FormClosedEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}