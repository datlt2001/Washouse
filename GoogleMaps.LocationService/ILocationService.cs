﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMaps.LocationService
{
    public interface ILocationService
    {
        /// <summary>
        /// Translates a Latitude / Longitude into a Region (state) using Google Maps api
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        Region GetRegionFromLatLong(double latitude, double longitude);
        Region GetDistrictFromLatLong(double latitude, double longitude);


        /// <summary>
        /// Gets the latitude and longitude that belongs to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        MapPoint GetLatLongFromAddress(string address);


        /// <summary>
        /// Gets the directions.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The direction.</returns>
        Directions GetDirections(double latitude, double longitude);

        /// <summary>
        /// Gets the directions.
        /// </summary>
        /// <param name="originAddress">From address.</param>
        /// <param name="destinationAddress">To address.</param>
        /// <returns>The directions</returns>
        Directions GetDirections(AddressData fromAddress, AddressData toAddress);

    }
}
