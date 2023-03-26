using SortBigDataApp;

public class ArrayInMemoryMergeSort
{
    public Entity[] SortArray(Entity[] array, int left, int right)
    {
        if (left < right)
        {
            int middle = left + (right - left) / 2;
            SortArray(array, left, middle);
            SortArray(array, middle + 1, right);
            MergeArray(array, left, middle, right);
        }
        return array;
    }

    public void MergeArray(Entity[] array, int left, int middle, int right)
    {
        var leftArrayLength = middle - left + 1;
        var rightArrayLength = right - middle;
        var leftTempArray = new Entity[leftArrayLength];
        var rightTempArray = new Entity[rightArrayLength];
        int i, j;
        for (i = 0; i < leftArrayLength; ++i)
            leftTempArray[i] = array[left + i];
        for (j = 0; j < rightArrayLength; ++j)
            rightTempArray[j] = array[middle + 1 + j];
        i = 0;
        j = 0;
        int k = left;
        while (i < leftArrayLength && j < rightArrayLength)
        {
            if (Entity.IsLessOrEqual(leftTempArray[i], rightTempArray[j]))
            {
                array[k++] = leftTempArray[i++];
            }
            else
            {
                array[k++] = rightTempArray[j++];
            }
        }
        while (i < leftArrayLength)
        {
            array[k++] = leftTempArray[i++];
        }
        while (j < rightArrayLength)
        {
            array[k++] = rightTempArray[j++];
        }
    }
}
