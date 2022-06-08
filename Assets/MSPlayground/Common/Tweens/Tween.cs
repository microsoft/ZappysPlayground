namespace MSPlayground.Common
{
    /// <summary>
    /// TweenData with an initial and final target value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class Tween<T> : TweenData
    {
        public T From;
        public T To;
    }
}