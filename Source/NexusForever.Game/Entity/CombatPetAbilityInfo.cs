namespace NexusForever.Game.Entity
{
    /// <summary>
    /// What the owner's bot ability (the summon spell's Spell4IdPetSwitch) does when cast.
    /// </summary>
    public enum CombatPetAbilityAction
    {
        /// <summary>
        /// The bot ability spell has its own PetCastSpell effect (eg: 35657 Shield Boost).
        /// </summary>
        None,

        /// <summary>
        /// The owner casts a "Player Cast" spell on the selected target, its PetCastSpell effect commands the pet (eg: 35123 Barrage, 52419 Blitz).
        /// </summary>
        OwnerCast,

        /// <summary>
        /// The pet leaps to the selected target and casts its spell (eg: 70593 Strobe "Commands your Diminisherbot to leap to the nearest targeted enemy").
        /// </summary>
        PetLeapCast
    }

    /// <summary>
    /// An attack a pet uses on its own timer, separate from its auto attack rotation.
    /// </summary>
    /// <param name="OutOfRange">Only used when the target is out of the pet's attack range, eg: Stalker clone pounce.</param>
    public sealed record CombatPetSpecialAttack(uint Spell4BaseId, TimeSpan Interval, bool OutOfRange = false);

    /// <summary>
    /// Server side behaviour of a combat pet, keyed by the Spell4Base id of its summon spell.
    /// </summary>
    /// <remarks>
    /// The client data links each summon spell to its bot ability (Spell4IdPetSwitch) and the tooltips name the bot's spells, but which spells the bot casts on its own is server script.
    /// Spells are stored by Spell4Base id and cast at the tier of the summon spell, every bot spell has the same 9 tiers as its summon spell.
    /// </remarks>
    public sealed class CombatPetAbilityInfo
    {
        public uint SummonSpell4BaseId { get; init; }

        /// <summary>
        /// Engineer bots have stances, the pet bar, the two bot limit and one bot per summon spell.
        /// </summary>
        public bool IsBot { get; init; }

        /// <summary>
        /// Pet doesn't move, eg: Settler turret.
        /// </summary>
        public bool Stationary { get; init; }

        /// <summary>
        /// Spells cast in order on the pet's target every SummonPet attack interval.
        /// </summary>
        public uint[] AttackSpell4BaseIds { get; init; } = [];

        /// <summary>
        /// Attack interval for summons whose SummonPet effect has none.
        /// </summary>
        public TimeSpan AttackInterval { get; init; } = TimeSpan.FromSeconds(1.5d);

        public CombatPetSpecialAttack[] SpecialAttacks { get; init; } = [];

        /// <summary>
        /// Attacks for summons that create several different creatures, keyed by creature id.
        /// </summary>
        public Dictionary<uint, (uint[] AttackSpell4BaseIds, CombatPetSpecialAttack[] SpecialAttacks)> CreatureAttacks { get; init; } = [];

        /// <summary>
        /// Spell4Base id of the bot ability the summon spell switches to while the pet is summoned.
        /// </summary>
        public uint PetSwitchSpell4BaseId { get; init; }

        public CombatPetAbilityAction AbilityAction { get; init; }

        /// <summary>
        /// Spells cast for <see cref="AbilityAction"/>, by the owner for <see cref="CombatPetAbilityAction.OwnerCast"/> otherwise by the pet.
        /// </summary>
        public uint[] AbilitySpell4BaseIds { get; init; } = [];

        /// <summary>
        /// Spells the pet casts after one of its ability spells, keyed by the Spell4Base id of the ability spell.
        /// </summary>
        /// <remarks>
        /// The ability spells only move the pet, the effect the tooltip describes is a separate spell with no data link to it.
        /// </remarks>
        public Dictionary<uint, (TimeSpan Delay, uint[] Spell4BaseIds)> FollowUpSpells { get; init; } = [];

        private static readonly Dictionary<uint, CombatPetAbilityInfo> infos = new CombatPetAbilityInfo[]
        {
            // Artillerybot: "deals damage to 1 foe every 2s", Barrage
            new()
            {
                SummonSpell4BaseId    = 27002,
                IsBot                 = true,
                AttackSpell4BaseIds   = [20491],  // 34521 Engineer - Pet - Artillery Bot - Auto Attack
                PetSwitchSpell4BaseId = 34051,    // 51365 Engineer - Artillery Bot - Barrage - Base
                AbilityAction         = CombatPetAbilityAction.OwnerCast,
                AbilitySpell4BaseIds  = [20884]   // 35123 Engineer - Artillery Bot - Player Cast - Barrage
            },
            // Repairbot: "deals damage to 1 foe and restores shield to you every 3s", the attack proxies 70622 Auto Heal
            new()
            {
                SummonSpell4BaseId    = 26998,
                IsBot                 = true,
                AttackSpell4BaseIds   = [32801],  // 49983 Engineer - Pet - Repair Bot - Auto Attack
                PetSwitchSpell4BaseId = 21307,    // 35657 Engineer - Repair Bot - Shield Boost - Base
                AbilityAction         = CombatPetAbilityAction.None,
                FollowUpSpells        = new()
                {
                    // "Command the Repairbot to return to you and restore shield to allies": 70640 returns the bot, 35658 is the shield channel
                    [47607] = (TimeSpan.Zero, [21308])
                }
            },
            // Diminisherbot: "deals damage every 2s to 1 foe", Strobe
            new()
            {
                SummonSpell4BaseId    = 27021,
                IsBot                 = true,
                AttackSpell4BaseIds   = [20399],  // 34429 Engineer - Pet - Diminisher Bot - Auto Attack
                PetSwitchSpell4BaseId = 47569,    // 70593 Engineer - Diminisher Bot - Strobe - Base
                AbilityAction         = CombatPetAbilityAction.PetLeapCast,
                AbilitySpell4BaseIds  =
                [
                    27148,                        // 42963 Engineer - Pet - Diminisher Bot - Strobe - Base
                    36039                         // 56076 Engineer - Pet - Diminisher Bot - Strobe - Aura
                ]
            },
            // Bruiserbot: "deals damage twice every 2s to foes", Blitz
            new()
            {
                SummonSpell4BaseId    = 27082,
                IsBot                 = true,
                AttackSpell4BaseIds   = [21194],  // 35503 Engineer - Pet - Bruiser Bot - Crush - Auto Attack
                PetSwitchSpell4BaseId = 21192,    // 35501 Engineer - Bruiser Bot - Blitz - Base
                AbilityAction         = CombatPetAbilityAction.OwnerCast,
                AbilitySpell4BaseIds  = [34410],  // 52419 Engineer - Bruiser Bot - Player Cast - Blitz
                FollowUpSpells        = new()
                {
                    // "commands your Bruiser Bot to leap to the target, dealing damage and Taunting": 51319 leaps (500ms flight), 51322 is the damage and taunt
                    [34009] = (TimeSpan.FromMilliseconds(500d), [34012])
                }
            },

            // Esper Geist: "deals magic damage every 2.25s", the SummonPet effect has no attack interval
            new()
            {
                SummonSpell4BaseId  = 21743,
                AttackSpell4BaseIds =
                [
                    21759,                        // 36174 Esper - Pet - Geist - Auto Attack #1
                    23395                         // 39044 Esper - Pet - Geist - Auto Attack #2
                ],
                AttackInterval      = TimeSpan.FromMilliseconds(2250d)
            },
            // Esper Spectral Swarm phantoms (3 summoned by 36200), attack interval from the SummonPet effect
            new()
            {
                SummonSpell4BaseId  = 19349,
                AttackSpell4BaseIds =
                [
                    12879,                        // 26385 Esper - Spectral Swarm - Auto Attack #1
                    12880                         // 26386 Esper - Spectral Swarm - Auto Attack #2
                ]
            },
            // Stalker Clone: "deals damage 3 times every 1.25s and damage every 5s to a single foe. If an enemy unit is out of range, the clone pounces on the target"
            new()
            {
                SummonSpell4BaseId  = 23337,
                AttackSpell4BaseIds = [23348],    // 38984 Stalker - Clone Pet - Slash - Base
                SpecialAttacks      =
                [
                    new(23353, TimeSpan.FromSeconds(5d)),       // 38989 Stalker Clone Pet - Precision Strike
                    new(23613, TimeSpan.FromSeconds(15d), true) // 39273 Stalker Clone Pet - Pounce w/ Snare, 15s spell cooldown
                ]
            },
            // Stalker Don't Call it a Comeback: the same clone
            new()
            {
                SummonSpell4BaseId  = 25218,
                AttackSpell4BaseIds = [23348],
                SpecialAttacks      =
                [
                    new(23353, TimeSpan.FromSeconds(5d)),
                    new(23613, TimeSpan.FromSeconds(15d), true)
                ]
            },
            // Soldier Summon Reinforcements: a Soldier and a Gunner with basic and special attacks,
            // the tooltip doesn't give attack rates so basic attacks use the creature combat AI rate and specials every 10s
            new()
            {
                SummonSpell4BaseId = 22101,
                CreatureAttacks    = new()
                {
                    [33432] = SoldierReinforcementAttacks,
                    [33631] = SoldierReinforcementAttacks,
                    [33632] = SoldierReinforcementAttacks,
                    [33447] = GunnerReinforcementAttacks,
                    [33634] = GunnerReinforcementAttacks,
                    [33635] = GunnerReinforcementAttacks
                }
            },
            // Settler Summon Turret: "Summon in a Turret at your location that deals damage to any target in front of it"
            new()
            {
                SummonSpell4BaseId  = 22279,
                Stationary          = true,
                AttackSpell4BaseIds = [22285]     // 36824 Settler Turret - Ranged - Auto-Attack, 1s cast
            }
        }.ToDictionary(i => i.SummonSpell4BaseId);

        private static (uint[], CombatPetSpecialAttack[]) SoldierReinforcementAttacks =>
        (
            [
                22109,                            // 36615 Summon Reinforcements - Basic Attack - R2L
                22110                             // 36616 Summon Reinforcements - Basic Attack - L2R
            ],
            [new(22122, TimeSpan.FromSeconds(10d))] // 36629 Summon Reinforcements - Melee Special Attack, knockdown
        );

        private static (uint[], CombatPetSpecialAttack[]) GunnerReinforcementAttacks =>
        (
            [22120],                              // 36626 Summon Reinforcements - Basic Attack
            [new(22927, TimeSpan.FromSeconds(10d))] // 38419 Summon Reinforcements - Ranged Special Attack, knockdown
        );

        /// <summary>
        /// Return the auto attacks and special attacks of supplied creature.
        /// </summary>
        public (uint[] AttackSpell4BaseIds, CombatPetSpecialAttack[] SpecialAttacks) GetAttacks(uint creatureId)
        {
            return CreatureAttacks.TryGetValue(creatureId, out var attacks) ? attacks : (AttackSpell4BaseIds, SpecialAttacks);
        }

        /// <summary>
        /// Return <see cref="CombatPetAbilityInfo"/> for supplied summon spell base id.
        /// </summary>
        public static CombatPetAbilityInfo Get(uint summonSpell4BaseId)
        {
            return infos.TryGetValue(summonSpell4BaseId, out CombatPetAbilityInfo info) ? info : null;
        }

        /// <summary>
        /// Return <see cref="CombatPetAbilityInfo"/> for supplied bot ability (pet switch) base id.
        /// </summary>
        public static CombatPetAbilityInfo GetByPetSwitch(uint petSwitchSpell4BaseId)
        {
            return infos.Values.FirstOrDefault(i => i.PetSwitchSpell4BaseId == petSwitchSpell4BaseId);
        }
    }
}
