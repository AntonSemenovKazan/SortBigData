namespace SortBigDataApp
{
    public class InMemoryMergeSort
    {
        public void Sort(Queue<List<Entity>> queue)
        {
            while (queue.Count > 1)
            {
                var left = queue.Dequeue();
                var right = queue.Dequeue();

                var result = new List<Entity>();

                var leftEnumerator = left.GetEnumerator();
                var rightEnumerator = right.GetEnumerator();

                var leftHasItems = leftEnumerator.MoveNext();
                var rightHasItems = rightEnumerator.MoveNext();
                while (leftHasItems && rightHasItems)
                {
                    if (IsLessOrEqual(leftEnumerator.Current, rightEnumerator.Current))
                    {
                        result.Add(leftEnumerator.Current);
                        leftHasItems = leftEnumerator.MoveNext();
                    }
                    else
                    {
                        result.Add(rightEnumerator.Current);
                        rightHasItems = rightEnumerator.MoveNext();
                    }
                }

                while (leftHasItems)
                {
                    result.Add(leftEnumerator.Current);
                    leftHasItems = leftEnumerator.MoveNext();
                }

                while (rightHasItems)
                {
                    result.Add(rightEnumerator.Current);
                    rightHasItems = rightEnumerator.MoveNext();
                }

                queue.Enqueue(result);
            }
        }

        private static bool IsLessOrEqual(Entity first, Entity second)
        {
            var compareResult = string.Compare(first.StringPart, second.StringPart);
            if (compareResult == 0)
            {
                return first.NumberPart <= second.NumberPart;
            }

            return compareResult < 0;
        }
    }
}
