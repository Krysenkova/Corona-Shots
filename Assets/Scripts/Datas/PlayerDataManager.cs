using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Data
{
    public class PlayerDataManager : MonoBehaviour
    {
        private PlayerData _data;
        private BinaryFormatter _formatter = new BinaryFormatter();

        // Start is called before the first frame update
        void Start()
        {
            if (!SceneParameters.UseLoadedGame)
            {
                _data = new PlayerData();
            }
            else
            {
                _data = SceneParameters.PlayerData;

                // cleanup
                SceneParameters.PlayerData = null;
                // SceneParameters.UseLoadedGame = false;
            }
        }

        /// <summary>
        /// Get a specific weapon by its index. Returns null if weapon with provided index
        /// is not found
        /// </summary>
        public WeaponData GetWeapon(Weapons.WeaponTags weaponIndex)
        {
            foreach (WeaponData weaponData in _data.WeaponDatas)
            {
                if (weaponData.GetWeaponIndex() == weaponIndex)
                {
                    return weaponData;
                }
            }

            return null;
        }

        /// <summary>
        /// Get list of available weapons
        /// </summary>
        public WeaponData[] GetAllWeapons()
        {
            return _data.WeaponDatas;
        }

        /// <summary>
        /// Load resource from `/Resources` folder at runtime
        /// </summary>
        public void LoadResource(string filename, Transform parent, Vector3 position)
        {
            Debug.Log("Trying to load LevelPrefab from file (" + filename + ")...");
            var loadedObject = Resources.Load(filename);
            if (loadedObject == null)
            {
                throw new FileNotFoundException("...no file found - please check the configuration");
            }

            GameObject go = Instantiate(loadedObject, Vector3.zero, Quaternion.identity) as GameObject;
            go.transform.SetParent(parent.transform);
            go.transform.position = parent.position;
            go.transform.position += position;
        }

        /// <summary>
        /// Save player data by serializing the `PlayerData` class and saving it to `player.data` file
        /// </summary>
        public void SaveGame()
        {
            string path = Application.persistentDataPath + "/player.data";
            FileStream stream = new FileStream(path, FileMode.Create);

            _formatter.Serialize(stream, _data);
            stream.Close();
        }

        /// <summary>
        /// Load player data by deserializing the "player.data" file
        /// </summary>
        public void LoadGame(bool reloadScene)
        {
            string path = Application.persistentDataPath + "/player.data";
            if (File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                PlayerData playerData = _formatter.Deserialize(stream) as PlayerData;
                if (reloadScene)
                {
                    SceneParameters.PlayerData = playerData;
                    // reload scene
                    SceneParameters.UseLoadedGame = true;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // todo: integrate scene name to `LoadScene`
                }
                else
                {
                    _data = playerData;
                }

                stream.Close();
            }
            else
            {
                Debug.Log("No save files!");
                return;
            }
        }

        /// <summary>
        /// Save Settings
        /// </summary>
        public void SaveSettings(float vfxVolume, float musicVolume, float difficulty)
        {
            Debug.Log("Saving vfx: " + vfxVolume);
            _data.UpdateSettings(vfxVolume, musicVolume, difficulty);
            SaveGame();
            SceneParameters.UseLoadedGame = true;
        }
        public bool IsDataLoaded()
        {
            return _data != null;
        }

        /// <summary>
        /// Load settings
        /// returns [_vfxVolume, _musicVolume, _difficulty]
        /// </summary>
        public float[] LoadSettings()
        {

            if (!SceneParameters.UseLoadedGame)
            {
                LoadGame(false);
            }
            return _data.GetSettings();
        }

        public float GetVfxVolume()
        {
            float[] settings = LoadSettings();
            return settings[0];
        }
    }
}