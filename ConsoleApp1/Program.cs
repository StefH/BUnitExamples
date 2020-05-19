using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ClrTest.Reflection;
//using ILDisassembler;
using Mono.Reflection;

namespace ConsoleApp1
{
    static class Program
    {
        public class Foo
        {
            public string Str { get; set; }

            public int I { get; set; }

            //public const string FooName = "Foo";
            //public static string GetFooName() { return typeof(Foo).Name + ":" + FooName; }
        }

        static Type[] GetParameterTypes(MethodInfo method)
        {
            ParameterInfo[] pia = method.GetParameters();
            Type[] types = new Type[pia.Length];

            for (int i = 0; i < pia.Length; i++)
            {
                types[i] = pia[i].ParameterType;
            }
            return types;
        }

        static void PrintByteCode(MethodInfo method)
        {
            foreach (Instruction instruction in method.GetInstructions())
                PrintInstruction(instruction);
        }

        static void PrintInstruction(Instruction instruction)
        {
            Console.Write("{0}: {1} ",
                instruction.Offset,
                instruction.OpCode.Name);

            switch (instruction.OpCode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.InlineSwitch:
                    var branches = instruction.Operand as int[];
                    for (int i = 0; i < branches.Length; i++)
                    {
                        if (i > 0)
                            Console.Write(", ");
                        Console.Write(branches[i]);
                    }
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                    Console.Write((int)instruction.Operand);
                    break;
                case OperandType.InlineString:
                    Console.Write("\"{0}\"", instruction.Operand);
                    break;
                default:
                    Console.WriteLine(instruction.Operand);
                    break;
            }

            Console.WriteLine();
        }

