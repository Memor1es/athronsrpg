using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

public static class WeaponComponent
{

    public static string serializeComponentSet(this List<uint> dataSet)
    {
        return string.Join('|', dataSet.Select(a => Base36Extensions.ToBase36(a)).ToArray());
    }
    public class model
    {
        public WeaponHash weaponHash { get; set; }
        public uint componentHash { get; set; }
        public model() { }
        public model(WeaponHash weaponHash, uint componentHash) { this.componentHash = componentHash; this.weaponHash = weaponHash; }
    }
    public static void giveWeaponComponent(this Entity entity, WeaponHash weaponHash, uint componentHash)
    {
        var weaponComponents = entity.GetData<List<model>>("weaponComponents");
        weaponComponents.Add(new model(weaponHash, componentHash));
        entity.SetData("weaponComponents", weaponComponents);
    }
}