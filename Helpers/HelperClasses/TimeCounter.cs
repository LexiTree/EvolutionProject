﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Helpers.HelperClasses
{
    public delegate void CounterAction(object sender, ref float counter, float threshhold);
    internal class TimeCounter
    {
        public float Threshhold;
        public CounterAction Action;
        private float Counter;

        public TimeCounter(float threshhold, CounterAction action)
        {
            Threshhold = threshhold;
            Action = action;
        }

        public void Update(object sender)
        {
            Counter += Game1.delta;
            while (Counter > Threshhold)
            {
                Action.Invoke(sender, ref Counter, Threshhold);
            }
        }
    }
}
