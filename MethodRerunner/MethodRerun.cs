using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace furaga.MethodRerunner
{
    /// <summary>
    /// 各コマンドを実行したときの処理の実態
    /// コンテキストメニューの
    ///     "Save method paramters"をクリックしたらSaveMethodParameter(),
    ///     "Insert method test code"をクリックしたらInsertMethodTestCode()
    /// がそれぞれ実行される
    /// </summary>
    public static class MethodRerun
    {
        #region templates and keywords
        const string callMethodNonRetTemplate =
@"// load method parameters
    {0}
    {1}
// call method
    {2}{3};

    Assert.Fail();
";
        const string callMethodRetTemplate =
@"// load method parameters
    {0}
    {1}
// call method
    {2}{3};

    Assert.AreEqual(result, 1);
";
        const string saveCodeTemplate = 
@"FLib.ForceSerializer.Serialize({0}, @""{1}"", ""{0}"")";

        static readonly string[] keywords = new[] {
            "abstract", "as", "base", 
            "break", "case", "catch", "checked","class", "const", "continue", "default",
            "Delegate", "do", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", 
            "For", "foreach", "goto", "if", "implicit", "in", "interface", "internal",
            "is", "lock",  "namespace", "new", "null", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref","return",  "sealed","sizeof",
            "stackalloc", "static",  "struct", "switch", "this",  "throw", "true", "try", "typeof", 
            "unchecked",  "unsafe", "using", "virtual", "volatile", "while",};
        static readonly string[] predefinedTypes = new[] {
            "void", "uint",  "ulong", "ushort", "String",  "short", "sbyte", "long","float", "double", "char", "byte", "bool","decimal", "int",  "object", 
        };
        #endregion

        /// <summary>
        /// デバッグ実行時にブレークポイントで止まっているとき、
        /// 現在の関数の引数をFLib.ForceSerializerで保存する。
        /// </summary>
        internal static void SaveMethodParameter()
        {
            try
            {
                if (FLib.VSInfo.DTE2 == null)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "At least one document must be opened",
                        "Error: Save method parameters");
                    return;
                }
                if (FLib.VSInfo.Debugger2 == null)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "This command works only in debugging mode",
                        "Error: Save method parameters");
                    return;
                }

                // MyDocument/PartialExecution/ソリューション名/関数名以下に保存
                var methodName = FLib.VSInfo.Debugger2.CurrentStackFrame.FunctionName;

                var date = DateTime.Now;
                var dateStr = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

                var saveRootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(SolutionSaveRootDir(), methodName + "." + dateStr));

                if (!System.IO.Directory.Exists(saveRootDir))
                    System.IO.Directory.CreateDirectory(saveRootDir);

                string _methodName;
                SaveParameters(saveRootDir, out _methodName);

                System.Windows.Forms.MessageBox.Show(
                    "Successfully saved the parameters of " + methodName + " in " + saveRootDir,
                    "Save method parameters");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    ex.ToString() + ex.StackTrace,
                    "Error: Save method parameters");
                return;
            }
        }

        /// <summary>
        /// 保存した引数をロードして、関数呼び出しを行うコードを挿入する
        /// </summary>
        internal static void InsertMethodTestCode()
        {
            var rootDir = SolutionSaveRootDir();
            if (!System.IO.Directory.Exists(rootDir))
            {
                System.Windows.Forms.MessageBox.Show(
                    rootDir + " does not exist",
                    "Error: Insert method test code");
                return;
            }

            // 新しいフォームを開いて、どの関数のコードを生成するか選択
            var selectWin = new MethodSelectWindow(rootDir);
            selectWin.ShowDialog();
            var dir = selectWin.SelectedDir;
            if (dir != null && System.IO.Directory.Exists(dir))
                InsertMethodCallCode(dir);
        }

        static bool SaveParameters(string saveRootDir, out string currentMethodName)
        {
            try
            {
                // TODO: 現在ブレークポイントで止まっている位置の関数を選択
                // 現在はカーソルがある位置の関数を選択している

                // マウスカーソルが関数内にある場合、
                string docPath = FLib.VSInfo.DTE2.ActiveDocument.FullName;
                string code = System.IO.File.ReadAllText(docPath);
                //int textPos = (int)FLib.VSInfo.WpfTextView.Selection.Start.Position;
                EnvDTE.TextSelection sel = (EnvDTE.TextSelection)FLib.VSInfo.DTE2.ActiveDocument.Selection;
                int textPos = (int)sel.ActivePoint.AbsoluteCharOffset;

                var tokens = FLib.VSInfo.Debugger2.CurrentStackFrame.FunctionName.Split('.');
                var klassName = tokens.Length >= 2 ? tokens[tokens.Length - 2] : "";
                var funcName = tokens.Length >= 1 ? tokens[tokens.Length - 1] : "";

                string callMethodCode;
                List<string> saveCodes = SaveCodes(code, textPos, klassName, funcName, saveRootDir, out currentMethodName, out callMethodCode);
                
                if (saveCodes.Count >= 1)
                {
                    var dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(saveRootDir, currentMethodName));
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);

                    // 関数引数を保存
                    foreach (var c in saveCodes)
                        FLib.VSInfo.Debugger2.ExecuteStatement(c, 10, true);

                    // 関数呼び出し用のコードを保存
                    string filepath = System.IO.Path.Combine(dir, "callMethodCode.cs");
                    System.IO.File.WriteAllText(filepath, callMethodCode);
                }
                return true;
            }
            catch (Exception ex)
            {
                currentMethodName = "";
                Console.WriteLine(ex.ToString() + ex.StackTrace);
                return false;
            }
        }

        // TODO: ジェネリック関数への対応
        static List<string> SaveCodes(string code, int currentPos, string klassName, string funcName, string saveRootDir, out string currentMethodName, out string callMethodCode)
        {
            var syntaxes = SyntaxFactory.ParseSyntaxTree(code);
            var klass = ClassFromName(syntaxes.GetRoot(), klassName);
            var method = MethodFromName(syntaxes.GetRoot(), funcName);

            currentMethodName = "";
            callMethodCode = "";

            if (method != null)
            {
                var klassTokens = klass.DescendantTokens().ToList();
                string className = klassTokens[klassTokens.IndexOf(klassTokens.First(t => t.Text.Trim() == "class")) + 1].Text;

                List<List<string>> argTypeNames = new List<List<string>>();
                List<string> argNames = new List<string>();
                string retType;
                MethodInfo(method, out currentMethodName, out argTypeNames, out argNames, out retType);

                string saveDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(saveRootDir, currentMethodName));

                // "int x;"
                var dclCode = "";
                for (int i = 0; i < argNames.Count; i++)
                {
                    int skipNum = 0;
                    for (; skipNum < argTypeNames[i].Count; skipNum++)
                    {
                        if (!keywords.Contains(argTypeNames[i][skipNum]))
                            break;
                    }
                    dclCode += string.Format("\n    {0} {1};", string.Join("", argTypeNames[i].Skip(skipNum)), argNames[i]);
                }
                dclCode.Trim();

                // "FLib.ForceSerializer.Deserialize(out x);"
                var dsrCode = string.Format(@"string dir = @""{0}"";", saveDir);
                foreach (var arg in argNames)
                {
                    dsrCode += string.Format("\n    FLib.ForceSerializer.Deserialize(dir, \"{0}\", out {0});", arg);
                }
                dsrCode.Trim();

                //
                bool isStatic = method.DescendantTokens().Any(t => t.Text.Trim() == "static");
                string instantiateCode =
                    isStatic ?
                    "" :
                    string.Format("var inst = new {0}();\n    ", className);

                // "var ret = func(x, ref y, out z);"
                var callArgs = new List<string>();
                for (int i = 0; i < argNames.Count; i++)
                {
                    string prefix = argTypeNames[i].Contains("out") ? "out" :
                        argTypeNames[i].Contains("ref") ? "ref" :
                        "";
                    callArgs.Add(string.Format("{0} {1}", prefix, argNames[i]).Trim());
                }

                var callCode = string.Format("{0}{1}({2})",
                    retType.Trim() == "void" ? "" : "var result = ",
                    isStatic ? className + "." + currentMethodName : "inst." + currentMethodName,
                    string.Join(",", callArgs).Trim());

                var template = retType.Trim() == "void" ? callMethodNonRetTemplate : callMethodRetTemplate;
                callMethodCode = string.Format(callMethodRetTemplate, dclCode.Trim(), dsrCode.Trim(), instantiateCode, callCode.Trim());

                var saveCodes = new List<string>();
                foreach (var arg in argNames)
                {
                    string src = string.Format(saveCodeTemplate, arg, saveDir);
                    saveCodes.Add(src);
                }

                return saveCodes;
            }

            return new List<string>();
        }

        static string SolutionSaveRootDir()
        {
            // MyDocument/PartialExecution/
            var document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var pluginName = "PartialExecution";
            var solutionName = System.IO.Path.GetFileName(FLib.VSInfo.DTE2.Solution.FullName);
            var rootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(document, pluginName, solutionName));
            return rootDir;
        }

        static void InsertMethodCallCode(string dir)
        {
            try
            {
                // マウスカーソルが関数内にある場合、
                var insertPos = (int)FLib.VSInfo.WpfTextView.Selection.Start.Position;
                var length = (int)FLib.VSInfo.WpfTextView.Selection.End.Position - insertPos;
                string filepath = System.IO.Path.Combine(dir, "callMethodCode.cs");
                if (System.IO.File.Exists(filepath))
                {
                    EnvDTE.TextSelection sel = (EnvDTE.TextSelection)FLib.VSInfo.DTE2.ActiveDocument.Selection;
                    sel.NewLine(1);
                    sel.LineUp(false, 1);
                    sel.Text = System.IO.File.ReadAllText(filepath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + ex.StackTrace);
            }
        }

        static SyntaxNode ClassFromName(SyntaxNode tree, string name)
        {
            var nodes = tree.ChildNodes().ToList();
            foreach (var n in nodes)
            {
                if (n.IsKind(SyntaxKind.ClassDeclaration))
                {
                    var klassTokens = n.DescendantTokens().ToList();
                    string className = klassTokens[klassTokens.IndexOf(klassTokens.First(t => t.Text.Trim() == "class")) + 1].Text;
                    if (className == name)
                        return n;
                }
                else
                {
                    var res = ClassFromName(n, name);
                    if (res != null)
                        return res;
                }
            }
            return null;
        }
        static SyntaxNode MethodFromName(SyntaxNode tree, string name)
        {
            var nodes = tree.ChildNodes().ToList();
            foreach (var n in nodes)
            {
                if (n.IsKind(SyntaxKind.MethodDeclaration))
                {
                    string nm, ret;
                    List<List<string>> types;
                    List<string> args;
                    MethodInfo(n, out nm, out types, out args, out ret);
                    if (nm == name)
                        return n;
                }
                else
                {
                    var res = MethodFromName(n, name);
                    if (res != null)
                        return res;
                }
            }
            return null;
        }

        private static void MethodInfo(SyntaxNode method,
           out string methodName,
           out List<List<string>> argTypeNames,
           out List<string> argNames,
           out string retType)
        {

            // 関数名
            methodName = "";
            int nest = 0;
            foreach (var token in method.DescendantTokens().ToList())
            {
                bool finish = false;
                switch (token.Text.Trim())
                {
                    case "<":
                        nest++;
                        break;
                    case ">":
                        nest--;
                        break;
                    case "(":
                        finish = true;
                        break;
                    default:
                        if (nest <= 0)
                            methodName = token.Text.Trim();
                        break;
                }
                if (finish)
                    break;
            }

            // 引数
            argNames = new List<string>();
            argTypeNames = new List<List<string>>();
            var paramList = method.ChildNodes().Where(n => n.IsKind(SyntaxKind.ParameterList)).ToList();
            if (paramList.Count == 1)
            {
                foreach (var p in paramList[0].ChildNodes())
                {
                    var tokens = p.DescendantTokens().Select(t => t.Text).ToList();
                    var name = tokens.Last();
                    argTypeNames.Add(tokens.Take(tokens.Count - 1).ToList());
                    argNames.Add(name);
                }
            }

            // 返り値
            retType = string.Join("", method.DescendantNodes().ElementAt(0).DescendantTokens().Select(t => t.Text));
        }
    }
}