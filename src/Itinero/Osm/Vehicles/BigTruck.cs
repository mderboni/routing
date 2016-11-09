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

using System;
using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Profiles;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents the default OSM big truck profile.
    /// </summary>
    public class BigTruck : MotorVehicle
    {
        /// <summary>
        /// Creates a new big truck.
        /// </summary>
        public BigTruck()
        {
            this.Register(new Profiles.Profile(this.Name, ProfileMetric.TimeInSeconds, this.VehicleTypes, new Constraint[]
            {
                new Constraint("maxweight", true, 0),
                new Constraint("maxwidth", true, 0)
            }, this, null));
            this.Register(new Profiles.Profile(this.Name + ".shortest", ProfileMetric.TimeInSeconds, this.VehicleTypes, new Constraint[]
            {
                new Constraint("maxweight", true, 0),
                new Constraint("maxwidth", true, 0)
            }, this, null));
        }

        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return "BigTruck";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return new string[] { "vehicle", "motor_vehicle", "hgv" };
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as meta-data.
        /// </summary>
        public override HashSet<string> MetaWhiteList
        {
            get
            {
                return new HashSet<string>(new[] { "name" });
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as part of the profile.
        /// </summary>
        public override HashSet<string> ProfileWhiteList
        {
            get
            {
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// Creates a new fastest bigtruck profile but with weight and width constraints.
        /// </summary>
        /// <param name="weight">The weight in kilograms.</param>
        /// <param name="width">The width in meters.</param>
        public IProfileInstance Fastest(float weight, float width)
        {
            return this.Fastest().BuildConstrained(new float[] { weight, width });
        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public sealed override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whiteList)
        {
            string highway = string.Empty;
            if (attributes == null ||
                !attributes.TryGetValue("highway", out highway))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            var speed = 70.0f;
            var canstopon = true;
            switch (highway)
            {
                case "services":
                case "living_street":
                    speed = 5;
                    break;
                case "service":
                case "track":
                case "road":
                    speed = 30;
                    break;
                case "residential":
                case "unclassified":
                    speed = 50;
                    break;
                case "tertiary":
                case "tertiary_link":
                case "secondary":
                case "secondary_link":
                    speed = 70;
                    break;
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    speed = 90;
                    break;
                case "motorway":
                case "motorway_link":
                    canstopon = false;
                    speed = 120;
                    break;
                default:
                    return Profiles.FactorAndSpeed.NoFactor;
            }
            whiteList.Add("highway");

            // get max-speed tag if any.
            var maxSpeed = 0.0f;
            if (attributes.TryGetMaxSpeed(out maxSpeed))
            {
                whiteList.Add("maxspeed");
                speed = maxSpeed * 0.75f;
            }

            // access tags.
            if (!Vehicle.InterpretAccessValues(attributes, whiteList, this.VehicleTypes, "access"))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            // oneway restrictions.
            short direction = 0; // 0=bidirectional, 1=forward, 2=backward
            string oneway;
            string junction;
            if (attributes.TryGetValue("junction", out junction))
            {
                if (junction == "roundabout")
                {
                    whiteList.Add("junction");
                    direction = 1;
                }
            }
            if (attributes.TryGetValue("oneway", out oneway))
            {
                if (oneway == "yes")
                {
                    direction = 1;
                }
                else if (oneway == "no")
                { // explicitly tagged as not oneway.

                }
                else
                {
                    direction = 2;
                }
                whiteList.Add("oneway");
            }

            // try to parse maxweight.
            var maxweight = 0f;
            if (!attributes.TryGetMaxWeight(out maxweight))
            { // there is a valid max weight.
                maxweight = 0;
            }
            else
            {
                whiteList.Add("maxweight");
            }

            // try to parse maxwidth.
            var maxwidth = 0f;
            if (!attributes.TryGetMaxWidth(out maxwidth))
            { // there is a valid max width.
                maxwidth = 0;
            }
            else
            {
                whiteList.Add("maxwidth");
            }

            float[] constraints = null;
            if (maxwidth != 0 || maxweight != 0)
            {
                constraints = new float[] { maxweight, maxwidth };
            }

            speed = speed / 3.6f; // to m/s
            if (!canstopon)
            { // add canstop on info to direction.
                direction += 3;
            }

            return new Profiles.FactorAndSpeed()
            {
                Constraints = constraints,
                Direction = direction,
                SpeedFactor = 1.0f / speed,
                Value = 1.0f / speed
            };
        }
    }
}