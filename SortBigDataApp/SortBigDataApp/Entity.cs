namespace SortBigDataApp
{
    public class Entity
    {

        public Entity(string source)
        {
            var parsed = source.Split(". ");
            StringPart = parsed[1];
            NumberPart = int.Parse(parsed[0]);
        }

        public int NumberPart { get; }

        public string StringPart { get; }
    }
}
