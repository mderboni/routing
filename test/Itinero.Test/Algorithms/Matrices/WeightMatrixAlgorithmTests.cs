﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using System.Collections.Generic;
using Itinero.Algorithms.Matrices;

namespace Itinero.Test.Algorithms.Matrices
{
    /// <summary>
    /// Contains tests for the weight-matrix algorithm.
    /// </summary>
    [TestFixture]
    public class WeightMatrixAlgorithmTests
    {
        /// <summary>
        /// Tests a simple two-point matrix.
        /// </summary>
        [Test]
        public void Test1TwoPoints()
        {
            // build test case.
            var router = new RouterMock();
            var locations = new Coordinate[] { 
                    new Coordinate(0, 0),
                    new Coordinate(1, 1)};
            var matrixAlgorithm = new WeightMatrixAlgorithm<float>(router, Vehicle.Car.Fastest(), new DefaultWeightHandlerMock(),
                new Itinero.Algorithms.Search.MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Vehicle.Car.Fastest() }, locations, null));

            // run.
            matrixAlgorithm.Run();

            Assert.IsNotNull(matrixAlgorithm);
            Assert.IsTrue(matrixAlgorithm.HasRun);
            Assert.IsTrue(matrixAlgorithm.HasSucceeded);
            Assert.AreEqual(0, matrixAlgorithm.Errors.Count);
            var matrix = matrixAlgorithm.Weights;
            Assert.IsNotNull(matrix);
            Assert.AreEqual(0, matrix[0][0]);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(locations[0], locations[1]), matrix[0][1], 0.1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(locations[1], locations[0]), matrix[1][0], 0.1);
            Assert.AreEqual(0, matrix[1][1]);
            Assert.AreEqual(2, matrixAlgorithm.RouterPoints.Count);
            Assert.AreEqual(0, matrixAlgorithm.CorrectedIndexOf(0));
            Assert.AreEqual(1, matrixAlgorithm.CorrectedIndexOf(1));
        }

        /// <summary>
        /// Tests a simple two-point matrix but with one of the points not 'resolvable'.
        /// </summary>
        [Test]
        public void Test2TwoPoints()
        {
            // build test case.
            var router = new RouterMock();
            var locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(180, 1) };
            var matrixAlgorithm = new WeightMatrixAlgorithm<float>(router, Vehicle.Car.Fastest(), new DefaultWeightHandlerMock(),
                new Itinero.Algorithms.Search.MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Vehicle.Car.Fastest() }, locations, null));

            // run.
            matrixAlgorithm.Run();

            Assert.IsNotNull(matrixAlgorithm);
            Assert.IsTrue(matrixAlgorithm.HasRun);
            Assert.IsTrue(matrixAlgorithm.HasSucceeded);
            Assert.AreEqual(0, matrixAlgorithm.Errors.Count);
            Assert.AreEqual(1, matrixAlgorithm.MassResolver.Errors.Count);
            Assert.IsTrue(matrixAlgorithm.MassResolver.Errors.ContainsKey(1));
            var matrix = matrixAlgorithm.Weights;
            Assert.IsNotNull(matrix);
            Assert.AreEqual(0, matrix[0][0]);
            Assert.AreEqual(1, matrixAlgorithm.RouterPoints.Count);
            Assert.AreEqual(0, matrixAlgorithm.CorrectedIndexOf(0));
            Assert.AreEqual(0, matrixAlgorithm.OriginalIndexOf(0));

            // build test case.
            locations = new Coordinate[] {
                    new Coordinate(180, 0),
                    new Coordinate(1, 1) };
            matrixAlgorithm = new WeightMatrixAlgorithm<float>(router, Vehicle.Car.Fastest(), new DefaultWeightHandlerMock(),
                new Itinero.Algorithms.Search.MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Vehicle.Car.Fastest() }, locations, null));

            // run.
            matrixAlgorithm.Run();

            Assert.IsNotNull(matrixAlgorithm);
            Assert.IsTrue(matrixAlgorithm.HasRun);
            Assert.IsTrue(matrixAlgorithm.HasSucceeded);
            Assert.AreEqual(0, matrixAlgorithm.Errors.Count);
            Assert.AreEqual(1, matrixAlgorithm.MassResolver.Errors.Count);
            Assert.IsTrue(matrixAlgorithm.MassResolver.Errors.ContainsKey(0));
            matrix = matrixAlgorithm.Weights;
            Assert.IsNotNull(matrix);
            Assert.AreEqual(0, matrix[0][0]);
            Assert.AreEqual(1, matrixAlgorithm.RouterPoints.Count);
            Assert.AreEqual(0, matrixAlgorithm.CorrectedIndexOf(0));
            Assert.AreEqual(0, matrixAlgorithm.OriginalIndexOf(0));
            Assert.AreEqual(0, matrixAlgorithm.MassResolver.ResolvedIndexOf(1));
            Assert.AreEqual(1, matrixAlgorithm.MassResolver.LocationIndexOf(0));
        }

        /// <summary>
        /// Tests a simple two-point matrix but with one of the points not 'routable'.
        /// </summary>
        [Test]
        public void Test3TwoPoints()
        {
            // build test case.
            var invalidSet = new HashSet<int>();
            invalidSet.Add(1);
            var router = new RouterMock(invalidSet);
            var locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1) };
            var matrixAlgorithm = new WeightMatrixAlgorithm<float>(router, Vehicle.Car.Fastest(), new DefaultWeightHandlerMock(),
                new Itinero.Algorithms.Search.MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Vehicle.Car.Fastest() }, locations, null));

            // run.
            matrixAlgorithm.Run();

            Assert.IsNotNull(matrixAlgorithm);
            Assert.IsTrue(matrixAlgorithm.HasRun);
            Assert.IsTrue(matrixAlgorithm.HasSucceeded);
            Assert.AreEqual(1, matrixAlgorithm.Errors.Count);
            Assert.IsTrue(matrixAlgorithm.Errors.ContainsKey(1));
            var matrix = matrixAlgorithm.Weights;
            Assert.IsNotNull(matrix);
            Assert.AreEqual(0, matrix[0][0]);
            Assert.AreEqual(1, matrixAlgorithm.RouterPoints.Count);
            Assert.AreEqual(0, matrixAlgorithm.CorrectedIndexOf(0));
            Assert.AreEqual(0, matrixAlgorithm.OriginalIndexOf(0));

            // build test case.
            invalidSet.Clear();
            invalidSet.Add(0);
            router = new RouterMock(invalidSet);
            locations = new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1) };
            matrixAlgorithm = new WeightMatrixAlgorithm<float>(router, Vehicle.Car.Fastest(), new DefaultWeightHandlerMock(),
                new Itinero.Algorithms.Search.MassResolvingAlgorithm(router, new Itinero.Profiles.Profile[] { Vehicle.Car.Fastest() }, locations, null));

            // run.
            matrixAlgorithm.Run();

            Assert.IsNotNull(matrixAlgorithm);
            Assert.IsTrue(matrixAlgorithm.HasRun);
            Assert.IsTrue(matrixAlgorithm.HasSucceeded);
            Assert.AreEqual(1, matrixAlgorithm.Errors.Count);
            Assert.IsTrue(matrixAlgorithm.Errors.ContainsKey(0));
            matrix = matrixAlgorithm.Weights;
            Assert.IsNotNull(matrix);
            Assert.AreEqual(0, matrix[0][0]);
            Assert.AreEqual(1, matrixAlgorithm.RouterPoints.Count);
            Assert.AreEqual(0, matrixAlgorithm.CorrectedIndexOf(1));
            Assert.AreEqual(1, matrixAlgorithm.OriginalIndexOf(0));
        }
    }
}