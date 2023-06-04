namespace BLang.Syntax
{
    public enum ePrimitiveType
    {
        [PrimitiveType("i8", 1)]
        I8,
        [PrimitiveType("i16", 2)]
        I16, 
        [PrimitiveType("i32", 4)]
        I32, 
        [PrimitiveType("i64", 8)]
        I64,
        [PrimitiveType("f32", 4)]
        F32,
        [PrimitiveType("f64", 8)]
        F64,
        [PrimitiveType("bool", 1)]
        Bool,
        [PrimitiveType("char", 8)]
        Char,
    }
}
