﻿using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.NeuralNetworks;
using Project1.Helpers;
using Project1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.ProjectContent.Resources
{
    internal class Food
    {
        public float width;

        public float height;

        public float energy;

        public Color color;

        public Vector2 position;

        public Vector2 size => new Vector2(width, height);

        public Vector2 Center
        {
            get
            {
                return position + (size / 2.0f);
            }
            set
            {
                position = value - (size / 2.0f);
            }
        }

        public Food(float width, float height, float energy, Color color, Vector2 position, Vector2 center)
        {
            this.width = width;
            this.height = height;
            this.color = color;
            this.position = position;
            this.energy = energy;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawHelper.DrawPixel(spriteBatch, color, position, width, height);
        }
    }
}
