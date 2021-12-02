namespace Interpreter.Structs
{
    public class ClassValue
    {
        public bool isInterface;
        public int classId;

        public ClassValue(bool isInterface, int classId)
        {
            this.isInterface = isInterface;
            this.classId = classId;
        }
    }

}
