using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
