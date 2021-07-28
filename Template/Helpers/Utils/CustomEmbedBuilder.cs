
using Discord;

namespace Freya.Helpers.Util
{
    public class CustomEmbedBuilder : EmbedBuilder
    {
        public CustomEmbedBuilder()
        {
            //this.Color = new Color(0x2F3136);
        }
        public new Discord.Color Color { get; set; } = new Color(0x2A3136);
    }
}
