using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement.Command;
using NexusForever.Game.Abstract.Entity.Movement.Command.Position;

namespace NexusForever.Script.Template
{
    public interface IWorldEntityScript : IGridEntityScript
    {
        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        void OnActivateSuccess(IPlayer activator)
        {
        }

        /// <summary>
        /// Invoked when a spell of <paramref name="source"/> sends a ravel signal to <see cref="IWorldEntity"/> or to the entities around it.
        /// </summary>
        /// <remarks>
        /// Ravel signals (RavelSignal spell effect, signal id in DataBits01) were how retail spells talked to server side event scripts,
        /// eg: the Data Mining buttons of the Darkwhisper Database send signals 12511 to 12513.
        /// </remarks>
        void OnRavelSignal(IUnitEntity source, uint signal)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> talks to <see cref="IWorldEntity"/>.
        /// </summary>
        /// <returns>True if the script handled the interaction, this skips the default talk objective updates and dialog.</returns>
        bool OnTalkTo(IPlayer player)
        {
            return false;
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> tries to board <see cref="IWorldEntity"/>, a vehicle spawned in the world (eg: the capital tour rides).
        /// </summary>
        void OnBoard(IPlayer player)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is unsuccessfully activated by <see cref="IPlayer"/>.
        /// </summary>
        void OnActivateFail(IPlayer activator)
        {
        }

        /// <summary>
        /// Invoked when an <see cref="IEntityCommand"/> has finialised for <see cref="IWorldEntity"/>.
        /// </summary>
        void OnEntityCommandFinalise(IEntityCommand command)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> enters a zone.
        /// </summary>
        void OnEnterZone(IWorldEntity entity, uint zone)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> summons another <see cref="IWorldEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        void OnSummon(IWorldEntity summoned)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> unsummons another <see cref="IWorldEntity"/>.
        /// </summary>
        void OnUnsummon(IWorldEntity summoned)
        {
        }
    }
}
