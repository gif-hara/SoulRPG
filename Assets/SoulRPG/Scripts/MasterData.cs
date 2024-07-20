using System;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "MasterData", menuName = "SoulRPG/MasterData")]
    public sealed class MasterData : ScriptableObject
    {
        [SerializeField]
        private Item.DictionaryList items;
        public Item.DictionaryList Items => items;

        [SerializeField]
        private BorderData.DictionaryList borders;
        public BorderData.DictionaryList Borders => borders;

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var database = await UniTask.WhenAll(
                GoogleSpreadSheetDownloader.DownloadAsync("Dungeon.Test")
            );

            borders.Set(JsonHelper.FromJson<BorderData>(database[0]));
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("End MasterData Update");
        }
#endif

        [Serializable]
        public class Item
        {
            public int Id;

            public string Name;

            public Sprite Icon;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, Item>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class BorderData
        {
            public int x;

            public int y;

            public Border borders;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<Vector2Int, BorderData>
            {
                public DictionaryList() : base(x => new Vector2Int(x.x, x.y)) { }
            }
        }

        [Serializable]
        public class Border
        {
            public bool top;

            public bool bottom;

            public bool left;

            public bool right;
        }
    }
}
