using BLang.Syntax;

namespace BLang.Semantics
{
    public abstract class TypeInfo
    {
        protected TypeInfo(string name, int typeCode)
        {
            Name = name;
            TypeCode = typeCode;
        }

        public string Name { get; private init; }
        public int TypeCode { get; private set; }
    }

    public abstract class PrimitiveTypeInfo : TypeInfo
    {
        protected PrimitiveTypeInfo(ePrimitiveType type)
            : base (type.Name(), type.Code())
        {
            PrimitiveType = type;
        }

        public ePrimitiveType PrimitiveType { get; private init; }
    }

    public class I8TypeInfo : PrimitiveTypeInfo
    {
        public I8TypeInfo() : base(ePrimitiveType.I8)
        {
        }
    }

    public class I16TypeInfo : PrimitiveTypeInfo
    {
        public I16TypeInfo() : base(ePrimitiveType.I16)
        {
        }
    }

    public class I32TypeInfo : PrimitiveTypeInfo
    {
        public I32TypeInfo() : base(ePrimitiveType.I32)
        {
        }
    }

    public class I64TypeInfo : PrimitiveTypeInfo
    {
        public I64TypeInfo() : base(ePrimitiveType.I64)
        {
        }
    }

    public class F32TypeInfo : PrimitiveTypeInfo
    {
        public F32TypeInfo() : base(ePrimitiveType.F32)
        {
        }
    }

    public class F64TypeInfo : PrimitiveTypeInfo
    {
        public F64TypeInfo() : base(ePrimitiveType.F64)
        {
        }
    }

    public class BoolTypeInfo : PrimitiveTypeInfo
    {
        public BoolTypeInfo() : base(ePrimitiveType.Bool)
        {
        }
    }

    public class CharTypeInfo : PrimitiveTypeInfo
    {
        public CharTypeInfo() : base(ePrimitiveType.Char)
        {
        }
    }
}
