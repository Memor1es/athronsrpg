using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{
    public static class mainLicense
    {
       public enum license
        {
            vehicle,
            plane,
            boat,
            gun
        }
        public class model
        {

            public license type { get; set; }
            public int hours { get; set; }
            public model() { }
            public model(license type, int hours)
            {
                this.type = type;
                this.hours = hours; 
            }
        }
        public static async void giveLicense(this Player player, license license, int hours, Player by = null)
        {
            
           if (player.HasData("licenses"))
            {
                var curentlicenses = player.GetData<List<model>>("licenses");
                if (curentlicenses.Find(a => a.type == license) == null)
                {

                    curentlicenses.Add(new model(license, hours));
                    player.SetData("licenses", curentlicenses);
                    await databaseManager.updateQuery($"UPDATE accounts SET licenses = '{NAPI.Util.ToJson(curentlicenses)}' WHERE username = '{player.Name}' LIMIT 1").Execute();
                }
                else if (by != null)
                {
                    by.SendChatMessage($"{player.Name} already has the {license} for {hours}");
                }
              
            }
        }
        public static async Task setLicense(this Player player, license license, int hours, Player by = null)
        {

            if (player.HasData("licenses"))
            {
                var curentlicenses = player.GetData<List<model>>("licenses");
                if (curentlicenses.Find(a => a.type == license) == null)
                {

                    curentlicenses.Add(new model(license, hours));
                    player.SetData("licenses", curentlicenses);
                    await databaseManager.updateQuery($"UPDATE accounts SET licenses = '{NAPI.Util.ToJson(curentlicenses)}' WHERE username = '{player.Name}' LIMIT 1").Execute();
                }
                else
                {
                    curentlicenses.Find(a => a.type == license).hours = hours;
                    player.SetData("licenses", curentlicenses);
                    await databaseManager.updateQuery($"UPDATE accounts SET licenses = '{NAPI.Util.ToJson(curentlicenses)}' WHERE username = '{player.Name}' LIMIT 1").Execute();
                }

            }
        }
        public static async Task<bool> removeLicense(this Player player, license license, Player by = null)
        {
            if (player.HasData("licenses"))
            {
                var curentlicenses = player.GetData<List<model>>("licenses");
                if (curentlicenses.Find(a => a.type == license) != null)
                {
                    curentlicenses.Remove(curentlicenses.Find(a => a.type == license));
                   
                }
                else
                {
                    return false;
                }
                player.SetData("licenses", curentlicenses);

                await databaseManager.updateQuery($"UPDATE accounts SET licenses = '{NAPI.Util.ToJson(curentlicenses)}' WHERE username = '{player.Name}' LIMIT 1").Execute();
                return true;
            }
            return false;
        }
        public static List<model> getLicenses(this Player player)
        {
            return player.HasData("licenses") ? player.GetData<List<model>>("licenses") : new List<model>();
        }
        public static bool hasLicense(this Player player, license lic)
        {
            return player.getLicenses().Find(license=> license.type == lic && license.hours > 0) != null ? true : false;
        }
    }
}
