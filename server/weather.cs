using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rpg.Database;

namespace rpg
{
    public class weatherData :  Script
    {
        public static string toWeather(string main)
        {
            main = main.ToLower();
            string weather = "none";
            switch (main)
            {
                case "thunderstorm":
                    weather = "THUNDER"; break;
                case "drizzle":
                    weather = "CLEARING"; break;
                case "rain":
                    weather = "RAIN"; break;
                case "snow":
                    weather = "XMAS"; break;
                case "atmosphere":
                    weather = "FOGGY"; break;
                case "clouds":
                    weather = "OVERCAST"; break;
                case "clear":
                    weather = "CLEAR"; break;
                default:
                    weather = "CLEAR"; break;
            }
            return weather;
        }
        [Command("xmas")]
        public void setxmas(Player player)
        {
            NAPI.World.SetWeather("XMAS");
        }
        [Command("getweather")]
        public void getweaher(Player player)
        {
            player.SendChatMessage("Weather is " + NAPI.World.GetWeather());
        } 
        [ServerEvent(Event.ResourceStart)]
        public void serverWeatherStart()
        {
            DateTime lastWeatherChange = DateTime.Now;
            WebClient web = new WebClient();
            string response = "";
            string apiUrl = "http://api.openweathermap.org/data/2.5/weather?q=Lupeni,ro&APPID=a70b0e077ea94e010b6da2a6803a6574";
            new Thread(async () =>
            {

                Thread.CurrentThread.IsBackground = true;

              
                while (true)
                {


                    try
                    {
                        await databaseManager.selectQuery($"SELECT * FROM data WHERE id = 2", async (DbDataReader reader) =>
                        {


                            if (Math.Abs((DateTime.Now - DateTime.Parse((string)reader["lastWeatherUpdate"])).TotalMinutes) > 40)
                            {
                                response = web.DownloadString(apiUrl);
                                object dec = JsonConvert.DeserializeObject(response);
                                JObject obj = JObject.Parse(dec.ToString());
                                string name = obj["name"].ToString();
                                string weather = (string)obj["weather"][0]["main"];
                                weather = weather.ToLower();
                                await databaseManager.updateQuery($"UPDATE data SET lastWeatherUpdate = '{DateTime.Now}', weather = '{weather}' WHERE id = 2").Execute();
                                Console.WriteLine($"[Weather] at {DateTime.Now.ToString()}: Weather response in {name} is {weather}");
                                NAPI.Task.Run(() =>
                                {
                                   NAPI.World.SetWeather(toWeather(weather));
                                });
                            }
                            else
                            {
                                var weatherGame = toWeather((string)reader["weather"]);
                                Console.WriteLine((string)reader["weather"]);
                                NAPI.Task.Run(() =>
                                {
                                   NAPI.World.SetWeather(weatherGame);
                                });

                                Console.WriteLine($"[Weather] at {DateTime.Now.ToString()}: Old weather was set ({ weatherGame }), current value in database is {(string)reader["weather"]} and {Math.Abs((DateTime.Now - DateTime.Parse((string)reader["lastWeatherUpdate"])).TotalMinutes)} passed from the last weather update.");
                            }




                        }).Execute();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
    
                     Thread.Sleep(1800000);
                }

            }).Start();
        }
    }
}

/* await databaseManager.selectQuery($"SELECT * FROM data",  (DbDataReader reader) =>
                   {
                     //  if (!reader.HasRows)
                       {
                           response = web.DownloadString(apiUrl);
                           object dec = JsonConvert.DeserializeObject(response);
                           JObject obj = JObject.Parse(dec.ToString());
                           string name = obj["name"].ToString();
                           string weather = (string)obj["weather"][0]["main"];

                           //await databaseManager.updateQuery($"UPDATE data SET lastWeatherUpdate = '{DateTime.Now}',weather = '{toWeather(weather)}' WHERE id = 1").Result.Execute();
                           await databaseManager.updateQuery($"INSERT INTO data (lastWeatherUpdate, weather) VALUES('{DateTime.Now}', '{toWeather(weather)}')").Result.Execute();


                          // INSERT INTO room(person, address) VALUES(?person,? address)
                       }
                     /*  else 
                       {

                           if ((DateTime.Now - (DateTime)reader["lastWeatherUpdate"]).TotalMinutes > 3)
                           {
                               response = web.DownloadString(apiUrl);
                               object dec = JsonConvert.DeserializeObject(response);
                               JObject obj = JObject.Parse(dec.ToString());
                               string name = obj["name"].ToString();
                               string weather = (string)obj["weather"][0]["main"];

                               await databaseManager.updateQuery($"UPDATE data SET lastWeatherUpdate = '{DateTime.Now}',weather = '{toWeather(weather)}' WHERE id = 1").Result.Execute();
                               Console.WriteLine($"[{DateTime.Now.ToString()}] Weather response in {name} is {weather}");
                               NAPI.World.SetWeather(toWeather(weather));
                           }
                       }*/
