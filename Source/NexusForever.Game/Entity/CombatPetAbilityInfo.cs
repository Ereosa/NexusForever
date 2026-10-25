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
        /// Spells cast in order on the pet's target every SummonPet attack interval.
        /// </summary>
        public uint[] AttackSpell4BaseIds { get; init; } = [];

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
                AttackSpell4BaseIds   = [20491],  // 34521 Engineer - Pet - Artillery Bot - Auto Attack
                PetSwitchSpell4BaseId = 34051,    // 51365 Engineer - Artillery Bot - Barrage - Base
                AbilityAction         = CombatPetAbilityAction.OwnerCast,
                AbilitySpell4BaseIds  = [20884]   // 35123 Engineer - Artillery Bot - Player Cast - Barrage
            },
            // Repairbot: "deals damage to 1 foe and restores shield to you every 3s", the attack proxies 70622 Auto Heal
            new()
            {
                SummonSpell4BaseId    = 26998,
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
                AttackSpell4BaseIds   = [21194],  // 35503 Engineer - Pet - Bruiser Bot - Crush - Auto Attack
                PetSwitchSpell4BaseId = 21192,    // 35501 Engineer - Bruiser Bot - Blitz - Base
                AbilityAction         = CombatPetAbilityAction.OwnerCast,
                AbilitySpell4BaseIds  = [34410],  // 52419 Engineer - Bruiser Bot - Player Cast - Blitz
                FollowUpSpells        = new()
                {
                    // "commands your Bruiser Bot to leap to the target, dealing damage and Taunting": 51319 leaps (500ms flight), 51322 is the damage and taunt
                    [34009] = (TimeSpan.FromMilliseconds(500d), [34012])
                }
            }
        }.ToDictionary(i => i.SummonSpell4BaseId);

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
