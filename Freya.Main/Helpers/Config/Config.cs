
using System.Collections.Generic;
using System.Diagnostics;

public static class Config
{
    private static ulong[] eventModRoles =
    {
            744564449729445888
    };
    private static ulong[] maxPermsRoles =
    {
            740312785967251467//admin
    };
    public static ulong[] EventModRoles { get => eventModRoles; set => eventModRoles = value; }

    public static ulong[] MaxPermsRoles { get => maxPermsRoles; set => maxPermsRoles = value; }
    public enum RolesType
    {
        EventMod,
        MaxPerms
    }
    private static Dictionary<int, ulong[]> typeRoles = new()
    {
        { 0, EventModRoles },
        { 1, MaxPermsRoles }
    };
    public static Dictionary<int, ulong[]> TypeRoles { get => typeRoles; set => typeRoles = value; }


    //private static EventCategory[] eventsCategory = {
    //        new()
    //        {
    //            Name = "маленький",
    //            Text = "✧ маленький 5 баллов.",
    //            Events = new string[]
    //            { "пазлы", "дурак", "песня по тексту", "скоропечатанье", "монополия", "смехлыст", "угадай мелодию", "без остановки", "угадай знаменитость", "испорченный телефон" }
    //        }
    //        , new()
    //        {
    //            Name = "средний",
    //            Text = "✧ средний  10 баллов.",
    //            Events = new string[]
    //            { "коднеймс", "шляпа", "корова", "шпион", "правда или действие", "крокодил", "соло", "кто я" }
    //        }, new()
    //        {
    //            Name = "большой",
    //            Text = "✧ большой  15 баллов.",
    //            Events = new string[]
    //            { "своя игра", "психушка", "бункер", "шашки", "криминалист", "кинотеатр" }
    //        } };
    //public static EventCategory[] EventsCategory { get => eventsCategory; set => eventsCategory = value; }

    public static bool IsDebug => Debugger.IsAttached;

    public static class StaticVars
    {
        public const ulong MainGuild = 728716141802815539;
        public const ulong AdminGuild = 539086938964099074;

        //Channels

        public const ulong EventChannelsCategory = 847859894840983592;
        public const ulong EventReportsChannel = 870411778885046322;

        public const ulong RequestsChannel = 869201823364419584;

        public const ulong VacationChannel = 847919033217974322;

        public const ulong NewsChannel = 847865099972247562;
        public const ulong WarnReportsChannel = 870565342806691890;
        //Emoji
        public static class Emoji
        {
            public const string AcceptEmoji = "✅";
            public const string DeclineEmoji = "❌";
        }

        //role phoenix
        public const ulong PhoenixRole = 744564449729445888;
        //Bad Roles
        public const ulong EventBan = 730203472703783023;
        public const ulong ChatMute = 730203474263801903;
        public const ulong Mute = 730203474662260819;
        //Orion
    }

    public class EventCategory
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string[] Events { get; set; }
    }
}
