
using System.Collections.Generic;
using System.Diagnostics;

public static class Config
{
    private static ulong[] eventModRoles =
    {
            1047999116287950928
    };
    private static ulong[] maxPermsRoles =
    {
            1047999054812041266//admin
    };
    private static ulong[] OgmaRoles =
    {
            1047999054812041266//admin
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
        public const ulong MainGuild = 945817594835898409;
        public const ulong AdminGuild = 945817594835898409;

        //Channels

        public const ulong EventChannelsCategory = 1047993562681315338;
        public const ulong EventReportsChannel = 1047997688102277130;

        public const ulong RequestsChannel = 1047997688102277130;

        public const ulong VacationChannel = 1047997688102277130;

        public const ulong NewsChannel = 1047997688102277130;
        public const ulong WarnReportsChannel = 1047997688102277130;
        //Emoji
        public static class Emoji
        {
            public const string AcceptEmoji = "✅";
            public const string DeclineEmoji = "❌";
        }

        //role phoenix
        public const ulong PhoenixRole = 1047999247787761664;
        //Bad Roles
        public const ulong NotVerified = 1048000130894282792;
        public const ulong SecondWarn = 1047999981421854741;
        public const ulong EventBan = 1047999981421854741;
        public const ulong ChatMute = 1047999981421854741;
        public const ulong Mute = 1047999981421854741;
        //Orion
    }
    public class EventCategory
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string[] Events { get; set; }
    }
}
