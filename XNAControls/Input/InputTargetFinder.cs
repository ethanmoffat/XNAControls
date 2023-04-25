using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace XNAControls.Input
{
    public static class InputTargetFinder
    {
        public static IEnumerable<IEventReceiver> GetMouseOverEventTargetControl(IEnumerable<IGameComponent> collection, Point position)
        {
            var targets = collection
                .OfType<IEventReceiver>()
                .Where(IsValidMouseOverTarget)
                .ToList();

            var toProcess = new Queue<IXNAControl>(targets.OfType<IXNAControl>());
            while (toProcess.Any())
            {
                var parent = toProcess.Dequeue();

                var childTargets = parent.ChildControls.Where(IsValidMouseOverTarget);
                foreach (var child in childTargets)
                    toProcess.Enqueue(child);

                if (!targets.Contains(parent))
                    targets.Add(parent);
            }

            return targets;
        }

        public static IEventReceiver GetMouseDownEventTargetControl(IEnumerable<IGameComponent> collection, Point position, bool includeChildren = true)
        {
            var targets = collection
                .OfType<IEventReceiver>()
                .Where(x => IsValidMouseDownTarget(x, position))
                .ToList();

            if (includeChildren)
            {
                var toProcess = new Queue<IXNAControl>(targets.OfType<IXNAControl>());
                while (toProcess.Any())
                {
                    var parent = toProcess.Dequeue();

                    var childtargets = parent.ChildControls.Where(x => IsValidMouseDownTarget(x, position));
                    foreach (var child in childtargets)
                        toProcess.Enqueue(child);

                    if (!targets.Contains(parent))
                        targets.Add(parent);
                }
            }

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

        private static bool IsValidMouseOverTarget(IEventReceiver component)
        {
            return component as IDrawable == null || ((IDrawable)component).Visible;
        }

        private static bool IsValidMouseDownTarget(IEventReceiver eventReceiver, Point position)
        {
            return eventReceiver.EventArea.Contains(position) && AllParentsVisible(eventReceiver);
        }

        private static bool AllParentsVisible(IEventReceiver eventReceiver)
        {
            var control = eventReceiver as IXNAControl;
            if (control == null)
            {
                var drawable = eventReceiver as IDrawable;
                return drawable?.Visible ?? true;
            }

            var visible = true;
            var c = control;
            while ((c = c.ImmediateParent) != null)
                visible &= c.Visible;

            return control.Visible && visible;
        }
    }
}
