using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Common.Helpers
{
    public class Utilities
    {

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

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
