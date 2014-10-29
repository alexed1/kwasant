using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Newtonsoft.Json;

namespace Data.Entities
{
    public class EnvelopeDO : BaseDO, IEnvelope
    {
        class MergeDataDictionary : IDictionary<string, string>
        {
            private readonly EnvelopeDO _envelope;
            private Lazy<Dictionary<string, string>> _dictionary;

            private Dictionary<string, string> CreateInnerDictionary()
            {
                return string.IsNullOrEmpty(_envelope.MergeDataString) 
                    ? new Dictionary<string, string>() 
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(_envelope.MergeDataString);
            }

            private IDictionary<string, string> Dictionary
            {
                get
                {
                    return _dictionary.Value;
                }
            } 

            public MergeDataDictionary(EnvelopeDO envelope)
            {
                _envelope = envelope;
                Refresh();
            }

            public void Refresh()
            {
                _dictionary = new Lazy<Dictionary<string, string>>(CreateInnerDictionary, true);
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

        public const string SendGridHander = "Gmail";
        
        private readonly MergeDataDictionary _mergeData;

        public EnvelopeDO()
        {
            _mergeData = new MergeDataDictionary(this);
        }

        [NotMapped]
        public IDictionary<string, string> MergeData { get { return _mergeData; } }
        [NotMapped]
        IEmail IEnvelope.Email
        {
            get { return Email; }
            set { Email = (EmailDO)value; }
        }

        [Key]
        public int Id { get; set; }
        public string Handler { get; set; }
        public string TemplateName { get; set; }

        [ForeignKey("Email"), Required]
        public int? EmailID { get; set; }
        public virtual EmailDO Email { get; set; }
        
        [Column("MergeData")]
        public string MergeDataString { get; set; } // change notifications to MergeData dictionary should be added (by calling _mergeData.Refresh()) in future if needed.
    }
}
