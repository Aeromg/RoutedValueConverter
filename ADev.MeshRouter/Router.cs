using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace ADev.MeshRouter
{
    /// <summary>
    /// Provide Mesh structures routing methods
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge key</typeparam>
    public sealed class Router<TKey, TBridge>
    {
        /// <summary>
        /// All served meshes of current router
        /// </summary>
        public IList<Mesh<TKey, TBridge>> Meshes { get; private set; }

        /// <summary>
        /// Creates new router with empty meshes list
        /// </summary>
        public Router()
        {
            Meshes = new List<Mesh<TKey, TBridge>>();
        }

        #region Public methods

        /// <summary>
        /// Calculate shortest path between two nodes
        /// </summary>
        /// <param name="source">Source node key</param>
        /// <param name="destination">Destination node key</param>
        /// <returns>Calculated path</returns>
        public Path<TKey, TBridge> Route(TKey source, TKey destination)
        {

            var startMesh = GetMeshWithKey(source);
            var endMesh = GetMeshWithKey(destination);

            // unknown mesh
            if (startMesh == null || endMesh == null)
                return Path<TKey, TBridge>.BuildNotResolved(source, destination);

            Node<TKey, TBridge> startNode;
            Node<TKey, TBridge> endNode;

            startMesh.TryGetNode(source, out startNode);
            startMesh.TryGetNode(destination, out endNode);

            // how does it possible???
            if (startNode == null || endNode == null)
                return Path<TKey, TBridge>.BuildNotResolved(source, destination);

            var visitedStack = new HashSet<long>();
            var firstStep = new RoutingStep<TKey, TBridge>()
            {
                Source = startNode,
                Destination = endNode
            };

            var maxComplexity = int.MaxValue;
            var reached = Route(startNode, endNode, visitedStack, firstStep, ref maxComplexity);
            if(!reached)
                return Path<TKey, TBridge>.BuildNotResolved(source, destination);

#if DEBUG
            if (!firstStep.BestEnding.Destination.Key.Equals(destination))
                throw new Exception();
#endif

            return firstStep.BestEnding.ToPath();
        }

        public Mesh<TKey, TBridge> CreateMesh()
        {
            var mesh = new Mesh<TKey, TBridge>();
            Meshes.Add(mesh);
            return mesh;
        }

        #endregion

        #region Private methods

        // continue building route, appending alternate steps to currentStep
        // starts with start, need to reach destinationEnd
        // visited - all of visited nodes ID in current way alternate
        //
        // return false if deadlock or no more steps can be found
        // return true only if end reached
        bool Route(Node<TKey, TBridge> start,
           Node<TKey, TBridge> destinationEnd,
           HashSet<long> visited,
           RoutingStep<TKey, TBridge> currentStep,
           ref int maxComlexity)
        {

            // node is visited again then we must prevent deadlock
            if (visited.Contains(start.NodeId))
                return false;

            // achive the goal 
            if (start == destinationEnd)
                return true;

            bool reached = false;

            // mark this node as visited for this current way
            visited.Add(start.NodeId);

            // go recursion with each of next available steps
            var nextHopeNodes = start.NextHopeNodes;
            foreach (var nextNode in nextHopeNodes)
            {
                bool stepReached = false;
                var nextStep = currentStep.CreateNextStep(start, nextNode);
                var nextStepRouteComplexity = nextStep.RouteComplexity;

                if (nextStepRouteComplexity < maxComlexity)
                    stepReached = Route(nextNode, destinationEnd, visited, nextStep, ref maxComlexity);

                if (stepReached)
                {
                    reached = true;
                    maxComlexity = nextStepRouteComplexity;
                }
                else
                    ForgetWrongWay(nextStep);
            }

            visited.Remove(start.NodeId);

            return reached;
        }

        // empty the fork to intersection step
        void ForgetWrongWay(RoutingStep<TKey, TBridge> lastWrongStep)
        {
            var lastIntersection = lastWrongStep.LastIntersection;
            if (lastIntersection != null)
                lastIntersection.NextStepVariants.Remove(lastWrongStep);

            if(!lastWrongStep.IsFirstStep)
                lastWrongStep.PreviousStep.NextStepVariants.Remove(lastWrongStep);
        }

        // founds servable mesh that contains node with given key
        Mesh<TKey, TBridge> GetMeshWithKey(TKey key)
        {
            return
                (from mesh in Meshes
                 where mesh.ContainsKey(key)
                 select mesh).FirstOrDefault();
        }

        #endregion
    }
}
