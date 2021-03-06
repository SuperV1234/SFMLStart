﻿#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SFML.Audio;
using SFML.Graphics;
using SFMLStart.Utilities;

#endregion

namespace SFMLStart.Data
{
    public static class Assets
    {
        private static Dictionary<string, Image> _images;
        private static Dictionary<string, Texture> _textures;
        private static Dictionary<string, Animation> _animations;
        private static Dictionary<string, Tileset> _tilesets;
        private static Dictionary<string, Music> _music;

        static Assets() { Initialize(); }

        internal static Dictionary<string, Sound> Sounds { get; private set; }

        #region Initialization Methods
        private static void Initialize()
        {
            _images = new Dictionary<string, Image>();
            _textures = new Dictionary<string, Texture>();
            _tilesets = new Dictionary<string, Tileset>();
            _animations = new Dictionary<string, Animation>();
            Sounds = new Dictionary<string, Sound>();
            _music = new Dictionary<string, Music>();
            if (Settings.Assets.LoadImages) InitializeImages();
            if (Settings.Assets.LoadTextures) InitializeTextures();
            if (Settings.Assets.LoadTilesets) InitializeTilesets();
            if (Settings.Assets.LoadAnimations) InitializeAnimations();
            if (Settings.Assets.LoadSounds) InitializeSounds();
            if (Settings.Assets.LoadMusic) InitializeMusic();
        }
        private static void InitializeImages()
        {
            _images.Add("missingimage", new Image(16, 16, Color.Magenta));

            var imageDirectory = new DirectoryInfo(Settings.Paths.Images);
            if (!imageDirectory.Exists)
            {
                Utils.Log(string.Format("directory <<{0}>> does not exist", imageDirectory.FullName), "InitializeImages");
                return;
            }

            var imagesDirectories = new List<DirectoryInfo> {imageDirectory};
            imagesDirectories.AddRange(imageDirectory.GetDirectories("*", SearchOption.AllDirectories));

            foreach (var directoryInfo in imagesDirectories)
            {
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    if (!Settings.Assets.ImagesExtensions.Contains(fileInfo.Extension.ToLower())) continue;
                    var directoryName = directoryInfo.FullName.Replace(imageDirectory.FullName, @"");
                    if (!directoryInfo.FullName.EndsWith("/"))
                        directoryName = directoryName.Insert(directoryName.Length, "/");
                    _images.Add(directoryName + GetAssetName(fileInfo), new Image(fileInfo.FullName));
                    Utils.Log(string.Format("image <<{0}>> loaded", directoryName + GetAssetName(fileInfo)),
                              "InitializeImages");
                }
            }
        }
        private static void InitializeTextures()
        {
            foreach (var imageName in _images.Keys)
            {
                _textures.Add(imageName, new Texture(_images[imageName]));
                Utils.Log(string.Format("texture <<{0}>> created", imageName), "InitializeTextures", ConsoleColor.Green);
            }
        }
        private static void InitializeTilesets()
        {
            var tilesetDirectory = new DirectoryInfo(Settings.Paths.Tilesets);
            if (!tilesetDirectory.Exists)
            {
                Utils.Log(string.Format("directory <<{0}>> does not exist", tilesetDirectory.FullName),
                          "InitializeTilesets", ConsoleColor.Magenta);
                return;
            }

            var tilesetDirectories = new List<DirectoryInfo> {tilesetDirectory};
            tilesetDirectories.AddRange(tilesetDirectory.GetDirectories("*.", SearchOption.AllDirectories));

            foreach (var directoryInfo in tilesetDirectories)
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    if (!Settings.Assets.TilesetExtensions.Contains(fileInfo.Extension.ToLower())) continue;
                    LoadTileset(fileInfo);
                }
        }
        private static void InitializeAnimations()
        {
            var animationDirectory = new DirectoryInfo(Settings.Paths.Animations);
            if (!animationDirectory.Exists)
            {
                Utils.Log(string.Format("directory <<{0}>> does not exist", animationDirectory.FullName),
                          "InitializeAnimations", ConsoleColor.Yellow);
                return;
            }

            var animationDirectories = new List<DirectoryInfo> {animationDirectory};
            animationDirectories.AddRange(animationDirectory.GetDirectories("*.", SearchOption.AllDirectories));

            foreach (var directoryInfo in animationDirectories)
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    if (!Settings.Assets.AnimationExtensions.Contains(fileInfo.Extension.ToLower())) continue;
                    LoadAnimation(fileInfo);
                }
        }
        private static void InitializeSounds()
        {
            var soundsDirectory = new DirectoryInfo(Settings.Paths.Sounds);
            if (!soundsDirectory.Exists)
            {
                Utils.Log(string.Format("directory <<{0}>> does not exist", soundsDirectory.FullName),
                          "InitializeSounds", ConsoleColor.White);
                return;
            }

            var soundsDirectories = new List<DirectoryInfo> {soundsDirectory};
            soundsDirectories.AddRange(soundsDirectory.GetDirectories("*.", SearchOption.AllDirectories));

            foreach (var directoryInfo in soundsDirectories)
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    if (!Settings.Assets.SoundsExtensions.Contains(fileInfo.Extension.ToLower())) continue;
                    var directoryName = directoryInfo.FullName.Replace(soundsDirectory.FullName, @"");
                    if (!directoryInfo.FullName.EndsWith("/"))
                        directoryName = directoryName.Insert(directoryName.Length, "/");
                    var soundBuffer = new SoundBuffer(fileInfo.FullName);
                    Sounds.Add(directoryName + GetAssetName(fileInfo), new Sound(soundBuffer));
                    Utils.Log(string.Format("sound <<{0}>> loaded", directoryName + GetAssetName(fileInfo)),
                              "InitializeSounds", ConsoleColor.White);
                }
        }
        private static void InitializeMusic()
        {
            var musicDirectory = new DirectoryInfo(Settings.Paths.Music);
            if (!musicDirectory.Exists)
            {
                Utils.Log(string.Format("directory <<{0}>> does not exist", musicDirectory.FullName), "InitializeMusic",
                          ConsoleColor.Red);
                return;
            }

            var musicDirectories = new List<DirectoryInfo> {musicDirectory};
            musicDirectories.AddRange(musicDirectory.GetDirectories("*.", SearchOption.AllDirectories));

            foreach (var directoryInfo in musicDirectories)
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    if (!Settings.Assets.MusicExtensions.Contains(fileInfo.Extension.ToLower())) continue;
                    var directoryName = directoryInfo.FullName.Replace(musicDirectory.FullName, @"");
                    if (!directoryInfo.FullName.EndsWith("/"))
                        directoryName = directoryName.Insert(directoryName.Length, "/");
                    _music.Add(directoryName + GetAssetName(fileInfo), new Music(fileInfo.FullName));
                    Utils.Log(string.Format("sound <<{0}>> loaded", directoryName + GetAssetName(fileInfo)),
                              "InitializeMusic", ConsoleColor.Red);
                }
        }
        #endregion

        #region Utility Methods
        private static void LoadTileset(FileInfo mFileInfo)
        {
            var streamReader = File.OpenText(mFileInfo.FullName);

            var fileText = Regex.Replace(streamReader.ReadToEnd(), Settings.Assets.Regex, "");
            var separationByGroups = fileText.Split(Settings.Assets.SeparatorGroup);
            var separationByItems = separationByGroups[0].Split(Settings.Assets.SeparatorItem);
            var tileWidth = Int32.Parse(separationByItems[0]);
            var tileHeight = Int32.Parse(separationByItems[1]);
            var tileSeparation = Int32.Parse(separationByGroups[1]);

            _tilesets.Add(GetAssetName(mFileInfo), new Tileset(tileWidth, tileHeight, tileSeparation));
            Utils.Log(string.Format("tileset <<{0}>> loaded", GetAssetName(mFileInfo)), "InitializeTilesets",
                      ConsoleColor.Magenta);

            for (var iY = 2; iY < separationByGroups.Length; iY++)
                for (var iX = 0; iX < separationByGroups[iY].Split(',').Length; iX++)
                {
                    var label = separationByGroups[iY].Split(',')[iX];
                    if (String.IsNullOrEmpty(label)) continue;
                    _tilesets[GetAssetName(mFileInfo)].SetLabel(label, iX, iY - 2);
                }
        }
        private static void LoadAnimation(FileInfo mFileInfo)
        {
            var streamReader = File.OpenText(mFileInfo.FullName);

            var fileText = Regex.Replace(streamReader.ReadToEnd(), Settings.Assets.Regex, "");
            var separationByGroups = fileText.Split(Settings.Assets.SeparatorGroup);
            var animationLooped = Int32.Parse(separationByGroups[0]) == 1;
            var animationPingpong = Int32.Parse(separationByGroups[1]) == 1;
            var separationByItems = separationByGroups[2].Split(Settings.Assets.SeparatorItem);

            var result = new Animation(animationLooped, animationPingpong);
            for (var i = 0; i < separationByItems.Length; i++)
                if ((i + 1)%2 == 0) result.AddStep(separationByItems[i - 1], Int32.Parse(separationByItems[i]));
            _animations.Add(GetAssetName(mFileInfo), result);
            Utils.Log(string.Format("animation <<{0}>> created", GetAssetName(mFileInfo)), "InitializeAnimations",
                      ConsoleColor.Yellow);
        }
        private static string GetAssetName(FileSystemInfo mFileInfo) { return mFileInfo.Name.Replace(mFileInfo.Extension, ""); }
        #endregion

        #region Getters
        public static Texture GetTexture(string mTextureName)
        {
#if CHECK_ASSETS
            if (!_textures.ContainsKey(mTextureName))
            {
                Utils.Log(string.Format("Load: missing texture {0}", mTextureName), "Asset error");
                return _textures["missingimage"];
            }
#endif


            return _textures[mTextureName.Replace('\\', '/')];
        }
        public static Animation GetAnimation(string mAnimationName)
        {
#if CHECK_ASSETS
            if(!_animations.ContainsKey(mAnimationName))
            {
                Utils.Log(string.Format("Load: missing animation {0}", mAnimationName), "Asset error");
                return new Animation();
            }
#endif

            return _animations[mAnimationName.Replace('\\', '/')].Clone();
        }
        public static Tileset GetTileset(string mTilesetName)
        {
#if CHECK_ASSETS
            if(!_tilesets.ContainsKey(mTilesetName))
            {
                Utils.Log(string.Format("Load: missing tileset {0}", mTilesetName), "Asset error");
                return new Tileset(16,16,0);
            }
#endif

            return _tilesets[mTilesetName.Replace('\\', '/')];
        }
        public static Sound GetSound(string mSoundName)
        {
#if CHECK_ASSETS
            if(!Sounds.ContainsKey(mSoundName))
            {
                Utils.Log(string.Format("Load: missing sound {0}", mSoundName), "Asset error");
                return new Sound();
            }
#endif

            return Sounds[mSoundName.Replace('\\', '/')];
        }
        public static Music GetMusic(string mMusicName)
        {
#if CHECK_ASSETS
            if(!_music.ContainsKey(mMusicName))
            {
                Utils.Log(string.Format("Load: missing music {0}", mMusicName), "Asset error");
                Utils.Log(string.Format("Load: missing music {0} - fatal error", mMusicName), "Asset error");
            }
#endif

            return _music[mMusicName.Replace('\\', '/')];
        }
        #endregion
    }
}