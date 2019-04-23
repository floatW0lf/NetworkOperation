namespace NetworkOperation
{
    public struct SessionProperty
    {
        public SessionProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
    }
}