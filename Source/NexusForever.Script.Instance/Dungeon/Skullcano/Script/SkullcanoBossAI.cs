using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.Skullcano.Script
{
    // Skullcano's bosses: no written guide survives, the fights are rebuilt from each boss' spells in the client data (the Spell4
    // descriptions name the boss and the mechanic) and the archived Jabbithole ability lists; order, thresholds and timings are guesses.

    /// <summary>
    /// Stew-Shaman Tugga: Fire Bolt, Magma Totems (summoned in pairs, then trios), Molten Rain; below half health the Feeding Frenzy
    /// (Devour Flesh, Meat Wave).
    /// </summary>
    [ScriptFilterCreatureId(24493, 24898)]
    public class SkullcanoTuggaAI : DungeonBossAI
    {
        public SkullcanoTuggaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36097, 6d, 2d);   // Fire Bolt
            AddAbility(41016, 18d, 10d); // Molten Rain
            Every(25d, () => { Cast(40988); SummonAdds(32664, 38592, 2, 8f); }, 8d); // Magma Totem Pair
            AtHealth(0.5f, () =>
            {
                Cast(41531);                 // Feeding Frenzy
                AddAbility(41010, 15d, 3d);  // Devour Flesh
                AddAbility(40981, 20d, 8d);  // Meat Wave
            });
        }
    }

    /// <summary>
    /// Thunderfoot: Pulverize, Thunder Pound (a mushroom grows where it lands), Seismic Tremor, Spore Clouds; Enraging Fury at 20%.
    /// </summary>
    [ScriptFilterCreatureId(24475, 24893)]
    public class SkullcanoThunderfootAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [36309, 36310];

        public SkullcanoThunderfootAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36283, 10d, 4d);  // Pulverize
            AddAbility(36437, 24d, 15d); // Seismic Tremor
            Every(16d, () => { Cast(36341); SummonAdds(33348, 33350, 1, 6f); }, 9d);  // Thunder Pound, Mushroom
            Every(30d, () => { Cast(36332); SummonAdds(33079, 59974, 2, 10f); }, 20d); // Summon Spore Cloud
            AtHealth(0.2f, () => Cast(36324));                                          // Enraging Fury
        }
    }

    /// <summary>
    /// Bosun Octog: Shred, Hookshot, Noxious Ink, Scab Stab; Radio Dispatch drops a Razooki at 75%, the Squirg phase at 50% (Ink
    /// Shield, squirgling offspring), Razookis and squirglings again at 25%.
    /// </summary>
    [ScriptFilterCreatureId(24486, 24894)]
    public class SkullcanoOctogAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [36628, 36650];

        public SkullcanoOctogAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41850, 8d, 4d);   // Shred
            AddAbility(31283, 14d, 7d);  // Hookshot
            AddAbility(36614, 18d, 11d); // Noxious Ink
            AddAbility(41443, 22d, 15d); // Scab Stab
            AtHealth(0.75f, () => { Cast(41454); SummonAdds(39419, 56779, 2, 12f); });                      // Radio Dispatch, Razooki
            AtHealth(0.5f, () => { Cast(41831); Cast(41872); SummonAdds(38426, 56952, 3, 10f); });          // Squirg Phase, Ink Shield
            AtHealth(0.25f, () => { SummonAdds(39419, 56779, 1, 12f); SummonAdds(38426, 56952, 3, 10f); });
        }
    }

    /// <summary>
    /// Mordechai Redmoon: Pin, Vicious Barrage, Turret Grenade, Disperse Grenades, Evasive Maneuver; the Terraformer joins at 66 and 33%
    /// (Molten Torrent, Combustion), his crew at 50%.
    /// </summary>
    [ScriptFilterCreatureId(24489, 24895)]
    public class SkullcanoMordechaiAI : DungeonBossAI
    {
        protected override uint[] AutoAttacks => [38532, 38536];

        public SkullcanoMordechaiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager, creatureInfoManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39481, 12d, 5d);  // Pin
            AddAbility(38465, 15d, 9d);  // Vicious Barrage
            AddAbility(39492, 20d, 14d); // Turret Grenade
            AddAbility(39477, 24d, 18d); // Disperse Grenades
            AddAbility(39531, 30d, 22d); // Evasive Maneuver
            AtHealth(0.66f, () => Cast(39391));                             // Terraformer: Molten Torrent
            AtHealth(0.5f, () => SummonAdds(24618, 24920, 2, 14f));          // his crew: Redmoon Crushers
            AtHealth(0.33f, () => Cast(39501));                             // Terraformer: Combustion
        }
    }
}
