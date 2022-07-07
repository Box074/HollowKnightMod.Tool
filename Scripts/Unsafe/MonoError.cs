
namespace HKTool.Unsafe;

public unsafe struct MonoError
{
    ushort error_code;
    ushort hidden_0; /*DON'T TOUCH */

	fixed long hidden_1 [12]; /*DON'T TOUCH */
}
