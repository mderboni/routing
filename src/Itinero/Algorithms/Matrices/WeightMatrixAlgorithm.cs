﻿// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
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

using Itinero.Profiles;
using System.Collections.Generic;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public class WeightMatrixAlgorithm<T> : AlgorithmBase, IWeightMatrixAlgorithm<T>
        where T : struct
    {
        private readonly RouterBase _router;
        private readonly IProfileInstance _profile;
        private readonly WeightHandler<T> _weightHandler;
        private readonly IMassResolvingAlgorithm _massResolver;

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate[] locations)
            : this(router, profile, weightHandler, new MassResolvingAlgorithm(
                router, new IProfileInstance[] { profile }, locations))
        {

        }

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, IMassResolvingAlgorithm massResolver)
        {
            _router = router;
            _profile = profile;
            _weightHandler = weightHandler;
            _massResolver = massResolver;
        }

        private Dictionary<int, RouterPointError> _errors; // all errors per routerpoint idx.
        private List<int> _correctedIndices; // the original routerpoint per resolved point index.
        private List<RouterPoint> _correctedResolvedPoints; // only the valid resolved points.
        private T[][] _weights; // the weights between all valid resolved points.        
        
        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected sealed override void DoRun()
        {
            // run mass resolver if needed.
            if (!_massResolver.HasRun)
            {
                _massResolver.Run();
            }

            // create error and resolved point management data structures.
            _correctedResolvedPoints = _massResolver.RouterPoints;
            _errors = new Dictionary<int, RouterPointError>(_correctedResolvedPoints.Count);
            _correctedIndices = new List<int>(_correctedResolvedPoints.Count);
            for (var i = 0; i < _correctedResolvedPoints.Count; i++)
            {
                _correctedIndices.Add(i);
            }
            
            // calculate matrix.
            var nonNullInvalids = new HashSet<int>();
            _weights = _router.CalculateWeight(_profile, _weightHandler, _correctedResolvedPoints.ToArray(), nonNullInvalids);

            // take into account the non-null invalids now.
            if (nonNullInvalids.Count > 0)
            { // shrink lists and add errors.
                foreach (var invalid in nonNullInvalids)
                {
                    _errors[invalid] = new RouterPointError()
                    {
                        Code = RouterPointErrorCode.NotRoutable,
                        Message = "Location could not routed to or from."
                    };
                }

                _correctedResolvedPoints = _correctedResolvedPoints.ShrinkAndCopyList(nonNullInvalids);
                _correctedIndices = _correctedIndices.ShrinkAndCopyList(nonNullInvalids);
                _weights = _weights.SchrinkAndCopyMatrix(nonNullInvalids);
            }
            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the router.
        /// </summary>
        public RouterBase Router
        {
            get
            {
                return _router;
            }
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        public IProfileInstance Profile
        {
            get
            {
                return _profile;
            }
        }

        /// <summary>
        /// Gets the mass resolver.
        /// </summary>
        public IMassResolvingAlgorithm MassResolver
        {
            get
            {
                return _massResolver;
            }
        }

        /// <summary>
        /// Gets the weights between all valid router points.
        /// </summary>
        public T[][] Weights
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _weights;
            }
        }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        public List<RouterPoint> RouterPoints
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _correctedResolvedPoints;
            }
        }

        /// <summary>
        /// Returns the corrected index of the routerpoint, given the original index.
        /// </summary>
        public int CorrectedIndexOf(int originalIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _correctedIndices.IndexOf(originalIdx);
        }

        /// <summary>
        /// Returns the original index of the routerpoint, given the corrected index.
        /// </summary>
        public int OriginalIndexOf(int routerPointIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _correctedIndices[routerPointIdx];
        }

        /// <summary>
        /// Returns the errors indexed per original routerpoint index.
        /// </summary>
        public Dictionary<int, RouterPointError> Errors
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _errors;
            }
        }
    }

    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public sealed class WeightMatrixAlgorithm : WeightMatrixAlgorithm<float>
    {
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, IMassResolvingAlgorithm massResolver)
            : base(router, profile, profile.DefaultWeightHandler(router), massResolver)
        {

        }
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, Coordinate[] locations)
            : base(router, profile, profile.DefaultWeightHandler(router), locations)
        {

        }
    }
}