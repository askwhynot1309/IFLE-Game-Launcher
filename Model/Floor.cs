using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFLEGameLauncher.Model
{
    public class Floor
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public float Height { get; set; }

        public float Width { get; set; }

        public float Length { get; set; }

        public string Status { get; set; } = null!;

    }
}
