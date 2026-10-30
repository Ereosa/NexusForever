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
        private static readonly string[] modes = { "spline", "path", "control", "late" };

        [Command(Permission.Movement, "Set how your next taxi flights are driven: spline, path, control or late.", "mode")]
        public void HandleTaxiMode(ICommandContext context,
            [Parameter("Flight mode: spline (default), path, control or late.")]
            string mode)
        {
            IPlayer player = context.GetTargetOrInvoker<IPlayer>();
            if (string.IsNullOrWhiteSpace(mode))
            {
                context.SendMessage($"Taxi mode: {(TaxiEntity.TestModes.TryGetValue(player.CharacterId, out string current) ? current : "spline")}. Modes: {string.Join(", ", modes)}.");
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
