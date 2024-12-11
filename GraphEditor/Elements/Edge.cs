namespace GraphEditor.Elements
{
    public class Edge
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public int Weight { get; set; } = 1;
        public int Capacity { get; set; } = 0;
        public bool Direction { get; set; } = false; // true - от конца к началу, false - наоборот
        public bool isDirectionShowed { get; set; } = false;

        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
        }
    }
}
