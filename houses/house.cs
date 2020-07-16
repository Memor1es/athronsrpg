using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using Bootstrapper;
using RAGE;
using rpg.Database;

namespace rpg
{
    public class house : Script
    {
        #region
        public Vector3 getHouses( int size )
        {
            Random random = new Random();
            Vector3 returnPos = new Vector3();

            Vector3[] smallHouses = new[] { new Vector3(-9.96562, -1438.54, 31.1015), new Vector3(-1150.703, -1520.713, 10.633) };

            Vector3[] mediumHouses = new[] { new Vector3(347.2686, -999.2955, -99.19622) };

            Vector3[] bigHouses = new[] { new Vector3( -802.311, 175.056, 72.8446 ), new Vector3( -169.286, 486.4938, 137.4436 ),
                                            new Vector3( 340.9412, 437.1798, 149.3925 ), new Vector3( -676.127, 588.612, 145.1698 ),
                                            new Vector3( 120.5, 549.952, 184.097 ) };
            switch ( size )
            {
                case 0:
                    returnPos = smallHouses[ random.Next( 0, 1 ) ];
                    break;
                case 1:
                    returnPos = mediumHouses[ 0 ];
                    break;
                case 2:
                    returnPos = bigHouses[ random.Next( 0, 4 ) ];
                    break;
            }
            return returnPos;
        }

        public class model
        {
            public int sale { get; set; }
            public String owner { get; set; }
            public Vector3 enter { get; set; }
            public TextLabel enterTextLabel { get; set; }
            public TextLabel exitTextLabel { get; set; }
            public Marker enterTextMarker { get; set; }
            public Blip enterBlip { get; set; }
            public Marker exitTextMarker { get; set; }
            public Vector3 exit { get; set; }
            public int size { get; set; }

            public String ipl { get; set; }
            public int id { get; set; }
            public bool locked { get; set; }
            public int rentPrice { get; set; }
            public List<string> renters { get; set; }
            public string description { get; set; }

            public model( ) { }


            public model( String owner, int id, int price, int rentPrice, Vector3 interior, String type, String ipl, string description )
            {
                this.owner = owner;
                this.id = id;
                this.sale = price;
                this.ipl = ipl;
                this.size = size;
                this.ipl = ipl;
                this.description = description;
            }
        }
        public static List<string> sizeHouse = new List<string>()
        {
            "Small",
            "Medium",
             "Big"
        };
        #endregion
        [ServerEvent(Event.ResourceStart)]
        public async void onResourceStart()
        {
            houses = await db.loadAllhouses();
            foreach (var house in houses)
            {
               

                NAPI.Task.Run(() =>
                {
                    house.enterTextLabel = NAPI.TextLabel.CreateTextLabel( $"~w~House ~b~{house.id}\n Description: ~b~{house.description}\n ~w~Owner: ~b~{house.owner}\n ~w~Size: ~b~{sizeHouse[ house.size ]}\n {( house.rentPrice > 0 ? $"~w~Tenants: ~b~{house.renters.Count}\n ~w~Rent: ~b~{house.rentPrice}\n ~b~To rent a room, type /rentroom \n" : "" )}  {( house.sale > 0 ? $"Price: {house.sale} \n /buyhouse" : "" )}", house.enter, 5f, 0.05f, 4, new Color(0, 128, 128)); 
                    house.enterTextMarker = NAPI.Marker.CreateMarker(0, house.enter, new Vector3(), new Vector3(), 0.5f, new Color(255, 255, 0), false, 0);
                    house.enterBlip = NAPI.Blip.CreateBlip(350, house.enter, 1, 22, $"House {house.id}");
                    house.enterBlip.SetSharedData( "houseId", house.id );
                    if (house.exit != null)
                    {
                 //       house.exitTextLabel = NAPI.TextLabel.CreateTextLabel($"House [{house.id}] \n Exit", house.exit, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, (uint)house.id);
                   //     house.exitTextMarker = NAPI.Marker.CreateMarker(0, house.exit, new Vector3(), new Vector3(), 1, new Color(0, 255, 0), true, (uint)house.id);
                    }
                  

                } );
            }
        }
        public static List<model> houses = new List<model>();

