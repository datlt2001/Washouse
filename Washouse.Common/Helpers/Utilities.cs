using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Washouse.Common.Helpers
{
    public class Utilities
    {
        public const decimal platformFee = 0;
        public static async Task<string> UploadFile(Microsoft.AspNetCore.Http.IFormFile file, string sDirectory, string newname = null)
        {
            try
            {
                if (newname == null) newname = file.FileName;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFile", sDirectory);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                var supportedTypes = new[] { "jpg", "jpeg", "png", "gif" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt.ToLower()))
                {
                    return null;
                }
                else
                {
                    string fullPath = path + "//" + newname;
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return newname;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }

        
        public static string MapDistrictName(string districtName)
        {
            Dictionary<string, string> mapDistrict = new Dictionary<string, string>
            {
                {"Quận 9", "Thành phố Thủ Đức" },
                {"District 9", "Thành phố Thủ Đức" },
                {"Quận 2", "Thành phố Thủ Đức" },
                {"District 2", "Thành phố Thủ Đức" },
                {"TP Thủ Đức", "Thành phố Thủ Đức" },
                {"Thủ Đức", "Thành phố Thủ Đức" },
                {"Quận Thủ Đức", "Thành phố Thủ Đức" },
                {"Tân Phú", "Quận Tân Phú" },
                {"Tân Bình", "Quận Tân Bình" },
                {"Phú Nhuận", "Quận Phú Nhuận" },
                {"Gò Vấp", "Quận Gò Vấp"},
                {"Bình Thạnh", "Quận Bình Thạnh" },
                {"Bình Tân", "Quận Bình Tân" },
                {"Nhà Bè", "Huyện Nhà Bè" },
                {"Hóc Môn", "Huyện Hóc Môn" },
                {"Củ Chi", "Huyện Củ Chi" },
                {"Cần Giờ", "Huyện Cần Giờ" },
                {"Bình Chánh", "Huyện Bình Chánh" }
            };

            if (mapDistrict.ContainsKey(districtName))
            {
                return mapDistrict[districtName];
            }
            else
            {
                return null;
            }
        }


        public static double CalculateDistance(decimal? Latitude_1, decimal? Longitude_1, decimal? Latitude_2, decimal? Longitude_2)
        {
            if (Latitude_1 == null || Longitude_1 == null || Latitude_2 == null || Longitude_2 == null)
                return 0;

            const double earthRadius = 6371; // Earth's radius in kilometers

            var lat1 = ToRadians((double)Latitude_1);
            var lon1 = ToRadians((double)Longitude_1);
            var lat2 = ToRadians((double)Latitude_2);
            var lon2 = ToRadians((double)Longitude_2);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = earthRadius * c;

            return distance;
        }

        public static async Task<int> CalculateDeliveryEstimatedTime(decimal? Latitude_1, decimal? Longitude_1, decimal? Latitude_2, decimal? Longitude_2)
        {
            if (Latitude_1 == null || Longitude_1 == null || Latitude_2 == null || Longitude_2 == null)
                return 0;
            string coordinates = $"{Longitude_1},{Latitude_1};{Longitude_2},{Latitude_2};";
            string url = $"https://router.project-osrm.org/route/v1/motorcycle/{coordinates}?overview=false";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JObject.Parse(json);
                        var duration = jsonResponse["routes"][0]["legs"].Sum(x => (double)x["duration"]);

                        // duration is in seconds, convert to minutes
                        var durationInMinutes = TimeSpan.FromSeconds(duration).TotalMinutes;
                        return (int)durationInMinutes;
                    }
                    else
                    {
                        return (int)Math.Round(CalculateDistance(Latitude_1,Longitude_1,Latitude_2,Longitude_2)*2);
                    }
                }
            }
            catch
            {
                return (int)Math.Round(CalculateDistance(Latitude_1, Longitude_1, Latitude_2, Longitude_2)*2);
            }
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static string? GenerateFileNameToSave(string incomingFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(incomingFileName);
            var extension = Path.GetExtension(incomingFileName);
            return $"{fileName}-{DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss")}{extension}";
        }

        public static async Task<dynamic> SearchRelativeAddress(string query)
        {
            string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={query}&format=json&limit=1";
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(json);
                        if (result.Count > 0)
                        {
                            return new
                            {
                                lat = result[0].lat,
                                lon = result[0].lon
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
