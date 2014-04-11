using System;
using System.Configuration;
using System.IO;
using Data.Models;

namespace Data.DataAccessLayer.Infrastructure
{
    public static class FileManager
    {
        static string GetAbsolutePath(String relativePath)
        {
            var directory = ConfigurationManager.AppSettings["LocalFileStorageDirectory"];
            directory = Path.GetFullPath(directory);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return Path.Combine(directory, relativePath);
        }

        public static void SaveFile(StoredFile file)
        {
            if (String.IsNullOrEmpty(file.StoredName))
                file.StoredName = Path.GetRandomFileName();

            var fileStream = new FileStream(GetAbsolutePath(file.StoredName), FileMode.Create);
            file.Data.CopyTo(fileStream);
            fileStream.Close();
        }

        public static void LoadFile(StoredFile file)
        {
            if (String.IsNullOrEmpty(file.StoredName))
                throw new Exception("File does not have a stored name");

            var memoryStream = new MemoryStream();
            var fileStream = new FileStream(GetAbsolutePath(file.StoredName), FileMode.Open);
            fileStream.CopyTo(memoryStream);
            fileStream.Close();
            file.Data = memoryStream;
        }
    }
}
