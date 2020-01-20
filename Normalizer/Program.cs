using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq;

namespace Normalizer
{
    public class PCTableNormalizer
    {
        private readonly SqliteConnection connection;

        private bool isDisposed;

        public PCTableNormalizer(string connectionString)
        {
            connection = new SqliteConnection(connectionString);
            connection.Open();
        }


        public static List<PCModel> GetPCList(string cs)
        {
            List<PCModel> PCList = new List<PCModel>();
            using (SqliteConnection con = new SqliteConnection(cs))
            {
                var taskListTemp = new List<PCModel>();
                con.Open();
                string stm = "SELECT * FROM PCTable";

                using (SqliteCommand cmd = new SqliteCommand(stm, con))
                {
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PCList.Add(new PCModel
                            {
                                Id = int.Parse(reader["Id"].ToString()),
                                Processor = reader["Processor"].ToString(),
                                Motherboard = reader["Motherboard"].ToString(),
                                GraphicsCard = reader["GraphicsCard"].ToString(),
                                Ram = reader["RAM"].ToString(),
                                Os = reader["OS"].ToString()
                            });
                        }
                        reader.Close();
                    }
                }
                con.Close();
                return PCList;
            }
        }

        public static void SetPCList(List<PCModel> PCList, string cs)
        {
            int graphCardIter = 0;
            List<GraphCardNormModel> graphCardNormList = new List<GraphCardNormModel>();
            int motherboardIter = 0;
            List<MotherboardNormModel> motherboardNormList = new List<MotherboardNormModel>();
            int osIter = 0;
            List<OSNormModel> osNormList = new List<OSNormModel>();
            int pcIter = 0;
            List<PCNormModel> pcNormList = new List<PCNormModel>();
            int processorIter = 0;
            List<ProcessorNormModel> processorNormList = new List<ProcessorNormModel>();
            int ramIter = 0;
            List<RAMNormModel> ramNormList = new List<RAMNormModel>();
            foreach (var item in PCList)
            {
                if(graphCardNormList.Find(x => x.Name == item.GraphicsCard) == null)
                {
                    graphCardNormList.Add(new GraphCardNormModel() { Id = graphCardIter, Name = item.GraphicsCard });
                    graphCardIter++;
                }
                if (motherboardNormList.Find(x => x.Name == item.Motherboard) == null)
                {
                    motherboardNormList.Add(new MotherboardNormModel() { Id = motherboardIter, Name = item.Motherboard });
                    motherboardIter++;
                }
                if (osNormList.Find(x => x.Name == item.Os) == null)
                {
                    osNormList.Add(new OSNormModel() { Id = osIter, Name = item.Os });
                    osIter++;
                }
                if (processorNormList.Find(x => x.Name == item.Processor) == null)
                {
                    processorNormList.Add(new ProcessorNormModel() { Id = processorIter, Name = item.Processor });
                    processorIter++;
                }
                if (ramNormList.Find(x => x.Name == item.Ram) == null)
                {
                    ramNormList.Add(new RAMNormModel() { Id = ramIter, Name = item.Ram });
                    ramIter++;
                }
                if (pcNormList.Find(x => x.Id == item.Id) == null)
                {
                    pcNormList.Add(new PCNormModel() { Id = item.Id, GraphCardId = graphCardNormList.Single(x => x.Name == item.GraphicsCard).Id,
                    MotherboardId = motherboardNormList.Single(x => x.Name == item.Motherboard).Id, OSId = osNormList.Single(x => x.Name == item.Os).Id,
                    ProcId = processorNormList.Single(x => x.Name == item.Processor).Id, RamId = ramNormList.Single(x => x.Name == item.Ram).Id});
                    //pcIter++;
                }

            }

            using (NpgsqlConnection con = new NpgsqlConnection(cs))
            {
                con.Open();

                string stm = "INSERT INTO public.\"graphicsCardTable\" values";
                for(int i=0; i-2<graphCardNormList.Count; i=i+2)
                {
                    stm += $" (@{i} , @{i + 1}),";
                }
                stm = stm.Remove(stm.Length - 1);

                int iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in graphCardNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter+1}", item.Name);
                        iter += 2;
                    }
                    comm.ExecuteNonQuery();
                }

                stm = "INSERT INTO public.\"motherboardTable\" values";
                for (int i = 0; i - 2 < motherboardNormList.Count; i = i + 2)
                {
                    stm += $" (@{i} , @{i + 1}),";
                }
                stm = stm.Remove(stm.Length - 1);
                iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in motherboardNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter + 1}", item.Name);
                        iter += 2;
                    }
                    comm.ExecuteNonQuery();
                }

                stm = "INSERT INTO public.\"osTable\" values";
                for (int i = 0; i - 2 < osNormList.Count; i = i + 2)
                {
                    stm += $" (@{i} , @{i + 1}),";
                }
                stm = stm.Remove(stm.Length - 1);
                iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in osNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter + 1}", item.Name);
                        iter += 2;
                    }
                    comm.ExecuteNonQuery();
                }

                stm = "INSERT INTO public.\"processorTable\" values";
                for (int i = 0; i - 2 < processorNormList.Count; i = i + 2)
                {
                    stm += $" (@{i} , @{i + 1}),";
                }
                stm = stm.Remove(stm.Length - 1);
                iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in processorNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter + 1}", item.Name);
                        iter += 2;
                    }
                    comm.ExecuteNonQuery();
                }

                stm = "INSERT INTO public.\"ramTable\" values";
                for (int i = 0; i - 2 < ramNormList.Count; i = i + 2)
                {
                    stm += $" (@{i} , @{i + 1}),";
                }
                stm = stm.Remove(stm.Length - 1);
                iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in ramNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter + 1}", item.Name);
                        iter += 2;
                    }
                    comm.ExecuteNonQuery();
                }


                stm = "INSERT INTO public.\"PCTable\" values";
                for (int i = 0; i - 6 < pcNormList.Count; i = i + 6)
                {
                    stm += $" (@{i} , @{i + 1} , @{i + 2} , @{i + 3} , @{i + 4} , @{i + 5}),";
                }
                stm = stm.Remove(stm.Length - 1);
                iter = 0;
                using (var comm = new NpgsqlCommand(stm, con))
                {
                    foreach (var item in pcNormList)
                    {

                        comm.Parameters.AddWithValue($"@{iter}", item.RamId);
                        comm.Parameters.AddWithValue($"@{iter + 1}", item.ProcId);
                        comm.Parameters.AddWithValue($"@{iter + 2}", item.MotherboardId);
                        comm.Parameters.AddWithValue($"@{iter + 3}", item.GraphCardId);
                        comm.Parameters.AddWithValue($"@{iter + 4}", item.Id);
                        comm.Parameters.AddWithValue($"@{iter + 5}", item.OSId);
                        iter += 6;
                    }
                    comm.ExecuteNonQuery();
                }
            }
        }

        static void Main(string[] args)
        {
            var pcList = GetPCList("Data Source="+"bands.db;");
            var postgresCS = "Server = 127.0.0.1; User Id = postgres; " + "Password=a54g5x; Database=PC_DB;";
            SetPCList(pcList, postgresCS);
            Console.WriteLine("Hello World!");
        }
    }
}
