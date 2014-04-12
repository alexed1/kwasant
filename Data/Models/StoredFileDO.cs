using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;

namespace Data.Models
{
    public class StoredFileDO : ISaveHook
    {
        [Key]
        public int StoredFileID { get; set; }

        public String OriginalName { get; set; }
        public String StoredName { get; set; }

        private Stream _data;

        public Stream GetData()
        {
            if (_data == null)
                FileManager.LoadFile(this);
            return _data;
        }
        public void SetData(Stream value)
        {
            _data = value;
        }
        
        [NotMapped]
        public String StringData
        {
            get
            {
                return new StreamReader(GetData()).ReadToEnd();
            }
            set
            {
                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                SetData(memStream);
            }
        }

        [NotMapped]
        public byte[] Bytes
        {
            get
            {
                MemoryStream memoryStream = new MemoryStream();
                GetData().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            set
            {
                SetData(new MemoryStream(value));
            }
        }

        void ISaveHook.SaveHook()
        {
            FileManager.SaveFile(this);
        }
    }
}
