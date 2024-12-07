namespace GraphEditor.Elements
{
    public class Edge
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }

        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
        }
    }
}
