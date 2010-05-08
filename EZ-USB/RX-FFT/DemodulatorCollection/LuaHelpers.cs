using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using System.Reflection;
using RX_FFT.Components.GDI;
using System.Collections;

namespace DemodulatorCollection
{
    public class LuaHelpers
    {

        [AttrLuaFunc("bit_not", "Bitwise negation", new[] { "input value" })]
        public static uint LuaNot(uint value)
        {
            return ~value;
        }

        [AttrLuaFunc("bit_shl", "Bitwise shift left", new[] { "", "" })]
        public static uint LuaShl(uint value, int count)
        {
            return value << count;
        }

        [AttrLuaFunc("bit_shr", "Bitwise shift right", new[] { "", "" })]
        public static uint LuaShr(uint value, int count)
        {
            return value >> count;
        }

        [AttrLuaFunc("bit_xor", "Bitwise XOR operation", new[] { "", "" })]
        public static uint LuaXor(uint val1, uint val2)
        {
            return val1 ^ val2;
        }

        [AttrLuaFunc("bit_or", "Bitwise OR operation", new[] { "", "" })]
        public static uint LuaOr(uint val1, uint val2)
        {
            return val1 | val2;
        }

        [AttrLuaFunc("bit_and", "Bitwise AND operation", new[] { "", "" })]
        public static uint LuaAnd(uint val1, uint val2)
        {
            return val1 & val2;
        }

        [AttrLuaFunc("bit_reverse", "Reverse the bit order", new[] { "source value", "number of bits to reverse" })]
        public static uint LuaReverse(uint value, int bits)
        {
            uint retVal = 0;

            for (int pos = 0; pos < bits; pos++)
            {
                retVal <<= 1;
                if ((value & 1) != 0)
                {
                    retVal |= 1;
                }
                value >>= 1;
            }

            return retVal;
        }

        [AttrLuaFunc("print", "Print string into log window", new[] { "Text to print" })]
        public static void LuaPrint(string text)
        {
            Log.AddMessage("LUA", text);
        }

        [AttrLuaFunc("new", "", new[] { "" })]
        public static object LuaNew(string className)
        {
            Type type = Type.GetType(className);

            if (type == null)
            {
                type = Type.GetType("DemodulatorCollection.Demodulators." + className);
            }
            if (type == null)
            {
                type = Type.GetType("DemodulatorCollection.BitClockSinks." + className);
            }
            if (type == null)
            {
                return null;
            }

            return Activator.CreateInstance(type);
        }

        public static object[] CallFunction(Lua luaVm, string name, params object[] parameters)
        {
            try
            {
                LuaFunction func = luaVm.GetFunction(name);

                if (func == null)
                {
                    Log.AddMessage("Failed to call " + name + " (Reason: Function not found). Stopping.");
                }
                else
                {
                    return func.Call(parameters);
                }
            }
            catch (LuaException luaEx)
            {
                Log.AddMessage(name + " caused an error: " + luaEx.Message);
            }

            return null;
        }


        public static void RegisterLuaFunctions(Lua luaVm, Object pTarget)
        {
            // Sanity checks
            if (luaVm == null /*|| LuaFuncs == null*/)
                return;

            // Get the target type
            Type pTrgType = pTarget.GetType();

            // ... and simply iterate through all it's methods
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                // ... then through all this method's attributes
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    // and if they happen to be one of our AttrLuaFunc attributes
                    if (attr.GetType() == typeof(AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                        ArrayList pParams = new ArrayList();

                        // Get the desired function name and doc string, along with parameter info
                        String strFName = pAttr.getFuncName();
                        String strFDoc = pAttr.getFuncDoc();
                        String[] pPrmDocs = pAttr.getFuncParams();

                        // Now get the expected parameters from the MethodInfo object
                        ParameterInfo[] pPrmInfo = mInfo.GetParameters();

                        // If they don't match, someone forgot to add some documentation to the 
                        // attribute, complain and go to the next method

                        if (pPrmDocs == null || pPrmInfo.Length != pPrmDocs.Length)
                        {
                            Log.AddMessage("Function " + mInfo.Name + " (exported as " +
                                              strFName + ") argument number mismatch. Declared " +
                                              pPrmDocs.Length + " but requires " +
                                              pPrmInfo.Length + ".");
                            break;
                        }

                        // Build a parameter <-> parameter doc hashtable
                        for (int i = 0; i < pPrmInfo.Length; i++)
                        {
                            pParams.Add(pPrmInfo[i].Name);
                        }

                        // Get a new function descriptor from this information
                        LuaFuncDescriptor pDesc = new LuaFuncDescriptor(strFName, strFDoc, (String[])pParams.ToArray(typeof(string)), pPrmDocs);

                        // Add it to the global hashtable
                        //LuaFuncs.Add(strFName, pDesc);

                        // And tell the VM to register it.
                        luaVm.RegisterFunction(strFName, pTarget, mInfo);
                    }
                }
            }
        }
    }

    public class AttrLuaFunc : Attribute
    {
        private String FunctionName;
        private String FunctionDoc;
        private String[] FunctionParameters = null;

        public AttrLuaFunc(String strFuncName, String strFuncDoc, params String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParamDocs;
        }

        public AttrLuaFunc(String strFuncName, String strFuncDoc)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = new string[0];
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }
    }

    public class LuaFuncDescriptor
    {
        private String FunctionName;
        private String FunctionDoc;
        private String[] FunctionParameters;
        private String[] FunctionParamDocs;
        private String FunctionDocString;

        public LuaFuncDescriptor(String strFuncName, String strFuncDoc, String[] strParams,
                                 String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParams;
            FunctionParamDocs = strParamDocs;

            String strFuncHeader = strFuncName + "(%params%) - " + strFuncDoc;
            String strFuncBody = "\n\n";
            String strFuncParams = "";

            Boolean bFirst = true;

            for (int i = 0; i < strParams.Length; i++)
            {
                if (!bFirst)
                    strFuncParams += ", ";

                strFuncParams += strParams[i];
                strFuncBody += "\t" + strParams[i] + "\t\t" + strParamDocs[i] + "\n";

                bFirst = false;
            }

            strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);
            if (bFirst)
                strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);

            FunctionDocString = strFuncHeader.Replace("%params%", strFuncParams) + strFuncBody;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }

        public String[] getFuncParamDocs()
        {
            return FunctionParamDocs;
        }

        public String getFuncHeader()
        {
            if (FunctionDocString.IndexOf("\n") == -1)
                return FunctionDocString;

            return FunctionDocString.Substring(0, FunctionDocString.IndexOf("\n"));
        }

        public String getFuncFullDoc()
        {
            return FunctionDocString;
        }
    }
}
