namespace Heroes.ReplayParser
{
    public class Location
    {
        public Location(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        internal static Location FromEventFormat(uint x, uint y, uint z)
        {
            var X = x / 4096d;
            var Y = y / 4096d;
            var Z = ((z & 0x80000000) == 0) ? -1d : 1d;
            Z *= (z & 0x7fffffff) / 4096d;
            return new Location(X, Y, Z);
        }
    }
}
