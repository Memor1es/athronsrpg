using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{
    public class test : Script
    {

        [Command("testat")]
        public void TestCommand(Player client, WeaponHash weaponhash, uint weaponComponent)
        {
            client.SetClothes(11, 1, 0);
            client.SetClothes(4, 36, 1);

        }
        public int getboneindex(Player handle, int boneid)
        {
           return NAPI.Native.FetchNativeFromPlayer<int>(handle, 0x3F428D08BE5AAE31, new object[]{ handle.Handle, boneid });

        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnect(Player client)
        {
            // reset data on connect
            client.ClearAttachments();
        }

        //REQUIRED
        [RemoteEvent("staticAttachments.Add")]
        private void OnStaticAttachmentAdd(Player client, string hash)
        {
            client.AddAttachment(Base36Extensions.FromBase36(hash), false);
        }

        //REQUIRED
        [RemoteEvent("staticAttachments.Remove")]
        private void OnStaticAttachmentRemove(Player client, string hash)
        {
            client.AddAttachment(Base36Extensions.FromBase36(hash), true);
        }
    }
}
