namespace Parser.Tokenizers
{
    public abstract class Lookahead<T>
    {
        protected List<T> ItemBuffer = new();
        protected bool EndReached;
        protected T? EndOfInputIndicator;

        public T Current => GetNext(0);
        public T Next => GetNext(1);

        public T GetNext(int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentException("Parameter 'offset' cannot be less than 0.");
            }
            while (ItemBuffer.Count <= offset && !EndReached)
            {
                if (TryFetch(out T item)) { ItemBuffer.Add(item); }
                else { EndReached = true; }
            }

            if (offset >= ItemBuffer.Count)
            {
                if (EndOfInputIndicator is null) { EndOfInputIndicator = EndOfInput; }
                return EndOfInputIndicator;
            }
            else
            {
                return ItemBuffer.ElementAt(offset);
            }
        }

        protected abstract T EndOfInput { get; }
        protected abstract bool TryFetch(out T item);

        public T Consume()
        {
            T result = Current;
            Consume(1);
            return result;
        }

        public void Consume(int itemsNumber)
        {
            if (itemsNumber < 0) { throw new ArgumentException("Parameter 'itemsNumber' cannot be less than 0."); }
            while (itemsNumber-- > 0)
            {
                if (ItemBuffer.Count != 0) { ItemBuffer.RemoveAt(0); }
                else
                {
                    if (EndReached) { return; }

                    if (TryFetch(out T item)) { ItemBuffer.Add(item); }
                    else { EndReached = true; }
                }
            }
        }
    }
}