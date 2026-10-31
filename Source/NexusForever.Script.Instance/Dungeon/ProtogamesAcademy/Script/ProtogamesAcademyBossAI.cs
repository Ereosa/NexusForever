using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.ProtogamesAcademy.Script
{
    // Protogames Academy's assessments: no written guide survives, the fights are rebuilt from each boss' spells in the client data (the
    // Spell4 descriptions name the boss and the mechanic, "[UPJ] e4342 - Invulnotron - IA Cycle - Knockdown (Cast)") and the objective
    // texts ("Defeat Invulnotron by interrupting its abilities"); order, thresholds and timings are guesses. The "IA Cycle" spells are
    // long casts behind interrupt armour that the party is meant to interrupt.

    /// <summary>
    /// Invulnotron, the first assessment, "interrupt its abilities": Interrupt Armor, then a long Knockdown, Tether, Subdue or Basic cast.
    /// </summary>
    [ScriptFilterCreatureId(67475, 71082)]
    public class ProtogamesAcademyInvulnotronAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [80564, 80565];

        private readonly uint[] cycle = [76101, 76109, 76111, 76164]; // Knockdown, Tether, Subdue, Basic (Cast)
        private int next;

        public ProtogamesAcademyInvulnotronAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(80556, 14d, 9d, veteranOnly: true); // Disintegrate
            Every(20d, () =>
            {
                Cast(76103); // Interrupt Armor
                Cast(cycle[next++ % cycle.Length]);
            }, 6d);
        }
    }

    /// <summary>
    /// Gromka the Flamewitch, "avoid telegraphs and interrupt her": Fire Totems that spin their flamethrowers, Flame Waves sweeping
    /// the arena, the Interrupt Armor cycle.
    /// </summary>
    [ScriptFilterCreatureId(67594, 71319)]
    public class ProtogamesAcademyGromkaAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76199, 76200];

        public ProtogamesAcademyGromkaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(25d, () => { Cast(76202); Cast(76201); }, 12d);   // Interrupt Armor, IA Cycle
            Every(30d, () => SummonAdds(67609, 71320, 2, 10f), 5d);  // Fire Totems (Flamethrower)
            Every(18d, () => SummonAdds(67619, 71321, 1, 14f), 15d); // Flame Wave
        }
    }

    /// <summary>
    /// Fire Totem: Flamethrower while it spins.
    /// </summary>
    [ScriptFilterCreatureId(67609, 71320)]
    public class ProtogamesAcademyFireTotemAI : DungeonBossAI
    {
        public ProtogamesAcademyFireTotemAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(6d, () => { Cast(76210); Cast(76209); }, 2d); // Random Spin, Flamethrower
        }
    }

    /// <summary>
    /// Flame Wave: its burning aura.
    /// </summary>
    [ScriptFilterCreatureId(67619, 71321)]
    public class ProtogamesAcademyFlameWaveAI : DungeonBossAI
    {
        public ProtogamesAcademyFlameWaveAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(8d, () => Cast(76221), 0.5d); // Flame Wave (Aura)
        }
    }

    /// <summary>
    /// Iruki Boldbeard, "interrupt him and run to the safe zone": Tracking fields chase the party, the Donut telegraphs leave only the
    /// Shield Generator's ring safe, each one faster than the last, the Interrupt Armor cycle.
    /// </summary>
    [ScriptFilterCreatureId(67663, 71538)]
    public class ProtogamesAcademyIrukiAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76263];

        private readonly uint[] donuts = [76292, 76324, 76326, 76328]; // Donut #1-4 (19, 16, 13 and 10 s casts)
        private int next;

        public ProtogamesAcademyIrukiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(76269, 16d, 6d);                            // Tracking
            Every(22d, () => { Cast(76301); Cast(76300); }, 10d);  // Interrupt Armor, IA Cycle
            Every(35d, () =>
            {
                SummonAdds(67690, 71581, 1, 12f);                  // Shield Generator, the safe zone
                Cast(donuts[Math.Min(next++, donuts.Length - 1)]);
            }, 20d);
        }
    }

    /// <summary>
    /// Seek-N-Slaughter, "defeat it and destroy the Bomb-O-Trons": Barrage, Battlebot Missiles, Bomb-O-Trons that explode after their
    /// fuse unless destroyed.
    /// </summary>
    [ScriptFilterCreatureId(67668, 71203)]
    public class ProtogamesAcademySeekNSlaughterAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76176, 76177];

        public ProtogamesAcademySeekNSlaughterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(76262, 18d, 8d);  // Barrage
            AddAbility(76282, 14d, 4d);  // Battlebot Missile
            Every(25d, () => { Cast(76346); SummonAdds(67724, 71204, 2, 12f); }, 12d); // Spawn Explosive, Bomb-O-Trons
        }
    }

    /// <summary>
    /// Bomb-O-Tron: lights its fuse and explodes when it runs out (20 s cast).
    /// </summary>
    [ScriptFilterCreatureId(67724, 71204)]
    public class ProtogamesAcademyBombOTronAI : DungeonBossAI
    {
        public ProtogamesAcademyBombOTronAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(30d, () => { Cast(81663); Cast(76342); }, 0.5d); // Bomb Fuse, Bomb Explosion
        }
    }

    /// <summary>
    /// Icebox Mk. 2, "Just Right": Frost Blast, Frost Machines that freeze whoever stands in their aura, Melee Mechs.
    /// </summary>
    [ScriptFilterCreatureId(67757, 71205)]
    public class ProtogamesAcademyIceboxAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76533, 76534];

        public ProtogamesAcademyIceboxAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(76460, 9d, 5d);                                // Blast
            Every(20d, () => Cast(IsVeteran ? 76587u : 79253u), 10d); // Frost Blast
            Every(30d, () => SummonAdds(67738, 71207, 2, 16f), 6d);   // Frost Machines
            AtHealth(0.5f, () => SummonAdds(68242, 71208, 3, 10f));   // Melee Mechs
        }
    }

    /// <summary>
    /// Frost Machine: the Frost Blast Aura.
    /// </summary>
    [ScriptFilterCreatureId(67738, 71207)]
    public class ProtogamesAcademyFrostMachineAI : DungeonBossAI
    {
        public ProtogamesAcademyFrostMachineAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            Every(10d, () => Cast(76576), 1d); // Frost Blast Aura
        }
    }

    /// <summary>
    /// Super-Invulnotron, "Botherbot Bustle": Smash and Pound, Blast, Botherbots that smash whoever they reach, a Shield that the
    /// Energized Generators feed (their Pillar Explosions hurt everyone near).
    /// </summary>
    [ScriptFilterCreatureId(68096, 71209)]
    public class ProtogamesAcademySuperInvulnotronAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [77033, 77034];

        public ProtogamesAcademySuperInvulnotronAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(77057, 15d, 6d); // Smash and Pound
            AddAbility(77044, 8d, 3d);  // Blast
            Every(20d, () => SummonAdds(68101, 71213, 3, 14f), 10d);                     // Botherbots
            AtHealth(0.66f, () => { Cast(77001); SummonAdds(68062, 0u, 2, 18f); });      // Shield, Energized Generators
            AtHealth(0.33f, () => { Cast(77001); SummonAdds(68062, 0u, 2, 18f); });
        }
    }

    /// <summary>
    /// Botherbot: Smashy.
    /// </summary>
    [ScriptFilterCreatureId(68101, 71213)]
    public class ProtogamesAcademyBotherbotAI : DungeonBossAI
    {
        public ProtogamesAcademyBotherbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(77081, 6d, 2d); // Smashy
        }
    }

    /// <summary>
    /// Wrathbone, the final assessment: Breath, Rock (shrapnel flies out from where it lands), Burn Down; the Dance and Adds transitions
    /// at 66 and 33% (Flamelings), Meteors.
    /// </summary>
    [ScriptFilterCreatureId(67944, 71226)]
    public class ProtogamesAcademyWrathboneAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76740, 76741];

        public ProtogamesAcademyWrathboneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(76762, 12d, 5d);  // Breath
            AddAbility(76999, 16d, 9d);  // Rock
            AddAbility(76991, 30d, 20d); // Burn Down
            AtHealth(0.66f, () => Cast(76975));                                       // Transition (Dance)
            AtHealth(0.33f, () => { Cast(76982); SummonAdds(67977, 71229, 4, 12f); }); // Transition (Adds), Flamelings
        }
    }

    /// <summary>
    /// Flamelings.
    /// </summary>
    [ScriptFilterCreatureId(67977, 71229)]
    public class ProtogamesAcademyFlamelingAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [76774, 76776];

        public ProtogamesAcademyFlamelingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }
    }
}
