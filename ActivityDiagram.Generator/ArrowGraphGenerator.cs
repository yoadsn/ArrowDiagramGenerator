using ActivityDiagram.Generator.Model;
using ActivityDiagram.Contracts.Model.Activities;
using ActivityDiagram.Contracts.Model.Graph;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Generator
{
    public class ArrowGraphGenerator
    {
        private IEnumerable<ActivityDependency> activityDependencies;
        private Dictionary<int, Activity> activitiesDictionary;
        private BidirectionalGraph<InternalEventVertex, InternalActivityEdge> arrowGraph;
        private Dictionary<Tuple<int, int>, int> edgesIdsMap;
        private Dictionary<string, int> verticeIdsMap;
        int edgesNextId = 0;
        int verticeNextId = 0;

        public ArrowGraphGenerator(IEnumerable<ActivityDependency> activityDependencies)
        {
            this.activityDependencies = activityDependencies;
            this.activitiesDictionary = this.activityDependencies.ToDictionary(dep => dep.Activity.Id, dep => dep.Activity);
        }

        public ActivityArrowGraph GenerateGraph()
        {
            InitializeInternalStructures();

            var nodeGraph = CreateActivityNodeGraphFromProject();
            var nodeGraphReduction = nodeGraph.ComputeTransitiveReduction();
            arrowGraph = ExplodeActivityNodeGraphToArrowGraph(nodeGraphReduction);
            RedirectArrowGraph();
            DropRedundantArrows();
            return CreateResultActivityArrowGraph();
        }

        private void InitializeInternalStructures()
        {
            edgesIdsMap = new Dictionary<Tuple<int, int>, int>();
            verticeIdsMap = new Dictionary<string, int>();
            edgesNextId = 0;
            verticeNextId = 0;
        }

        private BidirectionalGraph<int, SEdge<int>> CreateActivityNodeGraphFromProject()
        {
            return activityDependencies.
                SelectMany(act =>
                    act.Predecessors.Select(pred =>
                        new SEdge<int>(
                            pred, // Source
                            act.Activity.Id // Target
                            ))).ToBidirectionalGraph<int, SEdge<int>>();
        }

        private BidirectionalGraph<InternalEventVertex, InternalActivityEdge> ExplodeActivityNodeGraphToArrowGraph(BidirectionalGraph<int, SEdge<int>> nodeGraph)
        {
            var arrowGraph = new BidirectionalGraph<InternalEventVertex, InternalActivityEdge>();

            // Go over all vertice - add them as new activity edges.
            // activity vertex names are important for resuse when adding the edges.
            foreach (var vertex in nodeGraph.Vertices)
            {
                bool isCritical = activitiesDictionary[vertex].IsCritical;

                var startNode = InternalEventVertex.Create(vertex, ActivityVertexType.ActivityStart, isCritical);
                var endNode = InternalEventVertex.Create(vertex, ActivityVertexType.ActivityEnd, isCritical);
                arrowGraph.AddVertex(startNode);
                arrowGraph.AddVertex(endNode);

                InternalActivityEdge activityEdge = new InternalActivityEdge(startNode, endNode, isCritical, vertex);

                arrowGraph.AddEdge(activityEdge);
            }

            // Go over all edges - convert them to dummy edges.
            // Make sure connections are maintained.
            foreach (var edge in nodeGraph.Edges)
            {
                bool isSourceCritical = activitiesDictionary[edge.Source].IsCritical;
                bool isTargetCritical = activitiesDictionary[edge.Target].IsCritical;

                InternalActivityEdge activityEdge = new InternalActivityEdge(
                    InternalEventVertex.Create(edge.Source, ActivityVertexType.ActivityEnd, isSourceCritical),
                    InternalEventVertex.Create(edge.Target, ActivityVertexType.ActivityStart, isTargetCritical),
                    isSourceCritical && isTargetCritical);

                arrowGraph.AddEdge(activityEdge);
            }

            return arrowGraph;
        }

        /// <summary>
        /// This method implements the redirection phase.
        /// This phase looks at every activity end event and tries to pull as much dependency arrows of it's dependents
        /// to pass via it.
        /// This is a greedy process, where when ever a dependency is shared among all dependent nodes, it is redirected to point
        /// at the next vertex instead of on each one of them individually.
        /// </summary>
        private void RedirectArrowGraph()
        {
            // Go over every vertex
            foreach (var nexusVertex in arrowGraph.Vertices)
            {
                // Nexus vertices are end events of activities
                if (nexusVertex.Type == ActivityVertexType.ActivityEnd)
                {
                    // Get all the edges going out of the nexus
                    IEnumerable<InternalActivityEdge> nexusOutEdges;
                    if (arrowGraph.TryGetOutEdges(nexusVertex, out nexusOutEdges))
                    {
                        var depndents = nexusOutEdges.Select(edge => edge.Target);
                        var commonDependencies = GetCommonDependenciesForNodeGroup(depndents);

                        // Aside from the obvious common dependency (the nexus vertex) redirect all dependencies to the nexus vertex
                        foreach (var commonDependency in commonDependencies.Where(d => d != nexusVertex))
                        {
                            RedirectCommonDependencyToNexus(nexusVertex, depndents, commonDependency);
                        }
                    }
                }
            }
        }

        private HashSet<InternalEventVertex> GetCommonDependenciesForNodeGroup(IEnumerable<InternalEventVertex> vertices)
        {
            var commonDependencies = new HashSet<InternalEventVertex>();

            // Find the common dependencies for all target vertice
            foreach (var vertex in vertices)
            {
                IEnumerable<InternalActivityEdge> dependenciesOfTarget;
                if (arrowGraph.TryGetInEdges(vertex, out dependenciesOfTarget))
                {
                    // Always work with dependencies which are dummies - since activities cannot/should not be redirected.
                    dependenciesOfTarget = dependenciesOfTarget.Where(dep => !dep.ActivityId.HasValue);

                    if (commonDependencies.Count == 0)
                    {
                        foreach (var dependency in dependenciesOfTarget)
                        {
                            commonDependencies.Add(dependency.Source);
                        }
                    }
                    else
                    {
                        commonDependencies.IntersectWith(dependenciesOfTarget.Select(d => d.Source).AsEnumerable());
                    }
                }
                // Else can never happen - the out edge for the current vertice is the in edge of the dependent
                // so at least once exists.
            }

            return commonDependencies;
        }

        private void RedirectCommonDependencyToNexus(InternalEventVertex nexusVertex, IEnumerable<InternalEventVertex> depndents, InternalEventVertex commonDependency)
        {
            bool isAddedEdgeCritical = false;
            IEnumerable<InternalActivityEdge> edgesOutOfDependency;
            if (arrowGraph.TryGetOutEdges(commonDependency, out edgesOutOfDependency))
            {
                // Remove the edges between the dependency and the dependents of the nexus vertex
                var edgesToRemove = edgesOutOfDependency.Where(e => depndents.Contains(e.Target)).ToList();
                foreach (var edgeToRemove in edgesToRemove)
                {
                    arrowGraph.RemoveEdge(edgeToRemove);

                    // Replacing even one critical edge means the new edge would be also critical
                    isAddedEdgeCritical = isAddedEdgeCritical || edgeToRemove.IsCritical;
                }
            }
            // Else should never happen

            // This dependency should point at nexus vertex
            var edgeToAdd = new InternalActivityEdge(commonDependency, nexusVertex, isAddedEdgeCritical);
            arrowGraph.AddEdge(edgeToAdd);
        }
        
        /// <summary>
        /// This methods implements the Drop phase which reduces the graph complexity by removing dummy edges which do not add new information.
        /// Those dummy edeges connect a node which has only one input or only one output which is the dummy edge
        /// </summary>
        private void DropRedundantArrows()
        {
            bool dummyEdgeRemovedOnIteration = true;

            while (dummyEdgeRemovedOnIteration)
            {
                // Get all the current dummy edges in the graph
                var nonActivityEdges = arrowGraph.Edges.Where(e => !e.ActivityId.HasValue).ToList();

                foreach (var edge in nonActivityEdges)
                {
                    // Only remove one edge at a time - then, need to reevaluate the graph.
                    if (dummyEdgeRemovedOnIteration = TryRemoveDummyEdge(edge)) break;

                }
            }
        }

        private bool TryRemoveDummyEdge(InternalActivityEdge edge)
        {
            bool edgeRemoved = false;

            // If this is a single edge out or a single edge in - it adds no information to the graph and can be merged.
            var outDegree = arrowGraph.OutDegree(edge.Source);
            var inDegree = arrowGraph.InDegree(edge.Target);
          
            // Remove the vertex which has no other edges connected to it
            if (outDegree == 1)
            {
                IEnumerable<InternalActivityEdge> allIncoming;
                if (!arrowGraph.TryGetInEdges(edge.Source, out allIncoming))
                {
                    allIncoming = new List<InternalActivityEdge>();
                }

                bool abortMerge = WillParallelEdgesBeCreated(allIncoming, null, edge.Target);

                if (!abortMerge)
                {
                    // Add the edges with the new source vertex
                    // And remove the old edges
                    foreach (var incomingEdge in allIncoming.ToList())
                    {
                        arrowGraph.AddEdge(new InternalActivityEdge(incomingEdge.Source, edge.Target, incomingEdge.IsCritical, incomingEdge.ActivityId));
                        arrowGraph.RemoveEdge(incomingEdge);
                    }

                    // Remove the edge which is no longer needed
                    arrowGraph.RemoveEdge(edge);

                    // Now remove the vertex which is no longer needed
                    arrowGraph.RemoveVertex(edge.Source);

                    edgeRemoved = true;
                }
            }
            else if (inDegree == 1)
            {
                IEnumerable<InternalActivityEdge> allOutgoing;
                if (!arrowGraph.TryGetOutEdges(edge.Target, out allOutgoing))
                {
                    allOutgoing = new List<InternalActivityEdge>();
                }

                bool abortMerge = WillParallelEdgesBeCreated(allOutgoing, edge.Source, null);

                if (!abortMerge)
                {
                    // Add the edges with the new source vertex
                    // And remove the old edges
                    foreach (var outgoingEdge in allOutgoing.ToList())
                    {
                        arrowGraph.AddEdge(new InternalActivityEdge(edge.Source, outgoingEdge.Target, outgoingEdge.IsCritical, outgoingEdge.ActivityId));
                        arrowGraph.RemoveEdge(outgoingEdge);
                    }

                    // Remove the edge which is no longer needed
                    arrowGraph.RemoveEdge(edge);

                    // Now remove the vertex which is no longer needed
                    arrowGraph.RemoveVertex(edge.Target);

                    edgeRemoved = true;
                }
            }
            

            return edgeRemoved;
        }

        private bool WillParallelEdgesBeCreated(IEnumerable<InternalActivityEdge> plannedEdgesToReplace, InternalEventVertex plannedNewSource, InternalEventVertex plannedNewTarget)
        {
            bool abortMerge = false;
            foreach (var edge in plannedEdgesToReplace.ToList())
            {
                var sourceToTestWith = plannedNewSource ?? edge.Source;
                var targetToTestWith = plannedNewTarget ?? edge.Target;

                InternalActivityEdge dummy;
                if (arrowGraph.TryGetEdge(sourceToTestWith, targetToTestWith, out dummy))
                {
                    abortMerge = abortMerge || true;
                }
            }

            return abortMerge;
        }

        private ActivityArrowGraph CreateResultActivityArrowGraph()
        {
            var activityArrowGraph = new ActivityArrowGraph();

            foreach (var edge in arrowGraph.Edges)
            {
                var sourceVertex = CreateVertexEvent(edge.Source, arrowGraph.InDegree(edge.Source), arrowGraph.OutDegree(edge.Source));
                var targetVertex = CreateVertexEvent(edge.Target, arrowGraph.InDegree(edge.Target), arrowGraph.OutDegree(edge.Target));

                Activity edgeActivity;

                TryGetActivity(edge, out edgeActivity);

                activityArrowGraph.AddEdge(CreateActivityEdge(sourceVertex, targetVertex, edgeActivity, edge.IsCritical));
            }

            return activityArrowGraph;
        }

        private ActivityEdge CreateActivityEdge(EventVertex source, EventVertex target, Activity edgeActivity, bool isCritical)
        {
            var edgeUniqueKey = new Tuple<int, int>(source.Id, target.Id);
            int activityEdgeId;
            if (!edgesIdsMap.TryGetValue(edgeUniqueKey, out activityEdgeId))
            {
                edgesIdsMap[edgeUniqueKey] = activityEdgeId = edgesNextId;
                edgesNextId++;
            }

            return new ActivityEdge(activityEdgeId, source, target, edgeActivity, isCritical);
        }

        private EventVertex CreateVertexEvent(InternalEventVertex vertex, int inDegree, int outDegree)
        {
            int eventVertexId;
            if (!verticeIdsMap.TryGetValue(vertex.Id, out eventVertexId))
            {
                verticeIdsMap[vertex.Id] = eventVertexId = verticeNextId;
                verticeNextId++;
            }

            EventVertex eventVertex;
            if (inDegree == 0)
            {
                eventVertex = EventVertex.CreateGraphStart(eventVertexId);
            }
            else if (outDegree == 0)
            {
                eventVertex = EventVertex.CreateGraphEnd(eventVertexId);
            }
            else
            {
                eventVertex = EventVertex.Create(eventVertexId);
            }
            return eventVertex;
        }

        private bool TryGetActivity(InternalActivityEdge edge, out Activity activity)
        {
            activity = null;
            if (edge.ActivityId.HasValue && activitiesDictionary.ContainsKey(edge.ActivityId.Value))
            {
                activity = activitiesDictionary[edge.ActivityId.Value];
                return true;
            }

            return false;
        }

        private bool TryGetActivity(InternalEventVertex vertex, out Activity activity)
        {
            activity = null;
            return activitiesDictionary.TryGetValue(vertex.ActivityId, out activity);
        }
    }
}
