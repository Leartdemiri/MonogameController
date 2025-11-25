using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriphericalControl
{
    public class StickConfig
    {
        public string Name { get; }
        public float DeadZone { get; set; } = 0.15f;
        public float Sensitivity { get; set; } = 1.0f;
        public bool Inverted { get; set; } = false;

        public StickConfig(string name)
        {
            Name = name;
        }

        // Applique la calibration sur un axe (X ou Y) du stick
        public float Apply(float raw)
        {
            float value = raw;

            // Deadzone
            if (Math.Abs(value) < DeadZone)
                return 0f;

            float sign = Math.Sign(value);
            float magnitude = (Math.Abs(value) - DeadZone) / (1f - DeadZone);
            value = sign * magnitude;

            // Sensibilite
            value *= Sensitivity;

            // Clamp
            value = MathHelper.Clamp(value, -1f, 1f);

            // Inversion
            if (Inverted)
                value = -value;

            return value;
        }
    }
}
