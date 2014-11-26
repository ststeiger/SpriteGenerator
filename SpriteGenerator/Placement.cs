
using System.Linq;
// using SpriteGenerator.Utility;

namespace SpriteGenerator
{
    public class Placement
    {
		private System.Collections.Generic.List<SpriteGenerator.Utility.Module> modules;

		public Placement(System.Collections.Generic.List<SpriteGenerator.Utility.Module> _modules)
        {
            modules = _modules;
        }

        /// <summary>
        /// Gets the half perimeter of the placement.
        /// </summary>
        public int Perimeter
        {
            get { return modules.Max(m => m.X + m.Width) + modules.Max(m => m.Y + m.Height); }
        }

        /// <summary>
        /// Gets the width of the palcement.
        /// </summary>
        public int Width
        {
            get { return modules.Max(m => m.X + m.Width); }
        }

        /// <summary>
        /// Gets the height of the placement.
        /// </summary>
        public int Height
        {
            get { return modules.Max(m => m.Y + m.Height); }
        }

        /// <summary>
        /// Gets the modules in the placement.
        /// </summary>
		public System.Collections.Generic.List<SpriteGenerator.Utility.Module> Modules
        {
            get { return modules; }
        }
    }
}
