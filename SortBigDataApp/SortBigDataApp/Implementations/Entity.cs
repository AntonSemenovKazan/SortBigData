namespace SortBigDataApp.Implementations
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

        public static bool IsLessOrEqual(Entity first, Entity second)
        {
            var compareResult = string.Compare(first.StringPart, second.StringPart, StringComparison.Ordinal);
            if (compareResult == 0)
            {
                return first.NumberPart <= second.NumberPart;
            }

            return compareResult < 0;
        }
    }
}
