using System.Collections.Generic;

namespace Parser.ParseTree
{
    // Represents a property setter or getter.
    // I have the urge to rename this class to PropertyEtter
    public class PropertyMember : Node, ICodeContainer
    {
        public bool IsGetter { get { return this.FirstToken.Value == "get"; } }
        public bool IsSetter { get { return !this.IsGetter; } }
        public ModifierCollection Modifiers { get; private set; }
        public Executable[] Code { get; set; }
        public bool IsStubImplementation { get { return this.Code == null; } }
        public HashSet<string> ArgumentNameLookup { get; private set; }

        public PropertyMember(Token setOrGetToken, PropertyDefinition propertyDefinition, ModifierCollection modifiers)
            : base(setOrGetToken, propertyDefinition)
        {
            this.Modifiers = modifiers;
            this.ArgumentNameLookup = new HashSet<string>() { };
            if (this.IsSetter) this.ArgumentNameLookup.Add("value");
        }

        public List<Lambda> Lambdas { get; private set; }
    }
}
