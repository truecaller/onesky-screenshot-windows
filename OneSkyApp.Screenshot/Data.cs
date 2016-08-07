using System.Collections.Generic;

namespace OneSkyApp.Screenshot
{
    class Tag
    {
        public string key { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string file { get; set; }
    }

    class Screenshot
    {
        public string name { get; set; }
        public string image { get; set; }
        public IEnumerable<Tag> tags;
    }

    class Screenshots
    {
        public IEnumerable<Screenshot> screenshots;
    }
}
