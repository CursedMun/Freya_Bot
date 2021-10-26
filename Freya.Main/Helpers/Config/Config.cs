
using System.Collections.Generic;
using System.Diagnostics;

public static class Config
{
    private static ulong[] eventModRoles =
   {
            896871427410649140
    };
    private static ulong[] maxPermsRoles =
    {
            896871427410649140//admin
    };
    private static ulong[] OgmaRoles =
    {
            896871427410649140//admin
    };
    //private static ulong[] eventModRoles =
    //{
    //        744564449729445888
    //};
    //private static ulong[] maxPermsRoles =
    //{
    //        740312785967251467//admin
    //};
    //private static ulong[] OgmaRoles =
    //{
    //        739918620796125245//admin
    //};
    public static ulong[] EventModRoles { get => eventModRoles; set => eventModRoles = value; }

    public static ulong[] MaxPermsRoles { get => maxPermsRoles; set => maxPermsRoles = value; }
    public static ulong[] OgmaRole { get => OgmaRoles; set => OgmaRoles = value; }
    public enum RolesType
    {
        EventMod,
        MaxPerms,
        Ogma
    }
    private static Dictionary<int, ulong[]> typeRoles = new()
    {
        { 0, EventModRoles },
        { 1, MaxPermsRoles },
        { 2, OgmaRole }
    };
    public static Dictionary<int, ulong[]> TypeRoles { get => typeRoles; set => typeRoles = value; }
    public static bool IsDebug => Debugger.IsAttached;

    //public static class StaticVars
    //{
    //    public const ulong MainGuild = 728716141802815539;
    //    public const ulong AdminGuild = 539086938964099074;

    //    //Channels

    //    public const ulong EventChannelsCategory = 847859894840983592;
    //    public const ulong EventReportsChannel = 870411778885046322;

    //    public const ulong RequestsChannel = 869201823364419584;

    //    public const ulong VacationChannel = 847919033217974322;

    //    public const ulong NewsChannel = 847865099972247562;
    //    public const ulong WarnReportsChannel = 870565342806691890;
    //    //Emoji
    //    public static class Emoji
    //    {
    //        public const string AcceptEmoji = "✅";
    //        public const string DeclineEmoji = "❌";
    //    }

    //    //role phoenix
    //    public const ulong PhoenixRole = 744564449729445888;
    //    //Bad Roles
    //    public const ulong NotVerified = 77777777777777;
    //    public const ulong SecondWarn = 77777777777777;
    //    public const ulong EventBan = 730203472703783023;
    //    public const ulong ChatMute = 730203474263801903;
    //    public const ulong Mute = 730203474662260819;
    //    //Orion
    //}
    public static class StaticVars
    {
        public const ulong MainGuild = 882109412519591958;
        public const ulong AdminGuild = 882109412519591958;

        //Channels

        public const ulong EventChannelsCategory = 900054757903851540;
        public const ulong EventReportsChannel = 900054800530563072;

        public const ulong RequestsChannel = 900054800530563072;

        public const ulong VacationChannel = 900054800530563072;

        public const ulong NewsChannel = 900054800530563072;
        public const ulong WarnReportsChannel = 900054800530563072;
        //Emoji
        public static class Emoji
        {
            public const string AcceptEmoji = "✅";
            public const string DeclineEmoji = "❌";
        }

        //role phoenix
        public const ulong PhoenixRole = 888210172378497115;
        //Bad Roles
        public const ulong NotVerified = 896161540850458664;
        public const ulong SecondWarn = 896161540850458664;
        public const ulong EventBan = 896161540850458664;
        public const ulong ChatMute = 896161540850458664;
        public const ulong Mute = 896161540850458664;
        //Orion
    }
    public class EventCategory
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string[] Events { get; set; }
    }
}
