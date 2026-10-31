namespace NexusForever.Database.World.Model
{
    /// <summary>
    /// Vendor stock for a creature, used by units that are spawned by the server (e.g. settler improvements) instead of placed in a zone file.
    /// </summary>
    public class CreatureVendorModel
    {
        public uint CreatureId { get; set; }
        public float BuyPriceMultiplier { get; set; }
        public float SellPriceMultiplier { get; set; }
    }
}
