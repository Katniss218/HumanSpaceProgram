using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Vessels
{
    public class PartGraph : MonoBehaviour, IPartGraph
    {
        protected VesselPart[] _parts;
        protected VesselIsland[] _islands;
        protected VesselAttachmentGraph _attachments;

        protected readonly FComponentCache _componentCache = new FComponentCache();

        public IReadonlyVesselAttachmentGraph Attachments => _attachments;
        public IEnumerable<IReadonlyVesselIsland> Islands => _islands;
        public IEnumerable<VesselPart> Parts => _parts;

        public event Action<IPartGraph> OnModified;

        public IReadOnlyList<T> GetFComponents<T>() where T : class
        {
            return _componentCache.Get<T>();
        }

        public virtual void SetGraph(VesselAttachmentGraph graph)
        {
            _attachments = graph;
            RebuildIslands();
            RecalculatePartCache();
            OnModified?.Invoke(this);
        }

        public virtual void RecalculatePartCache()
        {
            if (_attachments == null)
            {
                _parts = Array.Empty<VesselPart>();
                _componentCache.Clear();
                return;
            }

            _parts = _attachments.Nodes.ToArray();

            foreach (var part in _parts)
            {
                part.Graph = this;
            }

            _componentCache.Clear();
            _componentCache.AddParts(_parts);
        }

        protected virtual void RebuildIslands()
        {
            if (_attachments == null)
            {
                _islands = Array.Empty<VesselIsland>();
                return;
            }

            // Group parts into islands based on Rigid connections
            List<VesselIsland> islands = new List<VesselIsland>();
            HashSet<VesselPart> visited = new HashSet<VesselPart>();

            foreach (var part in _attachments.Nodes)
            {
                if (visited.Contains(part))
                    continue;

                List<VesselPart> islandParts = new List<VesselPart>();
                Queue<VesselPart> queue = new Queue<VesselPart>();

                queue.Enqueue(part);
                visited.Add(part);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    islandParts.Add(current);

                    var connected = _attachments.GetEdges(current);
                    foreach (var edge in connected)
                    {
                        if (edge.Type == AttachmentEdgeType.Rigid)
                        {
                            var neighbor = edge.Target;
                            if (!visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                VesselIsland island = new VesselIsland(islandParts);
                islands.Add(island);
            }

            _islands = islands.ToArray();
        }
    }
}
