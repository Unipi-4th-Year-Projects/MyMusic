using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace MyMusic
{
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Song> GetAllSongsFromDatabase()
        {
            List<Song> songList = new List<Song>();

            string query = "SELECT * FROM Song";
            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var song = new Song(
                            reader["Path"].ToString(),
                            reader["Name"].ToString(),
                            reader["Artist"].ToString(),
                            Convert.ToInt32(reader["Mins"]),
                            Convert.ToInt32(reader["Secs"]),
                            reader["Album"].ToString(),
                            Convert.ToInt32(reader["Release"]),
                            reader["Language"].ToString(),
                            reader["Genre"].ToString()
                        );
                        songList.Add(song);
                    }
                }
            }
            return songList;
        }
    }
}
