﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;

namespace Itinero.Test.Algorithms.Search
{
    /// <summary>
    /// Contains tests for the resolve algorithm.
    /// </summary>
    [TestFixture]
    class ResolveAlgorithmTests
    {
        /// <summary>
        /// Tests resolving on an edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, 
                Constants.DefaultMaxEdgeDistance, 50f,
                    (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on an edge with a shape.
        /// </summary>
        [Test]
        public void TestOneEdgeWithShape()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var shape0 = new Coordinate(51.26763791763357f, 4.801728129386902f);
            var shape1 = new Coordinate(51.267070677950585f, 4.801749587059021f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, vertex0.Latitude, vertex0.Longitude);
            graph.AddVertex(1, vertex1.Latitude, vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, shape0, shape1);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0.
            location = shape0;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1.
            location = shape1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0 a bit to the top-right.
            location = new Coordinate(51.26771847181371f, 4.801915884017944f);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1 a bit to the bottom-right.
            location = new Coordinate(51.266986766160414f, 4.8019373416900635f);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on an edge with an isBetter function.
        /// </summary>
        [Test]
        public void TestOneEdgeWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on two edges with an isBetter function.
        /// </summary>
        [Test]
        public void TestTwoEdgesWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);
            var vertex2 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, vertex0.Latitude, vertex0.Longitude);
            graph.AddVertex(1, vertex1.Latitude, vertex1.Longitude);
            graph.AddVertex(2, vertex2.Latitude, vertex2.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);
            var expectedEdgeId = graph.AddEdge(0, 2, new uint[] { 1 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);
        }

        /// <summary>
        /// Tests resolving on an edge with a shape.
        /// </summary>
        [Test]
        public void TestTwoEdgesWithShapeWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var shape0 = new Coordinate(51.26763791763357f, 4.801728129386902f);
            var shape1 = new Coordinate(51.267070677950585f, 4.801749587059021f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);
            var vertex2 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddVertex(2, (float)vertex2.Latitude, (float)vertex2.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, shape0, shape1);
            var expectedEdgeId = graph.AddEdge(0, 2, new uint[] { 1 }, shape0, shape1);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape0.
            location = shape0;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape1.
            location = shape1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape0 a bit to the top-right.
            location = new Coordinate(51.26771847181371f, 4.801915884017944f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape1 a bit to the bottom-right.
            location = new Coordinate(51.266986766160414f, 4.8019373416900635f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);
        }
    }
}