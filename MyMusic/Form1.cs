using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WMPLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyMusic
{
    public partial class Form1 : Form
    {
        public WindowsMediaPlayer player = new WindowsMediaPlayer();
        //first database with songs
        public String connectionString = "Data source=songs.db;Version=3";
        SQLiteConnection connection;


        //second database with playlists
        String connectionString2 = "Data source=playlistsAvailable.db;Version=3";
        SQLiteConnection connection2;

        public static playlist All = new playlist("All Songs");         //all the songs
        public static List<playlist> playlists = new List<playlist>();  //all the playlists

        private bool buttonHidePressed = true;
        public Song psong = null;   //the song currently playing
        public Label llabel = null; //the last song label clicked


        public Form1()
        {
            InitializeComponent();
        }
        public void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            connection2 = new SQLiteConnection(connectionString2);
            connection2.Open();
            String createTableSQL2 = "CREATE TABLE IF NOT EXISTS PlaylistNames(Name Text)";
            SQLiteCommand createplaylistCommand = new SQLiteCommand(createTableSQL2, connection2);
            createplaylistCommand.ExecuteNonQuery();
            loadPlaylistNamesforlabels();
            string dataPath = Path.Combine(Application.StartupPath, "playlistsAvailable.db");
            GetPlaylistsFromDatabase(dataPath);
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (connection2.State != ConnectionState.Open)
                    connection2.Open();
                String searchforexistingPlaylistName = "SELECT * FROM PlaylistNames WHERE Name = @name";
                SQLiteCommand searchingPlaylist = new SQLiteCommand(searchforexistingPlaylistName, connection2);
                searchingPlaylist.Parameters.AddWithValue("name", textBox1.Text);
                object result = searchingPlaylist.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    MessageBox.Show("Playlist already exists.", "Existence", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    String playlistName = textBox1.Text;
                    playlist p1 = new playlist(playlistName);


                    String insertplaylistnameSQL2 = "Insert into PlaylistNames(Name)  values(@name)";
                    SQLiteCommand insertPlaylistName = new SQLiteCommand(insertplaylistnameSQL2, connection2);
                    insertPlaylistName.Parameters.AddWithValue("name", playlistName);
                    insertPlaylistName.ExecuteNonQuery();
                    connection2.Close();
                    p1.WriteListToFile(p1.filePath);
                    createlabelPlaylist(p1);
                    Application.Restart();
                }
            }
            else
            {
                MessageBox.Show("Give the playlist a Name!");
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            Form2 addForm = new Form2(All);
            addForm.ShowDialog();
        }

        private void createlabelPlaylist(playlist p1)
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = p1.ToString(),
                Font = new Font(FontFamily.GenericSansSerif, 14),
                Size = new Size(200, 30),
                ForeColor = Color.Black
            };
            flowLayoutPanel1.Controls.Add(label);
            label.DoubleClick += (s, ev) =>
            {
                flowLayoutPanel2.Controls.Clear();
                createlabelSongsinPlaylist(p1);
                label3.Text = "Songs in " + p1.name + ":";
            };
        }

        public void loadPlaylistNamesforlabels()
        {
            flowLayoutPanel1.Controls.Clear();
            using (SQLiteConnection connection2 = new SQLiteConnection(connectionString2))
            {
                connection2.Open();

                string query = "SELECT Name FROM PlaylistNames";
                using (SQLiteCommand command = new SQLiteCommand(query, connection2))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string playlistName = reader["Name"].ToString();
                            createlabelPlaylist(new playlist(playlistName));
                        }
                    }
                }
            }
        }

        //function to create the songs labels and their code.
        private void createlabelSongsinPlaylist(playlist p2)
        {
            //clear the panel
            flowLayoutPanel2.Controls.Clear();

            //get the length to know how many labels we need.
            int songslabel = p2.getLength();
            if (songslabel != 0)
            {
                //make a list with the songs.
                List<string> songNamesInPlaylist = ReadLinesToList(p2.filePath);

                //call the db helper to transofm the database to a list of objects.
                var dbHelper = new DatabaseHelper(connectionString);
                List<Song> songsFromDb = dbHelper.GetAllSongsFromDatabase();

                //in the songsFromDb there are all the songs. here we pick only those in the playlist we have clicked
                var songsInPlaylist = songsFromDb.Where(song => songNamesInPlaylist.Contains(song.name)).ToList();

                foreach (Song s in songsInPlaylist)
                {
                    //make label for those in the playlist
                    System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
                    {
                        Text = s.name,
                        Font = new Font(FontFamily.GenericSerif, 10),
                        Size = new Size(150, 30),
                        ForeColor = Color.Black
                    };
                    flowLayoutPanel2.Controls.Add(label2);

                    //if double click, start playing
                    label2.DoubleClick += (s1, ev) =>
                    {
                        curr.Text = $"{s.name} by {s.artist}";
                        curr.Refresh();
                        if (psong != null)
                        {

                            psong.playingNow = false;
                            psong.pauseSong(psong.savePath);
                        }
                        s.playingNow = true;
                        s.playSong(s.savePath);
                        s.changeVol(trackBar1.Value);// to keep the volume in the same level as the psong
                        psong = s;
                    };

                    //if right click show contextmenustrip with all the playlists you can put the song into
                    label2.MouseClick += (s1, ev) =>
                    {
                        if (ev.Button == MouseButtons.Right)
                        {
                            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

                            foreach (playlist p in playlists)
                            {
                                if (p.name != "All Songs")
                                {
                                    ToolStripMenuItem menuItem = new ToolStripMenuItem(p.name);
                                    menuItem.Click += (sender, e) =>
                                    {
                                        p.AddingSong(s);
                                        Application.Restart();
                                    };
                                    contextMenuStrip.Items.Add(menuItem);
                                }
                            }
                            ToolStripMenuItem deleteFromMenuItem = new ToolStripMenuItem("Delete from...");
                            deleteFromMenuItem.DropDownOpening += (sender, e) =>
                            {
                                deleteFromMenuItem.DropDownItems.Clear();
                                foreach (playlist p in playlists)
                                {
                                    String currSname = label2.Text;
                                    
                                    ToolStripMenuItem submenuItem = new ToolStripMenuItem(p.name);
                                    submenuItem.Click += (submenuSender, submenuEventArgs) =>
                                    {
                                        DialogResult result = MessageBox.Show($"Are you sure you want to delete {currSname} from {p.name}?","Confirmation", MessageBoxButtons.YesNo);
                                        if (result == DialogResult.Yes || p.name != "All Songs")
                                        {
                                            p.RemovingSong(s);
                                            Application.Restart();
                                        }
                                        else if (result == DialogResult.Yes || p.name == "All Songs")
                                        {
                                            connection.Open();
                                            string sql = "DELETE FROM Song WHERE Name = @Name";
                                            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                                            {
                                                command.Parameters.AddWithValue("@Name", s.name);
                                                command.ExecuteNonQuery();
                                                Application.Restart();
                                            }
                                        }
                                        else if (result == DialogResult.No)
                                        {
                                            
                                        }
                                    };
                                    deleteFromMenuItem.DropDownItems.Add(submenuItem);
                                }
                            };
                            contextMenuStrip.Items.Add(deleteFromMenuItem);
                            contextMenuStrip.Show(label2, new Point(ev.X, ev.Y));
                        }
                    };
                }
            }
        }

        //function to make a list from the lines of a txt.
        public List<string> ReadLinesToList(string filePath)
        {
            List<string> lines = new List<string>();
            //put each line of the txt in the list and return it.
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    //put only if it is not null or white space
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }
            return lines;
        }

        //function to transform a database (only one column) to an object list
        public static List<playlist> GetPlaylistsFromDatabase(string databaseFilePath)
        {
            //connnection
            string connectionString = $"Data Source={databaseFilePath};Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();


                //select the playlist and put the first column in the list (there's only 1 column)
                string query = "SELECT Name FROM PlaylistNames";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string playlistName = reader.GetString(0);
                            playlist pl = new playlist(playlistName);
                            playlists.Add(pl);
                        }
                    }
                }
                connection.Close();
            }
            return playlists;
        }

        //function to delete playlist
        private void deleteButton_Click(object sender, EventArgs e)
        {
            string playlistName = textBox1.Text;

            if (string.IsNullOrWhiteSpace(playlistName))
            {
                MessageBox.Show("Please enter the name of the playlist to delete.");
                return;
            }
            if (playlistName != "All Songs")
            {
                var confirmResult = MessageBox.Show($"Are you sure you want to delete the playlist '{playlistName}'?",
                                                "Confirm Delete!",
                                                MessageBoxButtons.YesNo);
                //if no
                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                //if yeah fs
                if (connection2.State != ConnectionState.Open)
                    connection2.Open();

                //open the database and delete
                string deletePlaylistSQL = "DELETE FROM PlaylistNames WHERE Name = @name";
                SQLiteCommand deleteCommand = new SQLiteCommand(deletePlaylistSQL, connection2);
                deleteCommand.Parameters.AddWithValue("@name", playlistName);
                int rowsAffected = deleteCommand.ExecuteNonQuery();

                //if everything's gucci, delete and the file.
                if (rowsAffected > 0)
                {
                    string playlistFilePath = Path.Combine(Application.StartupPath, $"{playlistName}.txt");
                    if (File.Exists(playlistFilePath))
                    {
                        try
                        {
                            File.Delete(playlistFilePath);
                        }
                        catch (IOException ex)
                        {
                            MessageBox.Show($"An error occurred while deleting the file: {ex.Message}");
                            return;
                        }
                    }
                    //refresh (it was the easy solution)
                    Application.Restart();
                }
                else
                {
                    MessageBox.Show($"The playlist '{playlistName}' does not exist or could not be deleted.");
                }
                connection2.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (buttonHidePressed == true)
            {
                panel2.Hide();
                flowLayoutPanel1.Location = new Point(-208, 39);
                flowLayoutPanel1.Size = new Size(1057, 540);
                panel3.Location = new Point(3, 58);
                panel3.Size = new Size(1078, 582);
                string picturePath = Path.Combine(Application.StartupPath, "icons\\next.png");
                button1.BackgroundImage = Image.FromFile(picturePath);
            }
            else
            {
                panel2.Show();
                flowLayoutPanel1.Location = new Point(12, 39);
                flowLayoutPanel1.Size = new Size(859, 531);
                panel2.Location = new Point(0, 58);
                panel2.Size = new Size(223, 582);
                panel3.Location = new Point(229, 58);
                panel3.Size = new Size(882, 582);
                string picturePath = Path.Combine(Application.StartupPath, "icons\\back.png");
                button1.BackgroundImage = Image.FromFile(picturePath);
            }
            buttonHidePressed = !buttonHidePressed;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (psong != null)
            {
                psong.pauseSong(psong.savePath);
                //timer1.Enabled = false;
                
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (psong != null)
            {
                psong.playSong(psong.savePath);
                //timer1.Enabled = true;
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("MyMusic is an app made by Unipi Students: \nAndreas Kandhlas, \nStelios Myaris, \nFotis Efthimiadis. \n\n" +
                "It was a project inspired by their professor Efthimios Alepis. \n\n" +
                "Being completely free of adds, it is now the leading music application in the world \n" +
                "Enjoy", "App Information");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (psong != null)
            {
                MessageBox.Show("Song's Information:\n" + psong.ToString(), "Song's Info");
            }
            else { MessageBox.Show("No song is playing", "Song's Info"); }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("MyMusic is an app made by Unipi Students: \nAndreas Kandhlas, \nStelios Myaris, \nFotis Efthimiadis. \n\n" +
                "It was a project inspired by their professor Efthimios Alepis. \n\n" +
                "Being completely free of adds, it is now the leading music application in the world \n" +
                "Enjoy", "App Information");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (psong != null)
            {
                psong.changeVol(trackBar1.Value);
            }
            label5.Text = "Volume: " + trackBar1.Value + "%";
        }
    }
}