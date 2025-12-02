using System.Collections.Generic;

namespace PeriphericalControl
{
    public class Profile
    {
        // Liste des configurations de sticks
        public List<StickConfig> Sticks { get; set; }

        /// <summary>
        /// constructeur
        /// </summary>
        public Profile()
        {
            Sticks = new List<StickConfig>();
        }
    }
}
