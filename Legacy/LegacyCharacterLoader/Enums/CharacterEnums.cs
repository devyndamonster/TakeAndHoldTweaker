using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Newtonsoft.Json;

namespace LegacyCharacterLoader.Enums
{
    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum ObjectCategory
    {
        Uncategorized,
        Firearm,
        Magazine,
        Clip,
        Cartridge,
        Attachment,
        SpeedLoader,
        Thrown,
        MeleeWeapon = 10,
        Explosive = 20,
        Powerup = 25,
        Target = 30,
        Tool = 40,
        Toy,
        Firework,
        Ornament,
        Loot = 50,
        VFX
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagEra
    {
        None,
        Colonial,
        WildWest,
        TurnOfTheCentury,
        WW1,
        WW2,
        PostWar,
        Modern,
        Futuristic,
        Medieval
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagSet
    {
        Real,
        GroundedFictional,
        SciFiFictional,
        Meme,
        MF,
        Holiday,
        TNH,
        NonCombat
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmSize
    {
        None,
        Pocket,
        Pistol,
        Compact,
        Carbine,
        FullSize,
        Bulky,
        Oversize
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmAction
    {
        None,
        BreakAction,
        BoltAction,
        Revolver,
        PumpAction,
        LeverAction,
        Automatic,
        RollingBlock,
        OpenBreach,
        Preloaded,
        SingleActionRevolver
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmFiringMode
    {
        None,
        SemiAuto,
        Burst,
        FullAuto,
        SingleFire
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmFeedOption
    {
        None,
        BreachLoad,
        InternalMag,
        BoxMag,
        StripperClip,
        EnblocClip
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmRoundPower
    {
        None,
        Tiny,
        Pistol,
        Shotgun,
        Intermediate,
        FullPower,
        AntiMaterial,
        Ordnance,
        Exotic,
        Fire
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagFirearmMount
    {
        None,
        Picatinny,
        Russian,
        Muzzle,
        Stock,
        Bespoke
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagAttachmentFeature
    {
        None,
        IronSight,
        Magnification,
        Reflex,
        Suppression,
        Stock,
        Laser,
        Illumination,
        Grip,
        Decoration,
        RecoilMitigation,
        BarrelExtension,
        Adapter,
        Bayonet,
        ProjectileWeapon,
        Bipod
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagMeleeStyle
    {
        None,
        Tactical,
        Tool,
        Improvised,
        Medieval,
        Shield,
        PowerTool
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagMeleeHandedness
    {
        None,
        OneHanded,
        TwoHanded
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagPowerupType
    {
        None = -1,
        Health,
        QuadDamage,
        InfiniteAmmo,
        Invincibility,
        GhostMode,
        FarOutMeat,
        MuscleMeat,
        HomeTown,
        SnakeEye,
        Blort,
        Regen,
        Cyclops,
        WheredIGo,
        ChillOut
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagThrownType
    {
        None,
        ManualFuse,
        Pinned,
        Strange
    }

    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum TagThrownDamageType
    {
        None,
        Kinetic,
        Explosive,
        Fire,
        Utility
    }
}
