using MovieRecommenderML.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MovieRecommenderML.ConsoleApp
{
    class Mysql { 

        private string path = AppDomain.CurrentDomain.BaseDirectory;
        private MySqlConnection connect = null;
        private Boolean connected = false;
        private readonly String _dbHost = "localhost";
        private readonly String _dbName = "movie";
        private readonly String _dbUser = "root";
        private readonly String _dbPass = "test";

        public Mysql()
        {
            Connecting();
        }

        public void Connecting()
        {
            try
            {
                String connectionString = $"SERVER={_dbHost};DATABASE={_dbName};UID={_dbUser};PASSWORD={_dbPass};";

                connect = new MySqlConnection(connectionString);

                if (connect.State != ConnectionState.Open)
                {
                    connect.Open();
                    connected = true;
                }
                else
                {
                    throw new Exception("Mysql Connect Error!");
                }

            }
            catch (Exception ex)
            {
            
            }
        }

        private Boolean CheckConnect()
        {
            var temp = connect.State.ToString();
            if (temp == "Open")
            {
                connected = true;
                return true;
            }
            else
            {
                connected = false;
                return false;
            }
        }

        public Hashtable Query(String query)
        {

            Hashtable result = new Hashtable();

            try
            {

                CheckConnect();

                if (!connected)
                {
                    Connecting();
                }

                if (connected)
                {
                    using (var command = new MySqlCommand(query, connect))
                    {
                        using (var dataReader = command.ExecuteReader())
                        {
                            int i = 0;
                            Int64 LastInsertId = command.LastInsertedId;
                            if (LastInsertId > 0)
                            {
                                result["LastInsertedId"] = LastInsertId;
                            }
                            else
                            {
                                while (dataReader.Read())
                                {
                                    Hashtable item = new Hashtable();
                                    for (int j = 0; j < dataReader.FieldCount; j++)
                                    {
                                        string columnName = dataReader.GetName(j);
                                        item[columnName] = dataReader[columnName];
                                    }

                                    result[i] = item;
                                    i++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return result;

        }

        public List<ModelInput> GetMovieList()
        {
            List<ModelInput> list = new List<ModelInput>();
            try
            {
                String sql = String.Format("SELECT * FROM enc34_movie_list WHERE list_id={0}", (int)1);
                Hashtable items = Query(sql);
                for (int i = 0; i < items.Count; i++)
                {
                    Random rnd = new Random();
                    Hashtable item = (Hashtable)items[i];
                    ModelInput modelInput = new ModelInput()
                    {
                        UserId = Convert.ToSingle(item["user_id"]),
                        MovieId = Convert.ToSingle(item["movie_id"]),
                        Rating = (float)rnd.Next(5,10),
                        Timestamp = 0
                    };
                    list.Add(modelInput);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return list;
        }
        public ModelMovie GetMovie(int movieId)
        {
            try
            {
                String sql = String.Format("SELECT * FROM enc34_movies WHERE id={0}", (int)movieId);
                Hashtable items = Query(sql);
                for (int i = 0; i < items.Count; i++)
                {
                    Hashtable item = (Hashtable)items[i];
                    ModelMovie modelMovie = new ModelMovie()
                    {
                        MovieName = item["title"].ToString(),
                        MovieId = Convert.ToSingle(item["id"])
                    };
                    return modelMovie;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }
        public List<ModelInput> GetAllMovie(int userId)
        {
            List<ModelInput> list = new List<ModelInput>();
            try
            {
                String sql = String.Format("SELECT * FROM enc34_movie_list WHERE list_id={0} and user_id!={1}", (int)1, (int)userId);
                Hashtable items = Query(sql);
                for (int i = 0; i < items.Count; i++)
                {
                    Random rnd = new Random();
                    Hashtable item = (Hashtable)items[i];
                    ModelInput modelInput = new ModelInput()
                    {
                        UserId = Convert.ToSingle(item["user_id"]),
                        MovieId = Convert.ToSingle(item["movie_id"]),
                        Rating = (float)rnd.Next(5,10),
                        Timestamp = 0
                    };
                    list.Add(modelInput);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return list;
        }

    }
}
