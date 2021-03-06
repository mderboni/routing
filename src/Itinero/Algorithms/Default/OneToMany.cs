﻿// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default
{
    /// <summary>
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public class OneToMany<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly RouterPoint _source;
        private readonly IList<RouterPoint> _targets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _maxSearch;
        
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, WeightHandler<T> weightHandler,
            RouterPoint source, IList<RouterPoint> targets, T maxSearch)
        {
            _routerDb = routerDb;
            _weightHandler = weightHandler;
            _source = source;
            _targets = targets;
            _maxSearch = maxSearch;
        }

        private EdgePath<T>[] _best;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _best = new EdgePath<T>[_targets.Count];

            // register the targets and determine one-edge-paths.
            var sourcePaths = _source.ToEdgePaths(_routerDb, _weightHandler, true);
            var targetIndexesPerVertex = new Dictionary<uint, LinkedTarget>();
            var targetPaths = new IEnumerable<EdgePath<T>>[_targets.Count];
            for (var i = 0; i < _targets.Count; i++)
            {
                var targets = _targets[i].ToEdgePaths(_routerDb, _weightHandler, false);
                targetPaths[i] = targets;

                // determine one-edge-paths.
                if (_source.EdgeId == _targets[i].EdgeId)
                { // on same edge.
                    _best[i] = _source.EdgePathTo(_routerDb, _weightHandler, _targets[i]);
                }

                // register targets.
                for (var t = 0; t < targets.Length; t++)
                {
                    var target = targetIndexesPerVertex.TryGetValueOrDefault(targets[t].Vertex);
                    targetIndexesPerVertex[targets[t].Vertex] = new LinkedTarget()
                    {
                        Target = i,
                        Next = target
                    };
                }
            }

            // determine the best max search radius.
            var max = _weightHandler.Zero;
            for(var s = 0; s < _best.Length; s++)
            {
                if(_best[s] == null)
                {
                    max = _maxSearch;
                }
                else
                {
                    if (_weightHandler.IsLargerThan(_best[s].Weight, max))
                    {
                        max = _best[s].Weight;
                    }
                }
            }

            // run the search.
            var dykstra = new Dykstra<T>(_routerDb.Network.GeometricGraph.Graph, null, _weightHandler,
                sourcePaths, max, false);
            dykstra.WasFound += (vertex, weight) =>
            {
                LinkedTarget target;
                if(targetIndexesPerVertex.TryGetValue(vertex, out target))
                { // there is a target for this vertex.
                    while(target != null)
                    {
                        var best = _best[target.Target];
                        foreach(var targetPath in targetPaths[target.Target])
                        {
                            EdgePath<T> path;
                            dykstra.TryGetVisit(vertex, out path);
                            if(targetPath.Vertex == vertex)
                            { // there is a path here.
                                var total = _weightHandler.Add(targetPath.Weight, weight);
                                if(best == null ||
                                   _weightHandler.IsSmallerThan(total, best.Weight))
                                { // not a best path yet, just add this one.
                                    if (_targets[target.Target].IsVertex(_routerDb, path.Vertex))
                                    { // target is the exact vertex.
                                        best = path;
                                    }
                                    else
                                    { // target is not the exact vertex.
                                        best = new EdgePath<T>(_targets[target.Target].VertexId(_routerDb),
                                            total, path);
                                    }
                                }
                                break;
                            }
                        }

                        // set again.
                        _best[target.Target] = best;

                        // move to next target.
                        target = target.Next;
                    }
                }
                return false;
            };
            dykstra.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the path to the given target.
        /// </summary>
        /// <returns></returns>
        public EdgePath<T> GetPath(int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var best = _best[target];
            if (best != null)
            {
                return best;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }

        /// <summary>
        /// Tries to get the path to the given target.
        /// </summary>
        /// <returns></returns>
        public bool TryGetPath(int target, out EdgePath<T> path)
        {
            this.CheckHasRunAndHasSucceeded();

            path = _best[target];
            return path != null;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public T[] Weights
        {
            get
            {
                var weights = new T[_best.Length];
                for (var i = 0; i < _best.Length; i++)
                {
                    weights[i] = _weightHandler.Infinite;
                    if (_best[i] != null)
                    {
                        weights[i] = _best[i].Weight;
                    }
                }
                return weights;
            }
        }

        private class LinkedTarget
        {
            public int Target { get; set; }

            public LinkedTarget Next { get; set; }
        }
    }

    /// <summary>
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public sealed class OneToMany : OneToMany<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterBase router, Profile profile,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(router.Db, profile.DefaultWeightHandler(router), source, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, Func<ushort, Factor> getFactor,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(routerDb, new DefaultWeightHandler(getFactor), source, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, DefaultWeightHandler weightHandler,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(routerDb, weightHandler, source, targets, maxSearch)
        {

        }
    }
}