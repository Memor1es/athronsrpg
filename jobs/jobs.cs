using GTANetworkAPI;
using rpg.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rpg {
   public class jobs : Script {
       
    
 
        public class jobSkill
        {
            public int level { get; set; }
            public int xp { get; set; }
            public playerManager.job job { get; set; }
            public jobSkill( ) { }
            public jobSkill(playerManager.job job, int level, int xp )
            {
                this.job = job;
                this.level = level;
                this.xp = xp;
            }
        }

        public void addPlayerJobXp(Player player, playerManager.job type,  int xp)
        {
            var jobSkills = player.GetData<List<jobSkill>>("jobSkills");
            var jobSkill  = jobSkills.Find( job => job.job == type );
            jobSkill.xp += xp;

        }
       

        [ Command( "getjob" )]
        public async Task onGetJob( Player player ) {
            if ( player.IsInVehicle ) {
                player.SendChatMessage( "!{#CEF0AC}You can use this command only when you are not in a vehicle." );
                return;
            }

            if ( doesPlayerHasJob( player ) != 0 ) {
                player.SendChatMessage( "!{#CECECE}You already have a job." );
                return;
            }

            var closestJob = getClosestJob( player );
            if ( player.Position.DistanceTo( closestJob.position ) > 4 )
                return;

            new accountController( player ).job = closestJob.type;
            player.SendChatMessage( $"{"!{#2CB6D0}"}Your job is now {closestJob.name}." );

            await databaseManager.updateQuery( $"UPDATE accounts SET job = '{(int)closestJob.type}' WHERE username = '{player.Name}'" ).Execute( );
        }
        [Command("testca")]
        public void onStartcaracer(Player player)
        {
            player.TriggerEvent("startCreator");
        }
        [Command( "quitjob" )]
        public async Task onQuitJob( Player player ) {
            if ( player.IsInVehicle ) {
                player.SendChatMessage( "!{#CEF0AC}You can use this command only when you are not in a vehicle." );
                return;
            }

            if ( doesPlayerHasJob( player ) == 0) {
                player.SendChatMessage( "!{#CECECE}You don't have a job." );
                return;
            }

            new accountController( player ).job = 0;
            player.SendChatMessage( "!{#CECECE}You have quit your job." );

            await databaseManager.updateQuery( $"UPDATE accounts SET job = '{playerManager.job.none}' WHERE username = '{player.Name}'" ).Execute( );
        }

        [Command( "gotojob", "Syntax: /gotojob [id]" )]
        public void OnGotoJob( Player player, playerManager.job id ) {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            player.Position = jobList.Find( job => job.type == id ).position;
        }

        public static List<jobModels> jobList = new List<jobModels>( );

        public class jobModels {
            public playerManager.job type { get; set; }
            public string name { get; set; }
            public Vector3 position { get; set; }

            public jobModels( ) { }
            public jobModels( playerManager.job type, string name, Vector3 position ) {
                this.type = type;
                this.name = name;
                this.position = position;
            }
        }

        public jobModels getClosestJob( Player player ) {
            jobModels closest = new jobModels( 0, "none", new Vector3( ) );
            if ( doesPlayerHasJob( player ) == 0 ) {
                float closestDistance = float.MaxValue;
                foreach ( var job in jobList ) {
                    float distance = player.Position.DistanceTo( job.position );
                    if ( closestDistance > distance ) {
                        closestDistance = distance;
                        closest = job;
                    }
                }
            }
            return closest;
        }

        public static playerManager.job doesPlayerHasJob( Player player ) {
            if ( player.HasSharedData( "job" ) )
                return player.GetSharedData<playerManager.job>( "job" );

            return 0;
        }
    }
}
