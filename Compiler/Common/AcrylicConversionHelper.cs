namespace Common
{
    // Acrylic does not support ? syntax for nullable types...yet.
    // This class helps keep the transpilation simple.

    public class NullableBoolean
    {
        public bool Value { get; private set; }
        public NullableBoolean(bool value)
        {
            this.Value = value;
        }
    }

    public class NullableInteger
    {
        public int Value { get; private set; }
        public NullableInteger(int value)
        {
            this.Value = value;
        }
    }
}
