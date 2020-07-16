using GTANetworkAPI;
using rpg.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Timers;

namespace rpg {
    class policeDepartment : Script {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            Timer timer_Jail = new Timer();
            timer_Jail.Interval = 3000;
            timer_Jail.Elapsed += jailTimerAsync;
           // timer_Jail.Start();

            NAPI.Task.Run(() =>
           {
       //        NAPI.TextLabel.CreateTextLabel("~b~Faction: ~w~1\n ~b~Name: ~w~Los Santos Police Departament\n ~b~HQ Status: ~w~(unlocked)", new Vector3(433.24393, -982.09484, 30.710026), 25, 1, 4, new Color(0, 0, 0));
           });
        }

        public static Player getPlayer(string targetString)
        {
            return int.TryParse(targetString, out int targetId) ? NAPI.Pools.GetAllPlayers().Find(player => player.Value == targetId) : NAPI.Pools.GetAllPlayers().Find(player => player.Name.ToLower() == targetString.ToLower());
        }

        public void sendDispatchMessage(string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(player => player.GetSharedData<int>("faction") == 1).ForEach(player => { player.SendChatMessage($"{message}"); });
        }
       
        public static string getRankName( int name ) {
            string toReturn = "";

            switch ( name ) {
                case 1:
                toReturn = " LS Officer";
                break;
                case 2:
                toReturn = " LS Inspector";
                break;
                case 3:
                toReturn = " LS Commissioner";
                break;
                case 4:
                toReturn = " LS General";
                break;
                case 5:
                toReturn = " LS Captain";
                break;
                case 6:
                toReturn = "* LS Quaestor";
                break;
                case 7:
                toReturn = "** LS Chief-Quaestor";
                break;
            }

            return toReturn;
        }

        [ServerEvent( Event.PlayerSpawn )]
        public async void onPlayerSpawn( Player player ) {

            if ( player.HasData( "jailTime" ) && player.GetData<int>( "jailTime" ) > 0 ) {
                List<mdc> mdc = getPlayerMdc( player.Name ).Result;
                player.SendChatMessage( $"~w~-- MDC [ID {player.Value}] - {player.Name}] [~y~W:{player.GetSharedData<int>( "wantedLevel" )}~w~] [Wanted expires in {player.GetData<int>( "jailTime" )} seconds]" );
                foreach ( mdc _mdc in mdc )
                    player.SendChatMessage( $"~w~-- {_mdc.wantedReason} - reporter: {_mdc.cop}" );

                player.SetData( "jailTime", player.GetSharedData<int>( "wantedLevel" ) * 200 );
                player.SetSharedData( "wantedLevel", 0 );
                player.SetData( "date", DateTime.Now );
                putPlayerInJail( player, player.GetData<int>( "jailTime" ) );

                await databaseManager.updateQuery( $"UPDATE accounts SET jailTime = {player.GetData<int>( "jailTime" )}" ).Execute( );
            }
        }

