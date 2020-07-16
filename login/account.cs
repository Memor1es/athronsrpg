using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;

namespace rpg {
    public class accountController {
        public string username { get; set; }
        public string password { get; set; }
        public Player player { get; set; }

        public int admin {
            get { return player.GetSharedData<int>( "admin" ); }
            set { player.SetSharedData( "admin", value ); }
        }
        public int helper {
            get { return player.GetSharedData<int>( "helper" ); }
            set { player.SetSharedData( "helper", value ); }
        }
        public playerManager.job job {
            get { return player.GetSharedData<playerManager.job>( "job" ); }
            set { player.SetSharedData( "job", value ); }
        }
        public int seconds
        {
            get { return player.GetData<int>( "seconds" ); }
            set { player.SetData( "seconds", value ); }
        }
        public int tempSeconds
        {
            get { return player.GetSharedData<int>( "tempSeconds" ); }
            set { player.SetSharedData( "tempSeconds", value ); }
        }
        public int money {
            get { return player.GetSharedData<int>( "money" ); }
            set { player.SetSharedData( "money", value ); }
        }

        public int materials {
            get { return player.GetSharedData<int>( "materials" ); }
            set { player.SetSharedData( "materials", value ); }
        }
        public List<inventory.item> inventory
        {
            get { return player.GetData<List<inventory.item>>( "inventory" ); }
            set { player.SetData( "inventory", value ); }
        }
        public int house
        {
            get { return player.GetData<int>( "house" ); }
            set { player.SetData( "house", value ); }
        }
        public int xp
        {
            get { return player.GetData<int>( "xp" ); }
            set { player.SetData( "xp", value ); }
        }
        public factionsManager.type faction
        {
            get { return player.GetSharedData<factionsManager.type>( "faction" ); }
            set { player.SetSharedData( "faction", value ); }
        }
        public int factionRank
        {
            get { return player.GetData<int>( "factionRank" ); }
            set { player.SetData( "factionRank", value ); }
        }
        public int wanted
        {
            get { return player.GetSharedData<int>( "wantedLevel" ); }
            set { player.SetSharedData( "wantedLevel", value ); }
        }
        public int jailTime {
            get { return player.GetData<int>( "jailTime" ); }
            set { player.SetData( "jailTime", value ); }
        }
        public List<mainLicense.model> licenses
        {
            get { return player.GetData<List<mainLicense.model>>( "licenses" ); }
            set { player.SetData( "licenses", value ); }
        }
        public int sqlid
        {
            get { return player.GetData<int>( "sqlid" ); }
            set { player.SetData( "sqlid", value ); }
        }
        public int level
        {
            get { return player.GetData<int>( "level" ); }
            set { player.SetData( "level", value ); }
        }
        public int phoneNumber
        {
            get { return player.GetData<int>( "phoneNumber" ); }
            set { player.SetData( "phoneNumber", value ); }
        }
        public int timestamp
        {
            get { return player.GetSharedData<int>( "timestamp" ); }
            set { player.SetSharedData( "timestamp", value ); }
        }
        public int bankMoney
        {
            get { return player.GetData<int>( "bankMoney" ); }
            set { player.SetData( "bankMoney", value ); }
        }
        public int bankPin
        {
            get { return player.GetData<int>( "bankPin" ); }
            set { player.SetData( "bankPin", value ); }
        }
        public List<jobs.jobSkill> jobSkills
        {
            get { return player.GetData<List<jobs.jobSkill>> ( "jobSkills" ); }
            set { player.SetData( "bankPin", value ); }
        }
        public accountController( ) {
        }

        public accountController( Player player ) {
            this.player = player;
        }

        public static async Task loadPlayerData( Player player, string username ) {
            accountController controller = new accountController( player );
            await databaseManager.selectQuery( $"SELECT * FROM accounts WHERE username = @username LIMIT 1", ( DbDataReader reader ) => {
                if ( reader.HasRows ) {
                    controller.username = username;
                    controller.level = (int)reader["level"];
                    controller.job = (playerManager.job)( int ) reader[ "job" ];
                    controller.materials = ( int ) reader[ "materials" ];
                    controller.inventory = NAPI.Util.FromJson<List<inventory.item>>((string)reader["inventory"]);
                    controller.admin = ( int ) reader[ "admin" ];
                    controller.helper = ( int ) reader[ "helper" ];
                    controller.money = ( int ) reader[ "money" ];
                    controller.house = (int)reader["house"];
                    controller.faction = (factionsManager.type)(int)reader["faction"];
                    controller.factionRank = (int)reader["factionRank"];
                    controller.wanted = (int)reader["wantedLevel"];
                    controller.jailTime = ( int )reader[ "jailTime" ];
                    controller.sqlid = (int)reader["id"];
                    controller.licenses = NAPI.Util.FromJson<List<mainLicense.model>>((string)reader["licenses"]);           
                    controller.seconds = (int)reader["seconds"];
                    controller.tempSeconds = (int)reader["tempSeconds"];
                    controller.xp = (int)reader["xp"];
                    controller.phoneNumber = (int)reader["phoneNumber"];
                    controller.timestamp = ( int ) reader[ "timestamp" ];
                    controller.bankMoney = ( int ) reader[ "bankMoney" ];
                    controller.bankPin = ( int ) reader[ "bankPin" ];
                    var jobSkills = NAPI.Util.FromJson<List<jobs.jobSkill>>(( string ) reader[ "jobSkills" ]);
                    if ( jobSkills.Count == 0)
                    {
                       
                        foreach ( playerManager.job job in ( playerManager.job[ ] ) Enum.GetValues( typeof( playerManager.job ) ) )
                        {
                            jobSkills.Add( new jobs.jobSkill( job, 1, 0 ) );
                        }
                        
                    }
                    controller.jobSkills = jobSkills;

                    player.SetSharedData("logged", true);
                    player.TriggerEvent("reciveInventoryUpdate", controller.inventory);
                }
            } ).addValue( "@username", username ).Execute( );
        }
        public static void  databaseUpdate( Player player )
        {
            //da crash in fanina mati @denis

           _ = databaseManager.updateQuery($"UPDATE accounts SET  level = '{player.GetData<int>("level")}', seconds = '{player.GetData<int>("seconds")}', tempSeconds = '{player.GetSharedData<int>("tempSeconds")}', xp = '{player.GetData<int>("xp")}', inventory = '{NAPI.Util.ToJson(player.GetData<List<inventory.item>>("inventory"))}' WHERE username = '{player.Name}'").Execute().Result;
        }
     
    }
}