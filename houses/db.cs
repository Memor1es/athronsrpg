using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using rpg.Database;
using GTANetworkAPI;

namespace rpg
{
    public class db
    {
        public static async Task<List<house.model>> loadAllhouses( )
        {
            List<house.model> houses = new List<house.model>();
            await databaseManager.selectQuery( "SELECT * FROM houses", ( DbDataReader reader ) =>
            {
                house.model house = new   house.model();

                house.id = (int)reader["id"];
                house.owner = (string)reader["owner"];
                house.enter = NAPI.Util.FromJson<Vector3>((string)reader["enterPos"]);
                house.exit = NAPI.Util.FromJson<Vector3>((string)reader["exitPos"]);
                house.sale = (int)reader["sale"];
                house.rentPrice = (int)reader["rentPrice"];
                house.size = (int)reader["size"];
                house.locked = (bool)reader["locked"];
                house.ipl = (string)reader["ipl"];
                house.renters = NAPI.Util.FromJson<List<string>>((string)reader["renters"]);
                house.description = (string)reader["description"];

                houses.Add( house );    
            } ).Execute( );

            return houses;
        }

        public static async Task<int> addHouse(house.model house)
        {
            int i = await databaseManager.updateQuery($"INSERT INTO houses (owner, enterPos, exitPos, price, ipl, size, description) VALUES ('{house.owner}', '{NAPI.Util.ToJson(house.enter)}', '{NAPI.Util.ToJson(house.exit)}', '{house.sale}', '{house.ipl}', '{house.size}', '{house.description}')").Execute();
            return i;
        }

        public static async Task removeHouse(int id)
        {
            await databaseManager.updateQuery($"DELETE FROM houses WHERE id = '{id}' LIMIT 1").Execute();
        }

     

      
        public static async void lockHouse(int id, bool value)
        {
            await databaseManager.updateQuery($"UPDATE houses SET locked = {value} WHERE id = '{id}' LIMIT 1").Execute();
        }
    }
}
