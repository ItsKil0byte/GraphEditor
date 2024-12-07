namespace GraphEditor.Elements
{
    public class Graph
    {
        public List<Vertex> Vertices { get; } = [];
        public List<Edge> Edges { get; } = [];

        public void AddVertex(Vertex vertex) => Vertices.Add(vertex);
        public void AddEdge(Edge edge) => Edges.Add(edge);
        public void RemoveVertex(Vertex vertex)
        {
            Edges.RemoveAll(e => e.Start == vertex || e.End == vertex);
            Vertices.Remove(vertex);
        }
        public void RemoveEdge(Edge edge) => Edges.Remove(edge);
    }
}
