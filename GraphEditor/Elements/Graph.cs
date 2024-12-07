namespace GraphEditor.Elements
{
    public class Graph
    {
        public List<Vertex> Vertices { get; } = [];
        public List<Edge> Edges { get; } = [];

        public void AddVertex(Vertex vertex) => Vertices.Add(vertex);

        // NOTE: возможно будут переписаны все методы под bool
        public bool AddEdge(Edge edge)
        {
            if (!Edges.Any(e => (e.Start == edge.Start && e.End == edge.End) || (e.Start == edge.End && e.End == edge.Start)))
            {
                Edges.Add(edge);
                return true;
            }

            return false;
        }
        public void RemoveVertex(Vertex vertex)
        {
            Edges.RemoveAll(e => e.Start == vertex || e.End == vertex);
            Vertices.Remove(vertex);
        }
        public void RemoveEdge(Edge edge) => Edges.Remove(edge);
    }
}
