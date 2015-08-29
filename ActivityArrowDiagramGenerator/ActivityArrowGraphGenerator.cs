using ActivityArrowDiagramGenerator.Model;
using ActivityDiagram.Contracts.Model.Activities;
using ActivityDiagram.Contracts.Model.Graph;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator
{
    public class ActivityArrowGraphGenerator
    {
        private IEnumerable<ActivityDependency> activityDependencies;
        private Dictionary<int, Activity> activitiesDictionary;

        public ActivityArrowGraphGenerator(IEnumerable<ActivityDependency> activityDependencies)
        {
            this.activityDependencies = activityDependencies;
            this.activitiesDictionary = this.activityDependencies.ToDictionary(dep => dep.Activity.Id, dep => dep.Activity);
        }

        public ActivityArrowGraph GenerateGraph()
        {
            var nodeGraph = CreateActivityNodeGraphFromProject();
            var reduction = nodeGraph.ComputeTransitiveReduction();
            var activityArrowDiagram = GenerateADGraph(reduction);
            RedirectADGraph(activityArrowDiagram);
            MergeADGraph(activityArrowDiagram);
            return CreateActivityArrowGraph(activityArrowDiagram);
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

        private static BidirectionalGraph<ADVertex, ADEdge<ADVertex>> GenerateADGraph(BidirectionalGraph<int, SEdge<int>> nodeGraph)
        {
            var adGraph = new BidirectionalGraph<ADVertex, ADEdge<ADVertex>>();

            // Go over all vertice - add them as a new activity edges.
            // activity vertex name are important for resuse when adding the edges.
            foreach (var vertex in nodeGraph.Vertices)
            {
                var startNode = ADVertex.New(vertex, ActivityVertexType.ActivityStart);
                var endNode = ADVertex.New(vertex, ActivityVertexType.ActivityEnd);
                adGraph.AddVertex(startNode);
                adGraph.AddVertex(endNode);

                ADEdge<ADVertex> activityEdge = new ADEdge<ADVertex>(startNode, endNode, vertex);

                adGraph.AddEdge(activityEdge);
            }

            // Go over all edges - convert them to activity edges.
            // Make sure connections are maintained.
            foreach (var edge in nodeGraph.Edges)
            {
                ADEdge<ADVertex> activityEdge = new ADEdge<ADVertex>(
                    ADVertex.New(edge.Source, ActivityVertexType.ActivityEnd),
                    ADVertex.New(edge.Target, ActivityVertexType.ActivityStart));

                adGraph.AddEdge(activityEdge);
            }

            return adGraph;
        }

        private static void RedirectADGraph(BidirectionalGraph<ADVertex, ADEdge<ADVertex>> adGraph)
        {
            // Go over every vertex
            foreach (var pivotVertex in adGraph.Vertices)
            {
                // We only care at the moment about activity end vertice
                if (pivotVertex.Type == ActivityVertexType.ActivityEnd)
                {
                    // Get all the edges going out of this vertex
                    IEnumerable<ADEdge<ADVertex>> foundOutEdges;
                    if (adGraph.TryGetOutEdges(pivotVertex, out foundOutEdges))
                    {
                        var commonDependenciesForAllTargets = new HashSet<ADVertex>();
                        // Find the common dependencies for all target vertice
                        foreach (var outEdge in foundOutEdges)
                        {
                            var target = outEdge.Target;
                            if (target.Type == ActivityVertexType.ActivityStart)
                            {
                                IEnumerable<ADEdge<ADVertex>> dependenciesOfTarget;
                                if (adGraph.TryGetInEdges(target, out dependenciesOfTarget))
                                {
                                    if (commonDependenciesForAllTargets.Count == 0)
                                    {
                                        foreach (var dependency in dependenciesOfTarget)
                                        {
                                            commonDependenciesForAllTargets.Add(dependency.Source);
                                        }
                                    }
                                    else
                                    {
                                        commonDependenciesForAllTargets.IntersectWith(dependenciesOfTarget.Select(d => d.Source).AsEnumerable());
                                    }
                                }
                                // Else can never happen - the out edge for the current vertice is the in edge of the dependent
                                // so at least once exists.
                            }
                            else // That means the inspected vertice has a dependent which is not an activity (need to inspect these cases)
                            {
                            }
                        }

                        // Now, if we have some common dependncies of all targets which are not the current vertex - they should be redirected
                        foreach (var commonDependency in commonDependenciesForAllTargets.Where(d => d != pivotVertex))
                        {
                            IEnumerable<ADEdge<ADVertex>> edgesOutOfDependency;
                            if (adGraph.TryGetOutEdges(commonDependency, out edgesOutOfDependency))
                            {
                                var depndents = foundOutEdges.Select(e => e.Target);

                                // This dependency should no longer point at the dependents of this vertex
                                var edgesToRemove = edgesOutOfDependency.Where(e => depndents.Contains(e.Target)).ToList();

                                foreach (var edgeToRemove in edgesToRemove)
                                {
                                    adGraph.RemoveEdge(edgeToRemove);
                                }
                            }
                            // Else should never happen

                            // This dependency should point at this vertex
                            var edgeToAdd = new ADEdge<ADVertex>(commonDependency, pivotVertex);
                            adGraph.AddEdge(edgeToAdd);
                        }
                    }
                }
            }
        }

        private static void MergeADGraph(BidirectionalGraph<ADVertex, ADEdge<ADVertex>> adGraph)
        {
            // Go over all non-activity edges - and try to merge them
            var nonActivityEdges = adGraph.Edges.Where(e => !e.ActivityId.HasValue).ToList();
            foreach (var edge in nonActivityEdges)
            {
                // If this is a single edge out or a single edge in - it adds no information to the graph and can be merged.
                var outDegree = adGraph.OutDegree(edge.Source);
                var inDegree = adGraph.InDegree(edge.Target);
                if (outDegree == 1 || inDegree == 1)
                {
                    // Remove the vertex which has no other edges connected to it
                    if (outDegree == 1 && inDegree != 1)
                    {
                        IEnumerable<ADEdge<ADVertex>> allIncoming;
                        if (!adGraph.TryGetInEdges(edge.Source, out allIncoming))
                        {
                            allIncoming = new List<ADEdge<ADVertex>>();
                        }

                        bool abortMerge = false;

                        // Sanity check - don't make parallel edges (can have better huristic for this?)
                        foreach (var incomingEdge in allIncoming.ToList()) {
                            ADEdge<ADVertex> dummy;
                            if (adGraph.TryGetEdge(incomingEdge.Source, edge.Target, out dummy))
                            {
                                abortMerge = true;
                            }
                        }

                        if (!abortMerge)
                        {
                            // Add the edges with the new source vertex
                            // And remove the old edges
                            foreach (var incomingEdge in allIncoming.ToList())
                            {
                                adGraph.AddEdge(new ADEdge<ADVertex>(incomingEdge.Source, edge.Target, incomingEdge.ActivityId));
                                adGraph.RemoveEdge(incomingEdge);
                            }

                            // Rmove the edge which is no longer needed
                            adGraph.RemoveEdge(edge);

                            // Now remove the vertex which was merged
                            adGraph.RemoveVertex(edge.Source);
                        }
                    }
                    else
                    {
                        IEnumerable<ADEdge<ADVertex>> allOutgoing;
                        if (!adGraph.TryGetOutEdges(edge.Target, out allOutgoing))
                        {
                            allOutgoing = new List<ADEdge<ADVertex>>();
                        }

                        bool abortMerge = false;

                        // Sanity check - don't make parallel edges (can have better huristic for this?)
                        foreach (var incomingEdge in allOutgoing.ToList())
                        {
                            ADEdge<ADVertex> dummy;
                            if (adGraph.TryGetEdge(edge.Source, incomingEdge.Target, out dummy))
                            {
                                abortMerge = true;
                            }
                        }

                        if (!abortMerge)
                        {
                            // Add the edges with the new source vertex
                            // And remove the old edges
                            foreach (var outgoingEdge in allOutgoing.ToList())
                            {
                                adGraph.AddEdge(new ADEdge<ADVertex>(edge.Source, outgoingEdge.Target, outgoingEdge.ActivityId));
                                adGraph.RemoveEdge(outgoingEdge);
                            }

                            // Rmove the edge which is no longer needed
                            adGraph.RemoveEdge(edge);

                            // Now remove the vertex which was merged
                            adGraph.RemoveVertex(edge.Target);
                        }
                    }
                }
            }
        }

        private ActivityArrowGraph CreateActivityArrowGraph(BidirectionalGraph<ADVertex, ADEdge<ADVertex>> graph)
        {
            var edgesNextId = 0;
            var edgesIdsMap = new Dictionary<Tuple<int, int>, int>();
            var verticeNextId = 0;
            var verticeIdsMap = new Dictionary<string, int>();

            var activityArrowGraph = new ActivityArrowGraph();

            foreach (var edge in graph.Edges)
            {
                var sourceVertex = CreateVertexEvent(edge.Source, ref verticeNextId, verticeIdsMap);
                var targetVertex = CreateVertexEvent(edge.Target, ref verticeNextId, verticeIdsMap);

                Activity edgeActivity;

                TryGetActivity(edge, out edgeActivity);

                activityArrowGraph.AddEdge(CreateActivityEdge(sourceVertex, targetVertex, edgeActivity, ref edgesNextId, edgesIdsMap));
            }

            return activityArrowGraph;
        }

        private ActivityEdge CreateActivityEdge(EventVertex source, EventVertex target, Activity edgeActivity, ref int edgesNextId, Dictionary<Tuple<int, int>, int> edgesIdsMap)
        {
            var edgeUniqueKey = new Tuple<int, int>(source.Id, target.Id);
            int activityEdgeId;
            if (!edgesIdsMap.TryGetValue(edgeUniqueKey, out activityEdgeId))
            {
                edgesIdsMap[edgeUniqueKey] = activityEdgeId = edgesNextId;
                edgesNextId++;
            }

            if (edgeActivity != null)
            {
                return new ActivityEdge(activityEdgeId, source, target, edgeActivity);
            }
            else
            {
                return new ActivityEdge(activityEdgeId, source, target);
            }
        }

        private EventVertex CreateVertexEvent(ADVertex vertex, ref int verticeNextId, Dictionary<string, int>  verticeIdsMap)
        {
            int eventVertexId;
            if (!verticeIdsMap.TryGetValue(vertex.Id, out eventVertexId))
            {
                verticeIdsMap[vertex.Id] = eventVertexId = verticeNextId;
                verticeNextId++;
            }

            Activity activity;
            EventVertex eventVertex;
            if (vertex.Type == ActivityVertexType.Milestone && TryGetActivity(vertex, out activity))
            {
                eventVertex = EventVertex.CreateMilestone(eventVertexId, activity);
            }
            else
            {
                eventVertex = EventVertex.Create(eventVertexId);
            }
            return eventVertex;
        }

        private bool TryGetActivity(ADEdge<ADVertex> edge, out Activity activity)
        {
            activity = null;
            if (edge.ActivityId.HasValue && activitiesDictionary.ContainsKey(edge.ActivityId.Value))
            {
                activity = activitiesDictionary[edge.ActivityId.Value];
                return true;
            }

            return false;
        }

        private bool TryGetActivity(ADVertex vertex, out Activity activity)
        {
            activity = null;
            if (vertex.ActivityId.HasValue && activitiesDictionary.ContainsKey(vertex.ActivityId.Value))
            {
                activity = activitiesDictionary[vertex.ActivityId.Value];
                return true;
            }

            return false;
        }
    }
}
