using System.Linq;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.RBAC;
using NexusForever.WorldServer.Command.Context;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Movement, "A collection of commands to test taxi flights.", "taxi")]
    [CommandTarget(typeof(IPlayer))]
    public class TaxiCommandCategory : CommandCategory
    {
        private static readonly string[] modes = { "release", "spline", "path", "control", "late", "releasepath" };

        [Command(Permission.Movement, "Set how your next taxi flights are driven: release, spline, path, control, late or releasepath.", "mode")]
        public void HandleTaxiMode(ICommandContext context,
            [Parameter("Flight mode: release (default), spline, path, control, late or releasepath.")]
            string mode)
        {
            IPlayer player = context.GetTargetOrInvoker<IPlayer>();
            if (string.IsNullOrWhiteSpace(mode))
            {
                context.SendMessage($"Taxi mode: {(TaxiEntity.TestModes.TryGetValue(player.CharacterId, out string current) ? current : "release")}. Modes: {string.Join(", ", modes)}.");
                return;
            }

            mode = mode.Trim().ToLowerInvariant();
            if (!modes.Contains(mode))
            {
                context.SendMessage($"Unknown taxi mode {mode}. Modes: {string.Join(", ", modes)}.");
                return;
            }

            TaxiEntity.TestModes[player.CharacterId] = mode;
            context.SendMessage($"Taxi mode set to {mode}, it applies from your next flight.");
        }
    }
}
