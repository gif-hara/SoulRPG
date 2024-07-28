using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace HK
{
    [Serializable]
    public sealed class DictionaryData<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;

        private readonly Func<TValue, TKey> idSelector;

        public DictionaryData(Func<TValue, TKey> idSelector)
        {
            this.idSelector = idSelector;
            dictionary = null;
        }

        public void Set(IEnumerable<TValue> list)
        {
            dictionary ??= new Dictionary<TKey, TValue>();
            dictionary.Clear();
            foreach (var item in list)
            {
                dictionary.Add(idSelector(item), item);
            }
        }

        public void Add(TValue value)
        {
            dictionary.Add(idSelector(value), value);
        }

        public void Remove(TKey key)
        {
            dictionary.Remove(key);
        }

        public void Remove(TValue value)
        {
            var key = idSelector(value);
            dictionary.Remove(key);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public TValue Get(TKey key)
        {
            Assert.IsTrue(dictionary.ContainsKey(key), $"key={key}");
            return dictionary[key];
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public bool IsNull()
        {
            return dictionary == null;
        }
    }

    [Serializable]
    public abstract class DictionaryList<TKey, TValue>
    {
        [SerializeField]
        private List<TValue> list;
        public IReadOnlyList<TValue> List => list;

        private DictionaryData<TKey, TValue> dictionaryData;

        public DictionaryList(Func<TValue, TKey> idSelector)
        {
            dictionaryData = new DictionaryData<TKey, TValue>(idSelector);
        }

        public void Set(IEnumerable<TValue> list)
        {
            this.list = list.ToList();
            dictionaryData.Set(list);
        }

        public void Add(TValue value)
        {
            list.Add(value);
            dictionaryData.Add(value);
        }

        public void Remove(TValue value)
        {
            list.Remove(value);
            dictionaryData.Remove(value);
        }

        public void Clear()
        {
            list.Clear();
            dictionaryData.Clear();
        }

        public TValue Get(TKey key)
        {
            InitializeIfNull();
            return dictionaryData.Get(key);
        }

        public bool ContainsKey(TKey key)
        {
            InitializeIfNull();
            return dictionaryData.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            InitializeIfNull();
            return dictionaryData.TryGetValue(key, out value);
        }

        private void InitializeIfNull()
        {
            // UnityEditorの場合は毎回初期化する
#if UNITY_EDITOR
            dictionaryData.Set(list);
#else
            if (dictionaryData.IsNull())
            {
                dictionaryData.Set(list);
            }
#endif
        }
    }
}