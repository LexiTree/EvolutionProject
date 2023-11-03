﻿
#region global usings
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using EvoSim.ProjectContent.CellStuff;
global using EvoSim.ProjectContent.Camera;
global using EvoSim.ProjectContent.SceneStuff;
global using EvoSim.Interfaces;
global using System;
global using System.Collections.Generic;
global using System.Linq;
#endregion

using Microsoft.Xna.Framework.Input;
using EvoSim.Helpers;
using SharpDX.X3DAudio;
using System.Reflection;
using System.Runtime.CompilerServices;
using EvoSim.Helpers.HelperClasses;

namespace EvoSim
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static Random random;

        public static float delta;
        private static float oldTime;

        public static Vector2 ScreenSize => new Vector2(1900, 950);

        internal static List<ILoadable> loadCache;

        internal static List<IDraw> drawables = new List<IDraw>();

        internal static List<IUpdate> updatables = new List<IUpdate>();

        public static ButtonToggle ForceSim;
        public static bool forcingSim = false;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)ScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int)ScreenSize.Y;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            ForceSim = new ButtonToggle(new PressingButton(() => Keyboard.GetState().IsKeyDown(Keys.U)), new ButtonAction((object o) => forcingSim = true));
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            random = new Random();

            loadCache = new List<ILoadable>();

            foreach (Type type in Assembly.GetAssembly(typeof(Main)).GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(ILoadable)))
                {
                    object instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as ILoadable);
                }
            }

            loadCache.Sort((n, t) => n.LoadPriority.CompareTo(t.LoadPriority));

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load();
            }

            updatables.Sort((n, t) => n.UpdatePriority.CompareTo(t.UpdatePriority));
            drawables.Sort((n, t) => n.DrawPriority.CompareTo(t.DrawPriority));


            DrawHelper.MagicPixel = Content.Load<Texture2D>("sprites/MagicPixel");
            DrawHelper.Arial = Content.Load<SpriteFont>("fonts/Arial");
        }

        protected override void Update(GameTime gameTime)
        {
            ForceSim.Update(this);
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (forcingSim)
            {
                delta = 0.004f;
                for (int i = 0; i < 50000; i++)
                {
                    foreach (IUpdate updatable in updatables)
                    {
                        updatable.Update(gameTime);
                    }
                }
                forcingSim = false;
                base.Update(gameTime);
                return;
            }

            foreach (IUpdate updatable in updatables)
            {
                updatable.Update(gameTime);
            }

            oldTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            
            foreach (IDraw drawable in drawables)
            {
                drawable.Draw(_spriteBatch);
            }    

            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
