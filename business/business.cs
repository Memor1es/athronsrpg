using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;

using MySql.Data.MySqlClient;
using rpg.Database;

namespace rpg
{
    public enum type
    {
        bank,
        gasStation,
        gunShop,
        repair,
        shop
    }


    public class business : Script
    {
        #region 
        public static List<string> businessDisplayNames = new List<string>()
        {
            "Bank",
            "Gas Station",
            "Gun Shop",
            "Repair",
            "24/7"
        };
        public static List<int> businessBlipList = new List<int>()
        {
            108,
            361, 
            110, 
            73, 
            52, 
        
        };
        #endregion
        public static float businessPercentReward  = 0.5f;
        public class model
        {
            public int bizId { get; set; }
            public type type { get; set; }
           
            public string owner { get; set; }
            public string name { get; set; }
            public Vector3 enterPos { get; set; }
            public Vector3 exitPos { get; set; }
            public int sale { get; set; }
            public int balance { get; set; }
            public dynamic external { get; set; }
        }
        public static List<model> businessList = new List<model>();
        public static void createBusiness( model business ) {
            NAPI.Task.Run( ( ) => {
                TextLabel text = NAPI.TextLabel.CreateTextLabel( $"[{businessDisplayNames[(int)business.type]} {business.name}] \n \n Owner: {business.owner} \n ID: {business.bizId} \n { (business.sale > 0 ? $"Price: {business.sale} \n /buybusiness" : "") }", business.enterPos, 20.0f, 0.45f, 4, new Color( 255, 255, 255 ), false, 0 );
                text.SetData( "bizid", business.bizId );

                Blip businessBlip = NAPI.Blip.CreateBlip( business.enterPos );
                NAPI.Blip.SetBlipName( businessBlip, $"{businessDisplayNames[ ( int ) business.type ]} {business.name}" );
                NAPI.Blip.SetBlipSprite( businessBlip, businessBlipList[(int)business.type]);
                NAPI.Blip.SetBlipShortRange( businessBlip, true );
                businessBlip.SetData( "bizid", business.bizId );
            } );
        }
        public static void updateBusinessLabel( model business )
        {
            NAPI.Task.Run( ( ) => {
                TextLabel text = NAPI.Pools.GetAllTextLabels().Find(a=>a.HasData("bizid") && a.GetData<int>("bizid") == business.bizId);
                if (text != null)
                {
                    text.Text = $"[{businessDisplayNames[ ( int ) business.type ]} {business.name}] \n \n Owner: {business.owner} \n ID: {business.bizId} \n { ( business.sale > 0 ? $"Price: {business.sale} \n /buybusiness" : "" ) }";
                }
                Blip blip = NAPI.Pools.GetAllBlips().Find(a=>a.HasData("bizid") && a.GetData<int>("bizid") == business.bizId);
                if ( blip != null )
                {
                    blip.Name = $"{businessDisplayNames[ ( int ) business.type ]} {business.name}";
                }


            } );
        }


        public static model getClosestBusiness(Player player )
        {
            model business = null; float distance = 5f;
            foreach ( var businessModel in businessList )
            {
                var dist = player.Position.DistanceTo( businessModel.enterPos );
                if ( dist < distance )
                {
                    business = businessModel;
                    distance = dist;
                }
               
            }
            return business;
        }
       

        public static List<model> getBusinessList(type type )
        {
            return businessList.FindAll( business => business.type == type );
        }
        [Command("bwithdraw")]
        public async  void onPlayerCommandBusinessWithdraw(Player player, int amount )
        {
            if ( await businessWithdraw(player, amount))
            {
                player.SendChatMessage( $"You withdrawed {amount} from your business."  );
            }
        }