        public string houseIpl(int type)
        {
            string returnIpl = "noIPL";
            switch (type)
            {
                case 0:
                    returnIpl = "noIPL";
                    break;
                case 1:
                    returnIpl = "noIPL";
                    break;
                case 2:
                    returnIpl = "noIPL";
                    break;
                default:
                    break;
            }
            return returnIpl;
        }

        public static model getClosestHouse( Player player, float distance = 1.5f )
        {
            model house = null;
            foreach ( var housemodel in houses )
            {
                if ( player.Position.DistanceTo( housemodel.enter ) < distance )
                {
                    house = housemodel;
                    distance = player.Position.DistanceTo( housemodel.enter );
                }
                else if(player.Position.DistanceTo(housemodel.exit) < distance)
                {
                    house = housemodel;
                    distance = player.Position.DistanceTo(housemodel.exit);
                }
            }
        
            return house;
        }

        [Command("createhouse", "!{#CECECE}Syntax: ~w~/createhouse <size - 0/1/2> 0 = small, 1 = medium, 2 = big")]
        public async void createHouse(Player player, int size)
        {
            if (size >= 3)
            {
                player.SendChatMessage("!{#CECECE}Unknown size");
                return;
            }

            model house = new model();
            house.owner = "AdmBot";
            house.description = "House";
            house.sale = 5000000;
            house.size = size;
            house.enter = player.Position;
            house.exit = getHouses(size);
            house.ipl = houseIpl(size);

            house.id = await db.addHouse(house);
            houses.Add(house);

            player.SendChatMessage($"~y~House (id: {house.id}) was successfully created.");
        //    SendMessageToAdminsAdmCmd($"{player.Name} (id: {new accountController(player).sqlid}) created a new house (id:{house.id}).");

            NAPI.Task.Run(() =>
            {
                house.renters = new List<string>();
                house.enterTextLabel = NAPI.TextLabel.CreateTextLabel($"~w~House ~b~{house.id}\n ~b~{house.description}\n ~w~Owner: ~b~{house.owner}\n ~w~Size: ~b~{house.size}\n ~w~Tenants: ~b~{house.renters.Count}\n ~w~Rent: ~b~{house.rentPrice}\n ~b~To rent a room, type /rentroom", house.enter, 5f, 0.05f, 4, new Color(0, 128, 128)); ;
                house.enterTextMarker = NAPI.Marker.CreateMarker(0, house.enter, new Vector3(), new Vector3(), 0.5f, new Color(255, 255, 0), false, 0);
               
            });
        }

