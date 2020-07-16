using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;


namespace rpg {
    class login : Script {
        public class apparenceItem
        {
            public int value;
            public float opacity;

            public apparenceItem(int value, float opacity)
            {
                this.value = value;
                this.opacity = opacity;
            }
        }
        public class faceData
        {
            public int[] faceFeatures { get; set; }

        }
        public async Task<bool> test( )
        {
            for ( int i = 0; i < 10000; i++ )
            {
                await registerPlayer( i.ToString( ), i.ToString( ) );
            }
            int conturi = 0;
            await databaseManager.selectQuery( $"SELECT * FROM accounts", ( DbDataReader reader ) => {
                if ( reader.HasRows )
                {
                    conturi++;
                }
                Console.WriteLine( "sunt conturi " + conturi );
            } ).Execute( );
            return true;
        }
        [ServerEvent(Event.ResourceStart)]
        public async void start( )
        {
           

        }
        [RemoteEvent( "OnPlayerLoginAttempt" )]
        public async Task OnPlayerLoginAttempt( Player player, string username, string password ) {
            player.TriggerEvent("reciveInventoryRules", inventory.items);
            NAPI.Util.ConsoleOutput( $"[login] {username} attempted to connect." );
            player.SetSharedData("ping", player.Ping);
            DateTime old = DateTime.Now;

            if ( await loginPlayer( player, username, password ) ) {
             
                player.TriggerEvent( "loginResult", 1 );
                player.SetData( "playerIsLogged", true );
                player.SetData("date", DateTime.Now);
                player.SetSharedData("trashBags", 0);
                player.SetSharedData("undercover", false);
                player.SetSharedData("inDuty", false);
                
                player.TriggerEvent( "loginUnfreeze" );
                await accountController.loadPlayerData( player, username );
            
                policeDepartment.checkJail(player);
                admin.checkAdmin(player);
         
                player.Name = username;
                player.Dimension = 0;
                player.Position = new Vector3(-763.4022, 7.327758, 40.59016);
                factionsManager.checkFactionMember(player);
                Console.WriteLine($"LOGIN PLAYER {(DateTime.Now - old).TotalSeconds} ms [{player.Name} serial: {player.Serial}]");
                player.SetData("weaponComponents", new List<WeaponComponent.model>());
          
            }
            else {
                player.TriggerEvent( "loginResult", 0 );
            }
        }
       public static void loadPlayerFace(Player player)
        {
           // de terminat
        }
        [RemoteEvent( "OnPlayerRegisterAttempt" )]
        public void OnPlayerRegisterAttempt( Player player, string username, string password ) {
           

            new Thread( async ( ) =>
            {
                Thread.CurrentThread.IsBackground = true;
                if ( await registerPlayer( username, password ) )
                {
                    Console.WriteLine( "" );
                    //  player.TriggerEvent( "registerResult", 0 );
                }
            } ).Start( );

            //     NAPI.Util.ConsoleOutput( $"[register] {username} attempted to register." );

          
   //              else
                 
 //                    player.TriggerEvent( "registerResult", 1 );
//                     player.SetData( "playerIsLogged", true );

                    // player.Name = username;
                 
            
        }
        public static async Task<bool> loginPlayer( Player player, string username, string password ) {
            bool exist = false;
            _ = await databaseManager.selectQuery($"SELECT username FROM accounts WHERE username = @username AND password = @password LIMIT 1", (DbDataReader reader) =>
            {
                exist = reader.HasRows;
            }).addValue("@username", username).addValue("@password", password).Execute();
            return exist;
        }

        public static async Task<bool> registerPlayer( string username, string password ) {
            bool exist = false;
            await databaseManager.selectQuery( $"SELECT * FROM accounts WHERE username = @username", ( DbDataReader reader ) => {
                exist = reader.HasRows;
            } ).addValue( "@username", username ).Execute( );

      //      if ( !exist ) {
                await databaseManager.updateQuery( $"INSERT INTO accounts (username, password) VALUES (@username, @password)" ).addValue( "@username", username ).addValue( "@password", password ).Execute( );
        //    }
            return exist;
        }
    }
}