        public static void copDuty(Player player)
        {
            if (player.HasData("positionBeforeEnteringHouse") || player.Position.DistanceTo(new Vector3(446.03003, -985.2313, 30.689583)) < 10)
            {
                if (player.HasSharedData("inDuty") && player.GetSharedData<bool>("inDuty"))
                {
                    player.SendChatMessage($"{"!{#afa1c4}"}*{getRankName(player.GetData<int>("factionRank"))} {player.Name} ({player.Value}) places his badge and gun in his locker.");
                    player.SetSharedData("inDuty", false);
                    return;
                }

                player.SendChatMessage($"{"!{#afa1c4}"}*{getRankName(player.GetData<int>("factionRank"))} {player.Name} ({player.Value}) took a badge and a gun from his locker.");
                player.GiveWeapon(WeaponHash.Nightstick, 1);
                player.GiveWeapon(WeaponHash.Revolver_mk2, 500);
                player.GiveWeapon(WeaponHash.Smg, 1000);
                player.GiveWeapon(WeaponHash.Carbinerifle, 1000);
                player.GiveWeapon(WeaponHash.Bzgas, 25);
                player.Armor = 100;

                player.SetAccessories(0, 46, 0); //cascheta
                player.SetClothes(11, 55, 0);
                player.SetClothes(4, 31, 0);
                player.SetClothes(6, 25, 0);
                player.SetClothes(9, 15, 2);

                player.SetSharedData("inDuty", true);
                return;
            }
        }

        [Command( "r", "!{#CECECE}Syntax: ~w~/r [message]", GreedyArg = true )]
        public void onR( Player player, string message ) {

            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "!{#CECECE}Your group data is invalid." );
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }

            NAPI.Pools.GetAllPlayers( ).FindAll( target => target.HasSharedData("faction") && target.GetSharedData<int>( "faction" ) == 1 ).ForEach( target => {
                target.SendChatMessage( $"{"!{#8D8DFF}"}*{getRankName( target.GetData<int>( "factionRank" ) )} {player.Name} ({player.Value}): {message}, over." );
            } );
        }

        [Command("d", "!{#CECECE}Syntax: ~w~/d [message]", GreedyArg = true)]
        public void onD(Player player, string message)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("!{#CECECE}Your group data is invalid.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }

            NAPI.Pools.GetAllPlayers().FindAll(target => target.HasSharedData("faction") && target.GetSharedData<int>("faction") == 1).ForEach(target =>
            {
                target.SendChatMessage($"{"!{#FF3535}"}*{getRankName(target.GetData<int>("factionRank"))} {player.Name} ({player.Value}): {message}, over.");
            });
        }

        [Command( "su", "!{#CECECE}Syntax: ~w~/su [name/playerid] [level] [reason]", GreedyArg = true )]
        public async void onSu( Player player, string username, int wantedLevel, string wantedReason ) {
            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "!{#CECECE}You are not a cop." );
                return;
            }

            if ( player.HasSharedData( "inDuty" ) && !player.GetSharedData<bool>( "inDuty" ) ) {
                player.SendChatMessage( "!{#CECECE}You are not on duty." );
                return;
            }

            if ( wantedLevel > 6 ) {
                player.SendChatMessage( "!{#CECECE}Invalid wanted level (1-6)." );
                return;
            }

            Player target = getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            } else if ( target.HasSharedData( "faction" ) && target.GetSharedData<int>( "faction" ) == 1 ) {
                player.SendChatMessage( "!{#CECECE}The target is a cop." );
                return;
            }

            await givePlayerWanted( target, player, wantedLevel, wantedReason );

            if ( target.GetSharedData<int>( "wantedLevel" ) > 6 )
                target.SetSharedData( "wantedLevel", 6 );

            if ( player.HasSharedData( "inDuty" ) && player.GetSharedData<bool>( "inDuty" ) )
                sendDispatchMessage( $"~b~Dispatch: {target.Name} ({target.Value}) has committed a crime: {wantedReason}. Reporter: {player.Name}[{player.Value}]. W: +{wantedLevel}. New wanted level: {target.GetSharedData<int>( "wantedLevel" )}." );
        }

        [Command( "ta" )]
        public void onTa( Player player ) {
            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "!{#CECECE}You are not a cop." );
                return;
            }

            if ( player.HasSharedData( "inDuty" ) && !player.GetSharedData<bool>( "inDuty" ) ) {
                player.SendChatMessage( "!{#CECECE}You are not on duty." );
                return;
            }

            if ( player.HasData( "factionRank" ) && player.GetData<int>( "factionRank" ) < 2 ) {
                player.SendChatMessage( "!{#CECECE}You need a higher rank to equip a taser." );
                return;
            }

            player.GiveWeapon( WeaponHash.Stungun, 1 );
        }

        [ServerEvent( Event.PlayerWeaponSwitch )]
        public void onPlayerWeaponSwitch( Player player, WeaponHash oldWeapon, WeaponHash newWeapon ) {
            player.TriggerEvent("playerWeaponChange", oldWeapon, newWeapon);
            if ( newWeapon == WeaponHash.Stungun ) {
                foreach ( Player players in NAPI.Player.GetPlayersInRadiusOfPlayer( 30, player ) )
                    players.SendChatMessage( $"{"!{#afa1c4}"}{player.Name} ({player.Value}) equipped a taser." );
            }
        }

        [Command( "arrest", "!{#CECECE}Syntax: ~w~/arrest [name/playerid]" )]
        public async void onArrest( Player player, string username ) {
            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "!{#CECECE}You are not a cop." );
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player target = getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            } else if ( target.HasSharedData( "faction" ) && target.GetSharedData<int>( "faction" ) == 1 ) {
                player.SendChatMessage( "!{#CECECE}The target is a cop." );
                return;
            }

            if ( player.Position.DistanceTo( new Vector3( 478.14014, -1018.38727, 27.969374 ) ) > 5 ) {
                player.SendChatMessage( "!{#CECECE}You are not near the arrest point." );
                return;
            }

            if ( target.HasSharedData( "wantedLevel" ) && target.GetSharedData<int>( "wantedLevel" ) > 0 && target.HasSharedData( "playerCuffed" ) && target.GetSharedData<bool>( "playerCuffed" ) ) {
                await databaseManager.updateQuery( $"DELETE FROM mdc WHERE suspect = '{target.Name}'" ).Execute( );

                target.SetData( "jailTime", target.GetSharedData<int>( "wantedLevel" ) * 200 );
                target.SetSharedData( "money", target.GetSharedData<int>( "money" ) - target.GetSharedData<int>( "wantedLevel" ) * 600 );
                target.SendChatMessage($"{"!{#FF6347}"}You have been arrested by {player.Name} for {target.GetData<int>( "jailTime" )} seconds, and issued a fine of ${target.GetSharedData<int>("wantedLevel") * 600}." );

                NAPI.Chat.SendChatMessageToAll($"{"!{#FF6347}"}{player.Name} ({player.Value}) arrested {target.Name} ({target.Value}), issuing a fine of ${target.GetSharedData<int>("wantedLevel") * 600} with a sentence of {target.GetData<int>("jailTime")} seconds.");

                await databaseManager.updateQuery( $"UPDATE accounts SET money = '{target.GetSharedData<int>( "money" )}' WHERE username = '{target.Name}' LIMIT 1" ).Execute( );

                target.SetSharedData( "wantedLevel", 0 );
                target.SetData( "playerHasWanted", false );
                target.RemoveAllWeapons( );
                target.ClearAttachments( );
                putPlayerInJail( target, target.GetData<int>( "jailTime" ) );

                await databaseManager.updateQuery($"UPDATE accounts SET wantedLevel = '0' WHERE username = '{target.Name}'").Execute();

                await databaseManager.updateQuery( $"UPDATE accounts SET jailTime = '{target.GetData<int>( "jailTime" )}' WHERE username = '{target.Name}'" ).Execute( );
            }
        }

        [Command("cuff", "!{#CECECE}Syntax: ~w~/cuff [name/playerid]")]
        public void onCuff(Player player, string username)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("!{#CECECE}You are not a cop.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player target = getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
            else if (target.HasSharedData("faction") && target.GetSharedData<int>("faction") == 1)
            {
                player.SendChatMessage("!{#CECECE}You can't cuff cops.");
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 5)
            {
                player.SendChatMessage("!{#CECECE}The player is not near you.");
                return;
            }

            if (target.HasSharedData("wantedLevel") && target.GetSharedData<int>("wantedLevel") > 0)
            {
                if (target.HasData("playerHasHandsup") && !target.GetData<bool>("playerHasHandsup"))
                {
                    player.SendChatMessage("!{#CECECE}The player must be subdued.");
                    return;
                }

                if (target.HasSharedData("playerCuffed") && target.GetSharedData<bool>("playerCuffed"))
                    target.SetSharedData("playerCuffed", false);
                else
                    target.SetSharedData("playerCuffed", true);

                if (!target.IsInVehicle)
                {
                    target.AddAttachment("addCuffs", false);
                    target.PlayAnimation("mp_arresting", "idle", 49);
                }
                target.RemoveAllWeapons();

                target.SendChatMessage($"{"!{#afa1c4}"}You were cuffed by {player.Name} ({player.Value})");
                player.SendChatMessage($"{"!{#afa1c4}"}You cuffed {target.Name} ({target.Value})");
            }
        }

        [Command( "uncuff", "!{#CECECE}Syntax: ~w~/uncuff [name/playerid]" )]
        public void onUncuff( Player player, string username ) {
            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "!{#CECECE}You are not a cop." );
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player target = getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            if ( target.HasSharedData( "playerCuffed" ) && target.GetSharedData<bool>( "playerCuffed" ) ) {
                target.SetSharedData( "playerCuffed", false );
                target.StopAnimation( );
                target.ClearAttachments( );

                foreach ( Player players in NAPI.Player.GetPlayersInRadiusOfPlayer( 30, player ) )
                    players.SendChatMessage( $"{"!{#afa1c4}"}{target.Name} ({target.Value}) was cuffed by {player.Name} ({player.Value})" );

                target.SendChatMessage( $"{"!{#afa1c4}"}You were uncuffed by {player.Name} ({player.Value})" );
                player.SendChatMessage( $"{"!{#afa1c4}"}You uncuffed {player.Name} ({player.Value})" );
            }
        }

        [Command( "handsup" )]
        public void onHandsup( Player player ) {
            if ( player.HasSharedData( "playerCuffed" ) && player.GetSharedData<bool>( "playerCuffed" ) || player.HasData( "playerHasHandsup") && player.GetData<bool>( "playerHasHandsup") ) {
                player.StopAnimation( );
                player.SetData<bool>( "playerHasHandsup", false );
            } else {
              
                player.PlayAnimation( "random@mugging3", "handsup_standing_base", 49 );
                player.SetData<bool>( "playerHasHandsup", true );
            }
        }

        [Command( "so", "!{#CECECE}Syntax: ~w~/so [name/playerid]" )]
        public void onSo( Player player, string username ) {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("!{#CECECE}You are not a cop.");
                return;
            }

            if ( player.HasSharedData( "inDuty" ) && !player.GetSharedData<bool>( "inDuty" ) ) {
                player.SendChatMessage( "!{#CECECE}You are not on duty." );
                return;
            }

            Player target = getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            foreach ( Player players in NAPI.Player.GetPlayersInRadiusOfPlayer( 20, player ) ) {
                if ( target.IsInVehicle )
                    players.SendChatMessage( $"~y~(megaphone) {getRankName( player.GetData<int>( "factionRank" ) )} {player.Name} ({player.Value}): ~b~{target.Name} ({target.Value}), ~y~you are chased by the police! Pull over!" );
                else
                    players.SendChatMessage( $"~y~(megaphone) {getRankName( player.GetData<int>( "factionRank" ) )} {player.Name} ({player.Value}): ~b~{target.Name} ({target.Value}), ~y~you are chased by the police! Do you surrender?" );
            }
        }

        [Command( "mdc", "!{#CECECE}Syntax: ~w~/mdc [name/playerid]" )]
        public void onMdc( Player player, string username ) {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("!{#CECECE}You are not a cop.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player target = playerManager.getPlayer( username );
            if ( target == null ) {
                player.SendChatMessage( "!{#CECECE}The specified player ID is either not connected or has not authenticated." );
                return;
            }

            List<mdc> mdc = getPlayerMdc( target.Name ).Result;

            if ( target.HasSharedData( "wantedLevel" ) && target.GetSharedData<int>( "wantedLevel" ) > 0 ) {
                player.SendChatMessage( $"~w~-- MDC [ID {target.Value}] - {target.Name}] [~y~W:{target.GetSharedData<int>( "wantedLevel" )}~w~] [Wanted expires in {target.GetData<int>( "jailTime" )} seconds]" );
                foreach ( mdc _mdc in mdc )
                    player.SendChatMessage( $"~w~-- {_mdc.wantedReason} - reporter: {_mdc.cop}" );
            } else
                player.SendChatMessage( $"~w~-- MDC [ID {target.Value} - {target.Name}] [not wanted] --" );
        }

        [Command( "mymdc" )]
        public void onMyMdc( Player player ) {
            List<mdc> mdc = getPlayerMdc( player.Name ).Result;

            if ( player.HasSharedData( "wantedLevel" ) && player.GetSharedData<int>( "wantedLevel" ) > 0 ) {
                player.SendChatMessage( $"~w~-- MDC [ID {player.Value}] - {player.Name}] [~y~W:{player.GetSharedData<int>( "wantedLevel" )}~w~] [Wanted expires in {player.GetData<int>( "jailTime" )} seconds]" );
                foreach ( mdc _mdc in mdc )
                    player.SendChatMessage( $"~w~-- {_mdc.wantedReason} - reporter: {_mdc.cop}" );
            } else
                player.SendChatMessage( $"~w~-- MDC [ID {player.Value} - {player.Name}] [not wanted] --" );
        }

        [Command( "clear", "!{#CECECE}Syntax: ~w~/clear [name/playerid]" )]
        public async void clear( Player player, string username ) {
            if ( player.HasSharedData( "faction" ) && player.GetSharedData<int>( "faction" ) != 1 ) {
                player.SendChatMessage( "You are not a cop." );
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player target = playerManager.getPlayer( username );
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            player.SendChatMessage( $"{"!{#CECECE}"}You cleared {target.Name}'s wanted." );
            target.SendChatMessage( $"{"!{#CECECE}"}Your wanted level was cleared." );

            target.SetSharedData( "wantedLevel", 0 );
            await databaseManager.updateQuery( $"UPDATE accounts SET wanted = '0' WHERE username = '{target.Name}'" ).Execute( );
            await databaseManager.updateQuery( $"DELETE FROM mdc WHERE suspect = '{target.Name}'" ).Execute( );
        }
        [Command("radar")]
        public void onRadar(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("You are not a cop.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            player.TriggerEvent("onStartRadar");
        }
        [RemoteEvent("startRadar")]
        public void onStartRadar(Player player, string streetName, string zoneName)
        {
            player.GiveWeapon(WeaponHash.Marksmanpistol, 0);
            player.SetSharedData("inRadar", true);
            NAPI.Pools.GetAllPlayers().FindAll(target => target.HasSharedData("faction") && target.GetSharedData<int>("faction") == 1).ForEach(target =>
            {
                target.SendChatMessage($"{"!{#FF3535}"}*{getRankName(target.GetData<int>("factionRank"))} {player.Name} ({player.Value}) stationated a speeding radar in {zoneName} on {streetName}.");
            });
        }
        [Command("confiscate", Alias ="cl")]
        public async void confiscateLicense(Player player, string target)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("You are not a cop.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player targetPlayer = playerManager.getPlayer(target);
            if (targetPlayer == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
            if (await targetPlayer.removeLicense(mainLicense.license.vehicle, player))
            {
                targetPlayer.SendChatMessage("Ti-a fost confiscata licenta de masina.");
                player.SendChatMessage("iai confiscat lceitana de masina lui " + targetPlayer.Name);
            }
            else
            {
                player.SendChatMessage("nu are licenta de condus.");
            }

        }
        [Command("ticket")]
        public void onCopTicker(Player player, string target, int amount )
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("You are not a cop.");
                return;
            }
            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }
            Player targetPlayer = playerManager.getPlayer(target);
            if (targetPlayer == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
            if (player.Position.DistanceTo(targetPlayer.Position)  > 3)
            {
                player.SendChatMessage("!{#CECECE}You are not near the player.");
                return;
            }
            if (targetPlayer.HasData("needToPayTicket"))
            {
                targetPlayer.SendChatMessage($"{"!{#CECECE}"} The player needs first to pay the active ticket.");

                return;
            }
            targetPlayer.SetData("needToPayTicket", amount); targetPlayer.SetData("needToPayTicketId", player.Value);
            targetPlayer.SendChatMessage($"{"!{#CECECE}"} You need to pay {amount} to {player.Name}. Use /accept ticket to pay it.");
            
        }
        [Command("cancelradar")]
        public void onCancelRadar(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("You are not a cop.");
                return;
            }
            NAPI.Player.RemovePlayerWeapon(player, WeaponHash.Marksmanpistol);

            player.SetSharedData("inRadar", false);
        }
        [RemoteEvent("onCopLeaveRadar")]
        public void onCopLeaveRadar(Player player)
        {
            NAPI.Player.RemovePlayerWeapon(player, WeaponHash.Marksmanpistol);

            player.SetSharedData("inRadar", false);
        }
        [RemoteEvent("onCopSendPlayerSpeed")]
        public void onCopSendPlayerSpeed(Player player, Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Controller != null)
            {
                vehicle.Controller.SendChatMessage("[RADAR] You exceeded the speed limit.");
            }
        }
        [Command("fvrespawn")]
        public void onFvrespawn(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") != 1)
            {
                player.SendChatMessage("You are not a cop.");
                return;
            }

            if (player.HasData("factionRank") && player.GetData<int>("factionRank") <= 5)
            {
                player.SendChatMessage("!{#CECECE}You need a higher rank to use this command.");
                return;
            }

            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                if (veh.HasData("type") && veh.GetData<int>("type") == 6)
                    veh.Delete();
            }
        }

        static async void jailTimerAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (Player player in NAPI.Pools.GetAllPlayers())
                {
                    if (player.HasData("jailTime") && player.GetData<int>("jailTime") > 0 && player.HasData("date"))
                    {//Convert.ToDateTime
                        DateTime last = player.GetData<DateTime>("date");
                        int jail_time = player.GetData<int>("jailTime") - (DateTime.Now - last).Minutes;

                        player.SetData("jailTime", jail_time);
                        //  player.SendChatMessage($"you have remaining time {jail_time} lapsed {(DateTime.Now - last).Minutes} joined time {last}");
                        if (jail_time <= 0)
                        {
                            player.Position = new Vector3(463.9106, -993.8487, 24.91488).Around(1);
                            player.SendChatMessage("Ai fost eliberat deoarece ti-ai executat sentinta.");
                            player.SetData("jailTime", 0);



                            await databaseManager.updateQuery($"UPDATE accounts SET jailTime = '0' WHERE username = '{player.Name}'").Execute();
                        }


                    }
                    else if (player.HasSharedData("wantedLevel") && player.GetSharedData<int>("wantedLevel") > 0)
                    {
                        DateTime last = player.GetData<DateTime>("date");
                        if ((DateTime.Now - last).Minutes > 10 && player.GetSharedData<int>("wantedLevel") > 0)
                        {
                            int wantedLevel = player.GetSharedData<int>("wantedLevel") - 1;
                            player.SetSharedData("wantedLevel", wantedLevel);
                            player.SetData("date", DateTime.Now);
                            player.SendChatMessage($"~r~Your wanted level was decreased to { wantedLevel }.");
                            await databaseManager.updateQuery($"UPDATE accounts SET wantedLevel = '{wantedLevel}' WHERE username = '{player.Name}'").Execute();
                        }
                        if (player.GetSharedData<int>("wantedLevel") <= 0)
                        {
                            await databaseManager.updateQuery($"DELETE FROM mdc WHERE suspect = '{player.Name}'").Execute();
                            player.SendChatMessage("~w~Your wanted level was removed because you were not caught by the police.");
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception);
            }
        
        }

        public class mdc {
            public int id { get; set; }
            public string cop { get; set; }
            public string suspect { get; set; }
            public int wantedLevel { get; set; }
            public string wantedReason { get; set; }

            public mdc( ) { }

            public mdc( int id, string cop, string suspect, int wantedLevel, string wantedReason ) {
                this.id = id;
                this.cop = cop;
                this.suspect = suspect;
                this.wantedLevel = wantedLevel;
                this.wantedReason = wantedReason;
            }
        }

        public static async Task givePlayerWanted( Player suspect, Player player  , int wantedLevel, string wantedReason, bool server  =false) {
            mdc mdc = new mdc( );
            mdc.cop = !server ?player.Name : "unknown";
            mdc.suspect = suspect.Name;
            mdc.wantedLevel = wantedLevel;
            mdc.wantedReason = wantedReason;

            await databaseManager.updateQuery( $"INSERT INTO mdc (cop, suspect, wantedLevel, wantedReason) VALUES ('{mdc.cop}', '{mdc.suspect}', '{mdc.wantedLevel}', '{mdc.wantedReason}')" ).Execute( );

            suspect.SetSharedData( "wantedLevel", suspect.GetSharedData<int>( "wantedLevel" ) + mdc.wantedLevel );
            suspect.SetData( "date", DateTime.Now );

            if (suspect.GetSharedData<int>("wantedLevel") > 6)
                suspect.SetSharedData("wantedLevel", 6);

            var txt = ""; if (!server) txt = $" ({player.Value})";
            suspect.SendChatMessage( $"~r~You committed a crime: {mdc.wantedReason}, reported by {mdc.cop} {txt}. W: +{mdc.wantedLevel}. New wanted level: {suspect.GetSharedData<int>( "wantedLevel" )}." );

            await databaseManager.updateQuery( $"UPDATE accounts SET wantedLevel = '{suspect.GetSharedData<int>( "wantedLevel" )}' WHERE username = '{suspect.Name}' LIMIT 1" ).Execute( );
        }

        public static async Task<List<mdc>> getPlayerMdc( string username ) {
            List<mdc> mdcList = new List<mdc>( );

            await databaseManager.selectQuery( $"SELECT * FROM mdc WHERE suspect = '{username}'", ( DbDataReader reader ) => {
                mdc mdc = new mdc( );
                mdc.id = ( int )reader[ "id" ];
                mdc.cop = ( string )reader[ "cop" ];
                mdc.wantedReason = ( string )reader[ "wantedReason" ];
                mdcList.Add( mdc );
            } ).Execute( );

            return mdcList;
        }

        public static void checkJail(Player player)
        {
            if (player.HasData("jailTime") && player.GetData<int>("jailTime") > 0)
            {
                putPlayerInJail(player, player.GetData<int>("jailTime"));
                player.SendChatMessage($"~r~Because you got arrested, you lost ${player.GetSharedData<int>("wantedLevel") * 600} and you will be taken to the jail for {player.GetData<int>("jailTime")} seconds.");
            }
        }
        static Vector3[] jailCells = { new Vector3(459.1952, -1001.4942, 24.914871), new Vector3(458.9802, -997.8152, 24.914875), new Vector3(459.97733, -994.1974, 24.914867) }; static Random random = new Random();
        public static void putPlayerInJail(Player player, int time)
        {
        
            player.Position = jailCells[random.Next(0, 2)];
        }
    }
}