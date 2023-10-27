﻿using Microsoft.Xna.Framework;
using EvoSim.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoSim.ProjectContent.CellStuff
{
    public class CellStat
    {
        public float Value;

        public float Mutation;

        public float Mutation2;

        public float Min;

        public float Max;

        public bool Multiplicative;

        public static float Distance(CellStat a, CellStat b)
        {
            float range = a.Max - a.Min;

            return range * MathF.Abs(a.Value - b.Value);
        }

        public CellStat(float value, float mutation, float mutation2, float min, float max, bool multiplicative)
        {
            Value = value;
            Mutation = mutation;
            Mutation2 = mutation2;
            Min = min;
            Max = max;
            Multiplicative = multiplicative;
        }

        public void Mutate()
        {
            int mutationModifier = SceneManager.trainingMode ? 1 : 1;
            if (Multiplicative)
            {
                Value *= 1 + (Main.random.NextFloat(-Mutation, Mutation) * mutationModifier);
                Mutation *= 1 + (Main.random.NextFloat(-Mutation2, Mutation2) * mutationModifier);
            }
            else
            {
                Value += (Main.random.NextFloat(-Mutation, Mutation) * mutationModifier);
                Mutation += (Main.random.NextFloat(-Mutation2, Mutation2) * mutationModifier);
            }

            Value = Math.Clamp(Value, Min, Max);
        }

        public CellStat Combine(CellStat other)
        {
            CellStat ret = new CellStat(Value, Mutation, Mutation2, Min, Max, Multiplicative);
            ret.Value = MathHelper.Lerp(Value, other.Value, 0.5f);
            ret.Mutation = MathHelper.Lerp(Mutation, other.Mutation, 0.5f);
            ret.Mutation2 = MathHelper.Lerp(Mutation2, other.Mutation2, 0.5f);
            ret.Mutate();
            return ret;
        }

        public CellStat Duplicate()
        {
            CellStat ret = new CellStat(Value, Mutation, Mutation2, Min, Max, Multiplicative);
            ret.Mutate();
            return ret;
        }
    }
}
