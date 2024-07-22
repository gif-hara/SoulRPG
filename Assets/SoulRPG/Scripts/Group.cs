using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class Group<TKey, TValue>
    {
        [Serializable]
        public class Element
        {
            public TKey Key;

            public List<TValue> Value;
        }

        [SerializeField]
        private List<Element> list = new();
        public List<Element> List => list;

        private Dictionary<TKey, Element> dictionary;

        protected Func<TValue, TKey> keySelector;

        public Group(Func<TValue, TKey> keySelector)
        {
            this.keySelector = keySelector;
        }

        public void Set(IEnumerable<TValue> values)
        {
            list.Clear();
            foreach (var value in values)
            {
                var key = keySelector(value);
                var element = list.Find(x => x.Key.Equals(key));
                if (element == null)
                {
                    element = new Element { Key = key, Value = new List<TValue>() };
                    list.Add(element);
                }
                element.Value.Add(value);
            }
            dictionary = null;
        }

        public void Add(TValue value)
        {
            var key = keySelector(value);
            var element = list.Find(x => x.Key.Equals(key));
            if (element == null)
            {
                element = new Element { Key = key, Value = new List<TValue>() };
                list.Add(element);
            }
            element.Value.Add(value);
            if (dictionary != null)
            {
                dictionary.Add(key, element);
            }
        }

        public void Remove(TKey key)
        {
            var element = dictionary[key];
            list.Remove(element);
            if (dictionary != null)
            {
                dictionary.Remove(key);
            }
        }

        public void Clear()
        {
            list.Clear();
            dictionary = null;
        }

        public List<TValue> Get(TKey key)
        {
            InitializeIfNull();
            Assert.IsTrue(dictionary.ContainsKey(key), $"Key {key} is not found.");
            return dictionary[key].Value;
        }

        public bool ContainsKey(TKey key)
        {
            InitializeIfNull();
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out List<TValue> value)
        {
            InitializeIfNull();
            if (dictionary.TryGetValue(key, out var element))
            {
                value = element.Value;
                return true;
            }
            value = null;
            return false;
        }

        private void InitializeIfNull()
        {
            // UnityEditorの場合は毎回初期化する
#if UNITY_EDITOR
            dictionary = null;
#endif
            if (dictionary == null)
            {
                dictionary = new Dictionary<TKey, Element>();
                foreach (var element in list)
                {
                    dictionary.Add(element.Key, element);
                }
            }
        }
    }
}