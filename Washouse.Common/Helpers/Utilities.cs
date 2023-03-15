﻿using System;
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
                {"Quận 9", "Quận 9" },
                {"District 9", "Quận 9" },
                {"Thành phố Thủ Đức", "Quận Thủ Đức" },
                {"TP Thủ Đức", "Quận Thủ Đức" },
                {"Thủ Đức", "Quận Thủ Đức" }
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
