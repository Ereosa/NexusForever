using System.Numerics;
using NexusForever.Game.Abstract.Entity.Movement.Generator;
using NexusForever.Game.Abstract.Map;

namespace NexusForever.Game.Entity.Movement.Generator
{
    public class DirectMovementGenerator : IDirectMovementGenerator
    {
        public Vector3 Begin { get; set; }
        public Vector3 Final { get; set; }
        public IBaseMap Map { get; set; }

        public List<Vector3> CalculatePath()
        {
            return GroundPathBuilder.Build(Map, Begin, Final, 0f);
        }
    }
}
