namespace NexusForever.Game.Static.Quest
{
    /// <summary>
    /// What a target-less activate objective asks for according to its text.
    /// </summary>
    public enum QuestTextTrigger
    {
        None,
        /// <summary>
        /// Equip the item (eg: "Equip the $(item=12631)").
        /// </summary>
        EquipItem,
        /// <summary>
        /// Buy the item from a vendor (eg: "Buy a $(item=54409) from $(creature=47172)").
        /// </summary>
        BuyItem,
        /// <summary>
        /// Talk to the creature (eg: "Learn more about $(creature=48664)").
        /// </summary>
        TalkTo,
        /// <summary>
        /// Complete a public event, optionally in the named zone (eg: "Complete a Levian Bay event").
        /// </summary>
        PublicEvent,
        /// <summary>
        /// Complete the named adventure, dungeon, expedition or quest (eg: "Complete Veteran Bay of Betrayal", "Complete a Prime Dungeon").
        /// </summary>
        CompleteContent
    }
}
