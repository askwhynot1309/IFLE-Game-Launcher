using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFLEGameLauncher.Model
{
    public class Organization
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int UserLimit { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
