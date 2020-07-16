using GTANetworkAPI;
using rpg.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace rpg
{
    public class factionsManager : Script
    {
        public enum type
        {
            civilian,
            policeDepartment,
            hitmanAgency
        }
        public class factionMember
        {
            public string name { get; set; }
            public int rank { get; set; }
            public int sqlid { get; set; }
            public int days { get; set; }

            public dynamic raport { get; set; }
        }

        public class model
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<factionMember> factionMembers { get; set; }
            public int membersCount { get; set; }
            public type type { get; set; }
            public int maxMembers { get; set; }
            public Vector3 enterHq { get; set; }
            public Vector3 exitHq { get; set; }
            public bool locked { get; set; }
            public int blip { get; set; }
            public string ipl { get; set; }

        }
        public static List<model> factions = new List<model>();

        [ServerEvent(Event.ResourceStart)]
        public async void onResourceStart()
        {
            factions = await db.loadAllFactions();
            NAPI.Task.Run(() =>
            {
                foreach (var faction in factions)
                {
                    NAPI.TextLabel.CreateTextLabel($"~b~Faction: ~w~{faction.id}\n ~b~Name: ~w~{faction.name}\n ~b~HQ Status: ~w~({ (faction.locked ? "locked" : "unlocked")})", faction.enterHq, 25, 1, 4, new Color(0, 0, 0));
                    var blip = NAPI.Blip.CreateBlip(faction.blip, faction.enterHq, 1, 22, $"{faction.name}");


                    if ( faction.exitHq != new Vector3( 0, 0, 0 ) )
                    {
                        NAPI.TextLabel.CreateTextLabel( $"~r~{faction.name}\n  Exit", faction.exitHq, 25, 1, 4, new Color( 0, 0, 0 ) );
                      
                    }
                    blip.SetSharedData( "factionId", faction.id );
                    NAPI.Marker.CreateMarker(1, new Vector3(faction.enterHq.X, faction.enterHq.Y, faction.enterHq.Z - 0.90f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1f, 140, 4, 36, false, 0);
                    NAPI.Marker.CreateMarker(1, new Vector3(faction.exitHq.X, faction.exitHq.Y, faction.exitHq.Z - 0.90f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1f, 140, 4, 36, false, 0);
                }
            });
        }
        [RemoteEvent("playerEnterHQ")]
         public void onPlayerEnterHQ(Player player, int id )
        {
            enterHq( player, id );
        }
        [RemoteEvent( "playerExitHQ" )]
        public void onPlayerExitHQ( Player player )
        {
            exitHQ( player );
        }
        public void enterHq(Player client, int id)
        {
            var closestHQ = factions.Find(a=> a.id == id);
            if (closestHQ.locked && closestHQ.type != client.GetSharedData<type>("faction"))
            {
                client.SendChatMessage("This HQ is currently closed.");
                return;
            }
            if (closestHQ.exitHq == new Vector3(0, 0, 0))
            {
                client.SendChatMessage("This HQ doesn't have an interior.");
                return;
            }
            client.Dimension = (uint)closestHQ.id;
            client.Position = closestHQ.exitHq;
            client.SetData( "lastEnterHQ", closestHQ.enterHq );
            client.TriggerEvent( "onPlayerEnterHQ", closestHQ.exitHq );
        }

        

        public void exitHQ(Player client)
        {
            if (client.HasData("lastEnterHQ"))
            {
               
                    client.Dimension = 0;
                    client.Position = client.GetData<Vector3>( "lastEnterHQ" );
                    client.ResetData("lastEnterHq");
                
            }
        }

        [Command("duty")]
        public void onDuty(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") == 0)
            {
                player.SendChatMessage("!{#CECECE}You are not a faction member.");
                return;
            }

            switch (player.GetSharedData<type>("faction"))
            {
                case type.policeDepartment:
                    policeDepartment.copDuty(player);
                    break;
                case type.hitmanAgency:
                    hitmanAgency.hitmanDuty(player);
                    break;
                default:
                    break;
            }
        }

        [Command("f", "!{#CECECE}Syntax:~w~ /f [message]", GreedyArg = true)]
        public void onF(Player player, string message)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<type>("faction") != type.hitmanAgency)
            {
                player.SendChatMessage("!{#CECECE}Your group data is invalid.");
                return;
            }

            NAPI.Pools.GetAllPlayers().FindAll(target => target.HasSharedData("faction") && target.GetSharedData<type>("faction") == type.hitmanAgency).ForEach(target => {
                target.SendChatMessage($"{"!{#00B8E6}"}*{hitmanAgency.getRankName(target.GetData<int>("factionRank"))} {player.Name} ({player.Value}): {message}");
            });
        }
        public static string getFactionName(type faction)
        {
            var fa = factions.Find(f => f.type == faction);
           return fa != null ? fa.name : "Civilian";
        }
        public static void checkFactionMember(Player player)
        {
            //topoare ocupate
            var playerFaction = player.HasSharedData("faction") ? player.GetSharedData<type>("faction") : factionsManager.type.civilian;
            if ( playerFaction != factionsManager.type.civilian )
            {
                NAPI.Pools.GetAllPlayers().FindAll(target => target.Value != player.Value && target.HasSharedData("faction") && target.GetSharedData<type>("faction") == playerFaction ).ForEach(target =>
                {
                    target.SendChatMessage($"{"!{#A9C4E4}"}(Group) ~w~{player.Name} from your group has just logged in.");
                });

              
            }
        }

        public static void sendMessageToAll(type faction, string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(a => a.HasSharedData("faction") && a.GetSharedData<type>("faction") == faction).ForEach((player) => player.SendChatMessage(message));
        }
        public static model getClosestHQ(Player player, float distance = 1.5f)
        {
            model faction = null;
            foreach (var factionmodel in factions)
            {
                if (player.Position.DistanceTo(factionmodel.enterHq) < distance)
                {
                    faction = factionmodel;
                    distance = player.Position.DistanceTo(factionmodel.enterHq);
                }
                else if (player.Position.DistanceTo(factionmodel.exitHq) < distance)
                {
                    faction = factionmodel;
                    distance = player.Position.DistanceTo(factionmodel.exitHq);
                }
            }

            return faction;
        }


        public class db
        {
            public static async Task<List<model>> loadAllFactions()
            {
                List<model> factions = new List<model>();
                await databaseManager.selectQuery($"SELECT * FROM factions", (DbDataReader reader) =>
                {
                    model faction = new model();
                    faction.id = (int)reader["id"];
                    faction.membersCount = (int)reader["membersCount"];
                    faction.maxMembers = (int)reader["maxMembers"];
                    faction.enterHq = NAPI.Util.FromJson<Vector3>((string)reader["enterHq"]);
                    faction.exitHq = NAPI.Util.FromJson<Vector3>((string)reader["exitHq"]);
                    faction.ipl = (string)reader["ipl"];
                    faction.factionMembers = NAPI.Util.FromJson<List<factionMember>>((string)reader["factionMembers"]);
                    faction.locked = (bool)reader["locked"];
                    faction.blip = (int)reader["blip"];
                    faction.type = (type)reader["type"];
                    faction.name = (string)reader["name"];

                    factions.Add(faction);
                }).Execute();
                return factions;
            }
        }

    }
}
