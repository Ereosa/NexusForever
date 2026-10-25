using NexusForever.Database.Character;
using NexusForever.Game.Static.Crafting;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Game.Abstract.Crafting
{
    public interface ITradeskill : IDatabaseCharacter, INetworkBuildable<TradeskillInfo>
    {
        TradeskillType Id { get; }
        TradeskillEntry Entry { get; }

        uint Xp { get; set; }
        bool Active { get; set; }

        /// <summary>
        /// Current tier, derived from <see cref="Xp"/> and the <see cref="TradeskillTierEntry"/> requirements.
        /// </summary>
        TradeskillTierEntry Tier { get; }

        /// <summary>
        /// Highest tier available for this tradeskill.
        /// </summary>
        TradeskillTierEntry MaxTier { get; }

        bool IsHobby { get; }
        bool IsHarvesting { get; }
        bool IsAutoLearn { get; }
    }
}
