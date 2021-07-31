using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Freya.Helpers.Precondition
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireRoleAttribute : PreconditionAttribute
    {
        // Create a field to store the specified name
        private readonly ulong[] _roles;

        // Create a constructor so the name can be specified
        public RequireRoleAttribute(Config.RolesType RolesId)
        {
            bool validation = Config.TypeRoles.TryGetValue((int)RolesId, out _roles);
            if (!validation) return;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is IUser gUser)
            {
                var user = (context.Client as DiscordSocketClient).GetMainGuild().GetUser(gUser.Id);
                if (user.Roles.Any(r => _roles.Any(s => r.Id == s)))
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return await Task.FromResult(PreconditionResult.FromError($"You can't execute this command."));
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
