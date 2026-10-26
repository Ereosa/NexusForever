using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary.Script
{
    // The Sanctuary of the Swordmaiden's bosses: no written guide survives, the fights are rebuilt from each boss' spells in the client
    // data (the Spell4 descriptions name the boss and the mechanic) and the archived Jabbithole ability lists; order, thresholds and
    // timings are guesses.

    /// <summary>
    /// Deadringer Shallaos: the chimes of her hall resonate (Chimes - Resonance), her Echoes come out at 75, 50 and 25% (Echo Target
    /// Selection), Prison Chimes trap a player below half health.
    /// </summary>
    [ScriptFilterCreatureId(28600, 28599)]
    public class SwordmaidenSanctuaryShallaosAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [42008];

        public SwordmaidenSanctuaryShallaosAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(20d, () => Cast(54615), 10d);                                     // Chimes - Resonance
            AtHealth(0.75f, () => { Cast(54244); SummonAdds(40825, 0u, 2, 10f); }); // Echo Target Selection, Echoes
            AtHealth(0.5f, () => { Cast(54244); SummonAdds(40825, 0u, 2, 10f); SummonAdds(40884, 0u, 1, 6f); });
            AtHealth(0.25f, () => { Cast(54244); SummonAdds(40825, 0u, 3, 10f); SummonAdds(40884, 0u, 1, 6f); });
        }
    }

    /// <summary>
    /// Ondu Lifeweaver: Plague Smash; cycles through his Life Weaver (Heal), Fury (Blight Fury) and Resistance (Vitara's Resilience)
    /// phases, pulls the party in and roots them below half health (Pull To Location, PBAE Root, PBAE Explosion).
    /// </summary>
    [ScriptFilterCreatureId(28721, 28720)]
    public class SwordmaidenSanctuaryOnduAI : DungeonBossAI
    {
        private int stance;

        protected override uint[] AutoAttacks => [34235, 34236];

        public SwordmaidenSanctuaryOnduAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42420, 12d, 5d, normalOnly: true);  // Plague Smash
            AddAbility(63384, 12d, 5d, veteranOnly: true); // Plague Smash (veteran)
            Every(25d, () =>
            {
                // Life Weaver -> Fury -> Resistance
                uint[] phases = [42828, 42827, 42829];
                Cast(phases[stance++ % phases.Length]);
            }, 15d);
            AtHealth(0.5f, () => { Cast(43368); Cast(42436); });
            Every(30d, () => Cast(42442), 40d); // PBAE Explosion
        }
    }

    /// <summary>
    /// Moldwood Overlord Skash: Tentacle Wrath, Corrupting Revolver, Corruption Siphon; Summon the Swarm (Moldwood Swarmlings) and the
    /// Gaze of the Moldwood (Death Gaze) at 66 and 33%.
    /// </summary>
    [ScriptFilterCreatureId(28727, 28728)]
    public class SwordmaidenSanctuarySkashAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [42063, 42064];

        public SwordmaidenSanctuarySkashAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42070, 12d, 5d);  // Tentacle Wrath
            AddAbility(63469, 20d, 12d); // Corrupting Revolver
            AddAbility(42068, 24d, 18d); // Corruption Siphon
            Every(30d, () => { Cast(42060); SummonAdds(40393, 40396, 3, 10f); }, 15d); // Summon the Swarm
            AtHealth(0.66f, () => { Cast(42056); SummonAdds(40402, 40404, 1, 14f); }); // Death Gaze, Gaze of the Moldwood
            AtHealth(0.33f, () => { Cast(42973); SummonAdds(40402, 40404, 1, 14f); }); // Disgorge Swarmlings
        }
    }

    /// <summary>
    /// Rayna Darkspeaker: Raging Lava, Smoldering, Flame Leash, Flame Geyser; Molten Wave at 75, 50 and 25%.
    /// </summary>
    [ScriptFilterCreatureId(28733, 28732)]
    public class SwordmaidenSanctuaryRaynaAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [42051];

        public SwordmaidenSanctuaryRaynaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42059, 12d, 5d);                     // Raging Lava
            AddAbility(42322, 16d, 9d, normalOnly: true);   // Smoldering
            AddAbility(63319, 16d, 9d, veteranOnly: true);  // Smoldering (veteran)
            AddAbility(42087, 20d, 12d);                    // Flame Leash
            AddAbility(42349, 24d, 16d);                    // Flame Geyser
            AtHealth(0.75f, () => Cast(54397));             // Molten Wave
            AtHealth(0.5f, () => Cast(54397));
            AtHealth(0.25f, () => Cast(54397));
        }
    }

    /// <summary>
    /// Spiritmother Selene the Corrupted: Hack and Slash, Whirlwind, Shade Prison, Creeping and Engulfing Shadows; Blackout and Lights
    /// Out! with the Darkness Effigies at 70%, the Nightmare Ripple adds phase (Prison Shades) at 40%, Luminous Sanctuary at 15%.
    /// </summary>
    [ScriptFilterCreatureId(28735, 28736)]
    public class SwordmaidenSanctuarySeleneAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [42378, 42379];

        public SwordmaidenSanctuarySeleneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42472, 10d, 4d);                     // Hack and Slash
            AddAbility(42478, 16d, 8d);                     // Whirlwind
            AddAbility(42290, 20d, 12d);                    // Shade Prison
            AddAbility(42433, 24d, 16d);                    // Creeping Shadows
            AddAbility(42530, 28d, 20d, normalOnly: true);  // Engulfing Shadows
            AddAbility(42670, 28d, 20d, veteranOnly: true); // Engulfing Shadows (veteran)
            AtHealth(0.7f, () => { Cast(42540); SummonAdds(41238, 56936, 2, 16f); Cast(42675); });   // Blackout, Darkness Effigies, Lights Out!
            AtHealth(0.4f, () => { Cast(42786); SummonAdds(41331, 0u, 3, 12f); });                   // Nightmare Ripple, Prison Shades
            AtHealth(0.15f, () => Cast(56010));                                                     // Luminous Sanctuary
        }
    }
}