        public static string DumpMethod(Delegate method)
        {
            // For aggregating our response
            StringBuilder sb = new StringBuilder();

            // First we need to extract out the raw IL
            var mb = method.Method.GetMethodBody();
            var il = mb.GetILAsByteArray();

            //PrintByteCode(method.Method);
            var xx = Disassembler.GetInstructions(method.Method);
            foreach (var x in xx)
            {
                Console.WriteLine($"{x.OpCode} {x.Operand}");
            }

            int t = 9;

            //var ddd = DynamicMethodHelper.ConvertFrom(method.Method);

            //byte[] code = mb.GetILAsByteArray();
            //ILReader reader = new ILReader(method.Method);

            //DynamicMethod dm = new DynamicMethod(
            //    method.Method.Name,
            //    method.Method.ReturnType,
            //    GetParameterTypes(method.Method),
            //    typeof(DynamicMethodHelper));

            //DynamicILInfo ilInfo = dm.GetDynamicILInfo();

            //ILInfoGetTokenVisitor visitor = new ILInfoGetTokenVisitor(ilInfo, code);
            //reader.Accept(visitor);
            //ilInfo.SetCode(code, mb.MaxStackSize);



            // We'll also need a full set of the IL opcodes so we
            // can remap them over our method body
            var opCodes = typeof(OpCodes)
                .GetFields()
                .Select(fi => (OpCode)fi.GetValue(null));

            //opCodes.Dump();

            // For each byte in our method body, try to match it to an opcode
            var mappedIL = il.Select(op => opCodes.FirstOrDefault(opCode => opCode.Value == op));

            // OpCode/Operand parsing: 
            //     Some opcodes have no operands, some use ints, etc. 
            //  let's try to cover all cases
            var ilWalker = mappedIL.GetEnumerator();
            while (ilWalker.MoveNext())
            {
                var mappedOp = ilWalker.Current;
                if (mappedOp.OperandType != OperandType.InlineNone)
                {
                    // For operand inference:
                    // MOST operands are 32 bit, 
                    // so we'll start there
                    var byteCount = 4;
                    long operand = 0;
                    string token = string.Empty;

                    // For metadata token resolution            
                    var module = method.Method.Module;
                    Func<int, string> tokenResolver = tkn => string.Empty;
                    switch (mappedOp.OperandType)
                    {
                        // These are all 32bit metadata tokens
                        case OperandType.InlineMethod:
                            tokenResolver = tkn =>
                            {
                                var resMethod = module.SafeResolveMethod((int)tkn);
                                return string.Format("({0}())", resMethod == null ? "unknown" : resMethod.Name);
                            };
                            break;
                        case OperandType.InlineField:
                            tokenResolver = tkn =>
                            {
                                var field = module.SafeResolveField((int)tkn);
                                return string.Format("({0})", field == null ? "unknown" : field.Name);
                            };
                            break;
                        case OperandType.InlineSig:
                            tokenResolver = tkn =>
                            {
                                var sigBytes = module.SafeResolveSignature((int)tkn);
                                var catSig = string
                                    .Join(",", sigBytes);
                                return string.Format("(SIG:{0})", catSig == null ? "unknown" : catSig);
                            };
                            break;
                        case OperandType.InlineString:
                            tokenResolver = tkn =>
                            {
                                var str = module.SafeResolveString((int)tkn);
                                return string.Format("('{0}')", str == null ? "unknown" : str);
                            };
                            break;
                        case OperandType.InlineType:
                            tokenResolver = tkn =>
                            {
                                var type = module.SafeResolveType((int)tkn);
                                return string.Format("(typeof({0}))", type == null ? "unknown" : type.Name);
                            };
                            break;
                        // These are plain old 32bit operands
                        case OperandType.InlineI:
                        case OperandType.InlineBrTarget:
                        case OperandType.InlineSwitch:
                        case OperandType.ShortInlineR:
                            break;
                        // These are 64bit operands
                        case OperandType.InlineI8:
                        case OperandType.InlineR:
                            byteCount = 8;
                            break;
                        // These are all 8bit values
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineI:
                        case OperandType.ShortInlineVar:
                            byteCount = 1;
                            break;
                    }
                    // Based on byte count, pull out the full operand
                    for (int i = 0; i < byteCount; i++)
                    {
                        ilWalker.MoveNext();
                        operand |= ((long)ilWalker.Current.Value) << (8 * i);
                    }

                    var resolved = tokenResolver((int)operand);
                    resolved = string.IsNullOrEmpty(resolved) ? operand.ToString() : resolved;
                    sb.AppendFormat("{0} {1}",
                            mappedOp.Name,
                            resolved)
                        .AppendLine();
                }
                else
                {
                    sb.AppendLine(mappedOp.Name);
                }
            }
            return sb.ToString();
        }

        
        static void Main()
        {
            //Func<string, int> del =
            //    str =>
            //    {
            //        var i = int.Parse(str);
            //        return (int)Math.Pow(2, i);
            //    };

            


            string s1 = "stef1";
            string s2 = "stef2";
            Action<Foo> f1 = foo => foo.Str = s1 + s2;
            //Console.WriteLine(DumpMethod(f1));


            var ops = Sigil.Disassembler<Action<Foo>>.Disassemble(f1);
            //var ops = Sigil.Disassembler<Func<string, int>>.Disassemble(del);

            var calls = ops.Where(o => o.IsOpCode && new[] { OpCodes.Call, OpCodes.Callvirt }.Contains(o.OpCode)).ToList();
            var methods = calls.Select(c => c.Parameters.ElementAt(0)).Cast<MethodInfo>().ToList();


            var disassembler = new ILDisassembler.Disassembler();
            var ft = typeof(Foo);
            var strings = disassembler.DisassembleMethod(f1.Method);


            var xx1 = f1.Method.GetInstructions();
            foreach (var x in xx1)
            {
                //Console.WriteLine($"'{x.OpCode}' '{x.Operand}' '{x?.Operand?.GetType()}'");
            }

            var callVirt1 = xx1.FirstOrDefault(x => x.OpCode == OpCodes.Callvirt);
            if (callVirt1 != null && callVirt1.Operand is MethodInfo methodInfo1 && callVirt1.Previous?.Operand != null)
            {
                Console.WriteLine($"PropertyName  = {methodInfo1.Name.Replace("set_","")}");

                var value = callVirt1.Previous.Operand;
                if (value is MethodInfo mi)
                {
                    var i = mi.GetInstructions();

                    int xxxxx7 = 9;
                }
                else
                {
                    Console.WriteLine($"NORMAL PropertyValue = '{value}'");
                }
                
            }


            Console.WriteLine("___________________________________");

            Action<Foo> f2 = foo => foo.I = -42;
            var xx2 = f2.Method.GetInstructions();
            var callVirt2 = xx2.FirstOrDefault(x => x.OpCode == OpCodes.Callvirt);
            foreach (var x in xx2)
            {
                //Console.WriteLine($"{x.OpCode} {x.Operand}");
            }
            if (callVirt2 != null && callVirt2.Operand is MethodInfo methodInfo2 && callVirt2.Previous?.Operand != null)
            {
                Console.WriteLine($"PropertyName  = {methodInfo2.Name.Replace("set_", "")}");

                var value = callVirt2.Previous.Operand;
                Console.WriteLine($"PropertyValue = '{value}'");
            }


            //Console.WriteLine(DumpMethod(f2));




            //Func<int, string> stuff = i =>
            //{
            //    var m = 10312;
            //    var j = i + m;
            //    var k = j * j + i;
            //    var foo = "Bar";
            //    var asStr = k.ToString();
            //    return foo + asStr;
            //};
            //Console.WriteLine(DumpMethod(stuff));

            //Console.WriteLine(DumpMethod((Func<string>)Foo.GetFooName));

            //Console.WriteLine(DumpMethod((Action)Console.Beep));
        }
    }
}