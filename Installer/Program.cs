using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Installer
{
    internal class Program
    {
        const string GAME_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed";
        const string DLL_PATH = @"C:\Users\luca\Documents\Projects\NonlethalCompany\NonlethalCompany\bin\Debug";

        static void Main(string[] args)
        {
            if (File.Exists(GAME_PATH + @"\Assembly-CSharp.dll.bak"))
                File.Copy(GAME_PATH + @"\Assembly-CSharp.dll.bak", GAME_PATH + @"\Assembly-CSharp.dll", true);
            else
                File.Copy(GAME_PATH + @"\Assembly-CSharp.dll", GAME_PATH + @"\Assembly-CSharp.dll.bak", true);
            File.Copy(DLL_PATH + @"\NonlethalCompany.dll", GAME_PATH + @"\NonlethalCompany.dll", true);
            File.Copy(DLL_PATH + @"\0Harmony.dll", GAME_PATH + @"\0Harmony.dll", true);

            var gameImage = ModuleDefinition.FromFile(GAME_PATH + @"\Assembly-CSharp.dll");
            var importer = new ReferenceImporter(gameImage);
            var type = gameImage.GetAllTypes().Where(x => x.Name == "InitializeGame").First();
            var method = type.Methods.Where(x => x.Name == "Start").First();

            var dllImage = ModuleDefinition.FromFile(DLL_PATH + @"\NonlethalCompany.dll");
            TypeDefinition typeToImport = dllImage.TopLevelTypes.First(t => t.Name == "Game");
            MethodDefinition methodToImport = typeToImport.Methods.First(m => m.Name == "Initialize");
            IMethodDefOrRef importedMethod = methodToImport.ImportWith(importer);
            method.CilMethodBody.Instructions.Insert(0, new CilInstruction(CilOpCodes.Call, importedMethod));
            gameImage.Write(GAME_PATH + @"\Assembly-CSharp.dll");
        }
    }
}
