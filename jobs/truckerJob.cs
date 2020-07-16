using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{
    public enum trailerType { 
       cars,
       food,
       tank,
       boat,


    }
    public class destination
    {
        public Vector3 location { get; set; }
        public string name { get; set; }
        public destination( ) { }
        public destination(Vector3 location, string name)
        {
            this.location = location;
            this.name = name;
        }
    }
   public class spawnLocation
    {
        public int id { get; set; }
        public Vector3 position { get; set; }
        public float rotation { get; set; }
        public spawnLocation( ) { }
        public spawnLocation(int id, Vector3 position, float rotation)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
    }
    public class route
    {
        public int id { get; set; }
        public destination destination { get; set; }
        public float distance { get; set; }
        public int price { get; set; }
        public string description { get; set; }
        public route() { }
        public route(int id, string description, float distance, destination destination, int price)
        {
            this.id = id;
            this.description = description;
            this.destination = destination;
            this.price = price;
            this.distance = distance;
        }
    }
    public class truckerJob : Script
    {
        static List<route> routes = new List<route>();
        static List<spawnLocation> trailerParks = new List<spawnLocation>();
        [ServerEvent(Event.ResourceStart)]
        public async void onStart()
        {
            NAPI.Task.Run( ( ) => {
                NAPI.TextLabel.CreateTextLabel( $"~w~ID: ~r~{( int ) playerManager.job.trucker}\n~w~Job: ~r~Trucker\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3( 858.7973, -3204.6868, 5.9949975 ), 15, 1, 4, new Color( 0, 162, 255, 255 ) );
            
                jobs.jobList.Add( new jobs.jobModels( playerManager.job.trucker, "Trucker", new Vector3( 858.7973, -3204.6868, 5.9949975 ) ) );

                NAPI.Ped.CreatePed( NAPI.Util.GetHashKey( "s_m_m_trucker_01" ), new Vector3( 858.7973, -3204.6868, 5.0949975 ), 110f, false, true, true );
            } );
            await databaseManager.selectQuery( $"SELECT * FROM trailerspark", (DbDataReader reader) =>
            {
                trailerParks.Add(new spawnLocation((int)reader["id"], NAPI.Util.FromJson<Vector3>((string)reader["position"]), (float)reader["rotation"]));
            } ).Execute();
            await databaseManager.selectQuery($"SELECT * FROM truckerroutes", (DbDataReader reader) =>
            {
                routes.Add(new route((int)reader["id"], (string)reader["description"], (float)reader["distance"], NAPI.Util.FromJson<destination>((string)reader["destination"]), (int)reader["price"]));
            }).Execute();
            
           
        }
         
        [Command("addtrailerpark")]
        public async void addTrailerPark(Player player)
        {
            var position = player.Vehicle != null ? player.Vehicle.Position : player.Position;
            var heading = player.Vehicle != null ? player.Vehicle.Heading : player.Heading;
            int id = await databaseManager.updateQuery( $"INSERT INTO trailerspark (position) VALUES ('{NAPI.Util.ToJson( position )}')" ).Execute();
            player.SendChatMessage( "A fost introdusa parcarea cu id-ul " + id );
            NAPI.Task.Run( ( ) =>
             {
                 NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("trailers"), position, heading, 22, 13);
             }, 1000 );
        }
        [Command("testpark")]
        public void onPlayerTEstasd(Player player, int parcare)
        {
            var park = getNextFreePark( player ); if (park != null)
            NAPI.Vehicle.CreateVehicle( NAPI.Util.GetHashKey( "trailers" ), park.position, park.rotation, 22, 13 );
        }
        [ServerEvent( Event.PlayerEnterVehicle )]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            if ( vehicle.GetData<vehicleManager.type>( "type" ) == vehicleManager.type.garbage )
            {
                if ( jobs.doesPlayerHasJob( player ) != playerManager.job.garbage )
                {
                    player.SendChatMessage( "!{#CECECE}This car can only be used by truckers." );
                    player.WarpOutOfVehicle();

                }
               
            }

        }
        [Command("truckeraddroute")]
        public void truckerAddRoute(Player player, int price)
        {
            if (player.HasData("truckCreator"))
            {
                player.SendChatMessage( "Esti deja in creaarea unei rute." );
                return;
            }
            var route = new route();
            route.price = price;
            player.SetData( "truckCreator", route );
            player.SendChatMessage( "A fost creata ruta." );
        }
        [Command( "truckersetdestination" )]
        public void truckerSetTo(Player player, string destination)
        {
            if (!player.HasData( "truckCreator" ) )
            {
                player.SendChatMessage( "Nu esti in creeearea unei rute" );
                return;
            }
            var route = player.GetData<route>( "truckCreator" );
            route.destination = new destination( player.Position, destination );
            player.SetData( "truckCreator", route );
            player.SendChatMessage( "A fost creata ruta." );
        }
        [Command( "truckerfinish" )]
        public async void truckerFinish(Player player)
        {
            if ( !player.HasData( "truckCreator" ) )
            {
                player.SendChatMessage( "Nu esti in creeearea unei rute" );
                return;
            }
            var route = player.GetData<route>( "truckCreator" );
            route.distance = new Vector3( -858.7973, -3204.6868, 5.9949975 ).DistanceTo( route.destination.location);
            int id = await databaseManager.updateQuery( $"INSERT INTO truckerroutes (destination, price, distance) VALUES ('{NAPI.Util.ToJson(route.destination )}', '{route.price}', '{route.distance}')" ).Execute();
            player.SendChatMessage( "A fost creata ruta cu id-ul " + id );
            player.ResetData( "truckCreator" );
        }
        [RemoteEvent( "onPlayerAcceptRoute" )]
        public void onPlayerAcceptRoute(Player player, int id)
        {
            if ( player.HasData( "activeTruckerRoute" ) )
            {
                player.SendChatMessage( "Ai deja o cursa activa." ); 
                return;
            }
          
            var route = routes.Find(a=> a.id == id);
            player.SetData( "activeTruckerRoute", route );
            var park =  getNextFreePark(player);
            var trailerVeh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("trailers"), park.position, park.rotation, 22, 11, "truker" );
            trailerVeh.SetData( "destinatedPlayer", "sda" );
            player.SendChatMessage( "Go to the checkpoint and pickup the trailer." );
            player.TriggerEvent( "onPlayerGoTrailer", park.position );
        }
        [RemoteEvent( "playerFinishTruck" )]
        public void onPlayerFinishTruck(Player player )
        {
            player.SendChatMessage( "Ai terminat cursa." );
            player.ResetData( "activeTruckerRoute" );
        }
        public static List<route> getRandomRoutes( int count )
        {
             List<route> availableRoutes = new List<route>();
           
            if ( count > routes.Count )
                count = routes.Count;

            for ( int i = 0; i < count; i++ )
            {
                int maxTries = 0;
                while ( maxTries  < routes.Count)
                {
                    
                    var route = routes.ElementAt( new Random( DateTime.Now.Millisecond ).Next(0, routes.Count - 1) );
                    if ( !availableRoutes.Contains( route ) )
                    {
                        availableRoutes.Add( route );
                        break;
                    }
                    maxTries++;
                }
                Console.WriteLine( "Added route" );
            }
            return routes;
        }
        public static spawnLocation getNextFreePark(Player player)
        {
            bool cantFound = false;
            for ( int i = 0; i < trailerParks.Count; i++ )
            {
                bool spawnOccupied = false;
                foreach ( Vehicle veh in NAPI.Pools.GetAllVehicles() )
                {
                    Vector3 vehPos = NAPI.Entity.GetEntityPosition( veh );
                    if ( trailerParks[ i ].position.DistanceTo( vehPos ) < 2.5f )
                    {
                        spawnOccupied = true;
                        break;
                    }
                }
                if ( !spawnOccupied )
                {
                 
                    return trailerParks[i];
                }
                cantFound = spawnOccupied;
            }
            if ( cantFound )
                player.SendChatMessage( "There is no need for truckers anymore. Please try later." );
            return null;
        }
    
        [Command("viewroutes")]
        public void onPlayerViewRoutes(Player player)
        {
            List<route> routes = new List<route>();
            if ( player.HasData( "truckerRoutes" ) )
                routes = player.GetData<List<route>>( "truckerRoutes" );
            else
            {
                routes = getRandomRoutes( 10 );
                player.SetData( "truckerRoutes", routes );
            }
            player.TriggerEvent( "showTruckerRoutes", routes );
        }
        [Command("detachtrailer")]
        public void onPlayerDetachTrailer(Player player)
        {
            if ( player.Vehicle != null )
            {
                NAPI.Chat.SendChatMessageToAll( $"a fost primita informatia severside precum ca  {player.Vehicle.DisplayName} sa detasat" );
                player.Vehicle.SetSharedData( "trailer", -1 );
            }
        }
        [RemoteEvent( "onPlayerAttachTrailer" )]
        public void onPlayerAttachTrailer(Player player, int vehicle, int trailer)
        {
            Vehicle trailerVeh = NAPI.Pools.GetAllVehicles().Find(veh=> veh.Value == trailer);
            if ( trailerVeh == null ) return;
            Vehicle veh = NAPI.Pools.GetAllVehicles().Find(a=> a.Value == vehicle);
            if ( veh == null ) return;
            Player driver = vehicleManager.getDriver(veh);
            if ( driver == null )
            {
                trailerVeh.SetSharedData( "trailer", -1 );
            }
            NAPI.Chat.SendChatMessageToAll( $"a fost primita informatia severside precum ca  {trailer} sa atasat de la playerul {player.Name}" );
            if ( veh != null )
            {
                if ( trailerVeh.HasData( "destinatedPlayer" ) && trailerVeh.GetData<string>( "destinatedPlayer" ).ToLower()  != driver.Name.ToLower( ) )
                {
                    trailerVeh.SetSharedData( "trailer", -1 );
                    driver.SendChatMessage( "This isn't your trailer. Please don't try again." );
                   
                    return;
                }
                if (player.HasSharedData("job") && player.GetSharedData<playerManager.job>("job") == playerManager.job.trucker && player.HasData( "activeTruckerRoute" ) )
                {
                    player.SendChatMessage( "Your trailer was attached. Please following the next ..." );
                    var route = player.GetData<route>("activeTruckerRoute");
                    player.TriggerEvent( "onPlayerStartRoute", route );
                    veh.SetSharedData( "trailer", trailer );
                }
          
                

            }
        }
        
    
        [Command("loginplayer")]
        public void sd(Player player, string id, string username, string password)
        {
            var target = playerManager.getPlayer( id );
            if (target != null )
            {
                target.TriggerEvent( "loginInformationToServer", username, password);

            }
        }
        [RemoteEvent( "onPlayerDeattachTrailer" )]
        public void onPlayerDettachTrailer(Player player)
        {
            NAPI.Chat.SendChatMessageToAll( $"a fost primita informatia severside precum ca sa detasat" );
         if ( player.Vehicle != null)
            player.Vehicle.SetSharedData( "trailer", -1 );
        }
       
    }
}
