public struct Dimensions : IEquatable<Dimensions>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Dimensions(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int Fittable => X < Y ? X : Y;

    public bool Equals(Dimensions other)
    {
        return other.X == X && other.Y == Y;
    }

    public override bool Equals(object obj)
    {
        return Equals((Dimensions)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Dimensions lhs, Dimensions rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Dimensions lhs, Dimensions rhs)
    {
        return !(lhs == rhs);
    }
}