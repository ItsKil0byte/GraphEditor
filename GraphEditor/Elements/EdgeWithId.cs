namespace GraphEditor.Elements
{
    public class EdgeWithId : Edge
    {
        public int Id { get; set; }

        public EdgeWithId(Vertex start, Vertex end, int id) : base(start, end)
        {
            Id = id;
        }
    }
}