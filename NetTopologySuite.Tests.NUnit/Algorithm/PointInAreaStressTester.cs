﻿using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class PointInAreaStressTester
    {
        private readonly IGeometryFactory _geomFactory;
        private readonly IGeometry _area;
        private readonly int[] _locationCount = new int[3];
        public PointInAreaStressTester(IGeometryFactory geomFactory, IGeometry area)
        {
            _geomFactory = geomFactory;
            _area = area;
        }
        public int NumPoints { get; set; } = 10000;
        public IPointOnGeometryLocator TestPointInAreaLocator { get; set; }
        public IPointOnGeometryLocator ExpectedPointInAreaLocator { get; set; }
        public Boolean IgnoreBoundaryResults { get; set; } = true;
        /// <summary>
        /// Run
        /// </summary>
        /// <returns> true if all point locations were computed correctly</returns>
        public Boolean Run()
        {
            var sw = new Stopwatch();
            sw.Start();
            // default is to use the simple, non-indexed tester
            if (ExpectedPointInAreaLocator == null)
                ExpectedPointInAreaLocator = new SimplePointInAreaLocator(_area);
            int ptGridWidth = (int)Math.Sqrt(NumPoints);
            Envelope areaEnv = _area.EnvelopeInternal;
            double xStep = areaEnv.Width / (ptGridWidth - 1);
            double yStep = areaEnv.Height / (ptGridWidth - 1);
            for (int i = 0; i < ptGridWidth; i++)
            {
                for (int j = 0; j < ptGridWidth; j++)
                {
                    // compute test point
                    double x = areaEnv.MinX + i * xStep;
                    double y = areaEnv.MinY + j * yStep;
                    var pt = new Coordinate(x, y);
                    _geomFactory.PrecisionModel.MakePrecise(pt);
                    Boolean isEqual = TestPointInArea(pt);
                    if (!isEqual)
                        return false;
                }
            }
            sw.Stop();
            Console.WriteLine("Test completed in " + sw.ElapsedMilliseconds);
            PrintStats();
            return true;
        }
        public void PrintStats()
        {
            Console.WriteLine("Location counts: "
                              + "\nBoundary = " + _locationCount[(Int32)Location.Boundary]
                              + "\nInterior = " + _locationCount[(Int32)Location.Interior]
                              + "\nExterior = " + _locationCount[(Int32)Location.Exterior]
                );
        }
        /// <summary>
        /// TestPointInArea
        /// </summary>
        /// <param name="p"></param>
        /// <returns>true if the point location is determined to be the same by both PIA locators</returns>
        private Boolean TestPointInArea(Coordinate p)
        {
            //Console.WriteLine(WKTWriter.toPoint(p));
            Location loc1 = TestPointInAreaLocator.Locate(p);
            Location loc2 = ExpectedPointInAreaLocator.Locate(p);
            _locationCount[(Int32)loc1]++;
            if ((loc1 == Location.Boundary || loc2 == Location.Boundary)
                && IgnoreBoundaryResults)
                return true;
            return loc1 == loc2;
        }
    }
}
