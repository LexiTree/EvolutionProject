﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoSim.Interfaces;
using Microsoft.Xna.Framework.Input;
using EvoSim.ProjectContent.Terrain;
using EvoSim.Core.NeuralNetworks;
using EvoSim.ProjectContent.CellStuff;
using EvoSim.Helpers.HelperClasses;

namespace EvoSim.ProjectContent.Resources
{
    internal class FoodManager : ILoadable, IDraw, IUpdatable
    {
        #region priorities
        public float LoadPriority => 1.5f;

        public float DrawPriority => 0.7f;

        public float UpdatePriority => 1.0f;

        #endregion

        public static List<Food> foods = new List<Food>();

        public static float FoodEnergy => 70;

        public static Vector2 FoodSize => new Vector2(96, 96);

        public static int FoodAmount => 400;

        public static float FoodSpawnRate => 0.1f;


        private TimeCounter AutomaticFoodSpawner;
        private ButtonToggle ManualFoodSpawner;

        public void Load()
        {
            Main.drawables.Add(this);
            Main.updatables.Add(this);

            AutomaticFoodSpawner = new TimeCounter(FoodSpawnRate, new CounterAction((object o, ref float counter, float threshhold) =>
            {
                counter -= threshhold;
                NewFood(1);
            }));

            ManualFoodSpawner = new ButtonToggle(new PressingButton(() => Keyboard.GetState().IsKeyDown(Keys.E)), new ButtonAction((object o) => NewFood(FoodAmount)));
        }

        public void Unload()
        {

        }

        public void Update(GameTime gameTime)
        {
            ManualFoodSpawner.Update(this);
            AutomaticFoodSpawner.Update(this);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Food food in foods)
            {
                food.Draw(spriteBatch);
            }
        }

        private void NewFood(int numFood)
        {
            for (int i = 0; i < numFood; i++)
            {
                Vector2 pos = Vector2.Zero;
                pos.X = Main.random.Next((int)(TerrainManager.squareWidth * TerrainManager.gridWidth));
                pos.Y = Main.random.Next((int)(TerrainManager.squareHeight * TerrainManager.gridHeight));

                while (TerrainManager.ContainsRockWorld(pos))
                {
                    pos.X = Main.random.Next((int)(TerrainManager.squareWidth * TerrainManager.gridWidth));
                    pos.Y = Main.random.Next((int)(TerrainManager.squareHeight * TerrainManager.gridHeight));
                }
                Food newFood = new Food(FoodSize, FoodEnergy, Color.Yellow, pos);
                foods.Add(newFood);
            }
        }
    }
}
