using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace XNAControls.Input
{
    public class InputTargetFinder
    {
        public IEventReceiver GetMouseEventTargetControl(IEnumerable<IGameComponent> collection, Point position)
        {
            var searchCollection = collection.Concat(collection.OfType<IXNAControl>().SelectMany(x => x.ChildControls));

            var targets = searchCollection.OfType<IEventReceiver>()
                .Where(x => x.EventArea.Contains(position))
                .ToList();

            if (!targets.Any()) return null;
            if (targets.Count == 1) return targets.Single();

            var max = targets.Max(x => x.ZOrder);
            var maxTargets = targets.Where(x => x.ZOrder == max).ToList();

            if (maxTargets.Count == 1) return maxTargets.Single();

            // If multiple controls are updateable, the tie breaker should be the lowest UpdateOrder (which component updates first)
            if (maxTargets.All(x => x is IUpdateable))
            {
                var updateables = maxTargets.OfType<IUpdateable>();
                var minValue = updateables.Min(x => x.UpdateOrder);

                if (!updateables.All(x => x.UpdateOrder == minValue))
                {
                    return maxTargets.MinBy(x => ((IUpdateable)x).UpdateOrder);
                }
            }

            return maxTargets.Last();
        }
    }
}
