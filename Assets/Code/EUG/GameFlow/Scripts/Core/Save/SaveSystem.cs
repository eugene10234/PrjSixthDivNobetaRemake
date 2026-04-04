using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlow.Core.Save
{
    public sealed class SaveSystem : ISaveSystem
    {
        private const string SaveKey = "game_save_v1";
        private SaveData _cache;

        public bool HasSave => PlayerPrefs.HasKey(SaveKey);

        public void Save(SaveData data)
        {
            _cache = data;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public SaveData Load()
        {
            if (_cache != null) return _cache;
            if (!HasSave) return null;
            _cache = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
            return _cache;
        }

        public void Delete()
        {
            _cache = null;
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }
}
