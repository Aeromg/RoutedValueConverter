using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ADev.MeshRouter
{
    [DebuggerDisplay("{Source.Key.ToString()} -> {Destination.Key.ToString()}")]
    /// <summary>
    /// Represent full routing way or part of them as Routing Step including current step
    /// </summary>
    /// <typeparam name="TKey">Type of node key</typeparam>
    /// <typeparam name="TBridge">Type of bridge payload</typeparam>
    sealed class RoutingStep<TKey, TBridge>
    {
        /// <summary>
        /// Complexity of step using Parent
        /// </summary>
        const int DirectStepComplexity = 1;

        /// <summary>
        /// Complexity of no operation step
        /// </summary>
        const int ZeroStepComplexity = 0;

        IList<RoutingStep<TKey, TBridge>> _nextStepVariants;
        Bridge<TKey, TBridge> _stepBridge;
        int _totalComplexityCalculated;
        bool _isTotalComplexityCalculated = false;

        /// <summary>
        /// Previous node
        /// </summary>
        public Node<TKey, TBridge> Source { get; set; }

        /// <summary>
        /// Next node
        /// </summary>
        public Node<TKey, TBridge> Destination { get; set; }

        /// <summary>
        /// Previous routing step
        /// </summary>
        public RoutingStep<TKey, TBridge> PreviousStep { get; set; }

        /// <summary>
        /// List of all next step variants
        /// </summary>
        public IList<RoutingStep<TKey, TBridge>> NextStepVariants {
            get
            {
                if (_nextStepVariants == null)
                    _nextStepVariants = new List<RoutingStep<TKey, TBridge>>();

                return _nextStepVariants;
            }
        }

        /// <summary>
        /// Bridge between previous node and next node. Null if is direct step via Parent
        /// </summary>
        public Bridge<TKey, TBridge> StepBridge
        {
            get
            {
                if (_stepBridge == null)
                    _stepBridge = GetStepBridge();

                return _stepBridge;
            }
        }

        /// <summary>
        /// Head of routing way
        /// </summary>
        public RoutingStep<TKey, TBridge> FirstStep
        {
            get
            {
                var step = this;
                while (!step.IsFirstStep)
                    step = step.PreviousStep;

                return step;
            }
        }

        /// <summary>
        /// Sign this is head of routing way
        /// </summary>
        public bool IsFirstStep
        {
            get
            {
                return PreviousStep == null;
            }
        }

        /// <summary>
        /// Sign this in last step of routing way or deadlock 
        /// </summary>
        public bool IsLastStep
        {
            get
            {
                return _nextStepVariants == null || _nextStepVariants.Count == 0;
            }
        }

        /// <summary>
        /// Sign if this is intersection and exist more than one next steps
        /// </summary>
        public bool IsIntersection
        {
            get
            {
                return _nextStepVariants != null && _nextStepVariants.Count > 1;
            }
        }

        /// <summary>
        /// Get last intersection backwards since of this step
        /// </summary>
        public RoutingStep<TKey, TBridge> LastIntersection
        {
            get
            {
                var step = this;
                while (step != null && !step.IsIntersection)
                    step = step.PreviousStep;

                return step;
            }
        }

        /// <summary>
        /// Sign if this step using parent and not use bridge between nodes
        /// </summary>
        public bool IsDirectStep
        {
            get
            {
                return Source.EqualsOrContainsParent(Destination);
            }
        }

        /// <summary>
        /// Complexity of this step
        /// </summary>
        public int Complexity
        {
            get
            {
                if (IsFirstStep)
                    return ZeroStepComplexity;

                if (IsDirectStep)
                    return DirectStepComplexity;
                else
                    return StepBridge.Complexity;
            }
        }

        /// <summary>
        /// Complexity of all steps to begins of routing head including complexity of this step
        /// </summary>
        public int RouteComplexity
        {
            get
            {
                if (!_isTotalComplexityCalculated)
                {
                    lock (this)
                    {
                        _totalComplexityCalculated = GetRouteComplexityRecursive();
                        _isTotalComplexityCalculated = true;
                    }
                }

                return _totalComplexityCalculated;
            }
        }

        /// <summary>
        /// Creates new step, initialize, and append it to this next step variants
        /// </summary>
        /// <param name="source">Previous node</param>
        /// <param name="destination">Next node</param>
        /// <returns>New step with initialized fields</returns>
        public RoutingStep<TKey, TBridge> CreateNextStep(Node<TKey, TBridge> source,
                                                         Node<TKey, TBridge> destination)
        {
            var nextStep = new RoutingStep<TKey, TBridge>()
            {
                Source = source,
                Destination = destination,
                PreviousStep = this
            };
            NextStepVariants.Add(nextStep);
            return nextStep;
        }

        /// <summary>
        /// All endign steps from this step
        /// </summary>
        public IEnumerable<RoutingStep<TKey, TBridge>> Endings
        {
            get 
            {
                var endings = new List<RoutingStep<TKey, TBridge>>();

                if (IsLastStep)
                    endings.Add(this);

                foreach (var nextStep in NextStepVariants)
                    if (nextStep.IsLastStep)
                        endings.Add(nextStep);
                    else
                        endings.AddRange(nextStep.Endings);

                return endings;
            }
        }

        /// <summary>
        /// Best ending from this step
        /// </summary>
        public RoutingStep<TKey, TBridge> BestEnding
        {
            get
            {
                return (from ending in Endings
                        orderby ending.RouteComplexity ascending
                        select ending).FirstOrDefault();
            }
        }

        /// <summary>
        /// Converts this way step to Path.
        /// This step is the ending of new path
        /// </summary>
        /// <returns></returns>
        public Path<TKey, TBridge> ToPath()
        {
            var step = this;
            var pathSteps = new List<Step<TKey, TBridge>>();
            while (!step.IsFirstStep)
            {
                var pathStep = new Step<TKey, TBridge>()
                {
                    Key = step.Destination.Key,
                    Bridge = step.IsDirectStep ? default(TBridge) : step.StepBridge.Payload
                };
                pathSteps.Add(pathStep);
                step = step.PreviousStep;
            }

            pathSteps.Reverse();

            var firstStep = this.FirstStep;

            return new Path<TKey, TBridge>()
            {
                IsResolved = true,
                Steps = pathSteps,
                Source = firstStep.Source.Key,
                Destination = firstStep.Destination.Key,
                Complexity = this.RouteComplexity
            };
        }

        Bridge<TKey, TBridge> GetStepBridge()
        {
            return
                (from bridge in Source.Bridges
                 where bridge.Destination == Destination
                 select bridge).FirstOrDefault();
        }

        // cached, but stack overflow risk
        int GetRouteComplexityRecursive()
        {
            return
                IsFirstStep ?
                Complexity : Complexity + PreviousStep.RouteComplexity;
        }

        // uncached stable. Slower?
        int GetRouteComplexity()
        {
            var step = this;
            int routeComplexity = 0;

            do
            {
                routeComplexity += step.Complexity;
                step = step.PreviousStep;
            } while (step != null);

            return routeComplexity;
        }
    }
}

