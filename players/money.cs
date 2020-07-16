using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{
   public static class moneyManager 
    {
        public static async Task<bool> takeMoneyAsync(this Player player, int money)
        {
            if (player.HasSharedData("money") && player.GetSharedData<int>("money") > money)
            {
                player.SetSharedData("money", player.GetSharedData<int>("money") - money);
                await databaseManager.updateQuery($"UPDATE accounts SET money = '{player.GetSharedData<int>("money")}' WHERE username = '{player.Name}'").Execute();
                return true;
            }
            
            return false;
        }

        public static async Task<bool> giveMoneyDatabase( string name, int money )
        {
            await databaseManager.updateQuery( $"UPDATE accounts SET money = money + '{money}' WHERE username = '{name}'" ).Execute( );
            return true;
        }
        public static async Task<bool> takeMoneyDatabase( string name, int money )
        {
            await databaseManager.updateQuery( $"UPDATE accounts SET money = money - '{money}' WHERE username = '{name}'" ).Execute( );
            return true;
        }
        public static async Task giveMoney(this Player player, int money)
        {
            if (player.HasSharedData("money"))
            {
                player.SetSharedData("money", player.GetSharedData<int>("money") + money);
                await databaseManager.updateQuery($"UPDATE accounts SET money = '{player.GetSharedData<int>("money")}' WHERE username = '{player.Name}'").Execute();
            }
        }
        public static async Task<bool> bankWithdrawMoney( this Player player, int money, int bizId, bool browser = true )
        {
            if ( player.HasSharedData( "money" ) )
            {
                int bankMoney = player.HasData("bankMoney") ? player.GetData<int>("bankMoney") : 0;
                if ( bankMoney > money )
                {

                    bankMoney -= money; int playerMoney = player.GetSharedData<int>( "money" );

                    playerMoney += money;
                 
                    player.SetData( "bankMoney", bankMoney );
                    player.SetSharedData( "money", playerMoney );
                    await databaseManager.updateQuery( $"UPDATE accounts SET money = '{playerMoney}', bankMoney = '{bankMoney}' WHERE username = '{player.Name}'" ).Execute( );
                  if ( browser )
                    player.TriggerEvent( "updateBankBalance", bankMoney );
                    await business.utilities.addBalance( bizId, money );
                    return true;
                }      
            }
            return false;
        }
        public static async Task<bool> bankDepositMoney( this Player player, int money, int bizId, bool browser = true )
        {
            if ( player.HasSharedData( "money" ) )
            {
                int bankMoney = player.HasData("bankMoney") ? player.GetData<int>("bankMoney") : 0;
              
               int playerMoney = player.GetSharedData<int>( "money" );
  
                    playerMoney -= money;
                    bankMoney += money;
                    player.SetData( "bankMoney", bankMoney );
                    player.SetSharedData( "money", playerMoney );
                    await databaseManager.updateQuery( $"UPDATE accounts SET money = '{playerMoney}', bankMoney = '{bankMoney}' WHERE username = '{player.Name}'" ).Execute( );
                await business.utilities.addBalance( bizId, money );
                if (browser)  
                player.TriggerEvent( "updateBankBalance", bankMoney );
                return true;
                
            }
            return false;
        }
     
       

          public static async Task<bool> bankGiveMoney( this Player player, int money, string username = null )
        {
            if ( player == null || username != null )
            {
                await databaseManager.updateQuery( $"UPDATE accounts SET bankMoney = bankMoney + '{money}' WHERE username = '{username}'" ).Execute( );
            }
            else
            {


                if ( player.HasSharedData( "money" ) )
                {
                    int bankMoney = player.HasData("bankMoney") ? player.GetData<int>("bankMoney") : 0;

                    bankMoney += money;
                    player.SetData( "bankMoney", bankMoney );
                    await databaseManager.updateQuery( $"UPDATE accounts SET bankMoney = '{bankMoney}' WHERE username = '{player.Name}'" ).Execute( );

                    return true;

                }
            }
            
            return false;
        }

    }
}
