﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1.Interfaces;
using Microsoft.Xna.Framework.Input;
using Project1.ProjectContent.Terrain;
using Project1.Core.NeuralNetworks;
using Project1.Core.NeuralNetworks.NEAT;
using Project1.Helpers;
using Project1.Helpers.HelperClasses;

namespace Project1.ProjectContent.CellStuff
{
    internal class CellManager : ILoadable, IDraw, IUpdatable
    {
        #region priorities
        public float LoadPriority => 1.5f;

        public float DrawPriority => 1.0f;

        public float UpdatePriority => 1.0f;

        #endregion

        public static bool trainingMode = true;

        public static List<Cell> cells = new List<Cell>();

        public int StartingCells => 30;

        public static Simulation simulation;

        private ButtonToggle TrainingModeToggle;
        private ButtonToggle SimStarter;

        public void Load()
        {
            Game1.drawables.Add(this);
            Game1.updatables.Add(this);
            SimStarter = new ButtonToggle(new PressingButton(() => Keyboard.GetState().IsKeyDown(Keys.Space)), new ButtonAction((object o) => NewCells(StartingCells)));
            TrainingModeToggle = new ButtonToggle(new PressingButton(() => Keyboard.GetState().IsKeyDown(Keys.T)), new ButtonAction((object o) => trainingMode = !trainingMode));
        }

        public void Unload()
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string debugInfo = "Total Cells: " + cells.Count.ToString();
            if (simulation != null && simulation != default)
            {
                debugInfo += "\n Global Sharing: " + (simulation as NEATSimulation).globalSharing.ToString();
            }
            debugInfo += "\n Training mode: " + trainingMode.ToString();
            DrawHelper.DrawText(spriteBatch, debugInfo, Color.Black, new Vector2(50, 50), Vector2.One, false);
            simulation?.Agents.ForEach(cell => (cell as Cell).Draw(spriteBatch));
        }

        public void Update(GameTime gameTime)
        {
            SimStarter.Update(this);
            TrainingModeToggle.Update(this);

            simulation?.Update();

            var cellsToDestroy = cells.Where(n => !n.IsActive()).ToList();
            cellsToDestroy.ForEach(n => cells.Remove(n));
        }

        private void NewCells(int numCells)
        {
            var newsim = new CellNeatSimulation<Cell>(Cell.INPUTNUM, Cell.OUTPUTNUM, numCells, (IDna) => CreateRawCell(IDna), 1f);
            newsim.Deploy();
            simulation = newsim;
            
            /*for (int i = 0; i < numCells; i++)
            {
                Vector2 pos = Vector2.Zero;
                pos.X = Game1.random.Next((int)(TerrainManager.squareWidth * TerrainManager.gridWidth));
                pos.Y = Game1.random.Next((int)(TerrainManager.squareHeight * TerrainManager.gridHeight));
                Cell newCell = new Cell(Color.White, Vector2.One * 32, pos, 500, 1000);
                cells.Add(newCell);
            }*/
        }

        private Cell CreateRawCell(IDna dna)
        {
            Vector2 pos = Vector2.Zero;
            pos.X = Game1.random.Next((int)(TerrainManager.squareWidth * TerrainManager.gridWidth));
            pos.Y = Game1.random.Next((int)(TerrainManager.squareHeight * TerrainManager.gridHeight));

            while (TerrainManager.ContainsRockWorld(pos))
            {
                pos.X = Game1.random.Next((int)(TerrainManager.squareWidth * TerrainManager.gridWidth));
                pos.Y = Game1.random.Next((int)(TerrainManager.squareHeight * TerrainManager.gridHeight));
            }
            Cell newCell = new Cell(new Color(0, 0, 1.0f), Vector2.One * 32, pos, 500, dna);
            return newCell;
        }
    }
}
