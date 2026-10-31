namespace NexusForever.Game.Abstract.Entity
{
    public interface INonPlayerEntity : ICreatureEntity
    {
        IVendorInfo VendorInfo { get; }

        /// <summary>
        /// Make the entity a vendor with the supplied stock, for units that are spawned by the server instead of coming from a database entity.
        /// </summary>
        /// <remarks>
        /// Must be called before the entity is added to the map, the client is told about the vendor with the create flags.
        /// </remarks>
        void InitialiseVendor(IVendorInfo vendorInfo);
    }
}
