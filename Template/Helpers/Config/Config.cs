
using System.Collections.Generic;
using System.Diagnostics;

public static class Config
{
    private static ulong[] eventModRoles =
    {
            833867528207990844,
            744564449729445888,
            740312130456256552,
            740312785967251467,
            739918620796125245,
            580486811034583060,
            868871380807086100
    };
    private static ulong[] maxPermsRoles =
    {
            869603273202622524//admin
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
        public const ulong MainGuild = 572208352063389707;
        //Channels

        public const ulong EventChannelsCategory = 572208352063389713;
        public const ulong EventReportsChannel = 868871583635226694;

        public const ulong RequestsChannel = 868871583635226694;

        public const ulong VacationChannel = 868871583635226694;

        public const ulong NewsChannel = 868871583635226694;

        //Emoji
        public static class Emoji
        {
            public const string AcceptEmoji = "✅";
            public const string DeclineEmoji = "❌";
        }
    }






    public class EventCategory
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string[] Events { get; set; }
    }
}
