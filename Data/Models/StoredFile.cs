using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class StoredFile : ISaveHook
    {
        public int StoredFileID { get; set; }

        public String OriginalName { get; set; }
        public String StoredName { get; set; }

        private Stream _data;
        [NotMapped]
        public Stream Data
        {
            get
            {
                if (_data == null)
                    FileManager.LoadFile(this);
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        [NotMapped]
        public String StringData
        {
            get
            {                
                return new StreamReader(Data).ReadToEnd();
            }
            set
            {
                var memStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                Data = memStream;
            }
        }

        [NotMapped]
        public byte[] Bytes
        {
            get
            {
                var memoryStream = new MemoryStream();
                Data.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            set
            {
                Data = new MemoryStream(value);
            }
        }

        void ISaveHook.SaveHook()
        {
            FileManager.SaveFile(this);
        }
    }
}