        [Command("removehouse", "!{#CECECE}Syntax: ~w~/removehouse <house id>")]
        public async void removeHouse(Player player, int houseId)
        {
            model deletedHouse = houses.Find(house => house.id == houseId);
            if (deletedHouse == null)
            {
                player.SendChatMessage($"{"!{#CECECE}"}The house with (id: {houseId}) doesn't exist.");
                return;
            }

            player.SendChatMessage($"~y~House (id: {houseId}) was successfully deleted.");
         //   SendMessageToAdminsAdmCmd($"{player.Name} (id: {new accountController(player).sqlid}) deleted house (id: {houseId}).");

            await db.removeHouse(houseId);
            houses.Remove(deletedHouse);

            NAPI.Task.Run(() => {
                deletedHouse.enterTextLabel.Delete();
                deletedHouse.enterTextMarker.Delete();
                deletedHouse.exitTextLabel.Delete();
                deletedHouse.exitTextMarker.Delete();
            });
        }

        [Command("rentroom")]
        public async void rentRoomCommand(Player player)
        {
            if (player.HasData("house") && player.GetData<int>("house") == -1)
              await  playerRentRoom(player);
            else
                player.SendChatMessage("You are already a renter or you own a house.");
        }
        [Command("unrentroom")]
        public async void unRentRoomCommand(Player player)
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null)
            {
                player.SendChatMessage("You are not near a house.");
                return;
            }
            if (player.GetData<int>("house") == closestHouse.id)
            {
                if ( closestHouse.owner == player.Name)
                {
                    player.SendChatMessage("You can't unrent from your own house.");
                    return;
                }
             
                if ( closestHouse.renters.Contains(player.Name))
                    closestHouse.renters.Remove(player.Name);

                await databaseManager.updateQuery( $"UPDATE accounts SET house = '-1' WHERE username = '{player.Name}' LIMIT 1" ).Execute( );
                await databaseManager.updateQuery( $"UPDATE houses SET renters = '{NAPI.Util.ToJson( closestHouse.renters)}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
               
                player.SendChatMessage($"Room unrented, you are now homeless.");
                updateHouseLabel( closestHouse );
                if (player.HasData("positionBeforeEnteringHouse"))
                {
                    player.Dimension = 0;

                    player.Position = player.GetData<Vector3>("positionBeforeEnteringHouse");

                    player.ResetData("positionBeforeEnteringHouse");
                }
            }
            else
                player.SendChatMessage("You are not rented to this house.");
        }
        [Command("tenants")]
        public void viewTentantCommand(Player player)
        {
            var house = houses.Find(house => house.id == player.GetData<int>("house"));
            if ( house == null)
            {
                player.SendChatMessage("You don't own a house.");
                return;
            }
            player.SendChatMessage("Your tenants are:");
            house.renters.ForEach(renter =>
            {
                player.SendChatMessage(renter);
            });

        }
        [RemoteEvent( "playerEnterHouse" )]
        public void playerEnterHouseEvent( Player player, int id )
        {

              var closestHouse = houses.Find(a=> a.id == id);
                if ( closestHouse == null )
                {
                    return;
                }
                playerEnterHouse( player, closestHouse );
                player.TriggerEvent( "onPlayerEnterHouse", closestHouse.exit );
        }
        [RemoteEvent("playerExitHouse")]
        public void playerExitHouseEvent(Player player )
        {
            playerExitHouse( player );
        }
        [Command("enterhouse")]
        public void enterHouseCommand(Player player)
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null)
            {
                return;
            }
            playerEnterHouse( player, closestHouse );
        }

        [Command("exithouse")]
        public void exitHouseCommand(Player player)
        {
            playerExitHouse( player );
        }

