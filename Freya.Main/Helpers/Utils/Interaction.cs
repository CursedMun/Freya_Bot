using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Interactivity;

namespace Freya.Helpers.Interaction
{
    public class Interaction
    {
        public InteractivityService Interactivity { get; set; }
        public Interaction(InteractivityService interactivityService)
        {
            Interactivity = interactivityService;
        }
        /// <summary>
        /// Ask a user for something;
        /// </summary>
        /// <param name="message"></param>
        /// <param name="question"></param>
        /// <returns>Message content/ null</returns>
        public async Task<string[]> Ask(SocketUserMessage message, string[] questions)
        {
            List<string> Answers = new();
            for (int i = 0; i < questions.Length; i++)
            {
                var question = questions.ElementAt(i);
                var msg = await message.Channel.SendMessageAsync(question);
                var result = await Interactivity.NextMessageAsync(x => x.Author.Id == message.Author.Id && x.Channel.Id == message.Channel.Id);
                await msg.DeleteAsync();
                await result.Value.DeleteAsync();
                Answers.Add(result.IsSuccess ? result.Value.Content : "");
            }
            return Answers.ToArray();
        }
    }
}
