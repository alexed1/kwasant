using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Newtonsoft.Json;

namespace Data.Entities
{
    public class EnvelopeDO : IEnvelope
    {
        class MergeDataDictionary : IDictionary<string, string>
        {
            private readonly EnvelopeDO _envelope;
            private Dictionary<string, string> _dictionary;
            private IDictionary<string, string> Dictionary
            {
                get
                {
                    return _dictionary ?? (_dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(_envelope.MergeDataString));
                }
            } 

            public MergeDataDictionary(EnvelopeDO envelope)
            {
                _envelope = envelope;
            }

            private void UpdateEnvelope()
            {
                _envelope.MergeDataString = JsonConvert.SerializeObject(Dictionary);
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return Dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection<KeyValuePair<string,string>>

            public void Add(KeyValuePair<string, string> item)
            {
                Dictionary.Add(item);
                UpdateEnvelope();
            }

            public void Clear()
            {
                Dictionary.Clear();
                UpdateEnvelope();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                return Dictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                Dictionary.CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                var result = Dictionary.Remove(item);
                UpdateEnvelope();
                return result;
            }

            public int Count { get { return Dictionary.Count; } }
            public bool IsReadOnly { get { return Dictionary.IsReadOnly; } }

            #endregion

            #region Implementation of IDictionary<string,string>

            public bool ContainsKey(string key)
            {
                return Dictionary.ContainsKey(key);
            }

            public void Add(string key, string value)
            {
                Dictionary.Add(key, value);
                UpdateEnvelope();
            }

            public bool Remove(string key)
            {
                var result = Dictionary.Remove(key);
                UpdateEnvelope();
                return result;
            }

            public bool TryGetValue(string key, out string value)
            {
                return Dictionary.TryGetValue(key, out value);
            }

            public string this[string key]
            {
                get { return Dictionary[key]; }
                set 
                { 
                    Dictionary[key] = value;
                    UpdateEnvelope();
                }
            }

            public ICollection<string> Keys { get { return Dictionary.Keys; } }
            public ICollection<string> Values { get { return Dictionary.Values; } }

            #endregion
        }

        public const string GmailHander = "Gmail";
        public const string MandrillHander = "Mandrill";

        public EnvelopeDO()
        {
            MergeData = new MergeDataDictionary(this);
        }

        [Key]
        public int Id { get; set; }
        public string Handler { get; set; }
        public string TemplateName { get; set; }
        [NotMapped]
        public IDictionary<string, string> MergeData { get; private set; }
        [NotMapped]
        IEmail IEnvelope.Email { get; set; }
        [Required]
        public EmailDO Email { get; set; }
        [Column("MergeData")]
        public string MergeDataString { get; set; }
        [ForeignKey("Email"), Required]
        public int EmailID { get; set; }
    }
}