        [Command("buyhouse")]
        public async void onPlayerBuyHouse(Player player )
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null )
            {
                player.SendChatMessage( "Your need to be near a house." );

                return;
            }
            if ( closestHouse.owner == player.Name )
            {
                player.SendChatMessage( "You can't buy your house." );
                return;
            }
            if (closestHouse.sale <= 0)
            {
                player.SendChatMessage( "This house isn't for sale." );
                return;
            }
            if (player.GetSharedData<int>("money") < closestHouse.sale)
            {
                player.SendChatMessage( "You don't have enough money to buy this house." );
                return;
            }
            if ( closestHouse.owner != "AdmBot" )
            {
                var oldOwner = playerManager.getPlayer(closestHouse.owner);
                if ( oldOwner != null )
                {
                    oldOwner.SendChatMessage( $"Your house was bought by {player.Name} for { closestHouse.sale }. For the moment you are homeless but rich as fuck." );
                    await databaseManager.updateQuery( $"UPDATE accounts SET house = '-1' WHERE username = '{closestHouse.owner}'" ).Execute( );
                    player.SetData( "house", -1 );
                    await oldOwner.giveMoney( closestHouse.sale );
                }
                else
                {
                    await moneyManager.giveMoneyDatabase( closestHouse.owner, closestHouse.sale );
                    await databaseManager.updateQuery( $"UPDATE accounts SET house = '-1' WHERE username = '{closestHouse.owner}'" ).Execute( );
                }
            }


            await player.takeMoneyAsync( closestHouse.sale );
            player.SendChatMessage( $"You successfully bought this house for ${closestHouse.sale}" );
            closestHouse.owner = player.Name;
            closestHouse.sale = 0;
            var existAsRenter = closestHouse.renters.Find( a => a == player.Name );
            if ( existAsRenter  != null)
            closestHouse.renters.Remove( existAsRenter );
            updateHouseLabel( closestHouse );
             await databaseManager.updateQuery( $"UPDATE houses SET owner = '{closestHouse.owner}', renters = '{NAPI.Util.ToJson(closestHouse.renters)}', sale = '0' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
            await databaseManager.updateQuery( $"UPDATE accounts SET house = '{closestHouse.id}' WHERE username = '{closestHouse.owner}'" ).Execute( );


        }
        [Command("descriptionhouse", GreedyArg = true , Alias = "housename")]
        public async void playerDescriptionHouseAsync(Player player, string description)
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null )
            {
                player.SendChatMessage( "Your need to be near a house." );

                return;
            }
            if ( player.GetData<int>( "house" ) == closestHouse.id && closestHouse.owner == player.Name )
            {
                if (description.Length > 15)
                {
                    player.SendChatMessage( "Your description shoudn't exceed 15 characters." );
                    return;
                }
                if (description.Contains("pula") || description.Contains("coaie") || description.Contains("sugi") || description.Contains("muie") || description.Contains("dick") || description.Contains("ksenon"))
                {
                    player.SendChatMessage( "Your description contains forbidden words." );
                    return;
                }
                closestHouse.description = description;
                await databaseManager.updateQuery( $"UPDATE houses SET description = '{closestHouse.description}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
                player.SendChatMessage( $"Your house description is updated to {description}");
                updateHouseLabel( closestHouse );
            }
            else
                player.SendChatMessage( "You are not the owner of the house. " );
        }
        [Command( "sellhouse")]
        public async void onPlayerSellHouse( Player player, int price )
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null )
            {
                player.SendChatMessage( "Your need to be near a house." );

                return;
            }
            if ( player.GetData<int>( "house" ) == closestHouse.id && closestHouse.owner == player.Name )
            {
                
                closestHouse.sale = price;
                await databaseManager.updateQuery( $"UPDATE houses SET sale = '{closestHouse.sale}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
                player.SendChatMessage( $"Your house is for sell for {closestHouse.sale}" );
                updateHouseLabel( closestHouse );
            }
            else
                player.SendChatMessage( "You are not the owner of the house. " );
        }
        [Command( "cancelsellhouse")]
        public async void onPlayerCancelSellHouse( Player player )
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null )
            {
                player.SendChatMessage( "Your need to be near a house." );

                return;
            }
            if ( player.GetData<int>( "house" ) == closestHouse.id && closestHouse.owner == player.Name )
            {
                if (closestHouse.sale <= 0 )
                {
                    player.SendChatMessage( "Your house isn't for sell." );
                    return;
                }

                closestHouse.sale = 0;
                await databaseManager.updateQuery( $"UPDATE houses SET sale = '0' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
                player.SendChatMessage( $"Your house isn't for sell anymore." );
                updateHouseLabel( closestHouse );
            }
            else
                player.SendChatMessage( "You are not the owner of the house. " );
        }
        public static async Task onPayDayWork( )
        {
            foreach ( var house in houses )
            {
                foreach ( var renter in house.renters )
                {
                    var playerRenter = playerManager.getPlayer(renter);
                    if ( playerRenter != null )
                        await playerRenter.takeMoneyAsync( house.rentPrice );

                   

                }
                var houseOwner = playerManager.getPlayer(house.owner);
                if ( houseOwner != null )
                    await houseOwner.bankGiveMoney( house.rentPrice * house.renters.Count );
                else
                    await moneyManager.bankGiveMoney( null, house.rentPrice * house.renters.Count, house.owner);
            }
        }
        [Command("rentprice")]
        public async void setRentable(Player player, int price )
        {
            var closestHouse = getClosestHouse(player);
            if ( closestHouse == null )
            {
                player.SendChatMessage( "Your need to be near a house." );

                return;
            }
            if ( player.GetData<int>( "house" ) == closestHouse.id && closestHouse.owner == player.Name )
            {

                closestHouse.rentPrice = price;

                await databaseManager.updateQuery( $"UPDATE houses SET rentPrice = '{closestHouse.rentPrice}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
                player.SendChatMessage( $"House updated rent price to {price}." );
                updateHouseLabel( closestHouse );
            }
            else
                player.SendChatMessage( "You are not the owner of the house. " );

        }
        [Command("lockhouse")]
        public async void lockHouse(Player player)
        {
            var closestHouse = getClosestHouse(player);
            if (closestHouse == null)
            {
                player.SendChatMessage("Your need to be near a house.");
               
                return;
            }
            if (player.GetData<int>("house") == closestHouse.id && closestHouse.owner == player.Name)
            {

                closestHouse.locked = !closestHouse.locked;


                var s = closestHouse.locked ? "locked." : "unlocked.";

                await databaseManager.updateQuery( $"UPDATE houses SET locked = '{closestHouse.locked}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
                player.SendChatMessage($"House is now {s}.");
                updateHouseLabel( closestHouse );
            }
            else
                player.SendChatMessage("You are not the owner of the house. ");

           
        }

        public static void updateHouseLabel( model house )
        {
            NAPI.Task.Run( ( ) => {

                house.enterTextLabel.Text = $"~w~House ~b~{house.id}\n Description: ~b~{house.description}\n ~w~Owner: ~b~{house.owner}\n ~w~Size: ~b~{sizeHouse[ house.size ]}\n {(house.rentPrice > 0 ? $"~w~Tenants: ~b~{house.renters.Count}\n ~w~Rent: ~b~{house.rentPrice}\n ~b~To rent a room, type /rentroom \n" : "")}  {( house.sale > 0 ? $"Price: {house.sale} \n /buyhouse" : "" )}";


            } );
        }

        public async Task playerRentRoom(Player player)
        {
            model closestHouse = getClosestHouse(player);
            if (closestHouse == null)
                return;
            if (closestHouse.rentPrice <= 0)
            {
                player.SendChatMessage( "This house is unrentable." );
                return;
            }

            if (!closestHouse.renters.Contains(player.Name))
            {
                closestHouse.renters.Add(player.Name);

                playerEnterHouse(player, closestHouse, true);
                updateHouseLabel( closestHouse );
                player.TriggerEvent( "onPlayerEnterHouse", closestHouse.exit );

                await databaseManager.updateQuery( $"UPDATE accounts SET house = '{closestHouse.id}' WHERE username = '{player.Name}' LIMIT 1" ).Execute( );

                await databaseManager.updateQuery( $"UPDATE houses SET renters = '{NAPI.Util.ToJson( closestHouse.renters )}' WHERE id = '{closestHouse.id}' LIMIT 1" ).Execute( );
            }
           

            player.SendChatMessage("~y~House rented.");

        }
        
        public void playerEnterHouse(Player player, model closestHouse, bool bypassdoor = false)
        {

            if (closestHouse == null)
            {
                player.SendChatMessage("No house nearby.");
                return;
            }
            if (closestHouse.locked && !bypassdoor)
            {
                player.SendChatMessage("Door locked.");
                return;
            }
            if (jobs.doesPlayerHasJob(player) == playerManager.job.pizza && player.HasSharedData("inJob") && player.GetSharedData<bool>("inJob"))
            {
                player.SendChatMessage("Cannot enter house durring pizza job.");
                return;
            }
            player.SetData("positionBeforeEnteringHouse", player.Position);
            player.Dimension = (uint)closestHouse.id;
            player.Position = closestHouse.exit;
            //player.SendChatMessage("House " + closestHouse.id + " entered." );
            player.TriggerEvent("enterHouse");
        }
        
        public void playerExitHouse(Player player)
        {
            if (!player.HasData("positionBeforeEnteringHouse"))
            {
                return;
            }
            player.Dimension = 0;
           
            player.Position = player.GetData<Vector3>("positionBeforeEnteringHouse");

            player.ResetData("positionBeforeEnteringHouse");
        }

      
    }
}
