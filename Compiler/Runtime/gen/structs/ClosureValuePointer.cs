﻿using Interpreter.Structs;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Structs
{
    public class ClosureValuePointer
    {
        public Value value;

        public ClosureValuePointer(Value value)
        {
            this.value = value;
        }
    }

}
