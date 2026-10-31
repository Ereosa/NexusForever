using System.Numerics;
using NexusForever.Game.Abstract.Entity.Movement.Generator;
using NexusForever.Game.Abstract.Map;
using NexusForever.Shared;

namespace NexusForever.Game.Entity.Movement.Generator
{
    public class RandomMovementGenerator : IRandomMovementGenerator
    {
        public Vector3 Begin { get; set; }
        public Vector3 Leash { get; set; }
        public float Range { get; set; }
        public IBaseMap Map { get; set; }

        public List<Vector3> CalculatePath()
        {
            Vector3 final = Leash.GetRandomPoint2D(Range);
            final.Y = Leash.Y;

            return GroundPathBuilder.Build(Map, Begin, final, 0f);
        }
    }
}
