namespace GraphEditor.Elements
{
    public class Vertex
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Vertex(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }
}
