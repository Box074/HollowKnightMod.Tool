
namespace HKTool.Unsafe;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoClassField
{
    public void* field_type;
    public byte* name;
    public void* parentType;
    
    /*
	 * Offset where this field is stored; if it is an instance
	 * field, it's the offset from the start of the object, if
	 * it's static, it's from the start of the memory chunk
	 * allocated for statics for the class.
	 * For special static fields, this is set to -1 during vtable construction.
	*/
    public int offset;
}