        [Command( "buybusiness" )]
        public async void onPlayerBuyBusiness( Player player )
        {
            var business = getClosestBusiness(player);
            if ( business != null )
            {
                if (business.owner.ToLower() == player.Name.ToLower())
                {
                    player.SendChatMessage( "You can't buy your business." );
                    return;
                }
                if (business.sale > 0 )
                {
                    var money = player.GetSharedData<int>("money");
                    if (business.sale > money )
                    {
                        player.SendChatMessage( $"You don't enough money for this business. You need  ${business.sale - money} more." );
                        return;
                    }
                    var oldOwner = playerManager.getPlayer(business.owner);
                    if (oldOwner != null)
                    {
                        oldOwner.SendChatMessage( $"Your business was bought by {player.Name} for {business.sale}" );
                        await oldOwner.giveMoney( business.sale );
                    }
                    else
                    {
                       await moneyManager.giveMoneyDatabase( business.owner, business.sale );
                    }

                    await player.takeMoneyAsync( business.sale );
                    player.SendChatMessage( $"You sucessfully bought this business for {business.sale}" );
                    business.owner = player.Name;
                    business.sale = 0;
                    await databaseManager.updateQuery( $"UPDATE business SET owner = '{business.owner}', sale = '0' WHERE id = '{business.bizId}'" ).Execute( );
                 
                    updateBusinessLabel( business );
                }
                else
                {
                    player.SendChatMessage( "This business isn't for selling." );
                }
            }
            else
            {
                player.SendChatMessage( "You aren't near a business." );
            }
        }
        [Command( "sellbusiness" )]
        public async void onPlayerSellBusiness( Player player, int price )
        {
            var business = getClosestBusiness(player);
            if ( business != null )
            {
                if ( business.owner.ToLower( ) != player.Name.ToLower( ) )
                {
                    player.SendChatMessage( "You can't sell others's business." );
                    return;
                }
                if ( business.sale <= 0 )
                {
   
                    business.sale = price;
                    await databaseManager.updateQuery( $"UPDATE business SET sale = '{business.sale}' WHERE id = '{business.bizId}'" ).Execute( );

                    player.SendChatMessage( $"You marked this business for selling for {business.sale}" );


                    updateBusinessLabel( business );
                }
                else
                {
                    player.SendChatMessage( "This business is already selling." );
                }
            }
            else
            {
                player.SendChatMessage( "You aren't near a business." );
            }
        }
        [Command( "cancelsellbusiness" )]
        public async void onPlayerCancelSellBusiness( Player player, int price )
        {
            var business = getClosestBusiness(player);
            if ( business != null )
            {
                if ( business.owner.ToLower( ) != player.Name.ToLower( ) )
                {
                    player.SendChatMessage( "You can't cancel sell  others's business." );
                    return;
                }
                if ( business.sale > 0 )
                {

                    business.sale = 0;
                    await databaseManager.updateQuery( $"UPDATE business SET sale = '0' WHERE id = '{business.bizId}'" ).Execute( );

                    player.SendChatMessage( $"You are not selling your bussines anymore. " );


                    updateBusinessLabel( business );
                }
                else
                {
                    player.SendChatMessage( "This business isn't for sell." );
                }
            }
            else
            {
                player.SendChatMessage( "You aren't near a business." );
            }
        }
        [Command( "namebussiness", GreedyArg = true )]
        public async void onPlayerSellBusiness( Player player, string name )
        {
            var business = getClosestBusiness(player);
            if ( business != null )
            {
                if ( business.owner.ToLower( ) != player.Name.ToLower( ) )
                {
                    player.SendChatMessage( "You can't set others's business name." );
                    return;
                }
               

                    business.name = name;
                    await databaseManager.updateQuery( $"UPDATE business SET name = '{business.name}' WHERE id = '{business.bizId}'" ).Execute( );

                    player.SendChatMessage( $"You changed bussines name into {business.name}" );


                    updateBusinessLabel( business );
                
               
            }
            else
            {
                player.SendChatMessage( "You aren't near a business." );
            }
        }
        public static async Task<bool> businessWithdraw(Player player, int amount )
        {
            var business = getClosestBusiness(player);
            if (business != null )
            {
                if (business.owner.ToLower() != player.Name.ToLower())
                {
                    player.SendChatMessage( "This isn't your business." );
                    return false;
                }
                if ( business.balance > amount )
                {
                    await player.giveMoney(amount);
                    await utilities.takeBalance( business.bizId, amount );
                    return true;
                }

            }
            else
            {
                player.SendChatMessage( "There is no business." );
            }
            return false;
        }

        public class utilities
        {
            public static async Task addBalance( int bizId, int money )
            {
                var business = businessList.Find(a=> a.bizId == bizId);
                business.balance += ( int ) Math.Round( ( ( double ) businessPercentReward * ( double ) money ) / 100 );

                await databaseManager.updateQuery( $"UPDATE business SET balance = '{business.balance}' WHERE id = '{business.bizId}'" ).Execute( );

            }

            public static async Task takeBalance( int bizId, int money )
            {
                var business = businessList.Find(a=> a.bizId == bizId);
                business.balance -= money;

                await databaseManager.updateQuery( $"UPDATE business SET balance = '{business.balance}' WHERE id = '{business.bizId}'" ).Execute( );

            }

            public static async Task<bool> withdrawBalance( Player player, int bizId, int money )
            {
                
                var business = businessList.Find(a=> a.bizId == bizId);
                if ( business.balance < money )
                    return false;
                business.balance -= money;
                
                await player.giveMoney( money );

                await databaseManager.updateQuery( $"UPDATE business SET balance = '{business.balance}' WHERE id = '{business.bizId}'" ).Execute( );
                return true;
            }


        }
        public class logs
        {

        }
    }
}




