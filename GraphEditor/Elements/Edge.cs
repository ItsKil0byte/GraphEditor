namespace GraphEditor.Elements
{
    public class Edge
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public int Weight { get; set; } = 1;

        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
        }
    }
}
