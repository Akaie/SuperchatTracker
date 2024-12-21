using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YoutubeSuperChatApp
{
    public static class DataAccess
    {
        public static async Task InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder
                    .CreateFileAsync("superchat.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                string cmd = "CREATE TABLE IF NOT EXISTS SuperChat (" +
                    "Name NVARCHAR(256) NOT NULL, " +
                    "CurrencyType NVARCHAR(32) NOT NULL," +
                    "Amount BIGINT NOT NULL," +
                    "Date DATETIME NOT NULL," +
                    "ResolvedDT DATETIME NULL," +
                    "ResolvedFG BIT NOT NULL DEFAULT 0," +
                    "PRIMARY KEY(Name, Amount, Date)" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS Options (" +
                    "Name NVARCHAR(256) NOT NULL PRIMARY KEY UNIQUE," +
                    "Value NVARCHAR(256) NOT NULL" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS Rates (" +
                    "Currency NVARCHAR(10) NOT NULL PRIMARY KEY UNIQUE," +
                    "Rate DOUBLE NUL NULL" +
                    ");";

                SqliteCommand create = new(cmd, db);

                create.ExecuteReader();
                db.Close();
            }
        }

        public static Dictionary<string, double>? InitializeRates(bool updatedCurrency = false)
        {
            Dictionary<string, double> rates = [];
            string homeCurrency = GetOption("HomeCurrency");
            DateTime CurrencyDate;
            bool parseDateResult = DateTime.TryParse(GetOption("CurrencyDate"), out CurrencyDate);
            CurrencyDate = !parseDateResult ? DateTime.MinValue : CurrencyDate;
            if (homeCurrency == null || homeCurrency == "")
            {
                homeCurrency = "USD";
                SetOption("USD", "HomeCurrency");
            }
            SetOption(val: CurrencyDate.ToString(), name: "CurrencyDate");
            if (CurrencyDate < DateTime.Now.AddDays(-1) || updatedCurrency)
            {
                string path = "https://api.fxratesapi.com/latest?base=" + homeCurrency;
                HttpClient client = new HttpClient();
                ExchangeRoot? resultObj = null;
                try
                {
                    var result = client.GetAsync(path).Result.Content.ReadAsStream();
                    resultObj = JsonSerializer.Deserialize<ExchangeRoot>(result);
                }
                catch (HttpRequestException)
                {
                    MessageBox.Show("An error occured attempting to get currency rate exchange values. This may be rate limiting on the API used to keep these values up to date.");
                    return null;
                }
                if (resultObj != null)
                {
                    foreach (KeyValuePair<string, double> item in resultObj.rates)
                    {
                        UpdateRate(item.Key, item.Value);
                    }
                    rates = resultObj.rates;
                }
                SetOption(DateTime.Now.ToString(), "CurrencyDate");
            }
            else
            {
                rates = GetRates();
            }
            return rates;
        }

        public static void ClearChats()
        {
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand delete = new()
                {
                    Connection = db,

                    CommandText = "DELETE FROM SuperChat WHERE Date < datetime('now', '-2 day')"
                };
                delete.ExecuteNonQuery();
            }
        }

        public static void UpdateRate(string currency, double rate)
        {
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand insert = new()
                {
                    Connection = db,

                    CommandText = "INSERT INTO Rates(Currency, Rate) VALUES (@currency, @rate) " +
                    "ON CONFLICT(Currency) DO UPDATE SET Rate = excluded.Rate"
                };
                insert.Parameters.AddWithValue("@currency", currency);
                insert.Parameters.AddWithValue("@rate", rate);
                insert.ExecuteReader();
            }
        }

        public static Dictionary<string, double> GetRates()
        {
            Dictionary<string, double> rates = new Dictionary<string, double>();
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand select = new()
                {
                    Connection = db,

                    CommandText = "SELECT Currency, Rate FROM Rates"
                };

                SqliteDataReader result = select.ExecuteReader();

                while (result.Read())
                {
                    rates.Add(result.GetString(0), result.GetDouble(1));
                }
            }
            return rates;
        }

        public static double GetRate(string currency)
        {
            double rate = 0;
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand select = new()
                {
                    Connection = db,

                    CommandText = "SELECT rate FROM Rates WHERE Currency = @Currency"
                };

                select.Parameters.AddWithValue("@Currency", currency);

                SqliteDataReader result = select.ExecuteReader();
                result.Read();
                if (result.HasRows)
                {
                    rate = result.GetDouble(0);
                }
                db.Close();
                return rate;
            }
        }

        public static string GetOption(string input)
        {
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand select = new()
                {
                    Connection = db,

                    CommandText = "SELECT Value FROM Options WHERE Name = @Name"
                };

                select.Parameters.AddWithValue("@Name", input);

                SqliteDataReader result = select.ExecuteReader();
                result.Read();
                string val = "";
                if (result.HasRows)
                {
                    val = result.GetString(0);
                }
                db.Close();
                return val;
            }
        }

        public static void SetOption(string val, string name)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand insert = new()
                {
                    Connection = db,

                    CommandText = "INSERT INTO Options(Name, Value) VALUES(@Name, @Value)" +
                    "ON CONFLICT(Name) DO UPDATE SET Value = excluded.Value;"
                };

                insert.Parameters.AddWithValue("@Name", name);
                insert.Parameters.AddWithValue("@Value", val);

                insert.ExecuteNonQuery();
            }
        }

        public static List<Record> GetRecords(bool resolved = false)
        {
            var list = new List<Record>();
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();
                SqliteCommand select = new()
                {
                    Connection = db,
                    CommandText = "SELECT Name, CurrencyType, Amount, Date FROM SuperChat WHERE ResolvedFG = @Resolved"
                };
                if (resolved)
                {
                    select.Parameters.AddWithValue("@Resolved", 1);
                }
                else
                {
                    select.Parameters.AddWithValue("@Resolved", 0);
                }
                SqliteDataReader result = select.ExecuteReader();
                while (result.Read())
                {
                    Record r = new()
                    {
                        Name = result.GetString(0),
                        CurrencyType = result.GetString(1),
                        Amount = result.GetInt64(2),
                        Date = result.GetDateTime(3),
                    };
                    list.Add(r);
                }
            }
            return list;
        }

        public static void InsertRecord(string name, string ctype, ulong? amt, DateTimeOffset? date)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand insert = new()
                {
                    Connection = db,

                    CommandText = "INSERT INTO SuperChat(Name, CurrencyType, Amount, Date, ResolvedDT, ResolvedFG) VALUES(@Name, @cType, @Amt, @Date, NULL, 0)" +
                                    "ON CONFLICT(Name, Amount, Date) DO NOTHING"
                };

                insert.Parameters.AddWithValue("@Name", name);
                insert.Parameters.AddWithValue("@cType", ctype);
                insert.Parameters.AddWithValue("@Amt", amt);
                insert.Parameters.AddWithValue("@Date", date);

                insert.ExecuteNonQuery();
            }
        }

        public static void SetRecordResolved(string name, ulong? amt, DateTimeOffset? date)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand insert = new()
                {
                    Connection = db,

                    CommandText = "UPDATE SuperChat SET ResolvedFG = 1 " +
                                    "WHERE Name = @Name " +
                                    "AND Date = @Date " +
                                    "AND Amount = @Amt"
                };
                insert.Parameters.AddWithValue("@Name", name);
                insert.Parameters.AddWithValue("@Amt", amt);
                insert.Parameters.AddWithValue("@Date", date);

                insert.ExecuteNonQuery();
            }
        }

        public static void SetRecordUnresolved(string name, ulong? amt, DateTimeOffset? date)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                         "superchat.db");
            using var db = new SqliteConnection($"Filename={dbpath}");
            {
                db.Open();

                SqliteCommand insert = new()
                {
                    Connection = db,

                    CommandText = "UPDATE SuperChat SET ResolvedFG = 0 " +
                                    "WHERE Name = @Name " +
                                    "AND Date = @Date " +
                                    "AND Amount = @Amt"
                };
                insert.Parameters.AddWithValue("@Name", name);
                insert.Parameters.AddWithValue("@Amt", amt);
                insert.Parameters.AddWithValue("@Date", date);

                insert.ExecuteNonQuery();
            }
        }

        public class Record
        {
            public string Name { get; set; } = "";
            public string CurrencyType { get; set; } = "";
            public long Amount { get; set; } = 0;
            public DateTimeOffset? Date { get; set; } = DateTimeOffset.MinValue;
        }

        public class ExchangeRoot
        {
            public bool success { get; set; } = true;
            public string terms { get; set; } = "";
            public string privacy { get; set; } = "";
            public long timestamp { get; set; } = 0;
            public string date { get; set; } = "1/1/1900";
            public Dictionary<string, Double> rates { get; set; } = [];
        }
    }
}
