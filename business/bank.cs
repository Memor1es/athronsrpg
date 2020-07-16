using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{
    public class bank : Script
    {
        
        public class atm
        {
            public Vector3 position { get; set; }
            
            public atm( ) { }
            public atm( Vector3 position)
            {
                this.position = position;
      
            }
        }
        public class counter
        {
            public Vector3 pedPosition { get; set; }
            public float pedRotation { get; set; }
            public Vector3 textLabel { get; set; }
            public counter( ) { }
            public counter(Vector3 pedPosition, float pedRotation )
            {
                this.pedPosition = pedPosition;
                this.pedRotation = pedRotation;
            }
        }
        public class external
        {
            public List<atm> atms { get; set; }
            public List<counter> counters { get; set; }
        } 
       
        [ServerEvent(Event.ResourceStart)]
        public async void onStart( )
        {
            await databaseManager.selectQuery( $"SELECT * FROM business WHERE type = '{(int)type.bank}'", ( DbDataReader reader ) =>
            {
                business.model bank = new business.model(); ;
                bank.owner = ( string ) reader[ "owner" ];
                bank.enterPos = ( Vector3 ) NAPI.Util.FromJson<Vector3>( reader[ "enterPos" ] );
                bank.sale = ( int ) reader[ "sale" ];
                bank.external = NAPI.Util.FromJson<external>( ( string ) reader[ "external" ] );
                bank.name = ( string ) reader[ "name" ];
                bank.bizId = ( int ) reader[ "id" ];
                bank.balance = ( int ) reader[ "balance" ];

                business.businessList.Add( bank );
            } ).Execute( );
           NAPI.Task.Run( ( ) =>
            {
                foreach ( var bank in business.getBusinessList(type.bank) )
                {
                    business.createBusiness( bank );

                    for ( int i = 0; i < bank.external.counters.Count; i++ )
                    {
                        var counter = bank.external.counters[i];
                        NAPI.Ped.CreatePed( NAPI.Util.GetHashKey( "u_m_m_aldinapoli" ), counter.pedPosition, counter.pedRotation, false, true, true, true );
                        TextLabel textLabel = NAPI.TextLabel.CreateTextLabel( $"[Counter {i}]]", counter.textLabel, 30.0f, 0.75f, 4, new Color( 255, 255, 255 ), false, 0 );
                        ColShape bankShape = NAPI.ColShape.CreateSphereColShape(bank.enterPos, 40);
                        bankShape.SetData( "bank", true );
                        bankShape.SetData( "bizid", bank.bizId);
                    }
                    foreach (  var atm in bank.external.atms )
                    {
                        TextLabel textATM = NAPI.TextLabel.CreateTextLabel( $"[ATM {bank.name}]  \n Owner: {bank.owner} \n ", atm.position, 30.0f, 0.75f, 4, new Color( 255, 255, 255 ), false, 0 );
                        textATM.SetSharedData( "atmData", bank.bizId );
                        NAPI.Marker.CreateMarker(27, new Vector3( atm.position.X, atm.position.Y, atm.position.Z - 0.90f ), new Vector3( 0, 0, 0 ), new Vector3( 0, 0, 0 ), 0.6f, 140, 4, 36, false, 0 );
                    }  
                  
                 
                }
            } );
          
        }
        [ServerEvent( Event.PlayerEnterColshape )]
        public void onPlayerEntercolShape( ColShape shape, Player player )
        {
            if ( shape.HasData( "bank" ) && shape.HasData( "bizid" ) )
            {
                player.SetData( "enteredBank", shape.GetData<int>( "bizid" ) );

            }
        }
        [Command( "openatm" )]
        public void onPlayerOpenAtm( Player player )
        {
            player.TriggerEvent( "playerOpenATM", player.GetData<int>( "bankMoney" ), player.GetData<int>("bankPin"));
        }
        [RemoteEvent( "onPlayerTriggerOpenATM" )]
        public void onPlayerKeyOpenATM(Player player )
        {
            player.TriggerEvent( "playerOpenATM", player.GetData<int>( "bankMoney" ), player.GetData<int>( "bankPin" ) );
        }
        [Command("addatm")]
        public void onPlayerAddAtm(Player player)
        {
            if (!player.HasData( "inBankCreator" )){ player.SendChatMessage( "Nu esti in creearea unei banci." ); return; }

            var biz = player.GetData<business.model>("inBankCreator");
            if (biz == null )
            {
                player.SendChatMessage( "That bank doesn't exist." );
                return;
            }
            biz.external.atms.Add( new atm( player.Position ) );
            player.SetData( "inBankCreator", biz );
            //            await databaseManager.updateQuery( $"UPDATE business SET external = {NAPI.Util.ToJson(biz.external)}" ).Execute( );

        }
        [Command("addcounter")]
        public void onPlayerAddCounter(Player player )
        {
            if ( !player.HasData( "inBankCreator" )){ player.SendChatMessage( "Nu esti in creearea unei banci." ); return; }

            var biz = player.GetData<business.model>("inBankCreator");
            if ( biz == null )
            {
                player.SendChatMessage( "That bank doesn't exist." );
                return;
            }
            var counter = new counter( );
            counter.textLabel = player.Position;
            player.SetData( "counterCreator", counter );
            player.SendChatMessage( "a fost create counteru si sa pus textlabelu acm pune setcounterped acolo unde vrei sa fie pedu" );

        }
        [Command("setcounterped")]
        public void onPlayerSetCounterPed(Player player )
        {
            if ( !player.HasData( "inBankCreator" )){ player.SendChatMessage( "Nu esti in creearea unei banci." ); return; }

            var biz = player.GetData<business.model>("inBankCreator");
            if ( biz == null )
            {
                player.SendChatMessage( "That bank doesn't exist." );
                return;
            }
            if ( player.HasData( "counterCreator" ) )
            {
                var counter = player.GetData<counter>("counterCreator");
                counter.pedPosition = player.Position; counter.pedRotation = player.Heading;
                biz.external.counters.Add(counter );
                player.SetData( "inBankCreator", biz );
                player.ResetData( "counterCreator" );
                player.SendChatMessage( "A fost adaugat counterul cu success." );
            }
        }
        
        
        [Command("createbank")]
        public  void createBank(Player player, string name )
        {
            var bank = new business.model();
            bank.enterPos = player.Position;
            bank.owner = "AdmBot";
            bank.name = name;
            bank.external = new external( );
            bank.external.atms = new List<atm>( );
            bank.external.counters = new List<counter>( );
            player.SetData( "inBankCreator", bank );
        }
        [Command("bankfinish")]
        public async void onPlayerFinishBank(Player player )
        {
            if ( !player.HasData( "inBankCreator" )){ player.SendChatMessage( "Nu esti in creearea unei banci." ); return; }

            var biz = player.GetData<business.model>("inBankCreator");
            if ( biz == null )
            {
                player.SendChatMessage( "That bank doesn't exist." );
                return;
            }
            await databaseManager.updateQuery( $"INSERT INTO business (owner, name, type, enterPos, external) VALUES ('{biz.owner}', '{biz.name}', 'BANK', '{NAPI.Util.ToJson( biz.enterPos )}', '{NAPI.Util.ToJson( biz.external )}')" ).Execute( );
        }
       

    

       public static counter getClosestCounter(Player player )
        {
          

            var enteredBank = business.businessList.Find(a=> a.bizId == player.GetData<int>("enteredBank"));
            var counterResult =  new counter();

            float distance = 2.5f;
            foreach ( var counter in enteredBank.external.counters )
            {
                float dist = player.Position.DistanceTo( counter.textLabel );
                if ( dist < distance )
                {
                    counterResult = counter;
                    distance = dist;
                }
                
            }
            return counterResult;
        }
        [Command("withdraw")]
        public async  void onCommandWithdraw(Player player, int amount )
        {
            if ( canPerformBankTask( player ) )
            {

                if ( await player.bankWithdrawMoney( amount, player.GetData<int>( "enteredBank" ), false ) )
                    player.SendChatMessage( "You withdrawed " + amount );
                else
                    player.SendChatMessage( "You don't have enough money in bank balance." );
            }
        }
        [Command( "deposit" )]
        public async void onCommandDeposit( Player player, int amount)
        {
            if ( canPerformBankTask( player ) )
            {
                if ( await player.bankDepositMoney( amount,  player.GetData<int>( "enteredBank"), false) )  
                    player.SendChatMessage( "You deposited " + amount );
                else
                    player.SendChatMessage( "You don't enough money." );
            }
        }

        public bool canPerformBankTask(Player player )
        {
            if ( player.HasData( "enteredBank" ) )
            {
                var counter = getClosestCounter(player);
                if ( player.Position.DistanceTo( counter.textLabel ) < 2.5f )
                    return true;
            }
            return false;
        }

        [RemoteEvent( "playerWithdrawMoney" )]
        public async void onPlayerWithDraw( Player player, int amount, int bizID )
        {
            Console.WriteLine( "Withdraw " + amount );
       
            if (await player.bankWithdrawMoney(amount, bizID))
                player.TriggerEvent( "resultWithdraw", true );
           else
                player.TriggerEvent( "resultWithdraw", false );


        }

        [RemoteEvent( "playerDepositMoney" )]
        public async void onPlayerDeposit( Player player, int amount, int bizID )
        {
            Console.WriteLine( "Deposit " + amount );
            if ( await player.bankDepositMoney( amount, bizID ) )
                player.TriggerEvent( "resultDeposit", true );
            else
                player.TriggerEvent( "resultDeposit", false );


        }

       
    }

}

