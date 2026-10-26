using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary.Script
{
    // Boss abilities from the archived Jabbithole ability lists (the Spell4 descriptions name the boss and the ability), cooldowns guessed.
    // The bosses' scripted mechanics (Shallaos' chimes and echoes, Ondu's pull, Skash's spawners, Selene's darkness effigies and
    // shades) are server data that isn't in the client, they are left out.

    /// <summary>
    /// Deadringer Shallaos.
    /// </summary>
    [ScriptFilterCreatureId(28600, 28599)]
    public class SwordmaidenSanctuaryShallaosAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42008];

        public SwordmaidenSanctuaryShallaosAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ondu Lifeweaver: Plague Smash.
    /// </summary>
    [ScriptFilterCreatureId(28721, 28720)]
    public class SwordmaidenSanctuaryOnduAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34235, 34236];

        public SwordmaidenSanctuaryOnduAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42420, 12d, 5d, normalOnly: true);  // Plague Smash
            AddAbility(63384, 12d, 5d, veteranOnly: true); // Plague Smash (veteran)
        }
    }

    /// <summary>
    /// Moldwood Overlord Skash: Tentacle Wrath, Corrupting Revolver.
    /// </summary>
    [ScriptFilterCreatureId(28727, 28728)]
    public class SwordmaidenSanctuarySkashAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42063, 42064];

        public SwordmaidenSanctuarySkashAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42070, 12d, 5d);  // Tentacle Wrath
            AddAbility(63469, 20d, 12d); // Corrupting Revolver
        }
    }

    /// <summary>
    /// Rayna Darkspeaker: Raging Lava, Smoldering, Molten Wave.
    /// </summary>
    [ScriptFilterCreatureId(28733, 28732)]
    public class SwordmaidenSanctuaryRaynaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42051];

        public SwordmaidenSanctuaryRaynaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42059, 12d, 5d);                     // Raging Lava
            AddAbility(42322, 16d, 9d, normalOnly: true);   // Smoldering
            AddAbility(63319, 16d, 9d, veteranOnly: true);  // Smoldering (veteran)
            AddAbility(54394, 28d, 18d);                    // Molten Wave
        }
    }

    /// <summary>
    /// Spiritmother Selene the Corrupted: Shade Prison, Blackout, Lights Out!
    /// </summary>
    [ScriptFilterCreatureId(28735, 28736)]
    public class SwordmaidenSanctuarySeleneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42378, 42379];

        public SwordmaidenSanctuarySeleneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42290, 14d, 6d);  // Shade Prison
            AddAbility(42540, 22d, 12d); // Blackout
            AddAbility(42675, 30d, 20d); // Lights Out!
        }
    }
}
